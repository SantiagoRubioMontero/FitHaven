using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;  // Agregado para trabajar con UI Image
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;

public class FilePickerSystem : MonoBehaviour
{
    public string FinalPath;
    public Image uiImage;

    private DatabaseReference reference;
    private string userId;

    void Start()
    {
        // Obtiene la referencia al servicio de base de datos
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Obtiene el UserId desde PlayerPrefs (asegúrate de que el usuario esté autenticado)
        userId = PlayerPrefs.GetString("UserId");

    }

    public void LoadFile()
    {
        string FileType = NativeFilePicker.ConvertExtensionToFileType(".png"); //Permite seleccionar ruta del archivo, en este caso solo PNG

 
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                FinalPath = path; //Asigna la ruta del archivo
                PlayerPrefs.SetString("SavedImagePath", FinalPath);
                PlayerPrefs.Save();
                UpdateProfilePicDB();
                Debug.Log("Picked file: " + FinalPath); //Muestra la ruta por consola
                StartCoroutine("LoadTexture"); //Corutina para cargar archivo en Imagen
            }
        }, new string[] { FileType });
    }



    public IEnumerator LoadTexture()
    {
        // Realiza una solicitud para obtener la textura desde la ruta
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + FinalPath); ;

        // Envía la solicitud y espera a que termine
        yield return www.SendWebRequest();

        // Verifica si hubo un error durante la solicitud
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar la textura: " + www.error);
        }
        else
        {
            // Convierte la textura cargada en un sprite
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            // Convierte la textura en un sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Asigna el sprite al componente UI Image
            uiImage.sprite = sprite;
        }
    }

    public void UpdateProfilePicDB()
    {
        string imageUrl = PlayerPrefs.GetString("SavedImagePath");

        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "users/" + userId + "/imageUrl", imageUrl }, // Convertir a string
        };

        // Usamos UpdateChildrenAsync para solo actualizar los campos de height y weight
        reference.UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateChildrenAsync fue cancelado.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("Error al guardar los datos: " + task.Exception);
                    return;
                }

                // Si todo sale bien
                Debug.Log("URL imagen perfil actualizados exitosamente.");
              
            });
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;  // Agregado para trabajar con UI Image
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;

public class GalleryPickerSystem : MonoBehaviour
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

        // Cargar imagen guardada al iniciar (si existe)
        string savedPath = PlayerPrefs.GetString("SavedImagePath", null);
        if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
        {
            Debug.Log("Cargando imagen guardada: " + savedPath);
            StartCoroutine(LoadTexture(savedPath));
        }
        else
        {
            Debug.Log("No hay imagen guardada o archivo no encontrado.");
        }

    }

    public void LoadFile()
    {
        try
        {
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (path == null)
                {
                    Debug.Log("Operación cancelada.");
                    return;
                }

                string copiedPath = CopyImageToPersistentData(path);

                if (!string.IsNullOrEmpty(copiedPath))
                {
                    FinalPath = copiedPath;
                    PlayerPrefs.SetString("SavedImagePath", FinalPath);
                    PlayerPrefs.Save();
                    Debug.Log("Imagen copiada a: " + FinalPath);
                }
                else
                {
                    Debug.LogError("No se pudo copiar la imagen al almacenamiento interno.");
                    return;
                }

                Debug.Log("Archivo seleccionado: " + FinalPath);

                StartCoroutine(LoadTexture(path));

                if (!string.IsNullOrEmpty(userId))
                {
                    UpdateProfilePicDB(path);
                }
                else
                {
                    Debug.LogWarning("No se actualizó Firebase porque el UserId no está disponible.");
                }

            }, "Selecciona una imagen PNG", "image/png");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error inesperado al cargar la imagen: " + ex.Message);
        }
    }


    private IEnumerator LoadTexture(string path)
    {
        string finalPath = "file://" + path;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(finalPath))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al cargar la textura: " + www.error);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    if (uiImage != null)
                    {
                        uiImage.sprite = sprite;
                    }
                    else
                    {
                        Debug.LogWarning("uiImage es null. No se puede asignar la imagen.");
                    }
                }
                else
                {
                    Debug.LogError("La textura descargada es nula.");
                }
            }
        }
    }

    private void UpdateProfilePicDB(string imagePath)
    {
        var updates = new Dictionary<string, object>
        {
            { $"users/{userId}/imageUrl", imagePath }
        };

        reference.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
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

            Debug.Log("URL de imagen de perfil actualizada exitosamente.");
        });
    }

    private string CopyImageToPersistentData(string originalPath)
    {
        try
        {
            string fileName = Path.GetFileName(originalPath);
            string destinationPath = Path.Combine(Application.persistentDataPath, fileName);

            File.Copy(originalPath, destinationPath, true);

            return destinationPath;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error copiando imagen: " + e.Message);
            return null;
        }
    }
}
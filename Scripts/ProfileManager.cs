using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.IO;
using Firebase;
using Firebase.Database;
using Firebase.Auth;

//ENCARGADO DE MOSTRAR TODA LA INFORMACIÓN DEL PERFIL AL INICIAR SESIÓN Y BORRARLA AL SALIR
public class ProfileManager : MonoBehaviour
{

    public static ProfileManager Instance;// Singleton para acceder desde cualquier script

    public Text userNameText;
    public Text userEmailText;
    public Text userWeightText;
    public Text userHeightText;
    public Text userImcText;
    public Text userImcStatusText;

    //Imagenes status IMC
    public Sprite rojoSprite;
    public Sprite naranjaSprite;
    public Sprite verdeSprite;
    public Sprite amarilloSprite;
    public Sprite amarilloMedioSprite;

    public Image imcStatusImage;

    private string userImcStatusString;
    public Image profileImage;  // UI Image donde se mostrará la foto de perfil
    private DatabaseReference dbReference;

    // Referencia al script FirebaseController para poder llamar a sus funcioens
    public FirebaseController firebaseController;


    void Start()
    {
        string userName = PlayerPrefs.GetString("UserName", "Usuario");
        string userEmail = PlayerPrefs.GetString("UserEmail", "correo@example.com");

        userNameText.text = userName;
        userEmailText.text = userEmail;

        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        StartCoroutine(LoadProfileImageDB());
        StartCoroutine(LoadUserDataFromDatabase());
        
    }

    IEnumerator LoadUserDataFromDatabase()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference userRef = dbReference.Child("users").Child(userId);

        var dataTask = userRef.GetValueAsync();
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
        {
            Debug.LogError("Error al obtener datos del usuario: " + dataTask.Exception);
            yield break;
        }

        DataSnapshot snapshot = dataTask.Result;

        if (snapshot.Exists)
        {
            float weight = snapshot.Child("weight").Exists ? float.Parse(snapshot.Child("weight").Value.ToString()) : 0f;
            float height = snapshot.Child("height").Exists ? float.Parse(snapshot.Child("height").Value.ToString()) : 0f;
            float imc = snapshot.Child("imc").Exists ? float.Parse(snapshot.Child("imc").Value.ToString()) : 0f;
            string imcStatus = snapshot.Child("imcStatus").Exists ? snapshot.Child("imcStatus").Value.ToString() : "Desconocido";

            // Actualizar UI
            userWeightText.text = weight.ToString("F2");
            userHeightText.text = height.ToString("F2");
            userImcText.text = imc.ToString("F2");
            userImcStatusText.text = imcStatus;

            // Guardar en PlayerPrefs para reutilización si se quiere
            PlayerPrefs.SetFloat("UserWeight", weight);
            PlayerPrefs.SetFloat("UserHeight", height);
            PlayerPrefs.SetFloat("UserImc", imc);
            PlayerPrefs.SetString("UserImcStatus", imcStatus);
            PlayerPrefs.Save();

            userImcStatusString = imcStatus;
            imageImc();
        }
        else
        {
            Debug.LogWarning("No se encontraron datos físicos para este usuario.");
        }
    }


    //Dependiendo del IMC saca una imagen de un color u otro
    public void imageImc()
    {
        if (string.IsNullOrEmpty(userImcStatusString))
        {
            imcStatusImage.gameObject.SetActive(false);
            return;
        }

        imcStatusImage.gameObject.SetActive(true);

        // Asignar las imágenes de acuerdo al estado del IMC
        switch (userImcStatusString)
        {
            case "Bajo peso":
                imcStatusImage.sprite = rojoSprite;
                break;
            case "Peso normal":
                imcStatusImage.sprite = verdeSprite;
                break;
            case "Sobrepeso":
                imcStatusImage.sprite = amarilloSprite;
                break;
            case "Obesidad grado I":
                imcStatusImage.sprite = amarilloMedioSprite;
                break;
            case "Obesidad grado II":
                imcStatusImage.sprite = rojoSprite;
                break;
            case "Obesidad grado III":
                imcStatusImage.sprite = rojoSprite;
                break;
            default:
                imcStatusImage.gameObject.SetActive(false);
                break;
        }
    }
    
    /*
    public void LogOut()
    {
        firebaseController.LogOut();

        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.DeleteKey("UserEmail");
        PlayerPrefs.DeleteKey("UserWeight");
        PlayerPrefs.DeleteKey("UserHeight");
        PlayerPrefs.DeleteKey("UserImc");
        PlayerPrefs.DeleteKey("UserImcStatus");

        userNameText.text = "";
        userEmailText.text = "";
        userWeightText.text = "";
        userHeightText.text = "";
        userImcText.text = "";
        userImcStatusText.text = "";

        Debug.Log("Usuario LOGOUT con éxito");
    }*/

    IEnumerator LoadProfileImageDB()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference userRef = dbReference.Child("users").Child(userId).Child("imageUrl");

        var dataTask = userRef.GetValueAsync();
        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
        {
            Debug.LogError("Error al obtener la imagen: " + dataTask.Exception);
            yield break;
        }

        DataSnapshot snapshot = dataTask.Result;
        if (snapshot.Exists && !string.IsNullOrEmpty(snapshot.Value.ToString()))
        {
            string imageUrl = snapshot.Value.ToString();
            StartCoroutine(DownloadImage(imageUrl));
        }
        else
        {
            Debug.LogWarning("No hay imagen de perfil para este usuario.");
        }
    }

    IEnumerator DownloadImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            profileImage.sprite = sprite;
        }
        else
        {
            Debug.LogError("Error al descargar la imagen: " + www.error);
        }
    }
}





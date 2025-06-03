using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

//ENCARGADO DE GUARDAR LOS DATOS DE ALTURA Y PESO EN EL PERFIL
public class PhysicalDataManager : MonoBehaviour
{
    public InputField heightInputField; // Campo de entrada para la altura
    public InputField weightInputField; // Campo de entrada para el peso
    public Button updateButton; // Botón para actualizar datos
    public Text notificationText; // Para mostrar notificaciones o mensajes de éxito/error

    private DatabaseReference reference;
    private string userId;

    public AudioSource correctAudio;
    public AudioSource errorAudio;

    void Start()
    {
        //Límite de caracteres en los InputFields
        heightInputField.characterLimit = 5;
        weightInputField.characterLimit = 5;

        // Listeners para reemplazar '.' con ','
        heightInputField.onValueChanged.AddListener(delegate { FixDecimalSeparator(heightInputField); });
        weightInputField.onValueChanged.AddListener(delegate { FixDecimalSeparator(weightInputField); });

        // Obtiene la referencia al servicio de base de datos
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Obtiene el UserId desde PlayerPrefs (asegúrate de que el usuario esté autenticado)
        userId = PlayerPrefs.GetString("UserId");

        // Asigna el evento de click al botón
        updateButton.onClick.AddListener(UpdateUserPhysicalData);
    }

    // Este método se ejecuta cuando el usuario hace click en el botón
    public void UpdateUserPhysicalData()
    {
        // Asegúrate de que los campos no estén vacíos
        if (string.IsNullOrEmpty(heightInputField.text) || string.IsNullOrEmpty(weightInputField.text))
        {
            notificationText.text = "Por favor, completa todos los campos.";
            errorAudio.Play();
            return;
        }

        // Convierte los valores a float
        float height;
        float weight;
      //
        // Intentamos convertir las entradas a float, y si no se puede, mostramos un error
        if (!float.TryParse(heightInputField.text, out height) || !float.TryParse(weightInputField.text, out weight))
        {
            notificationText.text = "Por favor ingresa valores numéricos válidos para la altura y el peso.";
            errorAudio.Play();
            return;
        }


        float imc = (weight / ((height / 100f) * (height / 100f)));
        string imcStatus = GetImcStatus(imc);

        PlayerPrefs.SetFloat("UserWeight", weight);
        PlayerPrefs.SetFloat("UserHeight", height);
        PlayerPrefs.SetFloat("UserImc", imc);
        PlayerPrefs.SetString("UserImcStatus", imcStatus);
        PlayerPrefs.Save();

        // Preparamos un diccionario con los datos que queremos actualizar
        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "users/" + userId + "/height", height },  // Almacenar como float
            { "users/" + userId + "/weight", weight },   // Almacenar como float
            { "users/" + userId + "/imc", imc },         // Almacenar como float
            { "users/" + userId + "/imcStatus", imcStatus }  
        };
        // Usamos UpdateChildrenAsync para solo actualizar los campos de height y weight
        reference.UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateChildrenAsync fue cancelado.");
                    notificationText.text = "Hubo un error al guardar los datos.";
                    errorAudio.Play();
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("Error al guardar los datos: " + task.Exception);
                    notificationText.text = "Hubo un error al guardar los datos.";
                    errorAudio.Play();
                    return;
                }
                notificationText.text = "";
                correctAudio.Play();
                // Si todo sale bien
                Debug.Log("Datos físicos actualizados exitosamente.");
                //notificationText.text = "Datos actualizados con éxito.";
            });
    }

    string GetImcStatus(float imc)
    {
        if (imc < 18.5f)
            return "Bajo peso";
        else if (imc < 25f)
            return "Peso normal";
        else if (imc < 30f)
            return "Sobrepeso";
        else if (imc < 35f)
            return "Obesidad grado I";
        else if (imc < 40f)
            return "Obesidad grado II";
        else
            return "Obesidad grado III";
    }

    void FixDecimalSeparator(InputField inputField)
    {
        inputField.text = inputField.text.Replace('.', ',');
    }
}



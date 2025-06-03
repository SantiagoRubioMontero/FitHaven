using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

//---------------> NO ENST� EN USO <--------------
//ENCARGADO DE MOSTRAR LOS DATOS F�SICOS EN EL PERFIL
public class PhysicData : MonoBehaviour
{
    public InputField heightInputField; // Campo de entrada para la altura
    public InputField weightInputField; // Campo de entrada para el peso
    public InputField imcInputField;
    public Button updateButton; // Bot�n para actualizar datos
    public Text notificationText; // Para mostrar notificaciones o mensajes de �xito/error

    private DatabaseReference reference;
    private string userId;


    void Start()
    {
        // Obtiene la referencia al servicio de base de datos
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Obtiene el UserId desde PlayerPrefs (aseg�rate de que el usuario est� autenticado)
        userId = PlayerPrefs.GetString("UserId");

        // Asigna el evento de click al bot�n
        updateButton.onClick.AddListener(UpdateUserPhysicalData);
    }

    public void UpdateUserPhysicalData()
    {
        // Aseg�rate de que los campos no est�n vac�os
        if (string.IsNullOrEmpty(heightInputField.text) || string.IsNullOrEmpty(weightInputField.text))
        {
            notificationText.text = "Por favor, completa todos los campos.";
            return;
        }

        // Convierte los valores a float (puedes agregar validaciones si es necesario)
        float height = float.Parse(heightInputField.text);
        float weight = float.Parse(weightInputField.text);
        float imc = float.Parse(weightInputField.text);


        // Establecemos los valores para poder mostrarlos en PlayerPrefs
        PlayerPrefs.SetFloat("UserWeight", weight);
        PlayerPrefs.SetFloat("UserHeight", height);
        PlayerPrefs.SetFloat("IMC", imc);
        PlayerPrefs.Save();

        // Preparamos un diccionario con los datos que queremos actualizar
        var updates = new System.Collections.Generic.Dictionary<string, object>
        {
            { "users/" + userId + "/height", height.ToString() }, // Convertir a string
            { "users/" + userId + "/weight", weight.ToString() }, // Convertir a string
            { "users/" + userId + "/imc", imc.ToString() }
        };

        // Usamos UpdateChildrenAsync para solo actualizar los campos de height y weight
        reference.UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateChildrenAsync fue cancelado.");
                    notificationText.text = "Hubo un error al guardar los datos.";
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("Error al guardar los datos: " + task.Exception);
                    notificationText.text = "Hubo un error al guardar los datos.";
                    return;
                }

                // Si todo sale bien
                Debug.Log("Datos f�sicos actualizados exitosamente.");
                notificationText.text = "Datos actualizados con �xito.";
            });
    }
}


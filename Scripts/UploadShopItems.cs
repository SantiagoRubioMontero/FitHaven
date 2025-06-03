using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
public class UploadShopItems : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        // Llamar a CheckAndFixDependenciesAsync antes de utilizar Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // Las dependencias de Firebase están listas, ahora podemos acceder a la base de datos
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                // Obtiene el UserId desde PlayerPrefs (asegúrate de que el usuario esté autenticado)
                userId = PlayerPrefs.GetString("UserId");

                // Llama al método para subir los objetos
                UploadDefaultObjects();
            }
            else
            {
                Debug.LogError("Firebase no está completamente inicializado. Error: " + task.Exception);
            }
        });
    }

    void UploadDefaultObjects()
    {
        //Entramos en Firebase para obtener los datos de los Objetos llamados "Objects"
        dbReference.Child("users").Child(userId).Child("objects").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener Objetos del usuario: " + task.Exception);
                return;
            }

            //Firebase responde con ÉXITO
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; //Contiene los objetos actuales

                Dictionary<string, object> newObjects = new Dictionary<string, object>()
                {
                    { "Banco", new Dictionary<string, object> { { "name", "Banco" }, { "obtained", false } } },
                    { "Soporte de Discos", new Dictionary<string, object> { { "name", "Soporte de Discos" }, { "obtained", false } } },
                    { "Mancuerna", new Dictionary<string, object> { { "name", "Mancuerna" }, { "obtained", false } } },
                    { "PressBanca", new Dictionary<string, object> { { "name", "PressBanca" }, { "obtained", false } } },
                    { "Barra Eleiko", new Dictionary<string, object> { { "name", "Barra Eleiko" }, { "obtained", false } } },
                    { "Barra Rogue", new Dictionary<string, object> { { "name", "Barra Rogue" }, { "obtained", false } } },
                    { "Mancuerna 10kg", new Dictionary<string, object> { { "name", "Mancuerna 10kg" }, { "obtained", false } } },
                    { "Mancuerna 20kg", new Dictionary<string, object> { { "name", "Mancuerna 20kg" }, { "obtained", false } } },
                    { "Mancuerna 30kg", new Dictionary<string, object> { { "name", "Mancuerna 30kg" }, { "obtained", false } } },
                    { "Mancuerna 15kg", new Dictionary<string, object> { { "name", "Mancuerna 15kg" }, { "obtained", false } } },
                    { "Mancuerna 12kg", new Dictionary<string, object> { { "name", "Mancuerna 12kg" }, { "obtained", false } } },
                    { "Mancuerna 8kg", new Dictionary<string, object> { { "name", "Mancuerna 8kg" }, { "obtained", false } } },
                    { "Mancuerna 3kg", new Dictionary<string, object> { { "name", "Mancuerna 3kg" }, { "obtained", false } } },
                    { "Mancuerna 5kg", new Dictionary<string, object> { { "name", "Mancuerna 5kg" }, { "obtained", false } } },
                    { "Mancuerna 6kg", new Dictionary<string, object> { { "name", "Mancuerna 6kg" }, { "obtained", false } } },
                    { "Mancuernero", new Dictionary<string, object> { { "name", "Mancuernero" }, { "obtained", false } } },
                    { "Kettelbell", new Dictionary<string, object> { { "name", "Kettelbell" }, { "obtained", false } } },
                    { "Kettelbell 10kg", new Dictionary<string, object> { { "name", "Kettelbell 10kg" }, { "obtained", false } } },
                    { "Kettelbell 20kg", new Dictionary<string, object> { { "name", "Kettelbell 20kg" }, { "obtained", false } } },
                    { "Press de Pierna", new Dictionary<string, object> { { "name", "Press de Pierna" }, { "obtained", false } } },
                    { "Disco", new Dictionary<string, object> { { "name", "Disco" }, { "obtained", false } } },
                    { "Disco 15kg", new Dictionary<string, object> { { "name", "Disco 15kg" }, { "obtained", false } } },
                    { "Disco 20kg", new Dictionary<string, object> { { "name", "Disco 20kg" }, { "obtained", false } } },
                    { "Disco 25kg", new Dictionary<string, object> { { "name", "Disco 25kg" }, { "obtained", false } } },
                    { "Rack de Dominadas", new Dictionary<string, object> { { "name", "Rack de Dominadas" }, { "obtained", false } } },
                    { "Rack de Sentadilla", new Dictionary<string, object> { { "name", "Rack de Sentadilla" }, { "obtained", false } } }

                };

                //Diccionario para guardar objetos a añadir (se actualiza cada vez que se usa esta función)
                Dictionary<string, object> objectsToAdd = new Dictionary<string, object>();

                // Comparar con los existentes y añadir solo los nuevos
                foreach (var obj in newObjects)
                {
                    if (!snapshot.HasChild(obj.Key)) // Solo si el object no existe en Firebase lo añadimos al diccionario de solo los nuevos
                    {
                        objectsToAdd[obj.Key] = obj.Value;
                    }
                }
                //AÑADIR NUEVOS OBJETOS:
                // Si hay objetos a añadir en el diccionario de los nuevos
                if (objectsToAdd.Count > 0)
                {
                    //Actualizamos el Firebase con los nuevos datos a Añadir
                    dbReference.Child("users").Child(userId).Child("objects").UpdateChildrenAsync(objectsToAdd).ContinueWith(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log("Nuevos Objetos añadidos con éxito.");
                        }
                        else
                        {
                            Debug.LogError("Error al subir los nuevos Objetos: " + updateTask.Exception);
                        }
                    });
                }
                else
                {
                    Debug.Log("Todos los Objetos ya existen en Firebase.");
                }

                //ELIMINAR OBJETOS BORRADOS:
                //Para los objetos que están en Firebase, comprueba uno a uno
                foreach (var existingObject in snapshot.Children)
                {
                    string existingKey = existingObject.Key;

                    // Si el objeto ya no está en la lista de nuevos objetos, lo eliminamos
                    if (!newObjects.ContainsKey(existingKey))
                    {
                        //Eliminar en Firebase
                        dbReference.Child("users").Child(userId).Child("objects").Child(existingKey).RemoveValueAsync().ContinueWith(removeTask =>
                        {
                            if (removeTask.IsCompleted)
                            {
                                Debug.Log($"Objeto {existingKey} eliminado con éxito.");
                            }
                            else
                            {
                                Debug.LogError("Error al eliminar el Objeto: " + removeTask.Exception);
                            }
                        });
                    }
                }
            }
        });
    }
}

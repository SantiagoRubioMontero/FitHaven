using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

public class LoadUserObjects : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;

    public List<GameObject> gameObjectsInScene; // Asigna los objetos desde el inspector

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = PlayerPrefs.GetString("UserId");
        LoadPurchasedObjects();
    }

    public void LoadPurchasedObjects()
    {
        dbReference.Child("users").Child(userId).Child("objects").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener objetos del usuario: " + task.Exception);
            }

            DataSnapshot snapshot = task.Result;

            foreach (GameObject obj in gameObjectsInScene)
            {
                Debug.Log(obj+" Desactivado");
                obj.SetActive(false); // Desactivamos todos los objetos al inicio
            }

            foreach (var child in snapshot.Children)
            {
                string objectName = child.Child("name").Value.ToString();
                bool obtained = bool.Parse(child.Child("obtained").Value.ToString());

                Debug.Log("ESTADO: "+ child +" "+ objectName +" "+ obtained );
                if (obtained)
                {
                    Debug.Log(objectName + " --> " + obtained);
                    foreach (GameObject obj in gameObjectsInScene)
                    {
                        if (obj.name == objectName)
                        {
                            Debug.Log(obj + " Activado");
                            obj.SetActive(true);
                        }
                    }
                }
            }
        });
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

//ENCARGADO DE ESCROLEAR Y DE CARGAR LOS OBJETIVOS EN LA BD PARA QUE SE VEAN
/*
 * Obtiene el ID del usuario actual desde PlayerPrefs.
 * Accede a Firebase y obtiene la lista de objetivos del usuario.
 * Elimina los objetivos anteriores en el ScrollView.
 * Ordena los objetivos por número (item1, item2, etc.).
 * Crea y muestra los objetivos en la UI si no están completados.
 */
public class DynamicScrollView : MonoBehaviour
{
    [SerializeField]
    private Transform scrollViewContent;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private List<Sprite> itemImages; // Lista de imágenes para los elementos

    [SerializeField]
    private Image levelImageDisplay; // Imagen donde se mostrará el icono del nivel

    [SerializeField]
    private List<Sprite> levelImages; // Lista de imágenes para los niveles

    private DatabaseReference reference;
    private string userId;


    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.25f); // Espera opcional
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                userId = PlayerPrefs.GetString("UserId");
                LoadObjectives();
            }
            else
            {
                Debug.LogError("Firebase no inicializado: " + task.Exception);
            }
        });
    }

    public void LoadObjectives()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Usuario no autenticado.");
            return;
        }

        //Se accede a firebase obteniendo la lista de objetivos(items) actuales del Usuario
        reference.Child("users").Child(userId).Child("items").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // Eliminar elementos anteriores en el ScrollView
                foreach (Transform child in scrollViewContent)
                {
                    Destroy(child.gameObject);
                }

                //Obtenemos y ordenamos los items para que no salga  el 10 antes que el 1, etc...
                List<DataSnapshot> orderedItems = new List<DataSnapshot>();

                // Agregar elementos a la lista
                foreach (var item in snapshot.Children)
                {
                    orderedItems.Add(item);
                }

                //Lista temporal para almacenar los objetivos del Firebase y se ordenan comparando solo los números del nombre,
                //por ejemplo item1 se extrae el 1
                orderedItems.Sort((a, b) =>
                    int.Parse(a.Key.Replace("item", "")) - int.Parse(b.Key.Replace("item", "")));

                // Obtener nivel del usuario
                int userLevel = PlayerPrefs.GetInt("UserLevel", 0);

                // Determinar el rango de objetivos a mostrar según el nivel
                int minItemIndex = 1;
                int maxItemIndex = 20;
                int levelImageIndex = 0;


                if (userLevel >= 5 && userLevel < 10)
                {
                    minItemIndex = 21;
                    maxItemIndex = 40;
                    levelImageIndex = 1;
                }
                else if (userLevel >= 10 && userLevel < 15)
                {
                    minItemIndex = 41;
                    maxItemIndex = 60;
                    levelImageIndex = 2;
                }
                else if (userLevel >= 15 && userLevel < 20)
                {
                    minItemIndex = 61;
                    maxItemIndex = 80;
                    levelImageIndex = 3;
                }

                // Cambiar imagen del nivel si existe una disponible
                if (levelImageDisplay != null && levelImageIndex < levelImages.Count)
                {
                    levelImageDisplay.sprite = levelImages[levelImageIndex];
                }

                // Mostrar objetivos dentro del rango correspondiente
                int index = 0;
                foreach (var item in orderedItems)
                {
                    string key = item.Key;
                    int itemIndex = int.Parse(key.Replace("item", ""));

                    // Filtrar por rango de nivel
                    if (itemIndex < minItemIndex || itemIndex > maxItemIndex)
                        continue;

                    string description = item.Child("name").Value.ToString();
                    bool completed = bool.Parse(item.Child("completed").Value.ToString());

                    if (!completed)
                    {
                        GameObject newItem = Instantiate(prefab, scrollViewContent);
                        Debug.Log($"Instanciando nuevo item con key: {key} y descripcion: {description}");

                        if (newItem.TryGetComponent<ScrollViewItem>(out ScrollViewItem itemScript))
                        {
                            itemScript.SetItemKey(key);
                            itemScript.SetDescription(description);

                            if (index < itemImages.Count)
                            {
                                itemScript.ChangeImage(itemImages[index]);
                            }
                        }
                        index++;
                    }
                }
            }
        });
    }
}

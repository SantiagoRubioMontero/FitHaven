using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class ShopManager : MonoBehaviour
{
    public List<ShopItemSO> shopItemSO; // Ahora es List para poder eliminar elementos dinámicamente
    public ShopTemplate[] shopPanels;   // Paneles UI

    [SerializeField]
    private List<Sprite> objImages; // Lista de imágenes para los niveles

    private DatabaseReference dbReference;
    private string userId;


    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.25f); // Espera opcional
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                userId = PlayerPrefs.GetString("UserId");
                LoadPanels();
            }
            else
            {
                Debug.LogError("Firebase no está completamente inicializado. Error: " + task.Exception);
            }
        });
    }

    public void LoadPanels()
    {
        dbReference.Child("users").Child(userId).Child("objects").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener objetos del usuario: " + task.Exception);
                return;
            }

            if (!task.IsCompleted) return;

            DataSnapshot snapshot = task.Result;
            List<ShopItemSO> itemsToDisplay = new List<ShopItemSO>();
            List<Sprite> imagesToDisplay = new List<Sprite>();

            Dictionary<string, bool> firebaseObjects = new Dictionary<string, bool>();

            foreach (var child in snapshot.Children)
            {
                string name = child.Child("name").Value?.ToString();
                bool obtained = false;

                var obtainedValue = child.Child("obtained").Value;
                if (obtainedValue != null)
                {
                    bool.TryParse(obtainedValue.ToString(), out obtained);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    firebaseObjects[name] = obtained;
                }
            }

            // Recorremos los objetos e imágenes
            for (int i = 0; i < shopItemSO.Count; i++)
            {
                var item = shopItemSO[i];
                if (firebaseObjects.ContainsKey(item.title) && firebaseObjects[item.title] == false)
                {
                    itemsToDisplay.Add(item);

                    if (i < objImages.Count)
                    {
                        imagesToDisplay.Add(objImages[i]);
                    }
                }
            }

            // Mostrar los objetos disponibles
            for (int i = 0; i < shopPanels.Length; i++)
            {
                if (i < itemsToDisplay.Count)
                {
                    ShopItemSO item = itemsToDisplay[i];
                    shopPanels[i].Setup(item);
                    shopPanels[i].gameObject.SetActive(true);

                    if (i < imagesToDisplay.Count && shopPanels[i].iconImage != null)
                    {
                        shopPanels[i].iconImage.sprite = imagesToDisplay[i];
                    }
                }
                else
                {
                    shopPanels[i].gameObject.SetActive(false);
                }
            }
        });
    }
}

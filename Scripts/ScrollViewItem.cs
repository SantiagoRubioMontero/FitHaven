using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

//SCRIPT DE CADA ITEM DEL SCROLLVIEW, ENCARGADO DE ASIGNAR XP Y MONEDAS ASI COMO CAMBIAR IMAGENES Y TEXTOS
public class ScrollViewItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image childImage; //Imagen unica de cada copia

    [SerializeField] 
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private TextMeshProUGUI itemText;

    [SerializeField]
    private GameObject confirmationMsg; //Imagen unica de cada copia

    public AudioSource audio;

    private string itemKey; // Se usará para identificar el objetivo en la BD
    private DatabaseReference reference;
    private string userId;

    //EXPERIENCIA:
    [SerializeField]
    private int experienceAmount = 100; // Cantidad de XP a otorgar al hacer clic
    private ExperienceManager experienceManager; // Referencia al ExperienceManager
    //MONEDAS:
    [SerializeField]
    private int moneyAmount = 100;// Cantidad de Monedas a otorgar al hacer clic
    private MoneyManager moneyManager;// Referencia al moneyManager

    private void Start()
    {
        confirmationMsg.SetActive(false);

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = PlayerPrefs.GetString("UserId");

        experienceManager = FindObjectOfType<ExperienceManager>(); // Busca automáticamente el ExperienceManager en la escena
        moneyManager = FindObjectOfType<MoneyManager>(); // Busca automáticamente el MoneyManager en la escena
    }

    //Establece la clave para cada ítem
    public void SetItemKey(string key)
    {
        itemKey = key;
        itemText.text = itemKey;
    }

    //Cambia la imagen de cada objetivo
     public void ChangeImage(Sprite image)
     {
         childImage.sprite = image;
     }

    public void SetDescription(string description)
    {
        descriptionText.text = description;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        confirmationMsg.SetActive(true);
    }

    public void OnConfirmClick()
    {
        

        Debug.Log($"Clicked Item, adding {experienceAmount} experience");
        Debug.Log($"Clicked Item, adding {moneyAmount} coins");
        if (experienceManager != null)
        {
            experienceManager.AddExperience(100); // Añade experiencia
            moneyManager.AddMoney(moneyAmount);
        }
        else
        {
            Debug.LogWarning("ExperienceManager not found in the scene!");
        }

        if (userId != null && !string.IsNullOrEmpty(itemKey))
        {
            reference.Child("users").Child(userId).Child("items").Child(itemKey).Child("completed").SetValueAsync(true)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log($" {itemKey} marcado como completado.");
                    }
                    else
                    {
                        Debug.LogError($"Error al marcar {itemKey}: {task.Exception}");
                    }
                });
        }
        confirmationMsg.SetActive(false);
        gameObject.SetActive(false); // Ocultar el objetivo tras completarlo
    }

    public void OnHideClick()
    {
        confirmationMsg.SetActive(false);
        audio.Play();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Database;
using UnityEngine.UI;

public class ShopTemplate : MonoBehaviour
{
    public TMP_Text titleTxt;
    public TMP_Text descriptionTxt;
    public TMP_Text priceTxt;
    public Image iconImage; // Imagen del ítem

    [SerializeField]
    private ShopItemSO shopItemSO;

    private MoneyManager moneyManager;// Referencia al moneyManager
    public AudioSource audioSource;  // Referencia al AudioSource
    private int totalMoney;

    private DatabaseReference dbReference;
    private string userId;

    private void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>(); // Busca automáticamente el MoneyManager en la escena
        totalMoney = PlayerPrefs.GetInt("UserMoney"); // Guardar dinero total localmente

        // Configurar referencia a Firebase
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = PlayerPrefs.GetString("UserId"); // Asegúrate de que el UserId esté almacenado en PlayerPrefs


        Debug.Log("UserID: " + userId);
        Debug.Log("TotalMoney: " + totalMoney);
    }

    public void OnClick()
    {
        int amount = int.Parse(priceTxt.text); // Pasamos el texto de precio a un int
        string objectName = titleTxt.text; // Usamos el título como identificador del objeto en Firebase


        if (totalMoney >= amount) // Asegúrate de que el usuario tiene suficiente dinero
        {
            audioSource.Play();
            moneyManager.SpendMoney(amount); // Usamos la función de gastar dinero del money manager


            // Actualizar Firebase para marcar el objeto como obtenido
            dbReference.Child("users").Child(userId).Child("objects").Child(objectName).Child("obtained").SetValueAsync(true).ContinueWith(updateTask =>
            {       
                if (updateTask.IsCompleted)
                {
                    Debug.Log(objectName + " comprado con éxito.");
                    Debug.Log("Intentando actualizar: object" + objectName);
                    gameObject.SetActive(false); // Desactivar el objeto en la UI
                }
                else
                {
                    Debug.LogError("Error al actualizar Firebase: " + updateTask.Exception);
                }
            });
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Dinero insuficiente");
        }
    }

    //Para poner los datos concretos a los templates en el SHopManager
    public void Setup(ShopItemSO item)
    {

        titleTxt.text = item.title;
        descriptionTxt.text = item.description;
        priceTxt.text = item.baseCost.ToString();
        shopItemSO = item; // Guardamos referencia si es necesario más adelante
    }

}

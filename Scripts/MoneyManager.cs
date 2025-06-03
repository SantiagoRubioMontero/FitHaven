using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.UI;
using TMPro;

//SE ENCARGA DE ACTUALIZAR LA UI DEL DINERO DE LA TIENDA
public class MoneyManager : MonoBehaviour
{
    int totalMoney;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI moneyText;

    private DatabaseReference reference;
    private string userId;

    void Start()
    {
        // Verificar las dependencias de Firebase antes de continuar
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Firebase está listo para usar
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                userId = PlayerPrefs.GetString("UserId");

                // Cargar dinero desde Firebase al iniciar
                LoadMoneyFromDatabase();
            }
            else
            {
                Debug.LogError("Firebase no está disponible. Error: " + task.Result);
            }
        });
    }

    void LoadMoneyFromDatabase()
    {
        reference.Child("users").Child(userId).Child("money").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error al cargar dinero desde Firebase.");
                return;
            }

            if (task.Result.Exists)
            {
                totalMoney = int.Parse(task.Result.Value.ToString());  // Convertir el valor a int
                PlayerPrefs.SetInt("UserMoney", totalMoney);  // Guardar localmente
                PlayerPrefs.Save();
            }
            else
            {
                totalMoney = 0; // Si no hay datos, empezar desde 0
            }
            UpdateInterface();
        });
    }

    public void AddMoney(int amount)
    {
        Debug.Log($"Añadiendo {amount} monedas");

        totalMoney += amount;
        UpdateInterface();
    }

    public void SpendMoney(int amount)
    {
        Debug.Log($"Quitando {amount} monedas");

        totalMoney -= amount;
        UpdateInterface();
    }

    void UpdateInterface()
    {
        moneyText.text = totalMoney.ToString();

        // Actualizar la base de datos con el nuevo valor
        UpdateDataBaseMoney();
    }

    void UpdateDataBaseMoney()
    {
        var updates = new Dictionary<string, object>
        {
            { "users/" + userId + "/money", totalMoney } // Guardar totalMoney en Firebase
        };

        PlayerPrefs.SetInt("UserMoney", totalMoney); // Guardar dinero total localmente
        PlayerPrefs.Save();

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

            Debug.Log("Datos de dinero actualizados exitosamente.");
        });
    }
}

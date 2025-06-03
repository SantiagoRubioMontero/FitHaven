using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class PremiumManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;

    private bool premium = false;

    public GameObject panelPremium;

    // Start is called before the first frame update
    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = PlayerPrefs.GetString("UserId");

        ProvePremium();
    }

    void ProvePremium()
    {
        dbReference.Child("users").Child(userId).Child("premium").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener bool del usuario: " + task.Exception);
            }

            if (task.IsCompleted && task.Result.Exists)
            { 
                premium = false;
                bool.TryParse(task.Result.Value.ToString(), out premium);

                if (premium)
                {
                    Debug.Log("Este usuario " + userId + " ES PREMIUM");
                    panelPremium.SetActive(false);
                }
                else 
                {
                    Debug.Log("Este usuario " + userId + " NO ES PREMIUM");
                    panelPremium.SetActive(true);
                }
            }

        });
    }

    public void OnClick()
    {
        dbReference.Child("users").Child(userId).Child("premium").SetValueAsync(true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                
                Debug.Log("Estado premium actualizado a TRUE para el usuario " + userId);
                premium = true;
                panelPremium.SetActive(false);
            }
            else
            {
                Debug.LogError("Error al actualizar el estado premium: " + task.Exception);
            }
        });
        
    }
}

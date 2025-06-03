using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsFirebaseReady { get; private set; } = false;

    public static event System.Action OnFirebaseReady;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // opcional si quieres mantenerlo entre escenas

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                IsFirebaseReady = true;
                Debug.Log("Firebase listo.");
                OnFirebaseReady?.Invoke(); // Notificar a los demás scripts
            }
            else
            {
                Debug.LogError("No se pudieron resolver las dependencias de Firebase: " + task.Exception);
            }
        });
    }
}

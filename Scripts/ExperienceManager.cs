using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class ExperienceManager : MonoBehaviour
{
    [Header("Experience")]
    private int currentLevel, totalExperience;
    private int previousLevelsExperience, nextLevelsExperience;

    [Header("Interface")]
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI experienceText;
    [SerializeField] Image experienceFill;

    private DatabaseReference reference;
    private string userId;

    public AudioSource audio;

    private const int XP_PER_LEVEL = 400;

    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        userId = PlayerPrefs.GetString("UserId");

        LoadExperienceFromDatabase();
    }

    void LoadExperienceFromDatabase()
    {
        reference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error al cargar datos desde Firebase.");
                return;
            }

            if (task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;

                totalExperience = snapshot.Child("xp").Exists ? int.Parse(snapshot.Child("xp").Value.ToString()) : 0;
                currentLevel = snapshot.Child("level").Exists ? int.Parse(snapshot.Child("level").Value.ToString()) : 0;

                PlayerPrefs.SetInt("UserXp", totalExperience);
                PlayerPrefs.SetInt("UserLevel", currentLevel);
                PlayerPrefs.Save();

                UpdateLevel();
            }
            else
            {
                totalExperience = 0;
                currentLevel = 0;
                UpdateLevel();
            }
        });
    }

    public void AddExperience(int amount)
    {
        Debug.Log($"Añadiendo {amount} XP");
        totalExperience += amount;
        CheckForLevelUp();
        UpdateInterface();
    }

    void CheckForLevelUp()
    {
        while (totalExperience >= GetXpForLevel(currentLevel + 1))
        {
            currentLevel++;
            audio.Play();
        }
        UpdateLevel();
    }

    void UpdateLevel()
    {
        previousLevelsExperience = GetXpForLevel(currentLevel);
        nextLevelsExperience = GetXpForLevel(currentLevel + 1);
        UpdateInterface();
    }

    int GetXpForLevel(int level)
    {
        return level * XP_PER_LEVEL;
    }

    void UpdateInterface()
    {
        int xpThisLevel = totalExperience - previousLevelsExperience;
        int xpToNextLevel = nextLevelsExperience - previousLevelsExperience;
        float fillAmount = (float)xpThisLevel / xpToNextLevel;

        levelText.text = currentLevel.ToString();
        experienceText.text = $"{xpThisLevel} exp / {xpToNextLevel} exp";
        experienceFill.fillAmount = fillAmount;

        UpdateDataBaseXp();
    }

    void UpdateDataBaseXp()
    {
        var updates = new Dictionary<string, object>
        {
            { $"users/{userId}/xp", totalExperience },
            { $"users/{userId}/level", currentLevel }
        };

        PlayerPrefs.SetInt("UserXp", totalExperience);
        PlayerPrefs.SetInt("UserLevel", currentLevel);
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

            Debug.Log("Datos XP y nivel actualizados exitosamente.");
        });
    }
}

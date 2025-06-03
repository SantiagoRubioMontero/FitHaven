using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScenes : MonoBehaviour
{
    public void GoProfileScene()
    {
        SceneManager.LoadScene("User");
    }
    public void GoComunityScene()
    {
        SceneManager.LoadScene("BlogUsers");
    }

    public void GoHomeScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void GoObjectivesScene()
    {
        SceneManager.LoadScene("Objectives");
    }

    public void GoShopScene()
    {
        SceneManager.LoadScene("Shopping");
    }

    public void GoMessageScene()
    {
        SceneManager.LoadScene("NewsLetter");
    }

    public void GoLogInScene()
    {
        SceneManager.LoadScene("FirebaseAut");
    }

    public void GoUsersDataScene()
    {
        SceneManager.LoadScene("UsersData");
    }

}

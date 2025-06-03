using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckManager : MonoBehaviour
{
    public GameObject panelCheck;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ShowPanelCheck()
    { 
        panelCheck.SetActive(true);
    }

    public void HidePanelCheck()
    {
        panelCheck.SetActive(false);
    }
}

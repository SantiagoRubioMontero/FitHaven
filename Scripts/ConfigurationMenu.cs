using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ConfigurationMenu : MonoBehaviour
{

    public GameObject panelConfiguracion;
    //public GameObject cube;

    private MeshRenderer cubeRenderer;
    private Collider cubeCollider;



    private void Start()
    {
        /*if (cube != null)
        {
            cubeRenderer = cube.GetComponent<MeshRenderer>();
            cubeCollider = cube.GetComponent<Collider>();
        }*/
    }

    public void ShowConfig()
    {
        panelConfiguracion.SetActive(true);
        Rotatable.configActive = true; // Bloquear la rotación cuando el panel se muestra

       /* foreach (Transform child in cube.transform)
        {
            child.gameObject.SetActive(false);
        }*/
    }

    public void HideConfig()
    {
        panelConfiguracion.SetActive(false);
        Rotatable.configActive = false; // Permitir la rotación cuando el panel se oculta

        /*foreach (Transform child in cube.transform)
        {
            child.gameObject.SetActive(true);
        }*/
    }


}

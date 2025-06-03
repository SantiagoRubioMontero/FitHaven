using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrightnessManager : MonoBehaviour
{
    public Slider sliderBrillo; // El slider de brillo
    public float sliderValue;
    public Image panelBrillo; // Panel para simular el brillo

    private void Start()
    {
        // Cargar el valor guardado del brillo
        sliderBrillo.value = PlayerPrefs.GetFloat("brillo", 0f);
        AdjustBrightness(sliderBrillo.value);
    }

    // Ajusta el brillo según el valor del slider y lo guarda
    public void AdjustBrightness(float value)
    {
        sliderValue = value;
        PlayerPrefs.SetFloat("brillo", sliderValue);
        PlayerPrefs.Save(); // Guarda el valor inmediatamente

        // Ajustar la transparencia del panel
        panelBrillo.color = new Color(panelBrillo.color.r, panelBrillo.color.g, panelBrillo.color.b, sliderValue);
    }
}

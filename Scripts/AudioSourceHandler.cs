using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceHandler : MonoBehaviour
{
    public AudioSource audioSource;
    // Start is called before the first frame update

    public void OnPointerClick()
    {
       
        audioSource.Play();
        
    }
}

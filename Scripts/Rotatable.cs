using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rotatable : MonoBehaviour
{
    public static Rotatable Instance;// Singleton para acceder desde cualquier script

    [SerializeField] private InputAction pressed, axis;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] public bool inverted = false;

    public static bool configActive = false;

    private Transform cam;
    private bool rotateAllowed;
    private Vector2 rotation;

    void Start()
    {
       //Asegura que solo haya una instancia del objeto
        if (FindObjectsOfType<Rotatable>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 
        DontDestroyOnLoad(this);
    }
    private void Awake()
    {
        cam = Camera.main.transform;
        pressed.Enable();
        axis.Enable();

        pressed.performed += _ => { StartCoroutine(Rotate()); }; //Cuando pulsemos la pantalla se realiza la función
        pressed.canceled +=_=> { rotateAllowed = false; }; //En el momento en el que no se esté pulsando se cancela la rotación
        axis.performed += context => { rotation = context.ReadValue<Vector2>(); };

    }

    //Función encargada de la rotación
    private IEnumerator Rotate()
    {
        Debug.Log("ESTA GIRANDO");
        rotateAllowed = true;
        while (rotateAllowed)
        {
            if (configActive || this == null) yield break;  // Detiene la rotación si el objeto ya no existe

            rotation *= speed;
            transform.Rotate(Vector3.up * (inverted ? -1 : 1), rotation.x, Space.World);
            transform.Rotate(-cam.right * (inverted ? -1 : 1), rotation.y, Space.World);
            yield return null;
        }
    }

    public void invertToggle()
    { 
        inverted = !inverted;
    }

    //Elimina la opcion de girar cuando e objeto se destruye, de esta forma no da error si lo pulsas
    private void OnDestroy()
    {
        pressed.Disable();
        axis.Disable();
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}

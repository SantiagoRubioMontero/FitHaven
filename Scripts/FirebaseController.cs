using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Google;
using static FirebaseController;


public class FirebaseController : MonoBehaviour
{
    public static FirebaseController Instance;// Singleton para acceder desde cualquier script

    //Paneles del menú de inicio de sesión
    public GameObject loginPanel, signupPanel, forgetPasswordPanel, notificationPanel;

    //Campos a rellenar por el jugador (EmailField, PasswordField, etc)
    public InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupUserName, forgetPassEmail;


    //Textos usados en notificaciones y panel de Usuario
    public Text notif_Title_Text, notif_Message_Text, profileUserName_Text, profileUserEmail_Text;

    public Toggle rememberMe;

    public ChangeScenes changeScene;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    //Booleanos usados para saber si el usuario ha iniciado sesión o no
    bool isSignedIn = false;
    bool isSigned = false;

    private DatabaseReference reference;

    public AudioSource audio;
    public AudioSource audioError;

    // INICIO SESION GOOGLE 
    public string GoogleAPI = "390910689257-h4nf5ut7hd8gf99h4m1r1mkemqtnm4ge.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;
    private bool isGoogleSignInInitialized = false;

    void Start()
    {

        DontDestroyOnLoad(gameObject); // Evita que se destruya al cambiar de escena

        // LOG IN DE GOOGLE
        /*
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = "390910689257-h4nf5ut7hd8gf99h4m1r1mkemqtnm4ge.apps.googleusercontent.com", //Cliente web de Firebase
            RequestIdToken = true
        };
        */
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                //Inicializar Firebase para usar sus recursos
                InitializeFirebase();

            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));

            }
        });


        // RECORDAR USUARIO O NO 
        
        //Si el toggle no esta activado (se desactiva en el logOut) el remember me se pondra a 0
        if (!rememberMe.isOn )
        {
            PlayerPrefs.SetInt("RememberMe", 0);
            PlayerPrefs.Save();
        }
        else 
        {
            int rememberMeValue = PlayerPrefs.GetInt("RememberMe", 0);
            Debug.Log("toggle es = " + rememberMeValue + " ");

            if (rememberMeValue == 1 && FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                // Usuario ya autenticado y RememberMe activado, ir directo a la MainScene
                Debug.Log("Usuario recordado, y ya autenticado. Redirigiendo a MainScene...");
                SceneManager.LoadScene("MainScene");
            }
        }

        
    }

    /// //////////////////////////////////////////////////////////////////////////////////////////////////
    // INICIO SESION CON GOOGLE
    
    public void GoogleLogIn()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };

            isGoogleSignInInitialized = true;
        }
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = GoogleAPI
        };
        GoogleSignIn.Configuration.RequestEmail = true;

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                Debug.Log("Cancelled");
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);

                Debug.Log("Faulted " + task.Exception);
            }
            else
            {
                Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                        Debug.Log("Faulted In Auth " + task.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        Debug.Log("Success");
                        user = auth.CurrentUser;
                        profileUserName_Text.text = user.DisplayName;
                        profileUserName_Text.text = user.Email;

                        PlayerPrefs.SetString("UserId", user.UserId);
                        PlayerPrefs.SetString("UserName", user.DisplayName);
                        PlayerPrefs.SetString("UserEmail", user.Email);
                        PlayerPrefs.Save();

                        SaveDataToDB(user.UserId, user.DisplayName, user.Email);
                        SceneManager.LoadScene("MainScene");
                    }
                });
            }
        });

    }
    ////////////////////////////////////////////////////////////////////////////////////////////////
    
    // GESTOR DE PANTALLAS
    public void OpenLogInPanel() //Abrir panel de LogIn
    {
        LoginWhipe();//Se limpian los campos de Email y Contraseña
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        //profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenSignUpPanel() //Abrir panel de SignUp
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        //profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    /*public void OpenProfilePanel() //Abrir panel de Perfil
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        //profilePanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }*/

    public void OpenForgetPasswordPanel() //Abrir panel de Contraseña olvidada
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        //profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }


    // GESTION DE INICIO DE SESION Y CREACION DE CUENTAS

    public void LoginUser() //Funcionamiento del menú de LogIn
    {
        //En caso de que los campos requeridos estén vacíos:
        if (string.IsNullOrEmpty(loginEmail.text) && string.IsNullOrEmpty(loginPassword.text))
        {
            //Notificación en caso de campo vacío
            showNotificationMessage("Error", "Rellene todos los campos!");
            audioError.Play();
            return;
        }

        PlayerPrefs.SetInt("RememberMe", rememberMe.isOn ? 1 : 0);
        PlayerPrefs.Save();

        //Hacer LogIn
        SignInUser(loginEmail.text, loginPassword.text);
    }

    public void SignUpUser() //Funcionamiento del menú de SignUp
    {
        //En caso de que los campos requeridos estén vacíos:
        if (string.IsNullOrEmpty(signupEmail.text) && string.IsNullOrEmpty(signupPassword.text) && string.IsNullOrEmpty(signupCPassword.text))
        {
            //Notificación en caso de campo vacío
            showNotificationMessage("Error", "Rellene todos los campos!");
            audioError.Play();
            return;
        }
        else if (signupPassword.text != signupCPassword.text)
        {
            //Notificación en caso de campo vacío
            showNotificationMessage("Error", "Las contraseñas no coinciden");
            audioError.Play();
            return;
        }
        audio.Play();
        //Hacer SignUp
        CreateUser(signupEmail.text, signupPassword.text, signupUserName.text);
    }

    public void forgetPass() //Funcionamiento del menú de ForgetPassword
    {
        //En caso de que los campos requeridos estén vacíos:
        if (string.IsNullOrEmpty(forgetPassEmail.text))
        {
            //Notificación en caso de campo vacío
            showNotificationMessage("Error", "Rellene todos los campos!");
            return;
        }

        //Hacer ForgetPassword
        forgetPasswordSubmit(forgetPassEmail.text);
    }

    //Función para mostrar mensajes de ERROR 
    private void showNotificationMessage(string title, string message)
    {
        notif_Title_Text.text = "" + title;
        notif_Message_Text.text = "" + message;

        notificationPanel.SetActive(true); //Superposición del panel de notificación
    }

    public void CloseNotif_Panel() //Función para cerrar el panel de notificaciones
    {
        notif_Title_Text.text = ""; //Se ponen en blanco los campos del panel
        notif_Message_Text.text = "";
        notificationPanel.SetActive(false);
    }

    public void LoginWhipe() //Función para que los campos del LogIn estén en blanco si se vuelve al menú
    {
        loginEmail.text = "";
        loginPassword.text = "";
    }
    /*
    public void LogOut() //Función para salir del perfil al menú de LogIn otra vez
    {
        auth.SignOut();
        profileUserEmail_Text.text = "";
        profileUserName_Text.text = "";
        
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene("FirebaseAut");


    }

    */

    void CreateUser(string email, string password, string Username) //Función para crear un usuario
    {
        //Excepciones en caso de que la tarea se cancele o surja un error durante la ejecución
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                //Para cada excepción se muestra la notificación correspondiente al error que corresponda
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage("Error", GetErrorMessage(errorCode));
                        audioError.Play();
                    }
                }

                return;
            }

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            Firebase.Auth.FirebaseUser newUser = result.User;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);

            UpdateUserProfile(Username);
            SaveDataToDB(newUser.UserId, Username, email);
        });
    }


    public void SignInUser(string email, string password)
    {
        if (auth == null)
        {
            Debug.LogError("FirebaseAuth no está inicializado.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync fue cancelado.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("Error en SignInWithEmailAndPasswordAsync: " + task.Exception);

                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = AuthError.WrongPassword;
                        showNotificationMessage("Error", GetErrorMessage(errorCode));
                        audioError.Play();
                    }
                }
                return;
            }

            
            Firebase.Auth.AuthResult result = task.Result;
            Firebase.Auth.FirebaseUser newUser = result.User;
            Debug.LogFormat("Inicio de sesión exitoso: {0} ({1})", newUser.DisplayName, newUser.UserId);

            // Guardar datos en PlayerPrefs antes de cambiar de escena
            PlayerPrefs.SetString("UserId",  newUser.UserId);
            PlayerPrefs.SetString("UserName", newUser.DisplayName);
            PlayerPrefs.SetString("UserEmail", newUser.Email);

            PlayerPrefs.Save();

            // Redirigir directamente a HomeScene
            Debug.Log("Cambiando a HomeScene...");
            SceneManager.LoadScene("MainScene"); // Asegúrate de que el nombre sea EXACTO al de la escena
        });
    }

    public void SaveDataToDB(string userId, string name, string email)
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        // Crear un objeto con los datos del usuario
        UserData userData = new UserData
        {
            UserName = name,
            UserEmail = email,
            weight = "0",
            height = "0",
            imageUrl="",
            xp= 0,
            level=0,
            money = 0,
            imc =0,
            imcStatus = "",
            items = "",
            objects="",
            premium = false
        };

        string json = JsonUtility.ToJson(userData);

        // Guardar los datos bajo el ID del usuario
        reference.Child("users").Child(userId).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SetRawJsonValueAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Error en SetRawJsonValueAsync: " + task.Exception);
                    return;
                }

                Debug.Log("Datos del usuario guardados exitosamente en la base de datos.");
            });
    }




    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }


    //Función para detectar los cambios de estado del usuario
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Usuario cerró sesión: " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Usuario inició sesión: " + user.UserId);
                isSignedIn = true;
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void UpdateUserProfile(string UserName) //Función que actualiza el perfil del usuario
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = UserName,
                //PhotoUrl = new Uri("https://dummyimage.com/300")
            };

            //Excepciones en caso de que la tarea se cancele o surja un error durante la ejecución
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");

                showNotificationMessage("Alert", "Account Successfully Created");
            });
        }
    }

   


    void Update()
    {
        if (isSignedIn)
        {
            if (isSigned)
            {
                isSigned = true;
                profileUserName_Text.text = "" + user.DisplayName;
                profileUserEmail_Text.text = "" + user.Email;
                //OpenProfilePanel();
            }
        }
    }

    //Notificaciones posibles para cada error del sistema
    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "Ya existe la cuenta con credenciales diferentes";
                break;
            case AuthError.MissingPassword:
                message = "Hace falta el contraseña";
                break;
            case AuthError.WeakPassword:
                message = "La contraseña es debil";
                break;
            case AuthError.WrongPassword:
                message = "La contraseña es Incorrecta";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "Ya existe la cuenta con ese correo electrónico";
                break;
            case AuthError.InvalidEmail:
                message = "Correo electronico invalido";
                break;
            case AuthError.MissingEmail:
                message = "Hace falta el correo electrónico";
                break;
            default:
                message = "Ocurrió un error";
                break;
        }
        return message;
    }

    void forgetPasswordSubmit(string forgetPasswordEmail) //Suscripción de contraseña olvidada 
    {
        //Se envía el email de recuperación de contraseña al email que el usuario indique en el panel correspondiente.

        auth.SendPasswordResetEmailAsync(forgetPasswordEmail).ContinueWithOnMainThread(task =>
        {
            //Excepciones en caso de que la tarea se cancele o surja un error durante la ejecución
            if (task.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled");
            }
            if (task.IsFaulted)
            {
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        showNotificationMessage("Error", GetErrorMessage(errorCode));
                        audioError.Play();
                    }
                }


            }

            showNotificationMessage("Alert", "Succesfully sent email");
        });
    }

}

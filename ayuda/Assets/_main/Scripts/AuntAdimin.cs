using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions; 
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Linq;


public class AuntAdimin : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    // Forgot Password variables
    [Header("Forgot Password")]
    public TMP_InputField emailForgotPasswordField;
    public TMP_Text confirmForgotPasswordText;

    [Header("UserData")]
    public TMP_InputField usernameField;
    public GameManager gameManager;
    public GameObject scoreElement;
    public Transform scoreboardContent;

    void Awake()
    {
        //Comprueba que todas las dependencias necesarias para Firebase estén presentes en el sistema
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Establece el objeto de instancia de autenticación
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }
    public void LoginButton()
    {
        //Llama a la corutina de inicio pasando el correo electrónico y la contraseña
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    public void RegisterButton()
    {
        //Llama a la corutina de registro pasando el correo electrónico, la contraseña y el nombre de usuario
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    public void ForgotPasswordButton()
    {
        //llama a la corutina recuperar contraceña
        StartCoroutine(ForgotPassword(emailForgotPasswordField.text));
    }
    public void SignOutButton()
    {
        auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearRegisterFeilds();
        ClearLoginFeilds();
    }
    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(usernameField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameField.text));
        if (User != null)
        {
            StartCoroutine(UpdateScoreInDatabase(gameManager.currentScore));
        }
        else
        {
            Debug.LogWarning("User is not logged in. Cannot update score.");
        }
    }
    public void ScoreboardButton()
    {
        StartCoroutine(LoadScoreboardData());
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Llama a la función de inicio de sesión de autenticación de Firebase pasando el correo electrónico y la contraseña
        Task<AuthResult> LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //Si hay errores manejarlos
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //El usuario ya ha iniciado sesión
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            yield return new WaitForSeconds(1);

            usernameField.text = User.DisplayName;
            UIManager.instance.UserDataScreen(); // Cambiar a la interfaz de usuario de datos del usuario
            confirmLoginText.text = "";
            ClearLoginFeilds();
            ClearRegisterFeilds();
        }
    }
    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //Si el campo de nombre de usuario está en blanco mostrar una advertencia
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //Si la contraseña no coincide muestra un aviso
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Llama a la función de inicio de sesión de autenticación de Firebase pasando el correo electrónico y la contraseña
            Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Espera hasta que se complete la tarea
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //Si hay errores manejarlos
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //El usuario ya ha sido creado
                //Ahora obtenemos el resultado
                User = RegisterTask.Result.User;

                if (User != null)
                {
                    //Crea un perfil de usuario y establece el nombre de usuario
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    // Llame a la función de actualización del perfil de usuario de autenticación de Firebase pasando el perfil con el nombre de usuario
                    Task ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Espera hasta que se complete la tarea
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {

                        //Si hay errores manejarlos
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //El nombre de usuario ya está configurado
                        //Ahora volvemos a la pantalla de inicio de sesión
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                    }
                }
            }
        }
    }
    private IEnumerator ForgotPassword(string _email)
    {
        var auth = FirebaseAuth.DefaultInstance;

        Task SendPasswordResetEmailTask = auth.SendPasswordResetEmailAsync(_email);
        yield return new WaitUntil(() => SendPasswordResetEmailTask.IsCompleted);

        if (SendPasswordResetEmailTask.Exception != null)
        {

            // Manejar errores de envío de correo electrónico de restablecimiento de contraseña
            Debug.LogWarning($"Failed to send password reset email: {SendPasswordResetEmailTask.Exception}");
            confirmForgotPasswordText.text = "Failed to send reset email";
        }
        else
        {
            // Correo electrónico de restablecimiento de contraseña enviado correctamente
            confirmForgotPasswordText.text = "Reset email sent successfully";
        }
    }
    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Crea un perfil de usuario y establece el nombre de usuario
        UserProfile profile = new UserProfile { DisplayName = _username };

        // Llame a la función de actualización del perfil de usuario de autenticación de Firebase pasando el perfil con el nombre de usuario
        Task ProfileTask = User.UpdateUserProfileAsync(profile);
        //Espera hasta que se complete la tarea
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //El nombre de usuario de autenticación ahora está actualizado
        }
    }
    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        // Establece el nombre de usuario del usuario actualmente conectado en la base de datos
        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //El nombre de usuario de la base de datos ahora está actualizado
        }
    }
    private IEnumerator UpdateScoreInDatabase(int _score)
    {

        Task DBTask = DBreference.Child("users").Child(User.UserId).Child("score").SetValueAsync(_score);

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogError("Failed to update score: " + DBTask.Exception);
        }
        else
        {
            Debug.Log("Score updated successfully.");
        }
    }
    private IEnumerator LoadScoreboardData()
    {
        var DBTask = DBreference.Child("users").OrderByChild("score").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) Debug.LogWarning($"Fallo en registrar la tarea {DBTask.Exception}");
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            //Destruyo todos los elementos de la tabla
            foreach (Transform child in scoreboardContent.transform) Destroy(child.gameObject);

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("username").Value.ToString();
                int score = int.Parse(childSnapshot.Child("score").Value.ToString());

                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, score);
            }
        }

        UIManager.instance.ScoreboardScreen();

    }
}

    [System.Serializable]
public class PlayerScore
{
    public string username;
    public int score;
}
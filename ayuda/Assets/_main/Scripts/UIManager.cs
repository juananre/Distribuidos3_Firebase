using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject ForgotPasswordUI;
    public GameObject userDataUI;
    public GameObject LiderboardUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
        ForgotPasswordUI.SetActive(false);
        userDataUI.SetActive(false);
    }
    public void RegisterScreen() // Regester button
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
        ForgotPasswordUI.SetActive(false);
        userDataUI.SetActive(false);
    }
    public void ForgotPasswordScreen()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        ForgotPasswordUI.SetActive(true);

    }
    public void UserDataScreen() //Logged in
    {
        ClearScreen();
        userDataUI.SetActive(true);
    }
    public void ClearScreen() //Turn off all screens
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        userDataUI.SetActive(false);
    }
    public void ScoreboardScreen()
    {
        userDataUI.SetActive(false);
        LiderboardUI.SetActive(true);
    }
    public void GameScreen()
    {
        userDataUI.SetActive(true);
        LiderboardUI.SetActive(false);
    }
}

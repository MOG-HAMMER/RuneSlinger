using System.Collections.Generic;
using ExitGames.Client.Photon;
using RuneSlinger.Base;
using RuneSlinger.Base.Commands;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    enum GameManagerState
    {
        Form,
        Sending,
        Error,
        LoggedIn
    }

    private string _displayUsername;
    private string _registerEmail;
    private string _registerPassword;
    private string _loginEmail;
    private string _loginPassword;
    private string _username;
    private string _error;
    private GameManagerState _state;

    public void Start()
    {
        

        _registerEmail = "";
        _username = "";
        _registerPassword = "";
        _loginEmail = "";
        _loginPassword = "";
        _state = GameManagerState.Form;
    }

    public void Update()
    {

    }

    public void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Width(800), GUILayout.Height(600));

        if (_state == GameManagerState.Form || _state == GameManagerState.Error)
        {
            GUILayout.Label("REGISTER RUNESLINGER ACCOUNT");

            if (_state == GameManagerState.Error)
                GUILayout.Label(string.Format("Error: {0}", _error));

            GUILayout.Label("Username");
            _username = GUILayout.TextField(_username);

            GUILayout.Label("Email");
            _registerEmail = GUILayout.TextField(_registerEmail);

            GUILayout.Label("Password");
            _registerPassword = GUILayout.TextField(_registerPassword);

            if (GUILayout.Button("Register"))
                Register(_username, _registerPassword, _registerEmail);

            GUILayout.Label("LOGIN RUNESLINGER ACCOUNT");

            GUILayout.Label("Email:");
            _loginEmail = GUILayout.TextField(_loginEmail);

            GUILayout.Label("Password:");
            _loginPassword = GUILayout.TextField(_loginPassword);

            if (GUILayout.Button("Login"))
                Login(_loginEmail, _loginPassword);
        }
        else if (_state == GameManagerState.Sending)
        {
            GUILayout.Label("Sending...");
        }
        else if (_state == GameManagerState.LoggedIn)
        {
            GUILayout.Label("Success: " + _displayUsername);
        }
        GUILayout.EndVertical();
    }

    private void Login(string email, string password)
    {
        _state = GameManagerState.Sending;
        NetworkManager.Instance.Dispatch(new LoginCommand(_loginEmail, _loginPassword), response =>
        {
            if (response.IsValid)
            {
                _state = GameManagerState.LoggedIn;
                _displayUsername = response.Response.Username;
            }
            else
            {
                _state = GameManagerState.Error;
                _error = response.ToErrorString();
            }
            
        });
    }

    private void Register(string username, string password, string email)
    {
        
        _state = GameManagerState.Sending;
        NetworkManager.Instance.Dispatch(new RegisterCommand(email, username, password), response =>
        {
            if (response.IsValid)
            {
                _state = GameManagerState.LoggedIn;
                _displayUsername = username;
            }
            else
            {
                _state = GameManagerState.Error;
                _error = response.ToErrorString();
            }
        });
    }
}
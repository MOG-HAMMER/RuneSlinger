using System.Collections.Generic;
using ExitGames.Client.Photon;
using RuneSlinger.Base;
using UnityEngine;

public class GameManager : MonoBehaviour, IPhotonPeerListener
{
    enum GameManagerState
    {
        Form,
        Sending,
        Error,
        Success
    }

    private PhotonPeer _photonPeer;
    private string _registerEmail;
    private string _registerPassword;
    private string _loginEmail;
    private string _loginPassword;
    private string _username;
    private string _error;
    private GameManagerState _state;

    public void Start()
    {
        _photonPeer = new PhotonPeer(this, ConnectionProtocol.Udp);
        if (!_photonPeer.Connect("127.0.0.1:5055", "RuneSlinger"))
            Debug.LogError("Could not connect to photon!");

        _registerEmail = "";
        _username = "";
        _registerPassword = "";
        _loginEmail = "";
        _loginPassword = "";
        _state = GameManagerState.Form;
    }

    public void Update()
    {
        _photonPeer.Service();
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
        else if (_state == GameManagerState.Success)
        {
            GUILayout.Label("Success!");
        }
        GUILayout.EndVertical();
    }

    private void Login(string email, string password)
    {
        _state = GameManagerState.Sending;
        _photonPeer.OpCustom((byte)RuneOperationCode.Login, new Dictionary<byte, object>
        {
            {(byte)RuneOperationCodeParameter.Email, email},
            {(byte)RuneOperationCodeParameter.Password, password},
        }, true);
    }

    private void Register(string username, string password, string email)
    {
        _state = GameManagerState.Sending;
        _photonPeer.OpCustom((byte) RuneOperationCode.Register, new Dictionary<byte, object>
        {
            {(byte)RuneOperationCodeParameter.Username, username},
            {(byte)RuneOperationCodeParameter.Email, email},
            {(byte)RuneOperationCodeParameter.Password, password},
        }, true);
    }

    public void OnApplicationQuit()
    {
        _photonPeer.Disconnect();
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        var response = (RuneOperationResponse) operationResponse.OperationCode;
        if (response == RuneOperationResponse.Error)
        {
            _state = GameManagerState.Error;
            _error = (string)operationResponse.Parameters[(byte) RuneOperationResponseParameter.ErrorMessage];
        }
        else if (response == RuneOperationResponse.FatalError || response == RuneOperationResponse.Invalid)
        {
            _state = GameManagerState.Error;
            _error = "YOU BROKE THE SERVER!";
        }
        else if (response == RuneOperationResponse.Success)
            _state = GameManagerState.Success;
        
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
    }

    public void OnEvent(EventData eventData)
    {
    }
}
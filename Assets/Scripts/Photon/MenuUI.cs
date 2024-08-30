using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MenuUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button CreateButton;
    [SerializeField] private Button JoinButton;
    [SerializeField] private TMPro.TMP_InputField createInput;
    [SerializeField] private TMPro.TMP_InputField joinInput;
    [SerializeField] private int MaxPlayerSala;

    private void Awake()
    {
        CreateButton.onClick.AddListener(CreateRoom);
        JoinButton.onClick.AddListener(JoinRoom);
    }

    private void OnDestroy()
    {
        CreateButton.onClick.RemoveAllListeners();
        JoinButton.onClick.RemoveAllListeners();
    }
    private void CreateRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            RoomOptions roomConfiguration = new RoomOptions();
            roomConfiguration.MaxPlayers = MaxPlayerSala;
            PhotonNetwork.CreateRoom(createInput.text, roomConfiguration);
        }
        else
        {
            Debug.LogError("ERROR AL CONECTAR");
        }
    }
    private void JoinRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRoom(joinInput.text);
        }
        else
        {
            Debug.LogError("No hay sala disponible");
        }
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GamePlay");
    }
}

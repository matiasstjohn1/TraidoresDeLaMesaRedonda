using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;  // Aseg�rate de que esta librer�a est� incluida
using UnityEngine.UI;

public class MenuUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button CreateButton;
    [SerializeField] private Button JoinButton;
    [SerializeField] private TMPro.TMP_InputField createInput;
    [SerializeField] private TMPro.TMP_InputField joinInput;
    [SerializeField] private int MaxPlayerSala;
    [SerializeField] private TMPro.TMP_InputField nicknameText;
    [SerializeField] private GameObject advertencia;

    private void Awake()
    {
        CreateButton.onClick.AddListener(CreateRoom);
        JoinButton.onClick.AddListener(JoinRoom);
        advertencia.SetActive(false);
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
            roomConfiguration.MaxPlayers = (byte)MaxPlayerSala; // Aseg�rate de convertir a byte
            PhotonNetwork.CreateRoom(createInput.text, roomConfiguration);
            PhotonNetwork.NickName = nicknameText.text;
        }
    }

    private void JoinRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.NickName = nicknameText.text;
            PhotonNetwork.JoinRoom(joinInput.text);
        }
    }

    public override void OnJoinedRoom()
    {
        // Este m�todo solo se ejecutar� si te unes correctamente a la sala
        PhotonNetwork.LoadLevel("GamePlay");
    }

    // Este m�todo se ejecuta cuando no puedes unirte a la sala
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode == 32758)  // C�digo 32758 es para sala llena
        {
            Debug.LogError($"Error al unirse a la sala: {message}");
        }
        else
        {
            Debug.LogError("La sala est� llena.");
            advertencia.SetActive(true); // Mostrar el mensaje de advertencia
            StartCoroutine(CloseAdvertenciaauto());   
        }
    }
    //Se cierra la advertencia sola despues de 10 segs
    public IEnumerator CloseAdvertenciaauto()
    {
        yield return new WaitForSeconds(10f);
        advertencia.SetActive(false);
    }
    // Bot�n para cerrar advertencia
    public void CloseAdvertencia()
    {

        advertencia.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Text.RegularExpressions;
using Photon.Pun.Demo.PunBasics;
using TMPro; // Si usas TextMeshPro, usa esta librería.

public class RoomMessageManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text messageText; // Campo de texto para mostrar mensajes.
    [SerializeField] private ScrollRect scrollRect; // Para manejar el scroll, opcional.

    private void Start()
    {
        if (messageText == null)
        {
            Debug.LogError("Por favor, asigna un componente TMP_Text en el inspector.");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string message = $"<color=green>El jugador {newPlayer.NickName} se unió a la partida.</color>";
        AddMessageToUI(message);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string message = $"<color=red>El jugador {otherPlayer.NickName} salió de la partida.</color>";
        AddMessageToUI(message);
    }

    private void AddMessageToUI(string message)
    {
        if (messageText != null)
        {
            messageText.text += message + "\n"; // Añade el mensaje con salto de línea.

            // Si usas un ScrollRect y quieres que siempre baje al final:
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f; // Baja al final del texto.
            }
        }
    }
}

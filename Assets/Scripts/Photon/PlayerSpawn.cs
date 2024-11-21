using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSpawn : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject cardPrefab; //Prefab de la carta o personaje
    [SerializeField] private List<Transform> spawnPositions; //Lista de posiciones de spawn para las cartas

    private bool cardsInstantiated = false;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient && !cardsInstantiated)
        {
            InstantiateCardsForAllPlayers();
        }
    }
    //Método que se llama cuando el jugador entra en la sala
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        SpawnPlayerCard();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        //Instanciar la carta para el nuevo jugador
        if (newPlayer == PhotonNetwork.LocalPlayer)
        {
            SpawnPlayerCard();
        }
    }

    //Método para instanciar las cartas para todos los jugadores
    private void InstantiateCardsForAllPlayers()
    {
        cardsInstantiated = true; //Marcamos que las cartas ya han sido instanciadas

        //Instanciamos una carta para cada jugador en la sala
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            int playerIndex = i;

            if (playerIndex < spawnPositions.Count)
            {
                PhotonNetwork.Instantiate(cardPrefab.name, spawnPositions[playerIndex].position, spawnPositions[playerIndex].rotation);
                Debug.Log($"Carta instanciada para {PhotonNetwork.PlayerList[i].NickName} en la posición {playerIndex}");
            }
            else
            {
                Debug.LogWarning("No hay suficientes posiciones de spawn para todos los jugadores.");
            }
        }
    }

    //Método para instanciar la carta solo para el jugador local
    private void SpawnPlayerCard()
    {
        //Posición de spawn al jugador basado en su ActorNumber
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        if (playerIndex < spawnPositions.Count)
        {
            PhotonNetwork.Instantiate(cardPrefab.name, spawnPositions[playerIndex].position, spawnPositions[playerIndex].rotation);
            Debug.Log($"Carta instanciada para {PhotonNetwork.LocalPlayer.NickName} en la posición {playerIndex}");
        }
        else
        {
            Debug.LogWarning("No hay suficientes posiciones de spawn para todos los jugadores.");
        }
    }
}
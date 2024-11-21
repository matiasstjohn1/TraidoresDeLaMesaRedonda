using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MusicPhotonManager : MonoBehaviour
{
    public AudioSource musicSource;
    private bool isPlaying = false;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // El MasterClient controla la música
            GetComponent<PhotonView>().RPC("PlayBackgroundMusic_RPC", RpcTarget.AllBuffered, PhotonNetwork.Time);
        }
    }

    [PunRPC]
    private void PlayBackgroundMusic_RPC(double startTime)
    {
        // Calcular el tiempo sincronizado
        double timeElapsed = PhotonNetwork.Time - startTime;

        if (!isPlaying)
        {
            musicSource.time = (float)timeElapsed; // Ajustar la posición de reproducción
            musicSource.Play();
            isPlaying = true;
        }
    }
}

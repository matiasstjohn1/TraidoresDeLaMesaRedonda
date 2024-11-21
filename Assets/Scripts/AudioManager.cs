using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AudioManager : MonoBehaviourPun
{
    public static AudioManager instance;

    public List<AudioClip> audioClipsAssets = new List<AudioClip>();

    private AudioSource audioSource;

    public float defaultVolume = 0.5f;
    private bool isPlaying = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
       
        DontDestroyOnLoad(gameObject); //Impido q se destruya en escena.
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = defaultVolume;
        audioSource.loop=false;
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
    public void StopSounds()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    //RPC para reproducir un sonido en todos los jugadores.
    [PunRPC]
    public void PlayCombatSounds_RPC(int soundIndex, double startTime)
    {
        if (soundIndex >= 0 && soundIndex < audioClipsAssets.Count)
        {
            double timeElapsed = PhotonNetwork.Time - startTime;

            if (!isPlaying)
            {
                audioSource.clip = audioClipsAssets[soundIndex];
                audioSource.time = (float)timeElapsed; 
                audioSource.Play();
                isPlaying = true;
            }
        }
    }
    public void PlayCombatSoundForAll(int soundIndex)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GetComponent<PhotonView>().RPC("PlayCombatSounds_RPC", RpcTarget.AllBuffered, soundIndex, PhotonNetwork.Time);
        }
    }
}

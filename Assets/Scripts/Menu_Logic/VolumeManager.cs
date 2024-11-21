using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public float sliderValue;
    public Image volumeImage;

    void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("volumeAudio", 0.5f);
        AudioListener.volume=volumeSlider.value;
        RevisarMute();
    }

    public void ChangeSlider(float valor)  
    { 
        sliderValue = valor;
        PlayerPrefs.SetFloat("volumeAudio", sliderValue);
        AudioListener.volume = volumeSlider.value;
        RevisarMute();
    }

    public void RevisarMute()
    {
        if(sliderValue==0)
        {
            volumeImage.enabled = true;
        }
        else
        {
            volumeImage.enabled = false;
        }
    }
}

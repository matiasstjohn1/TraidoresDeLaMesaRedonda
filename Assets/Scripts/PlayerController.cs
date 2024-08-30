using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    private PhotonView pv;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();  
        if (pv.IsMine)
        {
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (CardsManager.Instance != null && CardsManager.Instance.sprite.Count > 0)
        {
            spriteRenderer.sprite = CardsManager.Instance.sprite[CardsManager.Instance.c-1];
        }
        else
        {
            Debug.LogWarning("No hay cartas disponibles.");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectButton : MonoBehaviour
{
    public void OnProtectButtonClick()
    {
        TurnsSystem.Instance.ActivateProtectionFromButton(this.transform.position);
    }
    public void OnKickButtonClick()
    {
        TurnsSystem.Instance.ButtonKick(this.transform.position);
    }
}

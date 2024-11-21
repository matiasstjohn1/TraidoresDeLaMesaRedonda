using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    private PlayerController _playerController;


    void Start()
    {
        _playerController=GetComponent<PlayerController>();
    }

    void Update()
    {
    }

    public void Skill0() //Rey.
    {
        Debug.Log("Rey Arturo revelo su carta");
    }
    public void Skill1() //Mago.
    {
        Debug.Log("Merlin vio una carta");
    }
    public void Skill2() //Killer.
    {
        Debug.Log("Mordred mato a alguien");
    }
    public void Skill3() //ShieldMaster.
    {
        Debug.Log("Lancelot protegio a alguien");
    }
    public void ExecuteSkill()
    {
        if (_playerController != null && _playerController.playerID == 0)
        {
            Skill0();
        }
        if (_playerController != null && _playerController.playerID == 1)
        {
            Skill1();
        }
        if (_playerController != null && _playerController.playerID == 2)
        {
            Skill2();
        }
        if (_playerController != null && _playerController.playerID == 3)
        {
            Skill3();
        }
    }

}

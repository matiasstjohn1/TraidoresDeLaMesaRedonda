using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance;
    public List<Sprite> sprite = new List<Sprite>();
    public List<int> usedids = new List<int>();
    public int c;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

    }

    private void Start()
    {

    }

    void Update()
    {

    }
}

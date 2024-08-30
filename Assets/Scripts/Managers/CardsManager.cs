using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance;
    public List<Sprite> sprite = new List<Sprite>();
    public int c;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
        c=Random.Range(0,sprite.Count);
    }

    private void Start()
    {
        if (sprite[c] == null)
        {
            c += 1;
            if(c >=sprite.Count)
            {
                c=Random.Range(0,sprite.Count);
            }
        }
        sprite.Remove(sprite[c]);
    }

    void Update()
    {
        
    }
}

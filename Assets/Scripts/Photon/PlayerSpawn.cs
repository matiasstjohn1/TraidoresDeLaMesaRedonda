using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;

    public List<Transform> spawnPositions = new List<Transform>();
    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int[] spawnIndices = GenerateSpawnIndices();
            pv.RPC("SetSpawns", RpcTarget.AllBuffered, spawnIndices);
        }
    }

    private int[] GenerateSpawnIndices()
    {
        int[] indices = new int[spawnPositions.Count];
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            indices[i] = i;
        }

        System.Random rand = new System.Random();
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            int j = rand.Next(i, spawnPositions.Count);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }
        return indices;
    }

    [PunRPC]
    private void SetSpawns(int[] spawnIndices)
    {
        for (int i = 0; i < spawnIndices.Length; i++)
        {
            int spawnIndex = spawnIndices[i];
            if (spawnIndex < spawnPositions.Count)
            {
                PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPositions[spawnIndex].position, Quaternion.identity);
            }
        }
    }
}

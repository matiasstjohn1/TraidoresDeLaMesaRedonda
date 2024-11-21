using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private PhotonView pv;
    private SpriteRenderer spriteRenderer;

    public enum Role { King, Mage, Killer, ShieldMaster, Bystander, Bystander2 }
    public Role playerRole;
    public int playerID;

    //Panel de texto para el Player
    public GameObject rolePanelPrefab;
    private GameObject rolePanel;
    private Text roleText;
    [SerializeField] private TMPro.TextMeshPro nicknameText;

    //Fondo negro para el Player
    public GameObject fondoPrefab;
    private GameObject fondo;

    private static List<PlayerController> allPlayers = new List<PlayerController>();
    public bool isDead=false;
    public bool isProtect = false;
    public bool iSkip = false;
    public int votePlayerOk=0;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(pv.IsMine)
        {
            nicknameText.text = PhotonNetwork.NickName.ToString();
            Debug.Log(PhotonNetwork.NickName);
        }
        else
        {
            nicknameText.text = pv.Owner.NickName.ToString();
            Debug.Log(pv.Owner.NickName);
        }
    }

    private void Start()
    {
        rolePanel = Instantiate(rolePanelPrefab, GameObject.Find("Canvas").transform);
        rolePanel.SetActive(false);
        allPlayers.Add(this);

        fondo = Instantiate(fondoPrefab, GameObject.Find("Canvas").transform);
        fondo.SetActive(false);

        //El master de aca permite ver las cartas en una mismas posiciones.
        if (PhotonNetwork.IsMasterClient && allPlayers.Count == 6)
        {
            AssignRolesToAllPlayers();
            photonView.RPC("FindPlayers", RpcTarget.All);
            StartCoroutine(TurnsSystem.Instance.SetupGame()); 
        }
    }
    [PunRPC]
    private void FindPlayers()
    {
        TurnsSystem.Instance.players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
    }
    public void AssignRolesToAllPlayers()
    {
        List<Role> rolesAvailable = new List<Role> { Role.King, Role.Mage, Role.Killer, Role.ShieldMaster, Role.Bystander, Role.Bystander2 };
        int playerCount = PhotonNetwork.PlayerList.Length;

        for (int i = 0; i < playerCount; i++)
        {
            int randomIndex = Random.Range(0, rolesAvailable.Count);
            Role assignedRole = rolesAvailable[randomIndex];
            rolesAvailable.RemoveAt(randomIndex);

            PlayerController playerController = allPlayers[i];
            playerController.pv.RPC("ReceiveRole", RpcTarget.AllBuffered, (int)assignedRole);
        }
    }

    [PunRPC]
    private void ReceiveRole(int roleIndex)
    {
        playerRole = (Role)roleIndex;
        if (pv.IsMine)
        {
            LoadPlayerCard();
            ShowRolePanel();
        }
    }

    private void LoadPlayerCard()
    {
        int spriteIndex = (int)playerRole;
        if (CardsManager.Instance != null && CardsManager.Instance.sprite.Count > spriteIndex)
        {
            spriteRenderer.sprite = CardsManager.Instance.sprite[spriteIndex];
        }
    }

    //Ver el panel al inicio del juego//
    private void ShowRolePanel()
    {
        roleText = rolePanel.GetComponentInChildren<Text>();
        roleText.text = "Ahora tú eres: " + playerRole.ToString();
        rolePanel.SetActive(true);
        StartCoroutine(HideRolePanelAfterDelay(10f));
    }

    private IEnumerator HideRolePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rolePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        allPlayers.Remove(this);
    }

    //Cuando muere el player//
    public void DeadPlayer()
    {
       if(isDead == true)
         pv.RPC("ChangeColorToRed", RpcTarget.All);
    }

    [PunRPC]
    private void ChangeColorToRed()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
        if (pv.IsMine)
        {
           ActiveFondo();
        }
    }
    private void ActiveFondo()
    {
        fondo.SetActive(true);
    }

    //Cuando Merlin ve tu rol//
    public void ShowRoleMerlin(string revealedRole)
    {
        if(pv.IsMine == true)
        {
            roleText = rolePanel.GetComponentInChildren<Text>();
            roleText.text = "El rol es: " + revealedRole;
            rolePanel.SetActive(true);
            StartCoroutine(HideRolePanelAfterDelay(20f));
        }
    }
    public void ShieldAlert()
    {
        if (pv.IsMine == true)
        {
            roleText = rolePanel.GetComponentInChildren<Text>();
            roleText.text = "En este turno no puedes proteger ";
            rolePanel.SetActive(true);
            StartCoroutine(HideRolePanelAfterDelay(10f));
        }
    }

    //Cuando el jugador se exilia//
    [PunRPC]
    public void Suicide_RPC()
    {
        isDead = true;
        DeadPlayer();
    }
    //Cuenta votos de todos los jugadores a 1 solo//
    [PunRPC]
    public void CountVotes()
    {
        votePlayerOk += 1;
    }
    //Cuenta los skips de todos los jugadores//
    [PunRPC]
    public void CountSkips()
    {
        iSkip =true;
    }
}

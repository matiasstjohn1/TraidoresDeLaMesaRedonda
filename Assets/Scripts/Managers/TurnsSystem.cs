using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Text.RegularExpressions;
using Photon.Pun.Demo.PunBasics;

public enum TurnState { START, ACCIONES,DEBATE,EXPULSAR, WON, LOST }

public class TurnsSystem : MonoBehaviourPunCallbacks
{
    public static TurnsSystem Instance;
    public TurnState state;

    public List<GameObject> players;
    [SerializeField] GameObject[] BotonesGenerales;
    public bool isKingReveal = false;
    public string rol;
    private int dias;
    int t;


    [Header("Botones")]
    public GameObject BotonKill;
    public GameObject[] buttonsKill;
    public GameObject BotonMerlin;
    public GameObject[] buttonsReveal;
    public GameObject BotonShield;
    public GameObject[] buttonsShield;
    public GameObject BotonVote;

    [Header("Textos")]
    public GameObject TextoRevelar;
    [SerializeField] string textoEvento;
    public ScrollRect eventHistoryScrollRect;
    private string kingPlayerName = "";

    [Header("Historial de Eventos")]
    public List<string> eventHistory = new List<string>(); //Almacena los textos del historial.
    public TMPro.TextMeshProUGUI eventHistoryText;

    [Header("Sistema de Votación")]
    public Dictionary<GameObject, int> voteCount = new Dictionary<GameObject, int>(); //Rastrea los votos por jugador
    public int requiredVotes = 5;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

    }
    void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        TextoRevelar = canvas.transform.Find("TEXTOS").gameObject;
        photonView.RPC("SyncState", RpcTarget.All, TurnState.START);
    }

    void Update()
    {
        foreach (var player in players)
        {
            if (!voteCount.ContainsKey(player))
                voteCount[player] = 0;

            PlayerController pc = player.GetComponent<PlayerController>();

            if (pc.playerRole == PlayerController.Role.King && pc.isDead)
                GameManager.Instance.isKingDead = true;
            if (pc.playerRole == PlayerController.Role.Mage && pc.isDead)
                GameManager.Instance.isMageDead = true;
            if (pc.playerRole == PlayerController.Role.Killer && pc.isDead)
                GameManager.Instance.isKillerDead = true;
        }
    }
    public IEnumerator SetupGame()
    {
        if (dias > 1)
            photonView.RPC("Syncdays", RpcTarget.All, 0);
        if (GameManager.Instance.isKingDead == true && GameManager.Instance.isMageDead == true)
        {
            GameManager.Instance.TriggerDefeat();
        }
        if (GameManager.Instance.isKillerDead == true)
        {
            GameManager.Instance.TriggerWin();
        }
        photonView.RPC("SyncTextAndButtons", RpcTarget.All, false, ""); //Oculta todos los textos y botones
        yield return new WaitForSeconds(10f);

        int aliveCount = 0;

        foreach (var player in players)
        {
            player.GetComponent<PlayerController>().isProtect=false;
            //Verifico si el jugador no está muerto
            if (!player.GetComponent<PlayerController>().isDead)
            {
                aliveCount++;
            }
            player.GetComponent<PlayerController>().iSkip = false;
        }
        requiredVotes = aliveCount;

        photonView.RPC("SyncState", RpcTarget.All, TurnState.ACCIONES);
        StartCoroutine(ActionTurn());
    }
    public IEnumerator ActionTurn()
    {
        //Desactivamos los botones
        foreach (var boton in BotonesGenerales)
        {
            boton.SetActive(false);
        }

        //Activo boton habilidades
        if (BotonesGenerales.Length > 0)
        {
            photonView.RPC("SyncButton", RpcTarget.All, BotonesGenerales[0].name, true);
        }
        yield return new WaitForSeconds(60f); //Tiempo a cambiar
        photonView.RPC("SyncState", RpcTarget.All, TurnState.DEBATE);
        StartCoroutine(DebateTurn());
    }

    public IEnumerator DebateTurn()
    {
        photonView.RPC("SyncButton2", RpcTarget.All, BotonKill.name, false);
        photonView.RPC("SyncButton2", RpcTarget.All, BotonMerlin.name, false);
        photonView.RPC("SyncButton2", RpcTarget.All, BotonKill.name, false);
        AudioManager.instance.PlayCombatSoundForAll(0);
        
        photonView.RPC("SyncTextAndButtons", RpcTarget.All, true, "Turno de debate");

        TextoRevelar.SetActive(true);
        foreach (var boton in BotonesGenerales)
        {
            photonView.RPC("SyncButton", RpcTarget.All, boton.name, true);
        }
        //Revela quien es Arturo//
        if (isKingReveal)
        {
            string kingRevealedText = "El Rey Arturo es: " + kingPlayerName;
            AddEventToHistory(kingRevealedText);
            textoEvento = kingRevealedText;
        }
        //For each de cada player buscar isProtect o isDead//
        try
        {
            foreach (var player in players)
          {
            if(player.GetComponent<PlayerController>().isProtect)
            {
                string protectionText = "Un jugador fue protegido.";
                AddEventToHistory(protectionText);
                textoEvento = protectionText;
                //yield return new WaitForSeconds(2f);
            }
            if (player.GetComponent<PlayerController>().isDead)
            {               
                string deathText = "Murió un jugador.";
                AddEventToHistory(deathText);
                textoEvento = deathText;
                players.Remove(player);
            }
          }
        }
        catch
        {
            Debug.Log("ERROR");
        } 

        photonView.RPC("SyncButton", RpcTarget.All, BotonesGenerales[1].name, false);
        photonView.RPC("SyncButton", RpcTarget.All, BotonesGenerales[2].name, false);
        yield return new WaitForSeconds(5f);

        string debateText = "Debate en progreso.";
        AddEventToHistory(debateText);
        textoEvento = debateText;

        yield return new WaitForSeconds(60f);
        photonView.RPC("SyncState", RpcTarget.All, TurnState.EXPULSAR);
        StartCoroutine(ExpulsionTurn());
    }
    public IEnumerator ExpulsionTurn()
    {
        photonView.RPC("SyncButton", RpcTarget.All, BotonesGenerales[1].name, true);
        photonView.RPC("SyncButton", RpcTarget.All, BotonesGenerales[2].name, true);

        photonView.RPC("SyncTextAndButtons", RpcTarget.All, true, "Turno de expulsión. ¡Voten!");
        yield return new WaitForSeconds(60f);

        ExpulsePlayerWithMostVotes();

        AudioManager.instance.PlayCombatSoundForAll(1);
        ResetVotes();

        t += 1;
        photonView.RPC("Syncdays", RpcTarget.All, t);
        photonView.RPC("SyncState", RpcTarget.All, TurnState.START);
        StartCoroutine(SetupGame());
    }

    //Eventos//
    [PunRPC]
    private void SyncTextAndButtons(bool showText, string text)
    {
        TextoRevelar.SetActive(showText);
        if (TextoRevelar.TryGetComponent<Text>(out Text texto))
            texto.text = text;

        foreach (var boton in BotonesGenerales)
        {
            boton.SetActive(showText);
        }
    }
    [PunRPC]
    private void Syncdays(int num)
    {
        dias=num;
    }


    [PunRPC]
    private void SyncButton(string buttonName, bool state)
    {
        foreach (var boton in BotonesGenerales)
        {
            if (boton.name == buttonName)
                boton.SetActive(state);
        }
    }
    [PunRPC]
    private void SyncButton2(string boton, bool state)
    {
         if (boton == BotonKill.name)
            BotonKill.SetActive(state);
        if (boton == BotonMerlin.name)
            BotonKill.SetActive(state);
        if (boton == BotonShield.name)
            BotonKill.SetActive(state);
    }

    [PunRPC]
    private void SyncState(TurnState newState)
    {
        state = newState;
    }
    public void AddEventToHistory(string newEvent)
    {
        photonView.RPC("SyncEventHistory", RpcTarget.All, newEvent);
    }


    [PunRPC]
    private void SyncEventHistory(string newEvent)
    {
        eventHistory.Add(newEvent);
        if (eventHistoryText != null)
        {
            eventHistoryText.text = "";

            //Añade eventos al texto del historial
            foreach (string eventText in eventHistory)
            {
                eventHistoryText.text += eventText + "\n"; //Agrega evento como nueva línea
            }

            //Desplazar el ScrollRect hacia abajo automáticamente
            if (eventHistoryScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();  //Se actualiza en canvas
                eventHistoryScrollRect.verticalNormalizedPosition = 0f;  //Desplo hacia abajo
            }
        }
    }

    //BOTONES//

    //EMPIEZA SECCION DE VOTACION//
    public void ButtonVotar()
    {
        if (state == TurnState.EXPULSAR)
        {
            foreach (var play in players)
            {
                var playerController = play.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    BotonVote.SetActive(true);
                }
            }
        }
    }

    //Resetear votos//
    public void ResetVotes()
    {
        foreach (var player in players)
        {
            player.GetComponent<PlayerController>().votePlayerOk = 0;
            player.GetComponent<PlayerController>().iSkip = false;
        }
    }

    //Boton de Skip
    public void ButtonSkip()
    {
        foreach (var player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                player.GetComponent<PlayerController>().photonView.RPC("CountSkips", RpcTarget.All);
                Debug.Log("Decidiste skipear la votación.");

                BotonesGenerales[1].SetActive(false);
                BotonesGenerales[2].SetActive(false);
                BotonVote.SetActive(false);
            }
        }
    }

    // Boton de Expulsar
    public void ButtonKick(Vector3 buttonWorldPosition)
    {
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var player in players)
        {
            Vector3 playerPosition = player.transform.position;
            float distance = (buttonWorldPosition - playerPosition).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            closestPlayer.GetComponent<PlayerController>().photonView.RPC("CountVotes", RpcTarget.All);
            BotonVote.SetActive(false);
            BotonesGenerales[1].SetActive(false);
            BotonesGenerales[2].SetActive(false);
        }
    }

    //Método que saca al player con mas votos
    public void ExpulsePlayerWithMostVotes()
    {
        GameObject playerToExpel = null;
        int maxVotes = 0;
        int skip=0;

        foreach (var player in players)
        {
            int votes = player.GetComponent<PlayerController>().votePlayerOk;
            
            if (votes > maxVotes)
            {
                maxVotes = votes;  
                playerToExpel = player; 
            }
            if (player.GetComponent<PlayerController>().iSkip)
            {
                Debug.Log($"Jugador {player.name} decidió skipear la votación.");
                skip += 1;
            }
        }
        if(skip > maxVotes)
        {
            playerToExpel=null;
        }
        if (playerToExpel != null)
        {
            photonView.RPC("KillPlayer_RPC", RpcTarget.All, playerToExpel.GetPhotonView().ViewID);
            Debug.Log($"Jugador fue expulsado.");
            AddEventToHistory($"Jugador {PhotonNetwork.NickName} fue exiliado de Camelot.");
        }
    }
    //TERMINA SECCION DE VOTACION//

    //EMPIEZA SECCION DE HABILIDAD//
    public void ButtonHability()
    {
        if (state == TurnState.ACCIONES)
        {
            ActivateSubButtons();
        }
    }

    private void ActivateSubButtons()
    {
        foreach (var play in players)
        {
            var playerController = play.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //Verificamos si jugador es él.
                if (play.GetComponent<PhotonView>().IsMine)
                {
                    //Dependiendo del rol del jugador, activamos los sub-botones correspondientes
                    if (playerController.playerRole == PlayerController.Role.Killer)
                    {
                        BotonKill.SetActive(true);
                    }
                    else if (playerController.playerRole == PlayerController.Role.King)
                    {
                        photonView.RPC("KingReveal_RPC", RpcTarget.All, PhotonNetwork.NickName);
                        BotonesGenerales[0].SetActive(false);
                    }
                    else if (playerController.playerRole == PlayerController.Role.Mage)
                    {
                        BotonMerlin.SetActive(true);
                    }
                    else if (playerController.playerRole == PlayerController.Role.ShieldMaster&&dias==0)
                    {
                        BotonShield.SetActive(true);
                    }
                    else if (playerController.playerRole == PlayerController.Role.ShieldMaster && dias != 0)
                    {
                        play.GetComponent<PlayerController>().ShieldAlert();
                    }

                }
            }
        }
    }

    //Activacion de las habilidades segun el rol//
    public void ActivateProtectionFromButton(Vector3 buttonWorldPosition)
    {
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var player in players)
        {
            Vector3 playerPosition = player.transform.position;
            float distance = (buttonWorldPosition - playerPosition).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            foreach (var play in players)
            {
                var playerController = closestPlayer.GetComponent<PlayerController>();
                if (play.GetComponent<PhotonView>().IsMine)
                {
                    if (play.GetComponent<PlayerController>().playerRole == PlayerController.Role.Mage)
                    {
                        string roleOfClosestPlayer = playerController.playerRole.ToString();
                        play.GetComponent<PlayerController>().ShowRoleMerlin(roleOfClosestPlayer);
                        BotonMerlin.SetActive(false);
                        BotonesGenerales[0].SetActive(false);
                    }
                    if (play.GetComponent<PlayerController>().playerRole == PlayerController.Role.Killer)
                    {
                        photonView.RPC("KillPlayer_RPC", RpcTarget.All, closestPlayer.GetPhotonView().ViewID);
                        BotonKill.SetActive(false);
                        BotonesGenerales[0].SetActive(false);
                    }
                    if (play.GetComponent<PlayerController>().playerRole == PlayerController.Role.ShieldMaster)
                    {
                        photonView.RPC("ProtectPlayer_RPC", RpcTarget.All, closestPlayer.GetPhotonView().ViewID);
                        BotonShield.SetActive(false);
                        BotonesGenerales[0].SetActive(false);
                    }
                }
            }
        }
    }
    //Metodos PUN para cada acción//
    [PunRPC]
    private void ProtectPlayer_RPC(int playerViewID)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        player.GetComponent<PlayerController>().isProtect = true;
    }

    [PunRPC]
    private void KillPlayer_RPC(int playerViewID)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        if (player.GetComponent<PlayerController>().isProtect == false)
        {
            player.GetComponent<PlayerController>().isDead = true;
            player.GetComponent<PlayerController>().DeadPlayer();
        }
    }

    [PunRPC]
    private void KingReveal_RPC(string name)
    {
        TurnsSystem.Instance.isKingReveal=true;
        kingPlayerName = name;
    }

    //TERMINA SECCION DE HABILIDAD//
    //EMPIEZA SECCION DE BOTONES EXTRAS//
    public void ButtonSuicide()
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                if (player.GetComponent<PlayerController>().photonView.IsMine == true)
                {
                    player.GetComponent<PlayerController>().photonView.RPC("Suicide_RPC", RpcTarget.All);
                    //Añade el evento al historial de la partida
                    AddEventToHistory($"Jugador {PhotonNetwork.NickName} se exilió de Camelot.");
                }
            }
        }
    }

    //Boton de Chat global
    public void ButtonChatGlobal()
    {

    }
}

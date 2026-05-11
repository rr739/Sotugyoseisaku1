using UnityEngine;

using WebSocket = NativeWebSocket.WebSocket;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net.WebSockets;
using Unity.Mathematics;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Collections.Generic;
using System.Security.Cryptography;

public class TopViewClient : MonoBehaviour
{
    [SerializeField] InputField inputPlayerName;
    [SerializeField] InputField  inputRoomId;
    
    PlayerManager pm;
    public WebSocket ws;

    public string myPlayerId; // 自分のプレイヤーID
    public string myRoomID;
    public int myPlayerIndex;

    public GameObject InputPanel;
    public GameObject LobbyPanel;

    public Text P1Text;
    public Text P2Text;

    string remotePlayer;
    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>(); // プレイヤーの一覧

    void Start()
    {
        // pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

        Init(true,false);
    }

    void Update()
    {

        

#if !UNITY_WEBGL || UNITY_EDITOR
        if (ws != null)
        {
            ws.DispatchMessageQueue();
        }
#endif
    }
    void HandleMessage(string msg)
    {
        var res = JsonUtility.FromJson<InitResponse>(msg);

        if (res.type == "init")
        {
            // 自分のIDを保存
            myPlayerId = res.name_id;
            myRoomID   = res.room_id;
            myPlayerIndex = res.index;

            Debug.Log($"<color=cyan>【システム】接続完了。自分のID: {res.name_id}, 入室順: {res.index}</color>");

        

           
            // プレイヤーの初期化
            //CreatePlayer(res.name_id, Vector3.zero,res.index);

           
        }
        else
        {
            HandleWebSocketMessage(msg);
        }

        LobbyList(res.name_id, res.index);
    }
    
    async void Connect(string playerID, string roomID, int playerIndex)
    {
        
        ws = new WebSocket($"ws://10.22.8.43:8080/ws?room_id={roomID}&name_id={playerID}"); 
        ws.OnOpen += () =>
        {
            print("接続成功");
            //SceneManager.LoadScene("GameScene");
        };

        ws.OnMessage += (bytes) =>
        {
            var msg = System.Text.Encoding.UTF8.GetString(bytes);
            print("受信メッセージ：" + msg);
            HandleMessage(msg);
        };

        await ws.Connect();
    }
    public void PushJoinButton()
    {
        var playerNameInput = inputPlayerName.text; // 変数名をわかりやすく
        var roomIdInput = inputRoomId.text;

        // デバッグ：ここが空になっていないかチェック！
        Debug.Log($"接続試行: Name={playerNameInput}, Room={roomIdInput}");

        if (string.IsNullOrEmpty(roomIdInput) || string.IsNullOrEmpty(playerNameInput))
        {
            print("ルームID、プレイヤー名は必須です");
            return;
        }

        Init(false,true);
        

        Connect(playerNameInput, roomIdInput,myPlayerIndex); 
    }

    private void Init(bool IP, bool LP)
    {
        InputPanel.SetActive(IP);
        LobbyPanel.SetActive(LP);
    }

    private void LobbyList(string playerID, int playerIndex)
    {
        

        if (playerIndex == 0)
        {
            P1Text.text = playerID;

            P2Text.text = remotePlayer;
        }
        else if (playerIndex == 1)
        {
            P1Text.text = remotePlayer;
            P2Text.text = playerID;
        }
    }

  

    private void HandleWebSocketMessage(string msg)
    {
        var playerData = JsonUtility.FromJson<PlayerData>(msg);

        if (!players.ContainsKey(playerData.name_id))
        {
            // リストに存在しなければ登録
            remotePlayer = playerData.name_id;
        }
        else
        {
           
        }
    }
}
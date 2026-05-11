using UnityEngine;

using WebSocket = NativeWebSocket.WebSocket;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net.WebSockets;
using Unity.Mathematics;
using UnityEngine.UIElements;

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
            //pm.HandleWebSocketMessage(msg);
        }
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
        LobbyList();

        Connect(playerNameInput, roomIdInput,myPlayerIndex); 
    }

    private void Init(bool IP, bool LP)
    {
        InputPanel.SetActive(IP);
        LobbyPanel.SetActive(LP);
    }

    private void LobbyList()
    {
        if(myPlayerIndex == 0)
        {
            P1Text.text = inputPlayerName.text;
        }
        else if (myPlayerIndex == 1)
        {
            P2Text.text = inputPlayerName.text;
        }
    }
}
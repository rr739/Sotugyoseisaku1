using UnityEngine;
using UnityEngine.UI;
using WebSocket = NativeWebSocket.WebSocket;
using System.Net.WebSockets;

public class TopViewClient : MonoBehaviour
{

    [SerializeField] InputField inputRoomId;
    [SerializeField] InputField inputPlayerName;
    PlayerManager pm;
    public WebSocket ws;
    void Start()
    {
        pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
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
            pm.myPlayerId = res.id;
            pm.myPlayerIndex = res.index;

            Debug.Log($"<color=cyan>【システム】接続完了。自分のID: {res.id}, 入室順: {res.index}</color>");

            // プレイヤーの初期化
            pm.CreatePlayer(res.id, Vector3.zero, true);
        }
        else
        {
            pm.HandleWebSocketMessage(msg);
        }
    }
    
    async void Connect(string roomID, string playerName)
    {
        ws = new WebSocket($"ws://192.168.56.102:8080/ws?room_id={roomID}&name={playerName}");

        ws.OnOpen += () =>
        {
            print("接続成功");
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
        var roomId = "room123";
        var playerName = inputPlayerName.text;

        if (roomId == "" || playerName == "")
        {
            print("ルームID、プレイヤー名は必須です");
            return;
        }

        Connect(roomId, playerName);
    }

}
using UnityEngine;
using WebSocket = NativeWebSocket.WebSocket;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement; 

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public WebSocket ws;
    public string myPlayerId;
    public string myRoomID;
    public int myPlayerIndex;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
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

    public async void Connect(string playerID, string roomID)
    {
        myPlayerId = playerID;
        myRoomID = roomID;


        ws = new WebSocket($"ws://10.22.1.234:8080/ws?room_id={roomID}&name_id={playerID}");

        

        ws.OnOpen += () =>
        {
            print("接続成功");
        };

        ws.OnMessage += (bytes) =>
        {
            var msg = System.Text.Encoding.UTF8.GetString(bytes);
            print("受信メッセージ：" + msg);

            //  現在アクティブなシーンの名前を取得する
            string currentSceneName = SceneManager.GetActiveScene().name;

            //  シーン名に応じて、届いたデータの届け先を完全に仕分ける！
            if (currentSceneName == "SecondScene" || currentSceneName == "TopViewScene")
            { 
                var client = FindObjectOfType<TopViewClient>();
                if (client != null)
                {
                    client.HandleMessage(msg);
                }
            }
            else if (currentSceneName == "CharacterSelectScene")
            {
              
                var charManager = FindObjectOfType<CharacterSelectManager>();
                if (charManager != null)
                {
                    charManager.HandleRemoteMessage(msg);
                }
            }
            else if (currentSceneName == "GameScene")
            {
                
            }
            else if (currentSceneName == "StageSelectScene")
            {
                var stageManager = FindObjectOfType<StageManager>();
                if (stageManager != null)
                {
                    stageManager.HandleRemoteStageMessage(msg); 
                }
            }
        };

        await ws.Connect();
    }

    public async Task SendMessageAsync(string jsonMsg)
    {
        if (ws != null && ws.State == NativeWebSocket.WebSocketState.Open)
        {
            await ws.SendText(jsonMsg);
        }
        else
        {
            Debug.LogWarning("接続切断");
        }
    }

    private async void OnApplicationQuit()
    {
        if (ws != null)
        {
            await ws.Close();
            DeleteData();
        }
    }

    public void DeleteData()
    {
        ws = null;

        myPlayerId = string.Empty;
        myRoomID = string.Empty;

        myPlayerIndex = -1;

     

    }
}
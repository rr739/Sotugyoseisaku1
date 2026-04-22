using UnityEngine;
using SocketIOClient;
using System;

public class NetworkManager : MonoBehaviour
{
    public SocketIOUnity socket;

    [Header("Role Settings")]
    public bool isPlayer1; // Inspectorで片方をON、もう片方をOFFにしてビルドする

    [Header("Objects")]
    public GameObject player1;
    public GameObject player2;

    private GameObject myCharacter;    // 自分が動かすキャラ
    private GameObject otherCharacter; // ネットワークで動く相手のキャラ

    void Start()
    {
        // 1. 自分の役割を決める
        if (isPlayer1)
        {
            myCharacter = player1;
            otherCharacter = player2;
        }
        else
        {
            myCharacter = player2;
            otherCharacter = player1;
        }

        networkPosition = otherCharacter.transform.position;

        var uri = new Uri("http://192.168.56.102:3000");
        socket = new SocketIOUnity(uri);

        // 2. 受信処理
        socket.On("move", (response) => {
            string rawJson = response.ToString();
            string cleanedJson = rawJson.Substring(1, rawJson.Length - 2);
            PositionData data = JsonUtility.FromJson<PositionData>(cleanedJson);

            int myNumber = isPlayer1 ? 1 : 2;
            if (data.playerNumber != myNumber)
            {
                // ログを追加して、データが届いているか確認！
                Debug.Log($"相手(Player{data.playerNumber})の座標を受信: {data.x}, {data.y}");
                networkPosition = new Vector3(data.x, data.y, data.z);
            }
        });

        socket.Connect();
        StartCoroutine(SendPositionLoop());
    }

    private Vector3 networkPosition;

    void Update()
    {
        // 3. 自分のキャラを動かす（例：キーボード操作）
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        myCharacter.transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 5f);

        // 4. 相手のキャラを滑らかに動かす
        if (otherCharacter != null)
        {
            otherCharacter.transform.position = Vector3.Lerp(otherCharacter.transform.position, networkPosition, 0.1f);
        }

        
    }

    System.Collections.IEnumerator SendPositionLoop()
    {
        while (true)
        {
            if (socket.Connected)
            {
                var data = new
                {
                    playerNumber = isPlayer1 ? 1 : 2, // 自分が何番か送る
                    x = myCharacter.transform.position.x,
                    y = myCharacter.transform.position.y,
                    z = myCharacter.transform.position.z
                };
                socket.Emit("move", data);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}

[Serializable]
public class PositionData
{
    public int playerNumber;
    public float x, y, z;
}
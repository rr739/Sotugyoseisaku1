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

        var uri = new Uri("http://192.168.56.102:3000");
        socket = new SocketIOUnity(uri);

        // 2. 受信処理
        socket.On("move", (response) => {
            string rawJson = response.ToString();
            string cleanedJson = rawJson.Substring(1, rawJson.Length - 2);
            PositionData data = JsonUtility.FromJson<PositionData>(cleanedJson);

            // 届いたデータが「自分じゃない方」の番号なら、相手のキャラを動かす
            if (data.playerNumber != (isPlayer1 ? 1 : 2))
            {
                // UpdateでLerpするために座標を保存
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
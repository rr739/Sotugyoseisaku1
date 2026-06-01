using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.SocialPlatforms;

public class ObjectOnlineCommunication : MonoBehaviour
{
    [SerializeField] GameObject[] playersPrefab; // プレイヤーを表すPrefab
    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>(); // プレイヤーの一覧
    public string myPlayerId; // 自分のプレイヤーID

    /*[SerializeField] GameObject boxPrefab; // 動かしたい箱のPrefabなど
    private Dictionary<int, GameObject> stageObjects = new Dictionary<int, GameObject>();*/

    // プレイヤーが炎か氷かを取得
    void Start()
    {


        /*// 自分自身のキャラを生成
        if (NetworkManager.Instance != null)
        {
            int myId = NetworkManager.Instance.myCharaIndex; 
            int myCharaIndex = NetworkManager.Instance.myCharaIndex; 

            // 固有IDと、キャラの種類を別々で渡す
            CreatePlayer(myId, myCharaIndex, Vector3.zero, true);
        }*/

        if (NetworkManager.Instance != null)
        {
            int myCharaIndex = NetworkManager.Instance.myCharaIndex; // 0か1

            //  まず自分を生成 (初期位置は Vector3.zero などを適切な開始位置に)
            CreatePlayer(myCharaIndex, Vector3.zero, true);
            Debug.Log($"【初期化】自分のキャラ（{myCharaIndex}番）を生成しました。");

            // 相手のキャラも最初からシーンに配置しておく！
            // 自分が 0(炎) なら 相手は 1(氷) / 自分が 1(氷) なら 相手は 0(炎)
            int opponentCharaIndex = (myCharaIndex == 0) ? 1 : 0;

            // 相手の初期位置
            Vector3 opponentStartPos = new Vector3(2f, 0f, 0f);

            CreatePlayer(opponentCharaIndex, opponentStartPos, false);
            Debug.Log($"【初期化】相手のキャラ（{opponentCharaIndex}番）をあらかじめ生成しました。");
        }
    }

    public void CreatePlayer(int charaindex, Vector3 pos, bool isLocal)
    {

        // 生成するPrefabを選ぶために charaindex を使う
        var player = Instantiate(playersPrefab[charaindex], pos, Quaternion.identity);

        // Dictionaryには 0 または 1 をKeyにして保存する
        players[charaindex] = player;

        /*var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.isLocalPlayer = isLocal;
        }*/
    }
    public void HandleWebSocketMessage(string msg)
    {
        var data = JsonUtility.FromJson<InGameMoveData>(msg);

        if (data.dataType == "player")
        {
            HandlePlayerSync(data);
        }
        else if (data.dataType == "object")
        {
            //HandleObjectSync(data);
        }
    }

    

    private void HandlePlayerSync(InGameMoveData data)
    {
        // 【デバッグログ】何番のIDが送られてきているかコンソールで確認する
        Debug.Log($"受信したプレイヤーID: {data.id} / 自分のインデックス: {NetworkManager.Instance.myCharaIndex}");

        // 一番最初に自分かどうかをチェックする（まだ生成してない場合も含めて無視する）
        if (data.id == NetworkManager.Instance.myCharaIndex) return;

        Vector3 targetPos = new Vector3(data.position_x, data.position_y, 0);

        if (!players.ContainsKey(data.id))
        {
           
        }
        else
        {
            players[data.id].transform.position = targetPos;
        }
    }
   /* private void HandleObjectSync(InGameMoveData data)
    {
        Vector3 targetPos = new Vector3(data.position_x, data.position_y, 0);

        if (!stageObjects.ContainsKey(data.id))
        {
            var newObj = Instantiate(boxPrefab, targetPos, Quaternion.identity);
            stageObjects[data.id] = newObj;
        }
        else
        {
            stageObjects[data.id].transform.position = targetPos;
        }
    }*/
}

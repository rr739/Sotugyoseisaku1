using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.SocialPlatforms;

public class ObjectOnlineCommunication : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab; // プレイヤーを表すPrefab
    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>(); // プレイヤーの一覧
    public string myPlayerId; // 自分のプレイヤーID

    // プレイヤーが炎か氷かを取得
    void Start()
    {
        int charaindex = 0;


        // 自分自身のキャラを生成
        if (NetworkManager.Instance != null)
        {
            charaindex = NetworkManager.Instance.myCharaIndex;

            CreatePlayer(charaindex, Vector3.zero, true);
        }
    }

    public void CreatePlayer(int charaindex, Vector3 pos, bool isLocal)
    {
        // プレイヤーオブジェクト生成
        var canvas = GameObject.Find("Canvas");




        var player = Instantiate(playerPrefab, pos, Quaternion.identity, canvas.transform);


        // リスト追加
        players[charaindex] = player;
    }

    public void UpdatePlayer(InitResponse pd)
    {
        // 位置情報更新
        /*var player = players[pd.id];
        player.transform.position = new Vector3(pd.position_x, pd.position_y, 0);*/
    }

    public void HandleWebSocketMessage(string msg)
    { 
        var playerData = JsonUtility.FromJson<CharSelectData>(msg);


        

        if (!players.ContainsKey(playerData.char_index))
        {
            // リストに存在しなければ登録
            CreatePlayer(playerData.char_index, Vector3.zero, false);
        }
        else
        {
            // 存在すれば位置を移動
            UpdatePlayer(playerData);
        }
    }

}

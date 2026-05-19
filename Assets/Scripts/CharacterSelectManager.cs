using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("キャラのアイコン（選択肢）")]
    public RectTransform[] characterIcons;

    [Header("自分の選択枠（カーソル画像）")]
    public RectTransform selectionCursor;

    [Header("相手の選択枠（カーソル画像）")]
    public RectTransform remoteSelectionCursor;

    private int currentSelectIndex = 0; // 現在選んでいるキャラの番号 

    private int mySelectedChar = -1;     // 自分が確定したキャラ番号 
    private int remoteSelectedChar = -1; // 相手が確定したキャラ番号

    private bool isMySelectionConfirmed = false; // 自分が既に決定ボタンを押したかどうか

    public Text myInfoText;
    public Text otherInfoText;

    void Start()
    {
        int myIndex = 0;

        // 1P（Index 0）は 0番のキャラ、2P（Index 1）は 1番のキャラを初期位置にする
        if (NetworkManager.Instance != null)
        {
            myIndex = NetworkManager.Instance.myPlayerIndex;

            // 自分の番号を表示
            if (myInfoText != null)
                myInfoText.text = (myIndex == 0) ? "1P " : "2P";

            if (myIndex == 1 && characterIcons.Length > 1) currentSelectIndex = 1;

            
        }
        //  相手のテキストの初期表示を設定（自分が1Pなら相手は2P、自分が2Pなら相手は1P）
        if (otherInfoText != null)
        {
            int remoteIndex = (myIndex == 0) ? 2 : 1;
            otherInfoText.text = $"{remoteIndex}P";
        }

        UpdateCursorPosition();

        //  初期状態から相手のカーソルを見せるために最初からTrueにする、またはSetActiveを制御
        if (remoteSelectionCursor != null)
        {
            remoteSelectionCursor.gameObject.SetActive(true); // 常に表示
            // 相手の初期位置（1Pなら1、2Pなら0など）に適当に置いておく
            int remoteDefault = (NetworkManager.Instance != null && NetworkManager.Instance.myPlayerIndex == 0) ? 1 : 0;
            if (characterIcons.Length > remoteDefault)
            {
                remoteSelectionCursor.position = characterIcons[remoteDefault].position;
            }
        }
    }

    void Update()
    {
        // 既に決定（確定）しているなら、もうカーソル移動キーは受け付けない
        if (isMySelectionConfirmed) return;

        bool isMoved = false;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentSelectIndex++;
            if (currentSelectIndex >= characterIcons.Length) currentSelectIndex = 0;
            isMoved = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentSelectIndex--;
            if (currentSelectIndex < 0) currentSelectIndex = characterIcons.Length - 1;
            isMoved = true;
        }

        //カーソルが動いたら、位置を更新してサーバーにも送信する
        if (isMoved)
        {
            UpdateCursorPosition();
            SendCharacterState(currentSelectIndex, false); // is_ready = false (移動中)
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
        {
            SelectCharacter(currentSelectIndex);
        }
    }

    void UpdateCursorPosition()
    {
        if (characterIcons.Length == 0 || selectionCursor == null) return;
        selectionCursor.position = characterIcons[currentSelectIndex].position;
    }

    // データを送信する共通の関数を作りました
    //  これなら確実に動く！
    async void SendCharacterState(int index, bool isReady)
    {
        if (NetworkManager.Instance == null) return;

        CharSelectData msgData = new CharSelectData();

        // 親クラス（InitResponse）から受け継いだ大事な変数もすべて確実にセット！
        msgData.type = "char_select";
        msgData.name_id = NetworkManager.Instance.myPlayerId;
        msgData.room_id = NetworkManager.Instance.myRoomID;     // 👈 これが抜けてたから届かなかった！
        msgData.index = NetworkManager.Instance.myPlayerIndex;   // 👈 これも大事！
        msgData.IsStarted = false;

        // 子クラス（CharSelectData）の固有メンバー
        msgData.char_index = index;
        msgData.is_ready = isReady;

        string jsonMsg = JsonUtility.ToJson(msgData);
        await NetworkManager.Instance.SendMessageAsync(jsonMsg);
    }

    // キャラクターが確定した時の処理（決定キーを押したとき）
    void SelectCharacter(int index)
    {
        if (isMySelectionConfirmed) return;

        Debug.Log($"【自分】キャラ {index} 番で決定！サーバーに送信します。");

        mySelectedChar = index;
        isMySelectionConfirmed = true;

        //  決定フラグを true にして送信
        SendCharacterState(index, true);

        CheckBothPlayersReady();
    }

    // 外部（NetworkManager）からデータを受け取る部分
    public void HandleRemoteMessage(string msg)
    {
        var playerData = JsonUtility.FromJson<CharSelectData>(msg);
        if (playerData == null) return;

        if (playerData.type == "char_select")
        {
            // 相手からのデータの場合のみ処理
            if (playerData.name_id != NetworkManager.Instance.myPlayerId)
            {
                //  相手が「動かしただけ」でも「決定した」でも、カーソル位置はリアルタイムに同期する
                if (remoteSelectionCursor != null && characterIcons.Length > playerData.char_index)
                {
                    remoteSelectionCursor.gameObject.SetActive(true);
                    remoteSelectionCursor.position = characterIcons[playerData.char_index].position;
                }

                //  相手が「決定（is_ready == true）」した時だけ、選択番号を確定させる
                if (playerData.is_ready)
                {
                    remoteSelectedChar = playerData.char_index;
                    Debug.Log($"【同期】相手がキャラ {remoteSelectedChar} 番で決定しました。");
                    CheckBothPlayersReady();
                }
            }
        }
    }

    private void CheckBothPlayersReady()
    {
        if (mySelectedChar != -1 && remoteSelectedChar != -1)
        {
            if (mySelectedChar != remoteSelectedChar)
            {
                Debug.Log("お互い違うキャラが選ばれました！GameSceneへ遷移します。");
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogWarning("キャラが重複しています！選び直してください。");
                isMySelectionConfirmed = false;
                mySelectedChar = -1;

                //  被ったので、相手側にも自分が「未確定（移動中）」に戻ったことを通知する
                SendCharacterState(currentSelectIndex, false);
            }
        }
    }
}
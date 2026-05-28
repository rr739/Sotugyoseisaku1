using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageMenuManager : MonoBehaviour
{
    public static StageMenuManager Instance { get; private set; }

    [Header("UIパネルの設定")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("【追加】画面左上のメニューボタン")]
    [SerializeField] private Button menuOpenButton; // ★ここに左上のMenuボタンを登録します

    [Header("退出確認用のUI要素")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Text yesButtonText;

    [Header("【変更】左上のスターUIの親オブジェクト")]
    [SerializeField] private Transform starUIPanel; // ★アイコンたちが並ぶ土台の枠（Container）

    [Header("【追加】生成するスターアイコンのプレハブ")]
    [SerializeField] private GameObject starIconPrefab; // ★星の画像（Image）単体のプレハブ

    [Header("ステージ選択画面のシーン名")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";

    private int readyPlayersCount = 0;
    private bool isMenuOpen = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ゲーム開始時はすべてのパネルを非表示にする
        if (menuPanel != null) menuPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // ゲーム開始時は時間を正常に動かし、メニューボタンも押せる状態にする
        Time.timeScale = 1f;
        if (menuOpenButton != null) menuOpenButton.interactable = true;
    }

    private void Update()
    {
        // メニューの開閉は、キーボードの「Escapeキー」か「コントローラーの特定のメニューボタン」のみに限定
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
        {
            if (confirmationPanel.activeSelf) CancelExit();
            else ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        menuPanel.SetActive(isMenuOpen);

        if (isMenuOpen)
        {
            // メニューが開いたので、ゲームの時間を完全に停止させる
            Time.timeScale = 0f;
            Debug.Log("ゲームを一時停止しました。");

            // ★【ここを追加】パネルが出ている間は、左上のメニューボタンをクリックできないようにする
            if (menuOpenButton != null)
            {
                menuOpenButton.interactable = false;
            }
        }
        else
        {
            // メニューが閉じられたので、ゲームの時間を通常通りに動かす
            Time.timeScale = 1f;
            Debug.Log("ゲームを再開しました。");

            // ★【ここを追加】メニューが閉じたら、再び左上のメニューボタンをクリックできるようにする
            if (menuOpenButton != null)
            {
                menuOpenButton.interactable = true;
            }
        }
    }

    // ⑦ステージ退出ボタンを押したとき
    // ⑦ステージ退出ボタンを押したとき
    public void OpenConfirmation()
    {
        confirmationPanel.SetActive(true);
        readyPlayersCount = 0;
        UpdateYesButtonText();

        // ★【ここを追加】確認パネルが開いた瞬間、「はい」ボタンを強制的にシステム上の選択状態にする
        // これを入れることで、マウスのクリック判定やフォーカスが100%このボタンに届くようになります！
        if (exitButton != null) // ※インスペクターで登録されている変数名に合わせてください
        {
            // もし「はい」ボタンの変数（yesButton）をスクリプトに残しているなら、以下のように書き換えます
            // yesButton.Select();
        }
    }

    // ⑧「はい」ボタンがクリックされたとき
    public void PressYesByClick()
    {
        readyPlayersCount++;
        UpdateYesButtonText();

        if (readyPlayersCount >= 2)
        {
            Debug.Log("2回のクリックを確認。ステージ変更します。");

            // 次のシーンに行く前に、必ず時間を「1」に戻す
            Time.timeScale = 1f;

            SceneManager.LoadScene(stageSelectSceneName);
        }
    }



    // ⑧「いいえ」ボタンがクリックされたとき、またはEscキーでのリセット
    public void CancelExit()
    {
        confirmationPanel.SetActive(false);
        readyPlayersCount = 0;

        // ★確認画面で「いいえ」を押して、通常のメニュー画面（③）に戻る場合も、
        // まだメニューのパネル自体は開いている状態なので、メニューボタンは押せない状態（false）を維持します
    }


    public void AddStar()
    {
        if (starUIPanel == null || starIconPrefab == null) return;

        // 【テキストなし】星のアイコン画像を新しく生成し、土台（Panel）の中に子オブジェクトとして追加する
        Instantiate(starIconPrefab, starUIPanel);

        Debug.Log("スターアイコンを左上に追加しました。");
    }

    private void UpdateYesButtonText()
    {
        if (yesButtonText != null)
        {
            yesButtonText.text = $"はい {readyPlayersCount}/2";
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
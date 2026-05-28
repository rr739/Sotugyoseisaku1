using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageMenuManager : MonoBehaviour
{
    public static StageMenuManager Instance { get; private set; }

    [Header("UIパネルの設定")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("画面左上のメニューボタン")]
    [SerializeField] private Button menuOpenButton;

    [Header("退出確認用のUI要素")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button yesButton;
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
        if (Instance == null)
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
        if (menuPanel != null) menuPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        Time.timeScale = 1f;
        if (menuOpenButton != null) menuOpenButton.interactable = true;

        // ★ゲーム開始時は土台の中身（初期からあるアイコン等）を念のためすべて消して空にする
        if (starUIPanel != null)
        {
            foreach (Transform child in starUIPanel)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void Update()
    {
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
            Time.timeScale = 0f;
            if (menuOpenButton != null) menuOpenButton.interactable = false;
        }
        else
        {
            Time.timeScale = 1f;
            if (menuOpenButton != null) menuOpenButton.interactable = true;
        }
    }

    public void OpenConfirmation()
    {
        confirmationPanel.SetActive(true);
        readyPlayersCount = 0;
        UpdateYesButtonText();

        if (yesButton != null)
        {
            yesButton.Select();
        }
    }

    public void PressYesByClick()
    {
        readyPlayersCount++;
        UpdateYesButtonText();

        if (readyPlayersCount >= 2)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }

    public void CancelExit()
    {
        confirmationPanel.SetActive(false);
        readyPlayersCount = 0;
    }

    // ★スターを拾ったときに呼び出される関数
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
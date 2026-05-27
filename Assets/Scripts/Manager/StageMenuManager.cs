using UnityEngine;
using UnityEngine.UI; // ★標準のTextを使うためにこの行が必要です（残しておきます）
using UnityEngine.SceneManagement;
// using TMPro; // ★【削除またはコメントアウト】TMPは使わないので消してOKです

public class StageMenuManager : MonoBehaviour
{
    [Header("UIパネルの設定")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("【追加】メニューを開くためのボタン")]
    [SerializeField] private Button menuOpenButton;

    [Header("退出確認用のUI要素")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button yesButton;

    // ★【修正】「TextMeshProUGUI」から「Text」に書き戻します
    [SerializeField] private Text yesButtonText;

    [Header("ステージ選択画面のシーン名")]
    [SerializeField] private string stageSelectSceneName = "StageSelect";

    private int readyPlayersCount = 0;
    private bool isMenuOpen = false;

    private void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (menuOpenButton != null) menuOpenButton.Select();
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
            if (exitButton != null) exitButton.Select();
        }
        else
        {
            if (menuOpenButton != null) menuOpenButton.Select();
        }
    }

    public void OpenConfirmation()
    {
        confirmationPanel.SetActive(true);
        readyPlayersCount = 0;
        UpdateYesButtonText();
        if (yesButton != null) yesButton.Select();
    }

    public void PressYes()
    {
        readyPlayersCount++;
        UpdateYesButtonText();
        if (readyPlayersCount >= 2)
        {
            SceneManager.LoadScene(stageSelectSceneName);
        }
    }

    public void CancelExit()
    {
        readyPlayersCount = 0;
        confirmationPanel.SetActive(false);
        if (exitButton != null) exitButton.Select();
    }

    private void UpdateYesButtonText()
    {
        if (yesButtonText != null)
        {
            // ★中身の処理はそのままで、標準Textに対応します
            yesButtonText.text = $"はい {readyPlayersCount}/2";
        }
    }
}
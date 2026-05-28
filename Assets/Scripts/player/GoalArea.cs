using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalArea : MonoBehaviour
{
    [Header("クリアに必要なプレイヤー人数")]
    [SerializeField] private int requiredPlayersToClear = 2;

    [Header("次に進むステージ（シーン名）")]
    [SerializeField] private string nextStageSceneName = "StageSelect";

    private int currentPlayersInGoal = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            currentPlayersInGoal++;

            // ★【ここを追加】ゴールに触れたプレイヤーをその場に固定する
            player.CanMove = false;

            Debug.Log($"プレイヤーがゴールに到達！ 固定します。現在の人数: {currentPlayersInGoal}/{requiredPlayersToClear}");

            if (currentPlayersInGoal >= requiredPlayersToClear)
            {
                ClearStage();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            currentPlayersInGoal--;

            // ★【ここを追加】万が一ゴールから押し出されたり離れたりした場合は、再度動けるようにする
            player.CanMove = true;

            if (currentPlayersInGoal < 0)
            {
                currentPlayersInGoal = 0;
            }

            Debug.Log($"プレイヤーがゴールから離れました。動けるようになります。現在の人数: {currentPlayersInGoal}");
        }
    }

    private void ClearStage()
    {
        Debug.Log("全員到達！ステージクリア！");
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextStageSceneName);
    }
}
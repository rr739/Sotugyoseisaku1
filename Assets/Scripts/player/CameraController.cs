using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
<<<<<<< HEAD
    // ★外部（ObjectOnlineCommunication）から「生成された自分」を受け取るための変数
    private Transform targetPlayer;
=======
    [Header("プレイヤーの設定")]

    [SerializeField] private string targetName1 = "player1(Clone)";
    [SerializeField] private string targetName2 = "player2(Clone)";
    private GameObject player1;
    private GameObject player2;
>>>>>>> origin/WR_new

    [Header("カメラの追尾スピード（なめらかさ）")]
    [Range(0.01f, 1f)][SerializeField] private float smoothSpeed = 0.125f;

    [Header("これ以上落ちたらカメラ追尾を諦めるY座標のしきい値")]
    [SerializeField] private float fallThreshold = -20f;

    // ★ObjectOnlineCommunication から呼び出されるターゲット設定窓口
    public void SetTarget(Transform playerTransform)
    {
        targetPlayer = playerTransform;
        Debug.Log($"[カメラ設定] {playerTransform.name} を追尾対象に指定しました。");
    }

    private void LateUpdate()
    {
<<<<<<< HEAD
        // ターゲットがまだ生成されていない、または消滅した場合は何もしない
        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy) return;

        // キャラが落下しきい値より上にいる（生きている）かチェック
        bool isAlive = targetPlayer.position.y > fallThreshold;

        // 生きていればそのキャラの座標、落ちていたら現在のカメラ位置をキープする
        Vector3 targetPosition = isAlive ? targetPlayer.position : transform.position;
=======
        player1 = GameObject.Find(targetName1);
        player2 = GameObject.Find(targetName2);
        if (player1 == null || player2 == null) return;

        // 1Pと2Pがそれぞれ正常な位置（穴に落ちていない状態）にいるかチェック
        bool p1IsAlive = player1.transform.position.y > fallThreshold;
        bool p2IsAlive = player2.transform.position.y > fallThreshold;

        Vector3 targetPosition = transform.position;

        if (p1IsAlive && p2IsAlive)
        {
            // 【通常時】2人とも画面内にいるなら、2人のちょうど真ん中をターゲットにする
            targetPosition = (player1.transform.position + player2.transform.position) / 2f;
        }
        else if (p1IsAlive)
        {
            // 2Pだけが落ちたなら、1Pだけを追いかける
            targetPosition = player1.transform.position;
        }
        else if (p2IsAlive)
        {
            // 1Pだけが落ちたなら、2Pだけを追いかける
            targetPosition = player2.transform.position;
        }
>>>>>>> origin/WR_new

        // カメラのZ位置（-10など）は元の値を維持する
        targetPosition.z = transform.position.z;

        // 計算した目標位置に向かって、カメラをなめらかに移動させる
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
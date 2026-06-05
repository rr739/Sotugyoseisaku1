using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ★外部（ObjectOnlineCommunication）から「生成された自分」を受け取るための変数
    private Transform targetPlayer;

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
        // ターゲットがまだ生成されていない、または消滅した場合は何もしない
        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy) return;

        // キャラが落下しきい値より上にいる（生きている）かチェック
        bool isAlive = targetPlayer.position.y > fallThreshold;

        // 生きていればそのキャラの座標、落ちていたら現在のカメラ位置をキープする
        Vector3 targetPosition = isAlive ? targetPlayer.position : transform.position;

        // カメラのZ位置（-10など）は元の値を維持する
        targetPosition.z = transform.position.z;

        // 計算した目標位置に向かって、カメラをなめらかに移動させる
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
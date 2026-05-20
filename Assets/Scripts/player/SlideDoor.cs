using UnityEngine;

[RequireComponent(typeof(Animator))]
public class WarpDoor : MonoBehaviour
{
    // --- ワープ関連の機能
    // [Header("繋がっている相方の扉")]
    // [SerializeField] private WarpDoor companionDoor;

    // [Header("ワープ後の出現オフセット")]
    // [SerializeField] private Vector2 exitOffset = new Vector2(0f, 0f);

    // // 連鎖ワープ防止用のタイマー（静的変数にすることで、すべての扉で時間を共有します）
    // private static float nextWarpAvailableTime = 0f;
    // [Header("再ワープまでの待機時間")]
    // [SerializeField] private float warpCooldown = 0.5f; 
    // -------------------------------------------------

    private Animator animator;
    private bool isOpen = false;

    // アニメーターのパラメーター名（IsOpen）のハッシュ値
    private static readonly int IsOpenParam = Animator.StringToHash("IsOpen");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // スイッチから呼ばれる、扉の開閉状態を設定するメソッド
    public void SetDoorState(bool open)
    {
        isOpen = open;
        // アニメーターの「IsOpen」パラメーターを更新してアニメーションを切り替える
        animator.SetBool(IsOpenParam, open);
    }

    // --- ワープの衝突判定処理
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     // 扉が開いている 且つ 触れたのがプレイヤー 且つ クールタイムが終了している時だけワープ
    //     if (isOpen && Time.time >= nextWarpAvailableTime && other.TryGetComponent<PlayerController>(out PlayerController player))
    //     {
    //         WarpPlayer(player.gameObject);
    //     }
    // }

    // private void WarpPlayer(GameObject playerObj)
    // {
    //     if (companionDoor == null) return;

    //     // 次にワープができる時刻を「現在時刻 + クールタイム」に設定
    //     nextWarpAvailableTime = Time.time + warpCooldown;

    //     // ワープ先の座標を計算
    //     Vector3 warpTargetPosition = companionDoor.transform.position + (Vector3)companionDoor.exitOffset;
    //     warpTargetPosition.z = playerObj.transform.position.z;

    //     // 物理速度をリセットして座標を書き換え
    //     if (playerObj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
    //     {
    //         rb.velocity = Vector2.zero;
    //     }

    //     playerObj.transform.position = warpTargetPosition;

    //     Debug.Log($"{playerObj.name} がワープしました。次のワープまで {warpCooldown} 秒ロックします。");
    // }
    // -----------------------------------------------------
}
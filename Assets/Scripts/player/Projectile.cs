using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float lifeTime = 1.5f; // 1.5秒で自動消滅
    [SerializeField] private ElementType projectileType;

    [Header("Collision Settings")]
    // インスペクターで「壁(Wall)」「床(Ground)」「箱(Pushable)」にチェックを入れる
    [SerializeField] private LayerMask collisionLayers;

    private Rigidbody2D rb;
    private float moveDirection = 1f; // ★【追加】飛ぶ方向（1なら右、-1なら左）
    private bool isInitialized = false; // ★【追加】初期化が完了したかのフラグ

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 重力の影響を無効化
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        // 【時間経過で消滅】指定した秒数（1.5秒）後に自分を削除
        Destroy(gameObject, lifeTime);
    }

    // ★【追加】プレイヤーから「向き」と「属性」を受け取って、勢いよく飛ばす処理
    public void Initialize(float direction, ElementType playerElement)
    {
        moveDirection = direction;
        projectileType = playerElement; // プレイヤーの属性（FireやIce）を自動コピー
        isInitialized = true;

        // 向いている方向（右 or 左）に物理速度をセットする
        if (rb != null)
        {
            rb.velocity = new Vector2(speed * moveDirection, 0f);
        }
    }

    private void FixedUpdate()
    {
        // ★【追加】万が一、衝突などで弾の速度が落ちたり変わったりしないよう、消えるまで速度を一定に維持する
        if (isInitialized && rb != null)
        {
            rb.velocity = new Vector2(speed * moveDirection, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. ギミック（IInteractable）に当たった場合
        IInteractable target = other.GetComponent<IInteractable>();
        if (target != null)
        {
            target.OnInteract(projectileType); // 属性を伝達
            Destroy(gameObject);              // 即座に消滅
            return;
        }

        // 2. プレイヤーや壁などに当たった場合
        // LayerMaskに含まれるレイヤー（壁や床など）に接触したか判定
        if (((1 << other.gameObject.layer) & collisionLayers) != 0)
        {
            Destroy(gameObject); // 即座に消滅
        }
    }
}
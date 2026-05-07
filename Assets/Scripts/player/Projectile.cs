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

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // 重力の影響を無効化
        rb.gravityScale = 0f;

        // 向いている方向に飛ばす
        rb.velocity = transform.right * speed;

        // 【時間経過で消滅】指定した秒数（1.5秒）後に自分を削除
        Destroy(gameObject, lifeTime);
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
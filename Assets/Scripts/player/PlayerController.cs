using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Character Identity")]
    [SerializeField] private ElementType element;
    public ElementType Element => element;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float pushSpeedMultiplier = 0.5f;
    [SerializeField] private float jumpForce = 6.5f;

    [Header("Shoot Settings")]
    [SerializeField] private float fireRate = 0.3f; // 次の弾を撃てるようになるまでの秒数
    private float nextFireTime = 0f;               // 次に撃てる時刻を保持

    [Header("Input Names")]
    [SerializeField] private string horizontalAxis = "Horizontal1";
    [SerializeField] private string jumpButton = "Jump1";
    [SerializeField] private string fireButton = "Fire1";

    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask pushableLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isPushing;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        Move();
        if (Input.GetButtonDown(jumpButton) && isGrounded) Jump();

        // Input.GetButtonDown なので長押ししても1回しか反応しません
        if (Input.GetButtonDown(fireButton)) Shoot();
    }

    private void Move()
    {
        float moveInput = Input.GetAxisRaw(horizontalAxis);
        bool isActuallyPushing = isPushing && IsInputtingTowardsBox(moveInput);
        float currentSpeed = isActuallyPushing ? moveSpeed * pushSpeedMultiplier : moveSpeed;

        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
    }

    private bool IsInputtingTowardsBox(float moveInput)
    {
        if (moveInput == 0) return false;
        float checkDistance = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(moveInput, 0), checkDistance, pushableLayer);
        return hit.collider != null;
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Shoot()
    {
        // 1. クールタイム中（現在の時間が nextFireTime より前）なら処理を中断
        if (Time.time < nextFireTime) return;

        if (projectilePrefab == null || firePoint == null) return;

        // 2. 弾を生成
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 3. 次に撃てる時刻を「現在時刻 + 間隔」に更新
        nextFireTime = Time.time + fireRate;
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckContact(collision, true);
    private void OnCollisionExit2D(Collision2D collision) => CheckContact(collision, false);

    private void CheckContact(Collision2D collision, bool state)
    {
        int layer = collision.gameObject.layer;
        if (((1 << layer) & groundLayer) != 0) isGrounded = state;
        if (((1 << layer) & pushableLayer) != 0) isPushing = state;
    }
}
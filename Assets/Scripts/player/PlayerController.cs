using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("キャラクターの属性")]
    [SerializeField] private ElementType element; // 属性設定：Fire（炎）または Ice（氷）
    public ElementType Element => element;       // 他のクラスから属性を確認するための公開プロパティ

    [Header("移動・ジャンプ設定")]
    [SerializeField] private float moveSpeed = 5.0f;           // 基本の移動速度
    [SerializeField] private float pushSpeedMultiplier = 0.5f; // オブジェクト押し出し中の移動速度倍率（例: 0.5なら速度半分）
    [SerializeField] private float jumpForce = 6.5f;           // ジャンプ時に加える力の強さ

    [Header("射撃（クールタイム）設定")]
    [SerializeField] private float fireRate = 0.3f; // 次の弾を撃つまでに必要な待機時間（秒）
    private float nextFireTime = 0f;               // 次に発射が可能になる時刻の記録用

    [Header("インプット設定（InputManagerの名前）")]
    [SerializeField] private string horizontalAxis = "Horizontal1"; // 左右移動に使用する軸の名前
    [SerializeField] private string jumpButton = "Jump1";           // ジャンプに使用するボタンの名前
    [SerializeField] private string fireButton = "Fire1";           // 攻撃に使用するボタンの名前

    [Header("各種参照設定")]
    [SerializeField] private GameObject projectilePrefab; // 発射する弾のプレハブ
    [SerializeField] private Transform firePoint;         // 弾が生成（出現）するポイント
    [SerializeField] private LayerMask groundLayer;       // 地面判定を行う対象レイヤー
    [SerializeField] private LayerMask pushableLayer;     // 押し出し可能なオブジェクトのレイヤー

    private Rigidbody2D rb;
    private bool isGrounded; // 現在、地面に接地しているかどうかのフラグ
    private bool isPushing;  // 現在、押し出し対象に接触しているかどうかのフラグ
    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        // 毎フレーム移動処理を呼び出し
        Move();

        // ジャンプ：ボタンが押された瞬間 且つ 地面にいる時
        if (Input.GetButtonDown(jumpButton) && isGrounded) Jump();

        // 攻撃：ボタンが押された瞬間（GetButtonDownなので長押しでの連射を防止）
        if (Input.GetButtonDown(fireButton)) Shoot();
    }

    private void Move()
    {
        // 入力値を取得（-1, 0, 1）
        float moveInput = Input.GetAxisRaw(horizontalAxis);

        // 「箱に触れている」かつ「箱がある方向にキーを入力している」時だけ、本当に押していると判定
        bool isActuallyPushing = isPushing && IsInputtingTowardsBox(moveInput);

        // 押し状態なら速度を下げ、そうでなければ通常の速度を適用
        float currentSpeed = isActuallyPushing ? moveSpeed * pushSpeedMultiplier : moveSpeed;

        // 左右の速度を設定（y軸は現在の物理挙動を維持）
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
    }

    // 入力した方向に箱があるかどうかを確認する判定（Raycastを使用）
    private bool IsInputtingTowardsBox(float moveInput)
    {
        if (moveInput == 0) return false;

        float checkDistance = 0.5f; // キャラクターから前方どれくらいの距離まで確認するか
        // Raycast（光線）を飛ばして指定レイヤーのオブジェクトに当たったかチェック
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(moveInput, 0), checkDistance, pushableLayer);

        return hit.collider != null; // 当たったものがあればtrueを返す
    }

    private void Jump()
    {
        // ジャンプの瞬間に縦方向の速度をリセット（落下中などの勢いを消して安定させる）
        rb.velocity = new Vector2(rb.velocity.x, 0);
        // 上方向に瞬間的な力を加える
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Shoot()
    {
        // クールタイム判定：現在のゲーム時間が発射可能時刻を過ぎているか確認
        if (Time.time < nextFireTime) return;

        // プレハブや発射地点が未設定ならエラー防止のため中断
        if (projectilePrefab == null || firePoint == null) return;

        // 弾を生成（位置と向きをfirePointに合わせる）
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 次に撃てる時刻を更新（現在時刻 + 連射間隔）
        nextFireTime = Time.time + fireRate;
    }

    // 衝突開始時の判定
    private void OnCollisionEnter2D(Collision2D collision) => CheckContact(collision, true);
    // 衝突終了時の判定
    private void OnCollisionExit2D(Collision2D collision) => CheckContact(collision, false);

    // 衝突している相手が床か箱かを確認し、状態を更新する
    private void CheckContact(Collision2D collision, bool state)
    {
        int layer = collision.gameObject.layer;

        // ビット演算を使用してレイヤーが一致するかチェック（マジックナンバー回避）
        if (((1 << layer) & groundLayer) != 0) isGrounded = state;
        if (((1 << layer) & pushableLayer) != 0) isPushing = state;
    }
}
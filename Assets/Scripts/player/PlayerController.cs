using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;
using NativeWebSocket;


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
    [SerializeField] private string horizontalAxis = "Horizontal"; // 左右移動に使用する軸の名前
    [SerializeField] private string jumpButton = "Jump";           // ジャンプに使用するボタンの名前
    [SerializeField] private string fireButton = "Fire1";           // 攻撃に使用するボタンの名前

    [Header("各種参照設定")]
    [SerializeField] private GameObject projectilePrefab; // 発射する弾のプレハブ
    [SerializeField] private Transform firePoint;         // 弾が生成（出現）するポイント
    [SerializeField] private LayerMask groundLayer;       // 地面判定を行う対象レイヤー
    [SerializeField] private LayerMask pushableLayer;     // 押し出し可能なオブジェクトのレイヤー[

    public bool CanMove { get; set; } = true;

    private Rigidbody2D rb;
    private bool isGrounded; // 現在、地面に接地しているかどうかのフラグ
    private bool isPushing;  // 現在、押し出し対象に接触しているかどうかのフラグ

    NetworkManager client;

    public bool IsLocalPlayer { get; set; } = true;
    private NetworkManager networkManager;

    // 前回の座標を記録
    private Vector2 lastPosition;

    public Vector3 TargetPosition { get; set; }

    [SerializeField] private int projectilePrefabIndex;
    private int projectileCount = 0;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Start()
    {
        
        if (NetworkManager.Instance != null)
        {
            networkManager = NetworkManager.Instance;
        }
        else
        {
            Debug.LogError("NetworkManagerが見つかりません！");
        }

        lastPosition = transform.position;
    }

    void Update()
    {

        if (!IsLocalPlayer)
        {
            
            transform.position = Vector3.Lerp(transform.position, TargetPosition, 0.15f);
            return;
        }

        // 毎フレーム移動処理を呼び出し
        Move();

        // ジャンプ：ボタンが押された瞬間 且つ 地面にいる時
        if (Input.GetButtonDown(jumpButton) && isGrounded) Jump();

        // 攻撃：ボタンが押された瞬間
        if (Input.GetButtonDown(fireButton))
        {
            // ★【ここを追加】もし今、マウスがUI（メニューボタンや「はい」ボタン）の上にあるなら、射撃をスルーする
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Shoot();
        }

        if (Vector2.Distance(transform.position, lastPosition) > 0.01f)
        {
            SendPlayerData(transform.position);
            lastPosition = transform.position; // 記録を更新
        }
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

        

        // 自分の画面に弾を生成する
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 生まれた弾に、絶対に被らない固有のIDを計算して持たせる
        var identity = projectile.GetComponent<NetworkIdentity2D>();
        if (identity != null)
        {
            projectileCount++;

            int myColor = (Element == ElementType.Fire) ? 0 : 1;
            identity.objectId = ((myColor + 1) * 1000) + projectileCount;

            // 自分が撃った弾なので、当然自分が動かす権利を持つ
            identity.isOwnedByLocal = true;

            // 受信管理側のリスト（辞書）にも「今生まれたよ」と登録してあげる
            var onlineComm = FindObjectOfType<ObjectOnlineCommunication>();
            if (onlineComm != null)
            {
                onlineComm.syncObjects[identity.objectId] = identity;
            }

            // 相手の画面に弾を生成通信を送る
            SendSpawnProjectileEvent(identity.objectId, firePoint.position, firePoint.rotation);
        }

        // 次に撃てる時刻を更新（現在時刻 + 連射間隔）
        nextFireTime = Time.time + fireRate;
    }

    // 衝突開始時の判定
    private void OnCollisionEnter2D(Collision2D collision) => CheckContact(collision, true);
    // 衝突終了時の判定
    private void OnCollisionExit2D(Collision2D collision) => CheckContact(collision, false);

    // 衝突している相手が床か箱かを確認し、状態を更新する
    // 衝突している相手が床か箱かを確認し、状態を更新する
    private void CheckContact(Collision2D collision, bool state)
    {
        int layer = collision.gameObject.layer;

        // ビット演算を使用してレイヤーが一致するかチェック
        bool isGroundLayer = ((1 << layer) & groundLayer) != 0;
        bool isPushableLayer = ((1 << layer) & pushableLayer) != 0;

        if (isGroundLayer)
        {
            if (state) // 接触した（Enter）とき
            {
                // 衝突した面（接点）の向きを確認する
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    // normal.y が 0.7 以上のとき「上を向いている面（＝床）」と判定する
                    // （真上なら 1.0、45度の坂道なら約 0.707）
                    float minGroundAngleY = 0.7f;

                    if (contact.normal.y >= minGroundAngleY)
                    {
                        isGrounded = true;
                        break; // 床が見つかったのでループを抜ける
                    }
                }
            }
            else // 離れた（Exit）とき
            {
                isGrounded = false;
            }
        }

        if (isPushableLayer) isPushing = state;
    }

    

    // プレイヤーの位置を送る
    async void SendPlayerData(Vector3 pos)
    {
        // 安全チェック
        if (networkManager == null) return;

        InGameMoveData playerData = new InGameMoveData();

        playerData.dataType = "player";
        playerData.room_id = networkManager.myRoomID;

        
        int myRealChara = networkManager.myRealSelectedChar;
    
        if (myRealChara == -1)
        {
            myRealChara = (Element == ElementType.Fire) ? 0 : 1;
        }
        playerData.char_index = myRealChara;

        playerData.position_x = pos.x;
        playerData.position_y = pos.y;

        var jsonMsg = JsonUtility.ToJson(playerData);
        await networkManager.SendMessageAsync(jsonMsg);
    }
    private async void SendSpawnProjectileEvent(int id, Vector3 pos, Quaternion rot)
    {
        if (networkManager == null) return;

        InGameMoveData spawnData = new InGameMoveData();
        spawnData.dataType = "spawn_projectile"; // ★新しいデータタイプ「弾生成」
        spawnData.room_id = networkManager.myRoomID;
        spawnData.id = id; // 生成した固有ID
        spawnData.char_index = projectilePrefabIndex; // どの種類の弾を出すか（0番など）

        // 生成位置と角度もデータに乗せる
        spawnData.position_x = pos.x;
        spawnData.position_y = pos.y;
        // 角度（Z軸の回転）も送りたい場合は、余っている変数に入れるか拡張してください（今回は位置のみ簡易対応）

        string json = JsonUtility.ToJson(spawnData);
        await networkManager.SendMessageAsync(json);
    }
}


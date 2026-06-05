using UnityEngine;
using UnityEngine.XR;
<<<<<<< HEAD


=======
>>>>>>> f55d313c87ee20eb0d43e86588a5daea06cf825f

public class OffScreenIndicator : MonoBehaviour

{

    [Header("追尾ターゲット（画面外に行くプレイヤー）")]
<<<<<<< HEAD
<<<<<<< HEAD

    [SerializeField] private Transform targetPlayer;
=======
    [SerializeField] private string targetName ="player";
    private GameObject targetPlayer;
>>>>>>> origin/WR_new


=======
    [SerializeField] private string targetName ="player";
    private GameObject targetPlayer;
>>>>>>> f55d313c87ee20eb0d43e86588a5daea06cf825f

    [Header("表示するUIアイコン（ImageのRectTransform）")]

    [SerializeField] private RectTransform indicatorIcon;



    [Header("画面の端からどれくらい内側に固定するか")]

    [SerializeField] private float margin = 50f;



    [Header("アイコンを回転させるか？")]

    [SerializeField] private bool rotateIcon = true;



    [Header("矢印の初期向き調整（横を向いている画像なら0、上なら-90）")]

    [SerializeField] private float rotationOffset = -90f;



    private Camera mainCamera;



    private void Start()

    {

        mainCamera = Camera.main;

<<<<<<< HEAD
<<<<<<< HEAD

=======
        targetPlayer = GameObject.Find(targetName);
>>>>>>> origin/WR_new
=======
        targetPlayer = GameObject.Find(targetName);
>>>>>>> f55d313c87ee20eb0d43e86588a5daea06cf825f

        if (indicatorIcon != null)

        {

            indicatorIcon.gameObject.SetActive(false);

        }

    }



    private void LateUpdate()

    {

        // ★【安全対策】プレイヤーやアイコンが消滅、または非アクティブならUIを非表示にして何もしない

        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy || indicatorIcon == null || mainCamera == null)

        {

            if (indicatorIcon != null) indicatorIcon.gameObject.SetActive(false);

            return;

        }

<<<<<<< HEAD
<<<<<<< HEAD


        // ターゲットのワールド座標をスクリーン座標に変換

        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPlayer.position);
=======
        targetPlayer = GameObject.Find(targetName);

        // ターゲットのワールド座標をスクリーン座標に変換
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPlayer.transform.position);
>>>>>>> origin/WR_new


=======
        targetPlayer = GameObject.Find(targetName);

        // ターゲットのワールド座標をスクリーン座標に変換
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPlayer.transform.position);
>>>>>>> f55d313c87ee20eb0d43e86588a5daea06cf825f

        // 画面外にいるかどうかの判定

        bool isOffScreen = screenPos.x < 0 || screenPos.x > Screen.width ||

                           screenPos.y < 0 || screenPos.y > Screen.height ||

                           screenPos.z < 0;



        if (isOffScreen)

        {

            indicatorIcon.gameObject.SetActive(true);



            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0f) / 2f;

            Vector3 fromCenter = screenPos - screenCenter;



            if (screenPos.z < 0)

            {

                fromCenter *= -1f;

            }



            // 画面端のクランプ範囲を計算

            float minX = margin;

            float maxX = Screen.width - margin;

            float minY = margin;

            float maxY = Screen.height - margin;



            float angle = Mathf.Atan2(fromCenter.y, fromCenter.x);

            float slope = Mathf.Tan(angle);



            if (fromCenter.x > 0)

            {

                screenPos.x = maxX;

                screenPos.y = screenCenter.y + (screenCenter.x - margin) * slope;

            }

            else

            {

                screenPos.x = minX;

                screenPos.y = screenCenter.y - (screenCenter.x - margin) * slope;

            }



            if (screenPos.y > maxY)

            {

                screenPos.y = maxY;

                if (slope != 0) screenPos.x = screenCenter.x + (screenCenter.y - margin) / slope;

            }

            else if (screenPos.y < minY)

            {

                screenPos.y = minY;

                if (slope != 0) screenPos.x = screenCenter.x - (screenCenter.y - margin) / slope;

            }



            screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);

            screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

            screenPos.z = 0f;



            indicatorIcon.position = screenPos;



            // 回転の設定

            if (rotateIcon)

            {

                float rotationAngle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;

                indicatorIcon.rotation = Quaternion.Euler(0f, 0f, rotationAngle + rotationOffset);

            }

            else

            {

                indicatorIcon.rotation = Quaternion.identity; // 回転なし（常にまっすぐ）

            }

        }

        else

        {

            indicatorIcon.gameObject.SetActive(false);

        }

    }

} 


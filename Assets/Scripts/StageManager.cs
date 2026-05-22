using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void DeleteDataButton()
    {
        if (NetworkManager.Instance.ws != null)
        {
            Debug.Log("サーバー接続を切断中...");
            await NetworkManager.Instance.ws.Close();
            Debug.Log("サーバー接続完了");

        }

        NetworkManager.Instance.DeleteData();


       
    }
    private void CheckHost()
    {
        if (NetworkManager.Instance.myPlayerIndex == 0 )
        {
           
        }
        else
        {
           
        }
    }

    public void GoTutorialStage()
    {
        SceneManager.LoadScene("TutorialScene");

    }
}

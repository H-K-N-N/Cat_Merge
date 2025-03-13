using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    //public static TitleManager Instance { get; private set; }

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }
    //}

    // 게임 시작
    public void GameStart()
    {
        SceneManager.LoadScene("GameScene-Han");
    }

}

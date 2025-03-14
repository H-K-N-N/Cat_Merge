using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    private void Start()
    {
        //// 60프레임 고정
        //Application.targetFrameRate = 60;
    }

    // 게임 시작
    public void GameStart()
    {
        if (GoogleManager.Instance != null)
        {
            // 로딩 화면 표시 요청
            GoogleManager.Instance.ShowLoadingScreen(true);
        }

        // 게임 씬 로드
        SceneManager.LoadScene("GameScene-Han");
    }

}

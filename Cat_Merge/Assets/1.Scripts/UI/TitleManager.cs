using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // 게임 시작
    public void GameStart()
    {
        // 게임 시작 전 데이터 저장 (혹시 모를 경우를 대비)
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();

            // 로딩 화면 표시 요청
            GoogleManager.Instance.ShowLoadingScreen(true);
        }

        // 게임 씬 로드
        SceneManager.LoadScene("GameScene-Han");
    }

}

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    
    public void GameStart()
    {
        SceneManager.LoadScene("GameScene-Han");
    }

}

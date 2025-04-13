using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{


    #region Variables

    [SerializeField] private Button startButton;

    #endregion


    #region Unity Methods

    private void Start()
    {
        Application.targetFrameRate = 60;

        startButton.gameObject.SetActive(false);
    }

    #endregion


    #region Game Start

    // 게임 시작 버튼 활성화 함수
    public void EnableStartButton()
    {
        startButton.gameObject.SetActive(true);
    }

    // 게임 시작 버튼 클릭시 호출될 함수
    public void OnGameStart()
    {
        GetComponent<TitleAnimationManager>().StopBlinkAnimation();
        GetComponent<TitleAnimationManager>().StopBreathingAnimation();
        GetComponent<TitleAnimationManager>().StopCatAutoMovement();
    }

    #endregion


}

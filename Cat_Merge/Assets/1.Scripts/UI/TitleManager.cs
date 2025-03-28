using UnityEngine;
using TMPro;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI touchToStartText;
    private Coroutine blinkCoroutine;

    private void Start()
    {
        blinkCoroutine = StartCoroutine(BlinkText());
    }

    // 게임 시작 시 호출될 메서드
    public void OnGameStart()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        // 텍스트 알파값을 0으로 설정
        Color textColor = touchToStartText.color;
        touchToStartText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
    }

    // 텍스트 깜빡임 코루틴
    private IEnumerator BlinkText()
    {
        // 깜빡임 관련 변수들
        float fadeSpeed = 2f;        // 페이드 속도
        float minAlpha = 0.2f;       // 최소 알파값
        float maxAlpha = 1f;         // 최대 알파값

        while (true)
        {
            // 페이드 아웃
            float elapsedTime = 0f;
            Color startColor = touchToStartText.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, minAlpha);

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, endColor, elapsedTime);
                yield return null;
            }

            // 페이드 인
            elapsedTime = 0f;
            startColor = touchToStartText.color;
            endColor = new Color(startColor.r, startColor.g, startColor.b, maxAlpha);

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, endColor, elapsedTime);
                yield return null;
            }
        }
    }

}

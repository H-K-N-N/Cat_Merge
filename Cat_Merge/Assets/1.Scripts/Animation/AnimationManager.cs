using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{


    #region Variables

    public RectTransform spriteRect;        // UI Image의 RectTransform
    public Image spriteImage;               // UI Image의 투명도 조절을 위한 변수
    private float duration = 0.125f;        // 크기 변화 시간
    private float maxLifetime = 2f;         // 최대 생존 시간 (안전장치)

    #endregion


    #region Unity Methods

    private void Start()
    {
        StartCoroutine(ScaleAnimation());
        StartCoroutine(SafetyTimeout());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    #endregion


    #region Animation Methods

    // 최대 생존 시간 이후 오브젝트를 제거하는 코루틴
    private IEnumerator SafetyTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);
        Destroy(gameObject);
    }

    // 크기 변경 및 페이드 아웃 애니메이션 코루틴
    private IEnumerator ScaleAnimation()
    {
        // 1. 크기 30 → 175 (투명도 1 유지)
        // 2. 크기 175 → 150 (투명도 1 → 0)
        // 3. 애니메이션이 완전히 끝났거나 에러가 발생했을 때 오브젝트 제거

        SetAlpha(1f);
        yield return StartCoroutine(ChangeSize(spriteRect, new Vector2(30, 30), new Vector2(175, 175), duration, false));
        yield return StartCoroutine(ChangeSize(spriteRect, new Vector2(175, 175), new Vector2(150, 150), duration * 0.5f, true));
        Destroy(gameObject);
    }

    // 지정된 시간 동안 크기와 투명도를 변경하는 코루틴
    private IEnumerator ChangeSize(RectTransform target, Vector2 startSize, Vector2 endSize, float time, bool fadeOut)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            // 크기 조절
            target.sizeDelta = Vector2.Lerp(startSize, endSize, t);

            // 투명도 조절 (fadeOut이 true일 때만)
            if (fadeOut)
            {
                SetAlpha(Mathf.Lerp(1f, 0f, t));
            }

            yield return null;
        }

        // 최종 크기와 투명도 설정
        target.sizeDelta = endSize;
        if (fadeOut)
        {
            SetAlpha(0f);
        }
    }

    // 스프라이트 이미지의 투명도 설정 함수
    private void SetAlpha(float alpha)
    {
        if (spriteImage != null)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }

    #endregion


}
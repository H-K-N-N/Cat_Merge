using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public RectTransform spriteRect;                    // UI Image의 RectTransform
    public Image spriteImage;                           // UI Image의 투명도 조절을 위한 변수
    public float duration = 1.5f;                       // 크기 변화 시간
    private float maxLifetime = 3f;                     // 최대 생존 시간 (안전장치)

    private Vector2 minSize = new Vector2(30, 30);
    private Vector2 maxSize = new Vector2(175, 175);
    private Vector2 endSize = new Vector2(150, 150);

    private void Start()
    {
        StartCoroutine(ScaleAnimation());
        StartCoroutine(SafetyTimeout());
    }

    private IEnumerator SafetyTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);

        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ScaleAnimation()
    {
        // 1. 크기 30 → 175 (투명도 1 유지)
        SetAlpha(1f);
        yield return StartCoroutine(ChangeSize(spriteRect, minSize, maxSize, duration, false));

        // 2. 크기 175 → 150 (투명도 1 → 0)
        yield return StartCoroutine(ChangeSize(spriteRect, maxSize, endSize, duration * 0.5f, true));

        // 애니메이션이 완전히 끝났거나 에러가 발생했을 때 오브젝트 제거
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

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

    private void SetAlpha(float alpha)
    {
        if (spriteImage != null)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

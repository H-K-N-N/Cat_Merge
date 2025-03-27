using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public RectTransform spriteRect; // UI Image의 RectTransform
    public Image spriteImage; // UI Image의 투명도 조절을 위한 변수
    public float duration = 1.5f; // 크기 변화 시간

    private Vector2 minSize = new Vector2(30, 30);
    private Vector2 maxSize = new Vector2(175, 175);
    private Vector2 endSize = new Vector2(150, 150);

    void Start()
    {
        StartCoroutine(ScaleAnimation());
    }

    IEnumerator ScaleAnimation()
    {
        // 1. 크기 30 → 175 (투명도 1 유지)
        SetAlpha(1f);
        yield return StartCoroutine(ChangeSize(spriteRect, minSize, maxSize, duration, false));

        // 2. 크기 175 → 150 (투명도 1 → 0)
        yield return StartCoroutine(ChangeSize(spriteRect, maxSize, endSize, duration * 0.5f, true));
    }

    IEnumerator ChangeSize(RectTransform target, Vector2 startSize, Vector2 endSize, float time, bool fadeOut)
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

        target.sizeDelta = endSize;
        if (fadeOut)
        {
            SetAlpha(0f); // 최종적으로 완전히 투명하게 설정
        }

        if(spriteImage.color.a == 0)
        {
            Destroy(this.gameObject);
        }
    }

    void SetAlpha(float alpha)
    {
        if (spriteImage != null)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }
}

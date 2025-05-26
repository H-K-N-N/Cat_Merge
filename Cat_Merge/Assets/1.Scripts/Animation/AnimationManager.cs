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

    private bool isDestroyed = false;       // 파괴 예약 상태
    private Coroutine scaleAnimationCoroutine;
    private Coroutine safetyTimeoutCoroutine;

    #endregion


    #region Unity Methods

    private void Start()
    {
        if (!isDestroyed)
        {
            scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
            safetyTimeoutCoroutine = StartCoroutine(SafetyTimeout());
        }
    }

    private void OnDisable()
    {
        CleanupAnimation();
    }

    private void OnDestroy()
    {
        CleanupAnimation();
    }

    private void OnApplicationQuit()
    {
        CleanupAnimation();
    }

    #endregion


    #region Animation Methods

    // 최대 생존 시간 이후 오브젝트를 제거하는 코루틴
    private IEnumerator SafetyTimeout()
    {
        yield return new WaitForSeconds(maxLifetime);
        DestroyEffect();
    }

    // 크기 변경 및 페이드 아웃 애니메이션 코루틴
    private IEnumerator ScaleAnimation()
    {
        if (spriteRect == null || spriteImage == null || isDestroyed)
        {
            DestroyEffect();
            yield break;
        }

        // 1. 크기 30 → 175 (투명도 1 유지)
        SetAlpha(1f);

        var scaleUp = StartCoroutine(ChangeSize(spriteRect, new Vector2(30, 30), new Vector2(175, 175), duration, false));
        yield return scaleUp;

        if (isDestroyed)
        {
            StopCoroutine(scaleUp);
            DestroyEffect();
            yield break;
        }

        // 2. 크기 175 → 150 (투명도 1 → 0)
        var scaleDown = StartCoroutine(ChangeSize(spriteRect, new Vector2(175, 175), new Vector2(150, 150), duration * 0.5f, true));
        yield return scaleDown;

        if (isDestroyed)
        {
            StopCoroutine(scaleDown);
        }

        DestroyEffect();
    }

    // 지정된 시간 동안 크기와 투명도를 변경하는 코루틴
    private IEnumerator ChangeSize(RectTransform target, Vector2 startSize, Vector2 endSize, float time, bool fadeOut)
    {
        if (target == null || isDestroyed) yield break;

        float elapsedTime = 0;
        while (elapsedTime < time && !isDestroyed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;

            if (target == null) break;  // 추가 검사

            // 크기 조절
            target.sizeDelta = Vector2.Lerp(startSize, endSize, t);

            // 투명도 조절 (fadeOut이 true일 때만)
            if (fadeOut)
            {
                SetAlpha(Mathf.Lerp(1f, 0f, t));
            }

            yield return null;
        }

        if (!isDestroyed && target != null)
        {
            // 최종 크기와 투명도 설정
            target.sizeDelta = endSize;
            if (fadeOut)
            {
                SetAlpha(0f);
            }
        }
    }

    // 스프라이트 이미지의 투명도 설정 함수
    private void SetAlpha(float alpha)
    {
        if (spriteImage != null && !isDestroyed)
        {
            Color color = spriteImage.color;
            color.a = alpha;
            spriteImage.color = color;
        }
    }

    // 이펙트 정리 함수
    private void CleanupAnimation()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;

            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
                scaleAnimationCoroutine = null;
            }

            if (safetyTimeoutCoroutine != null)
            {
                StopCoroutine(safetyTimeoutCoroutine);
                safetyTimeoutCoroutine = null;
            }

            StopAllCoroutines();
        }
    }

    // 이펙트 제거 함수
    private void DestroyEffect()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            CleanupAnimation();
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion


}

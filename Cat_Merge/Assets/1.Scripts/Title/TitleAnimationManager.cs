using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class TitleAnimationManager : MonoBehaviour
{


    #region Variables

    [Header("Animation Objects")]
    [SerializeField] private RectTransform mainUIPanel;         // 메인 패널
    [SerializeField] private RectTransform level1CatLeft;       // 왼쪽 1레벨 고양이
    [SerializeField] private RectTransform level1CatRight;      // 오른쪽 1레벨 고양이
    [SerializeField] private RectTransform level2Cat;           // 중앙 2레벨 고양이
    [SerializeField] private RectTransform animationPanel;      // 애니메이션이 진행될 패널

    [SerializeField] private GameObject effectPrefab;           // 합성 이펙트 프리팹

    [SerializeField] private RectTransform titleNameImage;      // 타이틀 이름 이미지 (초기좌표:(0,1100), 이동좌표:(0,700))

    [SerializeField] private List<RectTransform> animationCats; // 타이틀씬 움직임용 고양이들

    [Header("Animation Settings")]
    private float waitDuration = 0.5f;                          // 대기 시간
    private float moveDuration = 1.0f;                          // 고양이 이동 시간
    private float zoomDuration = 0.5f;                          // 카메라 줌 시간

    private Vector2 leftStartPos = new Vector2(-130f, 0f);      // 왼쪽 고양이 시작 위치 (화면 왼쪽)
    private Vector2 rightStartPos = new Vector2(130f, 0f);      // 오른쪽 고양이 시작 위치 (화면 오른쪽)
    private Vector2 centerPos = Vector2.zero;                   // 중앙 위치

    private Camera mainCamera;                                  // 카메라 참조
    private float originalOrthographicSize;                     // 카메라 기존 크기
    private float zoomedOrthographicSize = 2.0f;                // 카메라 줌인 크기

    [Header("Title Drop Animation Settings")]
    private float startY = 1300f;                               // 시작 Y 좌표
    private float endY = 700f;                                  // 최종 Y 좌표

    [Header("Title Breathing Animation Settings")]
    private float breathingDuration = 3.0f;                     // 한 번의 호흡 주기 시간
    private float minScale = 0.95f;                             // 최소 크기 (95%)
    private float maxScale = 1.05f;                             // 최대 크기 (105%)
    private Coroutine breathingCoroutine;                       // 호흡 애니메이션 코루틴 참조 추가

    [Header("Touch To Start Text Settings")]
    [SerializeField] private TextMeshProUGUI touchToStartText;  // Touch To Start 텍스트
    private float fadeSpeed = 2f;                               // 페이드 속도
    private float minAlpha = 0.2f;                              // 최소 알파값
    private float maxAlpha = 0.8f;                              // 최대 알파값
    private Coroutine blinkCoroutine;                           // 깜빡임 코루틴 참조

    [Header("Cat Auto Movement Settings")]
    private bool isAnimating = false;                           // 이동 애니메이션 진행 여부
    private float autoMoveInterval = 2f;                        // 자동 이동 간격
    private float moveDurationCat = 1.0f;                       // 고양이 이동 시간

    [Header("readonly Settings")]
    private readonly List<Coroutine> catMoveCoroutines = new List<Coroutine>();
    private static readonly Vector2[] moveDirections = {
        new Vector2(30f, 0f),
        new Vector2(-30f, 0f),
        new Vector2(0f, 30f),
        new Vector2(0f, -30f),
        new Vector2(30f, 30f),
        new Vector2(30f, -30f),
        new Vector2(-30f, 30f),
        new Vector2(-30f, -30f)
    };

    private static readonly Color colorCache = new Color(1f, 1f, 1f, 0f);

    #endregion


    #region Unity Methods

    private void Start()
    {
        InitializeAnimationManager();
        StartCoroutine(PlayTitleAnimation());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        catMoveCoroutines.Clear();
        isAnimating = false;
    }

    #endregion


    #region Initialize

    // 초기 위치 및 상태 설정 함수
    private void InitializeAnimationManager()
    {
        mainCamera = Camera.main;
        originalOrthographicSize = mainCamera.orthographicSize;

        level1CatLeft.anchoredPosition = leftStartPos;
        level1CatRight.anchoredPosition = rightStartPos;

        InitializeRectTransform(level1CatLeft);
        InitializeRectTransform(level1CatRight);
        InitializeRectTransform(level2Cat);

        level1CatLeft.gameObject.SetActive(true);
        level1CatRight.gameObject.SetActive(true);
        level2Cat.gameObject.SetActive(false);
    }

    // UI 요소의 기준점을 중앙으로 설정하는 함수
    private void InitializeRectTransform(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    #endregion


    #region Animation Sequence

    // 타이틀씬 애니메이션
    private IEnumerator PlayTitleAnimation()
    {
        // 0. 잠시 대기
        yield return new WaitForSeconds(waitDuration);

        // 1. 고양이 중앙으로 이동 + 카메라 줌인
        StartCoroutine(ZoomCamera(zoomedOrthographicSize));
        yield return StartCoroutine(MoveCatsToCenter());

        // 2. 합성 이펙트 생성
        GameObject recallEffect = Instantiate(effectPrefab, level2Cat.transform.position, Quaternion.identity);
        recallEffect.transform.SetParent(animationPanel.transform);
        recallEffect.transform.localScale = Vector3.one * 2f;

        // 3. 1레벨 고양이 비활성화 및 2레벨 고양이 활성화
        level1CatLeft.gameObject.SetActive(false);
        level1CatRight.gameObject.SetActive(false);
        level2Cat.gameObject.SetActive(true);

        // 4. 잠시 대기
        yield return new WaitForSeconds(waitDuration);

        // 5. 카메라 줌아웃
        yield return StartCoroutine(ResetZoom());

        // 6. 게임 타이틀 이름 떨어지는 애니메이션
        yield return StartCoroutine(TitleDropAnimation());

        // 7. 게임 시작 버튼 활성화
        GetComponent<TitleManager>().EnableStartButton();

        // 8. 게임 시작 Text 깜빡임 애니메이션
        blinkCoroutine = StartCoroutine(BlinkTouchToStartText());

        // 9. 타이틀 제목 호흡 애니메이션
        breathingCoroutine = StartCoroutine(TitleBreathingAnimation());

        // 10. 고양이들 자동 이동 시작
        StartCatAutoMovement();
    }

    #endregion


    #region Title Merge Animation

    // 고양이 움직이는 애니메이션 코루틴
    private IEnumerator MoveCatsToCenter()
    {
        float elapsed = 0f;
        Vector2 leftStart = level1CatLeft.anchoredPosition;
        Vector2 rightStart = level1CatRight.anchoredPosition;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            level1CatLeft.anchoredPosition = Vector2.Lerp(leftStart, centerPos, smoothT);
            level1CatRight.anchoredPosition = Vector2.Lerp(rightStart, centerPos, smoothT);

            yield return null;
        }

        level1CatLeft.anchoredPosition = centerPos;
        level1CatRight.anchoredPosition = centerPos;
    }

    // 줌인 코루틴
    private IEnumerator ZoomCamera(float targetSize)
    {
        float elapsed = 0f;
        float startSize = mainCamera.orthographicSize;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / zoomDuration);
            yield return null;
        }
    }

    // 줌아웃 코루틴
    private IEnumerator ResetZoom()
    {
        yield return ZoomCamera(originalOrthographicSize);
    }

    #endregion


    #region Title Name Drop

    //// 타이틀 제목 드랍 애니메이션 코루틴 (1안)
    //private IEnumerator TitleDropAnimation()
    //{
    //    titleNameImage.anchoredPosition = new Vector2(0, startY);

    //    float gravity = 3000f;              // 중력 가속도
    //    float dampening = 0.4f;             // 튕길 때 에너지 손실 계수
    //    float velocity = 0f;                // 현재 속도
    //    float currentY = startY;            // 현재 Y 위치

    //    float maxDuration = 3f;             // 최대 실행 시간 (3초)
    //    float elapsedTime = 0f;             // 총 경과 시간
    //    float stuckTime = 0f;               // 같은 위치에 머무른 시간
    //    Vector2 lastPosition = titleNameImage.anchoredPosition;

    //    while (elapsedTime < maxDuration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        float deltaTime = Time.deltaTime;

    //        // 중력 적용
    //        velocity -= gravity * deltaTime;

    //        // 새로운 위치 계산
    //        currentY += velocity * deltaTime;

    //        // 바닥 충돌 체크
    //        if (currentY < endY + 0.01f)
    //        {
    //            currentY = endY;
    //            velocity = -velocity * dampening;
    //        }

    //        // 현재 위치 적용
    //        Vector2 newPosition = new Vector2(0, currentY);
    //        titleNameImage.anchoredPosition = newPosition;

    //        // 정지 상태 감지 후 종료
    //        if (Vector2.Distance(newPosition, lastPosition) < 0.01f)
    //        {
    //            stuckTime += deltaTime;
    //            if (stuckTime > 0.5f)
    //            {
    //                break;
    //            }
    //        }
    //        else
    //        {
    //            stuckTime = 0f;
    //        }

    //        lastPosition = newPosition;
    //        yield return null;
    //    }

    //    titleNameImage.anchoredPosition = new Vector2(0, endY);
    //}

    // 타이틀 제목 드랍 애니메이션 코루틴 (2안)
    private IEnumerator TitleDropAnimation()
    {
        titleNameImage.anchoredPosition = new Vector2(0, startY);

        // 1. 위에서 아래로 이동
        float dropDuration = 0.5f;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, startY);
        Vector2 overshootPos = new Vector2(0, endY - 60f);

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            titleNameImage.anchoredPosition = Vector2.Lerp(startPos, overshootPos, smoothT);
            yield return null;
        }

        // 2. 아래에서 위로 반동
        float bounceUpDuration = 0.3f;
        elapsed = 0f;
        startPos = titleNameImage.anchoredPosition;
        Vector2 bouncePos = new Vector2(0, endY + 30f);

        while (elapsed < bounceUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceUpDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            titleNameImage.anchoredPosition = Vector2.Lerp(startPos, bouncePos, smoothT);
            yield return null;
        }

        // 3. 마지막 안착
        float settleDuration = 0.2f;
        elapsed = 0f;
        startPos = titleNameImage.anchoredPosition;
        Vector2 finalPos = new Vector2(0, endY);

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / settleDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            titleNameImage.anchoredPosition = Vector2.Lerp(startPos, finalPos, smoothT);
            yield return null;
        }

        titleNameImage.anchoredPosition = finalPos;
    }

    #endregion


    #region Touch To Start Text Animation

    // 게임 시작 Text 깜빡임 애니메이션 코루틴
    private IEnumerator BlinkTouchToStartText()
    {
        Color textColor = touchToStartText.color;
        touchToStartText.color = colorCache;

        Color maxColor = new Color(textColor.r, textColor.g, textColor.b, maxAlpha);
        Color minColor = new Color(textColor.r, textColor.g, textColor.b, minAlpha);

        while (true)
        {
            // 페이드 인 (minAlpha -> maxAlpha)
            float elapsedTime = 0f;
            Color startColor = touchToStartText.color;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, maxColor, elapsedTime);
                yield return null;
            }

            // 페이드 아웃 (maxAlpha -> minAlpha)
            elapsedTime = 0f;
            startColor = touchToStartText.color;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * fadeSpeed;
                touchToStartText.color = Color.Lerp(startColor, minColor, elapsedTime);
                yield return null;
            }
        }
    }

    // 게임 시작 Text 깜빡임 애니메이션 코루틴 중단 함수
    public void StopBlinkAnimation()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);

            touchToStartText.color = colorCache;
        }
    }

    #endregion


    #region Title Breathing Animation

    // 타이틀 제목 호흡 애니메이션 코루틴
    private IEnumerator TitleBreathingAnimation()
    {
        Vector3 originalScale = titleNameImage.localScale;
        float currentScale = 1.0f;
        bool isIncreasing = true;

        while (true)
        {
            float elapsed = 0f;
            float startScale = currentScale;
            float targetScale = isIncreasing ? maxScale : minScale;

            while (elapsed < breathingDuration / 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (breathingDuration / 2);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                currentScale = Mathf.Lerp(startScale, targetScale, smoothT);
                titleNameImage.localScale = originalScale * currentScale;

                yield return null;
            }

            isIncreasing = !isIncreasing;  // 방향 전환
        }
    }

    // 타이틀 제목 호흡 애니메이션 코루틴 중단 함수
    public void StopBreathingAnimation()
    {
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
        }
    }

    #endregion


    #region Cat Auto Movement

    // 자동이동 시작 함수
    private void StartCatAutoMovement()
    {
        catMoveCoroutines.Clear();

        foreach (RectTransform cat in animationCats)
        {
            catMoveCoroutines.Add(StartCoroutine(AutoMoveCat(cat)));
        }
    }

    // 자동이동 코루틴
    private IEnumerator AutoMoveCat(RectTransform catTransform)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(autoMoveInterval * 0.8f, autoMoveInterval * 1.2f));

            if (!isAnimating)
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)catTransform.anchoredPosition + randomDirection;

                targetPosition = AdjustPositionToBounds(targetPosition);

                yield return StartCoroutine(SmoothMoveCat(catTransform, targetPosition));
            }
        }
    }

    // 랜덤 위치 좌표 함수
    private Vector3 GetRandomDirection()
    {
        return moveDirections[Random.Range(0, moveDirections.Length)];
    }

    // 화면 범위를 벗어나지 않게 조정하는  함수
    private Vector3 AdjustPositionToBounds(Vector3 position)
    {
        Vector3 adjustedPosition = position;
        float padding = 50f;

        // mainUIPanel의 크기를 기준으로 경계 설정
        Rect panelRect = mainUIPanel.rect;
        float minX = panelRect.xMin + padding;
        float maxX = panelRect.xMax - padding;
        float minY = panelRect.yMin + padding;
        float maxY = panelRect.yMax - padding;

        adjustedPosition.x = Mathf.Clamp(adjustedPosition.x, minX, maxX);
        adjustedPosition.y = Mathf.Clamp(adjustedPosition.y, minY, maxY);

        return adjustedPosition;
    }

    // 고양이 부드럽게 이동하는 코루틴
    private IEnumerator SmoothMoveCat(RectTransform catTransform, Vector3 targetPosition)
    {
        isAnimating = true;

        Vector3 startPosition = catTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDurationCat)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDurationCat;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            catTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);
            yield return null;
        }

        catTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

    // 자동 이동을 중지하는 함수
    public void StopCatAutoMovement()
    {
        foreach (Coroutine coroutine in catMoveCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        catMoveCoroutines.Clear();

        isAnimating = false;
    }

    #endregion


}

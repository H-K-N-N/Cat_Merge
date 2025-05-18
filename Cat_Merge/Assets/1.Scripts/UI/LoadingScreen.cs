using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

// 로딩 화면 스크립트
public class LoadingScreen : MonoBehaviour
{


    #region Variables

    public static LoadingScreen Instance { get; private set; }

    [Header("---[UI Components]")]
    [SerializeField] private RectTransform circleImage;         // 중앙 원형 이미지
    [SerializeField] private RectTransform[] borderImages;      // 8개의 테두리 이미지

    [Header("---[Canvas Settings]")]
    private Canvas loadingScreenCanvas;
    private CanvasScaler canvasScaler;
    [HideInInspector] public float animationDuration = 1.25f;

    [Header("---[Animation Settings]")]
    private Coroutine currentAnimation;
    private Vector2 maxMaskSize;
    private Vector2 minMaskSize = Vector2.zero;

    private Vector2[] borderOffsets = new Vector2[8];           // 테두리 이미지들의 초기 위치 오프셋 (시계방향으로 좌상단부터)

    [Header("---[ETC]")]
    private static readonly Vector2 anchorValue = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 zeroVector = Vector2.zero;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeLoadingScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 완료 시 카메라 업데이트하는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLoadingScreenCamera();
    }

    #endregion


    #region Initialize

    // 로딩 스크린 초기화 함수
    private void InitializeLoadingScreen()
    {
        loadingScreenCanvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1080, 1920);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 1f;

        maxMaskSize = new Vector2(20000, 20000);

        // 중앙 원형 이미지 설정
        circleImage.anchorMin = anchorValue;
        circleImage.anchorMax = anchorValue;
        circleImage.pivot = anchorValue;
        circleImage.anchoredPosition = zeroVector;
        circleImage.sizeDelta = maxMaskSize;

        // 테두리 이미지들의 초기 오프셋 설정
        SetupBorderOffsets();
        SetupBorderImages();

        gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 테두리 이미지 오프셋 설정 함수
    private void SetupBorderOffsets()
    {
        float size = maxMaskSize.x;
        borderOffsets[0] = new Vector2(-size, size);    // 좌상
        borderOffsets[1] = new Vector2(0, size);        // 상
        borderOffsets[2] = new Vector2(size, size);     // 우상

        borderOffsets[3] = new Vector2(-size, 0);       // 좌
        borderOffsets[4] = new Vector2(size, 0);        // 우

        borderOffsets[5] = new Vector2(-size, -size);   // 좌하
        borderOffsets[6] = new Vector2(0, -size);       // 하
        borderOffsets[7] = new Vector2(size, -size);    // 우하
    }

    // 테두리 이미지 기본 설정 함수
    private void SetupBorderImages()
    {
        for (int i = 0; i < borderImages.Length; i++)
        {
            borderImages[i].anchorMin = anchorValue;
            borderImages[i].anchorMax = anchorValue;
            borderImages[i].pivot = anchorValue;
            borderImages[i].sizeDelta = maxMaskSize;
        }
        UpdateBorderPositions(maxMaskSize);
    }

    #endregion


    #region UI Management

    // 테두리 이미지 위치 업데이트 함수
    private void UpdateBorderPositions(Vector2 currentSize)
    {
        float currentHalf = currentSize.x * 0.5f;
        float borderHalf = maxMaskSize.x * 0.5f;
        Vector2 basePosition = circleImage.anchoredPosition;

        for (int i = 0; i < borderImages.Length; i++)
        {
            Vector2 offset;
            borderImages[i].sizeDelta = maxMaskSize;

            switch (i)
            {
                case 0: // 좌상
                    offset = new Vector2(-borderHalf - currentHalf, borderHalf + currentHalf);
                    break;
                case 1: // 상
                    offset = new Vector2(0, borderHalf + currentHalf);
                    break;
                case 2: // 우상
                    offset = new Vector2(borderHalf + currentHalf, borderHalf + currentHalf);
                    break;
                case 3: // 좌
                    offset = new Vector2(-borderHalf - currentHalf, 0);
                    break;
                case 4: // 우
                    offset = new Vector2(borderHalf + currentHalf, 0);
                    break;
                case 5: // 좌하
                    offset = new Vector2(-borderHalf - currentHalf, -borderHalf - currentHalf);
                    break;
                case 6: // 하
                    offset = new Vector2(0, -borderHalf - currentHalf);
                    break;
                case 7: // 우하
                    offset = new Vector2(borderHalf + currentHalf, -borderHalf - currentHalf);
                    break;
                default:
                    offset = Vector2.zero;
                    break;
            }

            borderImages[i].anchoredPosition = basePosition + offset;
        }
    }

    // 로딩 스크린 카메라 업데이트 함수
    public void UpdateLoadingScreenCamera()
    {
        if (loadingScreenCanvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                loadingScreenCanvas.worldCamera = mainCamera;
                loadingScreenCanvas.planeDistance = 100f;
            }
        }
    }

    #endregion


    #region Animation Control

    // 로딩 스크린 표시/숨김 제어 함수
    public void Show(bool show, Vector2 position = default)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);

            // 위치 설정 (position이 default(0,0)가 아닐 때만)
            if (position != default)
            {
                // Canvas 좌표계로 변환
                Vector2 canvasPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)loadingScreenCanvas.transform,
                    position,
                    loadingScreenCanvas.worldCamera,
                    out canvasPosition
                );

                // 중앙 이미지와 모든 Border 이미지의 부모 설정
                circleImage.anchoredPosition = canvasPosition;
                foreach (var borderImage in borderImages)
                {
                    borderImage.anchoredPosition = canvasPosition;
                }
            }
            else
            {
                // 기본값으로 중앙 설정
                circleImage.anchoredPosition = zeroVector;
                foreach (var borderImage in borderImages)
                {
                    borderImage.anchoredPosition = zeroVector;
                }
            }

            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }

            if (show)
            {
                currentAnimation = StartCoroutine(LoadingAnimationCoroutine());
            }
            else
            {
                currentAnimation = StartCoroutine(ExitAnimationCoroutine());
            }

            if (!GoogleManager.Instance.isDeleting)
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    // 로딩 애니메이션 코루틴
    private IEnumerator LoadingAnimationCoroutine()
    {
        circleImage.sizeDelta = maxMaskSize;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;
            progress = progress < 0.5f ? 2f * progress * progress : -1f + (4f - 2f * progress) * progress;

            Vector2 currentSize = Vector2.Lerp(maxMaskSize, minMaskSize, progress);
            circleImage.sizeDelta = currentSize;
            UpdateBorderPositions(currentSize);
            yield return null;
        }

        circleImage.sizeDelta = minMaskSize;
        UpdateBorderPositions(minMaskSize);
    }

    // 종료 애니메이션 코루틴
    private IEnumerator ExitAnimationCoroutine()
    {
        circleImage.sizeDelta = minMaskSize;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / animationDuration;
            progress = progress < 0.5f ? 2f * progress * progress : -1f + (4f - 2f * progress) * progress;

            Vector2 currentSize = Vector2.Lerp(minMaskSize, maxMaskSize, progress);
            circleImage.sizeDelta = currentSize;
            UpdateBorderPositions(currentSize);
            yield return null;
        }

        circleImage.sizeDelta = maxMaskSize;
        UpdateBorderPositions(maxMaskSize);
        gameObject.SetActive(false);
    }

    #endregion


}

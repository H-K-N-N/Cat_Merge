using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static TutorialManager Instance { get; private set; }

    [Header("---[Tutorial UI]")]
    [SerializeField] private GameObject tutorialPanel;          // 튜토리얼 패널
    [SerializeField] private TextMeshProUGUI tutorialText;      // 튜토리얼 텍스트
    [SerializeField] private Button tutorialButton;             // 튜토리얼 버튼
    [SerializeField] private Image enterImage;                  // 엔터(터치) 버튼

    [SerializeField] private GameObject spawnBlockingPanel;     // 유저 입력 차단용 패널 ([USER_ACTION]SpawnCat)
    [SerializeField] private GameObject mergeBlockingPanel;     // 유저 입력 차단용 패널 ([USER_ACTION]MergeCat)
    [SerializeField] private GameObject openItemBlockingPanel;  // 유저 입력 차단용 패널 ([USER_ACTION]OpenItemMenu)
    [SerializeField] private GameObject buyItemBlockingPanel;   // 유저 입력 차단용 패널 ([USER_ACTION]BuyMaxCatItem)
    [SerializeField] private GameObject mainBlockingPanel;      // 유저 입력 차단용 패널 ([USER_ACTION]ShowCatCount, [USER_ACTION]ShowGaugeBar, [USER_ACTION]ShowAutoMerge)


    [Header("---[Tutorial Settings]")]
    [SerializeField] private float typingSpeed = 0.05f;         // 타이핑 속도
    private int currentMainMessageIndex = 0;                    // 현재 메인 메시지 인덱스
    private int lastMainMessageIndex = 0;                       // 마지막으로 본 메인 메시지 인덱스
    private int currentDictionaryMessageIndex = 0;              // 현재 도감 메시지 인덱스
    private bool isTyping = false;                              // 현재 타이핑 중인지 여부
    private Coroutine typingCoroutine;                          // 타이핑 코루틴
    private bool isWaitingForUserAction = false;                // 유저 액션 대기 중인지 여부
    private bool isTutorialActive = false;                      // 튜토리얼 활성화 여부
    public bool IsTutorialActive => isTutorialActive;


    [Header("---[Enter Image Settings]")]
    [SerializeField] private float fadeSpeed = 2f;              // 페이드 속도
    [SerializeField] private float minAlpha = 0.2f;             // 최소 투명도
    [SerializeField] private float maxAlpha = 1f;               // 최대 투명도
    private Coroutine enterImageBlinkCoroutine;                 // 엔터 이미지 깜빡임 코루틴


    [Header("---[Tutorial Arrow]")]
    [SerializeField] private GameObject tutorialTopArrow;       // 튜토리얼 화살표 (위를 가리킴)
    [SerializeField] private GameObject tutorialBottomArrow;    // 튜토리얼 화살표 (아래를 가리킴)
    [SerializeField] private GameObject tutorialLeftArrow;      // 튜토리얼 화살표 (왼쪽을 가리킴)
    private bool shouldShowArrow;                               // 화살표 표시 여부
    private Coroutine arrowBlinkCoroutine;                      // 화살표 깜빡임 코루틴
    private GameObject currentArrow;                            // 현재 사용 중인 화살표

    // 합성 화살표 관련 변수
    private bool isMergeTutorialActive;                         // 합성 튜토리얼 활성화 여부
    private Coroutine mergeArrowCoroutine;                      // 합성 화살표 코루틴

    // 튜토리얼 단계를 정의하는 enum
    public enum TutorialStep
    {
        None,
        SpawnCat,           // 먹이주기 2번
        MergeCat,           // 고양이 합성 1번
        OpenItemMenu,       // 아이템 패널 열기
        BuyMaxCatItem,      // 최대 고양이 수 증가 아이템 구매
        ShowCatCount,       // 고양이 보유 수 표시
        ShowGaugeBar,       // 게이지바 표시
        ShowAutoMerge       // 자동 합성 표시
    }

    // 현재 튜토리얼 단계
    private TutorialStep currentTutorialStep = TutorialStep.None;
    public TutorialStep CurrentTutorialStep => currentTutorialStep;

    // 각 단계별 완료 조건 카운트
    private int spawnCount = 0;
    private int mergeCount = 0;

    private string[] mainTutorialMessages = new string[]        // 메인 튜토리얼 메시지
    {
        "반갑다옹! 우리랑 놀아줄 새로운 집사냐옹?",
        "게임을 어떻게 시작해야할지\n차근차근 알려주겠다옹~",
        "우리는 먹이를 주면 자연스럽게 몰려든다옹!\n'먹이주기'를 두 번 눌러보라옹!",
        "[USER_ACTION]SpawnCat",
        "이런식으로 우리를 부를 수 있다옹~\n먹이는 일정 시간마다 충전되니 참고하라옹~",
        "이번에는 두 고양이를 합쳐보자옹!\n고양이 한마리를 터치한 상태로\n같은 고양이 위로 드래그하면...",
        "더 높은 등급의 고양이가 등장한다옹!\n한번 직접 해보라옹!",
        "[USER_ACTION]MergeCat",
        "훌륭하다옹!\n그리고 우리는 평소에 '젤리'를 모은다옹.\n이 젤리는 여러 기능을 업그레이드하는데\n사용한다옹!",
        "젤리를 사용해 아이템을 구매하러 가보자옹!",
        "[USER_ACTION]OpenItemMenu",
        "[USER_ACTION]BuyMaxCatItem",
        "[USER_ACTION]ShowCatCount",
        "고양이들을 더 많이 모을 수 있게 됐다옹!\n고양이가 많을수록 더 많은 젤리를 생성하고..",
        "더 강해질 수 있다옹!\n강해져서 뭐하는지 궁금하냐옹?",
        "좌측에 게이지가 보이냐옹?\n저 게이지가 가득 차면 '대왕 쥐'가 나타나서\n우릴 괴롭힐거다옹.",
        "[USER_ACTION]ShowGaugeBar",
        "녀석들을 쫓아낼 수 있게 우릴 강하게\n만들어달라옹! 집사를 믿는다옹!",
        "마지막으로 자동합성에 대해 알려주겠다옹!\n다이아를 소모해서 자동으로 고양이들을\n부르고 합성해주는 기능이다옹!",
        "이때 직접 합성하는것도 가능하니\n자동으로 합성중에도 직접 합성해도 된다옹!",
        "[USER_ACTION]ShowAutoMerge",
        "내가 할 말은 끝이다옹!\n이것저것 눌러보며 재밌게 즐겨보라옹!"
    };
    private bool isMainTutorialEnd = false;                     // 메인 튜토리얼 완료 여부
    public bool IsMainTutorialEnd => isMainTutorialEnd;


    private string[] dictionaryTutorialMessages = new string[]  // 도감 튜토리얼 메시지
    {
        "도감을 열어봤냐옹~\n도감은 지금까지 모은 고양이를\n확인하는 공간이다옹!",
        "처음으로 데려온 고양이가 있을때마다\n다이아를 보상으로 가져오니\n꾸준히 합성해달라옹~",
        "그리고 고양이마다 집사와의\n'애정도'가 존재한다옹~",
        "고양이를 선택하고\n우측에 '고양이 정보' 쪽을 위아래로\n스크롤해보면 애정도를 볼 수 있다옹~",
        "애정도는 고양이를 모을수록 증가하고\n고양이들이 더 힘을 내서 강해지거나\n집사에게 좋은 효과를 줄거다옹~",
        "마지막으로 소파 위에 있는\n고양이를 터치하면 더 크게 구경할 수 있으니\n눌러보라옹!!"
    };
    private bool isDictionaryTutorialEnd = false;               // 도감 튜토리얼 완료 여부
    public bool IsDictionaryTutorialEnd => isDictionaryTutorialEnd;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeTutorial();
    }

    #endregion


    #region Initialize

    // 튜토리얼 초기화
    private void InitializeTutorial()
    {
        // 초기 설정
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 블로킹 패널들 초기 설정
        if (spawnBlockingPanel != null)
        {
            spawnBlockingPanel.SetActive(false);
        }
        if (mergeBlockingPanel != null)
        {
            mergeBlockingPanel.SetActive(false);
        }
        if (openItemBlockingPanel != null)
        {
            openItemBlockingPanel.SetActive(false);
        }
        if (buyItemBlockingPanel != null)
        {
            buyItemBlockingPanel.SetActive(false);
        }
        if (mainBlockingPanel != null)
        {
            mainBlockingPanel.SetActive(false);
        }

        // 화살표 이미지 초기 설정
        if (tutorialTopArrow != null)
        {
            tutorialTopArrow.SetActive(false);
        }
        if (tutorialBottomArrow != null)
        {
            tutorialBottomArrow.SetActive(false);
        }
        if (tutorialLeftArrow != null)
        {
            tutorialLeftArrow.SetActive(false);
        }

        // 버튼 이벤트 등록
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialButtonClick);
        }

        // 엔터 이미지 초기화
        if (enterImage != null)
        {
            enterImage.color = new Color(1f, 1f, 1f, maxAlpha);
        }
    }

    #endregion


    #region Tutorial System

    // 튜토리얼 시작 함수
    public void StartTutorial()
    {
        if (tutorialPanel == null || tutorialText == null) return;
        if (isMainTutorialEnd) return;

        isTutorialActive = true;
        tutorialPanel.SetActive(true);
        StartEnterImageBlink();

        currentMainMessageIndex = GetStartMessageIndex();

        // 각 단계별 초기화
        if (currentMainMessageIndex == 0)
        {
            spawnCount = 0;
            mergeCount = 0;
        }
        else if (currentMainMessageIndex <= 7)
        {
            mergeCount = 0;
        }

        ShowCurrentMessage();
    }

    // 현재 메시지 표시 함수
    private void ShowCurrentMessage()
    {
        if (currentMainMessageIndex >= mainTutorialMessages.Length)
        {
            EndTutorial();
            return;
        }

        string currentMessage = mainTutorialMessages[currentMainMessageIndex];

        // 유저 액션이 필요한 메시지인지 확인
        if (currentMessage.StartsWith("[USER_ACTION]"))
        {
            string actionType = currentMessage.Substring(13);
            HandleUserAction(actionType);
            return;
        }

        // 이전 타이핑 코루틴이 실행 중이라면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 새로운 타이핑 코루틴 시작
        typingCoroutine = StartCoroutine(TypeMessage(currentMessage));
    }

    // 메시지 타이핑 효과 코루틴
    private IEnumerator TypeMessage(string message)
    {
        isTyping = true;
        tutorialText.text = "";

        foreach (char letter in message)
        {
            tutorialText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    // 유저 액션 처리 함수
    private void HandleUserAction(string actionType)
    {
        isWaitingForUserAction = true;
        currentTutorialStep = (TutorialStep)Enum.Parse(typeof(TutorialStep), actionType);
        shouldShowArrow = true;

        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                // 고양이 소환 2회 대기
                spawnBlockingPanel.SetActive(true);
                UpdateArrowPosition("Spawn Button");
                break;

            case TutorialStep.MergeCat:
                // 고양이 합성 1회 대기
                mergeBlockingPanel.SetActive(true);
                isMergeTutorialActive = true;
                tutorialTopArrow.SetActive(true);
                StartMergeArrowMove();
                break;

            case TutorialStep.OpenItemMenu:
                // 아이템 패널 열기 대기
                openItemBlockingPanel.SetActive(true);
                UpdateArrowPosition("ItemMenu Button");
                break;

            case TutorialStep.BuyMaxCatItem:
                // 고양이 최대치 증가 아이템 1회 구매 대기
                buyItemBlockingPanel.SetActive(true);
                UpdateArrowPosition("IncreaseCatMaximumButton");
                break;

            case TutorialStep.ShowCatCount:
                StartCoroutine(ShowAndProceed());
                UpdateArrowPosition("CatCount");
                break;

            case TutorialStep.ShowGaugeBar:
                StartCoroutine(ShowAndProceed());
                UpdateArrowPosition("GaugeBar");
                break;

            case TutorialStep.ShowAutoMerge:
                StartCoroutine(ShowAndProceed());
                UpdateArrowPosition("AutoMerge");
                break;

            default:
                shouldShowArrow = false;
                break;
        }

        UpdateArrowVisibility();

        tutorialPanel.SetActive(false);
    }

    // 스폰 시스템 관련 함수
    public void OnCatSpawned()
    {
        if (currentTutorialStep != TutorialStep.SpawnCat) return;

        spawnCount++;
        if (spawnCount >= 2)
        {
            CompleteCurrentStep();
        }
    }

    // 합성 시스템 관련 함수
    public void OnCatMerged()
    {
        if (currentTutorialStep != TutorialStep.MergeCat) return;

        mergeCount++;
        if (mergeCount >= 1)
        {
            CompleteCurrentStep();
        }
    }

    // 아이템 상점 열기/닫기 체크 함수
    public void OnOpenItemMenu()
    {
        if (currentTutorialStep != TutorialStep.OpenItemMenu) return;

        CompleteCurrentStep();
    }

    // 아이템 구매 완료 체크 함수
    public void OnMaxCatItemPurchased()
    {
        if (currentTutorialStep != TutorialStep.BuyMaxCatItem) return;

        if (ActivePanelManager.Instance != null)
        {
            ActivePanelManager.Instance.ClosePanel("BottomItemMenu");
        }

        CompleteCurrentStep();
    }

    // 2초 대기 후 다음 단계로 진행하는 코루틴
    private IEnumerator ShowAndProceed()
    {
        // 블로킹 패널 활성화
        if (mainBlockingPanel != null)
        {
            mainBlockingPanel.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        // 블로킹 패널 비활성화
        if (mainBlockingPanel != null)
        {
            mainBlockingPanel.SetActive(false);
        }

        CompleteCurrentStep();
    }

    // 현재 단계 완료 처리 함수
    private void CompleteCurrentStep()
    {
        isWaitingForUserAction = false;
        shouldShowArrow = false;
        StopArrowMove();
        tutorialPanel.SetActive(true);

        // 블로킹 패널들 비활성화
        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                spawnBlockingPanel.SetActive(false);
                break;

            case TutorialStep.MergeCat:
                mergeBlockingPanel.SetActive(false);
                isMergeTutorialActive = false;
                break;

            case TutorialStep.OpenItemMenu:
                openItemBlockingPanel.SetActive(false);
                break;

            case TutorialStep.BuyMaxCatItem:
                buyItemBlockingPanel.SetActive(false);
                break;
        }

        currentMainMessageIndex++;
        lastMainMessageIndex = currentMainMessageIndex;
        ShowCurrentMessage();
    }

    // 튜토리얼 버튼 클릭 처리 함수
    private void OnTutorialButtonClick()
    {
        if (isWaitingForUserAction) return;

        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            tutorialText.text = isTutorialActive ? mainTutorialMessages[currentMainMessageIndex] : dictionaryTutorialMessages[currentDictionaryMessageIndex];
            isTyping = false;
        }
        else
        {
            if (isTutorialActive)
            {
                currentMainMessageIndex++;
                lastMainMessageIndex = currentMainMessageIndex;
                ShowCurrentMessage();
            }
            else
            {
                currentDictionaryMessageIndex++;
                ShowDictionaryMessage();
            }
        }
    }

    // 튜토리얼 종료 함수
    private void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        isMainTutorialEnd = true;
        StopEnterImageBlink();

        // 도감 버튼 상호작용 업데이트
        if (DictionaryManager.Instance != null)
        {
            DictionaryManager.Instance.UpdateDictionaryButtonInteractable();
        }

        NotificationManager.Instance.ShowNotification("튜토리얼 완료보상으로 100다이아를 획득하였습니다.");
        GameManager.Instance.Cash += 100;
    }

    // 튜토리얼 진행중에 게임 재접속했을때 다시 시작하는 위치를 결정하는 함수
    private int GetStartMessageIndex()
    {
        // 이전 메시지 위치에 따라 적절한 시작 위치 반환
        if (lastMainMessageIndex <= 3)
        {
            return 0;
        }
        else if (lastMainMessageIndex <= 7)
        {
            return 4;
        }
        else if (lastMainMessageIndex <= 11)
        {
            return 8;
        }
        else if (lastMainMessageIndex <= 16)
        {
            return 13;
        }
        else if (lastMainMessageIndex <= 20)
        {
            return 17;
        }
        else if (lastMainMessageIndex <= 21)
        {
            return 21;
        }

        return 0;
    }

    #endregion


    #region Panel State

    // 화살표 표시 여부 업데이트
    private void UpdateArrowVisibility()
    {
        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                // spawnBlockingPanel이 있으므로 항상 화살표 표시
                shouldShowArrow = true;
                break;

            case TutorialStep.MergeCat:
                // mergeBlockingPanel이 있으므로 항상 화살표 표시
                shouldShowArrow = true;
                break;

            case TutorialStep.OpenItemMenu:
                // 어떤 패널을 열어도 ItemMenu는 보이기 때문에 화살표 항상 표시
                shouldShowArrow = true;
                break;

            case TutorialStep.BuyMaxCatItem:
                // 아이템 패널이 열려있을 때만 화살표 표시
                bool isItemPanelOpen = ActivePanelManager.Instance.IsPanelActive("BottomItemMenu");
                shouldShowArrow = isItemPanelOpen;
                break;

            case TutorialStep.ShowCatCount:
            case TutorialStep.ShowGaugeBar:
            case TutorialStep.ShowAutoMerge:
                // mainBlockingPanel이 있으므로 항상 화살표 표시
                shouldShowArrow = true;
                break;

            default:
                shouldShowArrow = false;
                break;
        }

        // 화살표 상태 업데이트
        if (shouldShowArrow && currentArrow != null)
        {
            currentArrow.SetActive(true);
            StartArrowMove();
        }
        else if (!shouldShowArrow && currentArrow != null)
        {
            StopArrowMove();
        }
    }

    #endregion


    #region Dictionary Tutorial

    // 도감 튜토리얼 시작
    public void StartDictionaryTutorial()
    {
        if (tutorialPanel == null || tutorialText == null) return;
        if (isDictionaryTutorialEnd) return;
        if (isTutorialActive) return;

        tutorialPanel.SetActive(true);
        currentDictionaryMessageIndex = 0;
        StartEnterImageBlink();

        ShowDictionaryMessage();
    }

    // 도감 튜토리얼 메시지 표시
    private void ShowDictionaryMessage()
    {
        if (currentDictionaryMessageIndex >= dictionaryTutorialMessages.Length)
        {
            EndDictionaryTutorial();
            return;
        }

        string currentMessage = dictionaryTutorialMessages[currentDictionaryMessageIndex];

        // 이전 타이핑 코루틴이 실행 중이라면 중지
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 새로운 타이핑 코루틴 시작
        typingCoroutine = StartCoroutine(TypeDictionaryMessage(currentMessage));
    }

    // 도감 튜토리얼 메시지 타이핑 효과
    private IEnumerator TypeDictionaryMessage(string message)
    {
        isTyping = true;
        tutorialText.text = "";

        foreach (char letter in message)
        {
            tutorialText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    // 도감 튜토리얼 종료
    private void EndDictionaryTutorial()
    {
        isDictionaryTutorialEnd = true;
        tutorialPanel.SetActive(false);
        StopEnterImageBlink();

        NotificationManager.Instance.ShowNotification("도감 튜토리얼을 완료하였습니다.");
    }

    #endregion


    #region Enter Image Effect

    // 엔터 이미지 깜빡임 시작
    private void StartEnterImageBlink()
    {
        if (enterImage == null) return;

        // 이전 코루틴이 실행 중이라면 중지
        if (enterImageBlinkCoroutine != null)
        {
            StopCoroutine(enterImageBlinkCoroutine);
        }

        enterImageBlinkCoroutine = StartCoroutine(BlinkEnterImage());
    }

    // 엔터 이미지 깜빡임 중지
    private void StopEnterImageBlink()
    {
        if (enterImageBlinkCoroutine != null)
        {
            StopCoroutine(enterImageBlinkCoroutine);
            enterImageBlinkCoroutine = null;
        }

        // 이미지 초기 상태로 복구
        if (enterImage != null)
        {
            enterImage.color = new Color(1f, 1f, 1f, maxAlpha);
        }
    }

    // 엔터 이미지 깜빡임 코루틴
    private IEnumerator BlinkEnterImage()
    {
        float currentAlpha = maxAlpha;
        bool fadeOut = true;

        while (true)
        {
            if (fadeOut)
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, minAlpha, fadeSpeed * Time.deltaTime);
                if (currentAlpha <= minAlpha)
                {
                    fadeOut = false;
                }
            }
            else
            {
                currentAlpha = Mathf.MoveTowards(currentAlpha, maxAlpha, fadeSpeed * Time.deltaTime);
                if (currentAlpha >= maxAlpha)
                {
                    fadeOut = true;
                }
            }

            enterImage.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
    }

    #endregion


    #region Arrow Effect

    // 현재 단계에 맞는 화살표 선택
    private GameObject GetCurrentArrow()
    {
        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
            case TutorialStep.OpenItemMenu:
            case TutorialStep.ShowAutoMerge:
                return tutorialBottomArrow;
            case TutorialStep.BuyMaxCatItem:
            case TutorialStep.ShowCatCount:
                return tutorialTopArrow;
            case TutorialStep.ShowGaugeBar:
                return tutorialLeftArrow;
            case TutorialStep.MergeCat:
                return tutorialTopArrow;
            default:
                return null;
        }
    }

    // 화살표 위치 업데이트
    private void UpdateArrowPosition(string targetButtonName)
    {
        // 이전 화살표 비활성화
        if (currentArrow != null)
        {
            currentArrow.SetActive(false);
        }

        // 현재 단계에 맞는 화살표 선택
        currentArrow = GetCurrentArrow();
        if (currentArrow == null) return;

        RectTransform arrowRect = currentArrow.GetComponent<RectTransform>();
        if (arrowRect != null)
        {
            Vector2 targetPosition = GetArrowPosition(targetButtonName);

            // 화살표 활성화 및 위치 설정
            currentArrow.SetActive(true);
            arrowRect.localPosition = new Vector3(targetPosition.x, targetPosition.y, 0f);
            arrowRect.localScale = Vector3.one;
        }
    }

    // 각 버튼별 화살표 위치 반환
    private Vector2 GetArrowPosition(string targetButtonName)
    {
        switch (targetButtonName)
        {
            case "Spawn Button":
                return new Vector2(370f, -725.5f);
            case "ItemMenu Button":
                return new Vector2(-445f, -725.5f);
            case "IncreaseCatMaximumButton":
                return new Vector2(330f, 270f);
            case "CatCount":
                return new Vector2(-320f, 785f);
            case "GaugeBar":
                return new Vector2(-360f, -320f);
            case "AutoMerge":
                return new Vector2(-350f, -565f);
            default:
                return Vector2.zero;
        }
    }

    // 화살표 움직임 시작
    private void StartArrowMove()
    {
        if (currentTutorialStep == TutorialStep.MergeCat)
        {
            StartMergeArrowMove();
            return;
        }

        if (arrowBlinkCoroutine != null)
        {
            StopCoroutine(arrowBlinkCoroutine);
        }
        arrowBlinkCoroutine = StartCoroutine(MoveArrow());
    }

    // 화살표 화살표 움직임 중지
    private void StopArrowMove()
    {
        if (arrowBlinkCoroutine != null)
        {
            StopCoroutine(arrowBlinkCoroutine);
            arrowBlinkCoroutine = null;
        }

        if (mergeArrowCoroutine != null)
        {
            StopCoroutine(mergeArrowCoroutine);
            mergeArrowCoroutine = null;
        }

        isMergeTutorialActive = false;

        // 모든 화살표 비활성화
        if (tutorialTopArrow != null) tutorialTopArrow.SetActive(false);
        if (tutorialBottomArrow != null) tutorialBottomArrow.SetActive(false);
        if (tutorialLeftArrow != null) tutorialLeftArrow.SetActive(false);
        currentArrow = null;
    }

    // 화살표 움직임 코루틴
    private IEnumerator MoveArrow()
    {
        if (currentArrow == null) yield break;

        RectTransform arrowRect = currentArrow.GetComponent<RectTransform>();
        if (arrowRect == null) yield break;

        Vector2 originalPosition = arrowRect.localPosition;
        float moveTime = 0f;
        float moveDuration = 1f; // 한 번의 왕복에 걸리는 시간

        while (shouldShowArrow)
        {
            moveTime += Time.deltaTime;
            float normalizedTime = (moveTime % moveDuration) / moveDuration; // 0~1 사이의 값
            float moveProgress = Mathf.Sin(normalizedTime * Mathf.PI * 2) * 0.5f + 0.5f; // 0~1 사이를 부드럽게 왕복

            Vector3 newPosition = originalPosition;

            if (currentArrow == tutorialTopArrow)
            {
                float yOffset = Mathf.Lerp(0, -50f, moveProgress);
                newPosition.y = originalPosition.y + yOffset;
            }
            else if (currentArrow == tutorialBottomArrow)
            {
                float yOffset = Mathf.Lerp(0, 50f, moveProgress);
                newPosition.y = originalPosition.y + yOffset;
            }
            else if (currentArrow == tutorialLeftArrow)
            {
                float xOffset = Mathf.Lerp(0, 50f, moveProgress);
                newPosition.x = originalPosition.x + xOffset;
            }

            arrowRect.localPosition = newPosition;
            yield return null;
        }

        // 움직임이 끝나면 원래 위치로 복귀
        arrowRect.localPosition = originalPosition;
    }

    // 합성 화살표 이동 시작
    private void StartMergeArrowMove()
    {
        if (mergeArrowCoroutine != null)
        {
            StopCoroutine(mergeArrowCoroutine);
        }

        isMergeTutorialActive = true;
        tutorialTopArrow.SetActive(true);
        mergeArrowCoroutine = StartCoroutine(MoveMergeArrow());
    }

    // 합성 화살표 이동 코루틴
    private IEnumerator MoveMergeArrow()
    {
        float moveSpeed = 0.8f; // 이동 속도
        RectTransform arrowRect = tutorialTopArrow.GetComponent<RectTransform>();

        while (isMergeTutorialActive)
        {
            // 활성화된 고양이들의 위치 가져오기
            var activeCats = SpawnManager.Instance.GetActiveCats();
            if (activeCats.Count < 2) yield return null;

            // 첫 번째와 두 번째 고양이의 위치 가져오기
            Vector2 startPos = activeCats[0].GetComponent<RectTransform>().anchoredPosition + Vector2.down * 100f;
            Vector2 endPos = activeCats[1].GetComponent<RectTransform>().anchoredPosition + Vector2.down * 100f;

            float progress = 0f;
            arrowRect.anchoredPosition = startPos;

            while (progress < 1f && isMergeTutorialActive)
            {
                progress += Time.deltaTime * moveSpeed;
                arrowRect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
                yield return null;
            }
        }
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool isMainTutorialEnd;
        public bool isDictionaryTutorialEnd;
        public int lastMainMessageIndex;
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isMainTutorialEnd = this.isMainTutorialEnd,
            isDictionaryTutorialEnd = this.isDictionaryTutorialEnd,
            lastMainMessageIndex = this.lastMainMessageIndex
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.isMainTutorialEnd = savedData.isMainTutorialEnd;
        this.isDictionaryTutorialEnd = savedData.isDictionaryTutorialEnd;
        this.lastMainMessageIndex = savedData.lastMainMessageIndex;

        // 데이터 로드 후 도감 버튼 상호작용 업데이트
        if (DictionaryManager.Instance != null)
        {
            DictionaryManager.Instance.UpdateDictionaryButtonInteractable();
        }
    }

    #endregion


}

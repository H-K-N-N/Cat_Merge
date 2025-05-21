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
        AutoMerge           // 자동 합성 표시
    }

    // 현재 튜토리얼 단계
    private TutorialStep currentTutorialStep = TutorialStep.None;

    // 각 단계별 완료 조건 카운트
    private int spawnCount = 0;
    private int mergeCount = 0;

    private string[] mainTutorialMessages = new string[]        // 메인 튜토리얼 메시지
    {
        "반갑다옹! 우리랑 놀아줄 새로운 집사냐옹?",
        "마침 잘됐다옹! 어떻게 시작해야할지 차근차근 알려주겠다옹~",
        "우리는 먹이를 주면 자연스럽게 몰려든다옹!\n'먹이주기'를 두 번 눌러보라옹!",
        "[USER_ACTION]SpawnCat",
        "내 친구들이 왔다옹!\n이런식으로 우리를 부를 수 있다옹~\n먹이는 일정 시간마다 충전된다옹~",
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
        "[USER_ACTION]AutoMerge",
        "내가 할 말은 끝이다옹!\n이것저것 눌러보며 재밌게 즐겨보라옹!"
    };
    private bool isMainTutorialEnd = false;                     // 메인 튜토리얼 완료 여부
    

    private string[] dictionaryTutorialMessages = new string[]  // 도감 튜토리얼 메시지
    {
        "도감을 열어봤냐옹~\n지금까지 얼마나 많은 고양이를\n해금했는지 확인하는 공간이다옹!",
        "처음으로 데려온 고양이가 있을때마다\n다이아를 보상으로 가져오니\n꾸준히 합성해달라옹~",
        "그리고 고양이마다 집사와의\n'애정도'가 존재한다옹~",
        "고양이를 선택하고\n고양이 정보 쪽을 위아래로\n스크롤해보면 애정도를 볼 수 있다옹~",
        "애정도는 고양이를 모을수록 증가하고\n고양이들이 더 힘을 내서 강해지거나\n집사에게 좋은 효과를 줄거다옹~",
        "마지막으로 소파 위에 있는\n고양이를 터치하면 더 크게 구경할 수 있다옹!!"
    };
    [HideInInspector] public bool isDictionaryTutorialEnd = false;      // 도감 튜토리얼 완료 여부

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

        // 버튼 이벤트 등록
        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialButtonClick);
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

        // lastMainMessageIndex에 따라 시작 위치 결정
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

        // 튜토리얼 시작 시 시스템 일시 정지
        PauseGameSystems();

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
            string actionType = currentMessage.Substring(13); // "[USER_ACTION]" 이후의 문자열
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

        switch (currentTutorialStep)
        {
            case TutorialStep.SpawnCat:
                ResumeSpawnSystem();
                break;
            case TutorialStep.MergeCat:
                ResumeMergeSystem();
                break;
            case TutorialStep.OpenItemMenu:
                ResumeItemSystem();
                tutorialPanel.SetActive(false);
                break;
            case TutorialStep.BuyMaxCatItem:
                // 아이템 구매 대기
                break;
            case TutorialStep.ShowCatCount:
                StartCoroutine(ShowAndProceed());
                break;
            case TutorialStep.ShowGaugeBar:
                StartCoroutine(ShowAndProceed());
                break;
            case TutorialStep.AutoMerge:
                StartCoroutine(ShowAndProceed());
                break;
            default:
                break;
        }

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

    // 아이템 패널 열기 완료 체크 함수
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
        yield return new WaitForSeconds(2f);

        CompleteCurrentStep();
    }

    // 현재 단계 완료 처리 함수
    private void CompleteCurrentStep()
    {
        isWaitingForUserAction = false;
        PauseGameSystems();
        tutorialPanel.SetActive(true);

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

            tutorialText.text = isDictionaryTutorialEnd ?
                mainTutorialMessages[currentMainMessageIndex] :
                dictionaryTutorialMessages[currentDictionaryMessageIndex];

            isTyping = false;
        }
        else
        {
            if (isDictionaryTutorialEnd)
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

    // 게임 시스템 일시 정지 함수
    private void PauseGameSystems()
    {
        // 보스 게이지 정지
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.enabled)
            {
                BattleManager.Instance.enabled = false;
            }
        }

        // 자동 먹이 생성 정지
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = false;
        }

        // 기타 시스템들 정지
        if (AutoMergeManager.Instance != null)
        {
            AutoMergeManager.Instance.enabled = false;
        }
    }

    // 스폰 시스템 재개 함수
    private void ResumeSpawnSystem()
    {
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = true;
        }
    }

    // 합성 시스템 재개 함수
    private void ResumeMergeSystem()
    {
        if (MergeManager.Instance != null)
        {
            MergeManager.Instance.enabled = true;
        }
    }

    // 아이템 시스템 재개 함수
    private void ResumeItemSystem()
    {
        if (ItemMenuManager.Instance != null)
        {
            ItemMenuManager.Instance.enabled = true;
        }
    }

    // 게임 시스템 재개 함수
    private void ResumeGameSystems()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.enabled = true;
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.enabled = true;
        }

        if (AutoMergeManager.Instance != null)
        {
            AutoMergeManager.Instance.enabled = true;
        }
    }

    // 튜토리얼 종료 함수
    private void EndTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        isMainTutorialEnd = true;

        // 모든 시스템 재개
        ResumeGameSystems();

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


    #region Dictionary Tutorial

    // 도감 튜토리얼 시작
    public void StartDictionaryTutorial()
    {
        if (tutorialPanel == null || tutorialText == null) return;
        if (isDictionaryTutorialEnd) return;
        if (isTutorialActive) return;

        tutorialPanel.SetActive(true);
        currentDictionaryMessageIndex = 0;

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
    }

    #endregion


}

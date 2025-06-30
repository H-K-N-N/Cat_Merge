using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;

// 퀘스트 스크립트
[DefaultExecutionOrder(-1)]
public class QuestManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static QuestManager Instance { get; private set; }

    // QuestUI Class
    public class QuestUI
    {
        public TextMeshProUGUI questName;           // 퀘스트 이름
        public Slider questSlider;                  // Slider
        public TextMeshProUGUI countText;           // "?/?" Text
        public TextMeshProUGUI plusCashText;        // 보상 재화 개수 Text
        public Button rewardButton;                 // 보상 버튼
        public TextMeshProUGUI rewardText;          // 보상 획득 Text
        public GameObject rewardDisabledBG;         // 보상 버튼 비활성화 BG
        public Transform slotTransform;             // 해당 슬롯의 Transform 참조
        public Image questImage;                    // 퀘스트 이미지

        public class QuestData
        {
            public int currentCount;                // 현재 수치
            public int targetCount;                 // 목표 수치
            public int baseTargetCount;             // 기본 목표 수치 (초기값)
            public int rewardCount;                 // 보상 받은 횟수
            public int rewardCash;                  // 보상 캐쉬
            public bool isComplete;                 // 완료 여부
        }
        public QuestData questData = new QuestData();
    }

    // ======================================================================================================================

    [Header("---[QuestManager]")]
    [SerializeField] private Button questButton;                                // 퀘스트 버튼
    [SerializeField] private Image questButtonImage;                            // 퀘스트 버튼 이미지
    [SerializeField] private GameObject questMenuPanel;                         // 퀘스트 메뉴 Panel
    [SerializeField] private Button questBackButton;                            // 퀘스트 뒤로가기 버튼
    private ActivePanelManager activePanelManager;                              // ActivePanelManager
    [SerializeField] private GameObject questSlotPrefab;                        // Quest Slot Prefab

    [SerializeField] private GameObject[] mainQuestMenus;                       // 메인 퀘스트 메뉴 Panels
    [SerializeField] private Button[] subQuestMenuButtons;                      // 서브 퀘스트 메뉴 버튼 배열
    [SerializeField] private Transform[] questSlotParents;                      // 슬롯들이 배치될 부모 객체들 (일일, 주간, 반복)


    private Dictionary<string, QuestUI> dailyQuestDictionary = new Dictionary<string, QuestUI>();       // 일일퀘스트 Dictionary
    private Dictionary<string, QuestUI> weeklyQuestDictionary = new Dictionary<string, QuestUI>();      // 주간퀘스트 Dictionary

    private Dictionary<string, QuestUI> repeatQuestDictionary = new Dictionary<string, QuestUI>();      // 반복퀘스트 Dictionary
    private List<QuestUI> sortedRepeatQuestList = new List<QuestUI>();                                  // 정렬된 반복퀘스트 정보를 담을 List
    private Dictionary<Transform, Coroutine> activeAnimations = new Dictionary<Transform, Coroutine>(); // 반복퀘스트 정렬 코루틴 Dictionary

    // ======================================================================================================================

    [Header("---[Daily Special Reward UI]")]
    [SerializeField] private Slider dailySpecialRewardSlider;                   // Daily Special Reward Slider
    [SerializeField] private TextMeshProUGUI dailySpecialRewardCountText;       // "?/?" 텍스트
    [SerializeField] private TextMeshProUGUI dailySpecialRewardPlusCashText;    // Daily Special Reward 보상 재화 개수 Text
    [SerializeField] private Button dailySpecialRewardButton;                   // Daily Special Reward 버튼
    [SerializeField] private TextMeshProUGUI dailySpecialRewardText;            // Daily Special Reward 보상 획득 Text
    [SerializeField] private GameObject dailySpecialRewardDisabledBG;           // Daily Special Reward 보상 버튼 비활성화 BG
    private int dailySpecialRewardTargetCount;                                  // Daily 목표 횟수
    private int dailySpecialRewardQuestRewardCash = 500;                        // Daily Special Reward 퀘스트 보상 캐쉬 재화 개수
    private bool isDailySpecialRewardQuestComplete;                             // Daily Special Reward 퀘스트 완료 여부 상태

    [Header("---[Weekly Special Reward UI]")]
    [SerializeField] private Slider weeklySpecialRewardSlider;                  // Weekly Special Reward Slider
    [SerializeField] private TextMeshProUGUI weeklySpecialRewardCountText;      // "?/?" 텍스트
    [SerializeField] private TextMeshProUGUI weeklySpecialRewardPlusCashText;   // Weekly Special Reward 보상 재화 개수 Text
    [SerializeField] private Button weeklySpecialRewardButton;                  // Weekly Special Reward 버튼
    [SerializeField] private TextMeshProUGUI weeklySpecialRewardText;           // Weekly Special Reward 보상 획득 Text
    [SerializeField] private GameObject weeklySpecialRewardDisabledBG;          // Weekly Special Reward 보상 버튼 비활성화 BG
    private int weeklySpecialRewardTargetCount;                                 // Weekly 목표 횟수
    private int weeklySpecialRewardQuestRewardCash = 2500;                      // Weekly Special Reward 퀘스트 보상 캐쉬 재화 개수
    private bool isWeeklySpecialRewardQuestComplete;                            // Weekly Special Reward 퀘스트 완료 여부 상태

    // ======================================================================================================================

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject mainQuestButtonNewImage;                // Main 퀘스트 버튼의 New Image
    [SerializeField] private GameObject[] subQuestButtonNewImages;              // Sub 퀘스트 버튼들의 New Image

    [Header("---[All Reward Button]")]
    [SerializeField] private Button[] allRewardButtons;                         // All RewardButtons

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#FFCC74";                                 // 활성화상태 Color
    private string inactiveColorCode = "#FFFFFF";                               // 비활성화상태 Color

    private string resourcePath = "Sprites/UI/I_UI_Quest/";                     // 퀘스트들의 이미지 기본 경로

    // ======================================================================================================================

    // Enum으로 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum QuestMenuType
    {
        Daily,                              // 일일 퀘스트 메뉴
        Weekly,                             // 주간 퀘스트 메뉴
        Repeat,                             // 반복 퀘스트 메뉴
        End                                 // Enum의 끝
    }
    private QuestMenuType activeMenuType;   // 현재 활성화된 메뉴 타입

    // ======================================================================================================================
    // [퀘스트 변수 모음]

    // 일일 퀘스트 카운터
    private float dailyPlayTimeCount;
    private int dailyMergeCount;
    private int dailySpawnCount;
    private int dailyBattleCount;

    // 주간 퀘스트 카운터
    private float weeklyPlayTimeCount;
    private int weeklyMergeCount;
    private int weeklySpawnCount;
    private int weeklyBattleCount;

    // 반복 퀘스트 카운터 (누적)
    private int totalMergeCount;
    private int totalSpawnCount;
    private int totalBattleCount;
    //private int totalPurchaseCount;

    public int StageCount { get => BattleManager.Instance.BossStage; }  // 스테이지 단계

    private int dailySpecialRewardCount;                                // Daily 최종 퀘스트 진행 횟수
    public int DailySpecialRewardCount { get => dailySpecialRewardCount; set => dailySpecialRewardCount = value; }

    private int weeklySpecialRewardCount;                               // Weekly 최종 퀘스트 진행 횟수
    public int WeeklySpecialRewardCount { get => weeklySpecialRewardCount; set => weeklySpecialRewardCount = value; }


    private long lastDailyReset;                // 일일 퀘스트 리셋을 위한 시간 변수
    private long lastWeeklyReset;               // 주간 퀘스트 리셋을 위한 시간 변수

    private bool isDataLoaded = false;          // 데이터 로드 확인

    private readonly WaitForSeconds oneSecondWait = new WaitForSeconds(1f);
    private Coroutine questCheckCoroutine;

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
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        InitializeDebugControls();
#endif

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            lastDailyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastWeeklyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        InitializeQuestManager();

        CheckAndResetQuestsOnStart();

        UpdateAllUI();

        questCheckCoroutine = StartCoroutine(QuestCheckRoutine());
    }

    private void OnDisable()
    {
        if (questCheckCoroutine != null)
        {
            StopCoroutine(questCheckCoroutine);
            questCheckCoroutine = null;
        }
    }

    #endregion


    #region Daily & Weekly Quest Initialize Check

    // 퀘스트 체크 코루틴
    private IEnumerator QuestCheckRoutine()
    {
        while (true)
        {
            // 플레이타임 퀘스트 최대치 체크
            bool canAddDailyPlayTime = dailyQuestDictionary["플레이 시간"].questData.currentCount < dailyQuestDictionary["플레이 시간"].questData.targetCount;
            bool canAddWeeklyPlayTime = weeklyQuestDictionary["플레이 시간"].questData.currentCount < weeklyQuestDictionary["플레이 시간"].questData.targetCount;

            // 둘 중 하나라도 최대치에 도달하지 않았다면 플레이타임 추가
            if (canAddDailyPlayTime || canAddWeeklyPlayTime)
            {
                AddPlayTimeCount();
            }

            CheckAndResetQuests();

            yield return oneSecondWait;
        }
    }

    #endregion


    #region Initialize

    // 앱 시작 시 시간 체크 및 초기화
    private void CheckAndResetQuestsOnStart()
    {
        DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
        DateTimeOffset lastDailyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastDailyReset);
        DateTimeOffset lastWeeklyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastWeeklyReset);

        // 마지막 일일 리셋 이후 지난 날짜 수 계산
        int daysSinceLastDaily = (currentUtc.Date - lastDailyResetTime.Date).Days;

        // 하루 이상 지났다면 일일 퀘스트 초기화
        if (daysSinceLastDaily > 0)
        {
            //Debug.Log($"[Quest] 일일 퀘스트 초기화 - 마지막 리셋: {lastDailyResetTime}, 현재: {currentUtc}, 경과일: {daysSinceLastDaily}");
            ResetDailyQuests();
            lastDailyReset = currentUtc.ToUnixTimeSeconds();
        }

        // 주간 퀘스트 체크
        DateTimeOffset nextThursday = GetNextThursday(lastWeeklyResetTime);
        if (currentUtc >= nextThursday)
        {
            //Debug.Log($"[Quest] 주간 퀘스트 초기화 - 마지막 리셋: {lastWeeklyResetTime}, 현재: {currentUtc}, 다음 목요일: {nextThursday}");
            ResetWeeklyQuests();
            lastWeeklyReset = currentUtc.ToUnixTimeSeconds();
        }
    }

    // 모든 QuestManager 시작 함수들 모음
    private void InitializeQuestManager()
    {
        questMenuPanel.SetActive(false);
        mainQuestButtonNewImage.SetActive(false);
        activeMenuType = QuestMenuType.Daily;

        InitializeActivePanel();
        InitializeSubMenuButtons();

        InitializeQuests();
    }

    // 모든 Quest 초기화
    private void InitializeQuests()
    {
        InitializeDailyQuestManager();
        InitializeWeeklyQuestManager();
        InitializeRepeatQuestManager();
    }

    // ActivePanel 초기화 함수
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("QuestMenu", questMenuPanel, questButtonImage);

        questButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("QuestMenu");
            InitializeRepeatQuestUIFromSortedList();
        });
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }

    // Daily Quest 설정 함수
    private void InitializeDailyQuestManager()
    {
        InitializeQuest("플레이 시간", 600, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("고양이 합성 횟수", 60, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("고양이 소환 횟수", 60, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");
        InitializeQuest("전투 횟수", 1, 5, QuestMenuType.Daily, "I_UI_Mission_Daily.9");

        InitializeDailySpecialReward();

        allRewardButtons[(int)QuestMenuType.Daily].onClick.AddListener(ReceiveAllDailyRewards);
        UpdateAllDailyRewardButtonState();
    }

    // Weekly Quest 설정 함수
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("플레이 시간", 3000, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");
        InitializeQuest("고양이 합성 횟수", 600, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");
        InitializeQuest("고양이 소환 횟수", 600, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");
        InitializeQuest("전투 횟수", 5, 50, QuestMenuType.Weekly, "I_UI_Mission_Weekly.9");

        InitializeWeeklySpecialReward();

        allRewardButtons[(int)QuestMenuType.Weekly].onClick.AddListener(ReceiveAllWeeklyRewards);
        UpdateAllWeeklyRewardButtonState();
    }

    // Repeat Quest 설정 함수
    private void InitializeRepeatQuestManager()
    {
        InitializeQuest("고양이 합성 횟수", 160, 4, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("고양이 소환 횟수", 160, 4, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        //InitializeQuest("고양이 구매 횟수", 20, 5, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");
        InitializeQuest("전투 횟수", 3, 4, QuestMenuType.Repeat, "I_UI_Mission_Daily.9");

        // 초기 스크롤 위치 초기화
        InitializeScrollPosition();

        allRewardButtons[(int)QuestMenuType.Repeat].onClick.AddListener(ReceiveAllRepeatRewards);
        UpdateAllRepeatRewardButtonState();
    }

    // 도감 스크롤 패널 초기 위치를 설정하는 함수
    private void InitializeScrollPosition()
    {
        ScrollRect scrollRect = mainQuestMenus[(int)QuestMenuType.Repeat].GetComponentInChildren<ScrollRect>();
        RectTransform content = scrollRect.content;
        GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();

        // 슬롯 개수와 콘텐츠 크기 계산
        int slotCount = content.childCount;
        float cellHeight = gridLayout.cellSize.y;
        float spacingHeight = gridLayout.spacing.y;

        // 콘텐츠 전체 높이 계산 후 적용
        float contentHeight = (cellHeight + spacingHeight) * slotCount - spacingHeight;

        // 콘텐츠 크기와 ScrollRect 크기를 비교
        if (contentHeight > scrollRect.GetComponent<RectTransform>().rect.height)
        {
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            scrollRect.verticalNormalizedPosition = 1f;
        }
        else
        {
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollRect.GetComponent<RectTransform>().rect.height);
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    #endregion


    #region Sub Menus

    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)QuestMenuType.End; i++)
        {
            int index = i;
            subQuestMenuButtons[index].onClick.AddListener(() =>
            {
                ActivateMenu((QuestMenuType)index);

                if (index == (int)QuestMenuType.Repeat)
                {
                    InitializeRepeatQuestUIFromSortedList();
                }
            });
        }

        ActivateMenu(QuestMenuType.Daily);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(QuestMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainQuestMenus.Length; i++)
        {
            mainQuestMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subQuestMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subQuestMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // 서브 메뉴 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    #endregion


    #region New Image

    // New Image의 상태를 Update하는 함수
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedDailyRewards = HasUnclaimedDailyRewards();
        bool hasUnclaimedWeeklyRewards = HasUnclaimedWeeklyRewards();
        bool hasUnclaimedRepeatRewards = HasUnclaimedRepeatRewards();
        bool hasUnclaimedRewards = hasUnclaimedDailyRewards || hasUnclaimedWeeklyRewards || hasUnclaimedRepeatRewards;

        // Quest Button / Daily / Weekly / Repeat
        mainQuestButtonNewImage.SetActive(hasUnclaimedRewards);
        subQuestButtonNewImages[(int)QuestMenuType.Daily].SetActive(hasUnclaimedDailyRewards);
        subQuestButtonNewImages[(int)QuestMenuType.Weekly].SetActive(hasUnclaimedWeeklyRewards);
        subQuestButtonNewImages[(int)QuestMenuType.Repeat].SetActive(hasUnclaimedRepeatRewards);
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Daily)
    public bool HasUnclaimedDailyRewards()
    {
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable)
            {
                return true;
            }
        }
        return dailySpecialRewardButton.interactable;
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Weekly)
    public bool HasUnclaimedWeeklyRewards()
    {
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable)
            {
                return true;
            }
        }
        return weeklySpecialRewardButton.interactable;
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Repeat)
    public bool HasUnclaimedRepeatRewards()
    {
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                return true;
            }
        }
        return false;
    }

    #endregion


    #region Initialize and Update Quests

    // 퀘스트 초기화
    private void InitializeQuest(string questName, int targetCount, int rewardCash, QuestMenuType menuType, string imageName)
    {
        // Quest Slot 생성
        GameObject newQuestSlot = Instantiate(questSlotPrefab, questSlotParents[(int)menuType]);

        // QuestUI 매핑
        QuestUI questUI = new QuestUI
        {
            questName = newQuestSlot.transform.Find("Quest Name").GetComponent<TextMeshProUGUI>(),
            questSlider = newQuestSlot.transform.Find("Slider").GetComponent<Slider>(),
            countText = newQuestSlot.transform.Find("Slider/Count Text").GetComponent<TextMeshProUGUI>(),
            plusCashText = newQuestSlot.transform.Find("Plus Cash Text").GetComponent<TextMeshProUGUI>(),
            rewardButton = newQuestSlot.transform.Find("Reward Button").GetComponent<Button>(),
            rewardText = newQuestSlot.transform.Find("Reward Button/Reward Text").GetComponent<TextMeshProUGUI>(),
            rewardDisabledBG = newQuestSlot.transform.Find("Reward Button/DisabledBG").gameObject,
            slotTransform = newQuestSlot.transform,
            questImage = newQuestSlot.transform.Find("Icon").GetComponent<Image>(),

            questData = new QuestUI.QuestData
            {
                currentCount = 0,
                targetCount = targetCount,
                baseTargetCount = targetCount,
                rewardCount = 1,
                rewardCash = rewardCash,
                isComplete = false
            }
        };

        // UI 텍스트 초기화
        questUI.questName.text = questName;
        questUI.plusCashText.text = $"x {rewardCash}";
        questUI.rewardText.text = "받기";

        // 이미지 설정
        questUI.questImage.sprite = Resources.Load<Sprite>($"{resourcePath}{imageName}");

        // 보상 버튼 리스너 등록
        questUI.rewardButton.onClick.AddListener(() => ReceiveQuestReward(questName, menuType));

        // 퀘스트 데이터를 Dictionary에 추가 (메뉴 타입에 따라 다름)
        switch (menuType)
        {
            case QuestMenuType.Daily:
                dailyQuestDictionary[questName] = questUI;
                break;
            case QuestMenuType.Weekly:
                weeklyQuestDictionary[questName] = questUI;
                break;
            case QuestMenuType.Repeat:
                repeatQuestDictionary[questName] = questUI;
                break;
        }

        // UI 업데이트
        UpdateQuestUI(questName, menuType);
    }

    private void UpdateAllUI()
    {
        // 스페셜 보상 UI 업데이트
        UpdateDailySpecialRewardUI();
        UpdateWeeklySpecialRewardUI();

        // 보상 버튼 상태 업데이트
        UpdateAllDailyRewardButtonState();
        UpdateAllWeeklyRewardButtonState();
        UpdateAllRepeatRewardButtonState();

        // New 이미지 상태 업데이트
        UpdateNewImageStatus();

        SortRepeatQuests();
    }

    // 퀘스트 UI 업데이트
    private void UpdateQuestUI(string questName, QuestMenuType menuType)
    {
        // 메뉴 타입에 따라 해당 딕셔너리 선택
        QuestUI questUI = null;
        if (menuType == QuestMenuType.Daily && dailyQuestDictionary.ContainsKey(questName))
        {
            questUI = dailyQuestDictionary[questName];
        }
        else if (menuType == QuestMenuType.Weekly && weeklyQuestDictionary.ContainsKey(questName))
        {
            questUI = weeklyQuestDictionary[questName];
        }
        else if (menuType == QuestMenuType.Repeat && repeatQuestDictionary.ContainsKey(questName))
        {
            questUI = repeatQuestDictionary[questName];
        }

        // 퀘스트 UI가 없다면 종료
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // Slider 값 설정
        questUI.questSlider.maxValue = questData.targetCount;
        questUI.questSlider.value = Mathf.Min(questData.currentCount, questData.targetCount);

        // 텍스트 업데이트 (일일/주간 퀘스트는 targetCount를 넘지 않도록)
        int displayCount = questData.currentCount;
        if (menuType == QuestMenuType.Daily || menuType == QuestMenuType.Weekly)
        {
            displayCount = Mathf.Min(questData.currentCount, questData.targetCount);
        }
        questUI.countText.text = $"{displayCount} / {questData.targetCount}";

        // 완료 여부 확인
        bool isComplete = questData.currentCount >= questData.targetCount && !questData.isComplete;
        questUI.rewardButton.interactable = isComplete;
        questUI.rewardDisabledBG.SetActive(!isComplete);

        if (questData.isComplete)
        {
            questUI.rewardText.text = "수령 완료";
        }

        if (menuType == QuestMenuType.Daily)
        {
            UpdateAllDailyRewardButtonState();
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            UpdateAllWeeklyRewardButtonState();
        }
        else
        {
            UpdateAllRepeatRewardButtonState();
        }
        UpdateNewImageStatus();
    }

    // 퀘스트 진행 업데이트
    public void UpdateQuestProgress(string questName)
    {
        // 일일 퀘스트 업데이트
        if (dailyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = dailyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Daily);
        }
        // 주간 퀘스트 업데이트
        if (weeklyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = weeklyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Weekly);
        }
        // 반복 퀘스트 업데이트
        if (repeatQuestDictionary.ContainsKey(questName))
        {
            UpdateQuestUI(questName, QuestMenuType.Repeat);
            SortRepeatQuests();
        }
    }

    // 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveQuestReward(string questName, QuestMenuType menuType)
    {
        // 해당 메뉴 타입에 맞는 퀘스트 딕셔너리 선택
        QuestUI questUI = null;
        if (menuType == QuestMenuType.Daily && dailyQuestDictionary.ContainsKey(questName))
        {
            questUI = dailyQuestDictionary[questName];
        }
        else if (menuType == QuestMenuType.Weekly && weeklyQuestDictionary.ContainsKey(questName))
        {
            questUI = weeklyQuestDictionary[questName];
        }
        else if (menuType == QuestMenuType.Repeat && repeatQuestDictionary.ContainsKey(questName))
        {
            questUI = repeatQuestDictionary[questName];
        }

        // 퀘스트 UI가 없다면 종료
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // 보상 버튼이 비활성화되어 있거나 퀘스트가 이미 완료되었으면 종료
        if (!questUI.rewardButton.interactable || questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        if (menuType == QuestMenuType.Repeat)
        {
            ReceiveRepeatQuestReward(questName, questData.rewardCash);
        }
        else
        {
            ReceiveQuestReward(ref questData.isComplete, questData.rewardCash, questUI.rewardButton, questUI.rewardDisabledBG, menuType, questName);
        }
    }

    #endregion


    #region Quests

    #region PlayTime Quest

    // 플레이타임 증가 함수
    public void AddPlayTimeCount()
    {
        float addTime = 1f;

        // 일일 퀘스트 플레이타임
        if (dailyQuestDictionary["플레이 시간"].questData.currentCount < dailyQuestDictionary["플레이 시간"].questData.targetCount)
        {
            dailyPlayTimeCount += addTime;
            dailyQuestDictionary["플레이 시간"].questData.currentCount = (int)dailyPlayTimeCount;
        }

        // 주간 퀘스트 플레이타임
        if (weeklyQuestDictionary["플레이 시간"].questData.currentCount < weeklyQuestDictionary["플레이 시간"].questData.targetCount)
        {
            weeklyPlayTimeCount += addTime;
            weeklyQuestDictionary["플레이 시간"].questData.currentCount = (int)weeklyPlayTimeCount;
        }

        UpdateQuestProgress("플레이 시간");
    }

    // 플레이타임 리셋 함수
    public void ResetPlayTimeCount()
    {
        dailyPlayTimeCount = 0;
        weeklyPlayTimeCount = 0;
    }

    #endregion


    #region Merge Quest

    // 고양이 머지 증가 함수
    public void AddMergeCount()
    {
        dailyMergeCount++;
        weeklyMergeCount++;
        totalMergeCount++;

        dailyQuestDictionary["고양이 합성 횟수"].questData.currentCount = dailyMergeCount;
        weeklyQuestDictionary["고양이 합성 횟수"].questData.currentCount = weeklyMergeCount;
        repeatQuestDictionary["고양이 합성 횟수"].questData.currentCount = totalMergeCount;

        UpdateQuestProgress("고양이 합성 횟수");
    }

    // 고양이 머지 리셋 함수
    public void ResetMergeCount()
    {
        dailyMergeCount = 0;
        weeklyMergeCount = 0;
        totalMergeCount = 0;
    }

    #endregion


    #region Spawn Quest

    // 고양이 스폰 증가 함수
    public void AddSpawnCount()
    {
        dailySpawnCount++;
        weeklySpawnCount++;
        totalSpawnCount++;

        dailyQuestDictionary["고양이 소환 횟수"].questData.currentCount = dailySpawnCount;
        weeklyQuestDictionary["고양이 소환 횟수"].questData.currentCount = weeklySpawnCount;
        repeatQuestDictionary["고양이 소환 횟수"].questData.currentCount = totalSpawnCount;

        UpdateQuestProgress("고양이 소환 횟수");
    }

    // 고양이 스폰 리셋 함수
    public void ResetSpawnCount()
    {
        dailySpawnCount = 0;
        weeklySpawnCount = 0;
        totalSpawnCount = 0;
    }

    #endregion


    #region Purchase Cats Quest

    //// 고양이 구매 증가 함수
    //public void AddPurchaseCatsCount()
    //{
    //    totalPurchaseCount++;

    //    repeatQuestDictionary["고양이 구매 횟수"].questData.currentCount = totalPurchaseCount;

    //    UpdateQuestProgress("고양이 구매 횟수");
    //}

    //// 고양이 구매 리셋 함수
    //public void ResetPurchaseCatsCount()
    //{
    //    totalPurchaseCount = 0;
    //}

    #endregion


    #region Battle Count Quest

    // 배틀 증가 함수
    public void AddBattleCount()
    {
        dailyBattleCount++;
        weeklyBattleCount++;
        totalBattleCount++;

        dailyQuestDictionary["전투 횟수"].questData.currentCount = dailyBattleCount;
        weeklyQuestDictionary["전투 횟수"].questData.currentCount = weeklyBattleCount;
        repeatQuestDictionary["전투 횟수"].questData.currentCount = totalBattleCount;

        UpdateQuestProgress("전투 횟수");
    }

    // 배틀 리셋 함수
    public void ResetBattleCount()
    {
        dailyBattleCount = 0;
        weeklyBattleCount = 0;
        totalBattleCount = 0;
    }

    #endregion

    #endregion


    #region Special Reward Quest - Daily, Weekly

    // [Special Reward Quest - Daily]
    // Daily Special Reward Quest 초기 설정 함수
    private void InitializeDailySpecialReward()
    {
        dailySpecialRewardTargetCount = dailyQuestDictionary.Count;

        dailySpecialRewardButton.onClick.AddListener(ReceiveDailySpecialReward);
        isDailySpecialRewardQuestComplete = false;
        dailySpecialRewardButton.interactable = false;
        dailySpecialRewardDisabledBG.SetActive(true);
        dailySpecialRewardPlusCashText.text = $"x {dailySpecialRewardQuestRewardCash}";
        dailySpecialRewardText.text = "받기";

        UpdateDailySpecialRewardUI();
    }

    // Daily Special Reward 퀘스트 UI를 업데이트하는 함수
    private void UpdateDailySpecialRewardUI()
    {
        int currentCount = Mathf.Min((int)DailySpecialRewardCount, dailySpecialRewardTargetCount);

        // Slider 값 설정
        dailySpecialRewardSlider.maxValue = dailySpecialRewardTargetCount;
        dailySpecialRewardSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        dailySpecialRewardCountText.text = $"{currentCount} / {dailySpecialRewardTargetCount}";
        if (isDailySpecialRewardQuestComplete)
        {
            dailySpecialRewardText.text = "수령 완료";
        }

        bool isComplete = AllDailyQuestsCompleted() && !isDailySpecialRewardQuestComplete;
        dailySpecialRewardButton.interactable = isComplete;
        dailySpecialRewardDisabledBG.SetActive(!isComplete);
    }

    // Daily Special Reward 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveDailySpecialReward()
    {
        if (!dailySpecialRewardButton.interactable || isDailySpecialRewardQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        AddCash(dailySpecialRewardQuestRewardCash);
        isDailySpecialRewardQuestComplete = true;

        UpdateDailySpecialRewardUI();
        UpdateAllDailyRewardButtonState();
        UpdateNewImageStatus();
    }

    // 모든 Daily 퀘스트가 완료되었는지 확인하는 함수
    private bool AllDailyQuestsCompleted()
    {
        return dailyQuestDictionary.Values.All(quest => quest.questData.isComplete);
    }

    // Daily Special Reward 증가 함수
    public void AddDailySpecialRewardCount()
    {
        DailySpecialRewardCount++;
    }

    // Daily Special Reward 리셋 함수           - 나중에 정해진 시간에 초기화되게 하기 위해
    public void ResetDailySpecialRewardCount()
    {
        DailySpecialRewardCount = 0;
    }


    // [Special Reward Quest - Weekly]
    // Weekly Special Reward Quest 초기 설정 함수
    private void InitializeWeeklySpecialReward()
    {
        weeklySpecialRewardTargetCount = weeklyQuestDictionary.Count;

        weeklySpecialRewardButton.onClick.AddListener(ReceiveWeeklySpecialReward);
        isWeeklySpecialRewardQuestComplete = false;
        weeklySpecialRewardButton.interactable = false;
        weeklySpecialRewardDisabledBG.SetActive(true);
        weeklySpecialRewardPlusCashText.text = $"x {weeklySpecialRewardQuestRewardCash}";
        weeklySpecialRewardText.text = "받기";
        UpdateWeeklySpecialRewardUI();
    }

    // Weekly Special Reward 퀘스트 UI를 업데이트하는 함수
    private void UpdateWeeklySpecialRewardUI()
    {
        int currentCount = Mathf.Min((int)WeeklySpecialRewardCount, weeklySpecialRewardTargetCount);

        // Slider 값 설정
        weeklySpecialRewardSlider.maxValue = weeklySpecialRewardTargetCount;
        weeklySpecialRewardSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        weeklySpecialRewardCountText.text = $"{currentCount} / {weeklySpecialRewardTargetCount}";
        if (isWeeklySpecialRewardQuestComplete)
        {
            weeklySpecialRewardText.text = "수령 완료";
        }

        bool isComplete = AllWeeklyQuestsCompleted() && !isWeeklySpecialRewardQuestComplete;
        weeklySpecialRewardButton.interactable = isComplete;
        weeklySpecialRewardDisabledBG.SetActive(!isComplete);
    }

    // Weekly Special Reward 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveWeeklySpecialReward()
    {
        if (!weeklySpecialRewardButton.interactable || isWeeklySpecialRewardQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        AddCash(weeklySpecialRewardQuestRewardCash);
        isWeeklySpecialRewardQuestComplete = true;

        UpdateWeeklySpecialRewardUI();
        UpdateAllWeeklyRewardButtonState();
        UpdateNewImageStatus();
    }

    // 모든 Weekly 퀘스트가 완료되었는지 확인하는 함수
    private bool AllWeeklyQuestsCompleted()
    {
        return weeklyQuestDictionary.Values.All(quest => quest.questData.isComplete);
    }

    // Weekly Special Reward 증가 함수
    public void AddWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount++;
    }

    // Weekly Special Reward 리셋 함수          - 나중에 정해진 시간에 초기화되게 하기 위해
    public void ResetWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount = 0;
    }

    #endregion


    #region 전체 보상 받기 - AllReward

    // 모든 활성화된 보상을 지급하는 함수 - Daily
    private void ReceiveAllDailyRewards()
    {
        // 스페셜 보상 활성화 상태일시 지급
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            ReceiveDailySpecialReward();
        }

        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref dailyQuest.Value.questData.isComplete, dailyQuest.Value.questData.rewardCash,
                    dailyQuest.Value.rewardButton, dailyQuest.Value.rewardDisabledBG, QuestMenuType.Daily, dailyQuest.Key);
            }
        }

        UpdateNewImageStatus();
    }

    // All Reward 버튼 상태를 업데이트하는 함수 - Daily
    private void UpdateAllDailyRewardButtonState()
    {
        // 보상을 받을 수 있는 버튼이 하나라도 활성화되어 있는지 확인
        bool isAnyRewardAvailable = false;

        // 스페셜 보상 확인
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        // 일반 퀘스트 확인
        if (!isAnyRewardAvailable)
        {
            foreach (var dailyQuest in dailyQuestDictionary)
            {
                if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
                {
                    isAnyRewardAvailable = true;
                    break;
                }
            }
        }

        allRewardButtons[(int)QuestMenuType.Daily].interactable = isAnyRewardAvailable;
    }

    // 모든 활성화된 보상을 지급하는 함수 - Weekly
    private void ReceiveAllWeeklyRewards()
    {
        // 스페셜 보상 활성화 상태일시 지급
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            ReceiveWeeklySpecialReward();
        }

        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref weeklyQuest.Value.questData.isComplete, weeklyQuest.Value.questData.rewardCash,
                    weeklyQuest.Value.rewardButton, weeklyQuest.Value.rewardDisabledBG, QuestMenuType.Weekly, weeklyQuest.Key);
            }
        }

        UpdateNewImageStatus();
    }

    // All Reward 버튼 상태를 업데이트하는 함수 - Weekly
    private void UpdateAllWeeklyRewardButtonState()
    {
        // 보상을 받을 수 있는 버튼이 하나라도 활성화되어 있는지 확인
        bool isAnyRewardAvailable = false;

        // 스페셜 보상 확인
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        // 일반 퀘스트 확인
        if (!isAnyRewardAvailable)
        {
            foreach (var weeklyQuest in weeklyQuestDictionary)
            {
                if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
                {
                    isAnyRewardAvailable = true;
                    break;
                }
            }
        }

        allRewardButtons[(int)QuestMenuType.Weekly].interactable = isAnyRewardAvailable;
    }

    // 모든 활성화된 보상을 지급하는 함수 - Repeat
    private void ReceiveAllRepeatRewards()
    {
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                ReceiveRepeatQuestReward(repeatQuest.Key, repeatQuest.Value.questData.rewardCash);
            }
        }
    }

    // All Reward 버튼 상태를 업데이트하는 함수 - Repeat
    private void UpdateAllRepeatRewardButtonState()
    {
        // 보상을 받을 수 있는 버튼이 하나라도 활성화되어 있는지 확인
        bool isAnyRewardAvailable = false;
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                isAnyRewardAvailable = true;
                break;
            }
        }

        allRewardButtons[(int)QuestMenuType.Repeat].interactable = isAnyRewardAvailable;
        UpdateNewImageStatus();
    }


    // 개별 퀘스트 보상 지급 처리 함수 - Daily, Weekly
    private void ReceiveQuestReward(ref bool isQuestComplete, int rewardCash, Button rewardButton, GameObject disabledBG, QuestMenuType menuType, string questName)
    {
        isQuestComplete = true;
        AddCash(rewardCash);
        rewardButton.interactable = false;
        disabledBG.SetActive(true);

        // QuestType에 따라 특수 보상 카운트 추가
        if (menuType == QuestMenuType.Daily)
        {
            AddDailySpecialRewardCount();
            UpdateDailySpecialRewardUI();
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
            UpdateWeeklySpecialRewardUI();
        }

        // 퀘스트 UI 업데이트 호출
        UpdateQuestUI(questName, menuType);
    }

    // 개별 퀘스트 보상 지급 처리 함수 - Repeat
    private void ReceiveRepeatQuestReward(string questName, int rewardCash)
    {
        var questData = repeatQuestDictionary[questName].questData;

        questData.rewardCount++;

        int additionalMultiplier = (questData.rewardCount - 1) / 10;                    // 0부터 시작하여 10회마다 1씩 증가
        int currentIncrease = questData.baseTargetCount * (additionalMultiplier + 1);   // 기본값 + 추가 증가량
        questData.targetCount += currentIncrease;

        AddCash(rewardCash);

        UpdateQuestUI(questName, QuestMenuType.Repeat);
        SortRepeatQuests();
    }

    #endregion


    #region 추가 기능

    // 캐쉬 추가 함수
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    #endregion


    #region Repeat Quest Sort

    // 반복 퀘스트 정렬 함수
    private void SortRepeatQuests()
    {
        // Dictionary 값을 List로 변환
        var sortedQuests = repeatQuestDictionary.Values.ToList();

        // 정렬
        sortedQuests.Sort((a, b) =>
        {
            // 보상 버튼이 활성화된 퀘스트가 상단에 오도록 정렬
            if (a.rewardButton.interactable && !b.rewardButton.interactable) return -1;
            if (!a.rewardButton.interactable && b.rewardButton.interactable) return 1;

            // 보상 버튼이 동일하게 활성화된 경우, 활성화된 순서로 정렬
            if (a.rewardButton.interactable && b.rewardButton.interactable)
            {
                return a.slotTransform.GetSiblingIndex() - b.slotTransform.GetSiblingIndex();
            }

            return 0;
        });

        // 정렬된 순서로 List 저장
        sortedRepeatQuestList = new List<QuestUI>(sortedQuests);

        // 정렬된 순서로 슬롯 UI의 부모 내 위치 갱신
        Transform parentTransform = questSlotParents[(int)QuestMenuType.Repeat];

        // 슬롯들을 나누기: 보상 버튼이 활성화된 퀘스트와 그렇지 않은 퀘스트로
        List<QuestUI> rewardAvailableQuests = new List<QuestUI>();
        List<QuestUI> rewardUnavailableQuests = new List<QuestUI>();

        // 활성화된 보상 버튼이 있는 퀘스트들을 먼저 상단에 배치
        foreach (var quest in sortedQuests)
        {
            if (quest.rewardButton.interactable)
            {
                rewardAvailableQuests.Add(quest);
            }
            else
            {
                rewardUnavailableQuests.Add(quest);
            }
        }

        // 활성화된 보상 버튼이 있는 퀘스트들 상단에 배치
        int siblingIndex = 0;

        // 보상 버튼 활성화된 퀘스트들 먼저 배치
        foreach (var quest in rewardAvailableQuests)
        {
            Transform slotTransform = quest.slotTransform;

            if (slotTransform.parent != parentTransform)
            {
                slotTransform.SetParent(parentTransform);
            }

            // 애니메이션 적용
            AnimateSlotPosition(slotTransform, siblingIndex++);
        }

        // 보상 버튼이 활성화되지 않은 퀘스트들 그 아래에 배치
        foreach (var quest in rewardUnavailableQuests)
        {
            Transform slotTransform = quest.slotTransform;

            if (slotTransform.parent != parentTransform)
            {
                slotTransform.SetParent(parentTransform);
            }

            // 애니메이션 적용
            AnimateSlotPosition(slotTransform, siblingIndex++);
        }
    }

    // 슬롯 이동 애니메이션 처리 함수
    private void AnimateSlotPosition(Transform slotTransform, int targetSiblingIndex)
    {
        // 목표 위치 설정
        Vector3 targetPosition = GetSlotPosition(targetSiblingIndex);

        // 현재 위치에서 애니메이션 시작
        Coroutine animation = StartCoroutine(SlotMoveAnimation(slotTransform, targetPosition));
        activeAnimations[slotTransform] = animation;
    }

    // 슬롯의 목표 위치 계산 함수
    private Vector3 GetSlotPosition(int siblingIndex)
    {
        ScrollRect scrollRect = mainQuestMenus[(int)QuestMenuType.Repeat].GetComponentInChildren<ScrollRect>();
        RectTransform content = scrollRect.content;
        GridLayoutGroup gridLayout = content.GetComponent<GridLayoutGroup>();

        // 슬롯의 갯수와 각 슬롯의 셀 크기와 스페이싱 변수
        int slotCount = content.childCount;
        float cellHeight = gridLayout.cellSize.y;
        float spacingHeight = gridLayout.spacing.y;

        // 목표 위치 계산: siblingIndex에 해당하는 슬롯의 y 좌표
        int halfSlotCount = slotCount / 2;
        float targetYPosition;
        if (slotCount % 2 == 0)
        {
            targetYPosition = (halfSlotCount - siblingIndex - 1) * (cellHeight + spacingHeight) + (cellHeight / 2 + spacingHeight / 2);
        }
        else
        {
            targetYPosition = (halfSlotCount - siblingIndex) * (cellHeight + spacingHeight);
        }

        return new Vector3(0, targetYPosition, 0);
    }

    // 슬롯 이동 애니메이션 코루틴
    private IEnumerator SlotMoveAnimation(Transform slotTransform, Vector3 targetPosition)
    {
        const float animationDuration = 0.2f; // 애니메이션 지속 시간
        float elapsedTime = 0f;

        // 시작 위치 저장
        Vector3 startPosition = slotTransform.localPosition;

        // 애니메이션 실행
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float time = Mathf.Clamp01(elapsedTime / animationDuration);

            slotTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, time);

            yield return null;
        }

        // 애니메이션 완료 후 정확한 위치로 설정
        slotTransform.localPosition = targetPosition;

        // 슬롯 애니메이션 상태 제거
        activeAnimations.Remove(slotTransform);
    }

    // 정렬된 퀘스트 리스트 설정 함수
    private void InitializeRepeatQuestUIFromSortedList()
    {
        Transform parentTransform = questSlotParents[(int)QuestMenuType.Repeat];

        foreach (var quest in sortedRepeatQuestList)
        {
            Transform slotTransform = quest.slotTransform;
            slotTransform.SetParent(parentTransform);

            int index = sortedRepeatQuestList.IndexOf(quest);
            slotTransform.SetSiblingIndex(index);
        }
    }

    #endregion


    #region Time Reset System

    private void CheckAndResetQuests()
    {
#if UNITY_EDITOR
        long currentTime = useDebugTime ? debugCurrentTime.ToUnixTimeSeconds() : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DateTimeOffset currentUtc = DateTimeOffset.FromUnixTimeSeconds(currentTime);
#else
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DateTimeOffset currentUtc = DateTimeOffset.FromUnixTimeSeconds(currentTime);
#endif
        DateTimeOffset lastDailyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastDailyReset);
        DateTimeOffset lastWeeklyResetTime = DateTimeOffset.FromUnixTimeSeconds(lastWeeklyReset);

        // 일일 퀘스트 초기화 체크
        if (currentUtc.Date > lastDailyResetTime.Date)
        {
            //Debug.Log($"[Quest] 일일 퀘스트 초기화 - 현재: {currentUtc}, 마지막 리셋: {lastDailyResetTime}");
            ResetDailyQuests();
            lastDailyReset = currentTime;
        }

        // 주간 퀘스트 초기화 체크 (UTC 기준 목요일)
        DateTimeOffset nextThursday = GetNextThursday(lastWeeklyResetTime);
        if (currentUtc.Date >= nextThursday.Date && lastWeeklyResetTime.Date < nextThursday.Date)
        {
            //Debug.Log($"[Quest] 주간 퀘스트 초기화 - 현재: {currentUtc}, 마지막 리셋: {lastWeeklyResetTime}, 다음 목요일: {nextThursday}");
            ResetWeeklyQuests();
            lastWeeklyReset = currentTime;
        }
    }

    // 다음 목요일 자정 시간을 계산하는 함수
    private DateTimeOffset GetNextThursday(DateTimeOffset fromTime)
    {
        // 목요일 자정을 기준으로 계산
        DateTimeOffset thursdayMidnight = fromTime.Date;

        if (fromTime.DayOfWeek == DayOfWeek.Thursday)
        {
            // 목요일이면서 자정이 지난 경우
            if (fromTime.TimeOfDay > TimeSpan.Zero)
            {
                return thursdayMidnight.AddDays(7);
            }
            // 목요일이지만 자정인 경우
            return thursdayMidnight;
        }

        // 다음 목요일까지 남은 일수 계산
        int daysUntilThursday = ((int)DayOfWeek.Thursday - (int)fromTime.DayOfWeek + 7) % 7;

        return thursdayMidnight.AddDays(daysUntilThursday);
    }

    // 일일 퀘스트 초기화 함수
    private void ResetDailyQuests()
    {
        // 일일 퀘스트 초기화
        foreach (var quest in dailyQuestDictionary)
        {
            quest.Value.questData.currentCount = 0;
            quest.Value.questData.isComplete = false;
            quest.Value.rewardText.text = "받기";
            UpdateQuestUI(quest.Key, QuestMenuType.Daily);
        }

        // 일일 특별 보상 초기화
        isDailySpecialRewardQuestComplete = false;
        dailySpecialRewardCount = 0;
        UpdateDailySpecialRewardUI();

        // 일일 카운터만 초기화
        dailyPlayTimeCount = 0;
        dailyMergeCount = 0;
        dailySpawnCount = 0;
        dailyBattleCount = 0;

        // UI 업데이트
        UpdateAllDailyRewardButtonState();
        UpdateNewImageStatus();
    }

    // 주간퀘스트 초기화 함수
    private void ResetWeeklyQuests()
    {
        // 주간 퀘스트 초기화
        foreach (var quest in weeklyQuestDictionary)
        {
            quest.Value.questData.currentCount = 0;
            quest.Value.questData.isComplete = false;
            quest.Value.rewardText.text = "받기";
            UpdateQuestUI(quest.Key, QuestMenuType.Weekly);
        }

        // 주간 특별 보상 초기화
        isWeeklySpecialRewardQuestComplete = false;
        weeklySpecialRewardCount = 0;
        UpdateWeeklySpecialRewardUI();

        // 주간 카운터만 초기화
        weeklyPlayTimeCount = 0;
        weeklyMergeCount = 0;
        weeklySpawnCount = 0;
        weeklyBattleCount = 0;

        // UI 업데이트
        UpdateAllWeeklyRewardButtonState();
        UpdateNewImageStatus();
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        // 리셋 시간 (Unix timestamp)
        public long lastDailyReset;     // 마지막 일일 퀘스트 리셋 시간
        public long lastWeeklyReset;    // 마지막 주간 퀘스트 리셋 시간

        // 일일 퀘스트 카운터
        public float dailyPlayTimeCount;
        public int dailyMergeCount;
        public int dailySpawnCount;
        public int dailyBattleCount;

        // 주간 퀘스트 카운터
        public float weeklyPlayTimeCount;
        public int weeklyMergeCount;
        public int weeklySpawnCount;
        public int weeklyBattleCount;

        // 반복 퀘스트 카운터
        public int totalMergeCount;
        public int totalSpawnCount;
        //public int totalPurchaseCount;
        public int totalBattleCount;

        // 스페셜 보상 데이터
        public bool isDailySpecialRewardQuestComplete;
        public bool isWeeklySpecialRewardQuestComplete;
        public int dailySpecialRewardCount;
        public int weeklySpecialRewardCount;

        // 퀘스트 데이터를 저장할 구조체
        [Serializable]
        public struct QuestSaveData
        {
            public int currentCount;
            public int targetCount;
            public int baseTargetCount;
            public int rewardCount;
            public bool isComplete;
        }

        // Dictionary를 직렬화 가능한 형태로 저장
        public List<string> dailyQuestKeys = new List<string>();
        public List<QuestSaveData> dailyQuestValues = new List<QuestSaveData>();

        public List<string> weeklyQuestKeys = new List<string>();
        public List<QuestSaveData> weeklyQuestValues = new List<QuestSaveData>();

        public List<string> repeatQuestKeys = new List<string>();
        public List<QuestSaveData> repeatQuestValues = new List<QuestSaveData>();
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            // 리셋 시간
            lastDailyReset = this.lastDailyReset,
            lastWeeklyReset = this.lastWeeklyReset,

            // 일일 퀘스트 카운터
            dailyPlayTimeCount = this.dailyPlayTimeCount,
            dailyMergeCount = this.dailyMergeCount,
            dailySpawnCount = this.dailySpawnCount,
            dailyBattleCount = this.dailyBattleCount,

            // 주간 퀘스트 카운터
            weeklyPlayTimeCount = this.weeklyPlayTimeCount,
            weeklyMergeCount = this.weeklyMergeCount,
            weeklySpawnCount = this.weeklySpawnCount,
            weeklyBattleCount = this.weeklyBattleCount,

            // 반복 퀘스트 카운터
            totalMergeCount = this.totalMergeCount,
            totalSpawnCount = this.totalSpawnCount,
            //totalPurchaseCount = this.totalPurchaseCount,
            totalBattleCount = this.totalBattleCount,

            // 스페셜 보상 데이터
            isDailySpecialRewardQuestComplete = this.isDailySpecialRewardQuestComplete,
            isWeeklySpecialRewardQuestComplete = this.isWeeklySpecialRewardQuestComplete,
            dailySpecialRewardCount = this.dailySpecialRewardCount,
            weeklySpecialRewardCount = this.weeklySpecialRewardCount
        };

        // 일일 퀘스트 데이터 저장
        foreach (var quest in dailyQuestDictionary)
        {
            data.dailyQuestKeys.Add(quest.Key);
            data.dailyQuestValues.Add(new SaveData.QuestSaveData
            {
                currentCount = quest.Value.questData.currentCount,
                targetCount = quest.Value.questData.targetCount,
                isComplete = quest.Value.questData.isComplete
            });
        }

        // 주간 퀘스트 데이터 저장
        foreach (var quest in weeklyQuestDictionary)
        {
            data.weeklyQuestKeys.Add(quest.Key);
            data.weeklyQuestValues.Add(new SaveData.QuestSaveData
            {
                currentCount = quest.Value.questData.currentCount,
                targetCount = quest.Value.questData.targetCount,
                isComplete = quest.Value.questData.isComplete
            });
        }

        // 반복 퀘스트 데이터 저장
        foreach (var quest in repeatQuestDictionary)
        {
            data.repeatQuestKeys.Add(quest.Key);
            data.repeatQuestValues.Add(new SaveData.QuestSaveData
            {
                currentCount = quest.Value.questData.currentCount,
                targetCount = quest.Value.questData.targetCount,
                baseTargetCount = quest.Value.questData.baseTargetCount,
                rewardCount = quest.Value.questData.rewardCount,
                isComplete = quest.Value.questData.isComplete
            });
        }

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            // 초기 데이터 설정
            lastDailyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastWeeklyReset = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return;
        }

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // 리셋 시간 복원
        lastDailyReset = savedData.lastDailyReset;
        lastWeeklyReset = savedData.lastWeeklyReset;

        // 퀘스트 카운터 복원
        dailyPlayTimeCount = savedData.dailyPlayTimeCount;
        dailyMergeCount = savedData.dailyMergeCount;
        dailySpawnCount = savedData.dailySpawnCount;
        dailyBattleCount = savedData.dailyBattleCount;

        weeklyPlayTimeCount = savedData.weeklyPlayTimeCount;
        weeklyMergeCount = savedData.weeklyMergeCount;
        weeklySpawnCount = savedData.weeklySpawnCount;
        weeklyBattleCount = savedData.weeklyBattleCount;

        totalMergeCount = savedData.totalMergeCount;
        totalSpawnCount = savedData.totalSpawnCount;
        //totalPurchaseCount = savedData.totalPurchaseCount;
        totalBattleCount = savedData.totalBattleCount;

        // 스페셜 보상 데이터 복원
        isDailySpecialRewardQuestComplete = savedData.isDailySpecialRewardQuestComplete;
        isWeeklySpecialRewardQuestComplete = savedData.isWeeklySpecialRewardQuestComplete;
        dailySpecialRewardCount = savedData.dailySpecialRewardCount;
        weeklySpecialRewardCount = savedData.weeklySpecialRewardCount;

        // 퀘스트 데이터 복원
        LoadQuestData(savedData);

        CheckAndResetQuestsOnStart();

        UpdateAllUI();

        isDataLoaded = true;
    }

    private void LoadQuestData(SaveData savedData)
    {
        // 퀘스트 데이터 복원을 위한 공통 함수
        void RestoreQuestData(Dictionary<string, QuestUI> questDict, List<string> keys, List<SaveData.QuestSaveData> values, QuestMenuType menuType)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                string questKey = keys[i];
                if (questDict.ContainsKey(questKey))
                {
                    var questData = questDict[questKey].questData;
                    var savedQuestData = values[i];

                    // 현재 카운트 업데이트 (퀘스트 타입에 따라 다른 값 사용)
                    switch (menuType)
                    {
                        case QuestMenuType.Daily:
                            questData.currentCount = questKey switch
                            {
                                "플레이 시간" => (int)dailyPlayTimeCount,
                                "고양이 합성 횟수" => dailyMergeCount,
                                "고양이 소환 횟수" => dailySpawnCount,
                                "전투 횟수" => dailyBattleCount,
                                _ => savedQuestData.currentCount
                            };
                            break;
                        case QuestMenuType.Weekly:
                            questData.currentCount = questKey switch
                            {
                                "플레이 시간" => (int)weeklyPlayTimeCount,
                                "고양이 합성 횟수" => weeklyMergeCount,
                                "고양이 소환 횟수" => weeklySpawnCount,
                                "전투 횟수" => weeklyBattleCount,
                                _ => savedQuestData.currentCount
                            };
                            break;
                        case QuestMenuType.Repeat:
                            questData.currentCount = questKey switch
                            {
                                "고양이 합성 횟수" => totalMergeCount,
                                "고양이 소환 횟수" => totalSpawnCount,
                                "전투 횟수" => totalBattleCount,
                                _ => savedQuestData.currentCount
                            };
                            break;
                    }

                    // 반복 퀘스트의 경우 목표값도 복원
                    if (menuType == QuestMenuType.Repeat)
                    {
                        questData.targetCount = savedQuestData.targetCount;
                        questData.baseTargetCount = savedQuestData.baseTargetCount;
                        questData.rewardCount = savedQuestData.rewardCount;
                    }
                    else
                    {
                        questData.isComplete = savedQuestData.isComplete;
                        if (questData.isComplete)
                        {
                            questDict[questKey].rewardText.text = "수령 완료";
                            questDict[questKey].rewardButton.interactable = false;
                            questDict[questKey].rewardDisabledBG.SetActive(true);
                        }
                    }

                    // UI 초기화
                    //questDict[questKey].questSlider.maxValue = questData.targetCount;
                    //questDict[questKey].questSlider.value = Mathf.Min(questData.currentCount, questData.targetCount);
                    //questDict[questKey].countText.text = $"{Mathf.Min(questData.currentCount, questData.targetCount)} / {questData.targetCount}";
                    UpdateQuestUI(questKey, menuType);
                }
            }

            //// UI 업데이트
            //foreach (var quest in questDict)
            //{
            //    UpdateQuestUI(quest.Key, menuType);
            //}
        }

        //// 현재 카운트 값을 가져오는 함수 (일일)
        //int GetCurrentCount(string questKey, int savedCount)
        //{
        //    //switch (questKey)
        //    //{
        //    //    case "플레이 시간":
        //    //        return (int)dailyPlayTimeCount;
        //    //    case "고양이 합성 횟수":
        //    //        return menuType == QuestMenuType.Daily ? dailyMergeCount :
        //    //               menuType == QuestMenuType.Weekly ? weeklyMergeCount : totalMergeCount;
        //    //    case "고양이 소환 횟수":
        //    //        return menuType == QuestMenuType.Daily ? dailySpawnCount :
        //    //               menuType == QuestMenuType.Weekly ? weeklySpawnCount : totalSpawnCount;
        //    //    case "전투 횟수":
        //    //        return menuType == QuestMenuType.Daily ? dailyBattleCount :
        //    //               menuType == QuestMenuType.Weekly ? weeklyBattleCount : totalBattleCount;
        //    //    default:
        //    //        return savedCount;
        //    //}
        //    return questKey switch
        //    {
        //        "플레이 시간" => (int)dailyPlayTimeCount,
        //        "고양이 합성 횟수" => dailyMergeCount,
        //        "고양이 소환 횟수" => dailySpawnCount,
        //        "전투 횟수" => dailyBattleCount,
        //        _ => savedCount
        //    };
        //}

        // 각 퀘스트 타입별 데이터 복원
        RestoreQuestData(dailyQuestDictionary, savedData.dailyQuestKeys, savedData.dailyQuestValues, QuestMenuType.Daily);
        RestoreQuestData(weeklyQuestDictionary, savedData.weeklyQuestKeys, savedData.weeklyQuestValues, QuestMenuType.Weekly);
        RestoreQuestData(repeatQuestDictionary, savedData.repeatQuestKeys, savedData.repeatQuestValues, QuestMenuType.Repeat);

        // 스페셜 보상 UI 초기화
        UpdateDailySpecialRewardUI();
        UpdateWeeklySpecialRewardUI();

        //// 반복 퀘스트 정렬 및 UI 업데이트
        //SortRepeatQuests();
        //InitializeRepeatQuestUIFromSortedList();
    }

    #endregion


    #region Unity Editor Test

#if UNITY_EDITOR
    [Header("---[Debug Time Control]")]
    [SerializeField] private bool useDebugTime = false;
    [SerializeField] private Button skipDayButton;         // 다음날 스킵 버튼
    [SerializeField] private Button skipWeekButton;        // 다음 주 목요일 스킵 버튼
    private DateTimeOffset debugCurrentTime;

    // 디버그 시간 설정 함수
    public void SetDebugTime(int addDays = 0, int addHours = 0)
    {
        if (!useDebugTime) return;

        debugCurrentTime = DateTimeOffset.UtcNow.AddDays(addDays).AddHours(addHours);
        CheckAndResetQuests();
    }

    private void InitializeDebugControls()
    {
        if (skipDayButton != null)
        {
            skipDayButton.onClick.AddListener(SkipToNextDay);
        }

        if (skipWeekButton != null)
        {
            skipWeekButton.onClick.AddListener(SkipToNextThursday);
        }
    }

    // 디버그용 다음날로 이동
    public void SkipToNextDay()
    {
        if (!useDebugTime) return;
        //Debug.Log("다음날 버튼");

        SetDebugTime(1);
    }

    // 디버그용 다음 주 목요일로 이동
    public void SkipToNextThursday()
    {
        if (!useDebugTime) return;

        //Debug.Log("다음주 버튼");

        DateTimeOffset current = useDebugTime ? debugCurrentTime : DateTimeOffset.UtcNow;
        int daysUntilThursday = ((int)DayOfWeek.Thursday - (int)current.DayOfWeek + 7) % 7;
        if (daysUntilThursday == 0) daysUntilThursday = 7;

        // 다음 목요일로 시간 설정
        debugCurrentTime = current.AddDays(daysUntilThursday);

        // 마지막 주간 리셋 시간을 현재 시간보다 이전으로 설정하여 강제로 초기화 트리거
        lastWeeklyReset = debugCurrentTime.AddDays(-7).ToUnixTimeSeconds();

        // 퀘스트 체크 및 초기화 실행
        CheckAndResetQuests();
    }
#endif

    #endregion


}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

// Quest Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
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

        public class QuestData
        {
            public int currentCount;                // 현재 수치
            public int targetCount;                 // 목표 수치
            public int plusTargetCount;             // 목표 수치 증가 수치
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
    private int weeklySpecialRewardQuestRewardCash = 5000;                      // Weekly Special Reward 퀘스트 보상 캐쉬 재화 개수
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

    private float playTimeCount;                                        // 플레이타임 카운트
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int mergeCount;                                             // 고양이 머지 횟수
    public int MergeCount { get => mergeCount; set => mergeCount = value; }

    private int spawnCount;                                             // 고양이 스폰 횟수(먹이 준 횟수)
    public int SpawnCount { get => spawnCount; set => spawnCount = value; }

    private int purchaseCatsCount;                                      // 고양이 구매 횟수
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    private int battleCount;                                            // 전투 횟수
    public int BattleCount { get => battleCount; set => battleCount = value; }

    private int stageCount;                                             // 스테이지 단계
    public int StageCount { get => BattleManager.Instance.BossStage; }


    private int dailySpecialRewardCount;                                // Daily 최종 퀘스트 진행 횟수
    public int DailySpecialRewardCount { get => dailySpecialRewardCount; set => dailySpecialRewardCount = value; }

    private int weeklySpecialRewardCount;                               // Weekly 최종 퀘스트 진행 횟수
    public int WeeklySpecialRewardCount { get => weeklySpecialRewardCount; set => weeklySpecialRewardCount = value; }

    // ======================================================================================================================

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
        questMenuPanel.SetActive(false);
        mainQuestButtonNewImage.SetActive(false);
        activeMenuType = QuestMenuType.Daily;

        InitializeQuestManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("QuestMenu", questMenuPanel, questButtonImage);
    }

    private void Update()
    {
        AddPlayTimeCount();
    }

    // ======================================================================================================================
    // [Initialize]

    // 모든 QuestManager 시작 함수들 모음
    private void InitializeQuestManager()
    {
        InitializeQuestButton();
        InitializeSubMenuButtons();

        InitializeDailyQuestManager();
        InitializeWeeklyQuestManager();
        InitializeRepeatQuestManager();
    }

    // QuestButton 설정 함수
    private void InitializeQuestButton()
    {
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
        InitializeQuest("플레이 시간", 10, 5, QuestMenuType.Daily);
        InitializeQuest("고양이 머지 횟수", 1, 5, QuestMenuType.Daily);
        InitializeQuest("고양이 소환 횟수", 1, 5, QuestMenuType.Daily);
        InitializeQuest("고양이 구매 횟수", 1, 5, QuestMenuType.Daily);
        InitializeQuest("전투 횟수", 1, 5, QuestMenuType.Daily);

        InitializeDailySpecialReward();

        allRewardButtons[(int)QuestMenuType.Daily].onClick.AddListener(ReceiveAllDailyRewards);
        UpdateAllDailyRewardButtonState();
    }

    // Weekly Quest 설정 함수
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("플레이 시간", 20, 50, QuestMenuType.Weekly);
        InitializeQuest("고양이 머지 횟수", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("고양이 소환 횟수", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("고양이 구매 횟수", 10, 50, QuestMenuType.Weekly);

        InitializeWeeklySpecialReward();

        allRewardButtons[(int)QuestMenuType.Weekly].onClick.AddListener(ReceiveAllWeeklyRewards);
        UpdateAllWeeklyRewardButtonState();
    }

    // Repeat Quest 설정 함수
    private void InitializeRepeatQuestManager()
    {
        InitializeQuest("고양이 머지 횟수", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("고양이 소환 횟수", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("고양이 구매 횟수", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("보스 스테이지", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("샘플1", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("샘플2", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("샘플3", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("샘플4", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("샘플5", 1, 5, QuestMenuType.Repeat);
        InitializeQuest("샘플6", 1, 5, QuestMenuType.Repeat);

        // 초기 스크롤 위치 초기화
        InitializeScrollPosition();

        allRewardButtons[(int)QuestMenuType.Repeat].onClick.AddListener(ReceiveAllRepeatRewards);
        UpdateAllRepeatRewardButtonState();
    }

    // 스크롤 초기 위치를 설정하는 함수
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

    // ======================================================================================================================
    // [서브 메뉴]

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

    // ======================================================================================================================
    // [New Image 관련]

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Daily)
    public bool HasUnclaimedDailyRewards()
    {
        bool hasActiveQuestReward = false;
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable)
            {
                hasActiveQuestReward = true;
                break;
            }
        }

        hasActiveQuestReward = hasActiveQuestReward || dailySpecialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Weekly)
    public bool HasUnclaimedWeeklyRewards()
    {
        bool hasActiveQuestReward = false;
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable)
            {
                hasActiveQuestReward = true;
                break;
            }
        }

        hasActiveQuestReward = hasActiveQuestReward || weeklySpecialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Repeat)
    public bool HasUnclaimedRepeatRewards()
    {
        bool hasActiveQuestReward = false;
        foreach (var repeatQuest in repeatQuestDictionary)
        {
            if (repeatQuest.Value.rewardButton.interactable)
            {
                hasActiveQuestReward = true;
                break;
            }
        }

        return hasActiveQuestReward;
    }

    // New Image의 상태를 Update하는 함수
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedDailyRewards = HasUnclaimedDailyRewards();
        bool hasUnclaimedWeeklyRewards = HasUnclaimedWeeklyRewards();
        bool hasUnclaimedRepeatRewards = HasUnclaimedRepeatRewards();

        bool hasUnclaimedRewards = hasUnclaimedDailyRewards || hasUnclaimedWeeklyRewards || hasUnclaimedRepeatRewards;

        // Quest Button의 New Image 활성화/비활성화
        mainQuestButtonNewImage.SetActive(hasUnclaimedRewards);

        // Daily Quest Button의 New Image 활성화/비활성화
        subQuestButtonNewImages[(int)QuestMenuType.Daily].SetActive(hasUnclaimedDailyRewards);

        // Weekly Quest Button의 New Image 활성화/비활성화
        subQuestButtonNewImages[(int)QuestMenuType.Weekly].SetActive(hasUnclaimedWeeklyRewards);

        // Repeat Quest Button의 New Image 활성화/비활성화
        subQuestButtonNewImages[(int)QuestMenuType.Repeat].SetActive(hasUnclaimedRepeatRewards);
    }

    // ======================================================================================================================
    // [퀘스트 관련]

    // 퀘스트 초기화
    private void InitializeQuest(string questName, int targetCount, int rewardCash, QuestMenuType menuType)
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

            questData = new QuestUI.QuestData
            {
                currentCount = 0,
                targetCount = targetCount,
                plusTargetCount = targetCount,
                rewardCash = rewardCash,
                isComplete = false
            }
        };

        // UI 텍스트 초기화
        questUI.questName.text = questName;
        questUI.plusCashText.text = $"x {rewardCash}";
        questUI.rewardText.text = "받기";

        // 보상 버튼 리스너 등록
        questUI.rewardButton.onClick.AddListener(() => ReceiveQuestReward(questName, menuType));

        // 퀘스트 데이터를 Dictionary에 추가 (메뉴 타입에 따라 다름)
        if (menuType == QuestMenuType.Daily)
        {
            dailyQuestDictionary[questName] = questUI;
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            weeklyQuestDictionary[questName] = questUI;
        }
        else if (menuType == QuestMenuType.Repeat)
        {
            repeatQuestDictionary[questName] = questUI;
        }

        // UI 업데이트
        UpdateQuestUI(questName, menuType);
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
        questUI.questSlider.value = questData.currentCount;

        // 텍스트 업데이트
        questUI.countText.text = $"{questData.currentCount} / {questData.targetCount}";

        // 완료 여부 확인
        if (menuType == QuestMenuType.Repeat)
        {
            bool isComplete = questData.currentCount >= questData.targetCount;
            questUI.rewardButton.interactable = isComplete;
            questUI.rewardDisabledBG.SetActive(!isComplete);
        }
        else
        {
            bool isComplete = questData.currentCount >= questData.targetCount && !questData.isComplete;
            questUI.rewardButton.interactable = isComplete;
            questUI.rewardDisabledBG.SetActive(!isComplete);

            if (questData.isComplete)
            {
                questUI.rewardText.text = "수령 완료";
            }
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
            UpdateAllDailyRewardButtonState();
        }
        // 주간 퀘스트 업데이트
        if (weeklyQuestDictionary.ContainsKey(questName))
        {
            QuestUI questUI = weeklyQuestDictionary[questName];
            QuestUI.QuestData questData = questUI.questData;

            questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

            UpdateQuestUI(questName, QuestMenuType.Weekly);
            UpdateAllWeeklyRewardButtonState();
        }
        // 반복 퀘스트 업데이트
        if (repeatQuestDictionary.ContainsKey(questName))
        {
            UpdateQuestUI(questName, QuestMenuType.Repeat);
            UpdateAllRepeatRewardButtonState();
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

    // ======================================================================================================================
    // [PlayTime Quest]

    // 플레이타임 증가 함수
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;

        dailyQuestDictionary["플레이 시간"].questData.currentCount = (int)PlayTimeCount;
        weeklyQuestDictionary["플레이 시간"].questData.currentCount = (int)PlayTimeCount;

        UpdateQuestProgress("플레이 시간");
    }

    // 플레이타임 리셋 함수
    public void ResetPlayTimeCount()
    {
        PlayTimeCount = 0;
    }

    // ======================================================================================================================
    // [Merge Quest]

    // 고양이 머지 증가 함수
    public void AddMergeCount()
    {
        MergeCount++;

        dailyQuestDictionary["고양이 머지 횟수"].questData.currentCount = MergeCount;
        weeklyQuestDictionary["고양이 머지 횟수"].questData.currentCount = MergeCount;
        repeatQuestDictionary["고양이 머지 횟수"].questData.currentCount = MergeCount;

        UpdateQuestProgress("고양이 머지 횟수");
    }

    // 고양이 머지 리셋 함수
    public void ResetMergeCount()
    {
        MergeCount = 0;
    }

    // ======================================================================================================================
    // [Spawn Quest]

    // 고양이 스폰 증가 함수
    public void AddSpawnCount()
    {
        SpawnCount++;

        dailyQuestDictionary["고양이 소환 횟수"].questData.currentCount = SpawnCount;
        weeklyQuestDictionary["고양이 소환 횟수"].questData.currentCount = SpawnCount;
        repeatQuestDictionary["고양이 소환 횟수"].questData.currentCount = SpawnCount;

        UpdateQuestProgress("고양이 소환 횟수");
    }

    // 고양이 스폰 리셋 함수
    public void ResetSpawnCount()
    {
        SpawnCount = 0;
    }

    // ======================================================================================================================
    // [Purchase Cats Quest]

    // 고양이 구매 증가 함수
    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;

        dailyQuestDictionary["고양이 구매 횟수"].questData.currentCount = PurchaseCatsCount;
        weeklyQuestDictionary["고양이 구매 횟수"].questData.currentCount = PurchaseCatsCount;
        repeatQuestDictionary["고양이 구매 횟수"].questData.currentCount = PurchaseCatsCount;

        UpdateQuestProgress("고양이 구매 횟수");
    }

    // 고양이 구매 리셋 함수
    public void ResetPurchaseCatsCount()
    {
        PurchaseCatsCount = 0;
    }

    // ======================================================================================================================
    // [Battle Count Quest]

    // 배틀 증가 함수
    public void AddBattleCount()
    {
        BattleCount++;

        dailyQuestDictionary["전투 횟수"].questData.currentCount = BattleCount;

        UpdateQuestProgress("전투 횟수");
    }

    // 배틀 리셋 함수
    public void ResetBattleCount()
    {
        BattleCount = 0;
    }

    // ======================================================================================================================
    // [Stage Count Quest]

    // 스테이지 증가 함수
    public void AddStageCount()
    {
        repeatQuestDictionary["보스 스테이지"].questData.currentCount = StageCount - 1;

        UpdateQuestProgress("보스 스테이지");
    }

    // ======================================================================================================================
    // [Special Reward Quest]

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
    }

    // 모든 Daily 퀘스트가 완료되었는지 확인하는 함수
    private bool AllDailyQuestsCompleted()
    {
        foreach (var quest in dailyQuestDictionary)
        {
            if (!quest.Value.questData.isComplete)
            {
                return false;
            }
        }
        return true;
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
    }

    // 모든 Weekly 퀘스트가 완료되었는지 확인하는 함수
    private bool AllWeeklyQuestsCompleted()
    {
        foreach (var quest in weeklyQuestDictionary)
        {
            if (!quest.Value.questData.isComplete)
            {
                return false;
            }
        }
        return true;
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

    // ======================================================================================================================
    // [전체 보상 받기 관련 - AllReward]

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
    }

    // All Reward 버튼 상태를 업데이트하는 함수 - Daily
    private void UpdateAllDailyRewardButtonState()
    {
        // 보상을 받을 수 있는 버튼이 하나라도 활성화되어 있는지 확인
        bool isAnyRewardAvailable = false;
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                isAnyRewardAvailable = true;
                break;
            }
        }

        // 스페셜 보상도 확인
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
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
    }

    // All Reward 버튼 상태를 업데이트하는 함수 - Weekly
    private void UpdateAllWeeklyRewardButtonState()
    {
        // 보상을 받을 수 있는 버튼이 하나라도 활성화되어 있는지 확인
        bool isAnyRewardAvailable = false;
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
            {
                isAnyRewardAvailable = true;
                break;
            }
        }

        // 스페셜 보상도 확인
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
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
            UpdateAllDailyRewardButtonState();      // 원래 이거로만 호출이 되야하는데 PlayTime이 Update에 있어서 이게 없어도 상시 호출로인해 실행됌.
        }
        else if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
            UpdateWeeklySpecialRewardUI();
            UpdateAllWeeklyRewardButtonState();     // 원래 이거로만 호출이 되야하는데 PlayTime이 Update에 있어서 이게 없어도 상시 호출로인해 실행됌.
        }

        // 퀘스트 UI 업데이트 호출
        UpdateQuestUI(questName, menuType);
    }

    // 개별 퀘스트 보상 지급 처리 함수 - Repeat
    private void ReceiveRepeatQuestReward(string questName, int rewardCash)
    {
        repeatQuestDictionary[questName].questData.targetCount += repeatQuestDictionary[questName].questData.plusTargetCount;
        AddCash(rewardCash);

        UpdateQuestUI(questName, QuestMenuType.Repeat);
        UpdateAllRepeatRewardButtonState();
        SortRepeatQuests();
    }

    // ======================================================================================================================
    // [추가 기능들]

    // 캐쉬 추가 함수
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [반복퀘스트 정렬 관련]

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

    // ======================================================================================================================



}
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// 퀘스트 Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
    public static QuestManager Instance { get; private set; }

    public class QuestUI
    {
        public TextMeshProUGUI questName;           // 퀘스트 이름
        public Slider questSlider;                  // Slider
        public TextMeshProUGUI countText;           // "?/?" Text
        public TextMeshProUGUI plusCashText;        // 보상 재화 개수 Text
        public Button rewardButton;                 // 보상 버튼
        public TextMeshProUGUI rewardText;          // 보상 획득 Text
        public GameObject rewardDisabledBG;         // 보상 버튼 비활성화 BG

        public class QuestData
        {
            public int currentCount;                // 현재 수치
            public int targetCount;                 // 목표 수치
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

    [SerializeField] private GameObject[] mainQuestMenus;                       // 메인 퀘스트 메뉴 Panels
    [SerializeField] private Button[] subQuestMenuButtons;                      // 서브 퀘스트 메뉴 버튼 배열

    [SerializeField] private GameObject questSlotPrefab;                        // Quest Slot Prefab
    [SerializeField] private Transform[] questSlotParents;                      // 슬롯들이 배치될 부모 객체들 (일일, 주간, 반복)

    private Dictionary<string, QuestUI> dailyQuestDictionary = new Dictionary<string, QuestUI>();       // Daily Quest Dictionary
    private Dictionary<string, QuestUI> weeklyQuestDictionary = new Dictionary<string, QuestUI>();      // Weekly Quest Dictionary
    private Dictionary<string, QuestUI> repeatQuestDictionary = new Dictionary<string, QuestUI>();      // Repeat Quest Dictionary

    [SerializeField] private ScrollRect repeatQuestScrollRects;                 // 반복퀘스트의 스크롤뷰

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
    [SerializeField] private GameObject questButtonNewImage;                    // Main 퀘스트 버튼의 New Image
    [SerializeField] private GameObject dailyQuestButtonNewImage;               // Daily 퀘스트 버튼의 New Image
    [SerializeField] private GameObject weeklyQuestButtonNewImage;              // Weekly 퀘스트 버튼의 New Image
    [SerializeField] private GameObject repeatQuestButtonNewImage;              // Repeat 퀘스트 버튼의 New Image

    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;                       // Daily All RewardButton
    [SerializeField] private Button weeklyAllRewardButton;                      // Weekly All RewardButton
    [SerializeField] private Button repeatAllRewardButton;                      // Repeat All RewardButton

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#5f5f5f";                                 // 활성화상태 Color
    private string inactiveColorCode = "#FFFFFF";                               // 비활성화상태 Color

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

    private int dailySpecialRewardCount;                                // Daily 최종 퀘스트 진행 횟수
    public int DailySpecialRewardCount { get => dailySpecialRewardCount; set => dailySpecialRewardCount = value; }

    private int weeklySpecialRewardCount;                               // Weekly 최종 퀘스트 진행 횟수
    public int WeeklySpecialRewardCount { get => weeklySpecialRewardCount; set => weeklySpecialRewardCount = value; }

    // ======================================================================================================================

    // Enum으로 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum QuestMenuType
    {
        Daily,                  // 일일 퀘스트 메뉴
        Weekly,                 // 주간 퀘스트 메뉴
        Repeat,                 // 반복 퀘스트 메뉴
        End                     // Enum의 끝
    }
    private QuestMenuType activeMenuType;                               // 현재 활성화된 메뉴 타입

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
        questButtonNewImage.SetActive(false);
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
        UpdateQuestUI();
        UpdateAllDailyRewardButtonState();
        UpdateAllWeeklyRewardButtonState();
    }

    // ======================================================================================================================

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
        questButton.onClick.AddListener(() => activePanelManager.TogglePanel("QuestMenu"));
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }
    
    // Daily Quest 설정 함수
    private void InitializeDailyQuestManager()
    {
        InitializeQuest("PlayTime", 10, 5, QuestMenuType.Daily);
        InitializeQuest("Merge Cats", 1, 5, QuestMenuType.Daily);
        InitializeQuest("Spawn Cats", 1, 5, QuestMenuType.Daily);
        InitializeQuest("Purchase Cats", 1, 5, QuestMenuType.Daily);
        InitializeQuest("Battle", 1, 5, QuestMenuType.Daily);

        InitializeDailySpecialReward();

        // Daily AllReward 버튼 등록
        dailyAllRewardButton.onClick.AddListener(ReceiveAllDailyRewards);
    }

    // Weekly Quest 설정 함수
    private void InitializeWeeklyQuestManager()
    {
        InitializeQuest("PlayTime", 20, 50, QuestMenuType.Weekly);
        InitializeQuest("Merge Cats", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("Spawn Cats", 10, 50, QuestMenuType.Weekly);
        InitializeQuest("Purchase Cats", 10, 50, QuestMenuType.Weekly);

        InitializeWeeklySpecialReward();

        // AllReward 버튼 등록
        weeklyAllRewardButton.onClick.AddListener(ReceiveAllWeeklyRewards);
    }

    // Repeat Quest 설정 함수
    private void InitializeRepeatQuestManager()
    {
        // 초기 스크롤 위치 초기화
        repeatQuestScrollRects.verticalNormalizedPosition = 1f;

        //// AllReward 버튼 등록
        //repeatAllRewardButton.onClick.AddListener(ReceiveAllRewards);
    }

    // ======================================================================================================================

    // 모든 퀘스트 UI를 업데이트하는 함수
    private void UpdateQuestUI()
    {
        UpdateNewImageStatus();

        UpdateQuestProgress("PlayTime");
        UpdateQuestProgress("Merge Cats");
        UpdateQuestProgress("Spawn Cats");
        UpdateQuestProgress("Purchase Cats");
        UpdateQuestProgress("Battle");

        UpdateDailySpecialRewardUI();
        UpdateWeeklySpecialRewardUI();


        //UpdateDailyQuestUI();
        //UpdateWeeklyQuestUI();
        //UpdateRepeatQuestUI();
    }

    //// Daily Quest UI 업데이트 함수
    //private void UpdateDailyQuestUI()
    //{
    //    UpdateDailySpecialRewardUI();
    //}

    //// Weekly Quest UI 업데이트 함수
    //private void UpdateWeeklyQuestUI()
    //{
    //    UpdateWeeklySpecialRewardUI();
    //}

    //// Repeat Quest UI 업데이트 함수
    //private void UpdateRepeatQuestUI()
    //{

    //}

    // 캐쉬 추가 함수
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
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
            questData = new QuestUI.QuestData
            {
                currentCount = 0,
                targetCount = targetCount,
                rewardCash = rewardCash,
                isComplete = false
            }
        };

        // UI 텍스트 초기화
        questUI.questName.text = questName;
        questUI.plusCashText.text = $"x {rewardCash}";
        questUI.rewardText.text = "Accept";

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

        // 퀘스트 UI가 없다면 종료
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // Slider 값 설정
        questUI.questSlider.maxValue = questData.targetCount;
        questUI.questSlider.value = questData.currentCount;

        // 텍스트 업데이트
        questUI.countText.text = $"{questData.currentCount} / {questData.targetCount}";

        // 완료 여부 확인
        bool isComplete = questData.currentCount >= questData.targetCount && !questData.isComplete;
        questUI.rewardButton.interactable = isComplete;
        questUI.rewardDisabledBG.SetActive(!isComplete);

        if (questData.isComplete)
        {
            questUI.rewardText.text = "Complete";
        }
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

        // 퀘스트 UI가 없다면 종료
        if (questUI == null) return;

        QuestUI.QuestData questData = questUI.questData;

        // 보상 버튼이 비활성화되어 있거나 퀘스트가 이미 완료되었으면 종료
        if (!questUI.rewardButton.interactable || questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref questData.isComplete, questData.rewardCash, questUI.rewardButton, questUI.rewardDisabledBG, menuType);

        // UI 업데이트
        UpdateQuestUI(questName, menuType);
    }

    // ======================================================================================================================
    // [PlayTime Quest]

    // 플레이타임 증가 함수
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
        dailyQuestDictionary["PlayTime"].questData.currentCount = (int)PlayTimeCount;
        weeklyQuestDictionary["PlayTime"].questData.currentCount = (int)PlayTimeCount;
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
        dailyQuestDictionary["Merge Cats"].questData.currentCount = MergeCount;
        weeklyQuestDictionary["Merge Cats"].questData.currentCount = MergeCount;
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
        dailyQuestDictionary["Spawn Cats"].questData.currentCount = SpawnCount;
        weeklyQuestDictionary["Spawn Cats"].questData.currentCount = SpawnCount;
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
        dailyQuestDictionary["Purchase Cats"].questData.currentCount = PurchaseCatsCount;
        weeklyQuestDictionary["Purchase Cats"].questData.currentCount = PurchaseCatsCount;
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
        dailyQuestDictionary["Battle"].questData.currentCount = BattleCount;
    }

    // 배틀 리셋 함수
    public void ResetBattleCount()
    {
        BattleCount = 0;
    }

    // ======================================================================================================================
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
        dailySpecialRewardText.text = "Accept";
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
            dailySpecialRewardText.text = "Complete";
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

    // Daily Special Reward 리셋 함수
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
        weeklySpecialRewardText.text = "Accept";
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
            weeklySpecialRewardText.text = "Complete";
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

    // Weekly Special Reward 리셋 함수
    public void ResetWeeklySpecialRewardCount()
    {
        WeeklySpecialRewardCount = 0;
    }

    // ======================================================================================================================
    // [전체 보상 받기 관련]

    // 모든 활성화된 보상을 지급하는 함수 - Daily
    private void ReceiveAllDailyRewards()
    {
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref dailyQuest.Value.questData.isComplete, dailyQuest.Value.questData.rewardCash,
                    dailyQuest.Value.rewardButton, dailyQuest.Value.rewardDisabledBG, QuestMenuType.Daily);
            }
        }

        // 스페셜 보상도 지급
        if (dailySpecialRewardButton.interactable && !isDailySpecialRewardQuestComplete)
        {
            ReceiveDailySpecialReward();
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

        dailyAllRewardButton.interactable = isAnyRewardAvailable;
    }

    // 모든 활성화된 보상을 지급하는 함수 - Weekly
    private void ReceiveAllWeeklyRewards()
    {
        foreach (var weeklyQuest in weeklyQuestDictionary)
        {
            if (weeklyQuest.Value.rewardButton.interactable && !weeklyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref weeklyQuest.Value.questData.isComplete, weeklyQuest.Value.questData.rewardCash,
                    weeklyQuest.Value.rewardButton, weeklyQuest.Value.rewardDisabledBG, QuestMenuType.Weekly);
            }
        }

        // 스페셜 보상도 지급
        if (weeklySpecialRewardButton.interactable && !isWeeklySpecialRewardQuestComplete)
        {
            ReceiveWeeklySpecialReward();
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

        weeklyAllRewardButton.interactable = isAnyRewardAvailable;
    }

    // 개별 퀘스트 보상 지급 처리 함수
    private void ReceiveQuestReward(ref bool isQuestComplete, int rewardCash, Button rewardButton, GameObject disabledBG, QuestMenuType menuType)
    {
        isQuestComplete = true;
        AddCash(rewardCash);
        rewardButton.interactable = false;
        disabledBG.SetActive(true);

        // QuestType을 받아와서 Daily와 Weekly를 판별해야함
        if (menuType == QuestMenuType.Daily)
        {
            AddDailySpecialRewardCount();
        }
        if (menuType == QuestMenuType.Weekly)
        {
            AddWeeklySpecialRewardCount();
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
            subQuestMenuButtons[index].onClick.AddListener(() => ActivateMenu((QuestMenuType)index));
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
        questButtonNewImage.SetActive(hasUnclaimedRewards);

        // Daily Quest Button의 New Image 활성화/비활성화
        dailyQuestButtonNewImage.SetActive(hasUnclaimedDailyRewards);

        // Weekly Quest Button의 New Image 활성화/비활성화
        weeklyQuestButtonNewImage.SetActive(hasUnclaimedWeeklyRewards);

        // Repeat Quest Button의 New Image 활성화/비활성화
        repeatQuestButtonNewImage.SetActive(hasUnclaimedRepeatRewards);
    }


}

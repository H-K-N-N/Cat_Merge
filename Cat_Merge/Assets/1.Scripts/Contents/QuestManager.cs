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
    [SerializeField] private Button questButton;                        // 퀘스트 버튼
    [SerializeField] private Image questButtonImage;                    // 퀘스트 버튼 이미지
    [SerializeField] private GameObject questMenuPanel;                 // 퀘스트 메뉴 Panel
    [SerializeField] private Button questBackButton;                    // 퀘스트 뒤로가기 버튼
    private ActivePanelManager activePanelManager;                      // ActivePanelManager

    [SerializeField] private GameObject[] mainQuestMenus;               // 메인 퀘스트 메뉴 Panels
    [SerializeField] private Button[] subQuestMenuButtons;              // 서브 퀘스트 메뉴 버튼 배열



    [Header("---[Quest]")]
    [SerializeField] private GameObject questSlotPrefab;                // Quest Slot Prefab
    [SerializeField] private Transform questSlotParent;                 // 슬롯들이 배치될 부모 객체
    private Dictionary<string, QuestUI> dailyQuestDictionary = new Dictionary<string, QuestUI>(); // Daily Quest Dictionary



    [SerializeField] private ScrollRect repeatQuestScrollRects;         // 반복퀘스트의 스크롤뷰

    // ======================================================================================================================

    [Header("---[Special Reward UI]")]
    [SerializeField] private Slider specialRewardQuestSlider;           // Special Reward Slider
    [SerializeField] private TextMeshProUGUI specialRewardCountText;    // "?/?" 텍스트
    [SerializeField] private Button specialRewardButton;                // Special Reward 버튼
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI specialRewardText;         // Special Reward 보상 획득 Text
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward 보상 버튼 비활성화 BG
    private int specialRewardTargetCount;                               // 목표 횟수
    private int specialRewardQuestRewardCash = 500;                     // Special Reward 퀘스트 보상 캐쉬 재화 개수
    private bool isSpecialRewardQuestComplete;                          // Special Reward 퀘스트 완료 여부 상태

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject questButtonNewImage;            // Main 퀘스트 버튼의 New Image
    [SerializeField] private GameObject dailyQuestButtonNewImage;       // Daily 퀘스트 버튼의 New Image
    [SerializeField] private GameObject weeklyQuestButtonNewImage;      // Weekly 퀘스트 버튼의 New Image
    [SerializeField] private GameObject repeatQuestButtonNewImage;      // Repeat 퀘스트 버튼의 New Image

    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;               // Daily All RewardButton
    [SerializeField] private Button weeklyAllRewardButton;              // Weekly All RewardButton
    [SerializeField] private Button repeatAllRewardButton;              // Repeat All RewardButton

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#5f5f5f";                         // 활성화상태 Color
    private string inactiveColorCode = "#FFFFFF";                       // 비활성화상태 Color

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

    private int specialRewardCount;                                     // 최종 퀘스트 진행 횟수
    public int SpecialRewardCount { get => specialRewardCount; set => specialRewardCount = value; }

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
        UpdateAllRewardButtonState();
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
        InitializeQuest("PlayTime", 10, 5);
        InitializeQuest("Merge Cats", 1, 5);
        InitializeQuest("Spawn Cats", 1, 5);
        InitializeQuest("Purchase Cats", 1, 5);
        InitializeQuest("Battle", 1, 5);

        InitializeSpecialReward();

        // AllReward 버튼 등록
        dailyAllRewardButton.onClick.AddListener(ReceiveAllRewards);
    }

    // Weekly Quest 설정 함수
    private void InitializeWeeklyQuestManager()
    {

    }

    // Repeat Quest 설정 함수
    private void InitializeRepeatQuestManager()
    {
        // 초기 스크롤 위치 초기화
        repeatQuestScrollRects.verticalNormalizedPosition = 1f;
    }

    // ======================================================================================================================
    // [퀘스트 관련]

    // 퀘스트 초기화
    private void InitializeQuest(string questName, int targetCount, int rewardCash)
    {
        // Quest Slot 생성
        GameObject newQuestSlot = Instantiate(questSlotPrefab, questSlotParent);

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
        questUI.rewardButton.onClick.AddListener(() => ReceiveQuestReward(questName));

        // Dictionary에 등록
        dailyQuestDictionary[questName] = questUI;

        // UI 업데이트
        UpdateQuestUI(questName);
    }

    // 퀘스트 UI 업데이트
    private void UpdateQuestUI(string questName)
    {
        if (!dailyQuestDictionary.ContainsKey(questName)) return;

        QuestUI questUI = dailyQuestDictionary[questName];
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
        if (!dailyQuestDictionary.ContainsKey(questName)) return;

        QuestUI questUI = dailyQuestDictionary[questName];
        QuestUI.QuestData questData = questUI.questData;

        // 진행도 업데이트
        questData.currentCount = Mathf.Min(questData.currentCount, questData.targetCount);

        // UI 업데이트
        UpdateQuestUI(questName);
    }

    // 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveQuestReward(string questName)
    {
        if (!dailyQuestDictionary.ContainsKey(questName)) return;

        QuestUI questUI = dailyQuestDictionary[questName];
        QuestUI.QuestData questData = questUI.questData;

        if (!questUI.rewardButton.interactable || questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref questData.isComplete, questData.rewardCash, questUI.rewardButton, questUI.rewardDisabledBG);

        // UI 업데이트
        UpdateQuestUI(questName);
    }

    // ======================================================================================================================

    // 모든 퀘스트 UI를 업데이트하는 함수
    private void UpdateQuestUI()
    {
        UpdateNewImageStatus();

        UpdateDailyQuestUI();
        UpdateWeeklyQuestUI();
        UpdateRepeatQuestUI();
    }

    // Daily Quest UI 업데이트 함수
    private void UpdateDailyQuestUI()
    {
        UpdateQuestProgress("PlayTime");
        UpdateQuestProgress("Merge Cats");
        UpdateQuestProgress("Spawn Cats");
        UpdateQuestProgress("Purchase Cats");
        UpdateQuestProgress("Battle");

        UpdateSpecialRewardUI();
    }

    // Weekly Quest UI 업데이트 함수
    private void UpdateWeeklyQuestUI()
    {

    }

    // Repeat Quest UI 업데이트 함수
    private void UpdateRepeatQuestUI()
    {

    }

    // 캐쉬 추가 함수
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [PlayTime Quest]

    // 플레이타임 증가 함수
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
        dailyQuestDictionary["PlayTime"].questData.currentCount = (int)PlayTimeCount;
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
    // [Special Reward Quest]

    // Special Reward Quest 초기 설정 함수
    private void InitializeSpecialReward()
    {
        specialRewardTargetCount = dailyQuestDictionary.Count;

        specialRewardButton.onClick.AddListener(ReceiveSpecialReward);
        isSpecialRewardQuestComplete = false;
        specialRewardButton.interactable = false;
        specialRewardDisabledBG.SetActive(true);
        specialRewardPlusCashText.text = $"x {specialRewardQuestRewardCash}";
        specialRewardText.text = "Accept";
    }

    // Special Reward 퀘스트 UI를 업데이트하는 함수
    private void UpdateSpecialRewardUI()
    {
        int currentCount = Mathf.Min((int)SpecialRewardCount, specialRewardTargetCount);

        // Slider 값 설정
        specialRewardQuestSlider.maxValue = specialRewardTargetCount;
        specialRewardQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        specialRewardCountText.text = $"{currentCount} / {specialRewardTargetCount}";
        if (isSpecialRewardQuestComplete)
        {
            specialRewardText.text = "Complete";
        }

        bool isComplete = AllQuestsCompleted() && !isSpecialRewardQuestComplete;
        specialRewardButton.interactable = isComplete;
        specialRewardDisabledBG.SetActive(!isComplete);
    }

    // Special Reward 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveSpecialReward()
    {
        if (!specialRewardButton.interactable || isSpecialRewardQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        AddCash(specialRewardQuestRewardCash);
        isSpecialRewardQuestComplete = true;
    }

    // Special Reward 증가 함수
    public void AddSpecialRewardCount()
    {
        SpecialRewardCount++;
    }

    // Special Reward 리셋 함수
    public void ResetSpecialRewardCount()
    {
        SpecialRewardCount = 0;
    }

    // 모든 퀘스트가 완료되었는지 확인하는 함수
    private bool AllQuestsCompleted()
    {
        foreach (var quest in dailyQuestDictionary)
        {
            if (quest.Value.questData.isComplete)
            {
                continue;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    // ======================================================================================================================
    // [전체 보상 받기 관련]

    // 모든 활성화된 보상을 지급하는 함수
    private void ReceiveAllRewards()
    {
        foreach (var dailyQuest in dailyQuestDictionary)
        {
            if (dailyQuest.Value.rewardButton.interactable && !dailyQuest.Value.questData.isComplete)
            {
                ReceiveQuestReward(ref dailyQuest.Value.questData.isComplete, dailyQuest.Value.questData.rewardCash,
                    dailyQuest.Value.rewardButton, dailyQuest.Value.rewardDisabledBG);
            }
        }

        // 스페셜 보상도 지급
        if (specialRewardButton.interactable && !isSpecialRewardQuestComplete)
        {
            ReceiveSpecialReward();
        }
    }

    // 개별 퀘스트 보상 지급 처리 함수
    private void ReceiveQuestReward(ref bool isQuestComplete, int rewardCash, Button rewardButton, GameObject disabledBG)
    {
        isQuestComplete = true;
        AddCash(rewardCash);
        rewardButton.interactable = false;
        disabledBG.SetActive(true);

        AddSpecialRewardCount();
    }

    // All Reward 버튼 상태를 업데이트하는 함수
    private void UpdateAllRewardButtonState()
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
        if (specialRewardButton.interactable && !isSpecialRewardQuestComplete)
        {
            isAnyRewardAvailable = true;
        }

        dailyAllRewardButton.interactable = isAnyRewardAvailable;
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

        hasActiveQuestReward = hasActiveQuestReward || specialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Weekly)
    public bool HasUnclaimedWeeklyRewards()
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

        hasActiveQuestReward = hasActiveQuestReward || specialRewardButton.interactable;

        return hasActiveQuestReward;
    }

    // 보상을 받을 수 있는 상태를 확인하는 함수 (Repeat)
    public bool HasUnclaimedRepeatRewards()
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

        hasActiveQuestReward = hasActiveQuestReward || specialRewardButton.interactable;

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
        //weeklyQuestButtonNewImage.SetActive(hasUnclaimedWeeklyRewards);

        // Repeat Quest Button의 New Image 활성화/비활성화
        //repeatQuestButtonNewImage.SetActive(hasUnclaimedRepeatRewards);
    }


}

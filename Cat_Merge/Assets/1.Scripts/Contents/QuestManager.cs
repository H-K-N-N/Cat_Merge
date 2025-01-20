using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class QuestUI
{
    public Slider questSlider;                  // Slider
    public TextMeshProUGUI countText;           // "?/?" 텍스트
    public TextMeshProUGUI plusCashText;        // 보상 재화 개수 Text
    public Button rewardButton;                 // 보상 버튼
    public TextMeshProUGUI rewardText;          // 보상 획득 Text
    public GameObject rewardDisabledBG;         // 보상 버튼 비활성화 BG

    public class QuestData
    {
        public int currentCount;  // 현재 수치
        public int targetCount;   // 목표 수치
        public int rewardCash;    // 보상 캐쉬
        public bool isComplete;   // 완료 여부
    }
    public QuestData questData = new QuestData();
}

// 퀘스트 Script
public class QuestManager : MonoBehaviour
{
    // Singleton Instance
    public static QuestManager Instance { get; private set; }

    [Header("---[QuestManager]")]
    [SerializeField] private Button questButton;                        // 퀘스트 버튼
    [SerializeField] private Image questButtonImage;                    // 퀘스트 버튼 이미지
    [SerializeField] private GameObject questMenuPanel;                 // 퀘스트 메뉴 Panel
    [SerializeField] private Button questBackButton;                    // 퀘스트 뒤로가기 버튼
    private ActivePanelManager activePanelManager;                      // ActivePanelManager

    [SerializeField] private GameObject[] questMenus;                   // 메인 퀘스트 메뉴 Panels
    [SerializeField] private Button[] questMenuButtons;                 // 서브 퀘스트 메뉴 버튼 배열

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject questButtonNewImage;            // Main 퀘스트 버튼의 New Image
    [SerializeField] private GameObject dailyQuestButtonNewImage;       // Daily 퀘스트 버튼의 New Image
    [SerializeField] private GameObject weeklyQuestButtonNewImage;      // Weekly 퀘스트 버튼의 New Image
    [SerializeField] private GameObject repeatQuestButtonNewImage;      // Repeat 퀘스트 버튼의 New Image

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
    // [Quest]

    [Header("---[Quest UI & Data]")]
    public QuestUI playTimeQuestUI;         // PlayTime Quest
    public QuestUI mergeQuestUI;            // Merge Quest
    public QuestUI spawnQuestUI;            // Spawn Quest
    public QuestUI purchaseCatsQuestUI;     // Purchase Cats Quest
    public QuestUI battleQuestUI;           // Battle Quest
    private Dictionary<string, QuestUI.QuestData> quests = new Dictionary<string, QuestUI.QuestData>();

    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;               // Daily All RewardButton
    [SerializeField] private Button weeklyAllRewardButton;              // Weekly All RewardButton
    [SerializeField] private Button repeatAllRewardButton;              // Repeat All RewardButton

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

    [Header("---[Text UI Color]")]
    private string activeColorCode = "#5f5f5f";                         // 활성화상태 Color
    private string inactiveColorCode = "#FFFFFF";                       // 비활성화상태 Color

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
        //InitializeWeeklyQuestManager();
        //InitializeRepeatQuestManager();
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
        InitializePlayTimeQuest();
        InitializeMergeQuest();
        InitializeSpawnQuest();
        InitializePurchaseCatsQuest();
        InitializeBattleQuest();

        InitializeSpecialReward();

        InitializeAllRewardButton();
    }

    // AllRewardButton 설정 함수
    private void InitializeAllRewardButton()
    {
        dailyAllRewardButton.onClick.AddListener(ReceiveAllRewards);
    }

    // ======================================================================================================================

    // 모든 퀘스트 UI를 업데이트하는 함수
    private void UpdateQuestUI()
    {
        UpdateNewImageStatus();

        UpdateDailyQuestUI();
        //UpdateWeeklyQuestManager();
        //UpdateRepeatQuestManager();
    }

    // Daily Quest UI 업데이트 함수
    private void UpdateDailyQuestUI()
    {
        UpdatePlayTimeQuestUI();
        UpdateMergeQuestUI();
        UpdateSpawnQuestUI();
        UpdatePurchaseCatsQuestUI();
        UpdateBattleQuestUI();

        UpdateSpecialRewardUI();
    }

    // 캐쉬 추가 함수
    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    // ======================================================================================================================
    // [PlayTime Quest]

    // 플레이타임 퀘스트 초기 설정 함수
    private void InitializePlayTimeQuest()
    {
        // Dictionary에 초기값 설정
        quests["PlayTime"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 10,
            rewardCash = 5,
            isComplete = false
        };

        // 값 설정
        playTimeQuestUI.questData = quests["PlayTime"];

        // 보상 버튼 설정
        playTimeQuestUI.rewardButton.onClick.AddListener(ReceivePlayTimeReward);
        playTimeQuestUI.rewardButton.interactable = false;
        playTimeQuestUI.rewardDisabledBG.SetActive(false);
        playTimeQuestUI.plusCashText.text = $"x {playTimeQuestUI.questData.rewardCash}";
        playTimeQuestUI.rewardText.text = "Accept";
    }

    // 플레이타임 퀘스트 UI를 업데이트하는 함수
    private void UpdatePlayTimeQuestUI()
    {
        playTimeQuestUI.questData.currentCount = Mathf.Min((int)PlayTimeCount, playTimeQuestUI.questData.targetCount);

        // Slider 값 설정
        playTimeQuestUI.questSlider.maxValue = playTimeQuestUI.questData.targetCount;
        playTimeQuestUI.questSlider.value = playTimeQuestUI.questData.currentCount;

        // "?/?" 텍스트 업데이트
        playTimeQuestUI.countText.text = $"{playTimeQuestUI.questData.currentCount} / {playTimeQuestUI.questData.targetCount}";
        if (playTimeQuestUI.questData.isComplete)
        {
            playTimeQuestUI.rewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = playTimeQuestUI.questData.currentCount >= playTimeQuestUI.questData.targetCount && !playTimeQuestUI.questData.isComplete;
        playTimeQuestUI.rewardButton.interactable = isComplete;
        playTimeQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // 플레이타임 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceivePlayTimeReward()
    {
        if (!playTimeQuestUI.rewardButton.interactable || playTimeQuestUI.questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref playTimeQuestUI.questData.isComplete, playTimeQuestUI.questData.rewardCash, 
            playTimeQuestUI.rewardButton, playTimeQuestUI.rewardDisabledBG);
    }

    // 플레이타임 증가 함수
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
    }

    // 플레이타임 리셋 함수
    public void ResetPlayTimeCount()
    {
        PlayTimeCount = 0;
    }

    // ======================================================================================================================
    // [Merge Quest]

    // 고양이 머지 퀘스트 초기 설정 함수
    private void InitializeMergeQuest()
    {
        // Dictionary에 초기값 설정
        quests["Merge"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // 값 설정
        mergeQuestUI.questData = quests["Merge"];

        // 보상 버튼 설정
        mergeQuestUI.rewardButton.onClick.AddListener(ReceiveMergeReward);
        mergeQuestUI.rewardButton.interactable = false;
        mergeQuestUI.rewardDisabledBG.SetActive(false);
        mergeQuestUI.plusCashText.text = $"x {mergeQuestUI.questData.rewardCash}";
        mergeQuestUI.rewardText.text = "Accept";
    }

    // 고양이 머지 퀘스트 UI를 업데이트하는 함수
    private void UpdateMergeQuestUI()
    {
        mergeQuestUI.questData.currentCount = Mathf.Min((int)MergeCount, mergeQuestUI.questData.targetCount);

        // Slider 값 설정
        mergeQuestUI.questSlider.maxValue = mergeQuestUI.questData.targetCount;
        mergeQuestUI.questSlider.value = mergeQuestUI.questData.currentCount;

        // "?/?" 텍스트 업데이트
        mergeQuestUI.countText.text = $"{mergeQuestUI.questData.currentCount} / {mergeQuestUI.questData.targetCount}";
        if (mergeQuestUI.questData.isComplete)
        {
            mergeQuestUI.rewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = mergeQuestUI.questData.currentCount >= mergeQuestUI.questData.targetCount && !mergeQuestUI.questData.isComplete;
        mergeQuestUI.rewardButton.interactable = isComplete;
        mergeQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 머지 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveMergeReward()
    {
        if (!mergeQuestUI.rewardButton.interactable || mergeQuestUI.questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref mergeQuestUI.questData.isComplete, mergeQuestUI.questData.rewardCash,
            mergeQuestUI.rewardButton, mergeQuestUI.rewardDisabledBG);
    }

    // 고양이 머지 횟수 증가 함수
    public void AddMergeCount()
    {
        MergeCount++;
    }

    // 고양이 머지 횟수 리셋 함수
    public void ResetMergeCount()
    {
        MergeCount = 0;
    }

    // ======================================================================================================================
    // [Spawn Quest]

    // 고양이 스폰 퀘스트 초기 설정 함수
    private void InitializeSpawnQuest()
    {
        // Dictionary에 초기값 설정
        quests["Spawn"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // 값 설정
        spawnQuestUI.questData = quests["Spawn"];

        // 보상 버튼 설정
        spawnQuestUI.rewardButton.onClick.AddListener(ReceiveSpawnReward);
        spawnQuestUI.rewardButton.interactable = false;
        spawnQuestUI.rewardDisabledBG.SetActive(false);
        spawnQuestUI.plusCashText.text = $"x {spawnQuestUI.questData.rewardCash}";
        spawnQuestUI.rewardText.text = "Accept";
    }

    // 고양이 스폰 퀘스트 UI를 업데이트하는 함수
    private void UpdateSpawnQuestUI()
    {
        spawnQuestUI.questData.currentCount = Mathf.Min((int)SpawnCount, spawnQuestUI.questData.targetCount);

        // Slider 값 설정
        spawnQuestUI.questSlider.maxValue = spawnQuestUI.questData.targetCount;
        spawnQuestUI.questSlider.value = spawnQuestUI.questData.currentCount;

        // "?/?" 텍스트 업데이트
        spawnQuestUI.countText.text = $"{spawnQuestUI.questData.currentCount} / {spawnQuestUI.questData.targetCount}";
        if (spawnQuestUI.questData.isComplete)
        {
            spawnQuestUI.rewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = spawnQuestUI.questData.currentCount >= spawnQuestUI.questData.targetCount && !spawnQuestUI.questData.isComplete;
        spawnQuestUI.rewardButton.interactable = isComplete;
        spawnQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 스폰 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveSpawnReward()
    {
        if (!spawnQuestUI.rewardButton.interactable || spawnQuestUI.questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref spawnQuestUI.questData.isComplete, spawnQuestUI.questData.rewardCash,
            spawnQuestUI.rewardButton, spawnQuestUI.rewardDisabledBG);
    }

    // 고양이 스폰 횟수 증가 함수
    public void AddSpawnCount()
    {
        SpawnCount++;
    }

    // 고양이 스폰 횟수 리셋 함수
    public void ResetSpawnCount()
    {
        SpawnCount = 0;
    }

    // ======================================================================================================================
    // [Purchase Cats Quest]

    // 고양이 구매 퀘스트 초기 설정 함수
    private void InitializePurchaseCatsQuest()
    {
        // Dictionary에 초기값 설정
        quests["PurchaseCats"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // 값 설정
        purchaseCatsQuestUI.questData = quests["PurchaseCats"];

        // 보상 버튼 설정
        purchaseCatsQuestUI.rewardButton.onClick.AddListener(ReceivePurchaseCatsReward);
        purchaseCatsQuestUI.rewardButton.interactable = false;
        purchaseCatsQuestUI.rewardDisabledBG.SetActive(false);
        purchaseCatsQuestUI.plusCashText.text = $"x {purchaseCatsQuestUI.questData.rewardCash}";
        purchaseCatsQuestUI.rewardText.text = "Accept";
    }

    // 고양이 구매 퀘스트 UI를 업데이트하는 함수
    private void UpdatePurchaseCatsQuestUI()
    {
        purchaseCatsQuestUI.questData.currentCount = Mathf.Min((int)PurchaseCatsCount, purchaseCatsQuestUI.questData.targetCount);

        // Slider 값 설정
        purchaseCatsQuestUI.questSlider.maxValue = purchaseCatsQuestUI.questData.targetCount;
        purchaseCatsQuestUI.questSlider.value = purchaseCatsQuestUI.questData.currentCount;

        // "?/?" 텍스트 업데이트
        purchaseCatsQuestUI.countText.text = $"{purchaseCatsQuestUI.questData.currentCount} / {purchaseCatsQuestUI.questData.targetCount}";
        if (purchaseCatsQuestUI.questData.isComplete)
        {
            purchaseCatsQuestUI.rewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = purchaseCatsQuestUI.questData.currentCount >= purchaseCatsQuestUI.questData.targetCount && !purchaseCatsQuestUI.questData.isComplete;
        purchaseCatsQuestUI.rewardButton.interactable = isComplete;
        purchaseCatsQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 구매 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceivePurchaseCatsReward()
    {
        if (!purchaseCatsQuestUI.rewardButton.interactable || purchaseCatsQuestUI.questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref purchaseCatsQuestUI.questData.isComplete, purchaseCatsQuestUI.questData.rewardCash,
            purchaseCatsQuestUI.rewardButton, purchaseCatsQuestUI.rewardDisabledBG);
    }

    // 고양이 구매 횟수 증가 함수
    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;
    }

    // 고양이 구매 횟수 리셋 함수
    public void ResetPurchaseCatsCount()
    {
        PurchaseCatsCount = 0;
    }

    // ======================================================================================================================
    // [Battle Count Quest]

    // 배틀 퀘스트 초기 설정 함수
    private void InitializeBattleQuest()
    {
        // Dictionary에 초기값 설정
        quests["Battle"] = new QuestUI.QuestData
        {
            currentCount = 0,
            targetCount = 1,
            rewardCash = 5,
            isComplete = false
        };

        // 값 설정
        battleQuestUI.questData = quests["Battle"];

        // 보상 버튼 설정
        battleQuestUI.rewardButton.onClick.AddListener(ReceiveBattleReward);
        battleQuestUI.rewardButton.interactable = false;
        battleQuestUI.rewardDisabledBG.SetActive(false);
        battleQuestUI.plusCashText.text = $"x {battleQuestUI.questData.rewardCash}";
        battleQuestUI.rewardText.text = "Accept";
    }

    // 배틀 퀘스트 UI를 업데이트하는 함수
    private void UpdateBattleQuestUI()
    {
        battleQuestUI.questData.currentCount = Mathf.Min((int)BattleCount, battleQuestUI.questData.targetCount);

        // Slider 값 설정
        battleQuestUI.questSlider.maxValue = battleQuestUI.questData.targetCount;
        battleQuestUI.questSlider.value = battleQuestUI.questData.currentCount;

        // "?/?" 텍스트 업데이트
        battleQuestUI.countText.text = $"{battleQuestUI.questData.currentCount} / {battleQuestUI.questData.targetCount}";
        if (battleQuestUI.questData.isComplete)
        {
            battleQuestUI.rewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = battleQuestUI.questData.currentCount >= battleQuestUI.questData.targetCount && !battleQuestUI.questData.isComplete;
        battleQuestUI.rewardButton.interactable = isComplete;
        battleQuestUI.rewardDisabledBG.SetActive(!isComplete);
    }

    // 배틀 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveBattleReward()
    {
        if (!battleQuestUI.rewardButton.interactable || battleQuestUI.questData.isComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref battleQuestUI.questData.isComplete, battleQuestUI.questData.rewardCash,
            battleQuestUI.rewardButton, battleQuestUI.rewardDisabledBG);
    }

    // 배틀 횟수 증가 함수
    public void AddBattleCount()
    {
        BattleCount++;
    }

    // 배틀 횟수 리셋 함수
    public void ResetBattleCount()
    {
        BattleCount = 0;
    }

    // ======================================================================================================================
    // [Special Reward Quest]

    // Special Reward Quest 초기 설정 함수
    private void InitializeSpecialReward()
    {
        specialRewardTargetCount = quests.Count;

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
        foreach (var quest in quests)
        {
            if (quest.Value.isComplete)
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
        //foreach (var quest in quests)
        //{
        //    if (quest.Value.rewardButton.interactable && !quest.Value.questData.isComplete)
        //    {
        //        ReceiveQuestReward(ref quest.Value.questData.isComplete, quest.Value.questData.rewardCash,
        //            quest.Value.rewardButton, quest.Value.rewardDisabledBG);
        //    }
        //}

        // 플레이타임 퀘스트 보상 처리
        if (playTimeQuestUI.rewardButton.interactable && !playTimeQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref playTimeQuestUI.questData.isComplete, playTimeQuestUI.questData.rewardCash,
                playTimeQuestUI.rewardButton, playTimeQuestUI.rewardDisabledBG);
        }

        // 고양이 머지 퀘스트 보상 처리
        if (mergeQuestUI.rewardButton.interactable && !mergeQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref mergeQuestUI.questData.isComplete, mergeQuestUI.questData.rewardCash,
                mergeQuestUI.rewardButton, mergeQuestUI.rewardDisabledBG);
        }

        // 고양이 스폰 퀘스트 보상 처리
        if (spawnQuestUI.rewardButton.interactable && !spawnQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref spawnQuestUI.questData.isComplete, spawnQuestUI.questData.rewardCash,
                spawnQuestUI.rewardButton, spawnQuestUI.rewardDisabledBG);
        }

        // 고양이 구매 퀘스트 보상 처리
        if (purchaseCatsQuestUI.rewardButton.interactable && !purchaseCatsQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref purchaseCatsQuestUI.questData.isComplete, purchaseCatsQuestUI.questData.rewardCash,
                purchaseCatsQuestUI.rewardButton, purchaseCatsQuestUI.rewardDisabledBG);
        }

        // 전투 퀘스트 보상 처리
        if (battleQuestUI.rewardButton.interactable && !battleQuestUI.questData.isComplete)
        {
            ReceiveQuestReward(ref battleQuestUI.questData.isComplete, battleQuestUI.questData.rewardCash,
                battleQuestUI.rewardButton, battleQuestUI.rewardDisabledBG);
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
        bool isAnyRewardAvailable =
            (playTimeQuestUI.rewardButton.interactable && !playTimeQuestUI.questData.isComplete) ||
            (mergeQuestUI.rewardButton.interactable && !mergeQuestUI.questData.isComplete) ||
            (spawnQuestUI.rewardButton.interactable && !spawnQuestUI.questData.isComplete) ||
            (purchaseCatsQuestUI.rewardButton.interactable && !purchaseCatsQuestUI.questData.isComplete) ||
            (battleQuestUI.rewardButton.interactable && !battleQuestUI.questData.isComplete);

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
            questMenuButtons[index].onClick.AddListener(() => ActivateMenu((QuestMenuType)index));
        }

        ActivateMenu(QuestMenuType.Daily);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(QuestMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < questMenus.Length; i++)
        {
            questMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < questMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(questMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
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

    // 보상을 받을 수 있는 상태를 확인하는 함수
    public bool HasUnclaimedRewards()
    {
        bool hasActiveReward =
            playTimeQuestUI.rewardButton.interactable ||
            mergeQuestUI.rewardButton.interactable ||
            spawnQuestUI.rewardButton.interactable ||
            purchaseCatsQuestUI.rewardButton.interactable ||
            battleQuestUI.rewardButton.interactable ||
            specialRewardButton.interactable;

        return hasActiveReward;
    }

    // New Image의 상태를 Update하는 함수
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedRewards = HasUnclaimedRewards();

        // Quest Button의 New Image 활성화/비활성화
        questButtonNewImage.SetActive(hasUnclaimedRewards);

        // Daily Quest Button의 New Image 활성화/비활성화
        dailyQuestButtonNewImage.SetActive(hasUnclaimedRewards);
    }


}

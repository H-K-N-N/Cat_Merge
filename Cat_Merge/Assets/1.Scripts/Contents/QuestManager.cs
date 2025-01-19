using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    [SerializeField] private GameObject[] questMenus;                   // 퀘스트 메뉴 Panels
    [SerializeField] private Button[] questMenuButtons;                 // 퀘스트 서브 메뉴 버튼 배열

    [Header("---[New Image UI]")]
    [SerializeField] private GameObject newImage;                       // 퀘스트 버튼의 New Image 

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

    private int specialRewardCount;                                    // 최종 퀘스트 진행 횟수
    public int SpecialRewardCount { get => specialRewardCount; set => specialRewardCount = value; }

    // ======================================================================================================================

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // 플레이타임 Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" 텍스트
    [SerializeField] private Button playTimeRewardButton;               // 플레이타임 보상 버튼
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // 플레이타임 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI playTimeRewardText;        // 플레이타임 보상 획득 Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // 플레이타임 보상 버튼 비활성화 BG
    private int playTimeTargetCount = 10;                               // 목표 플레이타임 (초 단위)                 == 720
    private int playTimeQuestRewardCash = 5;                            // 플레이타임 퀘스트 보상 캐쉬 재화 개수
    private bool isPlayTimeQuestComplete;                               // 플레이타임 퀘스트 완료 여부

    [Header("---[Merge Cats Quest UI]")]
    [SerializeField] private Slider mergeQuestSlider;                   // 고양이 머지 Slider
    [SerializeField] private TextMeshProUGUI mergeCountText;            // "?/?" 텍스트
    [SerializeField] private Button mergeRewardButton;                  // 고양이 머지 보상 버튼
    [SerializeField] private TextMeshProUGUI mergePlusCashText;         // 고양이 머지 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI mergeRewardText;           // 고양이 머지 보상 획득 Text
    [SerializeField] private GameObject mergeRewardDisabledBG;          // 고양이 머지 보상 버튼 비활성화 BG
    private int mergeTargetCount = 1;                                   // 목표 고양이 머지 횟수                     == 100
    private int mergeQuestRewardCash = 5;                               // 고양이 머지 퀘스트 보상 캐쉬 재화 개수
    private bool isMergeQuestComplete;                                  // 고양이 머지 퀘스트 완료 여부

    [Header("---[Spawn Cats Quest UI]")]
    [SerializeField] private Slider spawnQuestSlider;                   // 고양이 스폰 Slider
    [SerializeField] private TextMeshProUGUI spawnCountText;            // "?/?" 텍스트
    [SerializeField] private Button spawnRewardButton;                  // 고양이 스폰 보상 버튼
    [SerializeField] private TextMeshProUGUI spawnPlusCashText;         // 고양이 스폰 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI spawnRewardText;           // 고양이 스폰 보상 획득 Text
    [SerializeField] private GameObject spawnRewardDisabledBG;          // 고양이 스폰 보상 버튼 비활성화 BG
    private int spawnTargetCount = 1;                                   // 목표 고양이 스폰 횟수                     == 100
    private int spawnQuestRewardCash = 5;                               // 고양이 스폰 퀘스트 보상 캐쉬 재화 개수
    private bool isSpawnQuestComplete;                                  // 고양이 스폰 퀘스트 완료 여부

    [Header("---[Purchase Cats Quest UI]")]
    [SerializeField] private Slider purchaseCatsQuestSlider;            // 고양이 구매 Slider
    [SerializeField] private TextMeshProUGUI purchaseCatsCountText;     // "?/?" 텍스트
    [SerializeField] private Button purchaseCatsRewardButton;           // 고양이 구매 보상 버튼
    [SerializeField] private TextMeshProUGUI purchaseCatsPlusCashText;  // 고양이 구매 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI purchaseCatsRewardText;    // 고양이 구매 보상 획득 Text
    [SerializeField] private GameObject purchaseCatsRewardDisabledBG;   // 고양이 구매 보상 버튼 비활성화 BG
    private int purchaseCatsTargetCount = 1;                            // 목표 고양이 구매 횟수                     == 10
    private int purchaseCatsQuestRewardCash = 5;                        // 고양이 구매 퀘스트 보상 캐쉬 재화 개수
    private bool isPurchaseCatsQuestComplete;                           // 고양이 구매 퀘스트 완료 여부

    [Header("---[Battle Quest UI]")]
    [SerializeField] private Slider battleQuestSlider;                  // 전투 Slider
    [SerializeField] private TextMeshProUGUI battleCountText;           // "?/?" 텍스트
    [SerializeField] private Button battleRewardButton;                 // 전투 보상 버튼
    [SerializeField] private TextMeshProUGUI battlePlusCashText;        // 전투 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI battleRewardText;          // 전투 보상 획득 Text
    [SerializeField] private GameObject battleRewardDisabledBG;         // 전투 보상 버튼 비활성화 BG
    private int battleTargetCount = 1;                                  // 목표 전투 횟수                           == 1
    private int battleQuestRewardCash = 5;                              // 전투 퀘스트 보상 캐쉬 재화 개수
    private bool isBattleQuestComplete;                                 // 전투 퀘스트 완료 여부



    [Header("---[All Reward Button]")]
    [SerializeField] private Button dailyAllRewardButton;               // Daily All RewardButton

    [Header("---[Special Reward UI]")]
    [SerializeField] private Slider specialRewardQuestSlider;           // Special Reward Slider
    [SerializeField] private TextMeshProUGUI specialRewardCountText;    // "?/?" 텍스트
    [SerializeField] private Button specialRewardButton;                // Special Reward 버튼
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward 보상 재화 개수 Text
    [SerializeField] private TextMeshProUGUI specialRewardText;         // Special Reward 보상 획득 Text
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward 보상 버튼 비활성화 BG
    private int specialRewardTargetCount = 5;                           // 목표 횟수
    private int specialRewardQuestRewardCash = 500;                     // Special Reward 퀘스트 보상 캐쉬 재화 개수 == 500
    private bool isSpecialRewardQuestComplete;                          // Special Reward 퀘스트 완료 여부 상태

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
        newImage.SetActive(false);
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

        InitializePlayTimeQuest();
        InitializeMergeQuest();
        InitializeSpawnQuest();
        InitializePurchaseCatsQuest();
        InitializeBattleQuest();

        InitializeSpecialReward();

        InitializeAllRewardButton();
    }

    // ======================================================================================================================

    // 모든 퀘스트 UI를 업데이트하는 함수
    private void UpdateQuestUI()
    {
        UpdatePlayTimeQuestUI();
        UpdateMergeQuestUI();
        UpdateSpawnQuestUI();
        UpdatePurchaseCatsQuestUI();
        UpdateBattleQuestUI();

        UpdateSpecialRewardUI();

        UpdateNewImageStatus();
    }

    // New Image 상태를 업데이트하는 함수
    private void UpdateNewImageStatus()
    {
        bool hasActiveReward =
            playTimeRewardButton.interactable ||
            mergeRewardButton.interactable ||
            spawnRewardButton.interactable ||
            purchaseCatsRewardButton.interactable ||
            battleRewardButton.interactable ||
            specialRewardButton.interactable;

        newImage.SetActive(hasActiveReward);
    }

    // QuestButton 설정 함수
    private void InitializeQuestButton()
    {
        questButton.onClick.AddListener(() => activePanelManager.TogglePanel("QuestMenu"));
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }

    // AllRewardButton 설정 함수
    private void InitializeAllRewardButton()
    {
        dailyAllRewardButton.onClick.AddListener(ReceiveAllRewards);
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
        playTimeRewardButton.onClick.AddListener(ReceivePlayTimeReward);
        isPlayTimeQuestComplete = false;
        playTimeRewardButton.interactable = false;
        playTimeRewardDisabledBG.SetActive(false);
        playTimePlusCashText.text = $"x {playTimeQuestRewardCash}";
        playTimeRewardText.text = "Accept";
    }

    // 플레이타임 퀘스트 UI를 업데이트하는 함수
    private void UpdatePlayTimeQuestUI()
    {
        // 최대 시간을 초과하지 않도록 설정
        int currentTime = Mathf.Min((int)PlayTimeCount, playTimeTargetCount);

        // Slider 값 설정
        playTimeQuestSlider.maxValue = playTimeTargetCount;
        playTimeQuestSlider.value = currentTime;

        // "?/?" 텍스트 업데이트
        playTimeCountText.text = $"{currentTime} / {playTimeTargetCount}";
        if (isPlayTimeQuestComplete)
        {
            playTimeRewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentTime >= playTimeTargetCount && !isPlayTimeQuestComplete;
        playTimeRewardButton.interactable = isComplete;
        playTimeRewardDisabledBG.SetActive(!isComplete);
    }

    // 플레이타임 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceivePlayTimeReward()
    {
        if (!playTimeRewardButton.interactable || isPlayTimeQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref isPlayTimeQuestComplete, playTimeQuestRewardCash, playTimeRewardButton, playTimeRewardDisabledBG);
        //isPlayTimeQuestComplete = true;
        //AddCash(playTimeQuestRewardCash);
        //AddSpecialRewardCount();
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
        mergeRewardButton.onClick.AddListener(ReceiveMergeReward);
        isMergeQuestComplete = false;
        mergeRewardButton.interactable = false;
        mergeRewardDisabledBG.SetActive(false);
        mergePlusCashText.text = $"x {mergeQuestRewardCash}";
        mergeRewardText.text = "Accept";
    }

    // 고양이 머지 퀘스트 UI를 업데이트하는 함수
    private void UpdateMergeQuestUI()
    {
        int currentCount = Mathf.Min((int)MergeCount, mergeTargetCount);

        // Slider 값 설정
        mergeQuestSlider.maxValue = mergeTargetCount;
        mergeQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        mergeCountText.text = $"{currentCount} / {mergeTargetCount}";
        if (isMergeQuestComplete)
        {
            mergeRewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= mergeTargetCount && !isMergeQuestComplete;
        mergeRewardButton.interactable = isComplete;
        mergeRewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 머지 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveMergeReward()
    {
        if (!mergeRewardButton.interactable || isMergeQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref isMergeQuestComplete, mergeQuestRewardCash, mergeRewardButton, mergeRewardDisabledBG);
        //isMergeQuestComplete = true;
        //AddCash(mergeQuestRewardCash);
        //AddSpecialRewardCount();
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
        spawnRewardButton.onClick.AddListener(ReceiveSpawnReward);
        isSpawnQuestComplete = false;
        spawnRewardButton.interactable = false;
        spawnRewardDisabledBG.SetActive(false);
        spawnPlusCashText.text = $"x {spawnQuestRewardCash}";
        spawnRewardText.text = "Accept";
    }

    // 고양이 스폰 퀘스트 UI를 업데이트하는 함수
    private void UpdateSpawnQuestUI()
    {
        int currentCount = Mathf.Min((int)SpawnCount, spawnTargetCount);

        // Slider 값 설정
        spawnQuestSlider.maxValue = spawnTargetCount;
        spawnQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        spawnCountText.text = $"{currentCount} / {spawnTargetCount}";
        if (isSpawnQuestComplete)
        {
            spawnRewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= spawnTargetCount && !isSpawnQuestComplete;
        spawnRewardButton.interactable = isComplete;
        spawnRewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 스폰 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveSpawnReward()
    {
        if (!spawnRewardButton.interactable || isSpawnQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref isSpawnQuestComplete, spawnQuestRewardCash, spawnRewardButton, spawnRewardDisabledBG);
        //isSpawnQuestComplete = true;
        //AddCash(spawnQuestRewardCash);
        //AddSpecialRewardCount();
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
        purchaseCatsRewardButton.onClick.AddListener(ReceivePurchaseCatsReward);
        isPurchaseCatsQuestComplete = false;
        purchaseCatsRewardButton.interactable = false;
        purchaseCatsRewardDisabledBG.SetActive(false);
        purchaseCatsPlusCashText.text = $"x {purchaseCatsQuestRewardCash}";
        purchaseCatsRewardText.text = "Accept";
    }

    // 고양이 구매 퀘스트 UI를 업데이트하는 함수
    private void UpdatePurchaseCatsQuestUI()
    {
        int currentCount = Mathf.Min((int)PurchaseCatsCount, purchaseCatsTargetCount);

        // Slider 값 설정
        purchaseCatsQuestSlider.maxValue = purchaseCatsTargetCount;
        purchaseCatsQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        purchaseCatsCountText.text = $"{currentCount} / {purchaseCatsTargetCount}";
        if (isPurchaseCatsQuestComplete)
        {
            purchaseCatsRewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= purchaseCatsTargetCount && !isPurchaseCatsQuestComplete;
        purchaseCatsRewardButton.interactable = isComplete;
        purchaseCatsRewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 구매 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceivePurchaseCatsReward()
    {
        if (!purchaseCatsRewardButton.interactable || isPurchaseCatsQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref isPurchaseCatsQuestComplete, purchaseCatsQuestRewardCash, purchaseCatsRewardButton, purchaseCatsRewardDisabledBG);
        //AddCash(purchaseCatsQuestRewardCash);
        //isPurchaseCatsQuestComplete = true;
        //AddSpecialRewardCount();
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
        battleRewardButton.onClick.AddListener(ReceiveBattleReward);
        isBattleQuestComplete = false;
        battleRewardButton.interactable = false;
        battleRewardDisabledBG.SetActive(false);
        battlePlusCashText.text = $"x {battleQuestRewardCash}";
        battleRewardText.text = "Accept";
    }

    // 배틀 퀘스트 UI를 업데이트하는 함수
    private void UpdateBattleQuestUI()
    {
        int currentCount = Mathf.Min((int)BattleCount, battleTargetCount);

        // Slider 값 설정
        battleQuestSlider.maxValue = battleTargetCount;
        battleQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        battleCountText.text = $"{currentCount} / {battleTargetCount}";
        if (isBattleQuestComplete)
        {
            battleRewardText.text = "Complete";
        }

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= battleTargetCount && !isBattleQuestComplete;
        battleRewardButton.interactable = isComplete;
        battleRewardDisabledBG.SetActive(!isComplete);
    }

    // 배틀 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveBattleReward()
    {
        if (!battleRewardButton.interactable || isBattleQuestComplete) return;

        // 보상 지급 처리 & 퀘스트 완료 처리
        ReceiveQuestReward(ref isBattleQuestComplete, battleQuestRewardCash, battleRewardButton, battleRewardDisabledBG);
        //isBattleQuestComplete = true;
        //AddCash(battleQuestRewardCash);
        //AddSpecialRewardCount();
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

        //// "?/?" 텍스트 업데이트
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
        if (isPlayTimeQuestComplete && isMergeQuestComplete && isSpawnQuestComplete && isPurchaseCatsQuestComplete && isBattleQuestComplete)
        {
            return true;
        }
        else
        {
            return false;
        }
    }




    // ======================================================================================================================
    // [전체 보상 받기 관련]

    // 모든 활성화된 보상을 지급하는 함수
    private void ReceiveAllRewards()
    {
        //bool isAnyRewardGiven = false;

        // 플레이타임 퀘스트 보상 처리
        if (playTimeRewardButton.interactable && !isPlayTimeQuestComplete)
        {
            ReceiveQuestReward(ref isPlayTimeQuestComplete, playTimeQuestRewardCash, playTimeRewardButton, playTimeRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // 고양이 머지 퀘스트 보상 처리
        if (mergeRewardButton.interactable && !isMergeQuestComplete)
        {
            ReceiveQuestReward(ref isMergeQuestComplete, mergeQuestRewardCash, mergeRewardButton, mergeRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // 고양이 스폰 퀘스트 보상 처리
        if (spawnRewardButton.interactable && !isSpawnQuestComplete)
        {
            ReceiveQuestReward(ref isSpawnQuestComplete, spawnQuestRewardCash, spawnRewardButton, spawnRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // 고양이 구매 퀘스트 보상 처리
        if (purchaseCatsRewardButton.interactable && !isPurchaseCatsQuestComplete)
        {
            ReceiveQuestReward(ref isPurchaseCatsQuestComplete, purchaseCatsQuestRewardCash, purchaseCatsRewardButton, purchaseCatsRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        // 전투 퀘스트 보상 처리
        if (battleRewardButton.interactable && !isBattleQuestComplete)
        {
            ReceiveQuestReward(ref isBattleQuestComplete, battleQuestRewardCash, battleRewardButton, battleRewardDisabledBG);
            //isAnyRewardGiven = true;
        }

        //// 보상을 지급한 경우 UI 업데이트 (Update문을 바꿔야 쓸듯)
        //if (isAnyRewardGiven)
        //{
        //    UpdateQuestsUI();
        //}
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
            (playTimeRewardButton.interactable && !isPlayTimeQuestComplete) ||
            (mergeRewardButton.interactable && !isMergeQuestComplete) ||
            (spawnRewardButton.interactable && !isSpawnQuestComplete) ||
            (purchaseCatsRewardButton.interactable && !isPurchaseCatsQuestComplete) ||
            (battleRewardButton.interactable && !isBattleQuestComplete);

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
        string colorCode = isActive ? "#5f5f5f" : "#FFFFFF";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }


}

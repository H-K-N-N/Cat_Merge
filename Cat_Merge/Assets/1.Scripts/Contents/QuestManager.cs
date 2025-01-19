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

    // ======================================================================================================================

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // 플레이타임 Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" 텍스트
    [SerializeField] private Button playTimeRewardButton;               // 플레이타임 보상 버튼
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // 플레이타임 보상 재화 개수 Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // 플레이타임 보상 버튼 비활성화 BG
    private int playTimeTargetCount = 10;                               // 목표 플레이타임 (초 단위)                 == 720
    private int playTimeQuestRewardCash = 5;                            // 플레이타임 퀘스트 보상 캐쉬 재화 개수
    private bool isPlayTimeQuestComplete;                               // 플레이타임 퀘스트 완료 여부

    [Header("---[Merge Cats Quest UI]")]
    [SerializeField] private Slider mergeQuestSlider;                   // 고양이 머지 Slider
    [SerializeField] private TextMeshProUGUI mergeCountText;            // "?/?" 텍스트
    [SerializeField] private Button mergeRewardButton;                  // 고양이 머지 보상 버튼
    [SerializeField] private TextMeshProUGUI mergePlusCashText;         // 고양이 머지 보상 재화 개수 Text
    [SerializeField] private GameObject mergeRewardDisabledBG;          // 고양이 머지 보상 버튼 비활성화 BG
    private int mergeTargetCount = 1;                                   // 목표 고양이 머지 횟수                     == 100
    private int mergeQuestRewardCash = 5;                               // 고양이 머지 퀘스트 보상 캐쉬 재화 개수
    private bool isMergeQuestComplete;                                  // 고양이 머지 퀘스트 완료 여부

    [Header("---[Spawn Cats Quest UI]")]
    [SerializeField] private Slider spawnQuestSlider;                   // 고양이 스폰 Slider
    [SerializeField] private TextMeshProUGUI spawnCountText;            // "?/?" 텍스트
    [SerializeField] private Button spawnRewardButton;                  // 고양이 스폰 보상 버튼
    [SerializeField] private TextMeshProUGUI spawnPlusCashText;         // 고양이 스폰 보상 재화 개수 Text
    [SerializeField] private GameObject spawnRewardDisabledBG;          // 고양이 스폰 보상 버튼 비활성화 BG
    private int spawnTargetCount = 1;                                   // 목표 고양이 스폰 횟수                     == 100
    private int spawnQuestRewardCash = 5;                               // 고양이 스폰 퀘스트 보상 캐쉬 재화 개수
    private bool isSpawnQuestComplete;                                  // 고양이 스폰 퀘스트 완료 여부

    [Header("---[Purchase Cats Quest UI]")]
    [SerializeField] private Slider purchaseCatsQuestSlider;            // 고양이 구매 Slider
    [SerializeField] private TextMeshProUGUI purchaseCatsCountText;     // "?/?" 텍스트
    [SerializeField] private Button purchaseCatsRewardButton;           // 고양이 구매 보상 버튼
    [SerializeField] private TextMeshProUGUI purchaseCatsPlusCashText;  // 고양이 구매 보상 재화 개수 Text
    [SerializeField] private GameObject purchaseCatsRewardDisabledBG;   // 고양이 구매 보상 버튼 비활성화 BG
    private int purchaseCatsTargetCount = 1;                            // 목표 고양이 구매 횟수                     == 10
    private int purchaseCatsQuestRewardCash = 5;                        // 고양이 구매 퀘스트 보상 캐쉬 재화 개수
    private bool isPurchaseCatsQuestComplete;                           // 고양이 구매 퀘스트 완료 여부

    [Header("---[Battle Quest UI]")]
    [SerializeField] private Slider battleQuestSlider;                  // 전투 Slider
    [SerializeField] private TextMeshProUGUI battleCountText;           // "?/?" 텍스트
    [SerializeField] private Button battleRewardButton;                 // 전투 보상 버튼
    [SerializeField] private TextMeshProUGUI battlePlusCashText;        // 전투 보상 재화 개수 Text
    [SerializeField] private GameObject battleRewardDisabledBG;         // 전투 보상 버튼 비활성화 BG
    private int battleTargetCount = 1;                                  // 목표 전투 횟수                           == 1
    private int battleQuestRewardCash = 5;                              // 전투 퀘스트 보상 캐쉬 재화 개수
    private bool isBattleQuestComplete;                                 // 전투 퀘스트 완료 여부

    [Header("---[Special Reward UI]")]
    [SerializeField] private Button specialRewardButton;                // Special Reward 버튼
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward 보상 재화 개수 Text
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward 보상 버튼 비활성화 BG
    private int specialRewardQuestRewardCash = 500;                     // Special Reward 퀘스트 보상 캐쉬 재화 개수 == 500
    private bool isSpecialRewardQuestComplete;                          // Special Reward 퀘스트 완료 여부 상태

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
    }

    // ======================================================================================================================

    // 모든 QuestManager 시작 함수들 모음
    private void InitializeQuestManager()
    {
        InitializeQuestButton();

        InitializePlayTimeQuest();
        InitializeMergeQuest();
        InitializeSpawnQuest();
        InitializePurchaseCatsQuest();
        InitializeBattleQuest();

        InitializeSpecialReward();
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
        playTimePlusCashText.text = $"+{playTimeQuestRewardCash}";
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
        if (isPlayTimeQuestComplete)
        {
            playTimeCountText.text = "Complete";
        }
        else
        {
            playTimeCountText.text = $"{currentTime}/{playTimeTargetCount}";
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
        isPlayTimeQuestComplete = true;
        AddCash(playTimeQuestRewardCash);
    }

    // 플레이타임 증가 함수
    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
    }

    // 
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
        mergePlusCashText.text = $"+{mergeQuestRewardCash}";
    }

    // 고양이 머지 퀘스트 UI를 업데이트하는 함수
    private void UpdateMergeQuestUI()
    {
        int currentCount = Mathf.Min((int)MergeCount, mergeTargetCount);

        // Slider 값 설정
        mergeQuestSlider.maxValue = mergeTargetCount;
        mergeQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        if (isMergeQuestComplete)
        {
            mergeCountText.text = "Complete";
        }
        else
        {
            mergeCountText.text = $"{currentCount}/{mergeTargetCount}";
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
        isMergeQuestComplete = true;
        AddCash(mergeQuestRewardCash);
    }

    // 고양이 머지 횟수 증가 함수
    public void AddMergeCount()
    {
        MergeCount++;
    }

    // 
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
        spawnPlusCashText.text = $"+{spawnQuestRewardCash}";
    }

    // 고양이 스폰 퀘스트 UI를 업데이트하는 함수
    private void UpdateSpawnQuestUI()
    {
        int currentCount = Mathf.Min((int)SpawnCount, spawnTargetCount);

        // Slider 값 설정
        spawnQuestSlider.maxValue = spawnTargetCount;
        spawnQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        if (isSpawnQuestComplete)
        {
            spawnCountText.text = "Complete";
        }
        else
        {
            spawnCountText.text = $"{currentCount}/{spawnTargetCount}";
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
        isSpawnQuestComplete = true;
        AddCash(spawnQuestRewardCash);
    }

    // 고양이 스폰 횟수 증가 함수
    public void AddSpawnCount()
    {
        SpawnCount++;
    }

    // 
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
        purchaseCatsPlusCashText.text = $"+{purchaseCatsQuestRewardCash}";
    }

    // 고양이 구매 퀘스트 UI를 업데이트하는 함수
    private void UpdatePurchaseCatsQuestUI()
    {
        int currentCount = Mathf.Min((int)PurchaseCatsCount, purchaseCatsTargetCount);

        // Slider 값 설정
        purchaseCatsQuestSlider.maxValue = purchaseCatsTargetCount;
        purchaseCatsQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        if (isPurchaseCatsQuestComplete)
        {
            purchaseCatsCountText.text = "Complete";
        }
        else
        {
            purchaseCatsCountText.text = $"{currentCount}/{purchaseCatsTargetCount}";
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
        AddCash(purchaseCatsQuestRewardCash);
        isPurchaseCatsQuestComplete = true;
    }

    // 고양이 구매 횟수 증가 함수
    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;
    }

    // 
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
        battlePlusCashText.text = $"+{battleQuestRewardCash}";
    }

    // 배틀 퀘스트 UI를 업데이트하는 함수
    private void UpdateBattleQuestUI()
    {
        int currentCount = Mathf.Min((int)BattleCount, battleTargetCount);

        // Slider 값 설정
        battleQuestSlider.maxValue = battleTargetCount;
        battleQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        if (isBattleQuestComplete)
        {
            battleCountText.text = "Complete";
        }
        else
        {
            battleCountText.text = $"{currentCount}/{battleTargetCount}";
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
        isBattleQuestComplete = true;
        AddCash(battleQuestRewardCash);
    }

    // 배틀 횟수 증가 함수
    public void AddBattleCount()
    {
        BattleCount++;
    }

    // 
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
        specialRewardPlusCashText.text = $"+{specialRewardQuestRewardCash}";
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

    // Special Reward 퀘스트 UI를 업데이트하는 함수
    private void UpdateSpecialRewardUI()
    {
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

}

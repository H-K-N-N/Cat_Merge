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
    [SerializeField] private GameObject newImage;                       // 퀘스트 Button의 New Image 

    // ======================================================================================================================

    private float playTimeCount;                                        // 플레이타임 카운트
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int mergeCount;                                             // 고양이 머지 횟수
    public int MergeCount { get => mergeCount; set => mergeCount = value; }

    private int feedCount;                                              // 고양이 스폰 횟수(먹이 준 횟수)
    public int FeedCount { get => feedCount; set => feedCount = value; }

    private int purchaseCatsCount;                                      // 고양이 구매 횟수
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    // ======================================================================================================================

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // 플레이타임 Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" 텍스트
    [SerializeField] private Button playTimeRewardButton;               // 플레이타임 보상 버튼
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // 플레이타임 보상 재화 개수 Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // 플레이타임 보상 버튼 비활성화 BG
    private int playTimeTargetCount = 720;                              // 목표 플레이타임 (초 단위)
    private int playTimeQuestRewardCash = 5;                            // 플레이타임 퀘스트 보상 캐쉬 재화 개수
    private bool isPlayTimeQuestComplete = false;                       // 플레이타임 퀘스트 완료 여부

    [Header("---[Merge Cats Quest UI]")]
    [SerializeField] private Slider mergeQuestSlider;                 // 머지 Slider
    [SerializeField] private TextMeshProUGUI mergeCountText;          // "?/?" 텍스트
    [SerializeField] private Button mergeRewardButton;                // 머지 보상 버튼
    [SerializeField] private TextMeshProUGUI mergePlusCashText;       // 머지 보상 재화 개수 Text
    [SerializeField] private GameObject mergeRewardDisabledBG;        // 머지 보상 버튼 비활성화 BG
    private int mergeTargetCount = 100;                               // 목표 머지 횟수
    private int mergeQuestRewardCash = 5;                             // 머지 퀘스트 보상 캐쉬 재화 개수
    private bool isMergeQuestComplete = false;                        // 머지 퀘스트 완료 여부

    [Header("---[Spawn Cats Quest UI]")]
    [SerializeField] private Slider giveFeedQuestSlider;                // 고양이 스폰 Slider
    [SerializeField] private TextMeshProUGUI giveFeedCountText;         // "?/?" 텍스트
    [SerializeField] private Button giveFeedRewardButton;               // 스폰 보상 버튼
    [SerializeField] private TextMeshProUGUI giveFeedPlusCashText;      // 스폰 보상 재화 개수 Text
    [SerializeField] private GameObject giveFeedRewardDisabledBG;       // 스폰 보상 버튼 비활성화 BG
    private int giveFeedTargetCount = 1;                                // 목표 스폰 횟수
    private int increaseGiveFeedTargetCount = 2;                        // 목표 스폰 횟수 증가치
    private int giveFeedQuestRewardCash = 5;                            // 스폰 퀘스트 보상 캐쉬 재화 개수
    private bool isGiveFeedQuestComplete = false;                       // 스폰 퀘스트 완료 여부

    [Header("---[Purchase Cats Quest UI]")]
    [SerializeField] private Slider purchaseCatsQuestSlider;            // 고양이 구매 Slider
    [SerializeField] private TextMeshProUGUI purchaseCatsCountText;     // "?/?" 텍스트
    [SerializeField] private Button purchaseCatsRewardButton;           // 고양이 구매 보상 버튼
    [SerializeField] private TextMeshProUGUI purchaseCatsPlusCashText;  // 고양이 구매 보상 재화 개수 Text
    [SerializeField] private GameObject purchaseCatsRewardDisabledBG;   // 고양이 구매 보상 버튼 비활성화 BG
    private int purchaseCatsTargetCount = 1;                            // 목표 고양이 구매 횟수
    private int increasePurchaseCatsTargetCount = 2;                    // 목표 고양이 구매 횟수 증가치
    private int purchaseCatsQuestRewardCash = 5;                        // 고양이 구매 퀘스트 보상 캐쉬 재화 개수
    private bool isPurchaseCatsQuestComplete = false;                   // 고양이 구매 퀘스트 완료 여부

    [Header("---[Special Reward UI]")]
    [SerializeField] private Button specialRewardButton;                // Special Reward 버튼
    [SerializeField] private TextMeshProUGUI specialRewardPlusCashText; // Special Reward 보상 재화 개수 Text
    [SerializeField] private GameObject specialRewardRewardDisabledBG;  // Special Reward 보상 버튼 비활성화 BG
    [SerializeField] private GameObject specialRewardDisabledBG;        // Special Reward 비활성화 BG
    private int specialRewardQuestRewardCash = 500;                     // Special Reward 퀘스트 보상 캐쉬 재화 개수
    private bool isSpecialRewardActive = false;                         // Special Reward 활성화 상태

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
        //InitializeMergeQuest();
        //InitializeGiveFeedQuest();
        //InitializePurchaseCatsQuest();
        //InitializeSpecialReward();
    }

    // ======================================================================================================================

    // 모든 퀘스트 UI를 업데이트하는 함수
    private void UpdateQuestUI()
    {
        UpdatePlayTimeQuestUI();
        //UpdateGiveFeedQuestUI();
        //UpdateMergeQuestUI();
        //UpdatePurchaseCatsQuestUI();
        //UpdateSpecialRewardUI();

        UpdateNewImageStatus();
    }

    // New Image 상태를 업데이트하는 함수
    private void UpdateNewImageStatus()
    {
        bool hasActiveReward =
            playTimeRewardButton.interactable ||
            giveFeedRewardButton.interactable ||
            mergeRewardButton.interactable ||
            purchaseCatsRewardButton.interactable ||
            specialRewardButton.interactable;

        newImage.SetActive(hasActiveReward);
    }

    // QuestButton 설정
    private void InitializeQuestButton()
    {
        questButton.onClick.AddListener(() => activePanelManager.TogglePanel("QuestMenu"));
        questBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("QuestMenu"));
    }

    // ======================================================================================================================

    // 퀘스트 관련 변수들

    public void AddCash(int amount)
    {
        GameManager.Instance.Cash += amount;
    }

    public void AddFeedCount()
    {
        FeedCount++;
    }

    public void ResetFeedCount(int count)
    {
        FeedCount = count;
    }

    public void AddMergeCount()
    {
        MergeCount++;
    }

    public void ResetMergeCount(int count)
    {
        MergeCount = count;
    }

    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
    }

    public void AddPurchaseCatsCount()
    {
        PurchaseCatsCount++;
    }

    public void ResetPurchaseCatsCount(int count)
    {
        PurchaseCatsCount = count;
    }

    // ======================================================================================================================

    // PlayTime 퀘스트 초기 설정
    private void InitializePlayTimeQuest()
    {
        playTimeRewardButton.onClick.AddListener(ReceivePlayTimeReward);
        playTimeRewardButton.interactable = false;
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
        UpdatePlayTimeQuestUI();
    }
    
    // ======================================================================================================================

    // Merge 퀘스트 초기 설정
    private void InitializeMergeQuest()
    {
        mergeRewardButton.onClick.AddListener(ReceiveMergeReward);
        mergeRewardButton.interactable = false;
        mergePlusCashText.text = $"+{mergeQuestRewardCash}";
    }

    // 머지 퀘스트 UI를 업데이트하는 함수
    private void UpdateMergeQuestUI()
    {
        int currentCount = MergeCount;

        // 목표가 10 이상이면 퀘스트 종료 상태 처리
        if (mergeTargetCount >= 10)
        {
            mergeQuestSlider.value = mergeQuestSlider.maxValue;
            mergeCountText.text = "Complete";
            mergeRewardButton.interactable = false;
            mergeRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        mergeQuestSlider.maxValue = mergeTargetCount;
        mergeQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        mergeCountText.text = $"{currentCount}/{mergeTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= mergeTargetCount;
        mergeRewardButton.interactable = isComplete;
        mergeRewardDisabledBG.SetActive(!isComplete);
    }

    // 머지 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveMergeReward()
    {
        int currentCount = MergeCount;

        // 목표가 10 이상인 경우 더 이상 보상 지급 불가
        if (mergeTargetCount >= 10)
        {
            isMergeQuestComplete = true;
            return;
        }

        if (currentCount >= mergeTargetCount)
        {
            // 초과된 횟수 계산
            int excessCount = currentCount - mergeTargetCount;

            // 퀘스트 완료 처리
            AddCash(mergeQuestRewardCash);
            ResetMergeCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdateMergeQuestUI();
        }
    }

    // ======================================================================================================================

    // GiveFeed 퀘스트 초기 설정
    private void InitializeGiveFeedQuest()
    {
        giveFeedRewardButton.onClick.AddListener(ReceiveGiveFeedReward);
        giveFeedRewardButton.interactable = false;
        giveFeedPlusCashText.text = $"+{giveFeedQuestRewardCash}";
    }

    // 스폰 퀘스트 UI를 업데이트하는 함수
    private void UpdateGiveFeedQuestUI()
    {
        int currentCount = FeedCount;

        // 목표가 10 이상이면 퀘스트 종료 상태 처리
        if (giveFeedTargetCount >= 10)
        {
            giveFeedQuestSlider.value = giveFeedQuestSlider.maxValue;
            giveFeedCountText.text = "Complete";
            giveFeedRewardButton.interactable = false;
            giveFeedRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        giveFeedQuestSlider.maxValue = giveFeedTargetCount;
        giveFeedQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        giveFeedCountText.text = $"{currentCount}/{giveFeedTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= giveFeedTargetCount;
        giveFeedRewardButton.interactable = isComplete;
        giveFeedRewardDisabledBG.SetActive(!isComplete);
    }

    // 스폰 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveGiveFeedReward()
    {
        int currentCount = FeedCount;

        // 목표가 10 이상인 경우 더 이상 보상 지급 불가
        if (giveFeedTargetCount >= 10)
        {
            isGiveFeedQuestComplete = true;
            return;
        }

        if (currentCount >= giveFeedTargetCount)
        {
            // 초과된 횟수 계산
            int excessCount = currentCount - giveFeedTargetCount;

            // 목표 스폰 횟수 증가 (퀘스트 난이도 상승)
            giveFeedTargetCount += increaseGiveFeedTargetCount;

            // 퀘스트 완료 처리
            AddCash(giveFeedQuestRewardCash);
            ResetFeedCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdateGiveFeedQuestUI();
        }
    }


    // ======================================================================================================================

    // Purchase Cats 퀘스트 초기 설정
    private void InitializePurchaseCatsQuest()
    {
        purchaseCatsRewardButton.onClick.AddListener(ReceivePurchaseCatsReward);
        purchaseCatsRewardButton.interactable = false;
        purchaseCatsPlusCashText.text = $"+{purchaseCatsQuestRewardCash}";
    }

    // 고양이 구매 퀘스트 UI를 업데이트하는 함수
    private void UpdatePurchaseCatsQuestUI()
    {
        int currentCount = PurchaseCatsCount;

        // 목표가 10 이상이면 퀘스트 종료 상태 처리
        if (purchaseCatsTargetCount >= 10)
        {
            purchaseCatsQuestSlider.value = purchaseCatsQuestSlider.maxValue;
            purchaseCatsCountText.text = "Complete";
            purchaseCatsRewardButton.interactable = false;
            purchaseCatsRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        purchaseCatsQuestSlider.maxValue = purchaseCatsTargetCount;
        purchaseCatsQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        purchaseCatsCountText.text = $"{currentCount}/{purchaseCatsTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= purchaseCatsTargetCount;
        purchaseCatsRewardButton.interactable = isComplete;
        purchaseCatsRewardDisabledBG.SetActive(!isComplete);
    }

    // 고양이 구매 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceivePurchaseCatsReward()
    {
        int currentCount = PurchaseCatsCount;

        // 목표가 10 이상인 경우 더 이상 보상 지급 불가
        if (purchaseCatsTargetCount >= 10)
        {
            isPurchaseCatsQuestComplete = true;
            return;
        }

        if (currentCount >= purchaseCatsTargetCount)
        {
            int excessCount = currentCount - purchaseCatsTargetCount;

            // 목표 고양이 구매 횟수 증가
            purchaseCatsTargetCount += increasePurchaseCatsTargetCount;

            // 퀘스트 완료 처리
            AddCash(purchaseCatsQuestRewardCash);
            ResetPurchaseCatsCount(excessCount);
            GameManager.Instance.UpdateCashText();
            UpdatePurchaseCatsQuestUI();
        }
    }

    // ======================================================================================================================



    // ======================================================================================================================

    // 퀘스트 초기 설정에서 Special Reward 버튼 초기화
    private void InitializeSpecialReward()
    {
        specialRewardButton.onClick.AddListener(ReceiveSpecialReward);
        specialRewardButton.interactable = false;
        specialRewardDisabledBG.SetActive(true);
        specialRewardPlusCashText.text = $"+{specialRewardQuestRewardCash}";
    }

    // 모든 퀘스트가 완료되었는지 확인하는 함수
    private bool AllQuestsCompleted()
    {
        if (isGiveFeedQuestComplete && isMergeQuestComplete && isPlayTimeQuestComplete && isPurchaseCatsQuestComplete)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Special Reward 활성화 상태를 업데이트하는 함수
    private void UpdateSpecialRewardUI()
    {
        if (AllQuestsCompleted())
        {
            isSpecialRewardActive = true;
            specialRewardDisabledBG.SetActive(false);
            specialRewardButton.interactable = true;
        }
    }

    // Special Reward 보상 지급 함수
    private void ReceiveSpecialReward()
    {
        if (isSpecialRewardActive)
        {
            // 보상 처리 로직
            AddCash(specialRewardQuestRewardCash);

            // 보상 지급 후 버튼만 비활성화 상태로 돌아감
            specialRewardRewardDisabledBG.SetActive(true);
            specialRewardButton.interactable = false;
            isSpecialRewardActive = false;

            // 퀘스트 UI 업데이트
            UpdateSpecialRewardUI();
            GameManager.Instance.UpdateCashText();
        }
    }

}

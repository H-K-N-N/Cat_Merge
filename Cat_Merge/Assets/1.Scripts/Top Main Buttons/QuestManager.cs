using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    private GameManager gameManager;                                    // GameManager

    [Header("---[QuestManager]")]
    [SerializeField] private ScrollRect questScrollRect;                // 퀘스트의 스크롤뷰
    [SerializeField] private Button questButton;                        // 퀘스트 버튼
    [SerializeField] private Image questButtonImage;                    // 퀘스트 버튼 이미지
    [SerializeField] private GameObject questMenuPanel;                 // 퀘스트 메뉴 Panel
    [SerializeField] private Button questBackButton;                    // 퀘스트 뒤로가기 버튼
    private bool isQuestMenuOpen;                                       // 퀘스트 메뉴 Panel의 활성화 상태

    // ======================================================================================================================

    [Header("---[Give Feed Quest UI]")]
    [SerializeField] private Slider giveFeedQuestSlider;                // 고양이 스폰 Slider
    [SerializeField] private TextMeshProUGUI giveFeedCountText;         // "?/?" 텍스트
    [SerializeField] private Button giveFeedRewardButton;               // 스폰 보상 버튼
    [SerializeField] private TextMeshProUGUI giveFeedPlusCashText;      // 스폰 보상 재화 개수 Text
    [SerializeField] private GameObject giveFeedRewardDisabledBG;       // 스폰 보상 버튼 비활성화 BG
    private int giveFeedTargetCount = 1;                                // 목표 스폰 횟수
    private int increaseGiveFeedTargetCount = 2;                        // 목표 스폰 횟수 증가치
    private int giveFeedQuestRewardCash = 5;                            // 스폰 퀘스트 보상 캐쉬 재화 개수
    private bool isGiveFeedQuestComplete = false;                       // 스폰 퀘스트 완료 여부

    [Header("---[Combine Quest UI]")]
    [SerializeField] private Slider combineQuestSlider;                 // 머지 Slider
    [SerializeField] private TextMeshProUGUI combineCountText;          // "?/?" 텍스트
    [SerializeField] private Button combineRewardButton;                // 머지 보상 버튼
    [SerializeField] private TextMeshProUGUI combinePlusCashText;       // 머지 보상 재화 개수 Text
    [SerializeField] private GameObject combineRewardDisabledBG;        // 머지 보상 버튼 비활성화 BG
    private int combineTargetCount = 1;                                 // 목표 머지 횟수
    private int increaseCombineTargetCount = 2;                         // 목표 머지 횟수 증가치
    private int combineQuestRewardCash = 5;                             // 머지 퀘스트 보상 캐쉬 재화 개수
    private bool isCombineQuestComplete = false;                        // 머지 퀘스트 완료 여부

    [Header("---[Cat GetCoin UI]")]
    [SerializeField] private Slider getCoinQuestSlider;                 // 획득코인 Slider
    [SerializeField] private TextMeshProUGUI getCoinCountText;          // "?/?" 텍스트
    [SerializeField] private Button getCoinRewardButton;                // 획득코인 보상 버튼
    [SerializeField] private TextMeshProUGUI getCoinPlusCashText;       // 획득코인 보상 재화 개수 Text
    [SerializeField] private GameObject getCoinRewardDisabledBG;        // 획득코인 보상 버튼 비활성화 BG
    private int getCoinTargetCount = 1;                                 // 목표 획득코인 횟수
    private int increaseGetCoinTargetCount = 2;                         // 목표 획득코인 갯수 증가치
    private int getCoinQuestRewardCash = 5;                             // 획득코인 퀘스트 보상 캐쉬 재화 개수
    private bool isGetCoinQuestComplete = false;                        // 획득코인 퀘스트 완료 여부

    [Header("---[PlayTime Quest UI]")]
    [SerializeField] private Slider playTimeQuestSlider;                // 플레이타임 Slider
    [SerializeField] private TextMeshProUGUI playTimeCountText;         // "?/?" 텍스트
    [SerializeField] private Button playTimeRewardButton;               // 플레이타임 보상 버튼
    [SerializeField] private TextMeshProUGUI playTimePlusCashText;      // 플레이타임 보상 재화 개수 Text
    [SerializeField] private GameObject playTimeRewardDisabledBG;       // 플레이타임 보상 버튼 비활성화 BG
    private int playTimeTargetCount = 10;                               // 목표 플레이타임 (초 단위)
    private int increasePlayTimeTargetCount = 20;                       // 목표 플레이타임 증가치
    private int playTimeQuestRewardCash = 5;                            // 플레이타임 퀘스트 보상 캐쉬 재화 개수
    private bool isPlayTimeQuestComplete = false;                       // 플레이타임 퀘스트 완료 여부

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
    private int specialRewardQuestRewardCash = 50;                      // Special Reward 퀘스트 보상 캐쉬 재화 개수
    private bool isSpecialRewardActive = false;                         // Special Reward 활성화 상태

    // ======================================================================================================================

    private void Start()
    {
        gameManager = GameManager.Instance;
        questMenuPanel.SetActive(false);

        InitializeQuestButton();

        InitializeGiveFeedQuest();
        InitializeCombineQuest();
        InitializeGetCoinQuest();
        InitializePlayTimeQuest();
        InitializePurchaseCatsQuest();
        InitializeSpecialReward();

        ResetScrollPositions();
    }

    private void Update()
    {
        UpdateGiveFeedQuestUI();
        UpdateCombineQuestUI();
        UpdateGetCoinQuestUI();
        UpdatePlayTimeQuestUI();
        UpdatePurchaseCatsQuestUI();

        UpdateSpecialRewardUI();
    }

    // ======================================================================================================================

    // QuestButton 설정
    private void InitializeQuestButton()
    {
        questButton.onClick.AddListener(ToggleQuestMenuPanel);
        questBackButton.onClick.AddListener(CloseDictionaryMenuPanel);
    }

    // 초기 스크롤 위치 초기화 함수
    private void ResetScrollPositions()
    {
        questScrollRect.verticalNormalizedPosition = 1f;
    }

    // 퀘스트 Panel 열고 닫는 함수
    private void ToggleQuestMenuPanel()
    {
        isQuestMenuOpen = !isQuestMenuOpen;
        questMenuPanel.SetActive(isQuestMenuOpen);
        UpdateButtonColor(questButtonImage, isQuestMenuOpen);
    }

    // 퀘스트 Panel 닫는 함수 (questBackButton)
    private void CloseDictionaryMenuPanel()
    {
        isQuestMenuOpen = false;
        questMenuPanel.SetActive(false);
        UpdateButtonColor(questButtonImage, false);
    }

    // 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? "#5f5f5f" : "#E2E2E2";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
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
        int currentCount = gameManager.FeedCount;

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
        int currentCount = gameManager.FeedCount;

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
            gameManager.AddCash(giveFeedQuestRewardCash);
            gameManager.ResetFeedCount(excessCount);
            gameManager.UpdateCashText();
            UpdateGiveFeedQuestUI();
        }
    }

    // ======================================================================================================================

    // Combine 퀘스트 초기 설정
    private void InitializeCombineQuest()
    {
        combineRewardButton.onClick.AddListener(ReceiveCombineReward);
        combineRewardButton.interactable = false;
        combinePlusCashText.text = $"+{combineQuestRewardCash}";
    }

    // 머지 퀘스트 UI를 업데이트하는 함수
    private void UpdateCombineQuestUI()
    {
        int currentCount = gameManager.CombineCount;

        // 목표가 10 이상이면 퀘스트 종료 상태 처리
        if (combineTargetCount >= 10)
        {
            combineQuestSlider.value = combineQuestSlider.maxValue;
            combineCountText.text = "Complete";
            combineRewardButton.interactable = false;
            combineRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        combineQuestSlider.maxValue = combineTargetCount;
        combineQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        combineCountText.text = $"{currentCount}/{combineTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= combineTargetCount;
        combineRewardButton.interactable = isComplete;
        combineRewardDisabledBG.SetActive(!isComplete);
    }

    // 머지 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveCombineReward()
    {
        int currentCount = gameManager.CombineCount;

        // 목표가 10 이상인 경우 더 이상 보상 지급 불가
        if (combineTargetCount >= 10)
        {
            isCombineQuestComplete = true;
            return;
        }

        if (currentCount >= combineTargetCount)
        {
            // 초과된 횟수 계산
            int excessCount = currentCount - combineTargetCount;

            // 목표 스폰 횟수 증가 (퀘스트 난이도 상승)
            combineTargetCount += increaseCombineTargetCount;

            // 퀘스트 완료 처리
            gameManager.AddCash(combineQuestRewardCash);
            gameManager.ResetCombineCount(excessCount);
            gameManager.UpdateCashText();
            UpdateCombineQuestUI();
        }
    }

    // ======================================================================================================================

    // GetCoin 퀘스트 초기 설정
    private void InitializeGetCoinQuest()
    {
        getCoinRewardButton.onClick.AddListener(ReceiveGetCoinReward);
        getCoinRewardButton.interactable = false;
        getCoinPlusCashText.text = $"+{getCoinQuestRewardCash}";
    }

    // 획득코인 퀘스트 UI를 업데이트하는 함수
    private void UpdateGetCoinQuestUI()
    {
        int currentCount = gameManager.GetCoinCount;

        // 목표가 10 이상이면 퀘스트 종료 상태 처리
        if (getCoinTargetCount >= 10)
        {
            getCoinQuestSlider.value = getCoinQuestSlider.maxValue;
            getCoinCountText.text = "Complete";
            getCoinRewardButton.interactable = false;
            getCoinRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        getCoinQuestSlider.maxValue = getCoinTargetCount;
        getCoinQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        getCoinCountText.text = $"{currentCount}/{getCoinTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= getCoinTargetCount;
        getCoinRewardButton.interactable = isComplete;
        getCoinRewardDisabledBG.SetActive(!isComplete);
    }

    // 획득코인 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveGetCoinReward()
    {
        int currentCount = gameManager.GetCoinCount;

        // 목표가 10 이상인 경우 더 이상 보상 지급 불가
        if (getCoinTargetCount >= 10)
        {
            isGetCoinQuestComplete = true;
            return;
        }

        if (currentCount >= getCoinTargetCount)
        {
            // 초과된 횟수 계산
            int excessCount = currentCount - getCoinTargetCount;

            // 목표 획득코인 횟수 증가 (퀘스트 난이도 상승)
            getCoinTargetCount += increaseGetCoinTargetCount;

            // 퀘스트 완료 처리
            gameManager.AddCash(getCoinQuestRewardCash);
            gameManager.ResetGetCoinCount(excessCount);
            gameManager.UpdateCashText();
            UpdateGetCoinQuestUI();
        }
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
        int currentTime = (int)gameManager.PlayTimeCount;

        // 목표가 200 이상이면 퀘스트 종료 상태 처리
        if (playTimeTargetCount >= 200)
        {
            playTimeQuestSlider.value = playTimeQuestSlider.maxValue;
            playTimeCountText.text = "Complete";
            playTimeRewardButton.interactable = false;
            playTimeRewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        playTimeQuestSlider.maxValue = playTimeTargetCount;
        playTimeQuestSlider.value = currentTime;

        // "?/?" 텍스트 업데이트
        playTimeCountText.text = $"{currentTime}/{playTimeTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentTime >= playTimeTargetCount;
        playTimeRewardButton.interactable = isComplete;
        playTimeRewardDisabledBG.SetActive(!isComplete);
    }

    // 플레이타임 퀘스트 보상 버튼 클릭 시 호출되는 함수
    private void ReceivePlayTimeReward()
    {
        int currentTime = (int)gameManager.PlayTimeCount;

        // 목표가 200 이상인 경우 더 이상 보상 지급 불가
        if (playTimeTargetCount >= 200)
        {
            isPlayTimeQuestComplete = true;
            return;
        }

        if (currentTime >= playTimeTargetCount)
        {
            int excessTime = currentTime - playTimeTargetCount;

            // 목표 플레이타임 증가
            playTimeTargetCount += increasePlayTimeTargetCount;

            // 퀘스트 완료 처리
            gameManager.AddCash(playTimeQuestRewardCash);
            gameManager.ResetPlayTime(excessTime);
            gameManager.UpdateCashText();
            UpdatePlayTimeQuestUI();
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
        int currentCount = gameManager.PurchaseCatsCount;

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
        int currentCount = gameManager.PurchaseCatsCount;

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
            gameManager.AddCash(purchaseCatsQuestRewardCash);
            gameManager.ResetPurchaseCatsCount(excessCount);
            gameManager.UpdateCashText();
            UpdatePurchaseCatsQuestUI();
        }
    }

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
        if (isGiveFeedQuestComplete && isCombineQuestComplete && isGetCoinQuestComplete && isPlayTimeQuestComplete && isPurchaseCatsQuestComplete)
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
            gameManager.AddCash(specialRewardQuestRewardCash);

            // 보상 지급 후 버튼만 비활성화 상태로 돌아감
            specialRewardRewardDisabledBG.SetActive(true);
            specialRewardButton.interactable = false;
            isSpecialRewardActive = false;

            // 퀘스트 UI 업데이트
            UpdateSpecialRewardUI();
            gameManager.UpdateCashText();
        }
    }

    // ======================================================================================================================


}

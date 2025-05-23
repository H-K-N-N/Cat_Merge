using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// 상점 스크립트
[DefaultExecutionOrder(-5)]
public class ShopManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static ShopManager Instance { get; private set; }

    [Header("---[Shop]")]
    [SerializeField] private Button shopButton;                     // 상점 버튼
    [SerializeField] private Image shopButtonImg;                   // 상점 버튼 이미지
    [SerializeField] private GameObject shopPanel;                  // 상점 메뉴 패널
    [SerializeField] private Button shopBackButton;                 // 상점 메뉴 뒤로가기 버튼
    [SerializeField] private GameObject shopNewImage;               // 상점 버튼 New 이미지 오브젝트

    private ActivePanelManager activePanelManager;                  // ActivePanelManager

    private bool isWaitingForAd = false;                            // 광고 대기 중인지 확인하는 변수 추가


    [Header("---[기본적인 쿨타임들]")]
    private const long DEFAULT_CASH_FOR_TIME_COOLTIME = 600;            // 무료 캐쉬 획득 쿨타임 (CashForTime) (600초)
    private const long DEFAULT_CASH_FOR_AD_COOLTIME = 600;              // 광고 캐쉬 획득 쿨타임 (CashForAd) (600초)
    private const long DEFAULT_DOUBLE_COIN_FOR_AD_COOLTIME = 600;       // 코인 2배 획득 쿨타임  (DoubleCoinForAd) (600초)

    [Header("---[Cash For Time]")]
    [SerializeField] private Button cashForTimeRewardButton;            // 쿨타임마다 활성화되는 무료 캐쉬 획득 (무료 캐쉬 획득 불가능 상태일때 비활성화 = interactable 비활성화)
    [SerializeField] private TextMeshProUGUI cashForTimeNameText;       // Item Name Text
    [SerializeField] private TextMeshProUGUI cashForTimeCoolTimeText;   // 광고 시청 쿨타임 Text
    [SerializeField] private TextMeshProUGUI cashForTimeInformationText;// Item Information Text
    [SerializeField] private GameObject cashForTimeNewImage;            // 무료 캐쉬 획득 가능 상태일때 활성화되는 New 이미지 오브젝트
    [SerializeField] private GameObject cashForTimeDisabledBG;          // 무료 캐쉬 획득 불가능 상태일때 활성화되는 이미지 오브젝트
    private long cashForTimeCoolTime = DEFAULT_CASH_FOR_TIME_COOLTIME;  // 무료 캐쉬 획득 쿨타임(게임이 종료되도 시간이 흐르도록 실제 시간을 바탕으로 계산)
    private long lastTimeReward = 0;                                    // 마지막 cashForTime 보상 시간 저장
    private long passiveCashForTimeCoolTimeReduction = 0;               // 패시브로 인한 cashForTime 쿨타임 감소량
    private int cashForTimePrice = 5;                                   // cashForTime 보상
    private int passiveCashForTimeAmount = 0;                           // 패시브로 인한 cashForTime 추가 무료 캐쉬 획득량
    public int CashForTimePrice => cashForTimePrice + passiveCashForTimeAmount;


    [Header("---[Cash For Ad]")]
    [SerializeField] private Button cashForAdRewardButton;              // 광고 시청으로 캐쉬 획득 (광고 시청 불가능 상태일때 비활성화 = interactable 비활성화)
    [SerializeField] private TextMeshProUGUI cashForAdNameText;         // Item Name Text
    [SerializeField] private TextMeshProUGUI cashForAdCoolTimeText;     // 광고 시청 쿨타임 Text
    [SerializeField] private TextMeshProUGUI cashForAdInformationText;  // Item Information Text
    [SerializeField] private GameObject cashForAdRewardOnImage;         // 광고 시청 가능 상태일때 활성화되는 이미지 오브젝트
    [SerializeField] private GameObject cashForAdRewardOffImage;        // 광고 시청 불가능 상태일때 활성화되는 이미지 오브젝트
    [SerializeField] private GameObject cashForAdNewImage;              // 광고 시청 가능 상태일때 활성화되는 New 이미지 오브젝트
    [SerializeField] private GameObject cashForAdDisabledBG;            // 광고 시청 불가능 상태일때 활성화되는 이미지 오브젝트
    private long cashForAdCoolTime = DEFAULT_CASH_FOR_AD_COOLTIME;      // 광고 시청 쿨타임 (게임이 종료되도 시간이 흐르도록 실제 시간을 바탕으로 계산)
    private long lastAdTimeReward = 0;                                  // 마지막 cashForAd 보상 시간 저장
    private long passiveCashForAdCoolTimeReduction = 0;                 // 패시브로 인한 cashForAd 쿨타임 감소량
    private int cashForAdPrice = 30;                                    // cashForAd 보상
    private int passiveCashForAdAmount = 0;                             // 패시브로 인한 cashForAd 추가 광고 캐쉬 획득량
    public int CashForAdPrice => cashForAdPrice + passiveCashForAdAmount;


    [Header("---[Double Coin For Ad]")]
    [SerializeField] private Button doubleCoinForAdButton;                      // 광고 시청으로 코인 획득량 증가 효과 획득
    [SerializeField] private TextMeshProUGUI doubleCoinForAdNameText;           // Item Name Text
    [SerializeField] private TextMeshProUGUI doubleCoinForAdCoolTimeText;       // 광고 시청 쿨타임 Text
    [SerializeField] private GameObject doubleCoinForAdRewardOnImage;           // 광고 시청 가능 상태일때 활성화되는 이미지 오브젝트
    [SerializeField] private GameObject doubleCoinForAdRewardOffImage;          // 광고 시청 불가능 상태일때 활성화되는 이미지 오브젝트
    [SerializeField] private GameObject doubleCoinForAdNewImage;                // 광고 시청 가능 상태일때 활성화되는 New 이미지 오브젝트
    [SerializeField] private GameObject doubleCoinForAdDisabledBG;              // 광고 시청 불가능 상태일때 활성화되는 이미지 오브젝트
    private long doubleCoinForAdCoolTime = DEFAULT_DOUBLE_COIN_FOR_AD_COOLTIME; // 코인 획득량 쿨타임 (게임이 종료되도 시간이 흐르도록 실제 시간을 바탕으로 계산)
    private long lastDoubleCoinTimeReward = 0;                                  // 마지막 doubleCoinForAd 보상 시간 저장
    private long passiveDoubleCoinForAdCoolTimeReduction = 0;                   // 패시브로 인한 doubleCoinForAd 쿨타임 감소량
    
    private const float doubleCoinDuration = 300f;                          // 코인 2배 지속시간 (300초)
    private const float doubleCoinMultiplier = 2f;                          // 코인 획득량 배수
    private float coinMultiplier = 1f;                                      // 현재 코인 배수
    private float multiplierEndTime = 0f;                                   // 배수 효과 종료 시간
    private float passiveDoubleCoinDurationIncrease = 0f;                   // 패시브로 인한 효과지속시간 증가량
    private float CurrentDoubleCoinDuration => doubleCoinDuration + passiveDoubleCoinDurationIncrease;


    [Header("---[Diamond Shop]")]
    [SerializeField] private Button buy100CashToCoinButton;                 // 100다이아 상품 버튼
    [SerializeField] private Button buy500CashToCoinButton;                 // 500다이아 상품 버튼
    [SerializeField] private Button buy2500CashToCoinButton;                // 2500다이아 상품 버튼
    private int selectedCashAmount = 0;                                     // 선택된 다이아 수량

    [SerializeField] private Image cantBuy100CashToCoinImage;               // 100다이아 상품 해금 이미지
    [SerializeField] private Image cantBuy500CashToCoinImage;               // 500다이아 상품 해금 이미지
    [SerializeField] private Image cantBuy2500CashToCoinImage;              // 2500다이아 상품 해금 이미지
    private const int UNLOCK_GRADE_100_CASH = 5;                            // 100다이아 상품 해금 등급
    private const int UNLOCK_GRADE_500_CASH = 10;                           // 500다이아 상품 해금 등급
    private const int UNLOCK_GRADE_2500_CASH = 15;                          // 2500다이아 상품 해금 등급

    [SerializeField] private GameObject buyCashToCoinPanel;                 // 상품 구매 확인 Panel
    [SerializeField] private TextMeshProUGUI buyCashToCoinText;             // 상품 구매 확인 Text
    [SerializeField] private Button closeBuyCashToCoinPanelButton;          // 상품 구매 패널 닫기 버튼
    [SerializeField] private Button buyCashToCoinButton;                    // 상품 구매 Yes 버튼
    private decimal calculatedCoinAmount = 0;                               // 계산된 젤리 수량


    [Header("---[ETC]")]
    private float battlePauseTime = 0f;                                     // 전투 중 멈춘 시간
    private bool isBattlePaused = false;                                    // 전투 중 멈춤 상태

    private readonly WaitForSeconds oneSecondWait = new WaitForSeconds(1f);

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
        InitializeShopManager();
        UpdateAllUI();
        UpdateDiamondShopUnlockState();
        StartCoroutine(CheckRewardStatus());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    #endregion


    #region Initialize

    // 기본 Shop 초기화 함수
    private void InitializeShopManager()
    {
        shopPanel.SetActive(false);

        InitializeActivePanel();
        InitializeButtonListeners();
    }

    // ActivePanel 초기화 함수
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("Shop", shopPanel, shopButtonImg);

        shopButton.onClick.AddListener(() => activePanelManager.TogglePanel("Shop"));
        shopBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("Shop"));
    }

    // 전체 UI 업데이트 함수
    private void UpdateAllUI()
    {
        UpdateCashForAdUI();
        UpdateCashForTimeUI();

        UpdateDoubleCoinForAdUI();

        UpdateShopNewImage();
    }

    // 버튼 리스너 초기화 함수
    private void InitializeButtonListeners()
    {
        cashForAdRewardButton.onClick.AddListener(OnClickCashForAd);
        cashForTimeRewardButton.onClick.AddListener(OnClickCashForTime);

        doubleCoinForAdButton.onClick.AddListener(OnClickDoubleCoinForAd);

        buy100CashToCoinButton.onClick.AddListener(() => OnClickBuyCashToCoin(100));
        buy500CashToCoinButton.onClick.AddListener(() => OnClickBuyCashToCoin(500));
        buy2500CashToCoinButton.onClick.AddListener(() => OnClickBuyCashToCoin(2500));
        closeBuyCashToCoinPanelButton.onClick.AddListener(CloseBuyCashToCoinPanel);
        buyCashToCoinButton.onClick.AddListener(OnConfirmBuyCashToCoin);
    }

    // New 이미지 상태 업데이트 함수
    private void UpdateShopNewImage()
    {
        bool isAnyNewImageActive = cashForAdNewImage.activeSelf || cashForTimeNewImage.activeSelf || doubleCoinForAdNewImage.activeSelf;
        shopNewImage.SetActive(isAnyNewImageActive);
    }

    #endregion


    #region Free Shop System

    // CashForTime UI 업데이트 함수
    private void UpdateCashForTimeUI()
    {
        bool isTimeRewardAvailable = IsCashForTimeAvailable();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));

        // 버튼 상태 업데이트
        cashForTimeRewardButton.interactable = isTimeRewardAvailable;
        cashForTimeNewImage.SetActive(isTimeRewardAvailable);
        cashForTimeDisabledBG.SetActive(!isTimeRewardAvailable);

        // 쿨타임 텍스트 업데이트
        if (isTimeRewardAvailable)
        {
            cashForTimeCoolTimeText.text = "준비 완료";
        }
        else
        {
            cashForTimeCoolTimeText.text = $"쿨타임 {(int)remainTime}초";
        }

        // 보상 관련 텍스트 업데이트
        if (cashForTimeNameText != null)
        {
            cashForTimeNameText.text = $"무료 다이아 {CashForTimePrice}개";
        }
        if (cashForTimeInformationText != null)
        {
            cashForTimeInformationText.text = $"무료 다이아 {CashForTimePrice}개 받기";
        }
    }

    // CashForTime 활성화 여부 판별 함수
    private bool IsCashForTimeAvailable()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return (currentTime - lastTimeReward) >= cashForTimeCoolTime;
    }

    // CashForTime 버튼 클릭시 실행되는 함수
    private void OnClickCashForTime()
    {
        if (!IsCashForTimeAvailable()) return;

        // 시간 보상 지급
        GameManager.Instance.Cash += CashForTimePrice;

        // 마지막 보상 시간 저장
        lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        UpdateCashForTimeUI();
    }

    // 패시브로 인한 CashForTime의 캐쉬 재화 추가 함수
    public void AddPassiveCashForTimeAmount(int amount)
    {
        passiveCashForTimeAmount += amount;

        UpdateCashForTimeUI();
    }

    // 패시브로 인한 CashForTime의 쿨타임 감소 함수
    public void AddPassiveCashForTimeCoolTimeReduction(long seconds)
    {
        passiveCashForTimeCoolTimeReduction += seconds;
        cashForTimeCoolTime = DEFAULT_CASH_FOR_TIME_COOLTIME - passiveCashForTimeCoolTimeReduction;

        UpdateCashForTimeUI();
    }

    #endregion


    #region Ad Shop System

    // 주기적으로 보상 상태를 체크하는 코루틴
    private IEnumerator CheckRewardStatus()
    {
        while (true)
        {
            UpdateAllUI();

            yield return oneSecondWait;
        }
    }

    // CashForAd UI 업데이트 함수
    private void UpdateCashForAdUI()
    {
        bool isAdAvailable = IsCashForAdAvailable() && !isWaitingForAd;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, cashForAdCoolTime - (currentTime - lastAdTimeReward));

        cashForAdRewardButton.interactable = isAdAvailable;
        cashForAdRewardOnImage.SetActive(isAdAvailable);
        cashForAdRewardOffImage.SetActive(!isAdAvailable);
        cashForAdNewImage.SetActive(isAdAvailable);
        cashForAdDisabledBG.SetActive(!isAdAvailable);

        // 쿨타임 텍스트 업데이트
        if (isAdAvailable)
        {
            cashForAdCoolTimeText.text = "준비 완료";
        }
        else
        {
            cashForAdCoolTimeText.text = $"쿨타임 {(int)remainTime}초";
        }

        // 보상 관련 텍스트 업데이트
        if (cashForAdNameText != null)
        {
            cashForAdNameText.text = $"광고 다이아 {CashForAdPrice}개";
        }
        if (cashForAdInformationText != null)
        {
            cashForAdInformationText.text = $"광고 시청 후 다이아 {CashForAdPrice}개 받기";
        }
    }

    // CashForTime 활성화 여부 판별 함수
    private bool IsCashForAdAvailable()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return (currentTime - lastAdTimeReward) >= cashForAdCoolTime;
    }

    // CashForAd 버튼 클릭시 실행되는 함수
    private void OnClickCashForAd()
    {
        if (!IsCashForAdAvailable() || isWaitingForAd) return;

        isWaitingForAd = true;
        cashForAdRewardButton.interactable = false;

        GetComponent<GoogleAdsManager>()?.ShowRewardedCashForAd();
    }

    // CashForAd 광고 시청 완료 시 실행되는 함수
    public void OnCashForAdRewardComplete()
    {
        // 광고 보상 지급
        GameManager.Instance.Cash += CashForAdPrice;

        // 마지막 광고 시청 시간 저장
        lastAdTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        isWaitingForAd = false;

        UpdateCashForAdUI();
    }

    // 패시브로 인한 CashForAd의 캐쉬 재화 추가 함수
    public void AddPassiveCashForAdAmount(int amount)
    {
        passiveCashForAdAmount += amount;
        UpdateCashForAdUI();
    }

    // 패시브로 인한 CashForAd의 쿨타임 감소 함수
    public void AddPassiveCashForAdCoolTimeReduction(long seconds)
    {
        passiveCashForAdCoolTimeReduction += seconds;
        cashForAdCoolTime = DEFAULT_CASH_FOR_AD_COOLTIME - passiveCashForAdCoolTimeReduction;

        UpdateCashForTimeUI();
    }


    // DoubleCoinForAd UI 업데이트 함수
    private void UpdateDoubleCoinForAdUI()
    {
        bool isAdAvailable = IsDoubleCoinForAdAvailable() && !isWaitingForAd;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, doubleCoinForAdCoolTime - (currentTime - lastDoubleCoinTimeReward));
        float remainEffectTime = GetRemainingEffectTime();

        doubleCoinForAdButton.interactable = isAdAvailable;
        doubleCoinForAdRewardOnImage.SetActive(isAdAvailable);
        doubleCoinForAdRewardOffImage.SetActive(!isAdAvailable);
        doubleCoinForAdNewImage.SetActive(isAdAvailable);
        doubleCoinForAdDisabledBG.SetActive(!isAdAvailable);

        if (doubleCoinForAdNameText != null)
        {
            doubleCoinForAdNameText.text = $"{CurrentDoubleCoinDuration}초 동안 재화 수급량 {doubleCoinMultiplier}배";
        }

        // 쿨타임 텍스트 업데이트
        if (remainEffectTime > 0)
        {
            doubleCoinForAdCoolTimeText.text = $"효과 지속시간 {(int)remainEffectTime}초";
        }
        else if (isAdAvailable)
        {
            doubleCoinForAdCoolTimeText.text = "준비 완료";
        }
        else
        {
            doubleCoinForAdCoolTimeText.text = $"쿨타임 {(int)remainTime}초";
        }
    }

    // 현재 코인 배수를 가져오는 함수
    public float CurrentCoinMultiplier
    {
        get
        {
            if (Time.time < multiplierEndTime)
            {
                return coinMultiplier;
            }
            else
            {
                // 효과가 끝났을 때 초기화
                if (coinMultiplier != 1f)
                {
                    coinMultiplier = 1f;
                    multiplierEndTime = 0f;
                    isBattlePaused = false;
                    battlePauseTime = 0f;
                    UpdateDoubleCoinForAdUI();
                }
                return 1f;
            }
        }
    }

    // 배수 효과 남은 시간을 가져오는 함수
    public float GetRemainingEffectTime()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive)
        {
            if (!isBattlePaused)
            {
                isBattlePaused = true;
                battlePauseTime = Time.time;
            }
            return Mathf.Max(0f, multiplierEndTime - battlePauseTime);
        }
        else
        {
            if (isBattlePaused)
            {
                isBattlePaused = false;
                float pausedDuration = Time.time - battlePauseTime;
                multiplierEndTime += pausedDuration;
            }
            return Mathf.Max(0f, multiplierEndTime - Time.time);
        }
    }

    // DoubleCoinForAd 활성화 여부 판별 함수
    private bool IsDoubleCoinForAdAvailable()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return (currentTime - lastDoubleCoinTimeReward) >= doubleCoinForAdCoolTime;
    }

    // DoubleCoinForAd 버튼 클릭시 실행되는 함수
    private void OnClickDoubleCoinForAd()
    {
        if (!IsDoubleCoinForAdAvailable() || isWaitingForAd) return;

        isWaitingForAd = true;
        doubleCoinForAdButton.interactable = false;

        GetComponent<GoogleAdsManager>()?.ShowRewardedDoubleCoinForAd();
    }

    // DoubleCoinForAd 광고 시청 완료 시 실행되는 함수
    public void OnDoubleCoinAdRewardComplete()
    {
        if (isWaitingForAd)
        {
            // 기존 효과 지속시간 계산
            float remainingTime = GetRemainingEffectTime();

            // 광고 보상 지급 - 모든 고양이의 재화 수급량 지정된 시간동안 2배로 증가
            coinMultiplier = doubleCoinMultiplier;

            // 기존 효과 지속시간이 있으면 그 시간에 새로운 지속시간을 더함
            if (remainingTime > 0)
            {
                multiplierEndTime = Time.time + remainingTime + CurrentDoubleCoinDuration;
            }
            else
            {
                multiplierEndTime = Time.time + CurrentDoubleCoinDuration;
            }

            // 전투 관련 상태 초기화
            isBattlePaused = false;
            battlePauseTime = 0f;

            // 마지막 광고 시청 시간 저장
            lastDoubleCoinTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            isWaitingForAd = false;

            UpdateDoubleCoinForAdUI();
        }
    }

    // 패시브로 인한 DoubleCoinForAd 효과 지속시간 증가 함수
    public void AddPassiveDoubleCoinDurationIncrease(float seconds)
    {
        passiveDoubleCoinDurationIncrease += seconds;

        UpdateDoubleCoinForAdUI();
    }

    // 패시브로 인한 DoubleCoinForAd 쿨타임 감소 함수
    public void AddPassiveDoubleCoinForAdCoolTimeReduction(long seconds)
    {
        passiveDoubleCoinForAdCoolTimeReduction += seconds;
        doubleCoinForAdCoolTime = DEFAULT_DOUBLE_COIN_FOR_AD_COOLTIME - passiveDoubleCoinForAdCoolTimeReduction;

        UpdateDoubleCoinForAdUI();
    }

    public void ResetAdState()
    {
        if (isWaitingForAd)
        {
            isWaitingForAd = false;

            // 버튼 상태 복구
            cashForAdRewardButton.interactable = IsCashForAdAvailable();
            doubleCoinForAdButton.interactable = IsDoubleCoinForAdAvailable();

            UpdateAllUI();
        }
    }

    #endregion


    #region Diamond Shop System

    // 다이아 상점 해금 상태 업데이트 함수
    public void UpdateDiamondShopUnlockState()
    {
        int maxUnlockedGrade = GetMaxUnlockedCatGrade();

        // 100 다이아 상품
        UpdateShopItemUnlockState(buy100CashToCoinButton, cantBuy100CashToCoinImage, maxUnlockedGrade >= UNLOCK_GRADE_100_CASH);

        // 500 다이아 상품
        UpdateShopItemUnlockState(buy500CashToCoinButton, cantBuy500CashToCoinImage, maxUnlockedGrade >= UNLOCK_GRADE_500_CASH);

        // 2500 다이아 상품
        UpdateShopItemUnlockState(buy2500CashToCoinButton, cantBuy2500CashToCoinImage, maxUnlockedGrade >= UNLOCK_GRADE_2500_CASH);
    }

    // 상점 아이템 해금 상태 업데이트 함수
    private void UpdateShopItemUnlockState(Button button, Image cantBuyImage, bool isUnlocked)
    {
        button.interactable = isUnlocked;
        cantBuyImage.gameObject.SetActive(!isUnlocked);
    }

    // 최대 해금 고양이 등급 가져오기 함수
    private int GetMaxUnlockedCatGrade()
    {
        for (int i = GameManager.Instance.AllCatData.Length - 1; i >= 0; i--)
        {
            if (DictionaryManager.Instance.IsCatUnlocked(i))
            {
                return i + 1;
            }
        }
        return 0;
    }

    // 젤리 교환 수치 계산 함수
    private decimal CalculateJellyAmount(int diamondAmount)
    {
        // 재화 수급 시간 가져오기 (ItemMenuManager의 reduceCollectingTimeLv 사용)
        var collectingTimeList = ItemFunctionManager.Instance.reduceCollectingTimeList;
        float collectingTime = collectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;

        // 최대 도감 해금 등급의 기본 재화 수급량 찾기
        decimal maxUnlockedCatCoin = 0;
        for (int i = GameManager.Instance.AllCatData.Length - 1; i >= 0; i--)
        {
            if (DictionaryManager.Instance.IsCatUnlocked(i))
            {
                maxUnlockedCatCoin = GameManager.Instance.AllCatData[i].CatGetCoin;
                break;
            }
        }

        // 배율 결정
        float multiplier = 1.0f;
        switch (diamondAmount)
        {
            case 100:
                multiplier = 1.2f;
                break;
            case 500:
                multiplier = 1.3f;
                break;
            case 2500:
                multiplier = 1.4f;
                break;
        }

        // 젤리 수량 계산: ((다이아 / 재화 수급 시간) * 최대 도감 해금 등급의 기본 재화 수급량) * 배율
        decimal jellyAmount = decimal.Round((diamondAmount / (decimal)collectingTime) * maxUnlockedCatCoin * (decimal)multiplier);
        return jellyAmount;
    }

    // 다이아 상품 구매 버튼 클릭시 실행되는 함수
    private void OnClickBuyCashToCoin(int diamondAmount)
    {
        // 해금 조건 체크
        int maxUnlockedGrade = GetMaxUnlockedCatGrade();
        bool canBuy = diamondAmount switch
        {
            100 => maxUnlockedGrade >= UNLOCK_GRADE_100_CASH,
            500 => maxUnlockedGrade >= UNLOCK_GRADE_500_CASH,
            2500 => maxUnlockedGrade >= UNLOCK_GRADE_2500_CASH,
            _ => false
        };

        if (!canBuy)
        {
            int requiredGrade = diamondAmount switch
            {
                100 => UNLOCK_GRADE_100_CASH,
                500 => UNLOCK_GRADE_500_CASH,
                2500 => UNLOCK_GRADE_2500_CASH,
                _ => 0
            };
            NotificationManager.Instance.ShowNotification($"{requiredGrade}등급 고양이 해금 시 구매 가능!!");
            return;
        }

        selectedCashAmount = diamondAmount;
        calculatedCoinAmount = CalculateJellyAmount(diamondAmount);

        UpdateBuyCashToCoinText();
        buyCashToCoinPanel.SetActive(true);
    }

    // 구매 확인 텍스트 업데이트 함수
    private void UpdateBuyCashToCoinText()
    {
        buyCashToCoinText.text = $"[{selectedCashAmount} 다이아]로\n젤리 [{GameManager.Instance.FormatNumber(calculatedCoinAmount)}]개를\n구매하시겠습니까?";
    }

    // 구매 확인 버튼 클릭시 실행되는 함수
    private void OnConfirmBuyCashToCoin()
    {
        if (GameManager.Instance.Cash < selectedCashAmount)
        {
            NotificationManager.Instance.ShowNotification("다이아가 부족합니다!!");
        }
        else
        {
            GameManager.Instance.Cash -= selectedCashAmount;
            GameManager.Instance.Coin += calculatedCoinAmount;
            NotificationManager.Instance.ShowNotification($"젤리 [{GameManager.Instance.FormatNumber(calculatedCoinAmount)}]개 구매 완료!!");
        }

        CloseBuyCashToCoinPanel();
    }

    // 구매 확인 패널 닫기 함수
    private void CloseBuyCashToCoinPanel()
    {
        buyCashToCoinPanel.SetActive(false);
        selectedCashAmount = 0;
        calculatedCoinAmount = 0;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public long lastAdTimeReward;               // 마지막 CashForAd 보상 받은 시간
        public long lastTimeReward;                 // 마지막 CasfFroTime 보상 받은 시간
        public long lastDoubleCoinTimeReward;       // 마지막 DoubleCoinForAd 보상 받은 시간
        public float multiplierEndTimeOffset;       // 재화 획득량 2배 효과 종료까지 남은 시간
    }

    public string GetSaveData()
    {
        float remainingTime = multiplierEndTime - Time.time;

        SaveData data = new SaveData
        {
            lastAdTimeReward = this.lastAdTimeReward,
            lastTimeReward = this.lastTimeReward,
            lastDoubleCoinTimeReward = this.lastDoubleCoinTimeReward,
            multiplierEndTimeOffset = remainingTime > 0 ? remainingTime : 0
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.lastAdTimeReward = savedData.lastAdTimeReward;
        this.lastTimeReward = savedData.lastTimeReward;
        this.lastDoubleCoinTimeReward = savedData.lastDoubleCoinTimeReward;

        // 저장된 배수 효과가 있다면 복원
        if (savedData.multiplierEndTimeOffset > 0)
        {
            this.coinMultiplier = doubleCoinMultiplier;
            this.multiplierEndTime = Time.time + savedData.multiplierEndTimeOffset;
        }
        else
        {
            this.coinMultiplier = 1f;
            this.multiplierEndTime = 0f;
        }

        // 전투 관련 상태 초기화
        isBattlePaused = false;
        battlePauseTime = 0f;

        UpdateAllUI();
        UpdateDiamondShopUnlockState();
    }

    #endregion


}

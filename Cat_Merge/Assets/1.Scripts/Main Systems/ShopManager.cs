using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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

    private bool isWaitingForAd = false;                                // 광고 대기 중인지 확인하는 변수 추가

    [Header("---[Free]")]
    [SerializeField] private Button cashForAdRewardButton;              // 광고 시청으로 캐쉬 획득 (광고 시청 불가능 상태일때 비활성화 = interactable 비활성화)
    [SerializeField] private TextMeshProUGUI cashForAdCoolTimeText;     // 광고 시청 쿨타임 Text
    [SerializeField] private GameObject cashForAdRewardOnImage;         // 광고 시청 가능 상태일때 활성화되는 이미지 오브젝트
    [SerializeField] private GameObject cashForAdRewardOffImage;        // 광고 시청 불가능 상태일때 활성화되는 이미지 오브젝트
    [SerializeField] private GameObject cashForAdNewImage;              // 광고 시청 가능 상태일때 활성화되는 New 이미지 오브젝트
    [SerializeField] private GameObject cashForAdDisabledBG;            // 광고 시청 불가능 상태일때 활성화되는 이미지 오브젝트
    private int cashForAdCoolTime = 10;                                // 광고 시청 쿨타임 (게임이 종료되도 시간이 흐르도록 실제 시간을 바탕으로 계산)
    private long lastAdTimeReward = 0;                                  // 마지막 cashForAd 보상 시간 저장
    private int cashForAdPrice = 30;                                    // cashForAd 보상

    [SerializeField] private Button cashForTimeRewardButton;            // 쿨타임마다 활성화되는 무료 캐쉬 획득 (무료 캐쉬 획득 불가능 상태일때 비활성화 = interactable 비활성화)
    [SerializeField] private TextMeshProUGUI cashForTimeCoolTimeText;   // 광고 시청 쿨타임 Text
    [SerializeField] private GameObject cashForTimeNewImage;            // 무료 캐쉬 획득 가능 상태일때 활성화되는 New 이미지 오브젝트
    [SerializeField] private GameObject cashForTimeDisabledBG;          // 무료 캐쉬 획득 불가능 상태일때 활성화되는 이미지 오브젝트
    private int cashForTimeCoolTime = 10;                              // 무료 캐쉬 획득 쿨타임 (게임이 종료되도 시간이 흐르도록 실제 시간을 바탕으로 계산)
    private long lastTimeReward = 0;                                    // 마지막 cashForTime 보상 시간 저장
    private long remainingCoolTimeBeforeAd = 0;                         // 광고 시청 전 남은 쿨타임을 저장
    private int cashForTimePrice = 5;                                   // cashForTime 보상

    //[Header("---[Ad]")]



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
        StartCoroutine(CheckRewardStatus());
    }

    private void OnDisable()
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



    }

    // 버튼 리스너 초기화 함수
    private void InitializeButtonListeners()
    {
        cashForAdRewardButton.onClick.AddListener(OnClickCashForAd);
        cashForTimeRewardButton.onClick.AddListener(OnClickCashForTime);


    }

    // New 이미지 상태 업데이트 함수
    private void UpdateShopNewImage()
    {
        bool isAnyNewImageActive = cashForAdNewImage.activeSelf || cashForTimeNewImage.activeSelf;
        shopNewImage.SetActive(isAnyNewImageActive);
    }

    #endregion


    #region Free Shop System

    // 주기적으로 보상 상태를 체크하는 코루틴
    private IEnumerator CheckRewardStatus()
    {
        WaitForSeconds waitTime = new WaitForSeconds(1f);

        while (true)
        {
            UpdateCashForAdUI();
            UpdateCashForTimeUI();
            UpdateShopNewImage();
            yield return waitTime;
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
            cashForAdCoolTimeText.text = "준비 완료"; //$"쿨타임 {cashForAdCoolTime}초";
        }
        else
        {
            cashForAdCoolTimeText.text = $"쿨타임 {remainTime}초";
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

        // CashForTime의 남은 쿨타임 저장
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        remainingCoolTimeBeforeAd = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));

        // TODO: 광고 재생 로직 추가
        isWaitingForAd = true;
        cashForAdRewardButton.interactable = false;

        GetComponent<GoogleAdsManager>().ShowRewardedInterstitialAd();
    }

    // 광고 시청 완료 시 실행되는 함수
    public void OnAdRewardComplete()
    {
        // 광고 보상 지급
        GameManager.Instance.Cash += cashForAdPrice;

        // 마지막 광고 시청 시간 저장
        lastAdTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // CashForTime의 마지막 보상 시간을 광고 시청 시간만큼 조정
        if (remainingCoolTimeBeforeAd > 0)
        {
            lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (cashForTimeCoolTime - remainingCoolTimeBeforeAd);
        }

        GoogleSave();

        isWaitingForAd = false;
        UpdateCashForAdUI();
    }

    // CashForTime UI 업데이트 함수
    private void UpdateCashForTimeUI()
    {
        bool isTimeRewardAvailable = IsCashForTimeAvailable();

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainTime = Math.Max(0, cashForTimeCoolTime - (currentTime - lastTimeReward));

        cashForTimeRewardButton.interactable = isTimeRewardAvailable;
        cashForTimeNewImage.SetActive(isTimeRewardAvailable);
        cashForTimeDisabledBG.SetActive(!isTimeRewardAvailable);

        if (isTimeRewardAvailable)
        {
            cashForTimeCoolTimeText.text = "준비 완료"; //$"쿨타임 {cashForTimeCoolTime}초";
        }
        else
        {
            cashForTimeCoolTimeText.text = $"쿨타임 {remainTime}초";
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
        GameManager.Instance.Cash += cashForTimePrice;

        // 마지막 보상 시간 저장
        lastTimeReward = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        GoogleSave();

        UpdateCashForTimeUI();
    }

    #endregion


    #region Ad Shop System



    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public long lastAdTimeReward;
        public long lastTimeReward;
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            lastAdTimeReward = this.lastAdTimeReward,
            lastTimeReward = this.lastTimeReward
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.lastAdTimeReward = savedData.lastAdTimeReward;
        this.lastTimeReward = savedData.lastTimeReward;

        UpdateAllUI();
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}
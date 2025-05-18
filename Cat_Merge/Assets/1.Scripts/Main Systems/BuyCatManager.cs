using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// 고양이 구매 스크립트
[DefaultExecutionOrder(-3)]
public class BuyCatManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static BuyCatManager Instance { get; private set; }

    [Header("---[BuyCat]")]
    [SerializeField] private ScrollRect buyCatScrollRect;       // 고양이 구매 스크롤뷰
    [SerializeField] private Button bottomBuyCatButton;         // 고양이 구매 버튼
    [SerializeField] private Image bottomBuyCatButtonImg;       // 고양이 구매 버튼 이미지
    [SerializeField] private GameObject buyCatMenuPanel;        // 아이템 메뉴 판넬
    [SerializeField] private Button buyCatBackButton;           // 고양이 구매 메뉴 뒤로가기 버튼
    [SerializeField] private GameObject buyCatSlotPrefab;       // 고양이 구매 슬롯 프리팹
    [SerializeField] private Transform scrollRectContents;      // 스크롤뷰 컨텐츠 Transform
    [SerializeField] private TextMeshProUGUI emptyCatText;      // 구매 가능한 고양이가 없을때 활성화되는 텍스트

    private Button[] buyCatCoinButtons;                         // 고양이 재화로 구매 버튼
    private Button[] buyCatCashButtons;                         // 크리스탈 재화로 구매 버튼

    private ActivePanelManager activePanelManager;              // ActivePanelManager

    // 개별 고양이 등급에 대한 구매 정보 클래스
    [Serializable]
    public class CatPurchaseInfo
    {
        public int catGrade;
        public int coinPurchaseCount;
        public int cashPurchaseCount;
        public long coinPrice;
        public long cashPrice;

        // 코인 구매 비용 계산 함수
        public long CalculateCoinPrice()
        {
            Cat catData = GameManager.Instance.AllCatData[catGrade];
            int effectivePurchaseCount = Mathf.Min(coinPurchaseCount, 100); // 최대 100회까지만 계산
            double basePrice = catData.CatGetCoin * 3600.0;
            double multiplier = Math.Pow(1.128, effectivePurchaseCount);
            double exactPrice = basePrice * multiplier;

            return (long)Math.Floor(exactPrice);
        }

        // 캐시 구매 비용 계산 함수
        public long CalculateCashPrice()
        {
            Cat catData = GameManager.Instance.AllCatData[catGrade];
            return (catData.CatGrade * 10) + (5 * cashPurchaseCount);
        }
    }

    // UI 요소들을 담을 클래스
    private class BuyCatSlotUI
    {
        public int catGrade;
        public GameObject coinDisabledBg;
        public GameObject cashDisabledBg;
        public Button coinButton;
        public Button cashButton;
        public TextMeshProUGUI countExplainText;
        public TextMeshProUGUI coinFeeText;
        public TextMeshProUGUI cashFeeText;
        public Image catImage;
        public TextMeshProUGUI titleText;
    }

    // 모든 고양이 구매 정보를 관리하는 딕셔너리
    private Dictionary<int, CatPurchaseInfo> catPurchaseInfos = new Dictionary<int, CatPurchaseInfo>();
    private Dictionary<int, BuyCatSlotUI> catSlotUIs = new Dictionary<int, BuyCatSlotUI>();

    private bool isDataLoaded = false;              // 데이터 로드 확인

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
        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            InitializeCatPurchaseInfos();
        }

        InitializeBuyCatManager();
    }

    #endregion


    #region Initialize

    // 기본 BuyCat 초기화 함수
    private void InitializeBuyCatManager()
    {
        buyCatMenuPanel.SetActive(false);
        emptyCatText.gameObject.SetActive(true);

        InitializeActivePanel();
        CreateBuyCatSlots();
        InitializeScrollPositions();
        UpdateAllUI();
    }

    // ActivePanel 초기화 함수
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BuyCatMenu", buyCatMenuPanel, bottomBuyCatButtonImg);

        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // 고양이 구매 슬롯 초기화 함수
    private void CreateBuyCatSlots()
    {
        // 기존 슬롯들 제거
        foreach (Transform child in scrollRectContents)
        {
            Destroy(child.gameObject);
        }
        catSlotUIs.Clear();

        // 모든 고양이 데이터에 대해 슬롯 생성
        int catCount = GameManager.Instance.AllCatData.Length;
        buyCatCoinButtons = new Button[catCount];
        buyCatCashButtons = new Button[catCount];

        for (int i = 0; i < catCount; i++)
        {
            CreateBuyCatSlot(i);
        }
    }

    // 고양이 구매 슬롯 추가 함수
    private void CreateBuyCatSlot(int catGrade)
    {
        GameObject slotObject = Instantiate(buyCatSlotPrefab, scrollRectContents);
        Cat catData = GameManager.Instance.AllCatData[catGrade];

        BuyCatSlotUI slotUI = new BuyCatSlotUI
        {
            catGrade = catGrade,
            coinDisabledBg = slotObject.transform.Find("BackGround/BuyWithCoin Button/Coin DisabledBG").gameObject,
            cashDisabledBg = slotObject.transform.Find("BackGround/BuyWithCash Button/Cash DisabledBG").gameObject,
            coinButton = slotObject.transform.Find("BackGround/BuyWithCoin Button").GetComponent<Button>(),
            cashButton = slotObject.transform.Find("BackGround/BuyWithCash Button").GetComponent<Button>(),
            countExplainText = slotObject.transform.Find("BackGround/BuyCount Text").GetComponent<TextMeshProUGUI>(),
            coinFeeText = slotObject.transform.Find("BackGround/BuyWithCoin Button/CoinFee Text").GetComponent<TextMeshProUGUI>(),
            cashFeeText = slotObject.transform.Find("BackGround/BuyWithCash Button/CashFee Text").GetComponent<TextMeshProUGUI>(),
            catImage = slotObject.transform.Find("BackGround/Cat Image BackGround").GetComponent<Image>(),
            titleText = slotObject.transform.Find("BackGround/Title Text").GetComponent<TextMeshProUGUI>()
        };

        // 버튼 배열에 저장
        buyCatCoinButtons[catGrade] = slotUI.coinButton;
        buyCatCashButtons[catGrade] = slotUI.cashButton;

        // 버튼 이벤트 설정 - catGrade를 클로저로 캡처
        int capturedGrade = catGrade;
        slotUI.coinButton.onClick.AddListener(() => BuyCatCoin(slotUI.coinButton, capturedGrade));
        slotUI.cashButton.onClick.AddListener(() => BuyCatCash(slotUI.cashButton, capturedGrade));

        // 기본 정보 설정
        slotUI.catImage.sprite = catData.CatImage;
        slotUI.titleText.text = $"{catData.CatGrade}. {catData.CatName}";

        // 초기에는 모든 슬롯을 비활성화
        slotObject.SetActive(false);

        catSlotUIs[catGrade] = slotUI;
    }

    // 새로운 고양이가 해금되었을 때 호출되는 함수
    public void UnlockBuySlot(int unlockedCatGrade)
    {
        Cat unlockedCat = GameManager.Instance.AllCatData[unlockedCatGrade];
        int canOpenerValue = unlockedCat.CanOpener - 1;

        if (canOpenerValue < 0)
        {
            return;
        }

        // CanOpener 값에 해당하는 등급까지의 구매 슬롯 활성화
        for (int i = 0; i <= canOpenerValue; i++)
        {
            if (catSlotUIs.TryGetValue(i, out BuyCatSlotUI slotUI))
            {
                Transform slotTransform = slotUI.coinButton.transform.parent.parent;
                slotTransform.gameObject.SetActive(true);
            }
        }

        CheckAndUpdateEmptyText();
    }

    // 활성화된 슬롯이 있는지 확인하고 텍스트 표시를 업데이트하는 함수
    private void CheckAndUpdateEmptyText()
    {
        bool hasActiveSlot = false;
        foreach (var slotUI in catSlotUIs.Values)
        {
            Transform slotTransform = slotUI.coinButton.transform.parent.parent;
            if (slotTransform.gameObject.activeSelf)
            {
                hasActiveSlot = true;
                break;
            }
        }

        // 활성화된 슬롯이 있으면 텍스트를 숨기고, 없으면 텍스트를 표시
        emptyCatText.gameObject.SetActive(!hasActiveSlot);
    }

    // 초기 스크롤 위치 초기화 함수
    private void InitializeScrollPositions()
    {
        if (buyCatScrollRect != null && buyCatScrollRect.content != null)
        {
            // 전체 슬롯 개수 계산 (한 줄에 1개씩)
            int totalSlots = GameManager.Instance.AllCatData.Length;

            // 행의 개수는 슬롯 개수와 동일 (한 줄에 1개씩)
            int rowCount = totalSlots;

            // Grid Layout Group 설정값
            const int SPACING_Y = 0;            // y spacing
            const int CELL_SIZE_Y = 150;        // y cell size
            const int VIEWPORT_HEIGHT = 1180;   // viewport height

            // 전체 컨텐츠 높이 계산: (행 간격 * (행 개수-1)) + (셀 높이 * 행 개수)
            float contentHeight = (SPACING_Y * (rowCount - 1)) + (CELL_SIZE_Y * rowCount);

            // 스크롤 시작 위치 계산: -(전체 높이 - 뷰포트 높이) * 0.5f
            float targetY = -(contentHeight - VIEWPORT_HEIGHT) * 0.5f;

            // 스크롤 컨텐츠의 위치를 상단으로 설정
            buyCatScrollRect.content.anchoredPosition = new Vector2(
                buyCatScrollRect.content.anchoredPosition.x,
                targetY
            );

            // 스크롤 속도 초기화
            buyCatScrollRect.velocity = Vector2.zero;
        }
    }

    // 고양이 구매 정보 초기화 함수
    private void InitializeCatPurchaseInfos()
    {
        if (!GameManager.Instance) return;

        catPurchaseInfos.Clear();
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            var info = new CatPurchaseInfo
            {
                catGrade = i,
                coinPurchaseCount = 0,
                cashPurchaseCount = 0
            };
            info.coinPrice = info.CalculateCoinPrice();
            info.cashPrice = info.CalculateCashPrice();
            catPurchaseInfos[i] = info;
        }

        UpdateAllUI();
    }

    // 전체 UI 업데이트 함수
    private void UpdateAllUI()
    {
        // 초기화 중에는 저장하지 않도록 수정
        bool shouldSave = isDataLoaded;
        isDataLoaded = false;

        foreach (var pair in catPurchaseInfos)
        {
            int catGrade = pair.Key;
            var info = pair.Value;
            if (catSlotUIs.TryGetValue(catGrade, out BuyCatSlotUI slotUI))
            {
                slotUI.countExplainText.text = $"구매 횟수 : {info.coinPurchaseCount}회 + {info.cashPurchaseCount}회";
                slotUI.coinFeeText.text = $"{GameManager.Instance.FormatNumber(info.CalculateCoinPrice())}";
                slotUI.cashFeeText.text = $"{GameManager.Instance.FormatNumber(info.CalculateCashPrice())}";
            }
        }

        isDataLoaded = shouldSave;
    }

    #endregion


    #region Button System

    // 고양이 구매 버튼 함수(코인으로 구매)
    private void BuyCatCoin(Button button, int catGrade)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        var info = GetCatPurchaseInfo(catGrade);
        long currentPrice = info.CalculateCoinPrice();

        if (GameManager.Instance.Coin < currentPrice)
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Coin -= currentPrice;
        info.coinPurchaseCount++;
        info.coinPrice = info.CalculateCoinPrice(); // 다음 구매 가격 업데이트

        ProcessCatPurchase(catGrade);
    }

    // 고양이 구매 버튼 함수(크리스탈로 구매)
    private void BuyCatCash(Button button, int catGrade)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        var info = GetCatPurchaseInfo(catGrade);
        long currentPrice = info.CalculateCashPrice();

        if (GameManager.Instance.Cash < currentPrice)
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Cash -= currentPrice;
        info.cashPurchaseCount++;
        info.cashPrice = info.CalculateCashPrice(); // 다음 구매 가격 업데이트

        ProcessCatPurchase(catGrade);
    }

    // 고양이 구매 처리 함수
    private void ProcessCatPurchase(int catGrade)
    {
        //QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catGrade);
        GetComponent<SpawnManager>().SpawnGradeCat(catGrade);

        UpdateAllUI();
    }

    // 고양이 구매 정보 가져오기
    private CatPurchaseInfo GetCatPurchaseInfo(int catGrade)
    {
        return catPurchaseInfos[catGrade];
    }

    #endregion


    #region Battle System

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleBuyCatState()
    {
        bottomBuyCatButton.interactable = false;

        if (buyCatMenuPanel.activeSelf == true)
        {
            activePanelManager.TogglePanel("BuyCatMenu");
        }
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleBuyCatState()
    {
        bottomBuyCatButton.interactable = true;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public List<CatPurchaseInfo> purchaseInfos = new List<CatPurchaseInfo>();
        public List<int> unlockedSlots = new List<int>();
    }

    public string GetSaveData()
    {
        SaveData saveData = new SaveData();
        foreach (var infos in catPurchaseInfos)
        {
            saveData.purchaseInfos.Add(new CatPurchaseInfo
            {
                catGrade = infos.Value.catGrade,
                coinPurchaseCount = infos.Value.coinPurchaseCount,
                cashPurchaseCount = infos.Value.cashPurchaseCount,
                coinPrice = infos.Value.coinPrice,
                cashPrice = infos.Value.cashPrice
            });
        }

        // 활성화된 슬롯들의 등급 저장
        foreach (var pair in catSlotUIs)
        {
            Transform slotTransform = pair.Value.coinButton.transform.parent.parent;
            if (slotTransform.gameObject.activeSelf)
            {
                saveData.unlockedSlots.Add(pair.Key);
            }
        }
        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        catPurchaseInfos.Clear();
        foreach (var info in savedData.purchaseInfos)
        {
            catPurchaseInfos[info.catGrade] = new CatPurchaseInfo
            {
                catGrade = info.catGrade,
                coinPurchaseCount = info.coinPurchaseCount,
                cashPurchaseCount = info.cashPurchaseCount,
                coinPrice = info.coinPrice,
                cashPrice = info.cashPrice
            };
        }

        CreateBuyCatSlots();

        // 슬롯들의 활성화 상태 복원
        foreach (var pair in catSlotUIs)
        {
            Transform slotTransform = pair.Value.coinButton.transform.parent.parent;
            slotTransform.gameObject.SetActive(savedData.unlockedSlots.Contains(pair.Key));
        }
        CheckAndUpdateEmptyText();
        UpdateAllUI();

        isDataLoaded = true;
    }

    #endregion


}

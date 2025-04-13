using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// 고양이 구매 스크립트
[DefaultExecutionOrder(-2)]
public class BuyCatManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static BuyCatManager Instance { get; private set; }

    [Header("---[BuyCat]")]
    [SerializeField] private Button bottomBuyCatButton;                                 // 고양이 구매 버튼
    [SerializeField] private Image bottomBuyCatButtonImg;                               // 고양이 구매 버튼 이미지
    [SerializeField] private GameObject[] buyCatCoinDisabledBg;                         // 버튼 클릭 못할 때의 배경 (Coin)
    [SerializeField] private GameObject[] buyCatCashDisabledBg;                         // 버튼 클릭 못할 때의 배경 (Cash)
    [SerializeField] private GameObject buyCatMenuPanel;                                // 아이템 메뉴 판넬
    [SerializeField] private Button buyCatBackButton;                                   // 고양이 구매 메뉴 뒤로가기 버튼

    [SerializeField] private Button[] buyCatCoinButtons;                                // 고양이 재화로 구매 버튼
    [SerializeField] private Button[] buyCatCashButtons;                                // 크리스탈 재화로 구매 버튼

    [SerializeField] private TextMeshProUGUI[] buyCatCountExplainTexts;                 // 고양이 구매 횟수 설명창
    [SerializeField] private TextMeshProUGUI[] buyCatCoinFeeTexts;                      // 고양이 구매 비용 (골드)
    [SerializeField] private TextMeshProUGUI[] buyCatCashFeeTexts;                      // 고양이 구매 비용 (크리스탈)

    private ActivePanelManager activePanelManager;                                      // ActivePanelManager

    // 개별 고양이 등급에 대한 구매 정보 클래스
    [Serializable]
    public class CatPurchaseInfo
    {
        public int catGrade;
        public int coinPurchaseCount;
        public int cashPurchaseCount;
        public decimal coinPrice;
        public decimal cashPrice;
    }

    // 모든 고양이 구매 정보를 관리하는 딕셔너리
    private Dictionary<int, CatPurchaseInfo> catPurchaseInfos = new Dictionary<int, CatPurchaseInfo>();


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
        UpdateAllUI();
    }

    #endregion


    #region Initialize

    // 기본 BuyCat 초기화 함수
    private void InitializeBuyCatManager()
    {
        buyCatMenuPanel.SetActive(false);

        InitializeActivePanel();
        InitializeButtonListeners();
    }

    // ActivePanel 초기화 함수
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BuyCatMenu", buyCatMenuPanel, bottomBuyCatButtonImg);

        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // 고양이 구매 정보 초기화 함수
    private void InitializeCatPurchaseInfos()
    {
        if (!GameManager.Instance) return;

        catPurchaseInfos.Clear();
        for (int i = 0; i < 3; i++)
        {
            catPurchaseInfos[i] = new CatPurchaseInfo
            {
                catGrade = i,
                coinPurchaseCount = 0,
                cashPurchaseCount = 0,
                coinPrice = 5,
                cashPrice = 5
            };
        }

        UpdateAllUI();
    }

    // 전체 UI 업데이트 함수
    private void UpdateAllUI()
    {
        // 초기화 중에는 저장하지 않도록 수정
        bool shouldSave = isDataLoaded;
        isDataLoaded = false;

        for (int i = 0; i < 3; i++)
        {
            var info = GetCatPurchaseInfo(i);
            buyCatCountExplainTexts[i].text = $"구매 횟수 : {info.coinPurchaseCount}회 + {info.cashPurchaseCount}회";
            buyCatCoinFeeTexts[i].text = $"{GameManager.Instance.FormatPriceNumber(info.coinPrice)}";
            buyCatCashFeeTexts[i].text = $"{GameManager.Instance.FormatPriceNumber(info.cashPrice)}";
        }

        isDataLoaded = shouldSave;
    }

    // 버튼 리스너 초기화 함수
    private void InitializeButtonListeners()
    {
        for (int i = 0; i < 3; i++)
        {
            int index = i;

            buyCatCoinButtons[index].onClick.RemoveAllListeners();
            buyCatCoinButtons[index].onClick.AddListener(() => BuyCatCoin(buyCatCoinButtons[index]));

            buyCatCashButtons[index].onClick.RemoveAllListeners();
            buyCatCashButtons[index].onClick.AddListener(() => BuyCatCash(buyCatCashButtons[index]));
        }
    }

    #endregion


    #region Button System

    // 고양이 구매 버튼 함수(코인으로 구매)
    private void BuyCatCoin(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        int catIndex = GetButtonIndex(button, buyCatCoinButtons);
        if (catIndex == -1) return;

        var info = GetCatPurchaseInfo(catIndex);
        if (GameManager.Instance.Coin < info.coinPrice)
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Coin -= info.coinPrice;
        info.coinPurchaseCount++;
        info.coinPrice *= 2;

        ProcessCatPurchase(catIndex);
    }

    // 고양이 구매 버튼 함수(크리스탈로 구매)
    private void BuyCatCash(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        int catIndex = GetButtonIndex(button, buyCatCashButtons);
        if (catIndex == -1) return;

        var info = GetCatPurchaseInfo(catIndex);
        if (GameManager.Instance.Cash < info.cashPrice)
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Cash -= info.cashPrice;
        info.cashPurchaseCount++;
        info.cashPrice *= 2;

        ProcessCatPurchase(catIndex);
    }

    // 고양이 구매 처리 함수
    private void ProcessCatPurchase(int catIndex)
    {
        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);
        GetComponent<SpawnManager>().SpawnGradeCat(catIndex);

        UpdateAllUI();

        GoogleSave();
    }

    // 버튼 인덱스 반환 함수
    private int GetButtonIndex(Button button, Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (button == buttons[i])
            {
                return i;
            }
        }
        return -1;
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
        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        
        catPurchaseInfos.Clear();
        foreach (var info in savedData.purchaseInfos)
        {
            if (info.catGrade < 3)
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
        }

        UpdateAllUI();

        isDataLoaded = true;
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

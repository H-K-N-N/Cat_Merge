using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BuyCatManager : MonoBehaviour, ISaveable
{
    // Singleton Instance
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

    private int[] buyCatCoinCounts;                                                     // 고양이 구매 횟수 (코인)
    private int[] buyCatCashCounts;                                                     // 고양이 구매 횟수 (크리스탈)
    private int[] buyCatCoinFee;                                                        // 고양이 구매 비용 (코인)
    private int[] buyCatCashFee;                                                        // 고양이 구매 비용 (크리스탈)

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
        InitializeItemMenuManager();

        buyCatCoinCounts = new int[GameManager.Instance.AllCatData.Length];   // 고양이 구매 횟수 (코인)
        buyCatCashCounts = new int[GameManager.Instance.AllCatData.Length];   // 고양이 구매 횟수 (크리스탈)
        buyCatCoinFee = new int[GameManager.Instance.AllCatData.Length];      // 고양이 구매 비용 (코인)
        buyCatCashFee = new int[GameManager.Instance.AllCatData.Length];      // 고양이 구매 비용 (크리스탈)

        // 고양이 구매 메뉴 관련
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            buyCatCoinCounts[i] = 0;
            buyCatCashCounts[i] = 0;
            buyCatCoinFee[i] = i * 5 + 10;
            buyCatCashFee[i] = 5;

            buyCatCountExplainTexts[i].text = $"구매 횟수 : {buyCatCoinCounts[i]}회 + {buyCatCashCounts[i]}회";
            buyCatCoinFeeTexts[i].text = $"{buyCatCoinFee[i]}";
            buyCatCashFeeTexts[i].text = $"{buyCatCashFee[i]}";
        }

        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {

            if (i < buyCatCoinButtons.Length && i < buyCatCashButtons.Length) // 얘도 포함
            {
                int index = i; // 얘가 있어야 함 근데 이유는 모름
                buyCatCoinButtons[index].onClick.AddListener(() => BuyCatCoin(buyCatCoinButtons[index]));
                buyCatCashButtons[index].onClick.AddListener(() => BuyCatCash(buyCatCashButtons[index]));
            }
            else
            {
                Debug.LogWarning($"Index {i} is out of bounds for buyCatCoinButtons array.");
            }
        }
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BuyCatMenu", buyCatMenuPanel, bottomBuyCatButtonImg);
    }

    // ======================================================================================================================

    private void InitializeItemMenuManager()
    {
        buyCatMenuPanel.SetActive(false);
        OpenCloseBottomBuyCatMenuPanel();
    }

    // ======================================================================================================================
    // [Battle]

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

    // ======================================================================================================================

    // 고양이 구매 메뉴 판넬 여는 함수
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // 고양이 구매 버튼 함수(코인으로 구매)
    private void BuyCatCoin(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        int catIndex = -1;
        for (int i = 0; i < buyCatCoinButtons.Length; i++)
        {
            if (button == buyCatCoinButtons[i])
            {
                catIndex = i;
                break;
            }
        }

        if (catIndex == -1) return;

        if (GameManager.Instance.Coin < buyCatCoinFee[catIndex])
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Coin -= buyCatCoinFee[catIndex];
        buyCatCoinCounts[catIndex]++;
        buyCatCoinFee[catIndex] *= 2;

        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);

        SpawnManager catSpawn = GetComponent<SpawnManager>();
        catSpawn.SpawnGradeCat(catIndex);

        buyCatCountExplainTexts[catIndex].text = $"구매 횟수 : {buyCatCoinCounts[catIndex]}회 + {buyCatCashCounts[catIndex]}회";
        buyCatCoinFeeTexts[catIndex].text = $"{buyCatCoinFee[catIndex]:N0}";

        if (GoogleManager.Instance != null)
        {
            Debug.Log("구글 저장");
            GoogleManager.Instance.SaveGameState();
        }
    }

    // 고양이 구매 버튼 함수(크리스탈로 구매)
    private void BuyCatCash(Button button)
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        int catIndex = -1;
        for (int i = 0; i < buyCatCashButtons.Length; i++)
        {
            if (button == buyCatCashButtons[i])
            {
                catIndex = i;
                break;
            }
        }

        if (catIndex == -1) return;

        if (GameManager.Instance.Cash < buyCatCashFee[catIndex])
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Cash -= buyCatCashFee[catIndex];
        buyCatCashCounts[catIndex]++;
        buyCatCashFee[catIndex] *= 2;

        QuestManager.Instance.AddPurchaseCatsCount();
        DictionaryManager.Instance.UnlockCat(catIndex);

        SpawnManager catSpawn = GetComponent<SpawnManager>();
        catSpawn.SpawnGradeCat(catIndex);

        buyCatCountExplainTexts[catIndex].text = $"구매 횟수 : {buyCatCoinCounts[catIndex]}회 + {buyCatCashCounts[catIndex]}회";
        buyCatCashFeeTexts[catIndex].text = $"{buyCatCashFee[catIndex]:N0}";

        if (GoogleManager.Instance != null)
        {
            Debug.Log("구글 저장");
            GoogleManager.Instance.SaveGameState();
        }
    }

    // ======================================================================================================================

    #region Save System
    [Serializable]
    private class SaveData
    {
        public int[] buyCatCoinCounts;      // 고양이별 코인 구매 횟수
        public int[] buyCatCashCounts;      // 고양이별 캐시 구매 횟수
        public int[] buyCatCoinFee;         // 고양이별 코인 구매 비용
        public int[] buyCatCashFee;         // 고양이별 캐시 구매 비용
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            buyCatCoinCounts = this.buyCatCoinCounts,
            buyCatCashCounts = this.buyCatCashCounts,
            buyCatCoinFee = this.buyCatCoinFee,
            buyCatCashFee = this.buyCatCashFee
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // 데이터 복원
        this.buyCatCoinCounts = savedData.buyCatCoinCounts;
        this.buyCatCashCounts = savedData.buyCatCashCounts;
        this.buyCatCoinFee = savedData.buyCatCoinFee;
        this.buyCatCashFee = savedData.buyCatCashFee;

        // UI 업데이트
        UpdateAllBuyCatUI();
    }

    // UI 상태 업데이트
    private void UpdateAllBuyCatUI()
    {
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            if (i < buyCatCountExplainTexts.Length)
            {
                buyCatCountExplainTexts[i].text = $"구매 횟수 : {buyCatCoinCounts[i]}회 + {buyCatCashCounts[i]}회";
                buyCatCoinFeeTexts[i].text = $"{buyCatCoinFee[i]:N0}";
                buyCatCashFeeTexts[i].text = $"{buyCatCashFee[i]:N0}";
            }
        }
    }
    #endregion

}

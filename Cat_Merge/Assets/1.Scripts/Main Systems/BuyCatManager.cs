using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyCatManager : MonoBehaviour
{
    // Singleton Instance
    public static BuyCatManager Instance { get; private set; }

    [Header("---[BuyCat]")]
    [SerializeField] private Button bottomBuyCatButton;                                 // 고양이 구매 버튼
    [SerializeField] private Image bottomBuyCatButtonImg;                               // 고양이 구매 버튼 이미지
    [SerializeField] private GameObject[] buyCatCoinDisabledBg;                         // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject[] buyCatCashDisabledBg;                         // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject buyCatMenuPanel;                                // 아이템 메뉴 판넬
    private bool isOnToggleBuyCat;                                                      // 아이템 메뉴 판넬 토글
    [SerializeField] private Button buyCatBackButton;                                   // 고양이 구매 메뉴 뒤로가기 버튼

    [SerializeField] private Button[] buyCatCoinButtons;                                // 고양이 재화로 구매 버튼
    [SerializeField] private Button[] buyCatCashButtons;                                // 크리스탈 재화로 구매 버튼

    [SerializeField] private TextMeshProUGUI[] buyCatCountExplainTexts;                 // 고양이 구매 횟수 설명창
    [SerializeField] private TextMeshProUGUI[] buyCatCoinFeeTexts;                      // 고양이 구매 비용 (골드)
    [SerializeField] private TextMeshProUGUI[] buyCatCashFeeTexts;                      // 고양이 구매 비용 (크리스탈)

    private ActivePanelManager activePanelManager;                                      // ActivePanelManager

    private int[] buyCatCoinCounts;                                                     // 고양이 구매 횟수(코인)
    private int[] buyCatCashCounts;                                                     // 고양이 구매 횟수(크리스탈)
    private int[] buyCatCoinFee;                                                        // 고양이 구매 비용 (코인)
    private int[] buyCatCashFee;                                                        // 고양이 구매 비용 (크리스탈)

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

        buyCatCoinCounts = new int[GameManager.Instance.AllCatData.Length];   // 고양이 구매 횟수(코인)
        buyCatCashCounts = new int[GameManager.Instance.AllCatData.Length];   // 고양이 구매 횟수(크리스탈)
        buyCatCoinFee = new int[GameManager.Instance.AllCatData.Length];      // 고양이 구매 비용 (코인)
        buyCatCashFee = new int[GameManager.Instance.AllCatData.Length];      // 고양이 구매 비용 (크리스탈)

        // 고양이 구매 메뉴 관련
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            buyCatCoinCounts[i] = 0;
            buyCatCashCounts[i] = 0;
            buyCatCoinFee[i] = i * 5 + 10;
            buyCatCashFee[i] = 5;

            buyCatCountExplainTexts[i].text = $"BuyCount :{buyCatCoinCounts[i]}cnt + {buyCatCashCounts[i]}cnt";
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

    // 고양이 구매 메뉴 판넬 여는 함수
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        bottomBuyCatButton.onClick.AddListener(() => activePanelManager.TogglePanel("BuyCatMenu"));
        buyCatBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BuyCatMenu"));
    }

    // 고양이 구매 버튼 함수(코인으로 구매)
    private void BuyCatCoin(Button b)
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            if (b == buyCatCoinButtons[0])
            {
                Debug.Log("첫번째 고양이 구매 버튼 클릭(코인으로)");

                GameManager.Instance.Coin -= buyCatCoinFee[0];
                buyCatCoinCounts[0]++;
                buyCatCoinFee[0] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(0);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(0);

                buyCatCountExplainTexts[0].text = $"BuyCount :{buyCatCoinCounts[0]}cnt + {buyCatCashCounts[0]}cnt";
                buyCatCoinFeeTexts[0].text = $"{buyCatCoinFee[0]}";
            }
            else if (b == buyCatCoinButtons[1])
            {
                Debug.Log("두번쩨 고양이 구매 버튼 클릭(코인으로)");

                GameManager.Instance.Coin -= buyCatCoinFee[1];
                buyCatCoinCounts[1]++;
                buyCatCoinFee[1] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(1);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(1);

                buyCatCountExplainTexts[1].text = $"BuyCount :{buyCatCoinCounts[1]}cnt + {buyCatCashCounts[1]}cnt";
                buyCatCoinFeeTexts[1].text = $"{buyCatCoinFee[1]}";
            }
            else if (b == buyCatCoinButtons[2])
            {
                Debug.Log("세번째 고양이 구매 버튼 클릭(코인으로)");

                GameManager.Instance.Coin -= buyCatCoinFee[2];
                buyCatCoinCounts[2]++;
                buyCatCoinFee[2] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(2);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(2);

                buyCatCountExplainTexts[2].text = $"BuyCount :{buyCatCoinCounts[2]}cnt + {buyCatCashCounts[2]}cnt";
                buyCatCoinFeeTexts[2].text = $"{buyCatCoinFee[2]}";
            }
        }
    }

    // 고양이 구매 버튼 함수(크리스탈로 구매)
    private void BuyCatCash(Button b)
    {
        // 현재 고양이 수가 고양이 최대 수 미만일 때
        if (GameManager.Instance.CanSpawnCat())
        {
            if (b == buyCatCashButtons[0])
            {
                Debug.Log("첫번째 고양이 구매 버튼 클릭(캐쉬로)");

                GameManager.Instance.Cash -= buyCatCashFee[0];
                buyCatCashCounts[0]++;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(0);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(0);

                buyCatCountExplainTexts[0].text = $"BuyCount :{buyCatCoinCounts[0]}cnt + {buyCatCashCounts[0]}cnt";
                buyCatCashFeeTexts[0].text = $"{buyCatCashFee[0]}";
            }
            else if (b == buyCatCashButtons[1])
            {
                Debug.Log("두번째 고양이 구매 버튼 클릭(캐쉬로)");

                GameManager.Instance.Cash -= buyCatCashFee[1];
                buyCatCashCounts[1]++;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(1);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(1);

                buyCatCountExplainTexts[1].text = $"BuyCount :{buyCatCoinCounts[1]}cnt + {buyCatCashCounts[1]}cnt";
                buyCatCashFeeTexts[1].text = $"{buyCatCashFee[1]}";
            }
            else if (b == buyCatCashButtons[2])
            {
                Debug.Log("세번째 고양이 구매 버튼 클릭(캐쉬로)");

                GameManager.Instance.Cash -= buyCatCashFee[2];
                buyCatCashCounts[2]++;

                QuestManager.Instance.AddPurchaseCatsCount();
                DictionaryManager.Instance.UnlockCat(2);

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(2);

                buyCatCountExplainTexts[2].text = $"BuyCount :{buyCatCoinCounts[2]}cnt + {buyCatCashCounts[2]}cnt";
                buyCatCashFeeTexts[2].text = $"{buyCatCashFee[2]}";
            }
        }
    }

    // 고양이 구매 관련 UI 업데이트 함수
    private void UpdateBuyCatUI()
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
            {
                // 코인 상태에 따른 UI 업데이트
                bool canAffordWithCoin = GameManager.Instance.Coin >= buyCatCoinFee[i];
                buyCatCoinButtons[i].interactable = canAffordWithCoin;
                buyCatCoinDisabledBg[i].SetActive(!canAffordWithCoin);

                // 크리스탈 상태에 따른 UI 업데이트
                bool canAffordWithCash = GameManager.Instance.Cash >= buyCatCashFee[i];
                buyCatCoinButtons[i].interactable &= canAffordWithCash;
                buyCatCashDisabledBg[i].SetActive(!canAffordWithCash);
            }
        }
    }

    private void InitializeItemMenuManager()
    {
        OpenCloseBottomBuyCatMenuPanel();
    }

    private void Update()
    {
        UpdateBuyCatUI();
    }
}


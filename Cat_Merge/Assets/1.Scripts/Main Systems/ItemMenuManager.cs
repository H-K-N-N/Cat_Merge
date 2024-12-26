using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemMenuManager : MonoBehaviour
{
    [Header("---[ItemMenu]")]
    [SerializeField] private Button bottomItemButton;               // 아이템 버튼
    [SerializeField] private Image bottomItemButtonImg;             // 아이템 버튼 이미지

    [SerializeField] private GameObject itemMenuPanel;              // 아이템 메뉴 판넬
    private bool isOnToggleItemMenu;                                // 아이템 메뉴 판넬 토글
    [SerializeField] private Button itemBackButton;                 // 아이템 뒤로가기 버튼

    // 아이템 메뉴 버튼 그룹(배열로 관리함)
    enum EitemMenuButton
    {
        itemMenuButton = 0,
        colleagueMenuButton,
        backgroundMenuButton,
        goldenSomethingMenuButton,
        end,
    }
    // 아이템 메뉴 판넬들 그룹(배열로 관리함)
    enum EitemMenues
    {
        itemMenues = 0,
        colleagueMenues,
        backgroundMenues,
        goldenMenues,
        end,
    }

    // 아이템 메뉴 안에 아이템, 동료, 배경, 황금깃털의 버튼들
    [Header("---[ItemMenues]")]
    [SerializeField] private Button[] itemMenuButtons;                  // itemPanel에서의 아이템 메뉴 버튼들
    [SerializeField] private GameObject[] itemMenues;                   // itemPanel에서의 메뉴 스크롤창 판넬
    private bool[] isItemMenuButtonsOn = new bool[4];                   // 버튼 색상 감지하기 위한 bool타입 배열

    // 아이템 메뉴 안에 아이템 목록
    [Header("---[ItemMenuList]")]
    [SerializeField] private Button catMaximumIncreaseButton;           // 고양이 최대치 증가 버튼
    [SerializeField] private GameObject disabledBg;                     // 버튼 클릭 못할 때의 배경
    private int catMaximumIncreaseFee = 10;                             // 고양이 최대치 증가 비용
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseText;    // 고양이 최대치 증가 텍스트
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // 고양이 최대치 증가 레벨 텍스트
    private int catMaximumIncreaseLv = 1;                               // 고양이 최대치 증가 레벨

    // ======================================================================================================================

    [Header("---[BuyCat]")]
    [SerializeField] private Button bottomBuyCatButton;                                 // 고양이 구매 버튼
    [SerializeField] private Image bottomBuyCatButtonImg;                               // 고양이 구매 버튼 이미지
    [SerializeField] private GameObject[] buyCatCoinDisabledBg;                           // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject[] buyCatCashDisabledBg;                           // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject buyCatMenuPanel;                                // 아이템 메뉴 판넬
    private bool isOnToggleBuyCat;                                                      // 아이템 메뉴 판넬 토글
    [SerializeField] private Button buyCatBackButton;                                   // 고양이 구매 메뉴 뒤로가기 버튼

    [SerializeField] private Button[] buyCatCoinButtons;                                // 고양이 재화로 구매 버튼
    [SerializeField] private Button[] buyCatCashButtons;                                // 크리스탈 재화로 구매 버튼

    [SerializeField] private TextMeshProUGUI[] buyCatCountExplainTexts;                 // 고양이 구매 횟수 설명창
    [SerializeField] private TextMeshProUGUI[] buyCatCoinFeeTexts;                      // 고양이 구매 비용 (골드)
    [SerializeField] private TextMeshProUGUI[] buyCatCashFeeTexts;                      // 고양이 구매 비용 (크리스탈)

    private int[] buyCatCoinCounts;// = new int[3];   // 고양이 구매 횟수(코인)
    private int[] buyCatCashCounts;// = new int[3];   // 고양이 구매 횟수(크리스탈)
    private int[] buyCatCoinFee;// = new int[3];      // 고양이 구매 비용 (코인)
    private int[] buyCatCashFee;// = new int[3];      // 고양이 구매 비용 (크리스탈)
    // ======================================================================================================================

    private void Awake()
    {

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
        {
            itemMenuButtons[(int)EitemMenuButton.itemMenuButton].GetComponent<Image>().color = parsedColor;
        }

        itemMenuButtons[(int)EitemMenuButton.itemMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.itemMenues]));
        itemMenuButtons[(int)EitemMenuButton.colleagueMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.colleagueMenues]));
        itemMenuButtons[(int)EitemMenuButton.backgroundMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.backgroundMenues]));
        itemMenuButtons[(int)EitemMenuButton.goldenSomethingMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.goldenMenues]));

        for (int i = 1; i < 4; i++)
        {
            isItemMenuButtonsOn[i] = false;
        }
        UpdateIncreaseMaximumFeeText();
        UpdateIncreaseMaximumLvText();

        bottomItemButton.onClick.AddListener(OpenCloseBottomItemMenuPanel);
        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
        itemBackButton.onClick.AddListener(CloseBottomItemMenuPanel);

        // ======================================================================================================================
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
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

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

    // ======================================================================================================================

    // ======================================================================================================================

    // 아이템 메뉴 판넬 여는 함수
    private void OpenCloseBottomItemMenuPanel()
    {
        //Debug.Log("Item Toggle");
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = !isOnToggleItemMenu;
            if (isOnToggleItemMenu)
            {
                //Debug.Log("Item Toggle On");
                itemMenuPanel.SetActive(true);

                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
            else
            {
                //Debug.Log("Item Toggle Off");
                itemMenuPanel.SetActive(false);

                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
        }
    }

    // 아이템 메뉴 판넬 닫는 함수(뒤로가기 버튼)
    private void CloseBottomItemMenuPanel()
    {
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = false;
            itemMenuPanel.SetActive(false);

            ChangeSelectedButtonColor(isOnToggleItemMenu);
        }
    }

    // 바텀 메뉴 버튼 색상 변경
    private void ChangeSelectedButtonColor(bool isOnToggle)
    {
        if (isOnToggle)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
            {
                if (itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if (buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            {
                if (!itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if (!buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
            }
        }
    }

    // 아이템 메뉴의 아이템, 동료, 배경, 황금깃털 버튼 색상 변경
    private void ChangeSelectedButtonColor(GameObject menues)
    {
        if (menues.gameObject.activeSelf)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColorT))
            {
                for (int i = 0; i < (int)EitemMenues.end; i++)
                {
                    if (isItemMenuButtonsOn[i])
                    {
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColorF))
                        {
                            itemMenuButtons[i].GetComponent<Image>().color = parsedColorF;
                        }
                    }
                }
            }
        }
    }

    // 고양이 최대치 증가
    private void IncreaseCatMaximum()
    {
        //Debug.Log("고양이 최대치 증가 버튼 클릭");
        if (catMaximumIncreaseButton != null)
        {
            GameManager.Instance.Coin -= catMaximumIncreaseFee;
            GameManager.Instance.MaxCats++;
            catMaximumIncreaseFee *= 2;
            catMaximumIncreaseLv++;
            UpdateIncreaseMaximumFeeText();
            UpdateIncreaseMaximumLvText();

        }
    }

    // 고양이 수 텍스트 UI 업데이트하는 함수
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseText != null)
        {
            if (GameManager.Instance.Coin < catMaximumIncreaseFee)
            {
                catMaximumIncreaseButton.interactable = false;
                disabledBg.SetActive(true);
            }
            else
            {

                catMaximumIncreaseButton.interactable = true;
                disabledBg.SetActive(false);
            }
            catMaximumIncreaseText.text = $"{catMaximumIncreaseFee}";
        }
    }

    // 고양이 최대치 증가 레벨 UI 업데이트 하는 함수
    private void UpdateIncreaseMaximumLvText()
    {
        catMaximumIncreaseLvText.text = $"Lv.{catMaximumIncreaseLv}";
    }

    // 버튼 클릭 이벤트
    private void OnButtonClicked(GameObject activePanel)
    {
        // 모든 UI 패널 비활성화
        for (int i = 0; i < (int)EitemMenues.end; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // 클릭된 버튼의 UI만 활성화
        activePanel.SetActive(true);

        for (int i = 0; i < (int)EitemMenues.end; i++)
        {
            if (itemMenues[i].activeSelf)
            {
                isItemMenuButtonsOn[i] = true;
            }
        }
        ChangeSelectedButtonColor(activePanel);
    }

    // ======================================================================================================================

    // 아이템 메뉴 판넬 여는 함수
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = !isOnToggleBuyCat;
            if (isOnToggleBuyCat)
            {
                Debug.Log("Buy Cat Toggle On");
                buyCatMenuPanel.SetActive(true);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
            else
            {
                Debug.Log("Buy Cat Toggle Off");
                buyCatMenuPanel.SetActive(false);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
        }
    }

    // 아이템 메뉴 판넬 닫는 함수(뒤로가기 버튼)
    private void CloseBottomBuyCatMenuPanel()
    {
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = false;
            buyCatMenuPanel.SetActive(false);
            ChangeSelectedButtonColor(isOnToggleBuyCat);
        }
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

    private void Update()
    {
        UpdateBuyCatUI();
    }
}
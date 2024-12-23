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
    [SerializeField] private Button bottomBuyCatButton;                 // 고양이 구매 버튼
    [SerializeField] private Image bottomBuyCatButtonImg;               // 고양이 구매 버튼 이미지
    [SerializeField] private GameObject buyCatCoinDisabledBg;           // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject buyCatCashDisabledBg;           // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject buyCatMenuPanel;                // 아이템 메뉴 판넬
    private bool isOnToggleBuyCat;                                      // 아이템 메뉴 판넬 토글
    [SerializeField] private Button buyCatBackButton;                   // 고양이 구매 메뉴 뒤로가기 버튼

    [SerializeField] private Button buyCatCoinButton;                   // 고양이 재화로 구매 버튼
    [SerializeField] private Button buyCatCashButton;                   // 크리스탈 재화로 구매 버튼

    [SerializeField] private TextMeshProUGUI buyCatCountExplainText;    // 고양이 구매 횟수 설명창
    [SerializeField] private TextMeshProUGUI buyCatCoinFeeText;         // 고양이 구매 비용 (골드)
    [SerializeField] private TextMeshProUGUI buyCatCashFeeText;         // 고양이 구매 비용 (크리스탈)
    private int buyCatCoinCount = 0;                                    // 고양이 구매 횟수(코인)
    private int buyCatCashCount = 0;                                    // 고양이 구매 횟수(크리스탈)
    private int buyCatCoinFee = 5;                                      // 고양이 구매 비용 (코인)
    private int buyCatCashFee = 5;                                      // 고양이 구매 비용 (크리스탈)

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




        // 고양이 구매 메뉴 관련
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

        buyCatCoinButton.onClick.AddListener(BuyCatCoin);
        buyCatCashButton.onClick.AddListener(BuyCatCash);
    }

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
            GameManager.Instance.Coin -= catMaximumIncreaseFee;

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
        //Debug.Log("Buy Cat Toggle");
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
    private void BuyCatCoin()
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            Debug.Log("고양이 구매 버튼 클릭(코인으로)");
            GameManager.Instance.Coin -= buyCatCoinFee;
            buyCatCoinCount++;
            buyCatCoinFee *= 2;

            QuestManager.Instance.AddPurchaseCatsCount();

            SpawnManager catSpawn = GetComponent<SpawnManager>();
            catSpawn.OnClickedSpawn();

            UpdateBuyCatUI();
        }
    }

    // 고양이 구매 버튼 함수(코인으로 구매)
    private void BuyCatCash()
    {
        // 현재 고양이 수가 고양이 최대 수 미만일 때
        if (GameManager.Instance.CanSpawnCat())
        {
            Debug.Log("고양이 구매 버튼 클릭(캐쉬로)");
            GameManager.Instance.Cash -= buyCatCashFee;
            buyCatCashCount++;

            QuestManager.Instance.AddPurchaseCatsCount();

            SpawnManager catSpawn = GetComponent<SpawnManager>();
            catSpawn.OnClickedSpawn();

            UpdateBuyCatUI();
        }
    }

    // 고양이 구매 관련 UI 업데이트 함수
    private void UpdateBuyCatUI()
    {
        buyCatCountExplainText.text = $"BuyCount :{buyCatCoinCount}cnt + {buyCatCashCount}cnt";
        buyCatCoinFeeText.text = $"{buyCatCoinFee}";
        buyCatCashFeeText.text = $"{buyCatCashFee}";
        if (GameManager.Instance.CanSpawnCat())
        {
            if (GameManager.Instance.Coin < buyCatCoinFee)
            {
                buyCatCoinButton.interactable = false;
                buyCatCoinDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCoinButton.interactable = true;
                buyCatCoinDisabledBg.SetActive(false);
            }

            if (GameManager.Instance.Cash < buyCatCashFee)
            {
                buyCatCashButton.interactable = false;
                buyCatCashDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCashButton.interactable = true;
                buyCatCashDisabledBg.SetActive(false);
            }
        }
    }


}

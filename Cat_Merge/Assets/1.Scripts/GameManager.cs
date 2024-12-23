using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
[DefaultExecutionOrder(-1)]     // 스크립트 실행 순서 조정
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }        // SingleTon

    // ======================================================================================================================

    // Data
    private Cat[] allCatData;                                       // 모든 고양이 데이터 보유
    public Cat[] AllCatData => allCatData;

    // ======================================================================================================================

    // Main UI Text
    [Header("---[Main UI Text]")]
    [SerializeField] private TextMeshProUGUI catCountText;          // 고양이 수 텍스트
    private int currentCatCount = 0;                                // 화면 내 고양이 수
    private int maxCats = 8;                                        // 최대 고양이 수

    // 기본 재화
    [SerializeField] private TextMeshProUGUI coinText;              // 기본재화 텍스트
    private int coin = 1000;                                        // 기본재화
    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();
        }
    }

    // 캐쉬 재화
    [SerializeField] private TextMeshProUGUI cashText;              // 캐쉬재화 텍스트
    private int cash = 1000;                                        // 캐쉬재화
    public int Cash 
    { 
        get => cash;
        set
        {
            cash = value;
            UpdateCashText();
        }
    }

    // ======================================================================================================================

    // ItemMenu Select Button
    [Header("ItemMenu")]
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
    [Header("ItemMenues")]
    [SerializeField] private Button[] itemMenuButtons;                  // itemPanel에서의 아이템 메뉴 버튼들
    [SerializeField] private GameObject[] itemMenues;                   // itemPanel에서의 메뉴 스크롤창 판넬
    private bool[] isItemMenuButtonsOn = new bool[4];                   // 버튼 색상 감지하기 위한 bool타입 배열


    // 아이템 메뉴 안에 아이템 목록
    [Header("ItemMenuList")]
    [SerializeField] private Button catMaximumIncreaseButton;           // 고양이 최대치 증가 버튼
    [SerializeField] private GameObject disabledBg;                     // 버튼 클릭 못할 때의 배경
    private int catMaximumIncreaseFee = 10;                             // 고양이 최대치 증가 비용
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseText;    // 고양이 최대치 증가 텍스트
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // 고양이 최대치 증가 레벨 텍스트
    private int catMaximumIncreaseLv = 1;                               // 고양이 최대치 증가 레벨

    // ======================================================================================================================

    [Header("BuyCat")]
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 기본 세팅
        LoadAllCats();
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();



        // 아이템 메뉴 관련
        bottomItemButton.onClick.AddListener(OpenCloseBottomItemMenuPanel);
        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
        itemBackButton.onClick.AddListener(CloseBottomItemMenuPanel);

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
        {
            itemMenuButtons[(int)EitemMenuButton.itemMenuButton].GetComponent<Image>().color = parsedColor;
        }

        itemMenuButtons[(int)EitemMenuButton.itemMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.itemMenues]));
        itemMenuButtons[(int)EitemMenuButton.colleagueMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.colleagueMenues]));
        itemMenuButtons[(int)EitemMenuButton.backgroundMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.backgroundMenues]));
        itemMenuButtons[(int)EitemMenuButton.goldenSomethingMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.goldenMenues]));

        for(int i = 1; i < 4; i++)
        {
            isItemMenuButtonsOn[i] = false;
        }
        UpdateIncreaseMaximumFeeText();
        UpdateIncreaseMaximumLvText();

        // 고양이 구매 메뉴 관련
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

        buyCatCoinButton.onClick.AddListener(BuyCatCoin);
        buyCatCashButton.onClick.AddListener(BuyCatCash);
    }

    // ======================================================================================================================

    // 고양이 정보 Load 함수
    private void LoadAllCats()
    {
        // CatDataLoader에서 catDictionary 가져오기
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();
        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader가 없거나 고양이 데이터가 로드되지 않았습니다.");
            return;
        }

        // Dictionary의 모든 값을 배열로 변환
        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);

        Debug.Log($"고양이 데이터 {allCatData.Length}개가 로드되었습니다.");
    }

    // ======================================================================================================================

    // 고양이 수 판별 함수
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // 현재 고양이 수 증가시키는 함수
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // 현재 고양이 수 감소시키는 함수
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // 고양이 수 텍스트 UI 업데이트하는 함수
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

    // 기본재화 텍스트 UI 업데이트하는 함수
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

    // 캐쉬재화 텍스트 UI 업데이트하는 함수
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            // 숫자를 3자리마다 콤마를 추가하여 표시
            cashText.text = cash.ToString("N0");
        }
    }

    // ======================================================================================================================

    // 아이템 메뉴 판넬 여는 함수
    private void OpenCloseBottomItemMenuPanel()
    {
        Debug.Log("Item Toggle");
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = !isOnToggleItemMenu;
            if (isOnToggleItemMenu)
            {
                Debug.Log("Item Toggle On");
                itemMenuPanel.SetActive(true);
                //if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
                //{
                //    bottomItemButtonImg.color = parsedColor;
                //}
                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
            else
            {
                Debug.Log("Item Toggle Off");
                itemMenuPanel.SetActive(false);
                //if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
                //{
                //    bottomItemButtonImg.color = parsedColor;
                //}
                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
        }
    }

    // 아이템 메뉴 판넬 닫는 함수(뒤로가기 버튼)
    private void CloseBottomItemMenuPanel()
    {
        if(itemMenuPanel != null)
        {
            isOnToggleItemMenu = false;
            itemMenuPanel.SetActive(false);
            //if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            //{
            //    bottomItemButtonImg.color = parsedColor;
            //}
            ChangeSelectedButtonColor(isOnToggleItemMenu);
        }
    }

    // 바텀 메뉴 버튼 색상 변경
    private void ChangeSelectedButtonColor(bool isOnToggle)
    {
        if(isOnToggle)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
            {
                if(itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if(buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
                
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            {
                if(!itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if(!buyCatMenuPanel.activeSelf)
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
                        // 색상변경
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColorF))
                        {
                             // 색상변경
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
        Debug.Log("고양이 최대치 증가 버튼 클릭");
        if (catMaximumIncreaseButton != null)
        {
            maxCats++;
            catMaximumIncreaseFee *= 2;
            catMaximumIncreaseLv++;
            UpdateCatCountText();
            UpdateIncreaseMaximumFeeText();
            UpdateIncreaseMaximumLvText();
        }
    }

    // 고양이 수 텍스트 UI 업데이트하는 함수
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseText != null)
        {
            if (coin < catMaximumIncreaseFee)
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
            UpdateCoinText();
            coin -= catMaximumIncreaseFee;

        }
    }

    // 고양이 최대치 증가 레벨 UI 업데이트 하는 함수
    private void UpdateIncreaseMaximumLvText()
    {
        catMaximumIncreaseLvText.text = $"Lv.{catMaximumIncreaseLv}";

    }

    // 버튼 클릭 이벤트
    void OnButtonClicked(GameObject activePanel)
    {
        // 모든 UI 패널 비활성화
        for(int i = 0; i < (int)EitemMenues.end; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // 클릭된 버튼의 UI만 활성화
        activePanel.SetActive(true);
       
        for(int i = 0; i < (int)EitemMenues.end; i++)
        {
            if(itemMenues[i].activeSelf)
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
        Debug.Log("Buy Cat Toggle");
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
        if (CanSpawnCat())
        {
            Debug.Log("고양이 구매 버튼 클릭(코인으로)");
            coin -= buyCatCoinFee;
            buyCatCoinCount++;
            buyCatCoinFee *= 2;

            QuestManager.Instance.AddPurchaseCatsCount();

            CatSpawn catSpawn = GetComponent<CatSpawn>();
            catSpawn.OnClickedSpawn();

            UpdateCoinText();
            UpdateBuyCatUI();
        }
    }

    // 고양이 구매 버튼 함수(코인으로 구매)
    private void BuyCatCash()
    {
        // 현재 고양이 수가 고양이 최대 수 미만일 때
        if (CanSpawnCat()) 
        {
            Debug.Log("고양이 구매 버튼 클릭(캐쉬로)");
            cash -= buyCatCashFee;
            buyCatCashCount++;

            QuestManager.Instance.AddPurchaseCatsCount();

            CatSpawn catSpawn = GetComponent<CatSpawn>();
            catSpawn.OnClickedSpawn();

            UpdateCashText();
            UpdateBuyCatUI();
        }
    }

    // 고양이 구매 관련 UI 업데이트 함수
    private void UpdateBuyCatUI()
    {
        buyCatCountExplainText.text = $"BuyCount :{buyCatCoinCount}cnt + {buyCatCashCount}cnt";
        buyCatCoinFeeText.text = $"{buyCatCoinFee}";
        buyCatCashFeeText.text = $"{buyCatCashFee}";
        if (CanSpawnCat())
        {
            if (coin < buyCatCoinFee)
            {
                buyCatCoinButton.interactable = false;
                buyCatCoinDisabledBg.SetActive(true);
            }
            else
            {
                buyCatCoinButton.interactable = true;
                buyCatCoinDisabledBg.SetActive(false);
            }

            if (cash < buyCatCashFee)
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

    // ======================================================================================================================
}

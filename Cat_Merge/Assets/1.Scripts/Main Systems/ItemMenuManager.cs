using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemMenuManager : MonoBehaviour
{
    // Singleton Instance
    public static ItemMenuManager Instance { get; private set; }

    private ActivePanelManager activePanelManager;                  // ActivePanelManager
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
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseFeeText; // 고양이 최대치 증가 비용 텍스트
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // 고양이 최대치 증가 레벨 텍스트
    private int catMaximumIncreaseLv = 1;                               // 고양이 최대치 증가 레벨
    [SerializeField] private TextMeshProUGUI catHowManyIncreaseText; // 고양이 증가 얼마나 했는지 텍스트
    private int catHowManyIncrease = 1;


    private int step = 0;
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

        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
    }

    private void Start()
    {
        // 판넬 토글
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BottomItemMenu", itemMenuPanel, bottomItemButtonImg);

        catMaximumIncreaseLvText.text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[0].step}";
        catHowManyIncreaseText.text = $"{ ItemFunctionManager.Instance.maxCatsList[0].value}->{ ItemFunctionManager.Instance.maxCatsList[1].value}"; // 1->2 2->3 3->4
        catMaximumIncreaseFeeText.text = $"{ ItemFunctionManager.Instance.maxCatsList[0].fee}";
    }

    // 아이템 메뉴 판넬 여는 함수
    private void OpenCloseBottomItemMenuPanel()
    {
        bottomItemButton.onClick.AddListener(() => activePanelManager.TogglePanel("BottomItemMenu"));
        itemBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BottomItemMenu"));
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
            //GameManager.Instance.Coin -= catMaximumIncreaseFee;
            //GameManager.Instance.MaxCats++;
            //catMaximumIncreaseFee *= 2;

            //catHowManyIncreaseText.text = $"{++catHowManyIncrease}->{catHowManyIncrease + 1}"; // 1->2 2->3 3->4

            //UpdateIncreaseMaximumFeeText();
            //UpdateIncreaseMaximumLvText();
            if (step < 0 || step > ItemFunctionManager.Instance.maxCatsList.Count)
            {
                Debug.LogError($"Index {step} is out of bounds! List Count: {ItemFunctionManager.Instance.maxCatsList.Count}");
                return;
            }
            step++;
            catMaximumIncreaseLvText.text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[step].step}";
            catHowManyIncreaseText.text = $"{ ItemFunctionManager.Instance.maxCatsList[step].value}->{ ItemFunctionManager.Instance.maxCatsList[step + 1].value}"; // 1->2 2->3 3->4
            catMaximumIncreaseFeeText.text = $"{ ItemFunctionManager.Instance.maxCatsList[step].fee}";
        }
    }

    // 고양이 수 텍스트 UI 업데이트하는 함수
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseFeeText != null)
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
            catMaximumIncreaseFeeText.text = $"{catMaximumIncreaseFee}";
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

    private void InitializeItemMenuManager()
    {
        OpenCloseBottomItemMenuPanel();
    }
}
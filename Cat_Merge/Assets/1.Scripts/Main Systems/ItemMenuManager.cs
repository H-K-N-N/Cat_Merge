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
    [SerializeField] private Button increaseCatMaximumButton;           // 고양이 최대치 증가 버튼
    [SerializeField] private Button reduceCollectingTimeButton;         // 재화 획득 시간 감소 버튼
    [SerializeField] private Button increaseFoodMaximumButton;          // 먹이 최대치 증가 버튼
    [SerializeField] private Button reduceProducingFoodTimeButton;      // 먹이 생성 시간 감소 버튼
    [SerializeField] private Button autoCollectingButton;               // 자동 먹이주기 버튼

    [SerializeField] private GameObject[] disabledBg;                   // 버튼 클릭 못할 때의 배경

    [SerializeField] private TextMeshProUGUI[] itemMenuesLvText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesValueText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesFeeText;

    // 고양이 최대치
    private int maxCatsLv = 0;
    public int MaxCatsLv { get => maxCatsLv; set => maxCatsLv = value; }

    // 재화 획득 시간
    private int reduceCollectingTimeLv = 0;
    public int ReduceCollectingTimeLv { get => reduceCollectingTimeLv; set => reduceCollectingTimeLv = value; }

    // 먹이 최대치
    private int maxFoodsLv = 0;
    public int MaxFoodsLv { get => maxFoodsLv; set => maxFoodsLv = value; }

    // 먹이 생성 시간
    private int reduceProducingFoodTimeLv = 0;
    public int ReduceProducingFoodTimeLv { get => reduceProducingFoodTimeLv; set => reduceProducingFoodTimeLv = value; }

    // 자동 먹이주기 시간
    private int autoCollectingLv = 0;
    public int AutoCollectingLv { get => autoCollectingLv; set => autoCollectingLv = value; }

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";             // 비활성화상태 Color

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

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString(activeColorCode, out Color parsedColor))
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

        increaseCatMaximumButton.onClick.AddListener(IncreaseCatMaximum);
        reduceCollectingTimeButton.onClick.AddListener(ReduceCollectingTime);
        increaseFoodMaximumButton.onClick.AddListener(IncreaseFoodMaximum);
        reduceProducingFoodTimeButton.onClick.AddListener(ReduceProducingFoodTime);
        autoCollectingButton.onClick.AddListener(AutoCollecting);
        OpenCloseBottomItemMenuPanel();
    }

    private void Start()
    {
        // 판넬 토글
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BottomItemMenu", itemMenuPanel, bottomItemButtonImg);

        InitItemMenuText();
    }

    // ======================================================================================================================

    private void InitItemMenuText()
    {
        for (int i = 0; i <= 3; i++)
        {
            switch (i)
            {
                case 0:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.maxCatsList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.maxCatsList[0].value}->{ ItemFunctionManager.Instance.maxCatsList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.maxCatsList[0].fee}";
                    break;
                case 1:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.reduceCollectingTimeList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.reduceCollectingTimeList[0].value}->{ ItemFunctionManager.Instance.reduceCollectingTimeList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.reduceCollectingTimeList[0].fee}";
                    break;
                case 2:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.maxFoodsList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.maxFoodsList[0].value}->{ ItemFunctionManager.Instance.maxFoodsList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.maxFoodsList[0].fee}";
                    break;
                case 3:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[0].value}->{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[1].value}"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.reduceProducingFoodTimeList[0].fee}";
                    break;
                case 4:
                    itemMenuesLvText[i].text = $"Lv.{ ItemFunctionManager.Instance.autoCollectingList[0].step}";
                    itemMenuesValueText[i].text = $"{ ItemFunctionManager.Instance.autoCollectingList[0].value}sec"; // 1->2 2->3 3->4
                    itemMenuesFeeText[i].text = $"{ ItemFunctionManager.Instance.autoCollectingList[0].fee}";
                    break;
                default:
                    break;
            }
        }
    }

    // 아이템 메뉴 판넬 여는 함수
    private void OpenCloseBottomItemMenuPanel()
    {
        bottomItemButton.onClick.AddListener(() => activePanelManager.TogglePanel("BottomItemMenu"));
        itemBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BottomItemMenu"));
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

    // 아이템 메뉴의 아이템, 동료, 배경, 황금깃털 버튼 색상 변경
    private void ChangeSelectedButtonColor(GameObject menues)
    {
        if (menues.gameObject.activeSelf)
        {
            if (ColorUtility.TryParseHtmlString(activeColorCode, out Color parsedColorT))
            {
                for (int i = 0; i < (int)EitemMenues.end; i++)
                {
                    if (isItemMenuButtonsOn[i])
                    {
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString(inactiveColorCode, out Color parsedColorF))
                        {
                            itemMenuButtons[i].GetComponent<Image>().color = parsedColorF;
                        }
                    }
                }
            }
        }
    }

    // ======================================================================================================================
    // [Battle]

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleItemMenuState()
    {
        bottomItemButton.interactable = false;

        if (itemMenuPanel.activeSelf == true)
        {
            activePanelManager.TogglePanel("BottomItemMenu");
        }
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleItemMenuState()
    {
        bottomItemButton.interactable = true;
    }

    // ======================================================================================================================

    // 고양이 최대치 증가
    private void IncreaseCatMaximum()
    {
        if (itemMenuesFeeText[0] == null) return;

        bool isMaxLevel = maxCatsLv >= ItemFunctionManager.Instance.maxCatsList.Count - 1;
        if (isMaxLevel)
        {
            increaseCatMaximumButton.interactable = false;
            disabledBg[0].SetActive(true);
            itemMenuesFeeText[0].text = "구매 완료";
        }
        else
        {
            decimal fee = ItemFunctionManager.Instance.maxCatsList[maxCatsLv].fee;
            if (GameManager.Instance.Coin < fee)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
                return;
            }

            GameManager.Instance.Coin -= fee;
            maxCatsLv++;
            itemMenuesLvText[0].text = $"Lv.{ItemFunctionManager.Instance.maxCatsList[maxCatsLv].step}";

            isMaxLevel = maxCatsLv >= ItemFunctionManager.Instance.maxCatsList.Count - 1;
            if (isMaxLevel)
            {
                increaseCatMaximumButton.interactable = false;
                disabledBg[0].SetActive(true);
                itemMenuesFeeText[0].text = "구매 완료";
                itemMenuesValueText[0].text = $"{ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value}";
            }
            else
            {
                itemMenuesValueText[0].text = $"{ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value} -> {ItemFunctionManager.Instance.maxCatsList[maxCatsLv + 1].value}";
                itemMenuesFeeText[0].text = $"{ItemFunctionManager.Instance.maxCatsList[maxCatsLv + 1].fee:N0}";
            }
        }

        GameManager.Instance.MaxCats = ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value;
    }

    // 재화 획득 시간 감소
    private void ReduceCollectingTime()
    {
        if (itemMenuesFeeText[1] == null) return;

        bool isMaxLevel = reduceCollectingTimeLv >= ItemFunctionManager.Instance.reduceCollectingTimeList.Count - 1;
        if (isMaxLevel)
        {
            reduceCollectingTimeButton.interactable = false;
            disabledBg[1].SetActive(true);
            itemMenuesFeeText[1].text = "구매 완료";
        }
        else
        {
            decimal fee = ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].fee;
            if (GameManager.Instance.Coin < fee)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
                return;
            }

            GameManager.Instance.Coin -= fee;
            reduceCollectingTimeLv++;
            itemMenuesLvText[1].text = $"Lv.{ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].step}";

            isMaxLevel = reduceCollectingTimeLv >= ItemFunctionManager.Instance.reduceCollectingTimeList.Count - 1;
            if (isMaxLevel)
            {
                reduceCollectingTimeButton.interactable = false;
                disabledBg[1].SetActive(true);
                itemMenuesFeeText[1].text = "구매 완료";
                itemMenuesValueText[1].text = $"{ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].value}";
            }
            else
            {
                itemMenuesValueText[1].text = $"{ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv].value} -> {ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv + 1].value}";
                itemMenuesFeeText[1].text = $"{ItemFunctionManager.Instance.reduceCollectingTimeList[reduceCollectingTimeLv + 1].fee:N0}";
            }
        }
    }

    // 먹이 최대치 증가
    private void IncreaseFoodMaximum()
    {
        if (itemMenuesFeeText[2] == null) return;

        bool isMaxLevel = maxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count - 1;
        if (isMaxLevel)
        {
            increaseFoodMaximumButton.interactable = false;
            disabledBg[2].SetActive(true);
            itemMenuesFeeText[2].text = "구매 완료";
        }
        else
        {
            decimal fee = ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].fee;
            if (GameManager.Instance.Coin < fee)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
                return;
            }

            GameManager.Instance.Coin -= fee;
            maxFoodsLv++;
            itemMenuesLvText[2].text = $"Lv.{ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].step}";

            isMaxLevel = maxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count - 1;
            if (isMaxLevel)
            {
                increaseFoodMaximumButton.interactable = false;
                disabledBg[2].SetActive(true);
                itemMenuesFeeText[2].text = "구매 완료";
                itemMenuesValueText[2].text = $"{ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].value}";
            }
            else
            {
                itemMenuesValueText[2].text = $"{ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv].value} -> {ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv + 1].value}";
                itemMenuesFeeText[2].text = $"{ItemFunctionManager.Instance.maxFoodsList[maxFoodsLv + 1].fee:N0}";
            }
        }

        SpawnManager.Instance.UpdateFoodText();
        SpawnManager.Instance.OnMaxFoodIncreased();
    }

    // 먹이 생성 시간 감소
    private void ReduceProducingFoodTime()
    {
        if (itemMenuesFeeText[3] == null) return;

        bool isMaxLevel = reduceProducingFoodTimeLv >= ItemFunctionManager.Instance.reduceProducingFoodTimeList.Count - 1;
        if (isMaxLevel)
        {
            reduceProducingFoodTimeButton.interactable = false;
            disabledBg[3].SetActive(true);
            itemMenuesFeeText[3].text = "구매 완료";
        }
        else
        {
            decimal fee = ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].fee;
            if (GameManager.Instance.Coin < fee)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
                return;
            }

            GameManager.Instance.Coin -= fee;
            reduceProducingFoodTimeLv++;
            itemMenuesLvText[3].text = $"Lv.{ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].step}";

            isMaxLevel = reduceProducingFoodTimeLv >= ItemFunctionManager.Instance.reduceProducingFoodTimeList.Count - 1;
            if (isMaxLevel)
            {
                reduceProducingFoodTimeButton.interactable = false;
                disabledBg[3].SetActive(true);
                itemMenuesFeeText[3].text = "구매 완료";
                itemMenuesValueText[3].text = $"{ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].value}";
            }
            else
            {
                itemMenuesValueText[3].text = $"{ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv].value} -> {ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv + 1].value}";
                itemMenuesFeeText[3].text = $"{ItemFunctionManager.Instance.reduceProducingFoodTimeList[reduceProducingFoodTimeLv + 1].fee:N0}";
            }
        }
    }

    // 자동 먹이 시간 감소
    private void AutoCollecting()
    {
        if (itemMenuesFeeText[4] == null) return;

        bool isMaxLevel = autoCollectingLv >= ItemFunctionManager.Instance.autoCollectingList.Count - 1;
        if (isMaxLevel)
        {
            autoCollectingButton.interactable = false;
            disabledBg[4].SetActive(true);
            itemMenuesFeeText[4].text = "구매 완료";
        }
        else
        {
            decimal fee = ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].fee;
            if (GameManager.Instance.Coin < fee)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
                return;
            }

            GameManager.Instance.Coin -= fee;
            autoCollectingLv++;
            itemMenuesLvText[4].text = $"Lv.{ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].step}";

            isMaxLevel = autoCollectingLv >= ItemFunctionManager.Instance.autoCollectingList.Count - 1;
            if (isMaxLevel)
            {
                autoCollectingButton.interactable = false;
                disabledBg[4].SetActive(true);
                itemMenuesFeeText[4].text = "구매 완료";
                itemMenuesValueText[4].text = $"{ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].value}sec";
            }
            else
            {
                itemMenuesValueText[4].text = $"{ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv].value}sec -> {ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv + 1].value}sec";
                itemMenuesFeeText[4].text = $"{ItemFunctionManager.Instance.autoCollectingList[autoCollectingLv + 1].fee:N0}";
            }
        }
    }

    // ======================================================================================================================



}

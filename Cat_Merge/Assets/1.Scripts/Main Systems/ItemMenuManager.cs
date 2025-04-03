using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// 아이템 메뉴 스크립트
[DefaultExecutionOrder(-4)]
public class ItemMenuManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static ItemMenuManager Instance { get; private set; }

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
    [SerializeField] private Button foodUpgradeButton;                  // 먹이 업그레이드 버튼
    [SerializeField] private Button foodUpgrade2Button;                 // 먹이 업그레이드2 버튼
    [SerializeField] private Button foodUpgrade2DisabledButton;         // 먹이 업그레이드2 잠김버튼
    [SerializeField] private Button autoCollectingButton;               // 자동 먹이주기 버튼

    [SerializeField] private GameObject[] disabledBg;                   // 버튼 클릭 못할 때의 배경

    [Header("---[UI Text]")]
    [SerializeField] private TextMeshProUGUI[] itemMenuesLvText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesValueText;
    [SerializeField] private TextMeshProUGUI[] itemMenuesFeeText;

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";                   // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";                 // 비활성화상태 Color

    private ActivePanelManager activePanelManager;                      // ActivePanelManager

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

    // 먹이 업그레이드
    private int foodUpgradeLv = 0;
    public int FoodUpgradeLv { get => foodUpgradeLv; set => foodUpgradeLv = value; }

    // 먹이 업그레이드2
    private int foodUpgrade2Lv = 0;
    public int FoodUpgrade2Lv { get => foodUpgrade2Lv; set => foodUpgrade2Lv = value; }

    // 자동 먹이주기 시간
    private int autoCollectingLv = 0;
    public int AutoCollectingLv { get => autoCollectingLv; set => autoCollectingLv = value; }


    private bool isDataLoaded = false;              // 데이터 로드 확인

    public int minFoodLv = 2;
    public int maxFoodLv = 0;

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
            InitializeDefaultValues();
        }

        InitializeItemMenuManager();
        UpdateAllUI();
    }

    #endregion


    #region Initialize

    // 기본값 초기화 함수
    private void InitializeDefaultValues()
    {
        maxCatsLv = 0;
        reduceCollectingTimeLv = 0;
        maxFoodsLv = 0;
        reduceProducingFoodTimeLv = 0;
        foodUpgradeLv = 0;
        foodUpgrade2Lv = 0;
        autoCollectingLv = 0;
        minFoodLv = 2;
        maxFoodLv = 0;
    }

    // 기본 ItemMenuManager 초기화 함수
    private void InitializeItemMenuManager()
    {
        InitializeActivePanel();
        InitializeMenuButtons();
        InitializeItemButtons();
        SetupInitialMenuState();
    }

    // ActivePanel 초기화 함수
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("BottomItemMenu", itemMenuPanel, bottomItemButtonImg);

        bottomItemButton.onClick.AddListener(() => activePanelManager.TogglePanel("BottomItemMenu"));
        itemBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("BottomItemMenu"));
    }

    // 메뉴 버튼 초기화 함수
    private void InitializeMenuButtons()
    {
        for (int i = 0; i < itemMenuButtons.Length; i++)
        {
            int index = i;
            itemMenuButtons[i].onClick.RemoveAllListeners();
            itemMenuButtons[i].onClick.AddListener(() => OnButtonClicked(itemMenues[index]));
        }
    }

    // 아이템 버튼 초기화 함수
    private void InitializeItemButtons()
    {
        increaseCatMaximumButton.onClick.RemoveAllListeners();
        increaseCatMaximumButton.onClick.AddListener(IncreaseCatMaximum);

        reduceCollectingTimeButton.onClick.RemoveAllListeners();
        reduceCollectingTimeButton.onClick.AddListener(ReduceCollectingTime);

        increaseFoodMaximumButton.onClick.RemoveAllListeners();
        increaseFoodMaximumButton.onClick.AddListener(IncreaseFoodMaximum);

        reduceProducingFoodTimeButton.onClick.RemoveAllListeners();
        reduceProducingFoodTimeButton.onClick.AddListener(ReduceProducingFoodTime);

        foodUpgradeButton.onClick.RemoveAllListeners();
        foodUpgradeButton.onClick.AddListener(FoodUpgrade);

        foodUpgrade2Button.onClick.RemoveAllListeners();
        foodUpgrade2Button.onClick.AddListener(FoodUpgrade2);

        autoCollectingButton.onClick.RemoveAllListeners();
        autoCollectingButton.onClick.AddListener(AutoCollecting);
    }

    // 초기 메뉴 상태 설정 함수
    private void SetupInitialMenuState()
    {
        itemMenues[0].SetActive(true);
        for (int i = 1; i < itemMenues.Length; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        if (ColorUtility.TryParseHtmlString(activeColorCode, out Color parsedColor))
        {
            itemMenuButtons[0].GetComponent<Image>().color = parsedColor;
        }
        isItemMenuButtonsOn[0] = true;
    }

    #endregion


    #region UI Updates

    // 모든 UI 업데이트 함수
    private void UpdateAllUI()
    {
        bool shouldSave = isDataLoaded;
        isDataLoaded = false;

        UpdateItemUI(0, maxCatsLv, increaseCatMaximumButton, ItemFunctionManager.Instance.maxCatsList);
        UpdateItemUI(1, reduceCollectingTimeLv, reduceCollectingTimeButton, ItemFunctionManager.Instance.reduceCollectingTimeList);
        UpdateItemUI(2, maxFoodsLv, increaseFoodMaximumButton, ItemFunctionManager.Instance.maxFoodsList);
        UpdateItemUI(3, reduceProducingFoodTimeLv, reduceProducingFoodTimeButton, ItemFunctionManager.Instance.reduceProducingFoodTimeList);
        UpdateItemUI(4, foodUpgradeLv, foodUpgradeButton, ItemFunctionManager.Instance.foodUpgradeList);
        UpdateItemUI(5, foodUpgrade2Lv, foodUpgrade2Button, ItemFunctionManager.Instance.foodUpgrade2List);
        UpdateItemUI(6, autoCollectingLv, autoCollectingButton, ItemFunctionManager.Instance.autoCollectingList);

        UpdateRelatedSystems();

        isDataLoaded = shouldSave;
    }

    // 아이템 UI 업데이트 함수
    private void UpdateItemUI(int index, int level, Button button, List<(int step, float value, decimal fee)> itemList)
    {
        if (itemMenuesFeeText[index] == null) return;

        bool isMaxLevel = level >= itemList.Count - 1;
        if (isMaxLevel)
        {
            SetMaxLevelUI(index, level, button, itemList[level].value);
        }
        else
        {
            SetNormalLevelUI(index, level, itemList);
        }
    }

    // 최대 레벨 UI 설정 함수
    private void SetMaxLevelUI(int index, int level, Button button, float value)
    {
        button.interactable = false;
        disabledBg[index].SetActive(true);

        itemMenuesLvText[index].text = $"Lv.{level + 1}";
        itemMenuesValueText[index].text = $"{value}";
        itemMenuesFeeText[index].text = "구매완료";
    }

    
    // 일반 레벨 UI 설정 함수
    private void SetNormalLevelUI(int index, int level, List<(int step, float value, decimal fee)> itemList)
    {
        itemMenuesLvText[index].text = $"Lv.{itemList[level].step}";
        if (index == 4)
        {
            itemMenuesValueText[index].text = $"{itemList[level].value - 1} → {itemList[level + 1].value - 1}"; // 1(2-1) -> 2(3-1)
            maxFoodLv = (int)itemList[level].value - 1;

            itemMenuesValueText[5].text = $"{minFoodLv-1}~{maxFoodLv} → {minFoodLv}~{maxFoodLv}";

            if (maxFoodLv >= 15)
            {
                foodUpgrade2DisabledButton.gameObject.SetActive(false);
                foodUpgrade2Button.gameObject.SetActive(true);
            }
        }
        else if (index == 5)
        {
            minFoodLv = (int)itemList[level].value;
            itemMenuesValueText[index].text = $"{minFoodLv - 1}~{maxFoodLv} → {minFoodLv}~{maxFoodLv}";
        }
        else
        {
            itemMenuesValueText[index].text = $"{itemList[level].value} → {itemList[level + 1].value}";
        }

        itemMenuesFeeText[index].text = $"{GameManager.Instance.FormatPriceNumber(itemList[level].fee)}";
    }

    //public int minFoodLv = 0;
    //public int maxFoodLv = 0;
    //// 일반 레벨 UI 설정 함수
    //private void SetNormalLevelUI(int index, int level, List<(int step, float value, decimal fee)> itemList)
    //{
    //    itemMenuesLvText[index].text = $"Lv.{itemList[level].step}";
    //    if (index == 4)
    //    {
    //        itemMenuesValueText[index].text = $"{itemList[level].value - 1} → {itemList[level + 1].value - 1}";
    //        maxFoodLv = (int)itemList[level].value - 1;

    //        if (maxFoodLv >= 15)
    //        {
    //            if (foodUpgrade2DisabledButton.gameObject.activeSelf)
    //            {
    //                foodUpgrade2DisabledButton.gameObject.SetActive(false);
    //            }
    //            if (!foodUpgrade2Button.gameObject.activeSelf)
    //            {
    //                foodUpgrade2Button.gameObject.SetActive(true);
    //            }
    //        }
    //    }
    //    else if (index == 5)
    //    {
    //        minFoodLv = (int)itemList[level].value;

    //        if (maxFoodLv < 15)
    //        {
    //            itemMenuesValueText[index].text = $"{minFoodLv - 1}~15 → {minFoodLv}~15";
    //            return;
    //        }

    //        itemMenuesValueText[index].text = $"{minFoodLv-1}~{maxFoodLv} → {minFoodLv}~{maxFoodLv}";
    //    }
    //    else
    //    {
    //        itemMenuesValueText[index].text = $"{itemList[level].value} → {itemList[level + 1].value}";
    //    }

    //    itemMenuesFeeText[index].text = $"{GameManager.Instance.FormatPriceNumber(itemList[level].fee)}";
    //}

    // 시스템 업데이트 함수
    private void UpdateRelatedSystems()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MaxCats = (int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value;
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.UpdateFoodText();
        }
    }

    #endregion


    #region Button System

    // 버튼 클릭 처리 함수
    private void OnButtonClicked(GameObject activePanel)
    {
        // 모든 UI 패널 비활성화
        for (int i = 0; i < itemMenues.Length; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // 클릭된 버튼의 UI만 활성화
        activePanel.SetActive(true);

        for (int i = 0; i < itemMenues.Length; i++)
        {
            isItemMenuButtonsOn[i] = itemMenues[i].activeSelf;
        }
        UpdateMenuButtonColors();
    }

    // 메뉴 버튼 색상 업데이트 함수
    private void UpdateMenuButtonColors()
    {
        if (ColorUtility.TryParseHtmlString(activeColorCode, out Color activeColor) &&
            ColorUtility.TryParseHtmlString(inactiveColorCode, out Color inactiveColor))
        {
            for (int i = 0; i < itemMenuButtons.Length; i++)
            {
                itemMenuButtons[i].GetComponent<Image>().color = isItemMenuButtonsOn[i] ? activeColor : inactiveColor;
            }
        }
    }

    #endregion


    #region ItemMenu System

    // 아이템 업그레이드 처리 함수
    private void ProcessUpgrade(int index, ref int level, List<(int step, float value, decimal fee)> itemList, Button button, Action onSuccess = null)
    {
        if (itemMenuesFeeText[index] == null) return;

        if (level >= itemList.Count - 1)
        {
            SetMaxLevelUI(index, level, button, itemList[level].value);
            return;
        }

        decimal fee = itemList[level].fee;
        if (GameManager.Instance.Coin < fee)
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        GameManager.Instance.Coin -= fee;
        level++;

        if (level >= itemList.Count - 1)
        {
            SetMaxLevelUI(index, level, button, itemList[level].value);
        }
        else
        {
            UpdateItemUI(index, level,  button, itemList);
        }

        onSuccess?.Invoke();
        GoogleSave();
    }

    // 고양이 최대치 증가 함수
    private void IncreaseCatMaximum()
    {
        ProcessUpgrade(
            0, 
            ref maxCatsLv, 
            ItemFunctionManager.Instance.maxCatsList,
            increaseCatMaximumButton,
            () =>
            {
                GameManager.Instance.MaxCats = (int)ItemFunctionManager.Instance.maxCatsList[maxCatsLv].value;
            });

    }

    // 재화 획득 시간 감소 함수
    private void ReduceCollectingTime()
    {
        ProcessUpgrade(
            1, 
            ref reduceCollectingTimeLv,
            ItemFunctionManager.Instance.reduceCollectingTimeList,
            reduceCollectingTimeButton
            );
    }

    // 먹이 최대치 증가 함수
    private void IncreaseFoodMaximum()
    {
        ProcessUpgrade(
            2, 
            ref maxFoodsLv, 
            ItemFunctionManager.Instance.maxFoodsList,
            increaseFoodMaximumButton,
            () => 
            {
                SpawnManager.Instance.UpdateFoodText();
                SpawnManager.Instance.OnMaxFoodIncreased();
            });
    }

    // 먹이 생성 시간 감소 함수
    private void ReduceProducingFoodTime()
    {
        ProcessUpgrade(
            3, 
            ref reduceProducingFoodTimeLv,
            ItemFunctionManager.Instance.reduceProducingFoodTimeList,
            reduceProducingFoodTimeButton
            );
    }

    // 자동 먹이주기 시간 감소 함수
    private void FoodUpgrade()
    {
        ProcessUpgrade(
            4,
            ref foodUpgradeLv,
            ItemFunctionManager.Instance.foodUpgradeList,
            foodUpgradeButton
            );
    }

    // 자동 먹이주기 시간 감소 함수
    private void FoodUpgrade2()
    {
        ProcessUpgrade(
            5,
            ref foodUpgrade2Lv,
            ItemFunctionManager.Instance.foodUpgrade2List,
            foodUpgrade2Button
            );
    }

    // 자동 먹이주기 시간 감소 함수
    private void AutoCollecting()
    {
        ProcessUpgrade(
            6, 
            ref autoCollectingLv,
            ItemFunctionManager.Instance.autoCollectingList,
            autoCollectingButton
            );
    }

    #endregion


    #region Battle System

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

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public int maxCatsLv;                       // 고양이 최대 보유수 레벨
        public int reduceCollectingTimeLv;          // 재화 획득 시간 레벨
        public int maxFoodsLv;                      // 먹이 최대치 레벨
        public int reduceProducingFoodTimeLv;       // 먹이 생성 시간 레벨
        public int foodUpgradeLv;                   // 먹이 업그레이드1 레벨
        public int foodUpgrade2Lv;                  // 먹이 업그레이드2 레벨
        public int autoCollectingLv;                // 자동 먹이주기 레벨

        public int minFoodLv;                       // 먹이 최소 레벨
        public int maxFoodLv;                       // 먹이 최대 레벨
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            maxCatsLv = this.maxCatsLv,
            reduceCollectingTimeLv = this.reduceCollectingTimeLv,
            maxFoodsLv = this.maxFoodsLv,
            reduceProducingFoodTimeLv = this.reduceProducingFoodTimeLv,
            foodUpgradeLv = this.foodUpgradeLv,
            foodUpgrade2Lv = this.foodUpgrade2Lv,
            autoCollectingLv = this.autoCollectingLv,
            minFoodLv = this.minFoodLv,
            maxFoodLv = this.maxFoodLv
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.maxCatsLv = savedData.maxCatsLv;
        this.reduceCollectingTimeLv = savedData.reduceCollectingTimeLv;
        this.maxFoodsLv = savedData.maxFoodsLv;
        this.reduceProducingFoodTimeLv = savedData.reduceProducingFoodTimeLv;
        this.foodUpgradeLv = savedData.foodUpgradeLv;
        this.foodUpgrade2Lv = savedData.foodUpgrade2Lv;
        this.autoCollectingLv = savedData.autoCollectingLv;
        this.minFoodLv = savedData.minFoodLv;
        this.maxFoodLv = savedData.maxFoodLv;

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

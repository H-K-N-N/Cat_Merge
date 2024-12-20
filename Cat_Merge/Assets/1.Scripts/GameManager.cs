using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }        // SingleTon

    // Data
    private Cat[] allCatData;                                       // 모든 고양이 데이터 보유
    public Cat[] AllCatData => allCatData;
    private bool[] isCatUnlocked;                                   // 고양이 해금 여부 배열

    // ======================================================================================================================

    // 퀘스트를 위한 변수들
    private int feedCount;                                          // 고양이 스폰 횟수(먹이 준 횟수)
    public int FeedCount { get => feedCount; set => feedCount = value; }

    private int combineCount;                                       // 고양이 머지 횟수
    public int CombineCount { get => combineCount; set => combineCount = value; }

    private int getCoinCount;                                       // 획득코인 갯수
    public int GetCoinCount { get => getCoinCount; set => getCoinCount = value; }

    private float playTimeCount;                                    // 플레이타임 카운트
    public float PlayTimeCount { get => playTimeCount; set => playTimeCount = value; }

    private int purchaseCatsCount;                                  // 고양이 구매 횟수
    public int PurchaseCatsCount { get => purchaseCatsCount; set => purchaseCatsCount = value; }

    // ======================================================================================================================

    // Main UI Text
    [Header("---[Main UI Text]")]
    [SerializeField] private TextMeshProUGUI catCountText;          // 고양이 수 텍스트
    private int currentCatCount = 0;                                // 화면 내 고양이 수
    private int maxCats = 8;                                        // 최대 고양이 수

    // 기본 재화
    [SerializeField] private TextMeshProUGUI coinText;              // 기본재화 텍스트
    private int coin = 1000;                                        // 기본재화

    // 캐쉬 재화
    [SerializeField] private TextMeshProUGUI cashText;              // 캐쉬재화 텍스트
    private int cash = 1000;                                        // 캐쉬재화

    // ======================================================================================================================

    // Merge On/Off
    [Header("---[Merge On/Off]")]
    [SerializeField] private Button openMergePanelButton;           // 머지 패널 열기 버튼
    [SerializeField] private GameObject mergePanel;                 // 머지 On/Off 패널
    [SerializeField] private Button closeMergePanelButton;          // 머지 패널 닫기 버튼
    [SerializeField] private Button mergeStateButton;               // 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI mergeStateText;        // 머지 현재 상태 텍스트
    private bool isMergeEnabled = true;

    // ======================================================================================================================

    // AutoMove On/Off
    [Header("----[AutoMove On/Off]")]
    [SerializeField] private Button openAutoMovePanelButton;        // 자동 이동 패널 열기 버튼
    [SerializeField] private GameObject autoMovePanel;              // 자동 이동 On/Off 패널
    [SerializeField] private Button closeAutoMovePanelButton;       // 자동 이동 패널 닫기 버튼
    [SerializeField] private Button autoMoveStateButton;            // 자동 이동 상태 버튼
    [SerializeField] private TextMeshProUGUI autoMoveStateText;     // 자동 이동 현재 상태 텍스트
    private bool isAutoMoveEnabled = true;                          // 자동 이동 활성화 상태

    // ======================================================================================================================

    // AutoMerge
    [Header("---[AutoMerge]")]
    [SerializeField] private Button openAutoMergePanelButton;       // 자동 머지 패널 열기 버튼
    [SerializeField] private GameObject autoMergePanel;             // 자동 머지 패널
    [SerializeField] private Button closeAutoMergePanelButton;      // 자동 머지 패널 닫기 버튼
    [SerializeField] private Button autoMergeStateButton;           // 자동 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // 자동 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // 자동 머지 타이머 텍스트
    private int autoMergeCost = 30;                                 // 자동 머지 비용

    // Sort System
    [Header("---[Sort]")]
    [SerializeField] private Button sortButton;                     // 정렬 버튼
    [SerializeField] private Transform gamePanel;                   // GamePanel

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
    [SerializeField] private Button[] itemMenuButtons;              // itemPanel에서의 아이템 메뉴 버튼들
    [SerializeField] private GameObject[] itemMenues;               // itemPanel에서의 메뉴 스크롤창 판넬
    private bool[] isItemMenuButtonsOn = new bool[4];               // 버튼 색상 감지하기 위한 bool타입 배열


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
    [SerializeField] private Button bottomBuyCatButton;               // 고양이 구매 버튼
    [SerializeField] private Image bottomBuyCatButtonImg;             // 고양이 구매 버튼 이미지
    [SerializeField] private GameObject buyCatCoinDisabledBg;         // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject buyCatCashDisabledBg;         // 버튼 클릭 못할 때의 배경
    [SerializeField] private GameObject buyCatMenuPanel;              // 아이템 메뉴 판넬
    private bool isOnToggleBuyCat;                                    // 아이템 메뉴 판넬 토글
    [SerializeField] private Button buyCatBackButton;                 // 고양이 구매 메뉴 뒤로가기 버튼

    [SerializeField] private Button buyCatCoinButton;                // 고양이 재화로 구매 버튼
    [SerializeField] private Button buyCatCashButton;                // 크리스탈 재화로 구매 버튼

    [SerializeField] private TextMeshProUGUI buyCatCountExplainText;  // 고양이 구매 횟수 설명창
    [SerializeField] private TextMeshProUGUI buyCatCoinFeeText;       // 고양이 구매 비용 (골드)
    [SerializeField] private TextMeshProUGUI buyCatCashFeeText;       // 고양이 구매 비용 (크리스탈)
    private int buyCatCoinCount = 0;                                  // 고양이 구매 횟수(코인)
    private int buyCatCashCount = 0;                                  // 고양이 구매 횟수(크리스탈)
    private int buyCatCoinFee = 5;                                    // 고양이 구매 비용 (코인)
    private int buyCatCashFee = 5;                                    // 고양이 구매 비용 (크리스탈)

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

        // 자동이동 On/Off 관련
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMove);

        // 머지 On/Off 관련
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);

        // 정렬 관련
        sortButton.onClick.AddListener(SortCats);

        // 자동머지 관련
        UpdateAutoMergeCostText();
        openAutoMergePanelButton.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton.onClick.AddListener(StartAutoMerge);
        UpdateAutoMergeTimerVisibility(false);

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

    // 도감 해금 관련

    private void Start()
    {
        LoadUnlockedCats();
    }

    // 모든 해금 상태 불러오기
    public void LoadUnlockedCats()
    {
        InitializeCatUnlockData();
    }

    // 고양이 해금 여부 초기화
    public void InitializeCatUnlockData()
    {
        isCatUnlocked = new bool[AllCatData.Length];

        // 모든 고양이를 잠금 상태로 초기화
        for (int i = 0; i < isCatUnlocked.Length; i++)
        {
            isCatUnlocked[i] = false;
        }
    }

    // 특정 고양이를 해금
    public void UnlockCat(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length || isCatUnlocked[CatGrade])
        {
            return;
        }

        isCatUnlocked[CatGrade] = true;
        SaveUnlockedCats(CatGrade);
    }

    // 특정 고양이의 해금 여부 확인
    public bool IsCatUnlocked(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length)
        {
            return false;
        }

        return isCatUnlocked[CatGrade];
    }

    // 모든 해금 상태 저장
    public void SaveUnlockedCats(int CatGrade)
    {
        GetComponent<DictionaryManager>().UpdateDictionary(CatGrade);
        GetComponent<DictionaryManager>().ShowNewCatPanel(CatGrade);
    }

    // ======================================================================================================================

    // 퀘스트 관련

    private void Update()
    {
        AddPlayTimeCount();
    }

    public void AddCash(int amount)
    {
        cash += amount;
    }

    public void AddFeedCount()
    {
        FeedCount++;
    }

    public void ResetFeedCount(int count)
    {
        FeedCount = count;
    }

    public void AddCombineCount()
    {
        CombineCount++;
    }

    public void ResetCombineCount(int count)
    {
        CombineCount = count;
    }

    public void AddGetCoinCount(int count)
    {   // 고양이가 재화를 자동으로 얻을때마다 호출
        GetCoinCount += count;
    }

    public void ResetGetCoinCount(int count)
    {
        GetCoinCount = count;
    }

    public void AddPlayTimeCount()
    {
        PlayTimeCount += Time.deltaTime;
    }

    public void ResetPlayTime(int count)
    {
        PlayTimeCount = count;
    }

    public void AddPurchaseCatsCount()
    {   // 고양이를 구매할때 호출
        PurchaseCatsCount++;
    }

    public void ResetPurchaseCatsCount(int count)
    {
        PurchaseCatsCount = count;
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

    // 머지 On/Off 패널 여는 함수
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // 머지 상태 전환 함수
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    // 머지 상태 Text 업데이트 함수
    private void UpdateMergeStateText()
    {
        if (mergeStateText != null)
        {
            mergeStateText.text = isMergeEnabled ? "OFF" : "ON";
        }
    }

    // 머지 버튼 색상 업데이트 함수
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            Color normalColor = isMergeEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openMergePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openMergePanelButton.colors = colors;
        }
    }

    // 머지 패널 닫는 함수
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // 머지 상태 반환 함수
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }

    // ======================================================================================================================

    // 자동이동 패널 여는 함수
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // 자동이동 상태 전환 함수
    public void ToggleAutoMove()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;

        // 모든 고양이에 상태 적용
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        // 상태 업데이트 및 버튼 색상 변경
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();

        // 패널 닫기
        CloseAutoMovePanel();
    }

    // 자동이동 상태 Text 업데이트 함수
    private void UpdateAutoMoveStateText()
    {
        if (autoMoveStateText != null)
        {
            autoMoveStateText.text = isAutoMoveEnabled ? "OFF" : "ON";
        }
    }

    // 자동이동 버튼 색상 업데이트 함수
    private void UpdateOpenButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            Color normalColor = isAutoMoveEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openAutoMovePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openAutoMovePanelButton.colors = colors;
        }
    }

    // 자동이동 패널 닫는 함수
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // 자동이동 현재 상태 반환하는 함수
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

    // ======================================================================================================================

    // 고양이 정렬 함수
    private void SortCats()
    {
        StartCoroutine(SortCatsCoroutine());
    }

    // 고양이들을 정렬된 위치에 배치하는 코루틴
    private IEnumerator SortCatsCoroutine()
    {
        // 정렬 전 자동 이동 잠시 중지 (현재 자동이동이 진행중인 고양이도 중지 = CatData의 AutoMove가 실행중이어도 중지)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(false); // 자동 이동 비활성화
            }
        }

        // 고양이 객체들을 등급을 기준으로 정렬 (높은 등급이 먼저 오도록)
        List<GameObject> sortedCats = new List<GameObject>();
        foreach (Transform child in gamePanel)
        {
            sortedCats.Add(child.gameObject);
        }

        sortedCats.Sort((cat1, cat2) =>
        {
            int grade1 = GetCatGrade(cat1);
            int grade2 = GetCatGrade(cat2);

            if (grade1 == grade2) return 0;
            return grade1 > grade2 ? -1 : 1;
        });

        // 고양이 이동 코루틴 실행
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < sortedCats.Count; i++)
        {
            GameObject cat = sortedCats[i];
            Coroutine moveCoroutine = StartCoroutine(MoveCatToPosition(cat, i));
            moveCoroutines.Add(moveCoroutine);
        }

        // 모든 고양이가 이동을 마칠 때까지 기다리기
        foreach (Coroutine coroutine in moveCoroutines)
        {
            yield return coroutine;
        }

        // 정렬 후 자동 이동 상태 복구 (정렬이 완료되고 고양이들이 다시 주기마다 자동이동이 가능하게 원래상태로 복구)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(isAutoMoveEnabled);
            }
        }
    }

    // 고양이의 등급을 반환하는 함수
    private int GetCatGrade(GameObject catObject)
    {
        int grade = catObject.GetComponent<CatData>().catData.CatGrade;
        return grade;
    }

    // 고양이들을 부드럽게 정렬된 위치로 이동시키는 함수
    private IEnumerator MoveCatToPosition(GameObject catObject, int index)
    {
        RectTransform rectTransform = catObject.GetComponent<RectTransform>();

        // 목표 위치 계산 (index는 정렬된 순서)
        float targetX = (index % 7 - 3) * (rectTransform.rect.width + 10);
        float targetY = (index / 7) * (rectTransform.rect.height + 10);
        Vector2 targetPosition = new Vector2(targetX, -targetY);

        // 현재 위치와 목표 위치의 차이를 계산하여 부드럽게 이동
        float elapsedTime = 0f;
        float duration = 0.1f;          // 이동 시간 (초)

        Vector2 initialPosition = rectTransform.anchoredPosition;

        // 목표 위치로 부드럽게 이동
        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 이동이 끝났을 때 정확한 목표 위치에 도달
        rectTransform.anchoredPosition = targetPosition;
    }

    // ======================================================================================================================

    // 자동머지 비용 Text 업데이트 함수
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = $"{autoMergeCost}";
        }
    }

    // 자동머지 패널 여는 함수
    private void OpenAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(true);
        }
    }

    // 자동머지 패널 닫는 함수
    private void CloseAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // 자동머지 시작 함수
    private void StartAutoMerge()
    {
        if (cash >= autoMergeCost)
        {
            cash -= autoMergeCost;
            UpdateCashText();

            AutoMerge autoMergeScript = FindObjectOfType<AutoMerge>();
            if (autoMergeScript != null)
            {
                autoMergeScript.OnClickedAutoMerge();
            }
        }
        else
        {
            Debug.Log("Not enough coins to start AutoMerge!");
        }
    }

    // 자동머지 상태에 따라 타이머 텍스트 가시성 업데이트 함수
    public void UpdateAutoMergeTimerVisibility(bool isVisible)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.gameObject.SetActive(isVisible);
        }
    }

    // 자동머지 타이머 업데이트 함수
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}";
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
                //if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColor))
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
            //if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColor))
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
            if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColor))
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
                        if (ColorUtility.TryParseHtmlString("#E2E2E2", out Color parsedColorF))
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

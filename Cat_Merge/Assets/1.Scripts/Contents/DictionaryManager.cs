using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

// 고양이 도감 스크립트
[DefaultExecutionOrder(-6)]
public class DictionaryManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static DictionaryManager Instance { get; private set; }

    [Header("---[Dictionary Manager]")]
    [SerializeField] private ScrollRect[] dictionaryScrollRects;    // 도감의 스크롤뷰 배열
    [SerializeField] private Button dictionaryButton;               // 도감 버튼
    [SerializeField] private Image dictionaryButtonImage;           // 도감 버튼 이미지
    [SerializeField] private GameObject dictionaryMenuPanel;        // 도감 메뉴 Panel
    [SerializeField] private Button dictionaryBackButton;           // 도감 뒤로가기 버튼
    private ActivePanelManager activePanelManager;                  // ActivePanelManager

    [SerializeField] private GameObject slotPrefab;                 // 도감 슬롯 프리팹
    [SerializeField] private GameObject[] dictionaryMenus;          // 도감 메뉴 Panels
    [SerializeField] private Button[] dictionaryMenuButtons;        // 도감의 서브 메뉴 버튼 배열

    [SerializeField] private GameObject dictionaryButtonNewImage;   // 도감 Button의 New Image
    [SerializeField] private GameObject normalCatButtonNewImage;    // Normal Cat Button의 New Image
    private event Action OnCatDataChanged;                          // 이벤트 정의

    // 도감 해금 관련 변수
    private bool[] isCatUnlocked;                                   // 고양이 해금 여부 배열
    private bool[] isGetFirstUnlockedReward;                        // 고양이 첫 해금 보상 획득 여부 배열

    private int currentSelectedCatGrade;                            // 현재 선택된 고양이 등급 추적

    private static readonly Vector2 defaultTextPosition = new Vector2(0, 0);
    private static readonly Vector2 unlockedTextPosition = new Vector2(0, 100);
    private static readonly Color transparentIconColor = new Color(1f, 1f, 1f, 0f);
    private static readonly Color visibleIconColor = new Color(1f, 1f, 1f, 1f);


    [Header("---[New Cat Panel UI]")]
    [SerializeField] private GameObject newCatPanel;                // New Cat Panel
    [SerializeField] private Image newCatHighlightImage;            // New Cat Highlight Image
    [SerializeField] private Image newCatIcon;                      // New Cat Icon
    [SerializeField] private TextMeshProUGUI newCatName;            // New Cat Name Text
    [SerializeField] private TextMeshProUGUI newCatExplain;         // New Cat Explanation Text
    [SerializeField] private TextMeshProUGUI newCatGetCoin;         // New Cat Get Coin Text
    [SerializeField] private Button submitButton;                   // New Cat Panel Submit Button
    private Coroutine highlightRotationCoroutine;                   // Highlight 회전 코루틴 관리용 변수

    // Highlight Image 회전에 사용할 Vector3 캐싱
    private static readonly Vector3 rotationVector = new Vector3(0, 0, 1);
    private static readonly float rotationSpeed = 90f;

    // Enum으로 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum DictionaryMenuType
    {
        Normal,                 // 일반 고양이 메뉴
        Rare,                   // 희귀 고양이 메뉴
        Special,                // 특수 고양이 메뉴
        Background,             // 배경 메뉴
        End                     // Enum의 끝
    }
    private DictionaryMenuType activeMenuType;                      // 현재 활성화된 메뉴 타입


    [Header("---[Information Panel UI]")]
    [SerializeField] private Image informationCatIcon;              // Information Cat Icon
    [SerializeField] private TextMeshProUGUI informationCatDetails; // informationCatDetails Text
    [SerializeField] private GameObject catInformationPanel;        // catInformation Panel (상세정보 칸 Panel)
    [SerializeField] private RectTransform fullInformationPanel;    // fullInformation Panel (상세정보 스크롤 Panel)
    private static readonly Vector2 defaultInformationPanelPosition = new Vector2(0, -312.5f);  // Information Panel 위치 상수


    [Header("---[ScrollRect Transform]")]
    private const int SPACING_Y = 30;                               // y spacing
    private const int CELL_SIZE_Y = 280;                            // y cell size
    private const int VIEWPORT_HEIGHT = 680;                        // viewport height


    // 임시 (다른 서브 메뉴들을 추가한다면 어떻게 정리 할까 고민)
    [Header("---[Sub Contents]")]
    [SerializeField] private Transform scrollRectContents;          // 노말 고양이 scrollRectContents (도감에서 노말 고양이의 정보를 초기화 하기 위해)
                                                                    // 희귀 고양이 scrollRectContents
                                                                    // 특수 고양이 scrollRectContents


    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";             // 비활성화상태 Color


    [Header("---[ETC]")]
    private bool isDataLoaded = false;                              // 데이터 로드 확인

    #endregion


    #region Unity Menthods

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
        InitializeDictionaryManager();

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            InitializeCatUnlockData();
            PopulateDictionary();
        }

        InitializeNewImage();
        InitializeScrollPositions();
    }

    private void OnDestroy()
    {
        // 이벤트 핸들러 해제
        OnCatDataChanged -= UpdateNewImageStatus;
    }

    #endregion


    #region Initialize

    // 기본 상태 초기화
    private void InitializeDefaultValues()
    {
        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;
    }

    // 모든 구성요소 초기화
    private void InitializeDictionaryManager()
    {
        InitializeDefaultValues();
        InitializeActivePanel();

        InitializeDictionaryButton();

        InitializeSubMenuButtons();
    }

    // New 이미지 관련 이벤트 및 UI 초기화
    private void InitializeNewImage()
    {
        // 이벤트 등록 (상태가 변경될 때 UpdateNewImageStatus 호출)
        OnCatDataChanged += UpdateNewImageStatus;

        // 초기화 시 New Image UI 갱신
        UpdateNewImageStatus();
    }

    // 초기 스크롤 위치 초기화 함수
    private void InitializeScrollPositions()
    {
        foreach (var scrollRect in dictionaryScrollRects)
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                // 전체 슬롯 개수를 3의 배수로 올림 계산
                int totalSlots = GameManager.Instance.AllCatData.Length;
                int adjustedTotalSlots = Mathf.CeilToInt(totalSlots / 3f) * 3;

                // 행의 개수 계산 (3개씩 한 줄)
                int rowCount = adjustedTotalSlots / 3;

                // Grid Layout Group 설정값
                const int SPACING_Y = 30;           // y spacing
                const int CELL_SIZE_Y = 280;        // y cell size
                const int VIEWPORT_HEIGHT = 680;    // viewport height

                // 전체 컨텐츠 높이 계산: (행 간격 * (행 개수-1)) + (셀 높이 * 행 개수)
                float contentHeight = (SPACING_Y * (rowCount - 1)) + (CELL_SIZE_Y * rowCount);

                // 스크롤 시작 위치 계산: -(전체 높이 - 뷰포트 높이) * 0.5f
                float targetY = -(contentHeight - VIEWPORT_HEIGHT) * 0.5f;

                // 스크롤 컨텐츠의 위치를 상단으로 설정
                scrollRect.content.anchoredPosition = new Vector2(
                    scrollRect.content.anchoredPosition.x,
                    targetY
                );

                // 스크롤 속도 초기화
                scrollRect.velocity = Vector2.zero;
            }
        }
    }

    // ActivePanel 초기화 함수
    private void InitializeActivePanel()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("DictionaryMenu", dictionaryMenuPanel, dictionaryButtonImage);

        // 메인 튜토리얼 완료 여부에 따라 버튼 상호작용 설정
        if (TutorialManager.Instance != null)
        {
            dictionaryButton.interactable = TutorialManager.Instance.IsMainTutorialEnd;
        }

        dictionaryButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("DictionaryMenu");

            // DictionaryMenu가 활성화될 때만 실행
            if (activePanelManager.ActivePanelName == "DictionaryMenu")
            {
                // 도감 패널이 처음 열렸을 때 튜토리얼 실행
                if (TutorialManager.Instance != null && !TutorialManager.Instance.IsDictionaryTutorialEnd)
                {
                    TutorialManager.Instance.StartDictionaryTutorial();
                }
                UpdateInformationPanel();
            }
        });
        dictionaryBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("DictionaryMenu"));
    }

    // 도감 버튼 상호작용 업데이트 함수 추가
    public void UpdateDictionaryButtonInteractable()
    {
        if (TutorialManager.Instance != null && dictionaryButton != null)
        {
            dictionaryButton.interactable = TutorialManager.Instance.IsMainTutorialEnd;
        }
    }

    // DictionaryButton 초기화 함수
    private void InitializeDictionaryButton()
    {
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    #endregion


    #region Cat Unlock System

    // 모든 해금 상태 초기화 함수
    private void InitializeCatUnlockData()
    {
        int allCatDataLength = GameManager.Instance.AllCatData.Length;

        isCatUnlocked = new bool[allCatDataLength];
        isGetFirstUnlockedReward = new bool[allCatDataLength];

        // 모든 고양이를 잠금 상태로 초기화 & 첫 보상 획득 false로 초기화
        for (int i = 0; i < isCatUnlocked.Length; i++)
        {
            isCatUnlocked[i] = false;
            isGetFirstUnlockedReward[i] = false;
        }
    }

    // 특정 고양이 해금 함수
    public void UnlockCat(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length || isCatUnlocked[CatGrade]) return;

        isCatUnlocked[CatGrade] = true;
        SaveUnlockedCats(CatGrade);

        // 이벤트 발생
        OnCatDataChanged?.Invoke();

        // BuyCatManager의 구매 슬롯 상태 업데이트
        BuyCatManager.Instance?.UnlockBuySlot(CatGrade);
    }

    // 특정 고양이의 해금 여부 확인 함수
    public bool IsCatUnlocked(int catGrade)
    {
        return isCatUnlocked[catGrade];
    }

    // 특정 고양이의 첫 해금 보상 획득 함수
    public void GetFirstUnlockedReward(int catGrade)
    {
        if (catGrade < 0 || catGrade >= isGetFirstUnlockedReward.Length || isGetFirstUnlockedReward[catGrade])
        {
            return;
        }

        isGetFirstUnlockedReward[catGrade] = true;
        QuestManager.Instance.AddCash(GameManager.Instance.AllCatData[catGrade].CatFirstOpenCash);

        // 이벤트 발생
        OnCatDataChanged?.Invoke();
    }

    // 특정 고양이의 첫 해금 보상 획득 여부 확인 함수
    public bool IsGetFirstUnlockedReward(int catGrade)
    {
        return isGetFirstUnlockedReward[catGrade];
    }

    // 모든 해금 상태 저장 함수
    public void SaveUnlockedCats(int CatGrade)
    {
        GetComponent<DictionaryManager>().UpdateDictionary(CatGrade);
        GetComponent<DictionaryManager>().ShowNewCatPanel(CatGrade);
    }

    #endregion


    #region 도감 및 UI

    // 도감 데이터를 채우는 함수
    private void PopulateDictionary()
    {
        if (GameManager.Instance.AllCatData == null || GameManager.Instance.AllCatData.Length == 0) return;

        foreach (Transform child in scrollRectContents)
        {
            Destroy(child.gameObject);
        }

        foreach (Cat cat in GameManager.Instance.AllCatData)
        {
            InitializeSlot(cat);
        }
    }

    // 고양이 데이터를 바탕으로 초기 슬롯을 생성하는 함수
    private void InitializeSlot(Cat cat)
    {
        GameObject slot = Instantiate(slotPrefab, scrollRectContents);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI text = slot.transform.Find("Button/Name Text")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();
        GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;

        RectTransform textRect = text.GetComponent<RectTransform>();

        // CatGrade는 1부터 시작하므로 배열 인덱스로 변환
        int catIndex = cat.CatGrade - 1;

        if (IsCatUnlocked(catIndex))
        {
            button.interactable = true;
            text.text = $"{cat.CatGrade}. {cat.CatName}";
            iconImage.sprite = cat.CatImage;
            iconImage.color = visibleIconColor;

            friendshipNewImage.SetActive(FriendshipManager.Instance.HasUnclaimedFriendshipRewards(cat.CatGrade));

            // 첫 해금 보상 확인 시에도 인덱스 사용
            if (IsGetFirstUnlockedReward(catIndex))
            {
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                firstOpenBG.gameObject.SetActive(true);
                firstOpenCashtext.text = $"+{GameManager.Instance.FormatNumber(cat.CatFirstOpenCash)}";
            }

            // 버튼 클릭 이벤트 설정
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (!IsGetFirstUnlockedReward(catIndex))
                {
                    GetFirstUnlockedReward(catIndex);
                    firstOpenBG.gameObject.SetActive(false);
                }
                else
                {
                    ShowInformationPanel(catIndex);
                }
            });

            // 버튼 SFX 등록
            if (OptionManager.Instance != null)
            {
                button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
            }
        }
        else
        {
            button.interactable = false;
            text.text = "???";
            textRect.anchoredPosition = defaultTextPosition;
            iconImage.sprite = cat.CatImage;
            iconImage.color = transparentIconColor;
            firstOpenBG.gameObject.SetActive(false);
            friendshipNewImage.SetActive(false);
        }
    }

    // 새로운 고양이를 해금하면 도감을 업데이트하는 함수
    public void UpdateDictionary(int catGrade)
    {
        // catGrade는 이미 0-based index이므로 그대로 사용
        Transform slot = scrollRectContents.GetChild(catGrade);

        slot.gameObject.SetActive(true);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI text = slot.transform.Find("Button/Name Text")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();
        GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;

        RectTransform textRect = text.GetComponent<RectTransform>();

        button.interactable = true;

        Cat catData = GameManager.Instance.AllCatData[catGrade];
        iconImage.sprite = catData.CatImage;
        iconImage.color = visibleIconColor;

        friendshipNewImage.SetActive(FriendshipManager.Instance.HasUnclaimedFriendshipRewards(catGrade + 1));

        // 표시되는 등급은 1-based
        text.text = $"{catGrade + 1}. {catData.CatName}";
        textRect.anchoredPosition = unlockedTextPosition;

        if (!IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {GameManager.Instance.FormatNumber(catData.CatFirstOpenCash)}";
        }
        else
        {
            firstOpenBG.gameObject.SetActive(false);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (!IsGetFirstUnlockedReward(catGrade))
            {
                GetFirstUnlockedReward(catGrade);
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                ShowInformationPanel(catGrade);
            }
        });

        // 버튼 SFX 등록
        if (OptionManager.Instance != null)
        {
            button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
        }
    }

    // 도감이 활성화 되거나 서브 메뉴를 누르면 InformationPanel을 기본 정보로 업데이트 해주는 함수
    private void UpdateInformationPanel()
    {
        currentSelectedCatGrade = -1;

        // 이미지 설정
        informationCatIcon.gameObject.SetActive(false);

        // 버튼 이벤트 초기화
        Button iconButton = informationCatIcon.GetComponent<Button>();
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
        }

        // 텍스트 설정
        informationCatDetails.text = $"고양이를 선택하세요\n";

        // 스크롤 비활성화
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // 초기 catInformation Mask 초기화
        catInformationPanel.GetComponent<Mask>().enabled = true;

        // fullInformationPanel의 Y좌표를 -312.5f로 고정
        fullInformationPanel.anchoredPosition = defaultInformationPanelPosition;

        // 애정도 UI 초기화
        FriendshipManager.Instance.ResetUI();
    }

    // 고양이 정보를 Information Panel에 표시하는 함수
    private void ShowInformationPanel(int catGrade)
    {
        // catGrade는 0-based index이므로, UI에 표시할 때 1을 더함
        currentSelectedCatGrade = catGrade + 1;
        var catData = GameManager.Instance.AllCatData[catGrade];

        // 기존 정보 표시 코드
        informationCatIcon.gameObject.SetActive(true);
        informationCatIcon.sprite = catData.CatImage;

        // 아이콘 버튼 클릭 이벤트 설정
        Button iconButton = informationCatIcon.GetComponent<Button>();
        if (iconButton != null)
        {
            iconButton.onClick.RemoveAllListeners();
            iconButton.onClick.AddListener(() => ShowNewCatPanel(catGrade));

            // 버튼 SFX 등록
            if (OptionManager.Instance != null)
            {
                iconButton.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
            }
        }

        string catInfo = $"이름: {catData.CatName}\n" +
                         $"등급: {catData.CatGrade}\n" +
                         $"공격력: {catData.BaseDamage}%\n" +
                         $"체력: {catData.CatHp}\n" +
                         $"재화수급량: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // 스크롤 설정
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
        fullInformationPanel.anchoredPosition = defaultInformationPanelPosition;

        // 애정도 시스템 업데이트 호출
        FriendshipManager.Instance.OnCatSelected(currentSelectedCatGrade);
    }

    // 현재 선택된 고양이 등급 반환 함수 추가
    public int GetCurrentSelectedCatGrade()
    {
        return currentSelectedCatGrade;
    }

    #endregion


    #region New Cat Panel

    // 새로운 고양이 해금 효과 함수
    public void ShowNewCatPanel(int catGrade)
    {
        // 기존 회전 코루틴이 있다면 중지
        if (highlightRotationCoroutine != null)
        {
            StopCoroutine(highlightRotationCoroutine);
            highlightRotationCoroutine = null;
        }

        Cat newCat = GameManager.Instance.AllCatData[catGrade];

        newCatPanel.SetActive(true);

        newCatIcon.sprite = newCat.CatImage;
        newCatName.text = newCat.CatGrade.ToString() + ". " + newCat.CatName;
        newCatExplain.text = newCat.CatExplain;
        newCatGetCoin.text = "재화 획득량: " + newCat.CatGetCoin.ToString();

        // Highlight Image 회전 애니메이션 시작
        highlightRotationCoroutine = StartCoroutine(RotateHighlightImage());

        // 확인 버튼을 누르면 패널을 비활성화
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // Highlight Image 회전 애니메이션 코루틴
    private IEnumerator RotateHighlightImage()
    {
        // 회전 각도 초기화
        if (newCatHighlightImage != null)
        {
            newCatHighlightImage.transform.rotation = Quaternion.identity;
        }

        while (newCatPanel.activeSelf)
        {
            if (newCatHighlightImage != null)
            {
                newCatHighlightImage.transform.Rotate(rotationVector, rotationSpeed * Time.deltaTime);
            }
            yield return null;
        }

        highlightRotationCoroutine = null;
    }

    // New Cat Panel을 닫는 함수
    private void CloseNewCatPanel()
    {
        if (highlightRotationCoroutine != null)
        {
            StopCoroutine(highlightRotationCoroutine);
            highlightRotationCoroutine = null;
        }

        newCatPanel.SetActive(false);
    }

    #endregion


    #region Sub Menus

    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)DictionaryMenuType.End; i++)
        {
            int index = i;
            dictionaryMenuButtons[index].onClick.AddListener(() => ActivateMenu((DictionaryMenuType)index));
        }

        ActivateMenu(DictionaryMenuType.Normal);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(DictionaryMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < dictionaryMenus.Length; i++)
        {
            dictionaryMenus[i].SetActive(i == (int)menuType);
        }

        UpdateInformationPanel();
        UpdateSubMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < dictionaryMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(dictionaryMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // 서브 메뉴 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    #endregion


    #region Notification System

    // 보상을 받을 수 있는 상태를 확인하는 함수
    public bool HasUnclaimedRewards()
    {
        for (int i = 0; i < isCatUnlocked.Length; i++)
        {
            if (isCatUnlocked[i] && !isGetFirstUnlockedReward[i])
            {
                return true;
            }
        }
        return false;
    }

    // 애정도 보상을 받을 수 있는 고양이가 있는지 확인하는 함수 (friendship New Image)
    private bool HasUnclaimedFriendshipRewards()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            if (IsCatUnlocked(i - 1) && FriendshipManager.Instance.HasUnclaimedFriendshipRewards(i))
            {
                return true;
            }
        }
        return false;
    }

    // New Image UI를 갱신하는 함수
    public void UpdateNewImageStatus()
    {
        bool hasUnclaimedRewards = HasUnclaimedRewards();
        bool hasUnclaimedFriendshipRewards = HasUnclaimedFriendshipRewards();

        // 도감 Button의 New Image 활성화/비활성화
        if (dictionaryButtonNewImage != null)
        {
            dictionaryButtonNewImage.SetActive(hasUnclaimedRewards || hasUnclaimedFriendshipRewards);
        }

        // Normal Cat Button의 New Image 활성화/비활성화
        if (normalCatButtonNewImage != null)
        {
            normalCatButtonNewImage.SetActive(hasUnclaimedRewards || hasUnclaimedFriendshipRewards);
        }
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool[] isCatUnlocked;                   // 고양이 해금 상태
        public bool[] isGetFirstUnlockedReward;        // 첫 해금 보상 수령 상태
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isCatUnlocked = this.isCatUnlocked,
            isGetFirstUnlockedReward = this.isGetFirstUnlockedReward
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        LoadCatUnlockData(savedData);
        PopulateDictionary();

        UpdateNewImageStatus();

        isDataLoaded = true;
    }

    private void LoadCatUnlockData(SaveData savedData)
    {
        // 데이터 복원
        int length = GameManager.Instance.AllCatData.Length;
        isCatUnlocked = new bool[length];
        isGetFirstUnlockedReward = new bool[length];

        for (int i = 0; i < length; i++)
        {
            isCatUnlocked[i] = savedData.isCatUnlocked[i];
            isGetFirstUnlockedReward[i] = savedData.isGetFirstUnlockedReward[i];
        }
    }

    #endregion


}

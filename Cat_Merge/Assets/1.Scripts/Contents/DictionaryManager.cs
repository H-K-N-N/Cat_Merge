using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

// 고양이 도감 Script
public class DictionaryManager : MonoBehaviour
{
    // Singleton Instance
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

    // ======================================================================================================================

    // 도감 해금 관련 변수
    private bool[] isCatUnlocked;                                   // 고양이 해금 여부 배열
    private bool[] isGetFirstUnlockedReward;                        // 고양이 첫 해금 보상 획득 여부 배열

    // ======================================================================================================================

    [Header("---[New Cat Panel UI]")]
    [SerializeField] private GameObject newCatPanel;                // New Cat Panel
    [SerializeField] private Image newCatIcon;                      // New Cat Icon
    [SerializeField] private TextMeshProUGUI newCatName;            // New Cat Name Text
    [SerializeField] private TextMeshProUGUI newCatExplain;         // New Cat Explanation Text
    [SerializeField] private TextMeshProUGUI newCatGetCoin;         // New Cat Get Coin Text
    [SerializeField] private Button submitButton;                   // New Cat Panel Submit Button

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

    // 임시 (다른 서브 메뉴들을 추가한다면 어떻게 정리 할까 고민)
    [Header("---[Sub Contents]")]
    [SerializeField] private Transform scrollRectContents;          // 노말 고양이 scrollRectContents (도감에서 노말 고양이의 정보를 초기화 하기 위해)
                                                                    // 희귀 고양이 scrollRectContents
                                                                    // 특수 고양이 scrollRectContents

    // ======================================================================================================================

    [Header("---[Friendship UnLock Buttons]")]
    [SerializeField] private Button[] friendshipUnlockButtons;      // 
    [SerializeField] public GameObject[] friendshipGetCrystalImg;   // 
    [SerializeField] public GameObject[] friendshipLockImg;         // 
    [SerializeField] public GameObject[] friendshipStarImg;         // 배경 scrollRectContents

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
        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        InitializeDictionaryManager();

        InitializeFriendshipButton();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("DictionaryMenu", dictionaryMenuPanel, dictionaryButtonImage);
    }

    // ======================================================================================================================
    // [초기 설정]

    // 모든 시작 함수들
    private void InitializeDictionaryManager()
    {
        LoadUnlockedCats();
        ResetScrollPositions();
        InitializeDictionaryButton();
        InitializeSubMenuButtons();
        PopulateDictionary();

        // 이벤트 등록 (상태가 변경될 때 UpdateNewImageStatus 호출)
        OnCatDataChanged += UpdateNewImageStatus;

        // 초기화 시 New Image UI 갱신
        UpdateNewImageStatus();
    }

    // ======================================================================================================================
    // [해금 관련]

    // 모든 해금 상태를 불러오는 함수
    public void LoadUnlockedCats()
    {
        InitializeCatUnlockData();
    }

    // 고양이 해금 여부 초기화 함수
    public void InitializeCatUnlockData()
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

    // 특정 고양이를 해금하는 함수
    public void UnlockCat(int CatGrade)
    {
        if (CatGrade < 0 || CatGrade >= isCatUnlocked.Length || isCatUnlocked[CatGrade])
        {
            return;
        }

        isCatUnlocked[CatGrade] = true;
        SaveUnlockedCats(CatGrade);

        // 이벤트 발생
        OnCatDataChanged?.Invoke();
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
        QuestManager.Instance.AddCash(GameManager.Instance.AllCatData[catGrade].CatGetCoin);

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

    // ======================================================================================================================
    // [메인 설정]

    // 초기 스크롤 위치 초기화 함수
    private void ResetScrollPositions()
    {
        foreach (var scrollRect in dictionaryScrollRects)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // DictionaryButton 설정하는 함수
    private void InitializeDictionaryButton()
    {
        dictionaryButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("DictionaryMenu");

            // DictionaryMenu가 활성화될 때 InformationPanel 업데이트
            if (activePanelManager.ActivePanelName == "DictionaryMenu")
            {
                UpdateInformationPanel();
            }
        });
        dictionaryBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("DictionaryMenu"));

        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // 도감 데이터를 채우는 함수
    private void PopulateDictionary()
    {
        if (GameManager.Instance.AllCatData == null || GameManager.Instance.AllCatData.Length == 0)
        {
            Debug.LogError("No cat data found in GameManager.");
            return;
        }

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

        RectTransform textRect = text.GetComponent<RectTransform>();

        if (IsCatUnlocked(cat.CatGrade - 1))
        {
            button.interactable = true;
            text.text = $"{cat.CatGrade}. {cat.CatName}";
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

            // 첫 해금 보상을 받았다면 해당 슬롯의 firstOpenBG 비활성화 / 받지 않았다면 firstOpenBG 활성화
            if (IsGetFirstUnlockedReward(cat.CatGrade))
            {
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                firstOpenBG.gameObject.SetActive(true);
                firstOpenCashtext.text = $"+ {GameManager.Instance.AllCatData[cat.CatGrade].CatGetCoin}";
            }
        }
        else
        {
            button.interactable = false;
            text.text = "???";
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, 0);
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);

            firstOpenBG.gameObject.SetActive(false);
        }
    }

    // 새로운 고양이를 해금하면 도감을 업데이트하는 함수
    public void UpdateDictionary(int catGrade)
    {
        // scrollRectContents 내의 CatGrade와 동일한 순번의 슬롯을 찾아서 해당 슬롯을 업데이트
        Transform slot = scrollRectContents.GetChild(catGrade);

        slot.gameObject.SetActive(true);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI text = slot.transform.Find("Button/Name Text")?.GetComponent<TextMeshProUGUI>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();

        RectTransform textRect = text.GetComponent<RectTransform>();

        button.interactable = true;

        iconImage.sprite = GameManager.Instance.AllCatData[catGrade].CatImage;
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        text.text = $"{catGrade + 1}. {GameManager.Instance.AllCatData[catGrade].CatName}";
        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, 100);

        if (!IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {GameManager.Instance.AllCatData[catGrade].CatGetCoin}";
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
    }

    // 도감이 활성화 되거나 서브 메뉴를 누르면 InformationPanel을 기본 정보로 업데이트 해주는 함수
    private void UpdateInformationPanel()
    {
        // 이미지 설정
        informationCatIcon.sprite = null;

        // 텍스트 설정
        informationCatDetails.text = $"Select Cat\n";

        // 스크롤 비활성화
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // 초기 catInformation Mask 초기화
        catInformationPanel.GetComponent<Mask>().enabled = true;
    }

    // 고양이 정보를 Information Panel에 표시하는 함수
    private void ShowInformationPanel(int catGrade)
    {
        // 고양이 정보 불러오기
        var catData = GameManager.Instance.AllCatData[catGrade];

        // 이미지 설정
        informationCatIcon.sprite = catData.CatImage;

        // 텍스트 설정
        string catInfo = $"Name: {catData.CatName}\n" +
                         $"Grade: {catData.CatGrade}\n" +
                         $"Damage: {catData.CatDamage}\n" +
                         $"HP: {catData.CatHp}\n" +
                         $"GetCoin: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // 스크롤 활성화, 스크롤 중이었다면 멈춤
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;

        // fullInformationPanel의 Y좌표를 -312.5f로 고정
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);
    }

    // 새로운 고양이 해금 효과 함수
    public void ShowNewCatPanel(int catGrade)
    {
        Cat newCat = GameManager.Instance.AllCatData[catGrade];

        newCatPanel.SetActive(true);

        newCatIcon.sprite = newCat.CatImage;
        newCatName.text = newCat.CatName;
        newCatExplain.text = newCat.CatExplain;
        newCatGetCoin.text = "Get Coin : " + newCat.CatGetCoin.ToString();

        // 확인 버튼을 누르면 패널을 비활성화
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // New Cat Panel을 닫는 함수
    private void CloseNewCatPanel()
    {
        newCatPanel.SetActive(false);
    }

    // ======================================================================================================================
    // [서브 메뉴]

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
        string colorCode = isActive ? "#5f5f5f" : "#FFFFFF";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================

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

    // New Image UI를 갱신하는 함수
    private void UpdateNewImageStatus()
    {
        bool hasUnclaimedRewards = HasUnclaimedRewards();

        // 도감 Button의 New Image 활성화/비활성화
        if (dictionaryButtonNewImage != null)
        {
            dictionaryButtonNewImage.SetActive(hasUnclaimedRewards);
        }

        // Normal Cat Button의 New Image 활성화/비활성화
        if (normalCatButtonNewImage != null)
        {
            normalCatButtonNewImage.SetActive(hasUnclaimedRewards);
        }
    }

    // 
    private void OnDestroy()
    {
        // 이벤트 핸들러 해제
        OnCatDataChanged -= UpdateNewImageStatus;
    }


    // ============================================================================================================
    private void InitializeFriendshipButton()
    {
        friendshipUnlockButtons[0].onClick.AddListener(UnLockLevel1Friendship);
        friendshipUnlockButtons[1].onClick.AddListener(UnLockLevel2Friendship);
        friendshipUnlockButtons[2].onClick.AddListener(UnLockLevel3Friendship);
        friendshipUnlockButtons[3].onClick.AddListener(UnLockLevel4Friendship);
        friendshipUnlockButtons[4].onClick.AddListener(UnLockLevel5Friendship);

    }
    private void UnLockLevel1Friendship()
    {
        friendshipGetCrystalImg[0].SetActive(false);
        GameManager.Instance.Cash += 100;
        friendshipUnlockButtons[0].interactable = false;
    }
    private void UnLockLevel2Friendship()
    {
        friendshipGetCrystalImg[1].SetActive(false);
    }
    private void UnLockLevel3Friendship()
    {
        friendshipGetCrystalImg[2].SetActive(false);
    }
    private void UnLockLevel4Friendship()
    {
        friendshipGetCrystalImg[3].SetActive(false);
    }
    private void UnLockLevel5Friendship()
    {
        friendshipGetCrystalImg[4].SetActive(false);
    }
    //private void ShowFriendshipInfoPanel(int catGrade)
    //{
    //    // 고양이 정보 불러오기
    //    var catData = GameManager.Instance.AllCatData[catGrade];
    //    // 이미지 설정
    //    informationCatIcon.sprite = catData.CatImage;
    //    // 텍스트 설정
    //    string catInfo = $"Name: {catData.CatName}\n" +
    //                     $"Grade: {catData.CatGrade}\n" +
    //                     $"Damage: {catData.CatDamage}\n" +
    //                     $"HP: {catData.CatHp}\n" +
    //                     $"GetCoin: {catData.CatGetCoin}";
    //    informationCatDetails.text = catInfo;
    //    // 스크롤 활성화, 스크롤 중이었다면 멈춤
    //    catInformationPanel.GetComponent<ScrollRect>().enabled = true;
    //    catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
    //    //fullInformationPanel의 Y좌표를 -312.5f로 고정
    //    fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);
    //}
}

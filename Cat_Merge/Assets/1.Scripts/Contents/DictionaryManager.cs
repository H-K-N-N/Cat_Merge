using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


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
    [SerializeField] private Image newCatHighlightImage;            // New Cat Highlight Image
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
    private Sprite informationCatDefaultImage;                      // Information Cat Default Image
    [SerializeField] private TextMeshProUGUI informationCatDetails; // informationCatDetails Text
    [SerializeField] private GameObject catInformationPanel;        // catInformation Panel (상세정보 칸 Panel)
    [SerializeField] private RectTransform fullInformationPanel;    // fullInformation Panel (상세정보 스크롤 Panel)

    // 임시 (다른 서브 메뉴들을 추가한다면 어떻게 정리 할까 고민)
    [Header("---[Sub Contents]")]
    [SerializeField] private Transform scrollRectContents;          // 노말 고양이 scrollRectContents (도감에서 노말 고양이의 정보를 초기화 하기 위해)
                                                                    // 희귀 고양이 scrollRectContents
                                                                    // 특수 고양이 scrollRectContents

    // ======================================================================================================================

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";               // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";             // 비활성화상태 Color

    // ======================================================================================================================
    // [우정도]

    [Header("---[Test]")]
    [SerializeField] private Transform buttonParent; // 버튼이 배치될 부모 오브젝트
    [SerializeField] private Button[] buttonPrefabs; // 레벨 1~5 버튼 프리팹 배열 (5개)

    [Header("---[Friendship System]")]
    [SerializeField] private Transform friendshipButtonParent;  // 우정도 버튼들의 부모 Transform
    [SerializeField] private Button[] friendshipButtonPrefabs;  // 레벨 1~5 버튼 프리팹 배열
    private Button[] activeButtons; // 실제 사용되는 5개의 버튼

    // 현재 선택된 고양이 등급 추적
    public int currentSelectedCatGrade = -1;

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
            return;
        }

        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        // 우정도 버튼 초기화는 한 번만 실행
        InitializeFriendshipButtons();
        InitializeDictionaryManager();
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
        InitializeInformationCatDefaultImage();

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

        //// 버튼이 인스턴스화된 후 SFX 등록
        //if (button != null && OptionManager.Instance != null)
        //{
        //    button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
        //}
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

        // 버튼 업데이트 후 SFX 등록
        if (OptionManager.Instance != null)
        {
            button.onClick.AddListener(OptionManager.Instance.PlayButtonClickSound);
        }
    }

    private void InitializeInformationCatDefaultImage()
    {
        //informationCatDefaultImage = Resources.Load<Sprite>("Sprites/UI/I_UI_Book_mission/I_UI_Book_CatBG.9");
    }

    // 도감이 활성화 되거나 서브 메뉴를 누르면 InformationPanel을 기본 정보로 업데이트 해주는 함수
    private void UpdateInformationPanel()
    {
        currentSelectedCatGrade = -1;

        // 이미지 설정
        //informationCatIcon.sprite = informationCatDefaultImage;
        informationCatIcon.gameObject.SetActive(false);

        // 텍스트 설정
        informationCatDetails.text = $"고양이를 선택하세요\n";

        // 스크롤 비활성화
        catInformationPanel.GetComponent<ScrollRect>().enabled = false;

        // 초기 catInformation Mask 초기화
        catInformationPanel.GetComponent<Mask>().enabled = true;

        // fullInformationPanel의 Y좌표를 -312.5f로 고정
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

        // 우정도 버튼 비활성화
        if (activeButtons != null)
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }
    }

    // 고양이 정보를 Information Panel에 표시하는 함수
    private void ShowInformationPanel(int catGrade)
    {
        currentSelectedCatGrade = catGrade + 1;
        var catData = GameManager.Instance.AllCatData[catGrade];

        // 기존 정보 표시 코드...
        informationCatIcon.gameObject.SetActive(true);
        informationCatIcon.sprite = catData.CatImage;

        string catInfo = $"이름: {catData.CatName}\n" +
                         $"등급: {catData.CatGrade}\n" +
                         $"공격력 증가량: {catData.CatDamage}%\n" +
                         $"체력: {catData.CatHp}\n" +
                         $"재화수급량: {catData.CatGetCoin}";
        informationCatDetails.text = catInfo;

        // 스크롤 설정
        catInformationPanel.GetComponent<ScrollRect>().enabled = true;
        catInformationPanel.GetComponent<ScrollRect>().velocity = Vector2.zero;
        fullInformationPanel.anchoredPosition = new Vector2(0, -312.5f);

        // 우정도 UI 업데이트
        UpdateFriendshipButtonsForCat(currentSelectedCatGrade);
        FriendshipManager.Instance.UpdateFriendshipUI(currentSelectedCatGrade);
    }

    // 새로운 고양이 해금 효과 함수
    public void ShowNewCatPanel(int catGrade)
    {
        Cat newCat = GameManager.Instance.AllCatData[catGrade];

        newCatPanel.SetActive(true);

        newCatIcon.sprite = newCat.CatImage;
        newCatName.text = newCat.CatGrade.ToString() + ". " + newCat.CatName;
        newCatExplain.text = newCat.CatExplain;
        newCatGetCoin.text = "재화 획득량: " + newCat.CatGetCoin.ToString();

        // Highlight Image 회전 애니메이션 시작
        StartCoroutine(RotateHighlightImage());

        // 확인 버튼을 누르면 패널을 비활성화
        //submitButton.onClick.RemoveAllListeners();            // 이걸 지워야 sfx소리가 해당 버튼에 잘 들어감
        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // Highlight Image 회전 애니메이션 코루틴
    private IEnumerator RotateHighlightImage()
    {
        while (newCatPanel.activeSelf)
        {
            newCatHighlightImage.transform.Rotate(new Vector3(0, 0, 1), 90 * Time.deltaTime);
            yield return null;
        }
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
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
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

        // 우정도 버튼 정리
        if (activeButtons != null)
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
        }
    }

    // ======================================================================================================================

    // 현재 선택된 고양이 등급 반환 함수 추가
    public int GetCurrentSelectedCatGrade()
    {
        return currentSelectedCatGrade;
    }

    // ======================================================================================================================

    // 우정도 버튼 초기화
    private void InitializeFriendshipButtons()
    {
        if (activeButtons != null) return; // 이미 초기화되어 있다면 스킵

        activeButtons = new Button[5];

        // 5개의 버튼만 생성
        for (int i = 0; i < friendshipButtonPrefabs.Length; i++)
        {
            activeButtons[i] = Instantiate(friendshipButtonPrefabs[i], friendshipButtonParent);
            activeButtons[i].gameObject.SetActive(false); // 초기에는 비활성화
        }
    }

    // 우정도 버튼 상태 업데이트
    public void UpdateFriendshipButtonStates(int catGrade)
    {
        if (activeButtons == null) return;

        var friendshipInfo = FriendshipManager.Instance.GetFriendshipInfo(catGrade);

        for (int i = 0; i < activeButtons.Length; i++)
        {
            var button = activeButtons[i];
            if (button != null)
            {
                // LockBG 상태 업데이트
                Transform lockBG = button.transform.Find("LockBG");
                if (lockBG != null)
                {
                    lockBG.gameObject.SetActive(!friendshipInfo.isUnlocked[i]);
                }

                // FirstOpenBG 상태 업데이트
                Transform firstOpenBG = button.transform.Find("FirstOpenBG");
                if (firstOpenBG != null)
                {
                    firstOpenBG.gameObject.SetActive(friendshipInfo.isUnlocked[i] && !friendshipInfo.isClaimed[i]);
                }

                // Star 상태 업데이트
                Transform star = button.transform.FindDeepChild("Star");
                if (star != null)
                {
                    star.gameObject.SetActive(friendshipInfo.isUnlocked[i]);
                    if(friendshipInfo.isUnlocked[i])
                    {
                        GameObject fullStarObj = GameObject.Find("fullStar");
                        if(fullStarObj != null)
                        {
                            RectTransform fullStar = fullStarObj.GetComponent<RectTransform>(); // RectTransform 가져오기
                            if (fullStar != null)
                            {
                                Vector2 newOffsetMax = fullStar.offsetMax;
                                switch (i)
                                {
                                    case 0:
                                        newOffsetMax.x = -168; // right 값을 168로 변경
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 1:
                                        newOffsetMax.x = -126; // right 값을 168로 변경
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 2:
                                        newOffsetMax.x = -84; // right 값을 168로 변경
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 3:
                                        newOffsetMax.x = -42; // right 값을 168로 변경
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                    case 4:
                                        newOffsetMax.x = 0; // right 값을 168로 변경
                                        fullStar.offsetMax = newOffsetMax;
                                        break;
                                }
                            }
                        }                   
                    }
                }

                // 버튼 상호작용 상태 설정
                button.interactable = friendshipInfo.isUnlocked[i] && !friendshipInfo.isClaimed[i];
            }
        }
    }

    // 우정도 버튼 상태 업데이트 (고양이 선택 시)
    private void UpdateFriendshipButtonsForCat(int catGrade)
    {
        if (activeButtons == null) return;

        var friendshipInfo = FriendshipManager.Instance.GetFriendshipInfo(catGrade);

        for (int i = 0; i < activeButtons.Length; i++)
        {
            var button = activeButtons[i];
            if (button != null)
            {
                button.gameObject.SetActive(true);

                // 보상 금액 텍스트 설정
                Transform firstOpenBG = button.transform.Find("FirstOpenBG");
                if (firstOpenBG != null)
                {
                    TextMeshProUGUI cashText = firstOpenBG.Find("Cash Text")?.GetComponent<TextMeshProUGUI>();
                    if (cashText != null)
                    {
                        int rewardAmount = FriendshipManager.Instance.GetRewardAmount(i);
                        cashText.text = $"+ {rewardAmount}";
                    }
                }

                // 클릭 이벤트 재설정
                int level = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnFriendshipButtonClick(catGrade, level));
            }
        }

        UpdateFriendshipButtonStates(catGrade);
    }


    // 우정도 버튼 클릭 처리
    private void OnFriendshipButtonClick(int catGrade, int level)
    {
        if (FriendshipManager.Instance.CanClaimLevelReward(catGrade, level))
        {
            FriendshipManager.Instance.ClaimReward(catGrade, level);
            UpdateFriendshipButtonStates(catGrade);
        }
    }

}

public static class TransformExtensions
{
    public static Transform FindDeepChild(this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = child.FindDeepChild(name);
            if (found != null)
                return found;
        }
        return null;
    }
}
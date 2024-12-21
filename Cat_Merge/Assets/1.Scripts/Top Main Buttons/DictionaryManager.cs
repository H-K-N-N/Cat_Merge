using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 고양이 도감 Script
public class DictionaryManager : MonoBehaviour
{
    private GameManager gameManager;                                // GameManager

    [Header("---[Dictionary Manager]")]
    [SerializeField] private ScrollRect[] dictionaryScrollRects;    // 도감의 스크롤뷰 배열
    [SerializeField] private GameObject[] dictionaryMenus;          // 도감 메뉴 Panel
    [SerializeField] private Button dictionaryButton;               // 도감 버튼
    [SerializeField] private Image dictionaryButtonImage;           // 도감 버튼 이미지
    [SerializeField] private GameObject dictionaryMenuPanel;        // 도감 메뉴 Panel
    [SerializeField] private Button dictionaryBackButton;           // 도감 뒤로가기 버튼
    private bool isDictionaryMenuOpen;                              // 도감 메뉴 Panel의 활성화 상태

    [SerializeField] private GameObject slotPrefab;                 // 도감 슬롯 프리팹

    [SerializeField] private Button[] dictionaryMenuButtons;        // 도감의 서브 메뉴 버튼 배열

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

    // 임시 (다른 서브 메뉴들을 추가한다면 어떻게 정리 할까 고민)
    [SerializeField] private Transform scrollRectContents;          // 노말 고양이 scrollRectContents (도감에서 노멀 고양이의 정보를 초기화 하기 위해)
                                                                    // 희귀 고양이 scrollRectContents
                                                                    // 특수 고양이 scrollRectContents
                                                                    // 배경 scrollRectContents

    // ======================================================================================================================

    // Start
    private void Start()
    {
        gameManager = GameManager.Instance;

        newCatPanel.SetActive(false);
        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        dictionaryButton.onClick.AddListener(ToggleDictionaryMenuPanel);
        dictionaryBackButton.onClick.AddListener(CloseDictionaryMenuPanel);

        ResetScrollPositions();
        InitializeMenuButtons();
        PopulateDictionary();

        submitButton.onClick.AddListener(CloseNewCatPanel);
    }

    // 초기 스크롤 위치 초기화 함수
    private void ResetScrollPositions()
    {
        foreach (var scrollRect in dictionaryScrollRects)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeMenuButtons()
    {
        for (int i = 0; i < (int)DictionaryMenuType.End; i++)
        {
            int index = i;
            dictionaryMenuButtons[index].onClick.AddListener(() => ActivateMenu((DictionaryMenuType)index));
        }

        ActivateMenu(DictionaryMenuType.Normal);
    }

    // 도감 Panel 열고 닫는 함수
    private void ToggleDictionaryMenuPanel()
    {
        isDictionaryMenuOpen = !isDictionaryMenuOpen;
        dictionaryMenuPanel.SetActive(isDictionaryMenuOpen);
        UpdateButtonColor(dictionaryButtonImage, isDictionaryMenuOpen);
    }

    // 도감 Panel 닫는 함수 (dictionaryBackButton)
    private void CloseDictionaryMenuPanel()
    {
        isDictionaryMenuOpen = false;
        dictionaryMenuPanel.SetActive(false);
        UpdateButtonColor(dictionaryButtonImage, false);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(DictionaryMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < dictionaryMenus.Length; i++)
        {
            dictionaryMenus[i].SetActive(i == (int)menuType);
        }

        UpdateMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateMenuButtonColors()
    {
        for (int i = 0; i < dictionaryMenuButtons.Length; i++)
        {
            UpdateButtonColor(dictionaryMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? "#5f5f5f" : "#E2E2E2";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // 도감 데이터를 채우는 함수
    private void PopulateDictionary()
    {
        if (gameManager.AllCatData == null || gameManager.AllCatData.Length == 0)
        {
            Debug.LogError("No cat data found in GameManager.");
            return;
        }

        foreach (Transform child in scrollRectContents)
        {
            Destroy(child.gameObject);
        }

        foreach (Cat cat in gameManager.AllCatData)
        {
            InitializeSlot(cat);
        }
    }

    // 고양이 데이터를 바탕으로 초기 슬롯을 생성하는 함수
    private void InitializeSlot(Cat cat)
    {
        GameObject slot = Instantiate(slotPrefab, scrollRectContents);

        Button button = slot.transform.Find("Button")?.GetComponent<Button>();
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        TextMeshProUGUI text = slot.transform.Find("Text Image/Text")?.GetComponent<TextMeshProUGUI>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();

        // 나중에 데이터 불러오기 기능이 있다면 무조건 있어야하기 때문에 작성 (현재는 무조건 else문으로 넘어감)
        if (gameManager.IsCatUnlocked(cat.CatGrade - 1))
        {
            button.interactable = true;
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);
            text.text = $"{cat.CatGrade}. {cat.CatName}";

            // 첫 해금 보상을 받았다면 해당 슬롯의 firstOpenBG 비활성화 / 받지 않았다면 firstOpenBG 활성화
            firstOpenBG.gameObject.SetActive(false);
        }
        else
        {
            button.interactable = false;
            iconImage.sprite = cat.CatImage;
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);
            text.text = "???";

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
        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        TextMeshProUGUI text = slot.transform.Find("Text Image/Text")?.GetComponent<TextMeshProUGUI>();
        Image firstOpenBG = slot.transform.Find("Button/FirstOpenBG")?.GetComponent<Image>();
        TextMeshProUGUI firstOpenCashtext = slot.transform.Find("Button/FirstOpenBG/Cash Text")?.GetComponent<TextMeshProUGUI>();

        button.interactable = true;

        iconImage.sprite = gameManager.AllCatData[catGrade].CatImage;
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);

        text.text = $"{catGrade + 1}. {gameManager.AllCatData[catGrade].CatName}";
        
        if (!gameManager.IsGetFirstUnlockedReward(catGrade))
        {
            firstOpenBG.gameObject.SetActive(true);
            firstOpenCashtext.text = $"+ {gameManager.AllCatData[catGrade].CatGetCoin}";
        }
        else
        {
            firstOpenBG.gameObject.SetActive(false);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (!gameManager.IsGetFirstUnlockedReward(catGrade))
            {
                gameManager.GetFirstUnlockedReward(catGrade);
                firstOpenBG.gameObject.SetActive(false);
            }
            else
            {
                ShowNewCatPanel(catGrade);
            }
        });
    }

    // 새로운 고양이 해금 효과 & 도감에서 해당 고양이 버튼을 누르면 나오는 New Cat Panel 함수
    public void ShowNewCatPanel(int catGrade)
    {
        Cat newCat = gameManager.AllCatData[catGrade];

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


}

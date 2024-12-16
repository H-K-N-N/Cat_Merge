using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dictionary : MonoBehaviour
{
    // 도감 기능
    [Header("---[Dictionary]")]
    private GameManager gameManager;                                // GameManager SingleTon

    [SerializeField] private ScrollRect[] dictionaryScrollRects;    // 도감의 스크롤뷰 배열
    [SerializeField] private GameObject[] dictionaryMenus;          // 도감 메뉴 Panel

    [SerializeField] private Button dictionaryButton;               // 도감 버튼
    [SerializeField] private Image dictionaryButtonImage;           // 도감 버튼 이미지

    [SerializeField] private GameObject dictionaryMenuPanel;        // 도감 메뉴 Panel
    [SerializeField] private Button dictionaryBackButton;           // 도감 뒤로가기 버튼
    private bool isDictionaryMenuOpen;                              // 도감 메뉴 Panel의 활성화 상태

    [SerializeField] private GameObject slotPrefab;                 // 도감 슬롯 프리팹

    [SerializeField] private Button[] dictionaryMenuButtons;        // 도감의 서브 메뉴 버튼 배열

    // Enum으로 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum DictionaryMenuType
    {
        Normal,         // 일반 고양이 메뉴
        Rare,           // 희귀 고양이 메뉴
        Special,        // 특수 고양이 메뉴
        Background,     // 배경 메뉴
        End             // Enum의 끝을 표시
    }
    private DictionaryMenuType activeMenuType;                      // 현재 활성화된 메뉴 타입

    // 임시 (도감에서 노멀 고양이의 정보를 초기화 하기 위해)
    [SerializeField] private Transform scrollRectContents;          // 노말 고양이 scrollRectContents



    // Start()
    private void Start()
    {
        gameManager = GameManager.Instance;

        dictionaryMenuPanel.SetActive(false);
        activeMenuType = DictionaryMenuType.Normal;

        dictionaryButton.onClick.AddListener(ToggleDictionaryMenuPanel);
        dictionaryBackButton.onClick.AddListener(CloseDictionaryMenuPanel);

        ResetScrollPositions();
        InitializeMenuButtons();
        PopulateDictionary();
    }

    private void OnEnable()
    {
        //ResetScrollPositions();
    }

    // 초기 스크롤위치 초기화 함수
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

    // 도감 Panel 닫는 함수
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
            CreateSlot(cat);
        }
    }

    // 고양이 데이터를 바탕으로 슬롯을 생성하는 함수
    private void CreateSlot(Cat cat)
    {
        GameObject slot = Instantiate(slotPrefab, scrollRectContents);

        Image iconImage = slot.transform.Find("Button/Icon")?.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = cat.CatImage;
        }

        TextMeshProUGUI text = slot.transform.Find("Text Image/Text")?.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"{cat.CatId}. {cat.CatName}";
        }
    }


}

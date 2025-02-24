using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContentManager : MonoBehaviour
{
    #region Variables
    public static ContentManager Instance { get; private set; }

    [Header("---[Common UI Settings]")]
    [SerializeField] private Button contentButton;                  // 컨텐츠 버튼
    [SerializeField] private Image contentButtonImage;              // 컨텐츠 버튼 이미지
    [SerializeField] private GameObject contentPanel;               // 컨텐츠 패널
    [SerializeField] private Button contentBackButton;              // 컨텐츠 뒤로가기 버튼
    private ActivePanelManager activePanelManager;                  // ActivePanelManager

    [Header("---[Sub Menu Settings]")]
    [SerializeField] private GameObject[] mainContentMenus;         // 메인 컨텐츠 메뉴 Panels
    [SerializeField] private Button[] subContentMenuButtons;        // 서브 컨텐츠 메뉴 버튼 배열

    [Header("---[UI Color Settings]")]
    private const string activeColorCode = "#FFCC74";               // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";             // 비활성화상태 Color

    // 컨텐츠 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum ContentMenuType
    {
        Training,       // 체력 단련
        Menu2,          // 두 번째 메뉴
        Menu3,          // 세 번째 메뉴
        End             // Enum의 끝
    }
    private ContentMenuType activeMenuType;                         // 현재 활성화된 메뉴 타입
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
        contentPanel.SetActive(false);
        InitializeContentManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("ContentMenu", contentPanel, contentButtonImage);
    }
    #endregion

    #region Initialize
    // 모든 ContentsManager 시작 함수들 모음
    private void InitializeContentManager()
    {
        InitializeContentButton();
        InitializeSubMenuButtons();
    }

    // ContentButton 초기화 함수
    private void InitializeContentButton()
    {
        contentButton.onClick.AddListener(() =>
        {
            activePanelManager.TogglePanel("ContentMenu");
        });

        contentBackButton.onClick.AddListener(() =>
        {
            activePanelManager.ClosePanel("ContentMenu");
        });
    }
    #endregion

    #region Sub Menu System
    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)ContentMenuType.End; i++)
        {
            int index = i;
            subContentMenuButtons[index].onClick.AddListener(() =>
            {
                ActivateMenu((ContentMenuType)index);
            });
        }

        ActivateMenu(ContentMenuType.Training);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(ContentMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainContentMenus.Length; i++)
        {
            mainContentMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subContentMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subContentMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // 서브 메뉴 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        if (ColorUtility.TryParseHtmlString(isActive ? activeColorCode : inactiveColorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }
    #endregion

    #region Battle System
    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleState()
    {
        contentButton.interactable = false;
        if (contentPanel.activeSelf)
        {
            contentPanel.SetActive(false);
        }
    }

    // 전투 종료시 버튼 및 기능 활성화시키는 함수
    public void EndBattleState()
    {
        contentButton.interactable = true;
    }
    #endregion

    #region Utility Functions
    // 코루틴 정지 후 실행시키는 함수
    private void StopAndStartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(routine);
    }
    #endregion
}

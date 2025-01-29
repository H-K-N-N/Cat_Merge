using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    // Singleton Instance
    public static OptionManager Instance { get; private set; }

    [Header("---[OptionManager]")]
    [SerializeField] private Button optionButton;                               // 옵션 버튼
    [SerializeField] private Image optionButtonImage;                           // 옵션 버튼 이미지
    [SerializeField] private GameObject optionMenuPanel;                        // 옵션 메뉴 Panel
    [SerializeField] private Button optionBackButton;                           // 옵션 뒤로가기 버튼
    private ActivePanelManager activePanelManager;                              // ActivePanelManager

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
        optionMenuPanel.SetActive(false);

        InitializeOptionManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("OptionMenu", optionMenuPanel, optionButtonImage);
    }

    // ======================================================================================================================

    private void InitializeOptionManager()
    {
        InitializeOptionButton();
    }

    private void InitializeOptionButton()
    {
        optionButton.onClick.AddListener(() => activePanelManager.TogglePanel("OptionMenu"));
        optionBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("OptionMenu"));
    }

    // ======================================================================================================================



}

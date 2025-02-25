using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        JellyDungeon,   // 젤리 던전
        Menu3,          // 세 번째 메뉴
        End             // Enum의 끝
    }
    private ContentMenuType activeMenuType;                         // 현재 활성화된 메뉴 타입

    [Header("---[Training Settings]")]
    public TextMeshProUGUI levelUpCoin;                             // 다음 단계 상승 필요 재화
    public Button levelUpButton;                                    // 다음 단계 상승 버튼

    public TextMeshProUGUI trainingCoin;                            // 체력 단련 필요 재화
    public Button trainingButton;                                   // 체력 단련 버튼
    private int currentTrainingStage = 0;                           // 현재 체력단련 단계
    private bool isTrainingCompleted = false;                       // 체력단련 완료 여부

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
        InitializeTrainingSystem();
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

    private void InitializeTrainingSystem()
    {
        // 0레벨 TraingingDB를 불러와서 모든 데이터에 적용 (기본 데이터)
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null && trainingLoader.trainingDictionary.TryGetValue(0, out TrainingData trainingData))
        {
            Cat[] allCats = GameManager.Instance.AllCatData;
            foreach (Cat cat in allCats)
            {
                cat.GrowStat(trainingData.GrowthDamage, trainingData.GrowthHp);
            }
        }

        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(OnLevelUp);
        }
        if (trainingButton != null)
        {
            trainingButton.onClick.AddListener(OnTraining);
        }

        UpdateTrainingButtonUI();
    }
    #endregion

    #region Training System
    // 훈련 레벨업 버튼 클릭 처리
    private void OnLevelUp()
    {
        if (!isTrainingCompleted)
        {
            return;
        }

        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null)
        {
            int nextStage = currentTrainingStage + 1;
            if (trainingLoader.trainingDictionary.TryGetValue(nextStage, out TrainingData trainingData))
            {
                double requiredCoin = double.Parse(levelUpCoin.text);
                if (GameManager.Instance.Coin < requiredCoin)
                {
                    NotificationManager.Instance.ShowNotification("재화가 부족합니다!");
                    return;
                }
                GameManager.Instance.Coin -= (int)requiredCoin;

                Cat[] allCats = GameManager.Instance.AllCatData;
                foreach (Cat cat in allCats)
                {
                    cat.GrowStat(trainingData.GrowthDamage, trainingData.GrowthHp);
                }
                currentTrainingStage = nextStage;

                // 성장 데이터 저장 및 필드 고양이 정보 업데이트
                GameManager.Instance.SaveTrainingData(allCats);
                GameManager.Instance.UpdateAllCatsInField();

                isTrainingCompleted = false;
                levelUpButton.interactable = false;

                UpdateTrainingButtonUI();
            }
        }
    }

    // 체력단련 버튼 클릭 처리
    private void OnTraining()
    {
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null && trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage, out TrainingData trainingData))
        {
            double requiredCoin = double.Parse(trainingCoin.text);
            if (GameManager.Instance.Coin < requiredCoin)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!");
                return;
            }
            GameManager.Instance.Coin -= (int)requiredCoin;

            isTrainingCompleted = true;

            UpdateTrainingButtonUI();
        }
    }

    // 체력단련 버튼UI 업데이트
    private void UpdateTrainingButtonUI()
    {
        TrainingDataLoader trainingLoader = FindObjectOfType<TrainingDataLoader>();
        if (trainingLoader != null)
        {
            // 레벨업 버튼 상태 및 필요 재화 업데이트
            bool canLevelUp = trainingLoader.trainingDictionary.ContainsKey(currentTrainingStage + 1);
            levelUpButton.interactable = canLevelUp && isTrainingCompleted;

            // 다음 단계의 레벨업 필요 재화 표시
            if (canLevelUp)
            {
                levelUpCoin.text = trainingLoader.trainingDictionary[currentTrainingStage + 1].LevelUpCoin.ToString("N0");
            }

            // 다음 단계의 훈련 필요 재화 표시
            if (trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage + 1, out TrainingData nextData))
            {
                trainingCoin.text = nextData.TrainingCoin.ToString("N0");
                trainingButton.interactable = !isTrainingCompleted;
            }
            else
            {
                trainingButton.interactable = false;
            }
        }
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

}

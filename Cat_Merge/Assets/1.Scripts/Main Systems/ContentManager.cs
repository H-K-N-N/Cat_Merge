using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ContentManager : MonoBehaviour
{
    #region Variables
    public static ContentManager Instance { get; private set; }

    [Header("---[Common UI Settings]")]
    [SerializeField] private Button contentButton;              // 컨텐츠 버튼
    [SerializeField] private Image contentButtonImage;          // 컨텐츠 버튼 이미지
    [SerializeField] private GameObject contentPanel;           // 컨텐츠 패널
    [SerializeField] private Button contentBackButton;          // 컨텐츠 뒤로가기 버튼

    [Header("---[Sub Menu Settings]")]
    [SerializeField] private GameObject[] mainContentMenus;     // 메인 컨텐츠 메뉴 Panels
    [SerializeField] private Button[] subContentMenuButtons;    // 서브 컨텐츠 메뉴 버튼 배열

    [Header("---[UI Color Settings]")]
    private const string activeColorCode = "#FFCC74";           // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";         // 비활성화상태 Color

    // 컨텐츠 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum ContentMenuType
    {
        Training,       // 체력 단련
        JellyDungeon,   // 젤리 던전
        Menu3,          // 세 번째 메뉴
        End             // Enum의 끝
    }
    private ContentMenuType activeMenuType;                     // 현재 활성화된 메뉴 타입

    [Header("---[Training Settings]")]
    public TextMeshProUGUI nextLevelAblityInformationText;      // 다음 단계 도달 시 획득하는 추가 능력치 text

    public TextMeshProUGUI trainingLevelInformationText;        // 다음 단계 정보 text
    public TextMeshProUGUI trainingProgressText;                // 진행상황 % text
    private float trainingProgress = 0f;                        // 진행상황

    public TextMeshProUGUI trainingLevelText;                   // 체력 단련 N단계 text
    public TextMeshProUGUI trainingDataText;                    // 체력 단련 수치 text

    public TextMeshProUGUI levelUpCoinText;                     // 다음 단계 상승 필요 재화 text
    public Button levelUpButton;                                // 다음 단계 상승 버튼
    public TextMeshProUGUI trainingCoinText;                    // 체력 단련 필요 재화 text
    public Button trainingButton;                               // 체력 단련 버튼
    private int currentTrainingStage = 0;                       // 현재 체력단련 단계
    private bool isTrainingCompleted = false;                   // 체력단련 완료 여부
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
        ActivePanelManager.Instance.RegisterPanel("ContentMenu", contentPanel, contentButtonImage);
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
            ActivePanelManager.Instance.TogglePanel("ContentMenu");
        });

        contentBackButton.onClick.AddListener(() =>
        {
            ActivePanelManager.Instance.ClosePanel("ContentMenu");
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

        trainingProgressText.text = $"{trainingProgress}%";
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
                decimal requiredCoin = decimal.Parse(levelUpCoinText.text);
                if (GameManager.Instance.Coin < requiredCoin)
                {
                    NotificationManager.Instance.ShowNotification("재화가 부족합니다!");
                    return;
                }
                GameManager.Instance.Coin -= requiredCoin;

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

                // 레벨업 시 훈련 진행 상황을 0%로 초기화
                trainingProgress = 0f;
                trainingProgressText.text = $"{trainingProgress}%";

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
            decimal requiredCoin = decimal.Parse(trainingCoinText.text);
            if (GameManager.Instance.Coin < requiredCoin)
            {
                NotificationManager.Instance.ShowNotification("재화가 부족합니다!");
                return;
            }
            GameManager.Instance.Coin -= requiredCoin;

            isTrainingCompleted = true;

            // 훈련 진행 상황을 100%로 업데이트
            trainingProgress = 100f;
            trainingProgressText.text = $"{trainingProgress}%";

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
                levelUpCoinText.text = trainingLoader.trainingDictionary[currentTrainingStage + 1].LevelUpCoin.ToString("N0");
            }

            // 다음 단계의 훈련 필요 재화 표시
            if (trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage + 1, out TrainingData nextData))
            {
                trainingCoinText.text = nextData.TrainingCoin.ToString("N0");
                trainingButton.interactable = !isTrainingCompleted;
            }
            else
            {
                trainingButton.interactable = false;
            }

            // 다음 레벨 능력치 정보 업데이트
            if (canLevelUp && trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage + 1, out TrainingData nextLevelData))
            {
                if (nextLevelData.ExtraAbilityName == "없음")
                {
                    nextLevelAblityInformationText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(new Color32(59, 125, 35, 255))}>[기본 능력치 증가]</color>";
                }
                else
                {
                    nextLevelAblityInformationText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(new Color32(59, 125, 35, 255))}>[▲ {nextLevelData.ExtraAbilityName} {nextLevelData.ExtraAbilitySymbol} {nextLevelData.ExtraAbilityValue} {nextLevelData.ExtraAbilityUnit} ▲]</color>";
                }
            }

            // 현재까지의 총 증가된 수치를 계산
            Dictionary<string, float> totalStats = new Dictionary<string, float>()
            {
                {"공격 속도 증가", 0},
                {"치명타 확률 증가", 0},
                {"치명타 피해 증가", 0},
                {"재화 수급량 증가", 0},
                {"재화 수급 속도 증가", 0},
                {"고양이 보유 숫자 증가", 0}
            };

            // 0단계부터 현재 단계까지의 모든 추가 능력치를 합산
            for (int i = 0; i <= currentTrainingStage; i++)
            {
                if (trainingLoader.trainingDictionary.TryGetValue(i, out TrainingData data))
                {
                    if (data.ExtraAbilityName != "없음" && totalStats.ContainsKey(data.ExtraAbilityName))
                    {
                        totalStats[data.ExtraAbilityName] += (float)data.ExtraAbilityValue;
                    }
                }
            }

            // 훈련 데이터 텍스트 구성
            string statsText = "";

            // 기본 스탯 증가량 표시 (공격력, HP)
            if (trainingLoader.trainingDictionary.TryGetValue(currentTrainingStage, out TrainingData current))
            {
                statsText += $"+{current.GrowthDamage}\n";
                statsText += $"+{current.GrowthHp}\n";
            }

            // 추가 능력치 증가량 표시
            statsText += $"-{totalStats["공격 속도 증가"]}초\n";
            statsText += $"+{totalStats["치명타 확률 증가"]}%\n";
            statsText += $"+{totalStats["치명타 피해 증가"]}%\n";
            statsText += $"+{totalStats["재화 수급량 증가"]}%\n";
            statsText += $"-{totalStats["재화 수급 속도 증가"]}초\n";
            statsText += $"+{totalStats["고양이 보유 숫자 증가"]}마리";

            trainingDataText.text = statsText;
            trainingLevelText.text = $"체력 단련 {currentTrainingStage}단계";
            trainingLevelInformationText.text = $"{currentTrainingStage + 1}단계";
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

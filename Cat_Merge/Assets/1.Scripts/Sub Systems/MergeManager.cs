using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

// 고양이 수동합성 스크립트
[DefaultExecutionOrder(-2)]
public class MergeManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static MergeManager Instance { get; private set; }

    [Header("---[Merge On/Off System]")]
    [SerializeField] private Button openMergePanelButton;           // 머지 패널 열기 버튼
    [SerializeField] private GameObject mergePanel;                 // 머지 On/Off 패널
    [SerializeField] private Button closeMergePanelButton;          // 머지 패널 닫기 버튼
    [SerializeField] private Button mergeStateButton;               // 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI stateText;             // 현재 상태 텍스트 (활성화 or 비활성화)
    [SerializeField] private TextMeshProUGUI mergeButtonText;       // 머지 버튼 텍스트 (활성화 or 비활성화)
    private bool isMergeEnabled;                                    // 머지 활성화 상태
    //private bool previousMergeState;                                // 이전 상태 저장

    [Header("---[UI Color]")]
    private const string activeColorCode = "#FFCC74";               // 활성화상태 Color
    private const string inactiveColorCode = "#B1FF70";             // 비활성화상태 Color


    private bool isDataLoaded = false;                              // 데이터 로드 확인

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
    }

    private void Start()
    {
        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            isMergeEnabled = true;
            //previousMergeState = isMergeEnabled;
        }

        InitializeButtonListeners();
        UpdateMergeButtonColor();

        // 패널 등록
        ActivePanelManager.Instance.RegisterPanel("MergePanel", mergePanel, null, ActivePanelManager.PanelPriority.Medium);
    }

    #endregion


    #region Button System

    // 버튼 리스너 초기화 함수
    private void InitializeButtonListeners()
    {
        openMergePanelButton.onClick.AddListener(() => ActivePanelManager.Instance.TogglePanel("MergePanel"));
        closeMergePanelButton.onClick.AddListener(() => ActivePanelManager.Instance.ClosePanel("MergePanel"));
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }

    // 머지 패널 여는 함수
    private void OpenMergePanel()
    {
        ActivePanelManager.Instance.OpenPanel("MergePanel");
    }

    // 머지 패널 닫는 함수
    public void CloseMergePanel()
    {
        ActivePanelManager.Instance.ClosePanel("MergePanel");
    }

    // 머지 상태 토글 함수
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    #endregion


    #region Battle System

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleMergeState()
    {
        SaveAndDisableMergeState();
        DisableMergeUI();
    }

    // 합성 활성화 상태 저장 및 비활성화 함수
    private void SaveAndDisableMergeState()
    {
        //previousMergeState = isMergeEnabled;
        //isMergeEnabled = false;
    }

    // 합성 UI 비활성화 함수
    private void DisableMergeUI()
    {
        openMergePanelButton.interactable = false;
        if (mergePanel.activeSelf)
        {
            mergePanel.SetActive(false);
        }
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleMergeState()
    {
        //isMergeEnabled = previousMergeState;
        openMergePanelButton.interactable = true;
    }

    #endregion


    #region UI System

    // 머지 버튼 색상 업데이트 함수
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            string colorCode = !isMergeEnabled ? activeColorCode : inactiveColorCode;
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                openMergePanelButton.GetComponent<Image>().color = color;
            }
        }

        stateText.text = !isMergeEnabled ? "현재상태: 비활성화" : "현재상태: 활성화";
        mergeButtonText.text = isMergeEnabled ? "비활성화" : "활성화";
    }

    // 머지 상태 반환 함수
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }

    #endregion


    #region Merge System

    // 고양이 Merge 함수
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatGrade != cat2.CatGrade)
        {
            //Debug.LogWarning("등급이 다름");
            return null;
        }

        Cat nextCat = GetCatByGrade(cat1.CatGrade + 1);
        if (nextCat != null)
        {

            //Debug.Log($"합성 성공");
            DictionaryManager.Instance.UnlockCat(nextCat.CatGrade - 1);
            QuestManager.Instance.AddMergeCount();

            // 머지되는 고양이의 경험치 증가 (2)
            FriendshipManager.Instance.AddExperience(cat1.CatGrade, 2);

            // 생성되는 상위 등급 고양이의 경험치 증가 (1)
            FriendshipManager.Instance.AddExperience(nextCat.CatGrade, 1);

            return nextCat;
        }
        else
        {
            //Debug.LogWarning("더 높은 등급의 고양이가 없음");
            return null;
        }
    }

    // 고양이 등급 반환 함수
    public Cat GetCatByGrade(int grade)
    {
        GameManager gameManager = GameManager.Instance;
        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatGrade == grade)
            {
                return cat;
            }
        }
        return null;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool isMergeEnabled;         // 머지 활성화 상태
        //public bool previousMergeState;     // 이전 상태
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isMergeEnabled = this.isMergeEnabled
            //previousMergeState = this.previousMergeState
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.isMergeEnabled = savedData.isMergeEnabled;
        //this.previousMergeState = savedData.previousMergeState;

        UpdateMergeButtonColor();

        isDataLoaded = true;
    }

    #endregion


}

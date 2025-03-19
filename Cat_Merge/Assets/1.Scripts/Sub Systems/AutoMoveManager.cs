using UnityEngine;
using UnityEngine.UI;
using System;

// 고양이 자동이동 스크립트
[DefaultExecutionOrder(-1)]
public class AutoMoveManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static AutoMoveManager Instance { get; private set; }

    [Header("---[AutoMove On/Off System]")]
    [SerializeField] private Button openAutoMovePanelButton;        // 자동 이동 패널 열기 버튼
    [SerializeField] private GameObject autoMovePanel;              // 자동 이동 On/Off 패널
    [SerializeField] private Button closeAutoMovePanelButton;       // 자동 이동 패널 닫기 버튼
    [SerializeField] private Button autoMoveStateButton;            // 자동 이동 상태 버튼
    private const float autoMoveTime = 10f;                         // 자동 이동 시간
    private bool isAutoMoveEnabled;                                 // 자동 이동 활성화 상태
    private bool previousAutoMoveState;                             // 이전 상태 저장

    private bool isDataLoaded = false;

    [Header("---[UI Color]")]
    private const string activeColorCode = "#FFCC74";               // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";             // 비활성화상태 Color

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
        Debug.Log("AutoMoveManager 호출");

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            isAutoMoveEnabled = true;
            previousAutoMoveState = isAutoMoveEnabled;
        }

        InitializeButtonListeners();
        UpdateAutoMoveButtonColor();
    }

    #endregion

    

    #region Button System

    // 버튼 리스너 초기화
    private void InitializeButtonListeners()
    {
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMoveState);
    }

    // 자동이동 패널 여는 함수
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // 자동이동 패널 닫는 함수
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // 자동이동 상태 토글 함수
    public void ToggleAutoMoveState()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;
        ApplyAutoMoveStateToAllCats();
        UpdateAutoMoveButtonColor();
        CloseAutoMovePanel();

        GoogleSave();
    }

    #endregion

    

    #region Auto Move System

    // 모든 고양이에게 자동이동 상태 적용
    private void ApplyAutoMoveStateToAllCats()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned)
            {
                continue;
            }
            cat.SetAutoMoveState(isAutoMoveEnabled);
        }
    }

    // 자동이동 버튼 색상 업데이트 함수
    private void UpdateAutoMoveButtonColor()
    {
        if (openAutoMovePanelButton == null)
        {
            return;
        }

        string colorCode = !isAutoMoveEnabled ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            openAutoMovePanelButton.GetComponent<Image>().color = color;
        }
    }

    // 현재 자동이동 상태 반환 함수
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

    // 자동이동 시간 반환 함수
    public float AutoMoveTime()
    {
        return autoMoveTime;
    }

    #endregion

    

    #region Battle System

    // 전투 시작시 버튼 및 기능 비활성화 함수
    public void StartBattleAutoMoveState()
    {
        SaveAndDisableAutoMoveState();
        DisableAutoMoveUI();

        GoogleSave();
    }

    // 자동이동 상태 저장 및 비활성화 함수
    private void SaveAndDisableAutoMoveState()
    {
        previousAutoMoveState = isAutoMoveEnabled;
        isAutoMoveEnabled = false;
        ApplyAutoMoveStateToAllCats();
    }

    // 자동이동 UI 비활성화 함수
    private void DisableAutoMoveUI()
    {
        openAutoMovePanelButton.interactable = false;
        if (autoMovePanel.activeSelf)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleAutoMoveState()
    {
        RestoreAutoMoveState();
        EnableAutoMoveUI();

        GoogleSave();
    }

    // 자동이동 상태 복구 함수
    private void RestoreAutoMoveState()
    {
        isAutoMoveEnabled = previousAutoMoveState;
        ApplyAutoMoveStateToAllCats();
    }

    // 자동이동 UI 활성화 함수
    private void EnableAutoMoveUI()
    {
        openAutoMovePanelButton.interactable = true;
    }

    #endregion

    

    #region Sort System

    // 모든 고양이 이동 중지 함수
    public void StopAllCatsMovement()
    {
        StopAllCoroutines();

        // 모든 고양이 이동 초기화
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.StopAllMovement();
        }
    }

    #endregion

    

    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool isAutoMoveEnabled;          // 자동 이동 활성화 상태
        public bool previousAutoMoveState;      // 이전 상태
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            isAutoMoveEnabled = this.isAutoMoveEnabled,
            previousAutoMoveState = this.previousAutoMoveState,
        };
        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.isAutoMoveEnabled = savedData.isAutoMoveEnabled;
        this.previousAutoMoveState = savedData.previousAutoMoveState;

        UpdateAutoMoveButtonColor();
        ApplyAutoMoveStateToAllCats();

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            Debug.Log("구글 저장");
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}

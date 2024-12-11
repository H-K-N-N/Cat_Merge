using UnityEngine;
using TMPro;
using UnityEngine.UI;

// GameManager Script
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }    // SingleTon

    // Data
    private Cat[] allCatData;                                   // 모든 고양이 데이터 보유
    public Cat[] AllCatData => allCatData;

    private int maxCats = 8;                                    // 화면 내 최대 고양이 수
    private int currentCatCount = 0;                            // 화면 내 고양이 수

    // UI
    [SerializeField] private TextMeshProUGUI catCountText;      // 고양이 수 텍스트

    // Merge On/Off
    [Header("Merge On/Off")]
    [SerializeField] private Button openMergePanelButton;       // 머지 패널 열기 버튼
    [SerializeField] private GameObject mergePanel;             // 머지 On/Off 패널
    [SerializeField] private Button closeMergePanelButton;      // 머지 패널 닫기 버튼
    [SerializeField] private Button mergeStateButton;           // 머지 상태 변경 버튼
    [SerializeField] private TextMeshProUGUI mergeStateText;    // 머지 현재 상태 텍스트
    private bool isMergeEnabled = true;

    // AutoMove On/Off
    [Header ("AutoMove On/Off")]
    [SerializeField] private Button openAutoMovePanelButton;    // 자동 이동 패널 열기 버튼
    [SerializeField] private GameObject autoMovePanel;          // 자동 이동 On/Off 패널
    [SerializeField] private Button closeAutoMovePanelButton;   // 자동 이동 패널 닫기 버튼
    [SerializeField] private Button autoMoveStateButton;        // 자동 이동 상태 변경 버튼
    [SerializeField] private TextMeshProUGUI autoMoveStateText; // 자동 이동 현재 상태 텍스트
    private bool isAutoMoveEnabled = true;                      // 자동 이동 활성화 상태

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadAllCats();
        UpdateCatCountText();

        // 자동이동 On/Off 관련
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();
        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMove);

        // 머지 On/Off 관련
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }

    // 고양이 정보 Load 함수
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // 고양이 수 판별 함수
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // 현재 고양이 수 증가시키는 함수
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // 현재 고양이 수 감소시키는 함수
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // 고양이 수 텍스트 UI 업데이트하는 함수
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }



    // 머지 패널 열기
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // 머지 상태 전환 함수
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeStateText();
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    // 머지 상태 Text 업데이트 함수
    private void UpdateMergeStateText()
    {
        if (mergeStateText != null)
        {
            mergeStateText.text = isMergeEnabled ? "OFF" : "ON";
        }
    }

    // 머지 버튼 색상 업데이트
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            Color normalColor = isMergeEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openMergePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openMergePanelButton.colors = colors;
        }
    }

    // 머지 패널 닫기
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // 머지 상태 반환 함수
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }



    // 자동이동 패널 열기
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // 자동이동 상태 전환
    public void ToggleAutoMove()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;

        // 모든 고양이에 상태 적용
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        // 상태 업데이트 및 버튼 색상 변경
        UpdateAutoMoveStateText();
        UpdateOpenButtonColor();

        // 패널 닫기
        CloseAutoMovePanel();
    }

    // 자동이동 상태 Text 업데이트 함수
    private void UpdateAutoMoveStateText()
    {
        if (autoMoveStateText != null)
        {
            autoMoveStateText.text = isAutoMoveEnabled ? "OFF" : "ON";
        }
    }

    // 자동이동 버튼 색상 업데이트
    private void UpdateOpenButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            Color normalColor = isAutoMoveEnabled ? new Color(1f, 1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorBlock colors = openAutoMovePanelButton.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = normalColor;
            colors.pressedColor = normalColor;
            colors.selectedColor = normalColor;
            openAutoMovePanelButton.colors = colors;
        }
    }

    // 자동이동 패널 닫기
    public void CloseAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(false);
        }
    }

    // 자동이동 현재 상태 반환
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

}

using UnityEngine;
using UnityEngine.UI;

public class AutoMoveManager : MonoBehaviour
{
    // Singleton Instance
    public static AutoMoveManager Instance { get; private set; }

    [Header("---[AutoMove On/Off System]")]
    [SerializeField] private Button openAutoMovePanelButton;        // 자동 이동 패널 열기 버튼
    [SerializeField] private GameObject autoMovePanel;              // 자동 이동 On/Off 패널
    [SerializeField] private Button closeAutoMovePanelButton;       // 자동 이동 패널 닫기 버튼
    [SerializeField] private Button autoMoveStateButton;            // 자동 이동 상태 버튼
    public bool isAutoMoveEnabled = true;                           // 자동 이동 활성화 상태
    private bool previousAutoMoveState = true;                      // 이전 상태 저장

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

        UpdateAutoMoveButtonColor();

        openAutoMovePanelButton.onClick.AddListener(OpenAutoMovePanel);
        closeAutoMovePanelButton.onClick.AddListener(CloseAutoMovePanel);
        autoMoveStateButton.onClick.AddListener(ToggleAutoMoveState);
    }

    // ======================================================================================================================

    // 자동이동 패널 여는 함수
    private void OpenAutoMovePanel()
    {
        if (autoMovePanel != null)
        {
            autoMovePanel.SetActive(true);
        }
    }

    // 자동이동 상태 전환 함수
    public void ToggleAutoMoveState()
    {
        isAutoMoveEnabled = !isAutoMoveEnabled;

        // 모든 고양이에 상태 적용
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        UpdateAutoMoveButtonColor();
        CloseAutoMovePanel();
    }

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleAutoMoveState()
    {
        previousAutoMoveState = isAutoMoveEnabled;
        isAutoMoveEnabled = false;

        // 모든 고양이에 상태 적용
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        openAutoMovePanelButton.interactable = false;
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleAutoMoveState()
    {
        isAutoMoveEnabled = previousAutoMoveState;

        // 모든 고양이에 상태 적용
        CatData[] allCatDataObjects = FindObjectsOfType<CatData>();
        foreach (var catData in allCatDataObjects)
        {
            catData.SetAutoMoveState(isAutoMoveEnabled);
        }

        openAutoMovePanelButton.interactable = true;
    }

    // 자동이동 버튼 색상 업데이트 함수
    private void UpdateAutoMoveButtonColor()
    {
        if (openAutoMovePanelButton != null)
        {
            string colorCode = !isAutoMoveEnabled ? "#5f5f5f" : "#FFFFFF";
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                openAutoMovePanelButton.GetComponent<Image>().color = color;
            }
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

    // 자동이동 현재 상태 반환하는 함수
    public bool IsAutoMoveEnabled()
    {
        return isAutoMoveEnabled;
    }

}

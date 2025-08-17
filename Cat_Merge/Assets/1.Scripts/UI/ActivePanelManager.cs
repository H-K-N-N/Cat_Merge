using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// 패널 관리 스크립트
public class ActivePanelManager : MonoBehaviour
{


    #region Variables

    public static ActivePanelManager Instance { get; private set; }

    // 패널 우선순위 정의
    public enum PanelPriority
    {
        Low = 0,        // 일반 패널
        Medium = 1,     // 중요 패널
        High = 2,       // 시스템 패널
        Critical = 3    // 필수 패널 (종료 등)
    }

    private class PanelInfo
    {
        public GameObject Panel { get; }
        public Image ButtonImage { get; }
        public PanelPriority Priority { get; }
        public CanvasGroup CanvasGroup { get; }

        public PanelInfo(GameObject panel, Image buttonImage, PanelPriority priority)
        {
            Panel = panel;
            ButtonImage = buttonImage;
            Priority = priority;
            CanvasGroup = panel.GetComponent<CanvasGroup>();

            // CanvasGroup이 없으면 추가
            if (CanvasGroup == null)
            {
                CanvasGroup = panel.AddComponent<CanvasGroup>();
            }
        }
    }

    private readonly Dictionary<string, PanelInfo> panels = new Dictionary<string, PanelInfo>();
    private readonly Stack<string> activePanelStack = new Stack<string>();
    public string ActivePanelName => activePanelStack.Count > 0 ? activePanelStack.Peek() : null;

    [Header("---[UI Color]")]
    private const float inactiveAlpha = 180f / 255f;

    // 버튼 알파값을 조절할 특정 패널들
    private readonly string[] alphaControlPanels = { "DictionaryMenu", "OptionMenu", "QuestMenu" };

    // 임시 스택 재사용을 위한 객체
    private readonly Stack<string> tempStack = new Stack<string>();

    // UI Disable GameObject들의 CanvasGroup 컴포넌트들
    [Header("---[UI Disable]")]
    [SerializeField] private CanvasGroup mainUIPanelCanvasGroup;
    [SerializeField] private CanvasGroup mergeAutoFillAmountImgCanvasGroup;
    [SerializeField] private CanvasGroup spawnAutoFillAmountImgCanvasGroup;

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
        foreach (var panel in panels.Values)
        {
            panel.Panel.SetActive(false);
            if (panel.ButtonImage != null && ShouldControlButtonAlpha(panel))
            {
                UpdateButtonColor(panel.ButtonImage, false);
            }
        }
    }

    #endregion


    #region Panel Registration

    // 패널 등록 함수
    public void RegisterPanel(string panelName, GameObject panel, Image buttonImage = null, PanelPriority priority = PanelPriority.Low)
    {
        if (!panels.ContainsKey(panelName))
        {
            panels.Add(panelName, new PanelInfo(panel, buttonImage, priority));
        }
    }

    #endregion


    #region Panel Management

    // 패널 열기 함수
    public void OpenPanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            return;
        }

        var newPanel = panels[panelName];

        // 현재 활성화된 패널이 있다면
        if (activePanelStack.Count > 0)
        {
            var currentPanelName = activePanelStack.Peek();
            var currentPanel = panels[currentPanelName];

            // 새 패널의 우선순위가 더 높거나 같으면 현재 패널을 닫음
            if (newPanel.Priority >= currentPanel.Priority)
            {
                ClosePanel(currentPanelName);
            }
        }

        // 새 패널 열기
        newPanel.Panel.SetActive(true);
        if (newPanel.CanvasGroup != null)
        {
            newPanel.CanvasGroup.alpha = 1f;
            newPanel.CanvasGroup.blocksRaycasts = true;
        }

        activePanelStack.Push(panelName);
        UpdateButtonColors();
        UpdateUIDisableVisibility();
    }

    // 패널 닫기 함수
    public void ClosePanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            return;
        }

        PanelInfo panelInfo = panels[panelName];
        if (panelInfo.Panel.activeSelf)
        {
            // 패널을 비활성화하되 그리기를 방지하기 위해 CanvasGroup alpha를 0으로 설정
            if (panelInfo.CanvasGroup != null)
            {
                panelInfo.CanvasGroup.alpha = 0f;
                panelInfo.CanvasGroup.blocksRaycasts = false;
            }

            if (activePanelStack.Contains(panelName))
            {
                tempStack.Clear();
                while (activePanelStack.Count > 0)
                {
                    var currentPanel = activePanelStack.Pop();
                    if (currentPanel != panelName)
                    {
                        tempStack.Push(currentPanel);
                    }
                }
                while (tempStack.Count > 0)
                {
                    activePanelStack.Push(tempStack.Pop());
                }
            }

            // 스택의 최상위 패널 활성화
            if (activePanelStack.Count > 0)
            {
                var topPanel = panels[activePanelStack.Peek()];
                if (topPanel.CanvasGroup != null)
                {
                    topPanel.CanvasGroup.alpha = 1f;
                    topPanel.CanvasGroup.blocksRaycasts = true;
                }
            }

            UpdateButtonColors();
            UpdateUIDisableVisibility();
        }
    }

    // 패널 토글 함수
    public void TogglePanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            return;
        }

        if (IsPanelActive(panelName))
        {
            ClosePanel(panelName);
        }
        else
        {
            OpenPanel(panelName);
        }
    }

    // 모든 패널 닫기 함수
    public void CloseAllPanels()
    {
        while (activePanelStack.Count > 0)
        {
            ClosePanel(activePanelStack.Peek());
        }
    }

    #endregion


    #region UI Management

    // 모든 버튼 색상 업데이트 함수
    private void UpdateButtonColors()
    {
        foreach (var kvp in panels)
        {
            if (kvp.Value.ButtonImage != null && ShouldControlButtonAlpha(kvp.Value))
            {
                bool isActive = IsPanelActive(kvp.Key);
                UpdateButtonColor(kvp.Value.ButtonImage, isActive);
            }
        }
    }

    // 단일 버튼 색상 업데이트 함수
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        if (buttonImage == null)
        {
            return;
        }

        Color color = buttonImage.color;
        color.a = isActive ? 1f : inactiveAlpha;
        buttonImage.color = color;
    }

    // UI Disable GameObject들의 가시성 업데이트
    private void UpdateUIDisableVisibility()
    {
        // UI Disable을 적용할 특정 패널들
        string[] uiDisablePanels = { "BottomItemMenu", "BuyCatMenu", "ShopMenu", "DictionaryMenu", "QuestMenu", "OptionMenu" };

        // 현재 활성화된 패널 중에서 UI Disable을 적용할 패널이 있는지 확인
        bool hasUIDisablePanel = activePanelStack.Any(panelName => uiDisablePanels.Contains(panelName));

        // 메인 패널 이미지 가시성 제어
        if (mainUIPanelCanvasGroup != null)
        {
            mainUIPanelCanvasGroup.alpha = hasUIDisablePanel ? 0f : 1f;
            mainUIPanelCanvasGroup.blocksRaycasts = !hasUIDisablePanel;
        }
        if (mergeAutoFillAmountImgCanvasGroup != null)
        {
            mergeAutoFillAmountImgCanvasGroup.alpha = hasUIDisablePanel ? 0f : 1f;
            mergeAutoFillAmountImgCanvasGroup.blocksRaycasts = !hasUIDisablePanel;
        }
        if (spawnAutoFillAmountImgCanvasGroup != null)
        {
            spawnAutoFillAmountImgCanvasGroup.alpha = hasUIDisablePanel ? 0f : 1f;
            spawnAutoFillAmountImgCanvasGroup.blocksRaycasts = !hasUIDisablePanel;
        }
    }

    #endregion


    #region Utility Functions

    // 해당 패널의 버튼 알파값을 조절해야 하는지 확인하는 함수
    private bool ShouldControlButtonAlpha(PanelInfo panelInfo)
    {
        return alphaControlPanels.Any(panelName => panels.ContainsKey(panelName) && panels[panelName] == panelInfo);
    }

    // 현재 활성화된 패널이 있는지 확인하는 함수
    public bool HasActivePanel()
    {
        return activePanelStack.Count > 0;
    }

    // 특정 패널이 활성화되어 있는지 확인하는 함수
    public bool IsPanelActive(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            return false;
        }

        return panels[panelName].Panel.activeSelf && (panels[panelName].CanvasGroup == null || panels[panelName].CanvasGroup.alpha > 0f);
    }

    #endregion


}

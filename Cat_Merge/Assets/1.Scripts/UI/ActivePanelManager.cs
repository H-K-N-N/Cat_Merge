using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivePanelManager : MonoBehaviour
{
    private class PanelInfo                                 // PanelInfo Class
    {
        public GameObject Panel { get; }
        public Image ButtonImage { get; }

        public PanelInfo(GameObject panel, Image buttonImage)
        {
            Panel = panel;
            ButtonImage = buttonImage;
        }
    }

    private Dictionary<string, PanelInfo> panels;           // Panel과 버튼 정보를 저장할 Dictionary
    private string activePanelName;                         // 활성화된 Panel 이름
    public string ActivePanelName => activePanelName;

    [Header("---[UI Color]")]
    private const float inactiveAlpha = 180f / 255f;        // 비활성화상태 Alpha값

    private string[] topPanels = { "DictionaryMenu", "OptionMenu", "QuestMenu" };
    private string[] bottomPanels = { "BottomItemMenu", "BuyCatMenu" };

    // ======================================================================================================================

    private void Awake()
    {
        panels = new Dictionary<string, PanelInfo>();
        activePanelName = null;
    }

    private void Start()
    {
        if (panels != null)
        {
            foreach (var panel in panels.Values)
            {
                panel.Panel.SetActive(false);
                UpdateButtonColor(panel.ButtonImage, true);
            }
        }
    }

    // Panel과 버튼 등록
    public void RegisterPanel(string panelName, GameObject panel, Image buttonImage)
    {
        if (!panels.ContainsKey(panelName))
        {
            panels.Add(panelName, new PanelInfo(panel, buttonImage));
        }
    }

    // Panel 여닫는 함수
    public void TogglePanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            // 열려있던 Panel 버튼을 한번 더 눌렀다면
            if (activePanelName == panelName)
            {
                ClosePanel(activePanelName);
                activePanelName = null;
                // 특별 패널만 버튼 색상 변경
                foreach (var kvp in panels)
                {
                    if (System.Array.Exists(topPanels, x => x == kvp.Key))
                    {
                        UpdateButtonColor(kvp.Value.ButtonImage, true);
                    }
                }
            }
            else
            {
                // 현재 열려 있는 Panel 닫기
                if (activePanelName != null && activePanelName != panelName && panels.ContainsKey(activePanelName))
                {
                    ClosePanel(activePanelName);
                }

                // 새로운 Panel 열기
                PanelInfo panelInfo = panels[panelName];
                panelInfo.Panel.SetActive(true);
                activePanelName = panelName;

                // 특별 패널만 버튼 색상 업데이트
                foreach (var kvp in panels)
                {
                    if (System.Array.Exists(topPanels, x => x == kvp.Key))
                    {
                        UpdateButtonColor(kvp.Value.ButtonImage, kvp.Key == panelName);
                    }
                }
            }
        }
    }

    // Panel 닫는 함수
    public void ClosePanel(string panelName)
    {
        if (panels.ContainsKey(panelName))
        {
            PanelInfo panelInfo = panels[panelName];
            activePanelName = null;
            panelInfo.Panel.SetActive(false);

            // 특별 패널만 버튼 색상 변경
            foreach (var kvp in panels)
            {
                if (System.Array.Exists(topPanels, x => x == kvp.Key))
                {
                    UpdateButtonColor(kvp.Value.ButtonImage, true);
                }
            }
        }
    }

    // 버튼 색상 업데이트하는 함수
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        Color color = buttonImage.color;
        color.a = isActive ? 1f : inactiveAlpha;
        buttonImage.color = color;
    }

}

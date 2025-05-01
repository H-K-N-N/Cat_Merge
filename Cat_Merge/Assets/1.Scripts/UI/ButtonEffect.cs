using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ButtonEffect : MonoBehaviour
{
    [Header("스케일 조정값")]
    public float scaleFactor = 0.9f;
    public float scaleSpeed = 10f; // 크기 변하는 속도 (커질수록 빠르게)

    private class ButtonScaleData
    {
        public Vector3 originalScale;
        public Vector3 targetScale;
        public Transform transform;
    }

    private List<ButtonScaleData> buttonList = new List<ButtonScaleData>();


    private void Start()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);

        foreach (Button button in allButtons)
        {
            // Dictionary Panel과 Information Panel의 하위 버튼들은 제외
            if (IsButtonInExcludedArea(button))
            {
                continue;
            }

            var data = new ButtonScaleData
            {
                originalScale = button.transform.localScale,
                targetScale = button.transform.localScale,
                transform = button.transform
            };
            buttonList.Add(data);

            AddButtonEvents(button, data);
        }
    }

    private void Update()
    {
        foreach (var data in buttonList)
        {
            if (data.transform == null) 
            {
                continue;
            }

            data.transform.localScale = Vector3.Lerp(data.transform.localScale, data.targetScale, Time.deltaTime * scaleSpeed);
        }
    }

    // Normal Cat Dictionary과 Information Panel의 하위에 있는 버튼인지 확인하는 함수
    private bool IsButtonInExcludedArea(Button button)
    {
        Transform current = button.transform;

        bool foundNormalCatDictionary = false;
        bool foundInformationPanel = false;

        while (current != null)
        {
            string currentName = current.name;

            // Normal Cat Dictionary 관련 체크
            if (currentName == "Normal Cat Dictionary")
            {
                foundNormalCatDictionary = true;
            }
            // Information Panel 체크
            else if (currentName == "Information Panel")
            {
                foundInformationPanel = true;
            }

            // Normal Cat Dictionary의 하위 오브젝트인 경우
            if (foundNormalCatDictionary)
            {
                return true;
            }
            // Information Panel의 하위 오브젝트인 경우
            if (foundInformationPanel)
            {
                return true;
            }

            current = current.parent;
        }
        return false;
    }

    private void AddButtonEvents(Button button, ButtonScaleData data)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers = new List<EventTrigger.Entry>();

        // PointerDown
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((eventData) => OnButtonDown(data));
        trigger.triggers.Add(entryDown);

        // PointerUp
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((eventData) => OnButtonUp(data));
        trigger.triggers.Add(entryUp);
    }

    private void OnButtonDown(ButtonScaleData data)
    {
        data.targetScale = data.originalScale * scaleFactor;
    }

    private void OnButtonUp(ButtonScaleData data)
    {
        data.targetScale = data.originalScale;
    }


}
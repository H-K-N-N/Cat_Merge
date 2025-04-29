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
            // Dictionary Panel의 ScrollRectContents 하위의 버튼은 제외
            if (IsButtonInDictionaryScrollContent(button))
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

    // Dictionary Panel의 ScrollRectContents 하위에 있는 버튼인지 확인하는 함수
    private bool IsButtonInDictionaryScrollContent(Button button)
    {
        Transform current = button.transform;
        bool foundScrollRectContents = false;
        bool foundNormalCatDictionary = false;
        bool foundDictionaryPanel = false;

        while (current != null)
        {
            if (current.name == "ScrollRectContents")
            {
                foundScrollRectContents = true;
            }
            else if (current.name == "Normal Cat Dictionary")
            {
                foundNormalCatDictionary = true;
            }
            else if (current.name == "Dictionary Panel")
            {
                foundDictionaryPanel = true;
            }

            if (foundScrollRectContents && foundNormalCatDictionary && foundDictionaryPanel)
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
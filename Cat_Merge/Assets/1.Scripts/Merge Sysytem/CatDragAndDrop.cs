using UnityEngine;
using UnityEngine.EventSystems;

public class CatDragAndDrop : MonoBehaviour, IDragHandler, IDropHandler
{
    public Cat catData;                             // 드래그하는 고양이의 데이터
    public RectTransform rectTransform;                
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        // 화면 좌표를 UI 좌표로 변환
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent.GetComponent<RectTransform>(),
            eventData.position,
            parentCanvas.worldCamera,
            out localPointerPosition
        );

        // 드래그 위치 업데이트 (UI의 localPosition 사용)
        rectTransform.localPosition = localPointerPosition;
    }

    // 드롭 시
    public void OnDrop(PointerEventData eventData)
    {
        // 드롭 한 위치에
            // 같은 등급의 고양이가 있으면
                // 합성처리
                    // 합성처리 후 새로운 고양이 UI 갱신
            // 같은 등급의 고양이가 없으면 그냥 그 위치에 고양이 드롭

    }

}

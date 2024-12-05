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
        // 드랍한 위치에 근처의 CatPrefab을 찾기
        CatDragAndDrop nearbyCat = FindNearbyCat();
        if (nearbyCat != null && nearbyCat != this)
        {
            // 동일한 ID 확인 후 합성 처리
            if (nearbyCat.catData.CatId == this.catData.CatId)
            {
                // 합성 처리
                Cat mergedCat = FindObjectOfType<CatMerge>().MergeCats(this.catData, nearbyCat.catData);

                if (mergedCat != null)
                {
                    Debug.Log($"합성 성공: {mergedCat.CatName}");
                    nearbyCat.catData = mergedCat;
                    nearbyCat.UpdateCatUI();

                    // 현재 드래그 중인 CatPrefab 삭제
                    Destroy(this.gameObject);
                    return;
                }
                else
                {
                    Debug.LogWarning("합성 실패");
                    return;
                }
            }
            else
            {
                Debug.Log("다른 고양이가 존재");
            }
        }
        else
        {
            Debug.Log("드랍한 위치에 배치");
        }
    }

    // 범위 내에서 가장 가까운 CatPrefab 찾기
    private CatDragAndDrop FindNearbyCat()
    {
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect thisRect = GetWorldRect(rectTransform);

        // 현재 Rect 크기를 줄여서 감지 범위 조정 / 감지 범위 조정 비율
        thisRect = ShrinkRect(thisRect, 0.2f);

        foreach (Transform child in parentRect)
        {
            if (child == this.transform) continue;

            CatDragAndDrop otherCat = child.GetComponent<CatDragAndDrop>();
            if (otherCat != null)
            {
                Rect otherRect = GetWorldRect(otherCat.rectTransform);

                // Rect 간 충돌 확인
                if (thisRect.Overlaps(otherRect))
                {
                    // 충돌하는 첫 번째 CatPrefab 반환
                    return otherCat;
                }
            }
        }
        return null;
    }

    // RectTransform의 월드 좌표 기반 Rect 반환
    private Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    // Rect 크기를 줄이는 메서드
    private Rect ShrinkRect(Rect rect, float scale)
    {
        float widthReduction = rect.width * (1 - scale) / 2;
        float heightReduction = rect.height * (1 - scale) / 2;

        return new Rect(
            rect.x + widthReduction,
            rect.y + heightReduction,
            rect.width * scale,
            rect.height * scale
        );
    }

    // CatUI 갱신
    public void UpdateCatUI()
    {
        GetComponentInChildren<CatData>()?.SetCatData(catData);
    }

}

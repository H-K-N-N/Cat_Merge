using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// 고양이 드래그 앤 드랍 Script
public class CatDragAndDrop : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Cat catData;                             // 드래그하는 고양이의 데이터
    public RectTransform rectTransform;             // RectTransform 참조
    private Canvas parentCanvas;                    // 부모 Canvas 참조

    public bool isDragging { get; private set; }    // 드래그 상태 확인 플래그

    // ======================================================================================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // ======================================================================================================================

    // 드래그 시작 함수
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // 드래그된 고양이를 mergingCats에서 제거
        AutoMergeManager autoMerge = FindObjectOfType<AutoMergeManager>();
        if (autoMerge != null && autoMerge.IsMerging(this))
        {
            autoMerge.StopMerging(this);
        }

        // 드래그하는 객체를 부모의 자식 객체 중 최상단으로 이동 (드래그중인 객체가 UI상 제일 위로 보여질 수 있게)
        rectTransform.SetAsLastSibling();
    }

    // 드래그 진행중 함수
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

        // GamePanel RectTransform 가져오기
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect panelRect = new Rect(
            -parentRect.rect.width / 2,
            -parentRect.rect.height / 2,
            parentRect.rect.width,
            parentRect.rect.height
        );

        // 드래그 위치를 패널 범위 내로 제한
        localPointerPosition.x = Mathf.Clamp(localPointerPosition.x, panelRect.xMin, panelRect.xMax);
        localPointerPosition.y = Mathf.Clamp(localPointerPosition.y, panelRect.yMin, panelRect.yMax);

        // 드래그 위치 업데이트 (UI의 localPosition 사용)
        rectTransform.localPosition = localPointerPosition;
    }

    // 드래그 종료 함수
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 머지 상태가 OFF일 경우 머지 X
        if (!MergeManager.Instance.IsMergeEnabled())
        {
            return;
        }

        CheckEdgeDrop();

        CatDragAndDrop nearbyCat = FindNearbyCat();
        if (nearbyCat != null && nearbyCat != this)
        {
            // 자동 머지 중인지 확인
            if (IsAutoMerging(nearbyCat))
            {
                Debug.Log("자동 머지 중인 고양이와 합성할 수 없습니다.");
                return;
            }

            // 동일한 등급 확인 후 합성 처리
            if (nearbyCat.catData.CatGrade == this.catData.CatGrade)
            {
                // 합성할 고양이의 다음 등급이 존재하는지 확인
                Cat nextCat = FindObjectOfType<MergeManager>().GetCatByGrade(this.catData.CatGrade + 1);
                if (nextCat != null)
                {
                    // nextCat이 존재할 경우에만 애니메이션 시작
                    StartCoroutine(PullNearbyCat(nearbyCat));
                }
                else
                {
                    //Debug.LogWarning("다음 등급의 고양이가 없음");
                }
            }
            else
            {
                //Debug.LogWarning("등급이 다름");
            }
        }
        else
        {
            //Debug.Log("드랍한 위치에 배치");
        }
    }

    // 가장자리 드랍여부 확인 함수
    private void CheckEdgeDrop()
    {
        // GamePanel RectTransform 가져오기
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect panelRect = new Rect(
            -parentRect.rect.width / 2,
            -parentRect.rect.height / 2,
            parentRect.rect.width,
            parentRect.rect.height
        );

        Vector2 currentPos = rectTransform.localPosition;

        // 가장자리에 맞닿아 있는지 확인
        bool isNearLeft = Mathf.Abs(currentPos.x - panelRect.xMin) < 10;
        bool isNearRight = Mathf.Abs(currentPos.x - panelRect.xMax) < 10;
        bool isNearTop = Mathf.Abs(currentPos.y - panelRect.yMax) < 10;
        bool isNearBottom = Mathf.Abs(currentPos.y - panelRect.yMin) < 10;

        Vector3 targetPos = currentPos;

        // 가장자리에 가까우면 중심 방향으로 이동
        if (isNearLeft) targetPos.x += 30;
        if (isNearRight) targetPos.x -= 30;
        if (isNearTop) targetPos.y -= 30;
        if (isNearBottom) targetPos.y += 30;

        // 위치 보정 애니메이션 실행
        StartCoroutine(SmoothMoveToPosition(targetPos));
    }

    // 가장자리에서 안쪽으로 부드럽게 이동하는 애니메이션 코루틴
    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        rectTransform.localPosition = targetPosition;
    }

    // 자동 머지 중인지 확인하는 함수
    private bool IsAutoMerging(CatDragAndDrop nearbyCat)
    {
        AutoMergeManager autoMerge = FindObjectOfType<AutoMergeManager>();
        return autoMerge != null && autoMerge.IsMerging(nearbyCat);
    }

    // 합성시 고양이가 끌려오는 애니메이션 코루틴
    private IEnumerator PullNearbyCat(CatDragAndDrop nearbyCat)
    {
        Vector3 startPosition = nearbyCat.rectTransform.localPosition;
        Vector3 targetPosition = rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.1f;

        // nearbyCat을 드래그된 객체로 끌려오게 설정
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            nearbyCat.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        // 끌려온 후 합성 처리
        Cat mergedCat = FindObjectOfType<MergeManager>().MergeCats(this.catData, nearbyCat.catData);
        if (mergedCat != null)
        {
            //Debug.Log($"합성 성공: {mergedCat.CatName}");
            this.catData = mergedCat;
            UpdateCatUI();
            Destroy(nearbyCat.gameObject);
        }
        else
        {
            //Debug.LogWarning("합성 실패");
        }
    }

    // 범위 내에서 가장 가까운 CatPrefab을 찾는 함수
    private CatDragAndDrop FindNearbyCat()
    {
        RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
        Rect thisRect = GetWorldRect(rectTransform);

        // 현재 Rect 크기를 줄여서 감지 범위 조정 (감지 범위 조정 비율)
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
                    return otherCat;
                }
            }
        }
        return null;
    }

    // RectTransform의 월드 좌표 기반 Rect를 반환하는 함수
    private Rect GetWorldRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
    }

    // Rect 크기를 줄이는 함수 (고양이 드랍시 합성 범위)
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

    // CatUI 갱신하는 함수
    public void UpdateCatUI()
    {
        GetComponentInChildren<CatData>()?.SetCatData(catData);
    }

}

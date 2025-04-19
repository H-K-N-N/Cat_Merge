using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// 고양이 드래그 앤 드랍 Script
public class DragAndDropManager : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
{


    #region Variables

    public Cat catData;                                 // 드래그하는 고양이의 데이터
    public RectTransform rectTransform;                 // RectTransform 참조
    private Canvas parentCanvas;                        // 부모 Canvas 참조
    private RectTransform parentRect;                   // 캐싱용 부모 RectTransform
    private Vector2 panelBoundsMin;                     // 패널 경계 최소값 캐싱
    private Vector2 panelBoundsMax;                     // 패널 경계 최대값 캐싱
    private Vector2 dragOffset;                         // 드래그 시작 위치 오프셋

    public bool isDragging { get; private set; }        // 드래그 상태 확인 플래그

    private const float EDGE_THRESHOLD = 10f;           // 가장자리 감지 임계값
    private const float EDGE_PUSH = 30f;                // 가장자리에서 밀어내는 거리
    private const float MERGE_DETECTION_SCALE = 0.1f;   // 합성 감지 범위 스케일

    #endregion


    #region Unity Methods

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        parentRect = rectTransform.parent.GetComponent<RectTransform>();

        Vector2 panelSize = parentRect.rect.size * 0.5f;
        panelBoundsMin = -panelSize;
        panelBoundsMax = panelSize;
    }

    #endregion


    #region Click & Drag and Drop

    // 클릭한 순간
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        GetComponent<AnimatorManager>().ChangeState(CharacterState.isGrab);

        // 드래그 시작 위치 오프셋 계산
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, parentCanvas.worldCamera, out localPointerPosition))
        {
            dragOffset = rectTransform.localPosition - (Vector3)localPointerPosition;
        }

        // 자동 머지 중인 고양이 처리
        if (AutoMergeManager.Instance != null && AutoMergeManager.Instance.IsMerging(this))
        {
            AutoMergeManager.Instance.StopMerging(this);
        }
    }

    // 클릭 뗀 순간
    public void OnPointerClick(PointerEventData eventData)
    {
        isDragging = false;
        GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);
    }

    // 드래그 진행중
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, parentCanvas.worldCamera, out localPointerPosition)) return;

        // 드래그 오프셋 적용 및 위치 제한
        Vector2 targetPosition = localPointerPosition + dragOffset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, panelBoundsMin.x, panelBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, panelBoundsMin.y, panelBoundsMax.y);

        rectTransform.localPosition = targetPosition;

        // Y축 기준 정렬 (60fps 기준 3프레임마다 실행)
        if (Time.frameCount % 3 == 0)
        {
            UpdateSiblingIndexBasedOnY();
        }
    }

    // Y축 값을 기준으로 드래그 객체의 정렬을 업데이트하는 함수
    private void UpdateSiblingIndexBasedOnY()
    {
        int childCount = parentRect.childCount;
        float currentY = rectTransform.localPosition.y;
        int newIndex = 0;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform sibling = parentRect.GetChild(i).GetComponent<RectTransform>();
            if (sibling != null && sibling != rectTransform && currentY < sibling.localPosition.y)
            {
                newIndex = i + 1;
            }
        }

        if (rectTransform.GetSiblingIndex() != newIndex)
        {
            rectTransform.SetSiblingIndex(newIndex);
        }
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!gameObject.activeSelf) return;

        isDragging = false;

        if (BattleManager.Instance.IsBattleActive)
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isBattle);
            BattleDrop(eventData);
        }
        else
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);

            // 가장자리 드랍 체크는 머지 상태와 관계없이 항상 실행
            CheckEdgeDrop();

            // 머지 상태가 OFF일 경우 머지 X
            if (!MergeManager.Instance.IsMergeEnabled()) return;

            DragAndDropManager nearbyCat = FindNearbyCat();
            if (nearbyCat != null && nearbyCat != this && nearbyCat.gameObject.activeSelf)
            {
                // 동일한 등급 확인 후 합성 처리
                if (nearbyCat.catData.CatGrade == this.catData.CatGrade)
                {
                    Cat nextCat = MergeManager.Instance.GetCatByGrade(this.catData.CatGrade + 1);
                    if (nextCat != null)
                    {
                        StartCoroutine(PullNearbyCat(nearbyCat));
                    }
                }
            }
        }
    }

    // 가장자리 드랍여부 확인 함수
    private void CheckEdgeDrop()
    {
        Vector2 currentPos = rectTransform.localPosition;
        Vector2 targetPos = currentPos;
        bool needsAdjustment = false;

        // 가장자리 체크 및 보정
        if (Mathf.Abs(currentPos.x - panelBoundsMin.x) < EDGE_THRESHOLD) { targetPos.x += EDGE_PUSH; needsAdjustment = true; }
        if (Mathf.Abs(currentPos.x - panelBoundsMax.x) < EDGE_THRESHOLD) { targetPos.x -= EDGE_PUSH; needsAdjustment = true; }
        if (Mathf.Abs(currentPos.y - panelBoundsMax.y) < EDGE_THRESHOLD) { targetPos.y -= EDGE_PUSH; needsAdjustment = true; }
        if (Mathf.Abs(currentPos.y - panelBoundsMin.y) < EDGE_THRESHOLD) { targetPos.y += EDGE_PUSH; needsAdjustment = true; }

        if (needsAdjustment)
        {
            StartCoroutine(SmoothMoveToPosition(targetPos));
        }
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

    #endregion


    #region Battle System

    // 전투중일때 드래그 앤 드랍 관련 함수
    private void BattleDrop(PointerEventData eventData)
    {
        if (!BattleManager.Instance.IsBattleActive) return;

        BossHitbox currentBossHitBox = BattleManager.Instance.bossHitbox;
        GameObject droppedObject = eventData.pointerDrag;
        CatData catData = droppedObject.GetComponent<CatData>();

        // 히트박스 내부라면
        if (currentBossHitBox.IsInHitbox(this.rectTransform.anchoredPosition))
        {
            catData.MoveOppositeBoss();
        }
        // 히트박스 외부라면
        else if (!currentBossHitBox.IsInHitbox(this.rectTransform.anchoredPosition))
        {
            catData.MoveTowardBossBoundary();
        }
    }

    #endregion


    #region Merge System

    // 합성시 고양이가 끌려오는 애니메이션 코루틴
    private IEnumerator PullNearbyCat(DragAndDropManager nearbyCat)
    {
        // 자동 머지 중인 경우 해당 고양이들을 자동 머지에서 제외
        if (AutoMergeManager.Instance != null)
        {
            AutoMergeManager.Instance.StopMerging(this);
            AutoMergeManager.Instance.StopMerging(nearbyCat);
        }

        Vector3 startPosition = nearbyCat.rectTransform.localPosition;
        Vector3 targetPosition = rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            if (nearbyCat == null) yield break;

            elapsed += Time.deltaTime;
            nearbyCat.rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);

            yield return null;
        }

        if (nearbyCat != null)
        {
            CompleteMerge(this, nearbyCat);
        }
    }

    // 고양이 머지 처리 함수
    private void CompleteMerge(DragAndDropManager cat1, DragAndDropManager cat2)
    {
        if (cat1 == null || cat2 == null || !cat1.gameObject.activeSelf || !cat2.gameObject.activeSelf) return;
        if (cat1 == cat2) return;

        cat1.GetComponent<CatData>()?.CleanupCoroutines();
        cat2.GetComponent<CatData>()?.CleanupCoroutines();

        Cat mergedCat = MergeManager.Instance.MergeCats(cat1.catData, cat2.catData);
        if (mergedCat != null)
        {
            SpawnManager.Instance.ReturnCatToPool(cat2.gameObject);
            GameManager.Instance.DeleteCatCount();

            cat1.catData = mergedCat;
            cat1.UpdateCatUI();
            SpawnManager.Instance.RecallEffect(cat1.gameObject);
        }
    }

    // 범위 내에서 가장 가까운 고양이를 찾는 함수
    private DragAndDropManager FindNearbyCat()
    {
        // 현재 Rect 크기를 줄여서 감지 범위 조정 (감지 범위 조정 비율)
        Rect thisRect = GetWorldRect(rectTransform);
        thisRect = ShrinkRect(thisRect, MERGE_DETECTION_SCALE);

        DragAndDropManager nearestCat = null;
        float nearestDistance = float.MaxValue;

        int childCount = parentRect.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = parentRect.GetChild(i);
            if (child == transform || !child.gameObject.activeSelf) continue;

            DragAndDropManager otherCat = child.GetComponent<DragAndDropManager>();
            if (otherCat != null && thisRect.Overlaps(GetWorldRect(otherCat.rectTransform)))
            {
                float distance = Vector2.Distance(rectTransform.position, otherCat.rectTransform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCat = otherCat;
                }
            }
        }
        return nearestCat;
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
        float widthReduction = rect.width * (1 - scale) * 0.5f;
        float heightReduction = rect.height * (1 - scale) * 0.5f;

        return new Rect(
            rect.x + widthReduction,
            rect.y + heightReduction,
            rect.width * scale,
            rect.height * scale
        );
    }

    #endregion


    #region UI Update

    // CatUI 갱신하는 함수
    public void UpdateCatUI()
    {
        GetComponentInChildren<CatData>()?.SetCatData(catData);
    }

    #endregion


}

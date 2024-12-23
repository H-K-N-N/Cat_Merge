using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 객체가 가지고있는 고양이의 정보를 담는 Script
public class CatData : MonoBehaviour
{
    public Cat catData;                         // 고양이 데이터
    private Image catImage;                     // 고양이 이미지

    private RectTransform rectTransform;        // RectTransform 참조
    private RectTransform parentPanel;          // 부모 패널 RectTransform
    private CatDragAndDrop catDragAndDrop;      // CatDragAndDrop 참조

    private bool isAnimating = false;           // 애니메이션 중인지 확인 플래그
    private bool isAutoMoveEnabled = true;      // 자동 이동 활성화 상태

    // ======================================================================================================================

    private void Awake()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<CatDragAndDrop>();
    }

    private void Start()
    {
        UpdateCatUI();
    }

    // ======================================================================================================================

    // CatUI 최신화하는 함수
    public void UpdateCatUI()
    {
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // Cat 데이터 설정 함수
    public void SetCatData(Cat cat)
    {
        catData = cat;
        UpdateCatUI();
    }

    // 자동 이동을 활성화/비활성화하는 함수
    public void SetAutoMoveState(bool isEnabled)
    {
        isAutoMoveEnabled = isEnabled;

        // 자동 이동을 활성화하려면 코루틴 시작
        if (isAutoMoveEnabled)
        {
            if (!isAnimating)
            {
                StartCoroutine(AutoMove());
            }
        }
        else
        {
            StopAllCoroutines();
            isAnimating = false;
        }
    }

    // 10초마다 자동으로 이동하는 코루틴
    public IEnumerator AutoMove()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (!isAnimating && (catDragAndDrop == null || !catDragAndDrop.isDragging))
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)rectTransform.anchoredPosition + randomDirection;

                StartCoroutine(SmoothMoveToPosition(targetPosition));
                yield return new WaitForSeconds(0.5f);

                // 위치가 범위를 초과했으면 안쪽으로 조정
                if (IsOutOfBounds(targetPosition))
                {
                    Vector3 adjustedPosition = AdjustPositionToBounds(targetPosition);
                    StartCoroutine(SmoothMoveToPosition(adjustedPosition));
                }
            }
        }
    }

    // 랜덤 방향 계산 함수
    private Vector3 GetRandomDirection()
    {
        float moveRange = 30f;

        // 8방향 (상, 하, 좌, 우, 대각선 포함)
        Vector2[] directions = new Vector2[]
        {
            new Vector2(moveRange, 0f),
            new Vector2(-moveRange, 0f),
            new Vector2(0f, moveRange),
            new Vector2(0f, -moveRange),
            new Vector2(moveRange, moveRange),
            new Vector2(moveRange, -moveRange),
            new Vector2(-moveRange, moveRange),
            new Vector2(-moveRange, -moveRange)
        };

        // 랜덤으로 방향 선택
        int randomIndex = Random.Range(0, directions.Length);
        return directions[randomIndex];
    }

    // 범위 초과 여부를 확인하는 함수
    private bool IsOutOfBounds(Vector3 position)
    {
        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        return position.x <= minBounds.x || position.x >= maxBounds.x || position.y <= minBounds.y || position.y >= maxBounds.y;
    }

    // 초과된 위치를 안쪽으로 조정하는 함수
    private Vector3 AdjustPositionToBounds(Vector3 position)
    {
        Vector3 adjustedPosition = position;

        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        if (position.x <= minBounds.x) adjustedPosition.x = minBounds.x + 30f;
        if (position.x >= maxBounds.x) adjustedPosition.x = maxBounds.x - 30f;
        if (position.y <= minBounds.y) adjustedPosition.y = minBounds.y + 30f;
        if (position.y >= maxBounds.y) adjustedPosition.y = maxBounds.y - 30f;

        return adjustedPosition;
    }

    // 부드럽게 이동하는 코루틴
    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        isAnimating = true;

        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;
    }

    // 고양이가 파괴될때 고양이 수 감소시키는 함수
    private void OnDestroy()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.DeleteCatCount();
        }
    }

}

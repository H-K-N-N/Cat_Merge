using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 고양이 본인들의 정보
public class CatData : MonoBehaviour
{
    private Cat catData;                        // 고양이 데이터
    private Image catImage;                     // 고양이 이미지

    private RectTransform rectTransform;        // RectTransform 참조
    private bool isAnimating = false;           // 애니메이션 중인지 확인 플래그

    private void Awake()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateCatUI();
        StartCoroutine(PeriodicMovement());
    }

    // CatUI 최신화
    public void UpdateCatUI()
    {
        CatDragAndDrop catDragAndDrop = GetComponentInParent<CatDragAndDrop>();

        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // Cat 데이터 설정
    public void SetCatData(Cat cat)
    {
        catData = cat;
        UpdateCatUI();
    }

    // 10초마다 자동으로 이동하는 코루틴
    private IEnumerator PeriodicMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            if (!isAnimating)
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)rectTransform.anchoredPosition + randomDirection;
                StartCoroutine(SmoothMoveToPosition(targetPosition));
            }
        }
    }

    // 랜덤 방향 계산
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

}

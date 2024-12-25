using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 객체가 가지고있는 고양이의 정보를 담는 Script
public class CatData : MonoBehaviour
{
    public Cat catData;                         // 고양이 데이터
    private Image catImage;                     // 고양이 이미지
    
    private RectTransform rectTransform;        // RectTransform 참조
    private RectTransform parentPanel;          // 부모 패널 RectTransform
    private DragAndDropManager catDragAndDrop;  // DragAndDropManager 참조
    
    private bool isAnimating = false;           // 애니메이션 중인지 확인 플래그
    private bool isAutoMoveEnabled = true;      // 자동 이동 활성화 상태
    private Coroutine autoMoveCoroutine;        // 자동 이동 코루틴

    private TextMeshProUGUI collectCoinText;    // 자동 재화 획득 텍스트
    private Image collectCoinImage;             // 자동 재화 획득 이미지
    //private Animator catAnimator;               // 자동 재화 획득 Animator 컴포넌트 참조

    private float collectingTime = 2f;          // 자동 재화 수집 시간
    public float CollectingTime { get => collectingTime; set => collectingTime = value; }
    private bool isCollectingCoins = true;      // 자동 재화 수집 활성화 상태

    // ======================================================================================================================

    private void Awake()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<DragAndDropManager>();

        collectCoinText = transform.Find("CollectCoinText").GetComponent<TextMeshProUGUI>();
        collectCoinImage = transform.Find("CollectCoinImage").GetComponent<Image>();
        //catAnimator = GetComponent<Animator>();

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        UpdateCatUI();
        StartCoroutine(AutoCollectCoins());
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

    // ======================================================================================================================

    // 자동 이동을 활성화/비활성화하는 함수
    public void SetAutoMoveState(bool isEnabled)
    {
        isAutoMoveEnabled = isEnabled;

        // 자동 이동을 활성화하려면 코루틴 시작
        if (isAutoMoveEnabled)
        {
            if (!isAnimating && autoMoveCoroutine == null)
            {
                autoMoveCoroutine = StartCoroutine(AutoMove());
            }
        }
        else
        {
            // 모든 코루틴 중단 -> 자동 이동만 중단
            if (autoMoveCoroutine != null)
            {
                StopCoroutine(autoMoveCoroutine);
                autoMoveCoroutine = null;
            }
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

                yield return StartCoroutine(SmoothMoveToPosition(targetPosition));

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteCatCount();
        }
    }

    // ======================================================================================================================

    // 자동 재화 수집 코루틴
    private IEnumerator AutoCollectCoins()
    {
        while (isCollectingCoins)
        {
            yield return new WaitForSeconds(collectingTime);

            if (catData != null && GameManager.Instance != null)
            {
                int collectedCoins = catData.CatGetCoin;
                GameManager.Instance.Coin += collectedCoins;

                // 모션 실행 및 재화 획득 텍스트 활성화
                StartCoroutine(PlayCollectingAnimation(collectedCoins));
            }
        }
    }

    // 모션과 재화 획득 텍스트 처리 코루틴
    private IEnumerator PlayCollectingAnimation(int collectedCoins)
    {
        //// 애니메이션 시작 (아직 제작 안함)
        //if (catAnimator != null)
        //{
        //    catAnimator.SetTrigger("CollectCoin");      // CollectCoin 트리거 실행
        //}

        collectCoinText.text = $"+{collectedCoins}";
        collectCoinText.gameObject.SetActive(true);
        collectCoinImage.gameObject.SetActive(true);

        // 애니메이션 대기 (애니메이션 길이 설정에 맞추기)
        float animationDuration = 0.5f;     // 애니메이션 길이 (Animator 설정에 따라 조정)
        yield return new WaitForSeconds(animationDuration);

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }

}

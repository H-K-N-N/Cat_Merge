using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 고양이의 정보와 행동을 관리하는 스크립트
public class CatData : MonoBehaviour
{
    #region Variables
    [Header("Cat Data")]
    public Cat catData;                             // 고양이 기본 데이터
    private Image catImage;                         // 고양이 이미지 컴포넌트
    public double catHp;                            // 현재 고양이 체력
    public bool isStuned = false;                   // 기절 상태 여부
    private float stunTime = 10f;                   // 기절 시간

    [Header("HP UI")]
    [SerializeField] private GameObject hpImage;    // HP 이미지 오브젝트
    [SerializeField] private Image hpFillImage;     // HP Fill 이미지

    [Header("Transform")]
    private RectTransform rectTransform;            // 현재 오브젝트의 RectTransform
    private RectTransform parentPanel;              // 부모 패널의 RectTransform
    private DragAndDropManager catDragAndDrop;      // 드래그 앤 드롭 매니저

    [Header("Movement")]
    private bool isAnimating = false;               // 이동 애니메이션 진행 여부
    private Coroutine autoMoveCoroutine;            // 자동 이동 코루틴
    private Coroutine currentMoveCoroutine;         // 현재 진행 중인 이동 코루틴

    [Header("Coin Collection")]
    private TextMeshProUGUI collectCoinText;        // 코인 획득 텍스트
    private Image collectCoinImage;                 // 코인 획득 이미지
    private float collectingTime;                   // 코인 수집 간격
    public float CollectingTime { get => collectingTime; set => collectingTime = value; }
    private bool isCollectingCoins = true;          // 코인 수집 활성화 상태
    private Coroutine autoCollectCoroutine;         // 코인 수집 코루틴
    #endregion

    // ======================================================================================================================

    #region Unity Methods
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        UpdateCatUI();
        autoCollectCoroutine = StartCoroutine(AutoCollectCoins());
        UpdateHPBar();
    }

    // 오브젝트 파괴시 고양이 수 감소
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DeleteCatCount();
        }
    }
    #endregion

    // ======================================================================================================================

    #region Initialization
    // 컴포넌트 초기화
    private void InitializeComponents()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<DragAndDropManager>();

        collectCoinText = transform.Find("CollectCoinText").GetComponent<TextMeshProUGUI>();
        collectCoinImage = transform.Find("CollectCoinImage").GetComponent<Image>();

        hpImage = transform.Find("HP Image").gameObject;
        hpFillImage = hpImage.transform.Find("Fill Image").GetComponent<Image>();
        hpImage.SetActive(false);

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }

    // UI 업데이트
    public void UpdateCatUI()
    {
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // 고양이 데이터 설정
    public void SetCatData(Cat cat)
    {
        catData = cat;
        catHp = catData.CatHp;
        UpdateCatUI();
        UpdateHPBar();
    }

    // HP 바 업데이트 함수
    private void UpdateHPBar()
    {
        float hpRatio = (float)catHp / catData.CatHp;
        hpFillImage.fillAmount = hpRatio;

        // 전투중이고, 체력이 최대치가 아닐 때 HP 바 표시
        if (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive && hpRatio < 1f && hpRatio > 0f)
        {
            hpImage.SetActive(true);
        }
        else
        {
            hpImage.SetActive(false);
        }
    }
    #endregion

    // ======================================================================================================================

    #region Battle System
    // 보스 히트박스 경계로 고양이 밀어내기
    public void MoveOppositeBoss()
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        BossHitbox bossHitbox = BattleManager.Instance.bossHitbox;

        // 고양이가 히트박스 내부에 있는 경우
        if (bossHitbox.IsInHitbox(catPosition))
        {
            // 히트박스 경계에 위치하도록 이동
            Vector3 targetPosition = bossHitbox.GetClosestBoundaryPoint(catPosition);
            StartCoroutine(SmoothMoveToPosition(targetPosition));
        }
    }

    // 보스 히트박스 경계로 고양이 이동
    public void MoveTowardBossBoundary()
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        BossHitbox bossHitbox = BattleManager.Instance.bossHitbox;
        
        // 고양이가 히트박스 외부에 있는 경우
        if (!bossHitbox.IsInHitbox(catPosition))
        {
            // 히트박스 경계에 위치하도록 이동
            Vector3 targetPosition = bossHitbox.GetClosestBoundaryPoint(catPosition);
            StartCoroutine(SmoothMoveToPosition(targetPosition));
        }
    }

    // 데미지 처리
    public void TakeDamage(double damage)
    {
        catHp -= damage;
        UpdateHPBar();
        //Debug.Log($"남은 체력 : {catHp}");

        if (catHp <= 0)
        {
            StartCoroutine(StunAndRecover(stunTime));
        }
    }

    // 기절 및 회복 처리
    private IEnumerator StunAndRecover(float stunTime)
    {
        SetStunState(true);
        yield return new WaitForSeconds(stunTime);
        SetStunState(false);
        //Debug.Log("고양이 체력 회복 완료!");
    }

    // 기절 상태 설정
    private void SetStunState(bool isStunned)
    {
        UpdateHPBar();
        SetCollectingCoinsState(!isStunned);
        SetAutoMoveState(!isStunned);
        SetRaycastTarget(!isStunned);
        isStuned = isStunned;
        catImage.color = isStunned ? new Color(1f, 0.5f, 0.5f, 0.7f) : Color.white;

        if (!isStunned)
        {
            HealCatHP();
        }
    }

    // Raycast Target 설정
    private void SetRaycastTarget(bool isActive)
    {
        catImage.raycastTarget = isActive;
    }

    // 체력 회복
    public void HealCatHP()
    {
        catHp = catData.CatHp;
        UpdateHPBar();
    }
    #endregion

    // ======================================================================================================================

    #region Auto Movement
    // 자동 이동 상태 설정
    public void SetAutoMoveState(bool isEnabled)
    {
        if (isEnabled && !isStuned)
        {
            if (!isAnimating && autoMoveCoroutine == null)
            {
                autoMoveCoroutine = StartCoroutine(AutoMove());
            }
        }
        else
        {
            if (autoMoveCoroutine != null)
            {
                StopCoroutine(autoMoveCoroutine);
                autoMoveCoroutine = null;
            }
            isAnimating = false;
        }
    }

    // 자동 이동 실행
    private IEnumerator AutoMove()
    {
        while (true)
        {
            yield return new WaitForSeconds(AutoMoveManager.Instance.AutoMoveTime());

            if (!isAnimating && (catDragAndDrop == null || !catDragAndDrop.isDragging))
            {
                Vector3 randomDirection = GetRandomDirection();
                Vector3 targetPosition = (Vector3)rectTransform.anchoredPosition + randomDirection;

                yield return StartCoroutine(SmoothMoveToPosition(targetPosition));

                if (IsOutOfBounds(targetPosition))
                {
                    Vector3 adjustedPosition = AdjustPositionToBounds(targetPosition);
                    StartCoroutine(SmoothMoveToPosition(adjustedPosition));
                }
            }
        }
    }

    // 랜덤 이동 방향 반환
    private Vector3 GetRandomDirection()
    {
        float moveRange = 30f;
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

        return directions[Random.Range(0, directions.Length)];
    }

    // 이동 범위 초과 확인
    private bool IsOutOfBounds(Vector3 position)
    {
        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        return position.x <= minBounds.x || position.x >= maxBounds.x || position.y <= minBounds.y || position.y >= maxBounds.y;
    }

    // 범위 초과시 위치 조정
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

    // 부드러운 이동 시작
    private IEnumerator SmoothMoveToPosition(Vector3 targetPosition)
    {
        isAnimating = true;

        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
        }

        currentMoveCoroutine = StartCoroutine(DoSmoothMove(targetPosition));
        yield return currentMoveCoroutine;
    }

    // 실제 이동 실행
    private IEnumerator DoSmoothMove(Vector3 targetPosition)
    {
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
        currentMoveCoroutine = null;
    }
    #endregion

    // ======================================================================================================================

    #region Auto Coin Collection
    // 자동 재화 수집 상태 설정
    public void SetCollectingCoinsState(bool isEnabled)
    {
        isCollectingCoins = isEnabled;

        if (isCollectingCoins)
        {
            if (autoCollectCoroutine == null)
            {
                autoCollectCoroutine = StartCoroutine(AutoCollectCoins());
            }
        }
        else
        {
            if (autoCollectCoroutine != null)
            {
                StopCoroutine(autoCollectCoroutine);
                autoCollectCoroutine = null;
            }
            DisableCollectUI();
        }
    }

    // 코인 수집 UI 비활성화
    public void DisableCollectUI()
    {
        if (collectCoinText != null)
        {
            collectCoinText.gameObject.SetActive(false);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(false);
        }
    }

    // 자동 재화 수집 실행
    private IEnumerator AutoCollectCoins()
    {
        while (isCollectingCoins)
        {
            collectingTime = ItemFunctionManager.Instance.reduceCollectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;
            yield return new WaitForSeconds(collectingTime);

            if (catData != null && GameManager.Instance != null)
            {
                int collectedCoins = catData.CatGetCoin;
                GameManager.Instance.Coin += collectedCoins;
                StartCoroutine(PlayCollectingAnimation(collectedCoins));
            }
        }
    }

    // 재화 수집 애니메이션 실행
    private IEnumerator PlayCollectingAnimation(int collectedCoins)
    {
        collectCoinText.text = $"+{collectedCoins}";
        collectCoinText.gameObject.SetActive(true);
        collectCoinImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);
    }
    #endregion

    // ======================================================================================================================

    #region Movement Control
    // 모든 이동 코루틴 중지
    public void StopAllMovement()
    {
        if (autoMoveCoroutine != null)
        {
            StopCoroutine(autoMoveCoroutine);
            autoMoveCoroutine = null;
        }

        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }

        isAnimating = false;
    }
    #endregion

    // ======================================================================================================================


}

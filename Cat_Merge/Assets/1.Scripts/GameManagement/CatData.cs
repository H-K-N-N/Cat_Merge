using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 고양이의 정보와 행동을 관리하는 스크립트
public class CatData : MonoBehaviour, ICanvasRaycastFilter
{


    #region Variables

    private const float CLICK_AREA_SCALE = 0.9f;    // 클릭 영역 스케일
    private WaitForSeconds COLLECT_ANIMATION_DELAY = new WaitForSeconds(1f); // 4월 5일 0.5->1로 바꿈

    [Header("Cat Data")]
    public Cat catData;                             // 고양이 기본 데이터
    private Image catImage;                         // 고양이 이미지 컴포넌트
    public double catHp;                            // 현재 고양이 체력
    public bool isStuned = false;                   // 기절 상태 여부
    private const float STUN_TIME = 10f;            // 기절 시간

    [Header("HP UI")]
    [SerializeField] private GameObject hpImage;    // HP 이미지 오브젝트
    [SerializeField] private Image hpFillImage;     // HP Fill 이미지

    [Header("Transform")]
    private RectTransform rectTransform;            // 현재 오브젝트의 RectTransform
    private RectTransform parentPanel;              // 부모 패널의 RectTransform
    private DragAndDropManager catDragAndDrop;      // 드래그 앤 드롭 매니저
    private Vector2 rectSize;                       // 레이캐스트를 위한 rectSize
    private float rectHalfWidth;                    // rectSize의 Width 절반값
    private float rectHalfHeight;                   // rectSize의 Height 절반값

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


    #region Unity Methods

    private void Awake()
    {
        InitializeComponents();
        CacheRectTransformData();
    }

    private void Start()
    {
        UpdateCatUI();
        autoCollectCoroutine = StartCoroutine(AutoCollectCoins());
        UpdateHPBar();
    }

    #endregion


    #region Initialization

    // 컴포넌트 초기화 함수
    private void InitializeComponents()
    {
        catImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
        catDragAndDrop = GetComponentInParent<DragAndDropManager>();

        Transform coinTextTransform = transform.Find("CollectCoinText");
        Transform coinImageTransform = transform.Find("CollectCoinImage");
        if (coinTextTransform != null) collectCoinText = coinTextTransform.GetComponent<TextMeshProUGUI>();
        if (coinImageTransform != null) collectCoinImage = coinImageTransform.GetComponent<Image>();

        hpImage = transform.Find("HP Image").gameObject;
        hpFillImage = hpImage.transform.Find("Fill Image").GetComponent<Image>();
        hpImage.SetActive(false);

        if (collectCoinText != null) collectCoinText.gameObject.SetActive(false);
        if (collectCoinImage != null) collectCoinImage.gameObject.SetActive(false);
    }

    // 레이캐스트를 위한 rectSize 설정 함수
    private void CacheRectTransformData()
    {
        rectSize = rectTransform.rect.size;
        rectHalfWidth = rectSize.x * 0.5f;
        rectHalfHeight = rectSize.y * 0.5f;
    }

    // UI 업데이트 함수
    public void UpdateCatUI()
    {
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }
    
    // 고양이 데이터 설정 함수
    public void SetCatData(Cat cat)
    {
        // 기존 코루틴 정리
        if (autoCollectCoroutine != null)
        {
            StopCoroutine(autoCollectCoroutine);
            autoCollectCoroutine = null;
        }

        // UI 상태 초기화
        if (collectCoinText != null)
        {
            collectCoinText.gameObject.SetActive(false);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(false);
        }

        // 데이터 설정
        catData = cat;
        catHp = catData.CatHp;

        // UI 업데이트
        UpdateCatUI();
        UpdateHPBar();
        GetComponent<AnimatorManager>().ApplyAnim(catData.CatGrade);

        // 재화 수집 상태 초기화 및 시작
        isCollectingCoins = true;
        if (!BattleManager.Instance.IsBattleActive)
        {
            autoCollectCoroutine = StartCoroutine(AutoCollectCoins());
        }
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


    #region Battle System

    // 보스 히트박스 경계로 고양이 밀어내는 함수
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

    // 보스 히트박스 경계로 고양이 이동시키는 함수
    public void MoveTowardBossBoundary()
    {
        Vector3 catPosition = rectTransform.anchoredPosition;
        BossHitbox bossHitbox = BattleManager.Instance.bossHitbox;

        // 고양이가 히트박스 외부에 있는 경우
        if (!bossHitbox.IsInHitbox(catPosition) && BattleManager.Instance.isBattleActive)
        {
            // 히트박스 경계에 위치하도록 이동
            Vector3 targetPosition = bossHitbox.GetClosestBoundaryPoint(catPosition);
            StartCoroutine(SmoothMoveToPosition(targetPosition));
        }
    }

    // 데미지 처리 함수
    public void TakeDamage(double damage)
    {
        catHp -= damage;
        UpdateHPBar();

        if (catHp <= 0)
        {
            StartCoroutine(StunAndRecover(STUN_TIME));
        }
    }

    // 기절 및 회복 처리 함수
    private IEnumerator StunAndRecover(float stunTime)
    {
        SetStunState(true);
        yield return new WaitForSeconds(stunTime);
        SetStunState(false);
    }

    // 기절 상태 설정 함수
    private void SetStunState(bool isStunned)
    {
        UpdateHPBar();
        isStuned = isStunned;
        GetComponent<AnimatorManager>().ChangeState(CharacterState.isFaint);

        if (!isStunned)
        {
            HealCatHP();

            // 전투 중인지 확인
            if (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive)
            {
                GetComponent<AnimatorManager>().ChangeState(CharacterState.isBattle);

                // 전투 중이면 자동 재화 수집과 자동 이동은 비활성화 상태 유지
                SetCollectingCoinsState(false);
                SetAutoMoveState(false);
                SetRaycastTarget(true);  // 드래그는 가능하도록 설정

                // 보스 히트박스 경계로 이동
                MoveTowardBossBoundary();               
            }
            else
            {
                GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);

                // 전투 중이 아니면 모든 기능 활성화
                SetCollectingCoinsState(true);
                SetAutoMoveState(true);
                SetRaycastTarget(true);
            }
        }
        else
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isFaint);

            // 기절 상태로 진입할 때는 모든 기능 비활성화
            SetCollectingCoinsState(false);
            SetAutoMoveState(false);
            SetRaycastTarget(false);
        }
    }

    // Raycast Target 설정 함수
    private void SetRaycastTarget(bool isActive)
    {
        catImage.raycastTarget = isActive;
    }

    // 체력 회복 함수
    public void HealCatHP()
    {
        if (isStuned)
        {
            StopCoroutine(StunAndRecover(STUN_TIME));
            SetStunState(false);
        }
        catHp = catData.CatHp;
        UpdateHPBar();
    }

    #endregion


    #region Auto Movement

    // 자동 이동 상태 설정 함수
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

    // 자동 이동 코루틴
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

    // 랜덤 이동 방향 반환 함수
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

    // 이동 범위 초과 확인 함수
    private bool IsOutOfBounds(Vector3 position)
    {
        Vector2 minBounds = (Vector2)parentPanel.rect.min + parentPanel.anchoredPosition;
        Vector2 maxBounds = (Vector2)parentPanel.rect.max + parentPanel.anchoredPosition;

        return position.x <= minBounds.x || position.x >= maxBounds.x || position.y <= minBounds.y || position.y >= maxBounds.y;
    }

    // 범위 초과시 위치 조정 함수
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

    // 부드러운 이동 코루틴
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

    // 실제 이동 코루틴
    private IEnumerator DoSmoothMove(Vector3 targetPosition)
    {
        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        if (!GetComponent<DragAndDropManager>().isDragging)
        {
            if (BattleManager.Instance.isBattleActive)
            {
                GetComponent<AnimatorManager>().ChangeState(CharacterState.isWalk);
            }
            else
            {
                GetComponent<AnimatorManager>().ChangeState(CharacterState.isWalk);
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        isAnimating = false;

        if (!GetComponent<DragAndDropManager>().isDragging)
        {
            if (BattleManager.Instance.isBattleActive)
            {
                GetComponent<AnimatorManager>().ChangeState(CharacterState.isBattle);
            }
            else
            {
                GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);
            }
        }

        currentMoveCoroutine = null;
    }

    #endregion


    #region Auto Coin Collection

    // 자동 재화 수집 상태 설정 함수
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

    // 코인 수집 UI 비활성화 함수
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

    // 자동 재화 수집 코루틴
    private IEnumerator AutoCollectCoins()
    {
        if (autoCollectCoroutine != null) yield break;

        float currentDelayTime = 0f;
        WaitForSeconds delay = null;

        while (isCollectingCoins && gameObject.activeSelf)
        {
            if (!isCollectingCoins || !gameObject.activeSelf) break;

            collectingTime = ItemFunctionManager.Instance.reduceCollectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;
            //Debug.Log(collectingTime);

            // 딜레이 시간이 변경되었는지 확인
            if (delay == null || !Mathf.Approximately(currentDelayTime, collectingTime))
            {
                currentDelayTime = collectingTime;
                delay = new WaitForSeconds(currentDelayTime);
            }

            yield return delay;

            if (!isCollectingCoins || !gameObject.activeSelf) break;

            if (catData != null)
            {
                float currentMultiplier = ShopManager.Instance.CurrentCoinMultiplier;
                int collectedCoins = Mathf.RoundToInt(catData.CatGetCoin * currentMultiplier);

                GameManager.Instance.Coin += collectedCoins;
                StartCoroutine(PlayCollectingAnimation(collectedCoins));
            }
        }

        autoCollectCoroutine = null;
    }

    // 재화 수집 애니메이션 코루틴
    private IEnumerator PlayCollectingAnimation(int collectedCoins)
    {
        if (!BattleManager.Instance.isBattleActive && !GetComponent<DragAndDropManager>().isDragging)
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isGetCoin);
        }
        
        if (collectCoinText != null)
        {
            collectCoinText.text = $"+{collectedCoins}";
            collectCoinText.gameObject.SetActive(true);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(true);
        }

        yield return COLLECT_ANIMATION_DELAY;

        if (collectCoinText != null)
        {
            collectCoinText.gameObject.SetActive(false);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(false);
        }

        if (!BattleManager.Instance.isBattleActive && !GetComponent<DragAndDropManager>().isDragging)
        {
            GetComponent<AnimatorManager>().ChangeState(CharacterState.isIdle);
        }
    }
    
    #endregion


    #region Movement Control

    // 모든 이동 코루틴 중지 함수
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


    #region Raycast

    // 레이캐스트 필터링 함수
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint)) return false;


        Vector2 normalizedPoint = new Vector2(
            //localPoint.x / rectHalfWidth,
            (localPoint.x + 20f) / rectHalfWidth,
            localPoint.y / rectHalfHeight
        );

        return (normalizedPoint.x * normalizedPoint.x + normalizedPoint.y * normalizedPoint.y) <= (CLICK_AREA_SCALE * CLICK_AREA_SCALE);
    }

    #endregion


}
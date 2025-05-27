using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 고양이의 정보와 행동을 관리하는 스크립트
public class CatData : MonoBehaviour, ICanvasRaycastFilter
{


    #region Variables

    private const float CLICK_AREA_SCALE = 0.9f;    // 클릭 영역 스케일

    private static readonly WaitForSeconds COLLECT_ANIMATION_DELAY = new WaitForSeconds(1f);
    private static readonly WaitForSeconds STUN_DELAY = new WaitForSeconds(10f);
    private static readonly WaitForSeconds ATTACK_RETURN_DELAY = new WaitForSeconds(1f);

    // 고양이 자동이동 방향
    private static readonly Vector2[] MOVE_DIRECTIONS = new Vector2[]
    {
        new Vector2(30f, 0f),
        new Vector2(-30f, 0f),
        new Vector2(0f, 30f),
        new Vector2(0f, -30f),
        new Vector2(30f, 30f),
        new Vector2(30f, -30f),
        new Vector2(-30f, 30f),
        new Vector2(-30f, -30f)
    };

    [Header("Cat Data")]
    public Cat catData;                             // 고양이 기본 데이터
    private Image catImage;                         // 고양이 이미지 컴포넌트
    public double catHp;                            // 현재 고양이 체력
    public bool isStuned = false;                   // 기절 상태 여부

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

    [Header("Cat Image")]
    private GameObject catImageObject;              // 고양이 이미지 오브젝트
    private Image catSpriteImage;                   // 고양이 스프라이트 이미지

    [Header("Movement")]
    private bool isMoveAnimating = false;               // 이동 애니메이션 진행 여부
    private Coroutine autoMoveCoroutine;                // 자동 이동 코루틴
    private Coroutine currentMoveCoroutine;             // 현재 진행 중인 이동 코루틴
    private const float CAT_MOVE_SPEED = 100f;          // 기본 고양이 이동 속도
    private const float CAT_BATTLE_MOVE_SPEED = 400f;   // 전투 중 고양이 이동 속도

    [Header("Coin Collection")]
    private TextMeshProUGUI collectCoinText;        // 코인 획득 텍스트
    private Image collectCoinImage;                 // 코인 획득 이미지
    private bool isCollectingCoins = true;          // 코인 수집 활성화 상태
    private Coroutine autoCollectCoroutine;         // 코인 수집 코루틴

    [Header("Battle")]
    private Coroutine attackCoroutine;              // 공격 코루틴
    private float nextAttackTime = 0f;              // 다음 공격 시간

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
        StartCoroutine(SetupCatImage());
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

        catImageObject = transform.Find("Cat Image")?.gameObject;
        if (catImageObject != null)
        {
            catSpriteImage = catImageObject.GetComponent<Image>();

            // 중요: Cat Image의 Image 컴포넌트의 raycastTarget을 false로 설정
            // 이렇게 하면 원본 오브젝트만 레이캐스트에 반응하고 회전된 Cat Image는 무시됨
            if (catSpriteImage != null)
            {
                catSpriteImage.raycastTarget = false;
            }
        }

        Transform coinImageTransform = transform.Find("CollectCoinImage");
        Transform coinTextTransform = coinImageTransform.Find("CollectCoinText");

        if (coinImageTransform != null) collectCoinImage = coinImageTransform.GetComponent<Image>();
        if (coinTextTransform != null) collectCoinText = coinTextTransform.GetComponent<TextMeshProUGUI>();

        hpImage = transform.Find("HP Image").gameObject;
        hpFillImage = hpImage.transform.Find("Fill Image").GetComponent<Image>();
        hpImage.SetActive(false);

        if (collectCoinImage != null) collectCoinImage.gameObject.SetActive(false);
        if (collectCoinText != null) collectCoinText.gameObject.SetActive(false);
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

        // 원본 이미지 업데이트 (레이캐스트 영역용)
        if (catImage != null)
        {
            catImage.sprite = catData.CatImage;
            Color imageColor = catImage.color;
            imageColor.a = 0f;
            catImage.color = imageColor;
        }

        // Cat Image 스프라이트 업데이트
        if (catSpriteImage != null)
        {
            catSpriteImage.sprite = catData.CatImage;
        }

        // Cat Image의 AnimatorManager 가져오기
        AnimatorManager catImageAnimManager = GetCatImageAnimator();
        if (catImageAnimManager != null)
        {
            catImageAnimManager.ApplyAnim(catData.CatGrade);
        }
    }

    // 고양이 이미지 설정 코루틴
    private IEnumerator SetupCatImage()
    {
        // 한 프레임 기다려서 모든 컴포넌트가 초기화되도록 함
        yield return null;

        // 원본 이미지 비활성화 (투명하게 만들되 레이캐스트는 가능하도록)
        catImage.enabled = true;
        Color imageColor = catImage.color;
        imageColor.a = 0f;
        catImage.color = imageColor;
        catImage.sprite = catData.CatImage;

        // 원본 애니메이터 참조
        Animator originalAnimator = GetComponent<Animator>();
        AnimatorManager originalAnimManager = GetComponent<AnimatorManager>();

        if (catImageObject != null)
        {
            // 이미 애니메이터가 있는지 확인
            Animator catImageAnimator = catImageObject.GetComponent<Animator>();
            if (catImageAnimator == null && originalAnimator != null)
            {
                // Cat Image에 애니메이터 추가
                catImageAnimator = catImageObject.AddComponent<Animator>();
                catImageAnimator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;

                // 원본 애니메이터 비활성화
                originalAnimator.enabled = false;
            }

            // 이미 AnimatorManager가 있는지 확인
            AnimatorManager catImageAnimManager = catImageObject.GetComponent<AnimatorManager>();
            if (catImageAnimManager == null && originalAnimManager != null)
            {
                // Cat Image에 AnimatorManager 추가
                catImageAnimManager = catImageObject.AddComponent<AnimatorManager>();

                // AnimatorManager 데이터 복사 (다음 프레임에 수행)
                yield return null;

                // 원본 AnimatorManager에서 데이터 참조할 수 있는지 확인
                if (originalAnimManager.overrideDataList != null)
                {
                    catImageAnimManager.catGrade = originalAnimManager.catGrade;
                    catImageAnimManager.overrideDataList = originalAnimManager.overrideDataList;

                    // 복사 후 원본 비활성화
                    originalAnimManager.enabled = false;
                }
            }

            // 고양이 이미지 업데이트
            if (catData != null)
            {
                catSpriteImage.sprite = catData.CatImage;

                // AnimatorManager 등급 설정
                AnimatorManager animManager = catImageObject.GetComponent<AnimatorManager>();
                if (animManager != null)
                {
                    animManager.ApplyAnim(catData.CatGrade);
                }
            }
        }
    }

    // Cat Image의 AnimatorManager를 가져오는 함수
    private AnimatorManager GetCatImageAnimator()
    {
        // catImageObject가 있으면 사용
        if (catImageObject != null)
        {
            AnimatorManager anim = catImageObject.GetComponent<AnimatorManager>();
            if (anim != null)
            {
                return anim;
            }
        }

        // Cat Image 찾기
        Transform catImageTransform = transform.Find("Cat Image");
        if (catImageTransform != null)
        {
            AnimatorManager anim = catImageTransform.GetComponent<AnimatorManager>();
            if (anim != null)
            {
                // 나중을 위해 참조 저장
                catImageObject = catImageTransform.gameObject;
                return anim;
            }
        }

        // 부득이한 경우 현재 게임오브젝트의 AnimatorManager 사용
        return GetComponent<AnimatorManager>();
    }

    // 고양이 상태 변경을 위한 함수
    public void ChangeCatState(CatState state)
    {
        AnimatorManager anim = GetCatImageAnimator();
        if (anim != null)
        {
            anim.ChangeState(state);
        }
    }

    // 드래그 상태 변경을 위한 함수
    public void SetDragState(bool isDragging)
    {
        if (isDragging)
        {
            ChangeCatState(CatState.isGrab);
        }
        else
        {
            // 드래그 종료 시 상태 복원
            if (BattleManager.Instance.isBattleActive)
            {
                ChangeCatState(CatState.isBattle);
            }
            else
            {
                ChangeCatState(CatState.isIdle);
            }
        }
    }

    // 고양이 데이터 설정 함수
    public void SetCatData(Cat cat)
    {
        // 기존 코루틴 정리
        CleanupCoroutines();

        // UI 상태 초기화
        collectCoinText.gameObject.SetActive(false);
        collectCoinImage.gameObject.SetActive(false);

        // 데이터 설정
        catData = cat;
        catHp = catData.CatHp;

        // UI 업데이트
        UpdateCatUI();
        UpdateHPBar();

        // 애니메이션 업데이트
        AnimatorManager anim = GetCatImageAnimator();
        if (anim != null)
        {
            anim.ApplyAnim(catData.CatGrade);
        }

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
        else if (bossHitbox.IsInHitbox(catPosition) && BattleManager.Instance.isBattleActive)
        {
            // 히트박스에 도달하면 전투 상태로 전환
            SetBattleState(true);
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
            StartCoroutine(StunAndRecover());
        }
    }

    // 기절 및 회복 처리 함수
    private IEnumerator StunAndRecover()
    {
        SetStunState(true);
        yield return STUN_DELAY;
        SetStunState(false);
    }

    // 기절 상태 설정 함수
    private void SetStunState(bool isStunned)
    {
        UpdateHPBar();
        isStuned = isStunned;

        if (!isStunned)
        {
            // 기절에서 회복될 때
            if (BattleManager.Instance.IsBattleActive)
            {
                // 전투 중이면 전투 상태로
                ChangeCatState(CatState.isBattle);
            }
            else
            {
                // 전투 중이 아니면 기본 상태로
                ChangeCatState(CatState.isIdle);
                SetCollectingCoinsState(true);
                SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
            }
            // 어떤 상태든 레이캐스트는 활성화
            SetRaycastTarget(true);
        }
        else
        {
            // 기절 상태로 진입할 때
            ChangeCatState(CatState.isFaint);
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
            StopCoroutine(StunAndRecover());
            SetStunState(false);
        }
        catHp = catData.CatHp;
        UpdateHPBar();
    }

    // 전투 상태 설정 함수
    private void SetBattleState(bool isInBattle)
    {
        if (isInBattle && !isStuned)
        {
            ChangeCatState(CatState.isBattle);
            StartAttacking();
        }
        else
        {
            ChangeCatState(CatState.isIdle);
            StopAttacking();
        }
    }

    // 공격 시작 함수
    private void StartAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    // 공격 중지 함수
    private void StopAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    // 공격 코루틴
    private IEnumerator AttackRoutine()
    {
        // 히트박스에 도달하자마자 즉시 첫 공격 실행
        if (!isStuned)
        {
            PerformAttack();
        }

        // 첫 공격 후 다음 공격 시간 설정
        float baseAttackSpeed = catData.CatAttackSpeed;
        float actualAttackSpeed = baseAttackSpeed - catData.PassiveAttackSpeed;
        nextAttackTime = Time.time + actualAttackSpeed;

        while (BattleManager.Instance.IsBattleActive)
        {
            if (Time.time >= nextAttackTime && !isStuned)
            {
                RectTransform catRectTransform = GetComponent<RectTransform>();
                Vector3 catPosition = catRectTransform.anchoredPosition;
                DragAndDropManager dragManager = GetComponent<DragAndDropManager>();

                // 드래그 중이 아니고 히트박스 경계에 있을 때만 공격
                if (!dragManager.isDragging && BattleManager.Instance.bossHitbox.IsAtBoundary(catPosition))
                {
                    PerformAttack();

                    // 다음 공격 시간 설정
                    actualAttackSpeed = baseAttackSpeed - catData.PassiveAttackSpeed;
                    nextAttackTime = Time.time + actualAttackSpeed;
                }
            }
            yield return null;
        }
    }

    // 공격 실행 함수
    private void PerformAttack()
    {
        // 공격 실행
        BattleManager.Instance.TakeBossDamage(catData.CatDamage);

        // 공격 애니메이션으로 변경
        ChangeCatState(CatState.isAttack);

        // 공격 후 전투 대기 상태로 돌아가는 코루틴 시작
        StartCoroutine(ReturnToBattleState());
    }

    // 공격 후 전투 대기 상태로 돌아가는 코루틴
    private IEnumerator ReturnToBattleState()
    {
        yield return ATTACK_RETURN_DELAY;

        if (BattleManager.Instance.IsBattleActive && gameObject.activeSelf && !isStuned)
        {
            DragAndDropManager dragManager = GetComponent<DragAndDropManager>();
            if (!dragManager.isDragging)
            {
                ChangeCatState(CatState.isBattle);
            }
        }
    }

    #endregion


    #region Auto Movement & Move System

    // 자동 이동 상태 설정 함수
    public void SetAutoMoveState(bool isEnabled)
    {
        if (isEnabled && !isStuned)
        {
            if (!isMoveAnimating && autoMoveCoroutine == null)
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
            isMoveAnimating = false;
        }
    }

    // 자동 이동 코루틴
    private IEnumerator AutoMove()
    {
        while (true)
        {
            // 기본 이동 시간에 0.8~1.2 사이의 랜덤 계수를 곱함
            float randomMultiplier = UnityEngine.Random.Range(0.8f, 1.2f);
            float moveTime = AutoMoveManager.Instance.AutoMoveTime() * randomMultiplier;

            yield return new WaitForSeconds(moveTime);

            if (!isMoveAnimating && (catDragAndDrop == null || !catDragAndDrop.isDragging))
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
        return MOVE_DIRECTIONS[Random.Range(0, MOVE_DIRECTIONS.Length)];
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
        isMoveAnimating = true;

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
        float distance = Vector3.Distance(startPosition, targetPosition);

        // 이동 속도에 따른 이동 시간 계산
        float moveSpeed = CAT_MOVE_SPEED;

        // 전투 중일 때만 거리 기반 이동 시간 계산
        if (BattleManager.Instance.isBattleActive)
        {
            moveSpeed = CAT_BATTLE_MOVE_SPEED;
        }

        float duration = Mathf.Clamp(distance / moveSpeed, 0.1f, 5f);

        // 이동할 때는 무조건 walk 상태로 변경 (최우선)
        if (!GetComponent<DragAndDropManager>().isDragging)
        {
            ChangeCatState(CatState.isWalk);
        }

        // 고양이의 이동 방향에 따라 이미지 좌우반전
        Vector3 moveDirection = targetPosition - startPosition;
        if (moveDirection.x != 0)
        {
            bool isMovingRight = moveDirection.x > 0;

            // 이미지 오브젝트만 뒤집기 (자식 UI 요소들에 영향 없음)
            if (catImageObject != null)
            {
                if (isMovingRight)
                {
                    catImageObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    catImageObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }

        bool wasMoving = false;
        bool wasBattleActive = BattleManager.Instance.isBattleActive;
        while (elapsed < duration)
        {
            wasMoving = true;

            // 드래그 중이거나 전투 상태가 변경되면 이동 중단
            if (GetComponent<DragAndDropManager>().isDragging || wasBattleActive != BattleManager.Instance.isBattleActive)
            {
                break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, t);

            // 이동 중에는 계속 walk 상태 유지
            if (!GetComponent<DragAndDropManager>().isDragging)
            {
                ChangeCatState(CatState.isWalk);
            }
            yield return null;
        }

        // 이동이 완료되고 드래그 중이 아닐 때만 최종 위치 설정
        if (wasMoving && !GetComponent<DragAndDropManager>().isDragging && wasBattleActive == BattleManager.Instance.isBattleActive)
        {
            rectTransform.anchoredPosition = targetPosition;
        }

        isMoveAnimating = false;

        if (!GetComponent<DragAndDropManager>().isDragging)
        {
            // 전투가 아직 진행중이고 히트박스 경계에 도착했을 때 전투 상태로 변경
            if (BattleManager.Instance.isBattleActive && BattleManager.Instance.bossHitbox != null &&
                BattleManager.Instance.bossHitbox.IsAtBoundary(rectTransform.anchoredPosition))
            {
                SetBattleState(true);
            }
            else
            {
                SetBattleState(false);
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

        DragAndDropManager dragAndDropManager = GetComponent<DragAndDropManager>();
        Dictionary<float, WaitForSeconds> delayCache = new Dictionary<float, WaitForSeconds>();
        WaitForSeconds defaultDelay = new WaitForSeconds(3f);

        while (isCollectingCoins && gameObject.activeSelf)
        {
            if (!isCollectingCoins || !gameObject.activeSelf) break;

            // 수집 시간 업데이트
            float baseCollectingTime = ItemFunctionManager.Instance.reduceCollectingTimeList[ItemMenuManager.Instance.ReduceCollectingTimeLv].value;
            float newCollectingTime = (baseCollectingTime - catData.PassiveCoinCollectSpeed) * UnityEngine.Random.Range(0.8f, 1.2f);

            WaitForSeconds delay;
            if (!delayCache.TryGetValue(newCollectingTime, out delay))
            {
                delay = new WaitForSeconds(newCollectingTime);
                delayCache[newCollectingTime] = delay;
            }

            if (delay != null)
            {
                yield return delay;
            }
            else
            {
                yield return defaultDelay;
            }

            if (!isCollectingCoins || !gameObject.activeSelf) break;

            if (catData != null)
            {
                float currentMultiplier = ShopManager.Instance.CurrentCoinMultiplier;

                int baseCoins = catData.CatGetCoin;
                int multipliedCoins = Mathf.RoundToInt(baseCoins * currentMultiplier);

                GameManager.Instance.Coin += multipliedCoins;

                // 전투 중이거나 드래그 중이 아닐 때만 애니메이션 실행
                if (!BattleManager.Instance.IsBattleActive && !dragAndDropManager.isDragging)
                {
                    StartCoroutine(PlayCollectingAnimation(multipliedCoins));
                }
                else
                {
                    UpdateCollectUI(multipliedCoins);
                }
            }
        }

        autoCollectCoroutine = null;
    }

    // UI 업데이트 전용 함수 (애니메이션 없이)
    private void UpdateCollectUI(int coins)
    {
        if (collectCoinText != null)
        {
            //collectCoinText.text = $"+{coins}";
            collectCoinText.text = $"+{GameManager.Instance.FormatNumber(coins)}";
            collectCoinText.gameObject.SetActive(true);
        }
        if (collectCoinImage != null)
        {
            collectCoinImage.gameObject.SetActive(true);
        }

        StartCoroutine(DisableCollectUIDelayed());
    }

    // UI 비활성화 딜레이 코루틴
    private IEnumerator DisableCollectUIDelayed()
    {
        yield return COLLECT_ANIMATION_DELAY;
        DisableCollectUI();
    }

    // 재화 수집 애니메이션 코루틴
    private IEnumerator PlayCollectingAnimation(int collectedCoins)
    {
        // 이동 중이거나 드래그 중이면 애니메이션 상태를 변경하지 않음
        if (!BattleManager.Instance.isBattleActive && !GetComponent<DragAndDropManager>().isDragging && !isMoveAnimating)
        {
            ChangeCatState(CatState.isGetCoin);
        }

        if (collectCoinText != null)
        {
            //collectCoinText.text = $"+{collectedCoins}";
            collectCoinText.text = $"+{GameManager.Instance.FormatNumber(collectedCoins)}";
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

        // 이동 중이거나 드래그 중이면 애니메이션 상태를 변경하지 않음
        if (!BattleManager.Instance.isBattleActive && !GetComponent<DragAndDropManager>().isDragging && !isMoveAnimating)
        {
            ChangeCatState(CatState.isIdle);
        }
    }

    // 모든 코루틴 정리 함수
    public void CleanupCoroutines()
    {
        StopAllCoroutines();
        autoCollectCoroutine = null;
        isCollectingCoins = false;
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

        isMoveAnimating = false;
    }

    #endregion


    #region Raycast

    // 레이캐스트 필터링 함수
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint)) return false;

        // 항상 원형 영역으로 클릭 가능하도록 설정
        Vector2 normalizedPoint = new Vector2(
            localPoint.x / rectHalfWidth,
            localPoint.y / rectHalfHeight
        );

        // 원형 영역 체크 (회전에 영향받지 않음)
        float distance = normalizedPoint.magnitude;
        return distance <= CLICK_AREA_SCALE;
    }

    #endregion


}

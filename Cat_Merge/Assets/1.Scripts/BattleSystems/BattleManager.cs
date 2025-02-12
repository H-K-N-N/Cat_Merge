using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// BattleManager Script
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ======================================================================================================================

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // 보스 프리팹
    [SerializeField] private Transform bossUIParent;            // 보스를 배치할 부모 Transform (UI Panel 등)
    [SerializeField] private Slider respawnSlider;              // 보스 소환까지 남은 시간을 표시할 Slider UI

    private float spawnInterval = 10f;                          // 보스 등장 주기 (나중에 유저가 변경할 수 있게 수정할 계획도 있음)
    private Coroutine respawnSliderCoroutine;                   // Slider 코루틴
    private float bossSpawnTimer = 0f;                          // 보스 스폰 타이머
    private float sliderDuration;                               // Slider 유지 시간
    private float bossDuration;                                 // 보스 유지 시간 (sliderDuration + warningDuration)
    private int bossStage = 1;                                  // 보스 스테이지
    public int BossStage { get => bossStage; }

    private float bossAttackDelay = 2f;                         // 보스 공격 딜레이
    private float catAttackDelay = 1f;                          // 고양이 공격 딜레이

    private GameObject currentBoss = null;                      // 현재 보스
    [HideInInspector] public BossHitbox bossHitbox;             // 보스 히트박스
    private bool isBattleActive = false;                        // 전투 활성화 여부
    public bool IsBattleActive { get => isBattleActive; }

    // ======================================================================================================================

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (활성화/비활성화 제어)
    [SerializeField] private TextMeshProUGUI bossNameText;      // Boss Name Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text
    [SerializeField] private Button giveupButton;               // 항복 버튼

    private Mouse currentBossData = null;                       // 현재 보스 데이터
    private float currentBossHP;                                // 보스의 현재 HP
    private float maxBossHP;                                    // 보스의 최대 HP

    // ======================================================================================================================

    [Header("---[Warning Panel]")]
    [SerializeField] private GameObject warningPanel;           // 전투시스템 시작시 나오는 경고 Panel (warningDuration동안 지속)
    [SerializeField] private Slider warningSlider;              // 리스폰시간이 됐을때 차오르는 Slider (warningDuration만큼 차오름)
    public float warningDuration = 2f;                          // warningPanel 활성화 시간
    private Coroutine warningSliderCoroutine;                   // warningSlider 코루틴

    [SerializeField] private TextMeshProUGUI topWarningText;    // 상단 경고 텍스트
    [SerializeField] private TextMeshProUGUI bossDangerText;    // 보스 위험 텍스트
    [SerializeField] private TextMeshProUGUI bottomWarningText; // 하단 경고 텍스트
    private CanvasGroup warningPanelCanvasGroup;                // Warning Panel의 CanvasGroup
    private CanvasGroup topWarningCanvasGroup;                  // 상단 경고 텍스트의 CanvasGroup
    private CanvasGroup bossDangerCanvasGroup;                  // 보스 위험 텍스트의 CanvasGroup  
    private CanvasGroup bottomWarningCanvasGroup;               // 하단 경고 텍스트의 CanvasGroup

    // ======================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeBattleManager();
        giveupButton.onClick.AddListener(GiveUpState);
    }

    private void Start()
    {
        StartCoroutine(BossSpawnRoutine());
    }

    // BattleManager 초기 설정
    private void InitializeBattleManager()
    {
        // warningPanel 설정
        warningPanel.SetActive(false);

        // Slider 시간 설정 (보스 리스폰, 보스 전투 시간)
        spawnInterval = spawnInterval - warningDuration;
        sliderDuration = spawnInterval;
        bossDuration = sliderDuration + warningDuration;

        // respawnSlider 설정
        if (respawnSlider != null)
        {
            respawnSliderCoroutine = null;
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        // warning Slider 설정
        if (warningSlider != null)
        {
            warningSliderCoroutine = null;
            warningSlider.maxValue = warningDuration;
            warningSlider.value = 0f;
        }

        // Battle HP UI 초기화
        if (battleHPUI != null)
        {
            battleHPUI.SetActive(false);
        }

        InitializeCanvasGroup();
    }

    // CanvasGroup 컴포넌트 초기화
    private void InitializeCanvasGroup()
    {
        warningPanelCanvasGroup = warningPanel.GetComponent<CanvasGroup>();
        if (warningPanelCanvasGroup == null)
            warningPanelCanvasGroup = warningPanel.AddComponent<CanvasGroup>();

        topWarningCanvasGroup = topWarningText.gameObject.GetComponent<CanvasGroup>();
        if (topWarningCanvasGroup == null)
            topWarningCanvasGroup = topWarningText.gameObject.AddComponent<CanvasGroup>();

        bossDangerCanvasGroup = bossDangerText.gameObject.GetComponent<CanvasGroup>();
        if (bossDangerCanvasGroup == null)
            bossDangerCanvasGroup = bossDangerText.gameObject.AddComponent<CanvasGroup>();

        bottomWarningCanvasGroup = bottomWarningText.gameObject.GetComponent<CanvasGroup>();
        if (bottomWarningCanvasGroup == null)
            bottomWarningCanvasGroup = bottomWarningText.gameObject.AddComponent<CanvasGroup>();
    }

    // ======================================================================================================================
    // [BattleManager 핵심 기능]

    // 보스 스폰 코루틴
    private IEnumerator BossSpawnRoutine()
    {
        while (true)
        {
            // 보스가 없을 때만 게이지를 충전
            if (currentBoss == null)
            {
                bossSpawnTimer += Time.deltaTime;
                respawnSlider.value = bossSpawnTimer;

                // 게이지가 꽉 차면 보스 소환
                if (bossSpawnTimer >= spawnInterval)
                {
                    bossSpawnTimer = 0f;

                    yield return StartCoroutine(LoadWarningPanel());

                    LoadAndDisplayBoss();
                    StartBattle();
                }
            }

            yield return null;
        }
    }

    // warningPanel 활성화시 코루틴
    private IEnumerator LoadWarningPanel()
    {
        // 자동 머지 일시정지
        AutoMergeManager.Instance.PauseAutoMerge();

        warningPanel.SetActive(true);
        float elapsedTime = 0f;
        float halfDuration = warningDuration / 2f;

        // 초기 투명도 설정
        warningPanelCanvasGroup.alpha = 0f;
        topWarningCanvasGroup.alpha = 0f;
        bossDangerCanvasGroup.alpha = 0f;
        bottomWarningCanvasGroup.alpha = 0f;

        // Text 초기 위치 설정
        topWarningText.rectTransform.anchoredPosition = new Vector2(-755, 0);
        bossDangerText.rectTransform.anchoredPosition = new Vector2(810, 0);
        bottomWarningText.rectTransform.anchoredPosition = new Vector2(-755, 0);

        // 첫 1초: 투명 -> 반투명
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / halfDuration;

            // 투명도 조절 (0 -> 1.0)
            float alpha = Mathf.Lerp(0f, 1.0f, normalizedTime);
            warningPanelCanvasGroup.alpha = alpha;
            topWarningCanvasGroup.alpha = alpha;
            bossDangerCanvasGroup.alpha = alpha;
            bottomWarningCanvasGroup.alpha = alpha;

            // 텍스트 이동
            float moveProgress = elapsedTime / warningDuration;
            topWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bottomWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bossDangerText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(810, 0), new Vector2(-810, 0), moveProgress);

            // slider 업데이트
            warningSlider.value = elapsedTime;

            yield return null;
        }
        // 다음 1초: 반투명 -> 투명
        while (elapsedTime < warningDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = (elapsedTime - halfDuration) / halfDuration;

            // 투명도 조절 (1.0 -> 0)
            float alpha = Mathf.Lerp(1.0f, 0f, normalizedTime);
            warningPanelCanvasGroup.alpha = alpha;
            topWarningCanvasGroup.alpha = alpha;
            bossDangerCanvasGroup.alpha = alpha;
            bottomWarningCanvasGroup.alpha = alpha;

            // 텍스트 이동
            float moveProgress = elapsedTime / warningDuration;
            topWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bottomWarningText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-755, 0), new Vector2(755, 0), moveProgress);
            bossDangerText.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(810, 0), new Vector2(-810, 0), moveProgress);

            // slider 업데이트
            warningSlider.value = elapsedTime;

            yield return null;
        }

        warningPanel.SetActive(false);
    }



    // 보스 스폰 함수
    private void LoadAndDisplayBoss()
    {
        // bossStage에 맞는 Mouse를 가져와서 보스를 설정
        currentBossData = GetBossData();
        currentBoss = Instantiate(bossPrefab, bossUIParent);
        bossHitbox = currentBoss.GetComponent<BossHitbox>();

        // 보스의 MouseData를 설정
        MouseData mouseUIData = currentBoss.GetComponent<MouseData>();
        mouseUIData.SetMouseData(currentBossData);

        // 보스 위치 설정
        RectTransform bossRectTransform = currentBoss.GetComponent<RectTransform>();
        bossRectTransform.anchoredPosition = new Vector2(0f, 250f);

        UpdateBossUI();
    }

    // 해당 스테이지와 동일한 등급을 갖는 보스 데이터 불러오는 함수 (MouseGrade)
    private Mouse GetBossData()
    {
        // 모든 Mouse 데이터를 가져와서 bossStage에 맞는 MouseGrade를 찾음
        foreach (Mouse mouse in GameManager.Instance.AllMouseData)
        {
            if (mouse.MouseGrade == bossStage)
            {
                return mouse;
            }
        }

        return null;
    }

    // 전투 시작할때마다 Boss UI Panel 설정 함수
    private void UpdateBossUI()
    {
        battleHPUI.SetActive(true);
        bossNameText.text = currentBossData.MouseName;

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = maxBossHP;
        bossHPSlider.value = currentBossHP;
        bossHPText.text = $"{100f}%";
    }

    

    // 전투 시작 함수
    private void StartBattle()
    {
        // 전투 시작시 여러 외부 기능들 비활성화
        SetStartFunctions();

        // Slider관련 코루틴 시작
        StartCoroutine(ExecuteBattleSliders(warningDuration, sliderDuration));
        StartCoroutine(BossBattleRoutine(bossDuration));
        isBattleActive = true;

        // 고양이 자동 재화 수집 비활성화
        SetStartBattleAutoCollectState();

        // 보스 히트박스 내,외에 존재하는 고양이들 이동시키기
        PushCatsAwayFromBoss();
        MoveCatsTowardBossBoundary();

        // 보스 및 고양이 공격 코루틴 시작
        StartCoroutine(BossAttackRoutine());
        StartCoroutine(CatsAttackRoutine());
    }




    // Slider 감소 관리 코루틴
    private IEnumerator ExecuteBattleSliders(float warningDuration, float sliderDuration)
    {
        // 1. warningSlider 감소
        warningSliderCoroutine = StartCoroutine(DecreaseWarningSliderDuringBossDuration(warningDuration));
        yield return warningSliderCoroutine;

        // 2. respawnSlider 감소
        respawnSliderCoroutine = StartCoroutine(DecreaseSliderDuringBossDuration(sliderDuration));
        yield return respawnSliderCoroutine;
    }

    // 보스 유지시간동안 warningSlider가 감소하는 코루틴
    private IEnumerator DecreaseWarningSliderDuringBossDuration(float duration)
    {
        float elapsedTime = 0f;
        warningSlider.maxValue = duration;
        warningSlider.value = duration;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            warningSlider.value = duration - elapsedTime;

            yield return null;
        }

        warningSliderCoroutine = null;
    }

    // 보스 유지시간동안 respawnSlider가 감소하는 코루틴
    private IEnumerator DecreaseSliderDuringBossDuration(float duration)
    {
        float elapsedTime = 0f;
        respawnSlider.maxValue = duration;
        respawnSlider.value = duration;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            respawnSlider.value = duration - elapsedTime;

            yield return null;
        }

        respawnSliderCoroutine = null;
    }



    // 보스 배틀 코루틴
    private IEnumerator BossBattleRoutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 보스 체력이 0 이하가 되면 즉시 전투 종료
            if (currentBossHP <= 0)
            {
                EndBattle(true);
                yield break;
            }

            yield return null;
        }

        // 제한 시간 초과로 전투 종료
        EndBattle(false);
    }

    // 보스 스폰시 히트박스 범위 내에 있는 고양이를 밀어내는 함수
    private void PushCatsAwayFromBoss()
    {
        if (currentBoss == null)
        {
            Debug.LogError("No boss to push cats away from.");
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // 고양이가 히트박스 경계 내에 있는지 확인
            if (bossHitbox.IsInHitbox(catPosition))
            {
                // 고양이를 보스의 히트박스 외곽으로 밀어내기
                cat.MoveOppositeBoss(bossHitbox.Position, bossHitbox.Size);
            }
        }
    }

    // 보스 스폰시 히트박스 범위 밖에 있는 고양이를 히트박스 경계로 이동시키는 함수
    private void MoveCatsTowardBossBoundary()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            Debug.LogError("No boss or BossHitbox component is missing.");
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // 고양이가 히트박스 경계 밖에 있는지 확인
            if (!bossHitbox.IsInHitbox(catPosition))
            {
                // 고양이를 보스 히트박스 외곽으로 모으기
                cat.MoveTowardBossBoundary(bossHitbox.Position, bossHitbox.Size);
            }
        }
    }



    // 보스가 받는 데미지 함수
    public void TakeBossDamage(float damage)
    {
        if (!isBattleActive || currentBoss == null)
        {
            return;
        }
        currentBossHP -= damage;

        UpdateBossHPUI();
    }

    // 보스 HP Slider 및 텍스트 업데이트 함수
    private void UpdateBossHPUI()
    {
        bossHPSlider.value = currentBossHP;

        float hpPercentage = (currentBossHP / maxBossHP) * 100f;
        bossHPText.text = $"{hpPercentage:F2}%";
    }

    // ======================================================================================================================
    // [보스 공격 관련]

    // 보스 공격 코루틴
    private IEnumerator BossAttackRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(bossAttackDelay);
            BossAttackCats();
        }
    }

    // 보스가 히트박스 내 고양이 N마리를 공격하는 함수
    private void BossAttackCats()
    {
        if (currentBoss == null || bossHitbox == null) return;

        // 히트박스 경계에 있는 고양이를 찾음
        List<CatData> catsAtBoundary = new List<CatData>();
        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
            // 기절 상태의 고양이는 제외
            if (cat.isStuned) continue;

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                catsAtBoundary.Add(cat);
            }
        }

        if (catsAtBoundary.Count == 0) return;

        // 보스 공격 대상 선정
        int attackCount = Mathf.Min(catsAtBoundary.Count, currentBossData.NumOfAttack);
        List<CatData> selectedCats = new List<CatData>();

        while (selectedCats.Count < attackCount)
        {
            int randomIndex = Random.Range(0, catsAtBoundary.Count);
            CatData selectedCat = catsAtBoundary[randomIndex];

            if (!selectedCats.Contains(selectedCat))
            {
                selectedCats.Add(selectedCat);
            }
        }

        // 선택된 고양이들에게 데미지 적용
        foreach (var cat in selectedCats)
        {
            int damage = currentBossData.MouseDamage;
            cat.TakeDamage(damage);
        }
    }

    // ======================================================================================================================
    // [고양이 공격 관련]

    // 고양이 공격 코루틴
    private IEnumerator CatsAttackRoutine()
    {
        while (isBattleActive)
        {
            // 고양이 공격
            yield return new WaitForSeconds(catAttackDelay);
            CatsAttackBoss();
        }
    }

    // 히트박스 내 고양이들이 보스를 공격하는 함수
    private void CatsAttackBoss()
    {
        if (currentBoss == null || bossHitbox == null) return;

        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
            // 기절 상태인 고양이는 공격하지 않도록 처리
            if (cat.isStuned) continue;

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                int damage = cat.catData.CatDamage;
                TakeBossDamage(damage);
            }
        }
    }

    // ======================================================================================================================
    // [게임 종료 관련]

    // 항복 버튼 함수
    private void GiveUpState()
    {
        // giveup Panel관련 추가할거면 여기에 관련 함수 추가
        EndBattle(false);
    }

    // 전투 종료 함수
    public void EndBattle(bool isVictory)
    {
        isBattleActive = false;

        // 현재 실행 중인 슬라이더 코루틴 종료
        if (warningSliderCoroutine != null)
        {
            StopCoroutine(warningSliderCoroutine);
            warningSliderCoroutine = null;

            warningSlider.maxValue = warningDuration;
            warningSlider.value = 0f;
        }
        if (respawnSliderCoroutine != null)
        {
            StopCoroutine(respawnSliderCoroutine);
            respawnSliderCoroutine = null;

            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        if (isVictory)
        {
            bossStage++;
            QuestManager.Instance.AddStageCount();
        }

        // 전투 종료시 비활성화했던 기능들 다시 기존 상태로 복구
        SetEndFunctions();

        // 고양이 자동 재화 수집 활성화
        SetEndBattleAutoCollectState();

        // 퀘스트 갱신
        QuestManager.Instance.AddBattleCount();

        // 모든 고양이의 체력을 회복시켜줘야함
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned) continue;

            cat.HealCatHP();
        }

        // 자동 머지 재개
        AutoMergeManager.Instance.ResumeAutoMerge();
    }

    // ======================================================================================================================
    // [전투시 변경되는 기능들]

    // 모든 고양이의 자동 재화 수집 비활성화
    private void SetStartBattleAutoCollectState()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (!cat.isStuned)
            {
                cat.SetCollectingCoinsState(false);
                cat.DisableCollectUI();
            }
        }
    }

    // 모든 고양이의 자동 재화 수집 원래 상태로 복구 (활성화)
    private void SetEndBattleAutoCollectState()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (!cat.isStuned)
            {
                cat.SetCollectingCoinsState(true);
            }
        }
    }

    // 전투시작시 비활성화되는 외부 기능들
    private void SetStartFunctions()
    {
        MergeManager.Instance.StartBattleMergeState();
        AutoMoveManager.Instance.StartBattleAutoMoveState();
        GetComponent<SortManager>().StartBattleSortState();

        SpawnManager.Instance.StartBattleSpawnState();

        ItemMenuManager.Instance.StartBattleItemMenuState();
        BuyCatManager.Instance.StartBattleBuyCatState();
    }

    // 전투종료시 활성화되는 외부 기능들
    private void SetEndFunctions()
    {
        MergeManager.Instance.EndBattleMergeState();
        AutoMoveManager.Instance.EndBattleAutoMoveState();
        GetComponent<SortManager>().EndBattleSortState();

        SpawnManager.Instance.EndBattleSpawnState();

        ItemMenuManager.Instance.EndBattleItemMenuState();
        BuyCatManager.Instance.EndBattleBuyCatState();
    }

    // ======================================================================================================================



}

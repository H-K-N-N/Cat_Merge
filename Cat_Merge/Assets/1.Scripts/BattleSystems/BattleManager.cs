using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// BattleManager Script
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // 보스 프리팹
    [SerializeField] private Transform bossUIParent;            // 보스를 배치할 부모 Transform (UI Panel 등)
    [SerializeField] private Slider respawnSlider;              // 보스 소환까지 남은 시간을 표시할 Slider UI

    private float spawnInterval = 10f;                          // 보스 등장 주기
    private float timer = 0f;                                   // 보스 소환 타이머
    private float bossDuration = 10f;                           // 보스 유지 시간
    private int bossStage = 1;                                  // 보스 스테이지

    private float bossAttackDelay = 2f;                         // 보스 공격 딜레이
    private float catAttackDelay = 1f;                          // 고양이 공격 딜레이

    private GameObject currentBoss = null;                      // 현재 보스
    private BossHitbox bossHitbox;                              // 보스 히트박스
    private bool isBattleActive = false;                        // 전투 활성화 여부

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (활성화/비활성화 제어)
    [SerializeField] private TextMeshProUGUI bossNameText;      // Boss Name Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text

    private Mouse currentBossData = null;                       // 현재 보스 데이터
    private float currentBossHP;                                // 보스의 현재 HP
    private float maxBossHP;                                    // 보스의 최대 HP

    [Header("---[Warning Panel]")]
    [SerializeField] private GameObject warningPanel;           // 전투시스템 시작시 나오는 경고 Panel (warningTimer동안 지속)
    [SerializeField] private Slider warningSlider;              // 리스폰시간이 됐을때 차오르는 Slider (warningTimer만큼 차오름)
    private float warningTimer = 2.0f;

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
    }

    private void Start()
    {
        StartCoroutine(BossSpawnRoutine());
    }

    private void InitializeBattleManager()
    {
        // WarningPanel 설정
        warningPanel.SetActive(false);

        // respawnSlider 설정
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        // warning Slider 설정
        if (warningSlider != null)
        {
            warningSlider.maxValue = warningTimer;
            warningSlider.value = 0f;
        }

        // Battle HP UI 초기화
        if (battleHPUI != null)
        {
            battleHPUI.SetActive(false);
        }
    }

    // ======================================================================================================================

    // 보스 스폰 코루틴
    private IEnumerator BossSpawnRoutine()
    {
        while (true)
        {
            // 보스가 없을 때만 게이지를 충전
            if (currentBoss == null)
            {
                timer += Time.deltaTime;
                respawnSlider.value = timer;

                // 게이지가 꽉 차면 보스 소환
                if (timer >= spawnInterval)
                {
                    timer = 0f;

                    
                    yield return StartCoroutine(LoadWarningPanel());

                    LoadAndDisplayBoss();
                    StartBattle();
                }
            }

            yield return null;
        }
    }

    private IEnumerator LoadWarningPanel()
    {
        warningPanel.SetActive(true);
        float elapsedTime = 0f;

        //// WarningSlider 초기화
        //warningSlider.value = 0f;
        //warningSlider.maxValue = warningTimer;

        // WarningTimer 동안 WarningSlider 차오르게 하기
        while (elapsedTime < warningTimer)
        {
            elapsedTime += Time.deltaTime;
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

        // 보스를 항상 최하위 자식으로 설정 (UI상 고양이들 뒤에 생기게)
        currentBoss.transform.SetAsFirstSibling();

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

    // 배틀 시작할때마다 Boss UI Panel 설정 함수
    private void UpdateBossUI()
    {
        battleHPUI.SetActive(true);
        bossNameText.text = currentBossData.MouseName;

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = maxBossHP;
        bossHPSlider.value = currentBossHP;
        bossHPText.text = $"{maxBossHP}%";
    }

    // 보스가 받는 데미지 함수
    public void TakeBossDamage(float damage)
    {
        if (!isBattleActive || currentBoss == null) return;

        currentBossHP -= damage;
        if (currentBossHP < 0)
        {
            currentBossHP = 0;
        }

        UpdateBossHPUI();
    }

    // HP Slider 및 텍스트 업데이트 함수
    private void UpdateBossHPUI()
    {
        bossHPSlider.value = currentBossHP;

        float hpPercentage = (currentBossHP / maxBossHP) * 100f;
        bossHPText.text = $"{hpPercentage:F2}%";
    }

    // 전투 시작 함수
    private void StartBattle()
    {
        // 전투 시작시 여러 기능들 비활성화
        SetStartFunctions();

        StartCoroutine(ExecuteBattleSliders(warningTimer, bossDuration));
        StartCoroutine(BossBattleRoutine(warningTimer + bossDuration));
        isBattleActive = true;

        // 보스 히트박스 내에 존재하는 고양이들 밀어내기
        PushCatsAwayFromBoss();

        // 보스 히트박스 범위 밖에 있는 고양이들을 보스를 향해 이동 시키기
        MoveCatsTowardBossBoundary();

        // 보스 공격 코루틴 시작
        StartCoroutine(BossAttackRoutine());
        
        // 고양이 공격 코루틴 시작
        StartCoroutine(CatsAttackRoutine());
    }

    // [MergeManager, AutoMoveManager, SortManager, AutoMergeManager]
    // [SpawnManager]
    // [ItemMenuManager, BuyCatManager, DictionaryManager, QuestManager]
    private void SetStartFunctions()
    {
        GetComponent<MergeManager>().StartBattleMergeState();
        GetComponent<AutoMoveManager>().StartBattleAutoMoveState();
        GetComponent<SortManager>().StartBattleSortState();
        //GetComponent<AutoMergeManager>().StartBattleAutoMergeState();
        //GetComponent<SpawnManager>().StartBattleSpawnState();
    }
    private void SetEndFunctions()
    {
        GetComponent<MergeManager>().EndBattleMergeState();
        GetComponent<AutoMoveManager>().EndBattleAutoMoveState();
        GetComponent<SortManager>().EndBattleSortState();
        //GetComponent<AutoMergeManager>().EndBattleAutoMergeState();
        //GetComponent<SpawnManager>().EndBattleSpawnState();
    }

    // 슬라이더 감소 관리 코루틴
    private IEnumerator ExecuteBattleSliders(float warningDuration, float bossDuration)
    {
        // 1. warningSlider 감소
        yield return StartCoroutine(DecreaseWarningSliderDuringBossDuration(warningDuration));

        // 2. respawnSlider 감소
        yield return StartCoroutine(DecreaseSliderDuringBossDuration(bossDuration));
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

        // 각 고양이를 확인하여 보스의 히트박스 내에 있으면 밀어내기
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

    // 보스 공격 코루틴
    private IEnumerator BossAttackRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(bossAttackDelay);
            BossAttackCats();
        }
    }

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

    // 보스가 히트박스 내 고양이 N마리를 공격하는 함수
    private void BossAttackCats()
    {
        if (currentBoss == null || bossHitbox == null) return;

        // 히트박스 경계에 있는 고양이를 찾음
        List<CatData> catsAtBoundary = new List<CatData>();
        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
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

    // 히트박스 내 고양이들이 보스를 공격하는 함수
    private void CatsAttackBoss()
    {
        if (currentBoss == null || bossHitbox == null) return;

        CatData[] allCats = FindObjectsOfType<CatData>();

        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                int damage = cat.catData.CatDamage;
                TakeBossDamage(damage);
            }
        }
    }

    // 전투 종료 함수
    public void EndBattle(bool isVictory)
    {
        isBattleActive = false;
        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        if (isVictory)
        {
            bossStage++;
        }

        // respawnSlider 초기화
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        // warningSlider 초기화
        if (warningSlider != null)
        {
            warningSlider.maxValue = warningTimer;
            warningSlider.value = 0f;
        }

        // 전투 종료시 비활성화했던 기능들 다시 기존 상태로 복구
        SetEndFunctions();

        // 전투가 종료되면 모든 고양이의 체력을 최대로 회복하는 기능도 넣어야할듯


    }

    // ======================================================================================================================



}

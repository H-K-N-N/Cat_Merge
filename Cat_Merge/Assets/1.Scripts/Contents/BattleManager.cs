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
    //private RectTransform panelRectTransform;                   // Panel의 크기 정보 (배치할 범위)

    private float spawnInterval = 10f;                          // 보스 등장 주기
    private float timer = 0f;                                   // 보스 소환 타이머
    private float bossDuration = 10f;                           // 보스 유지 시간
    //private float bossAttackDelay = 2f;                         // 보스 공격 딜레이
    private int bossStage = 1;                                  // 보스 스테이지

    private GameObject currentBoss = null;                      // 현재 보스
    private bool isBattleActive = false;                        // 전투 활성화 여부
    //private BossHitbox bossHitbox;                              // 보스 히트박스

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (활성화/비활성화 제어)
    [SerializeField] private TextMeshProUGUI bossNameText;      // Boss Name Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text

    private Mouse currentBossData = null;                       // 현재 보스 데이터
    private float currentBossHP;                                // 보스의 현재 HP
    private float maxBossHP;                                    // 보스의 최대 HP

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
        //panelRectTransform = bossUIParent.GetComponent<RectTransform>();
        //bossHitbox = currentBoss.GetComponent<BossHitbox>();
        StartCoroutine(BossSpawnRoutine());
    }

    private void InitializeBattleManager()
    {
        // 슬라이더 UI 설정
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
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
                    LoadAndDisplayBoss();
                    StartBattle();
                }
            }

            yield return null;
        }
    }

    // 보스 스폰 함수
    private void LoadAndDisplayBoss()
    {
        // bossStage에 맞는 Mouse를 가져와서 보스를 설정
        currentBossData = GetBossData();
        currentBoss = Instantiate(bossPrefab, bossUIParent);

        // 보스의 MouseData를 설정
        MouseData mouseUIData = currentBoss.GetComponent<MouseData>();
        mouseUIData.SetMouseData(currentBossData);

        // 보스 위치 설정
        RectTransform bossRectTransform = currentBoss.GetComponent<RectTransform>();
        bossRectTransform.anchoredPosition = new Vector2(0f, 250f);

        // 보스의 히트박스 위치를 보스 위치와 동일하게 설정
        BossHitbox bossHitbox = currentBoss.GetComponent<BossHitbox>();
        bossHitbox.transform.position = bossRectTransform.position;

        // 보스를 항상 최하위 자식으로 설정 (UI상 고양이들 뒤에 생기게)
        currentBoss.transform.SetAsFirstSibling();

        UpdateBossUI();

        PushCatsAwayFromBoss();
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

        Debug.Log(currentBossData.MouseGrade + ", " + currentBossData.MouseHp);

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = maxBossHP;
        bossHPSlider.value = currentBossHP;
        bossHPText.text = $"{maxBossHP}%";
    }

    // 보스 스폰시 해당 범위의 고양이를 밀어내는 함수
    private void PushCatsAwayFromBoss()
    {
        if (currentBoss == null)
        {
            Debug.LogError("No boss to push cats away from.");
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        BossHitbox bossHitbox = currentBoss.GetComponent<BossHitbox>();

        if (bossHitbox == null)
        {
            Debug.LogError("BossHitbox component is missing on the boss.");
            return;
        }

        // 각 고양이를 확인하여 보스의 히트박스 내에 있으면 밀어내기
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            //Debug.Log(catPosition.x + ", " + catPosition.y);

            // 고양이가 보스의 히트박스 범위 내에 있는지 확인
            if (bossHitbox.IsInHitbox(catPosition))
            {
                // 해당 고양이를 히트박스 범위 외곽으로 밀기 (현재는 로그만)
                Debug.Log("히트박스 범위 내 존재");
            }
        }
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
        isBattleActive = true;
        StartCoroutine(DecreaseSliderDuringBossDuration(bossDuration));
        StartCoroutine(BossBattleRoutine(bossDuration));

        // 고양이들의 모든 행동 정지
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(false);
        }

        //// 고양이들이 보스를 향해 이동
        //foreach (var cat in allCats)
        //{
        //    cat.MoveTowardsBoss(currentBoss.transform.position);
        //}

        //// 보스 공격 코루틴 시작
        //StartCoroutine(BossAttackRoutine());
    }

    // 보스 유지시간동안 슬라이더가 감소하는 코루틴 
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

            // 임의로 테스트
            TakeBossDamage(Time.deltaTime * 5);

            // 보스 체력이 0 이하가 되면 즉시 전투 종료
            if (currentBossHP <= 0)
            {
                EndBattle(true); // 승리로 전투 종료
                yield break;
            }

            yield return null;
        }

        // 제한 시간 초과로 전투 종료
        EndBattle(false);
    }


    /*
    // 보스 공격 코루틴
    private IEnumerator BossAttackRoutine()
    {
        while (isBattleActive)
        {
            yield return new WaitForSeconds(bossAttackDelay);

            // 보스가 무작위 고양이 3마리를 공격
            Mouse bossData = GetBossData();
            CatData[] allCats = FindObjectsOfType<CatData>();
            List<CatData> randomCats = new List<CatData>();

            for (int i = 0; i < bossData.NumOfAttack; i++)
            {
                int randomIndex = Random.Range(0, allCats.Length);
                CatData selectedCat = allCats[randomIndex];
                randomCats.Add(selectedCat);
            }

            // 고양이들에게 피해 주기 (임시로 데미지 구현)
            foreach (var cat in randomCats)
            {
                cat.TakeDamage(10); // TakeDamage 메서드를 고양이 스크립트에서 구현해야 함
            }
        }
    }
    */

    // 전투 종료 함수
    public void EndBattle(bool isVictory)
    {
        isBattleActive = false;
        Destroy(currentBoss);
        currentBoss = null;
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

        // 고양이들의 상태 복구 (자동 이동 활성화) (지금은 그냥 활성화 이지만 자동이동 변수 가져와야함)
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
        }
    }

}

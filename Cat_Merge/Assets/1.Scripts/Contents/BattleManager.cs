using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// BattleManager Script
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // 보스 프리팹
    [SerializeField] private Transform bossUIParent;            // 보스를 배치할 부모 Transform (UI Panel 등)
    [SerializeField] private Slider respawnSlider;              // 보스 소환까지 남은 시간을 표시할 Slider UI
    private RectTransform panelRectTransform;                   // Panel의 크기 정보 (배치할 범위)

    private float spawnInterval = 5f;                           // 보스 등장 주기
    private float timer = 0f;                                   // 보스 소환 타이머
    private float bossDuration = 5f;                            // 보스 유지 시간

    private float bossAttackDelay = 2f;                         // 보스 공격 딜레이
    private int bossStage = 1;                                  // 보스 스테이지

    private GameObject currentBoss = null;                      // 현재 보스
    private bool isBattleActive = false;                        // 전투 활성화 여부

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
        panelRectTransform = bossUIParent.GetComponent<RectTransform>();
        StartCoroutine(BossSpawnRoutine());
    }

    // ======================================================================================================================

    private void InitializeBattleManager()
    {
        // 슬라이더 UI 설정
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
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
        Mouse bossData = GetBossData();
        currentBoss = Instantiate(bossPrefab, bossUIParent);

        // 보스의 MouseData를 설정
        MouseData mouseUIData = currentBoss.GetComponent<MouseData>();
        if (mouseUIData != null)
        {
            mouseUIData.SetMouseData(bossData);
        }

        // 보스 위치 설정
        RectTransform bossRectTransform = currentBoss.GetComponent<RectTransform>();
        if (bossRectTransform != null)
        {
            bossRectTransform.anchoredPosition = new Vector2(0f, 250f);
        }

        // 보스를 항상 최하위 자식으로 설정 (UI상 고양이들 뒤에 생기게)
        currentBoss.transform.SetAsFirstSibling();

        Debug.Log("보스 스폰");

        StartCoroutine(DecreaseSliderDuringBossDuration(bossDuration));
        StartCoroutine(DestroyBossAfterDelay(bossDuration));
    }

    // 보스 유지시간동안 슬라이더가 감소하는 코루틴
    private IEnumerator DecreaseSliderDuringBossDuration(float duration)
    {
        float elapsedTime = 0f;
        if (respawnSlider != null)
        {
            respawnSlider.maxValue = duration;
            respawnSlider.value = duration;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            if (respawnSlider != null)
            {
                respawnSlider.value = duration - elapsedTime;
            }
            yield return null;
        }
    }

    // 보스 유지시간 이후 보스 제거하는 코루틴
    private IEnumerator DestroyBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentBoss != null)
        {
            Destroy(currentBoss);
            currentBoss = null;
            Debug.Log("보스 제거");
            EndBattle();
        }
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

    // 전투 시작 함수
    private void StartBattle()
    {
        isBattleActive = true;

        // 고양이들의 모든 행동 정지
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(false);
        }

        //PushCatsAwayFromBoss();

        // 고양이들이 보스를 향해 이동
        foreach (var cat in allCats)
        {
            cat.MoveTowardsBoss(currentBoss.transform.position);
        }

        //StartCoroutine(BossAttackRoutine());
    }

    //// 보스 스폰시 해당 범위의 고양이를 밀어내는 함수
    //private void PushCatsAwayFromBoss()
    //{
    //    CatData[] allCats = FindObjectsOfType<CatData>();
    //    foreach (var cat in allCats)
    //    {
    //        Vector3 direction = cat.transform.position - currentBoss.transform.position;
    //        float distance = direction.magnitude;

    //        if (distance < 5f)
    //        {
    //            Vector3 pushDirection = direction.normalized * 2f;
    //            cat.transform.position += pushDirection;
    //        }
    //    }
    //}

    //// 보스 공격 코루틴
    //private IEnumerator BossAttackRoutine()
    //{
    //    while (isBattleActive)
    //    {
    //        yield return new WaitForSeconds(bossAttackDelay);

    //        // 보스가 무작위 고양이 3마리를 공격
    //        Mouse bossData = GetBossData();
    //        CatData[] allCats = FindObjectsOfType<CatData>();
    //        List<CatData> randomCats = new List<CatData>();

    //        for (int i = 0; i < bossData.NumOfAttack; i++)
    //        {
    //            int randomIndex = Random.Range(0, allCats.Length);
    //            CatData selectedCat = allCats[randomIndex];
    //            randomCats.Add(selectedCat);
    //        }

    //        // 고양이들에게 피해 주기 (임시로 데미지 구현)
    //        foreach (var cat in randomCats)
    //        {
    //            cat.TakeDamage(10); // TakeDamage 메서드를 고양이 스크립트에서 구현해야 함
    //        }
    //    }
    //}

    // 전투 종료 함수
    public void EndBattle()
    {
        isBattleActive = false;
        Destroy(currentBoss);
        currentBoss = null;
        //bossStage++;

        // 고양이들의 상태 복구 (자동 이동 활성화) (지금은 그냥 활성화 이지만 자동이동 변수 가져와야함)
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
        }
    }


}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

// BattleManager Script
public class BattleManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static BattleManager Instance { get; private set; }

    [Header("---[Battle System]")]
    [SerializeField] private GameObject bossPrefab;             // 보스 프리팹
    [SerializeField] private Transform bossUIParent;            // 보스를 배치할 부모 Transform (UI Panel 등)
    [SerializeField] private Slider respawnSlider;              // 보스 소환까지 남은 시간을 표시할 Slider UI


    private const float DEFAULT_SPAWN_INTERVAL = 30f;          // 보스 등장 주기 (300f)
    private float spawnInterval;                                // 보스 등장 주기
    private Coroutine respawnSliderCoroutine;                   // Slider 코루틴
    private float bossSpawnTimer = 0f;                          // 보스 스폰 타이머
    private float sliderDuration;                               // Slider 유지 시간
    private const float DEFAULT_BOSS_DURATION = 30f;            // 보스 유지 시간 (원래 30f)
    private float bossDuration;                                 // 보스 유지 시간
    private int bossStage = 1;                                  // 보스 스테이지
    public int BossStage => bossStage;

    private const float GIVEUP_BUTTON_DELAY = 2f;               // 항복 버튼 활성화 딜레이
    private const float BOSS_ATTACK_DELAY = 2f;                 // 보스 공격 딜레이
    private float catAttackDelay = 1f;                          // 고양이 공격 딜레이
    

    private GameObject currentBoss;                             // 현재 보스
    [HideInInspector] public BossHitbox bossHitbox;             // 보스 히트박스
    public bool isBattleActive;                                // 전투 활성화 여부
    public bool IsBattleActive => isBattleActive;

    private HashSet<int> clearedStages = new HashSet<int>();    // 클리어한 스테이지 저장

    [Header("---[Boss UI]")]
    [SerializeField] private GameObject battleHPUI;             // Battle HP UI (활성화/비활성화 제어)
    [SerializeField] private TextMeshProUGUI bossStageText;     // Boss Stage Text
    [SerializeField] private Slider bossHPSlider;               // HP Slider
    [SerializeField] private TextMeshProUGUI bossHPText;        // HP % Text
    [SerializeField] private Button giveupButton;               // 항복 버튼

    private Mouse currentBossData;                              // 현재 보스 데이터
    private double currentBossHP;                               // 보스의 현재 HP
    private double maxBossHP;                                   // 보스의 최대 HP


    [Header("---[Boss Result UI]")]
    [SerializeField] private GameObject battleResultPanel;              // 전투 결과 패널
    [SerializeField] private GameObject winPanel;                       // 승리 UI 패널
    [SerializeField] private GameObject losePanel;                      // 패배 UI 패널
    [SerializeField] private Button battleResultCloseButton;            // 전투 결과 패널 닫기 버튼
    [SerializeField] private TextMeshProUGUI battleResultCountdownText; // 전투 결과 패널 카운트다운 Text
    private Coroutine resultPanelCoroutine;                             // 결과 패널 자동 닫기 코루틴

    [SerializeField] private GameObject rewardSlotPrefab;          // Reward Slot 프리팹
    [SerializeField] private Transform winRewardPanel;             // Win Panel의 Reward Panel
    [SerializeField] private Transform loseRewardPanel;            // Lose Panel의 Reward Panel
    private Sprite cashSprite;                                     // 캐시 이미지
    private Sprite coinSprite;                                     // 코인 이미지
    private List<GameObject> activeRewardSlots = new List<GameObject>();  // 현재 활성화된 보상 슬롯들

    [Header("---[Boss AutoRetry UI]")]
    [SerializeField] private Button autoRetryPanelButton;               // 하위 단계 자동 도전 패널 버튼
    [SerializeField] private Image autoRetryPanelButtonImage;           // 패널 버튼 이미지
    [SerializeField] private GameObject autoRetryPanel;                 // 하위 단계 자동 도전 패널
    [SerializeField] private Button closeAutoRetryPanelButton;          // 하위 단계 자동 도전 패널 닫기 버튼
    [SerializeField] private Button autoRetryButton;                    // 하위 단계 자동 도전 토글 버튼
    [SerializeField] private RectTransform autoRetryHandle;             // 토글 핸들
    [SerializeField] private Image autoRetryButtonImage;                // 하위 단계 자동 도전 버튼 이미지
    [SerializeField] private TextMeshProUGUI currentMaxBossStageText;   // 도전 가능한 보스 최대 스테이지 텍스트
    private int currentMaxBossStage;                                    // 도전 가능한 보스 최대 스테이지
    private bool isAutoRetryEnabled;                                    // 하위 단계 자동 도전 상태
    private Coroutine autoRetryToggleCoroutine;                         // 토글 애니메이션 코루틴


    [Header("---[Warning UI]")]
    [SerializeField] private GameObject warningPanel;           // 전투시스템 시작시 나오는 경고 Panel (warningDuration동안 지속)
    [SerializeField] private Slider warningSlider;              // 리스폰시간이 됐을때 차오르는 Slider (warningDuration만큼 차오름)
    public float warningDuration = 2f;                          // warningPanel 활성화 시간
    private Coroutine warningSliderCoroutine;                   // warningSlider 코루틴

    [SerializeField] private Image topWarningImage;             // 상단 경고 이미지
    [SerializeField] private Image bossDangerImage;             // 보스 위험 이미지
    [SerializeField] private Image bottomWarningImage;          // 하단 경고 이미지
    private CanvasGroup warningPanelCanvasGroup;                // Warning Panel의 CanvasGroup
    private CanvasGroup topWarningCanvasGroup;                  // 상단 경고 이미지의 CanvasGroup
    private CanvasGroup bossDangerCanvasGroup;                  // 보스 위험 이미지의 CanvasGroup  
    private CanvasGroup bottomWarningCanvasGroup;               // 하단 경고 이미지의 CanvasGroup

    private const float WARNING_IMAGE_START_X = -640f;          // 경고 이미지 시작 좌표
    private const float WARNING_IMAGE_END_X = 640f;             // 경고 이미지 끝 좌표
    private const float BOSS_DANGER_START_X = 1040f;            // Boss Danger 이미지 시작 좌표
    private const float BOSS_DANGER_END_X = -1040f;             // Boss Danger 이미지 끝 좌표
    private const float WARNING_IMAGE_Y = 0f;                   // 경고 이미지 Y 좌표


    [Header("---[ETC]")]
    private Coroutine bossBattleCoroutine;                      // BossBattleRoutine 코루틴 추적을 위한 변수 추가
    private Coroutine bossSpawnRoutine;                         // BossSpawnRoutine 코루틴 추적을 위한 변수 추가
    private Coroutine bossAttackRoutine;                        // BossAttackRoutine 코루틴 추적을 위한 변수 추가
    private Coroutine catsAttackRoutine;                        // CatsAttackRoutine 코루틴 추적을 위한 변수 추가

    private bool isDataLoaded = false;                          // 데이터 로드 확인

    #endregion


    #region Unity Methods

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
    }

    private void Start()
    {
        InitializeBattleManager();

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            bossStage = 1;
            currentMaxBossStage = bossStage;

            isAutoRetryEnabled = false;
        }

        UpdateAutoRetryUI(isAutoRetryEnabled, true);
        bossSpawnRoutine = StartCoroutine(BossSpawnRoutine());
    }

    #endregion


    #region Initialization

    // BattleManager 초기 설정
    private void InitializeBattleManager()
    {
        InitializeWarningPanel();
        InitializeTimers();
        InitializeSliders();
        InitializeBattleUI();
        InitializeCanvasGroups();
        InitializeButtonListeners();

        InitializeRewardSprites();
        UpdateCurrentMaxBossStageText();
    }

    // warningPanel 초기화 함수
    private void InitializeWarningPanel()
    {
        warningPanel.SetActive(false);
    }

    // 시간 초기화 함수 (보스 리스폰, 보스 전투 시간)
    private void InitializeTimers()
    {
        spawnInterval = DEFAULT_SPAWN_INTERVAL - warningDuration;
        bossDuration = DEFAULT_BOSS_DURATION;
        sliderDuration = bossDuration - warningDuration;
    }

    // Sliders UI 초기화 함수
    private void InitializeSliders()
    {
        if (respawnSlider != null)
        {
            respawnSliderCoroutine = null;
            respawnSlider.maxValue = spawnInterval;
            respawnSlider.value = 0f;
        }

        if (warningSlider != null)
        {
            warningSliderCoroutine = null;
            warningSlider.maxValue = warningDuration;
            warningSlider.value = 0f;
        }
    }

    // Battle HP UI 초기화 함수
    private void InitializeBattleUI()
    {
        if (battleHPUI != null)
        {
            battleHPUI.SetActive(false);
        }
    }

    // 경고 UI 관련 CanvasGroup 초기화 함수
    private void InitializeCanvasGroups()
    {
        warningPanelCanvasGroup = SetupCanvasGroup(warningPanel);
        topWarningCanvasGroup = SetupCanvasGroup(topWarningImage.gameObject);
        bossDangerCanvasGroup = SetupCanvasGroup(bossDangerImage.gameObject);
        bottomWarningCanvasGroup = SetupCanvasGroup(bottomWarningImage.gameObject);
    }

    // UI 오브젝트에 CanvasGroup 컴포넌트가 없으면 추가하고 반환하는 함수
    private CanvasGroup SetupCanvasGroup(GameObject uiObject)
    {
        // 기존 CanvasGroup이 있으면 사용, 없으면 새로 추가
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = uiObject.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    // 버튼 리스너 초기화 함수
    private void InitializeButtonListeners()
    {
        giveupButton.onClick.AddListener(GiveUpState);
        battleResultCloseButton.onClick.AddListener(CloseBattleResultPanel);

        autoRetryPanelButton.onClick.AddListener(OpenAutoRetryPanel);
        autoRetryButton.onClick.AddListener(ToggleAutoRetry);
        closeAutoRetryPanelButton.onClick.AddListener(CloseAutoRetryPanel);

        autoRetryPanelButtonImage = autoRetryPanelButton.GetComponent<Image>();
        autoRetryButtonImage = autoRetryButton.GetComponent<Image>();
        UpdateToggleButtonImage(autoRetryButtonImage, isAutoRetryEnabled);
        UpdateAutoRetryPanelButtonColor(isAutoRetryEnabled);
        UpdateAutoRetryUI(isAutoRetryEnabled, true);
    }

    // 보상 이미지 초기화 함수
    private void InitializeRewardSprites()
    {
        cashSprite = Resources.Load<Sprite>("Sprites/UI/I_UI_Main/I_UI_paidcoin.9");
        coinSprite = Resources.Load<Sprite>("Sprites/UI/I_UI_Main/I_UI_coin.9");
    }

    // 최대 클리어 보스 스테이지 텍스트 업데이트 함수
    private void UpdateCurrentMaxBossStageText()
    {
        if (currentMaxBossStageText != null)
        {
            int maxClearedStage = clearedStages.Count > 0 ? clearedStages.Max() : 0;
            currentMaxBossStageText.text = $"최대 클리어 보스 스테이지 : {maxClearedStage}";
        }
    }

    // 패시브로 인한 고양이 공격속도 증가 함수
    public void AddPassiveCatAttackSpeedBuff(float amount)
    {
        catAttackDelay -= amount;
    }

    #endregion


    #region Battle

    #region Battle Core

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

        // Image 초기 위치 설정
        topWarningImage.rectTransform.anchoredPosition = new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y);
        bossDangerImage.rectTransform.anchoredPosition = new Vector2(BOSS_DANGER_START_X, WARNING_IMAGE_Y);
        bottomWarningImage.rectTransform.anchoredPosition = new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y);

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

            // 이미지 이동
            float moveProgress = elapsedTime / warningDuration;
            topWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bottomWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bossDangerImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(BOSS_DANGER_START_X, WARNING_IMAGE_Y),
                new Vector2(BOSS_DANGER_END_X, WARNING_IMAGE_Y),
                moveProgress);

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

            // 이미지 이동
            float moveProgress = elapsedTime / warningDuration;
            topWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bottomWarningImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(WARNING_IMAGE_START_X, WARNING_IMAGE_Y),
                new Vector2(WARNING_IMAGE_END_X, WARNING_IMAGE_Y),
                moveProgress);
            bossDangerImage.rectTransform.anchoredPosition = Vector2.Lerp(
                new Vector2(BOSS_DANGER_START_X, WARNING_IMAGE_Y),
                new Vector2(BOSS_DANGER_END_X, WARNING_IMAGE_Y),
                moveProgress);

            // slider 업데이트
            warningSlider.value = elapsedTime;

            yield return null;
        }

        warningPanel.SetActive(false);
    }

    // 보스 스폰 함수
    private void LoadAndDisplayBoss()
    {
        // 자동 재도전 상태에 따라 도전할 스테이지 결정
        if (isAutoRetryEnabled)
        {
            bossStage = Mathf.Max(1, currentMaxBossStage - 1);
        }
        else
        {
            bossStage = currentMaxBossStage;
        }

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
        bossStageText.text = $"{currentBossData.MouseGrade} 단계";

        maxBossHP = currentBossData.MouseHp;
        currentBossHP = maxBossHP;
        bossHPSlider.maxValue = (float)maxBossHP;
        bossHPSlider.value = (float)currentBossHP;
        bossHPText.text = $"{100f}%";
    } 

    // 전투 시작 함수
    private void StartBattle()
    {
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            AnimatorManager anim = cat.GetComponent<AnimatorManager>();
            if (anim != null)
            {
                anim.ChangeState(CharacterState.isBattle);
            }
        }

        // 전투 시작시 여러 외부 기능들 비활성화
        SetStartFunctions();

        // 항복 버튼 비활성화 후 2초 뒤 활성화
        giveupButton.interactable = false;
        StartCoroutine(EnableGiveupButton());

        // Slider관련 코루틴 시작
        StartCoroutine(ExecuteBattleSliders(warningDuration, sliderDuration));
        bossBattleCoroutine = StartCoroutine(BossBattleRoutine(bossDuration));
        isBattleActive = true;

        // 고양이 자동 재화 수집 비활성화
        SetStartBattleAutoCollectState();

        // DoubleCoinForAd 효과 일시정지
        ShopManager.Instance.GetRemainingEffectTime();

        // 보스 히트박스 내,외에 존재하는 고양이들 이동시키기
        PushCatsAwayFromBoss();
        MoveCatsTowardBossBoundary();

        // 보스 및 고양이 공격 코루틴 시작
        bossAttackRoutine = StartCoroutine(BossAttackRoutine());
        catsAttackRoutine = StartCoroutine(CatsAttackRoutine());
    }

    // 항복 버튼 활성화 코루틴
    private IEnumerator EnableGiveupButton()
    {
        yield return new WaitForSeconds(GIVEUP_BUTTON_DELAY);
        giveupButton.interactable = true;
    }

    #endregion


    #region Battle Sliders

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

    #endregion


    #region Battle Management

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
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // 고양이가 히트박스 경계 내에 있는지 확인 후 히트박스 외곽으로 밀어내기
            if (bossHitbox.IsInHitbox(catPosition))
            {
                cat.MoveOppositeBoss();
            }
        }
    }

    // 보스 스폰시 히트박스 범위 밖에 있는 고양이를 히트박스 경계로 이동시키는 함수
    private void MoveCatsTowardBossBoundary()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            // 고양이가 히트박스 경계 밖에 있는지 확인 후 히트박스 외곽으로 모으기
            if (!bossHitbox.IsInHitbox(catPosition))
            {
                cat.MoveTowardBossBoundary();
            }
        }
    }

    #endregion


    #region Battle System

    // 보스가 받는 데미지 함수
    public void TakeBossDamage(float damage)
    {
        if (!IsBattleActive || currentBoss == null)
        {
            return;
        }
        currentBossHP -= damage;

        // 보스의 MouseData 컴포넌트를 통해 데미지 텍스트 표시
        MouseData mouseData = currentBoss.GetComponent<MouseData>();
        if (mouseData != null)
        {
            mouseData.ShowDamageText(damage);
        }

        UpdateBossHPUI();
    }

    // 보스 HP Slider 및 텍스트 업데이트 함수
    private void UpdateBossHPUI()
    {
        bossHPSlider.value = (float)currentBossHP;

        double hpPercentage = (currentBossHP / maxBossHP) * 100f;
        hpPercentage = Math.Max(0, hpPercentage);
        bossHPText.text = $"{hpPercentage:F2}%";
    }

    // 보스 공격 코루틴
    private IEnumerator BossAttackRoutine()
    {
        while (IsBattleActive)
        {
            yield return new WaitForSeconds(BOSS_ATTACK_DELAY);
            BossAttackCats();
        }
    }

    // 보스가 히트박스 내 고양이 N마리를 공격하는 함수
    private void BossAttackCats()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            return;
        }

        // 히트박스 경계에 있는 고양이를 찾음
        List<CatData> catsAtBoundary = new List<CatData>();
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned)
            {
                continue;
            }

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;

            if (bossHitbox.IsAtBoundary(catPosition))
            {
                catsAtBoundary.Add(cat);
            }
        }

        if (catsAtBoundary.Count == 0)
        {
            return;
        }

        // 보스 공격 대상 선정
        int attackCount = Mathf.Min(catsAtBoundary.Count, currentBossData.NumOfAttack);
        List<CatData> selectedCats = new List<CatData>();
        while (selectedCats.Count < attackCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, catsAtBoundary.Count);
            CatData selectedCat = catsAtBoundary[randomIndex];

            if (!selectedCats.Contains(selectedCat))
            {
                selectedCats.Add(selectedCat);
            }
        }

        // 선택된 고양이들에게 데미지 적용
        foreach (var cat in selectedCats)
        {
            double damage = currentBossData.MouseDamage;
            cat.TakeDamage(damage);
        }
    }

    // 고양이 공격 코루틴
    private IEnumerator CatsAttackRoutine()
    {
        while (IsBattleActive)
        {
            yield return new WaitForSeconds(catAttackDelay);
            CatsAttackBoss();
        }
    }

    // 히트박스 내 고양이들이 보스를 공격하는 함수
    private void CatsAttackBoss()
    {
        if (currentBoss == null || bossHitbox == null)
        {
            return;
        }

        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            if (cat.isStuned)
            {
                continue;
            }

            RectTransform catRectTransform = cat.GetComponent<RectTransform>();
            Vector3 catPosition = catRectTransform.anchoredPosition;
            DragAndDropManager dragManager = cat.GetComponent<DragAndDropManager>();
            AnimatorManager anim = cat.GetComponent<AnimatorManager>();

            // 드래그 중이 아닐 때만 애니메이션 상태 변경
            if (dragManager != null && !dragManager.isDragging && anim != null && !cat.isStuned)
            {
                if (bossHitbox.IsAtBoundary(catPosition))
                {
                    // 공격 실행
                    int damage = cat.catData.CatDamage;
                    TakeBossDamage(damage);

                    // 공격 애니메이션으로 변경
                    anim.ChangeState(CharacterState.isAttack);

                    // 공격 후 전투 대기 상태로 돌아가는 코루틴 시작
                    StartCoroutine(ReturnToBattleStateCat(cat));
                }
                else
                {
                    // 히트박스 경계에 없는 경우 전투 대기 상태로
                    anim.ChangeState(CharacterState.isBattle);
                }
            }
        }
    }

    // 공격 후 전투 대기 상태로 돌아가는 코루틴
    private IEnumerator ReturnToBattleStateCat(CatData cat)
    {
        // 공격 애니메이션이 재생되는 시간
        yield return new WaitForSeconds(1f);

        // 전투가 아직 진행 중이고, 해당 고양이가 아직 활성화 상태일 때만 상태 변경
        if (isBattleActive && cat != null && cat.gameObject.activeSelf && !cat.isStuned)
        {
            DragAndDropManager dragManager = cat.GetComponent<DragAndDropManager>();
            AnimatorManager anim = cat.GetComponent<AnimatorManager>();
            if (dragManager != null && !dragManager.isDragging && anim != null)
            {
                anim.ChangeState(CharacterState.isBattle);
            }
        }
    }

    #endregion


    #region Battle Plus UI

    // 전투 결과 보여주는 함수
    private void ShowBattleResult(bool isVictory)
    {
        battleResultPanel.SetActive(true);
        winPanel.SetActive(isVictory);
        losePanel.SetActive(!isVictory);

        ClearRewardSlots();

        // 승리했을 때 보상 슬롯 생성
        if (isVictory)
        {
            bool isFirstClear = !clearedStages.Contains(bossStage);
            if (isFirstClear)
            {
                // 최초 클리어 보상
                CreateRewardSlot(winRewardPanel, cashSprite, currentBossData.ClearCashReward, true);
                CreateRewardSlot(winRewardPanel, coinSprite, currentBossData.ClearCoinReward, true);
            }
            else
            {
                // 반복 클리어 보상
                CreateRewardSlot(winRewardPanel, coinSprite, currentBossData.RepeatclearCoinReward, false);
            }
        }
        else
        {
            // 패배 했을때 보상 (클리어한 최대 스테이지의 반복 클리어 보상) (첫 번째 스테이지에서의 패배는 보상 없음)
            if (currentMaxBossStage > 1)
            {
                int maxClearedStage = clearedStages.Count > 0 ? clearedStages.Max() : 0;
                if (maxClearedStage > 0)
                {
                    Mouse maxClearedBossData = GetBossDataByStage(maxClearedStage);
                    if (maxClearedBossData != null)
                    {
                        CreateRewardSlot(loseRewardPanel, coinSprite, maxClearedBossData.RepeatclearCoinReward, false);
                    }
                }
            }
        }

        if (resultPanelCoroutine != null)
        {
            StopCoroutine(resultPanelCoroutine);
        }
        resultPanelCoroutine = StartCoroutine(AutoCloseBattleResultPanel());
    }

    // 보상 슬롯 제거 함수
    private void ClearRewardSlots()
    {
        foreach (GameObject slot in activeRewardSlots)
        {
            Destroy(slot);
        }
        activeRewardSlots.Clear();
    }

    // 전투 결과 패널 닫는 함수
    private void CloseBattleResultPanel()
    {
        if (resultPanelCoroutine != null)
        {
            StopCoroutine(resultPanelCoroutine);
            resultPanelCoroutine = null;
        }
        ClearRewardSlots();
        battleResultPanel.SetActive(false);
    }

    // 전투 결과 패널 자동으로 닫는 코루틴
    private IEnumerator AutoCloseBattleResultPanel()
    {
        float countdown = 3f;
        while (countdown > 0)
        {
            battleResultCountdownText.text = $"{countdown:F0}초 후 자동 닫힘";
            countdown -= Time.deltaTime;
            yield return null;
        }
        CloseBattleResultPanel();
    }

    // 자동 재도전 패널 열기
    private void OpenAutoRetryPanel()
    {
        autoRetryPanel.SetActive(true);
    }

    // 자동 재도전 패널 닫기
    private void CloseAutoRetryPanel()
    {
        autoRetryPanel.SetActive(false);
    }

    // 토글 버튼 이미지 업데이트
    private void UpdateToggleButtonImage(Image buttonImage, bool isOn)
    {
        string imagePath = isOn ? "Sprites/UI/I_UI_Option/I_UI_option_on_Frame.9" : "Sprites/UI/I_UI_Option/I_UI_option_off_Frame.9";
        buttonImage.sprite = Resources.Load<Sprite>(imagePath);
    }

    // 자동 재도전 토글 함수
    private void ToggleAutoRetry()
    {
        isAutoRetryEnabled = !isAutoRetryEnabled;
        UpdateAutoRetryUI(isAutoRetryEnabled);
        UpdateToggleButtonImage(autoRetryButtonImage, isAutoRetryEnabled);
        UpdateAutoRetryPanelButtonColor(isAutoRetryEnabled);

        GoogleSave();
    }

    // 자동 재도전 UI 업데이트
    private void UpdateAutoRetryUI(bool state, bool instant = false)
    {
        float targetX = state ? 65f : -65f;

        if (instant)
        {
            autoRetryHandle.anchoredPosition = new Vector2(targetX, autoRetryHandle.anchoredPosition.y);
        }
        else
        {
            if (autoRetryToggleCoroutine != null)
            {
                StopCoroutine(autoRetryToggleCoroutine);
            }
            autoRetryToggleCoroutine = StartCoroutine(AnimateAutoRetryHandle(targetX));
        }
    }

    // 토글 핸들 애니메이션
    private IEnumerator AnimateAutoRetryHandle(float targetX)
    {
        float elapsedTime = 0f;
        float startX = autoRetryHandle.anchoredPosition.x;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            autoRetryHandle.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), autoRetryHandle.anchoredPosition.y);
            yield return null;
        }

        autoRetryHandle.anchoredPosition = new Vector2(targetX, autoRetryHandle.anchoredPosition.y);
    }

    // 패널 버튼 색상 업데이트 함수 추가
    private void UpdateAutoRetryPanelButtonColor(bool isEnabled)
    {
        if (autoRetryPanelButtonImage != null)
        {
            autoRetryPanelButtonImage.color = isEnabled ?
                new Color32(130, 255, 0, 255) :
                new Color32(255, 153, 21, 255);
        }
    }

    // 보상 슬롯 생성 함수
    private void CreateRewardSlot(Transform parent, Sprite rewardSprite, decimal amount, bool isFirstClear = false)
    {
        GameObject rewardSlot = Instantiate(rewardSlotPrefab, parent);
        activeRewardSlots.Add(rewardSlot);

        // Reward Image 설정
        Image rewardImage = rewardSlot.transform.Find("Background Image/Reward Image").GetComponent<Image>();
        rewardImage.sprite = rewardSprite;

        // Reward Text 설정
        TextMeshProUGUI rewardText = rewardSlot.transform.Find("Reward Text").GetComponent<TextMeshProUGUI>();
        rewardText.text = GameManager.Instance.FormatPriceNumber(amount);

        // First Clear Image 설정
        GameObject firstClearImage = rewardSlot.transform.Find("First Clear Image").gameObject;
        firstClearImage.SetActive(isFirstClear);
    }

    #endregion


    #region Battle End

    // 항복 버튼 함수
    private void GiveUpState()
    {
        // giveup Panel관련 추가할거면 여기에 관련 함수 추가
        EndBattle(false);
    }

    // 전투 종료 함수
    public void EndBattle(bool isVictory)
    {
        if (!isBattleActive)
        {
            return;
        }

        isBattleActive = false;

        // 모든 전투 관련 코루틴 종료
        StopAllBattleCoroutines();

        // 슬라이더 초기화
        InitializeSliders();

        ShowBattleResult(isVictory);
        GiveStageReward(isVictory);
        if (isVictory)
        {
            currentMaxBossStage = Mathf.Max(currentMaxBossStage, bossStage + 1);
            UpdateCurrentMaxBossStageText();
            QuestManager.Instance.AddStageCount();
        }
        else
        {
            
        }

        Destroy(currentBoss);
        currentBossData = null;
        currentBoss = null;
        bossHitbox = null;
        battleHPUI.SetActive(false);

        GoogleSave();

        // 전투 종료시 비활성화했던 기능들 다시 기존 상태로 복구
        SetEndFunctions();

        // 고양이 자동 재화 수집 활성화
        SetEndBattleAutoCollectState();

        // 퀘스트 갱신
        QuestManager.Instance.AddBattleCount();

        // 고양이들의 체력 회복
        CatData[] allCats = FindObjectsOfType<CatData>();
        foreach (var cat in allCats)
        {
            cat.HealCatHP();

            AnimatorManager anim = cat.GetComponent<AnimatorManager>();
            if (anim != null)
            {
                anim.ChangeState(CharacterState.isIdle);
            }
        }

        AutoMoveManager.Instance.EndBattleAutoMoveState();

        // 자동 머지 재개
        AutoMergeManager.Instance.ResumeAutoMerge();

        // DoubleCoinForAd 효과 재개
        ShopManager.Instance.GetRemainingEffectTime();
    }

    // 보상 지급 함수
    private void GiveStageReward(bool isVictory)
    {
        if (currentBossData == null)
        {
            return;
        }

        if (isVictory)
        {
            bool isFirstClear = !clearedStages.Contains(bossStage);
            if (isFirstClear)
            {
                // 최초 클리어 보상
                GameManager.Instance.Cash += currentBossData.ClearCashReward;
                GameManager.Instance.Coin += currentBossData.ClearCoinReward;
                clearedStages.Add(bossStage);
            }
            else
            {
                // 반복 클리어 보상
                GameManager.Instance.Coin += currentBossData.RepeatclearCoinReward;
            }
        }
        else
        {
            if (currentMaxBossStage > 1)
            {
                // 패배 시 최대 클리어 스테이지의 반복 보상 지급
                int maxClearedStage = clearedStages.Count > 0 ? clearedStages.Max() : 0;
                if (maxClearedStage > 0)
                {
                    Mouse maxClearedBossData = GetBossDataByStage(maxClearedStage);
                    if (maxClearedBossData != null)
                    {
                        GameManager.Instance.Coin += maxClearedBossData.RepeatclearCoinReward;
                        Debug.Log(maxClearedBossData.RepeatclearCoinReward);
                    }
                }
            }
        }
    }

    // 특정 스테이지의 보스 데이터를 가져오는 함수
    private Mouse GetBossDataByStage(int stage)
    {
        foreach (Mouse mouse in GameManager.Instance.AllMouseData)
        {
            if (mouse.MouseGrade == stage)
            {
                return mouse;
            }
        }
        return null;
    }

    // 모든 전투 관련 코루틴을 종료하는 함수 추가
    private void StopAllBattleCoroutines()
    {
        if (warningSliderCoroutine != null)
        {
            StopCoroutine(warningSliderCoroutine);
            warningSliderCoroutine = null;
        }

        if (respawnSliderCoroutine != null)
        {
            StopCoroutine(respawnSliderCoroutine);
            respawnSliderCoroutine = null;
        }

        if (bossBattleCoroutine != null)
        {
            StopCoroutine(bossBattleCoroutine);
            bossBattleCoroutine = null;
        }

        // 보스와 고양이의 공격 코루틴도 종료
        if (bossAttackRoutine != null)
        {
            StopCoroutine(bossAttackRoutine);
            bossAttackRoutine = null;
        }

        if (catsAttackRoutine != null)
        {
            StopCoroutine(catsAttackRoutine);
            catsAttackRoutine = null;
        }
    }

    #endregion

    #endregion


    #region State Management

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
        //AutoMoveManager.Instance.EndBattleAutoMoveState();
        GetComponent<SortManager>().EndBattleSortState();

        SpawnManager.Instance.EndBattleSpawnState();

        ItemMenuManager.Instance.EndBattleItemMenuState();
        BuyCatManager.Instance.EndBattleBuyCatState();
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public int bossStage;               // 현재 보스 스테이지
        public bool isAutoRetryEnabled;     // 하위 단계 자동 도전 상태
        public List<int> clearedStages;     // 클리어한 스테이지들
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            bossStage = this.bossStage,
            isAutoRetryEnabled = this.isAutoRetryEnabled,
            clearedStages = new List<int>(this.clearedStages)
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        this.bossStage = savedData.bossStage;
        this.isAutoRetryEnabled = savedData.isAutoRetryEnabled;
        this.clearedStages = new HashSet<int>(savedData.clearedStages ?? new List<int>());
        this.currentMaxBossStage = this.clearedStages.Count > 0 ? this.clearedStages.Max() + 1 : 1;

        UpdateToggleButtonImage(autoRetryButtonImage, isAutoRetryEnabled);
        UpdateAutoRetryPanelButtonColor(isAutoRetryEnabled);
        UpdateAutoRetryUI(isAutoRetryEnabled, true);
        UpdateCurrentMaxBossStageText();

        // 전투 중이었다면 전투 상태 초기화
        if (isBattleActive)
        {
            EndBattle(false);
        }

        // 타이머 초기화
        bossSpawnTimer = 0f;
        respawnSlider.value = 0f;

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}

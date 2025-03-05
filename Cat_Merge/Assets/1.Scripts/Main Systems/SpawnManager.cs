using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

// 고양이 스폰 Script
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private Button spawnButton;                    // 스폰 버튼

    [SerializeField] private GameObject catPrefab;                  // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;                 // 고양이를 배치할 부모 Transform (UI Panel 등)
    private RectTransform panelRectTransform;                       // Panel의 크기 정보 (배치할 범위)
    private GameManager gameManager;                                // GameManager

    [SerializeField] private TextMeshProUGUI nowAndMaxFoodText;     // 스폰버튼 밑에 현재 먹이 갯수와 최대 먹이 갯수 (?/?)
    private int nowFood = 5;                                        // 현재 먹이 갯수
    public int NowFood
    {
        get => nowFood;
        set
        {
            nowFood = value;
            UpdateFoodText();
        }
    }
    private bool isStoppedReduceCoroutine = false;                  // 코루틴 종료 판별
    private bool isStoppedAutoCoroutine = false;

    [SerializeField] private Image foodFillAmountImg;               // 소환 이미지
    [SerializeField] public Image autoFillAmountImg;                // 자동소환 이미지

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
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent가 RectTransform을 가지고 있지 않습니다.");
            return;
        }

        UpdateFoodText();
        StartCoroutine(CreateFoodTime());
        StartCoroutine(AutoCollectingTime());
    }

    // ======================================================================================================================

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleSpawnState()
    {
        spawnButton.interactable = false;
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleSpawnState()
    {
        spawnButton.interactable = true;
    }

    // ======================================================================================================================

    // 고양이 스폰 버튼 클릭
    public void OnClickedSpawn()
    {
        if (gameManager.CanSpawnCat())
        {
            // 먹이가 1개 이상이거나 최대치 이하일 때 스폰
            if (NowFood > 0 && NowFood <= ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
            {
                // 먹이가 줄고 코루틴이 종료되어있다면 다시 시작(현재 먹이가 최대치일때 코루틴 종료되어있음)
                NowFood--;
                if (isStoppedReduceCoroutine)
                {
                    StartCoroutine(CreateFoodTime());
                    isStoppedReduceCoroutine = false;
                }
                if (isStoppedAutoCoroutine)
                {
                    StartCoroutine(AutoCollectingTime());
                    isStoppedAutoCoroutine = false;
                }

                // 업그레이드 시스템 확장성을 위해 변경한 고양이 생성 코드
                Cat catData = GetCatDataForSpawn();
                LoadAndDisplayCats(catData);
                QuestManager.Instance.AddSpawnCount();
                gameManager.AddCatCount();
                FriendshipManager.Instance.nowExp += 1;
                FriendshipManager.Instance.expGauge.value += 0.05f;

                // 수동 소환 후 먹이 생성 코루틴 재시작
                if (isStoppedReduceCoroutine)
                {
                    StartCoroutine(CreateFoodTime());
                    isStoppedReduceCoroutine = false;
                }
            }
            else
            {
                NotificationManager.Instance.ShowNotification("먹이가 부족합니다!!");
            }

        }
        else
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
        }
    }

    // 자동머지시 고양이 스폰 함수
    public void SpawnCat()
    {
        if (!gameManager.CanSpawnCat()) return;

        // 업그레이드 시스템 확장성을 위해 변경한 고양이 생성 코드
        Cat catData = GetCatDataForSpawn();
        GameObject newCat = LoadAndDisplayCats(catData);

        DragAndDropManager catDragAndDrop = newCat.GetComponent<DragAndDropManager>();
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = gameManager.AllCatData[0];
            catDragAndDrop.UpdateCatUI();
        }

        gameManager.AddCatCount();
        FriendshipManager.Instance.nowExp += 1;
        FriendshipManager.Instance.expGauge.value += 0.05f;

        // 자동 소환 후 먹이 생성 코루틴 재시작
        if (isStoppedReduceCoroutine)
        {
            StartCoroutine(CreateFoodTime());
            isStoppedReduceCoroutine = false;
        }
    }

    // 상점에서 이용될 등급에 따른 구매 후 스폰 (12/26 새로 작성)
    public void SpawnGradeCat(int grade)
    {
        if (!gameManager.CanSpawnCat()) return;
        Cat catData = gameManager.AllCatData[grade];

        GameObject newCat = LoadAndDisplayCats(catData);

        DragAndDropManager catDragAndDrop = newCat.GetComponent<DragAndDropManager>();
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = gameManager.AllCatData[grade];
            catDragAndDrop.UpdateCatUI();
        }

        gameManager.AddCatCount();
        FriendshipManager.Instance.nowExp += 1;
        FriendshipManager.Instance.expGauge.value += 0.05f;
    }

    // 고양이 스폰 데이터를 선택하는 함수 (업그레이드 시스템 대비)
    private Cat GetCatDataForSpawn()
    {
        // 예시: 랜덤으로 고양이 등급을 선택하거나 상점 업그레이드를 고려하여 선택
        // return gameManager.GetRandomCatForSpawn();  // 이 부분은 나중에 업그레이드 시스템에 맞게 수정
        int catGrade = 0;
        DictionaryManager.Instance.UnlockCat(catGrade);
        return gameManager.AllCatData[catGrade];
    }

    // Panel내 랜덤 위치에 고양이 배치하는 함수
    private GameObject LoadAndDisplayCats(Cat catData)
    {
        GameObject catUIObject = Instantiate(catPrefab, catUIParent);

        // CatData 설정
        CatData catUIData = catUIObject.GetComponent<CatData>();
        if (catUIData != null)
        {
            catUIData.SetCatData(catData);

            // 자동 이동 상태를 동기화
            if (gameManager != null)
            {
                catUIData.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
            }
        }

        // 랜덤 위치 설정
        Vector2 randomPos = GetRandomPosition(panelRectTransform);
        RectTransform catRectTransform = catUIObject.GetComponent<RectTransform>();
        if (catRectTransform != null)
        {
            catRectTransform.anchoredPosition = randomPos;
        }

        return catUIObject;
    }

    // Panel내 랜덤 위치 계산하는 함수
    Vector3 GetRandomPosition(RectTransform panelRectTransform)
    {
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        Vector3 respawnPos = new Vector3(randomX, randomY, 0f);
        return respawnPos;
    }

    // ======================================================================================================================

    // 먹이 생성 시간
    private IEnumerator CreateFoodTime()
    {
        float elapsed = 0f; // 경과 시간

        // 현재 먹이가 최대치 이하일 때 코루틴 시작
        while (NowFood < ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            foodFillAmountImg.fillAmount = 0f; // 정확히 1로 설정
            while (elapsed < ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value)
            {
                elapsed += Time.deltaTime; // 매 프레임마다 경과 시간 증가
                foodFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value); // 0 ~ 1 사이로 비율 계산
                yield return null; // 다음 프레임까지 대기
            }
            NowFood++;
            foodFillAmountImg.fillAmount = 1f; // 정확히 1로 설정
            elapsed = 0f;
        }

        // 현재 먹이갯수가 최대치이면 코루틴을 종료시킨다.
        if (NowFood == ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            StopCoroutine(CreateFoodTime());
            isStoppedReduceCoroutine = true;
        }
    }

    private IEnumerator AutoCollectingTime()
    {
        float elapsed = 0f;

        // 전투 중인지 확인하며 진행
        while (true)
        {
            // 전투 중일 때 대기
            if (BattleManager.Instance.IsBattleActive)
            {
                yield return null;
                continue;
            }

            // 먹이가 1 이상일경우 자동 수집 시작 (흐으음..)
            if (NowFood >= 1)
            {
                autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / (float)ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);

                // 수집 시간 완료될때까지 대기
                while (elapsed < ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    if (BattleManager.Instance.IsBattleActive)
                    {
                        yield return null;
                        break;
                    }

                    elapsed += Time.deltaTime;
                    autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / (float)ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);
                    yield return null;
                }

                // 완료되면 먹이 줄이고 고양이 생성
                if (!BattleManager.Instance.IsBattleActive && elapsed >= ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    if (gameManager.CanSpawnCat())
                    {
                        NowFood--;
                        SpawnCat();

                        // 자동 수집 후 먹이 생성 코루틴 재시작
                        if (isStoppedReduceCoroutine)
                        {
                            StartCoroutine(CreateFoodTime());
                            isStoppedReduceCoroutine = false;
                        }
                    }
                    elapsed = 0f; // 진행 상태 초기화

                }
            }

            // 먹이가 0개면 종료
            if (NowFood == 0)
            {
                isStoppedAutoCoroutine = true;
                yield break;
            }

            yield return null;
        }
    }

    // 최대 먹이 수량이 증가했을 때 호출되는 함수
    public void OnMaxFoodIncreased()
    {
        // 최대 레벨에 도달했는지 확인
        if (ItemMenuManager.Instance.MaxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count)
        {
            return;
        }

        // 현재 먹이 수가 최대치보다 작으면 먹이 생성 코루틴 시작
        int currentMaxFood = ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
        if (NowFood < currentMaxFood)
        {
            if (isStoppedReduceCoroutine)
            {
                StartCoroutine(CreateFoodTime());
                isStoppedReduceCoroutine = false;
            }
        }
    }

    public void UpdateFoodText()
    {
        if (ItemMenuManager.Instance.MaxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count)
        {
            // 최대 레벨일 경우 마지막 값 사용
            int lastMaxFood = ItemFunctionManager.Instance.maxFoodsList[ItemFunctionManager.Instance.maxFoodsList.Count - 1].value;
            nowAndMaxFoodText.text = $"({nowFood} / {lastMaxFood})";
        }
        else
        {
            // 현재 레벨의 값 사용
            int currentMaxFood = ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
            nowAndMaxFoodText.text = $"({nowFood} / {currentMaxFood})";
        }
    }

}

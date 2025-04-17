using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// 고양이 스폰 스크립트
[DefaultExecutionOrder(-3)]
public class SpawnManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static SpawnManager Instance { get; private set; }

    [SerializeField] private Button spawnButton;                    // 스폰 버튼
    [SerializeField] private Transform catUIParent;                 // 고양이를 배치할 부모 Transform (UI Panel 등)
    private RectTransform panelRectTransform;                       // Panel의 크기 정보 (배치할 범위)

    [SerializeField] private TextMeshProUGUI nowAndMaxFoodText;     // 스폰버튼 밑에 현재 먹이 갯수와 최대 먹이 갯수 (?/?)
    [SerializeField] private Image foodFillAmountImg;               // 소환 이미지
    [SerializeField] public Image autoFillAmountImg;                // 자동소환 이미지

    [SerializeField] public GameObject effectPrefab;

    // 오브젝트 풀 관련 변수
    private const int POOL_SIZE = 60;                               // 풀 크기
    private Queue<GameObject> catPool = new Queue<GameObject>();    // 고양이 오브젝트 풀
    private List<GameObject> activeCats = new List<GameObject>();   // 활성화된 고양이 목록

    private int nowFood = 5;                                        // 현재 먹이 갯수
    public int NowFood
    {
        get => nowFood;
        set
        {
            nowFood = Mathf.Max(0, value);                          // 먹이 갯수가 0 미만이 되지 않도록 보호
            UpdateFoodText();
        }
    }

    private bool isStoppedReduceCoroutine = false;                  // 코루틴 종료 판별

    private Coroutine createFoodCoroutine;
    private Coroutine autoCollectCoroutine;


    private bool isDataLoaded = false;                              // 데이터 로드 확인

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
        panelRectTransform = catUIParent.GetComponent<RectTransform>();

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            NowFood = 5;
        }

        InitializeObjectPool();
        InitializeSpawnSystem();
        UpdateFoodText();
    }

    #endregion


    #region Initialize

    // 스폰 시스템 초기화 함수
    private void InitializeSpawnSystem()
    {
        createFoodCoroutine = StartCoroutine(CreateFoodTime());
        autoCollectCoroutine = StartCoroutine(AutoCollectingTime());
    }

    #endregion


    #region Object Pool

    // 오브젝트 풀 초기화
    private void InitializeObjectPool()
    {
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject cat = Instantiate(GameManager.Instance.catPrefab, catUIParent);
            cat.SetActive(false);
            catPool.Enqueue(cat);
        }
    }

    // 풀에서 고양이 오브젝트 가져오기
    private GameObject GetCatFromPool()
    {
        if (catPool.Count > 0)
        {
            GameObject cat = catPool.Dequeue();
            cat.SetActive(true);
            activeCats.Add(cat);
            return cat;
        }
        return null;
    }

    // 고양이 오브젝트를 풀에 반환
    public void ReturnCatToPool(GameObject cat)
    {
        if (cat != null)
        {
            cat.SetActive(false);
            activeCats.Remove(cat);
            catPool.Enqueue(cat);
        }
    }

    #endregion


    #region Spawn System

    // 고양이 스폰 버튼 클릭 함수
    public void OnClickedSpawn()
    {
        if (!GameManager.Instance.CanSpawnCat())
        {
            NotificationManager.Instance.ShowNotification("고양이 보유수가 최대입니다!!");
            return;
        }

        if (NowFood <= 0 || NowFood > ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            NotificationManager.Instance.ShowNotification("먹이가 부족합니다!!");
            return;
        }

        NowFood--;
        SpawnBasicCat();
        RestartFoodCoroutineIfStopped();
        GoogleSave();
    }

    // 기본 고양이 스폰 함수
    private void SpawnBasicCat()
    {
        Cat catData = GetCatDataForSpawn();
        GameObject newCat = LoadAndDisplayCats(catData);

        if (newCat != null)
        {
            GameManager.Instance.AddCatCount();
            QuestManager.Instance.AddSpawnCount();
            FriendshipManager.Instance.AddExperience(1, 1);
        }
    }

    // 먹이 생성 코루틴 재시작 함수
    private void RestartFoodCoroutineIfStopped()
    {
        if (!isStoppedReduceCoroutine) return;

        isStoppedReduceCoroutine = false;
        createFoodCoroutine = StartCoroutine(CreateFoodTime());
    }

    // 자동머지 고양이 스폰 함수 (AutoMerge)
    public void SpawnCat()
    {
        if (!GameManager.Instance.CanSpawnCat()) return;

        Cat catData = GetCatDataForSpawn();
        GameObject newCat = LoadAndDisplayCats(catData);

        if (newCat != null)
        {
            SetupCatData(newCat, 0);
            GameManager.Instance.AddCatCount();
            FriendshipManager.Instance.AddExperience(1, 1);
        }
    }

    // 구매한 고양이 스폰 함수 (BuyCat)
    public void SpawnGradeCat(int grade)
    {
        if (!GameManager.Instance.CanSpawnCat()) return;

        Cat catData = GameManager.Instance.AllCatData[grade];
        GameObject newCat = LoadAndDisplayCats(catData);

        if (newCat != null)
        {
            SetupCatData(newCat, grade);
            GameManager.Instance.AddCatCount();
            FriendshipManager.Instance.AddExperience(grade + 1, 1);
        }
    }

    // 고양이 데이터 설정 함수
    private void SetupCatData(GameObject cat, int grade)
    {
        if (cat.TryGetComponent<DragAndDropManager>(out var dragAndDrop))
        {
            dragAndDrop.catData = GameManager.Instance.AllCatData[grade];
            dragAndDrop.UpdateCatUI();
        }
    }

    // 스폰할 고양이 데이터 반환 함수
    private Cat GetCatDataForSpawn()
    {
        int basicCatGrade;
        //basicCatGrade = UnityEngine.Random.Range(0, ItemMenuManager.Instance.maxFoodLv); // ※   1등급 고양이 부터 먹이 레벨 등급까지 소환인데 등급 통일을 좀 해야할듯
        //      어디 함수가면 1등급 고양이는 매개변수에 1 넣어야 하는데
        //      여기는 0을 넣어야지 1등급 고양이가 나옴 

        // 1등급 나오고 싶으면 0 
        // 먹이업글 2를 개방 했을때 최소 2등급 부터 나와야하는데? 2등급이 나오려면 여기서는 1을 넣어야하고 ...

        if (ItemMenuManager.Instance.maxFoodLv >= 15)
        {
            basicCatGrade = UnityEngine.Random.Range(ItemMenuManager.Instance.minFoodLv - 1, ItemMenuManager.Instance.maxFoodLv);
            //Debug.Log($"minFoodLv: {ItemMenuManager.Instance.minFoodLv - 1}");
        }
        else
        {
            basicCatGrade = UnityEngine.Random.Range(0, ItemMenuManager.Instance.maxFoodLv);
        }
        //Debug.Log($"maxFoodLv: {ItemMenuManager.Instance.maxFoodLv}");

        // 이건 나중에 고양이들 많아지면 빼도됌. (예외처리: 현재 마지막고양이보다 값이 높으면 마지막고양이로 대체)
        //Debug.Log($"변경 전: {basicCatGrade}");
        if (GameManager.Instance.AllCatData.Length <= basicCatGrade)
        {
            basicCatGrade = GameManager.Instance.AllCatData.Length - 1;
        }
        //Debug.Log($"변경 후: {basicCatGrade}");

        DictionaryManager.Instance.UnlockCat(basicCatGrade);
        return GameManager.Instance.AllCatData[basicCatGrade];
    }

    // Panel내 랜덤 위치에 고양이 배치하는 함수
    public GameObject LoadAndDisplayCats(Cat catData)
    {
        GameObject catUIObject = GetCatFromPool();
        if (catUIObject == null) return null;

        // CatData 설정
        if (catUIObject.TryGetComponent<CatData>(out var catUIData))
        {
            catUIData.SetCatData(catData);
            catUIData.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
        }

        // 랜덤 위치 설정
        if (catUIObject.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.anchoredPosition = GetRandomPosition();
        }

        RecallEffect(catUIObject);

        return catUIObject;
    }

    // Panel내 랜덤 위치 계산하는 함수
    private Vector2 GetRandomPosition()
    {
        float halfWidth = panelRectTransform.rect.width * 0.5f;
        float halfHeight = panelRectTransform.rect.height * 0.5f;

        return new Vector2(
            UnityEngine.Random.Range(-halfWidth, halfWidth),
            UnityEngine.Random.Range(-halfHeight, halfHeight)
        );
    }

    // 먹이 생성 코루틴
    private IEnumerator CreateFoodTime()
    {
        float elapsed = 0f;
        WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

        while (true)
        {
            int maxFood = (int)ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
            if (NowFood >= maxFood)
            {
                isStoppedReduceCoroutine = true;
                foodFillAmountImg.fillAmount = 1f;
                yield break;
            }

            foodFillAmountImg.fillAmount = 0f;
            float producingTime = ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value;
            while (elapsed < producingTime)
            {
                elapsed += Time.deltaTime;
                foodFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / producingTime);
                yield return waitFrame;
            }

            foodFillAmountImg.fillAmount = 1f;
            NowFood++;
            elapsed = 0f;
            GoogleSave();
        }
    }

    // 자동 스폰 코루틴
    private IEnumerator AutoCollectingTime()
    {
        float elapsed = 0f;
        WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

        while (true)
        {
            // 전투중이면 대기
            if (BattleManager.Instance.IsBattleActive)
            {
                yield return waitFrame;
                continue;
            }

            float autoTime = ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value;
            autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / autoTime);

            // 수집 시간 완료될때까지 대기
            while (elapsed < autoTime)
            {
                if (BattleManager.Instance.IsBattleActive) break;

                elapsed += Time.deltaTime;
                autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / autoTime);
                yield return waitFrame;
            }

            // 완료되면 먹이 줄이고 고양이 생성
            if (NowFood > 0 && !BattleManager.Instance.IsBattleActive && elapsed >= autoTime)
            {
                if (GameManager.Instance.CanSpawnCat())
                {
                    NowFood--;
                    SpawnBasicCat();
                    RestartFoodCoroutineIfStopped();
                    GoogleSave();
                }
                elapsed = 0f;
            }

            yield return waitFrame;
        }
    }

    // 최대 먹이 수량 증가 처리 함수 (ItemMenu)
    public void OnMaxFoodIncreased()
    {
        if (ItemMenuManager.Instance.MaxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count) return;

        int currentMaxFood = (int)ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
        if (NowFood < currentMaxFood && isStoppedReduceCoroutine)
        {
            RestartFoodCoroutineIfStopped();
        }
    }

    // 먹이 텍스트 업데이트 함수
    public void UpdateFoodText()
    {
        int maxFood;

        // 현재 레벨이 최대 레벨보다 크거나 같으면 최대 레벨의 값을 가져옴
        if (ItemMenuManager.Instance.MaxFoodsLv >= ItemFunctionManager.Instance.maxFoodsList.Count)
        {
            maxFood = (int)ItemFunctionManager.Instance.maxFoodsList[ItemFunctionManager.Instance.maxFoodsList.Count - 1].value;
        }
        else
        {
            maxFood = (int)ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
        }

        nowAndMaxFoodText.text = $"({nowFood} / {maxFood})";
    }

    // 구름 이펙트 적용 함수
    public void RecallEffect(GameObject catObj)
    {
        GameObject recallEffect = Instantiate(effectPrefab, catObj.transform.position, Quaternion.identity);
        recallEffect.transform.SetParent(catObj.transform);
        recallEffect.transform.localScale = Vector3.one;
    }

    #endregion


    #region Battle System

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

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public int nowFood;                                                     // 현재 음식
        public List<CatInstanceData> activeCats = new List<CatInstanceData>();  // 활성화된 고양이 데이터
    }

    [Serializable]
    private class CatInstanceData
    {
        public int catGrade;
        public float posX;
        public float posY;
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            nowFood = this.nowFood,
            activeCats = new List<CatInstanceData>()
        };

        // 활성화된 고양이 데이터 저장
        foreach (var cat in activeCats)
        {
            if (cat.TryGetComponent<CatData>(out var catData) &&
                cat.TryGetComponent<RectTransform>(out var rectTransform))
            {
                data.activeCats.Add(new CatInstanceData
                {
                    catGrade = catData.catData.CatGrade,
                    posX = rectTransform.anchoredPosition.x,
                    posY = rectTransform.anchoredPosition.y
                });
            }
        }

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);
        this.nowFood = savedData.nowFood;
        UpdateFoodText();

        // 기존 활성화된 고양이들을 풀로 반환
        foreach (var cat in activeCats.ToArray())
        {
            ReturnCatToPool(cat);
        }
        activeCats.Clear();

        // 저장된 고양이 데이터로 재생성
        foreach (var catData in savedData.activeCats)
        {
            Cat cat = GameManager.Instance.AllCatData[catData.catGrade - 1];
            GameObject newCat = LoadAndDisplayCats(cat);
            if (newCat != null && newCat.TryGetComponent<RectTransform>(out var rectTransform))
            {
                rectTransform.anchoredPosition = new Vector2(catData.posX, catData.posY);
                GameManager.Instance.AddCatCount();
            }
        }

        int maxFood = (int)ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value;
        if (this.nowFood < maxFood)
        {
            if (createFoodCoroutine != null)
            {
                StopCoroutine(createFoodCoroutine);
            }

            isStoppedReduceCoroutine = false;
            createFoodCoroutine = StartCoroutine(CreateFoodTime());
        }

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

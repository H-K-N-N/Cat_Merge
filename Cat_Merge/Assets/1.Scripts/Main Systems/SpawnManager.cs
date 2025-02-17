using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

// 고양이 스폰 Script
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private Button spawnButton;                    // 스폰 버튼

    [SerializeField] private GameObject catPrefab;      // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;     // 고양이를 배치할 부모 Transform (UI Panel 등)
    private RectTransform panelRectTransform;           // Panel의 크기 정보 (배치할 범위)
    private GameManager gameManager;                    // GameManager

    [SerializeField] private TextMeshProUGUI nowAndMaxFoodText;     // 스폰버튼 밑에 현재 먹이 갯수와 최대 먹이 갯수
    private int nowFood = 5;                                        // 현재 먹이 갯수
    private bool isStoppedReduceCoroutine = false;                        // 코루틴 종료 판별
    private bool isStoppedAutoCoroutine = false;

    [SerializeField] private Image foodFillAmountImg;
    [SerializeField] public Image autoFillAmountImg;

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

        StartCoroutine(CreateFoodTime());
        StartCoroutine(AutoCollectingTime());
    }

    private void Update()
    {
        // 현재 먹이 갯수와 최대치 갯수 표시(나중에 함수로 빼둘 것)
        nowAndMaxFoodText.text = $"({nowFood} / {ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value})";
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
            if (nowFood > 0 && nowFood <= ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
            {
                // 먹이가 줄고 코루틴이 종료되어있다면 다시 시작(현재 먹이가 최대치일때 코루틴 종료되어있음)
                nowFood--;
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
            }
            else
            {
                NotificationManager.Instance.ShowNotification("No Food");
                Debug.Log("먹이 갯수 부족");
            }

        }
        else
        {
            NotificationManager.Instance.ShowNotification("Full Cats");
            Debug.Log("고양이 최대 보유 갯수에 도달했습니다!");
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
        while (nowFood < ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
        {
            foodFillAmountImg.fillAmount = 0f; // 정확히 1로 설정
            while (elapsed < ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value)
            {
                elapsed += Time.deltaTime; // 매 프레임마다 경과 시간 증가
                foodFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.reduceProducingFoodTimeList[ItemMenuManager.Instance.ReduceProducingFoodTimeLv].value); // 0 ~ 1 사이로 비율 계산
                yield return null; // 다음 프레임까지 대기
            }
            nowFood++;
            foodFillAmountImg.fillAmount = 1f; // 정확히 1로 설정
            elapsed = 0f;
        }

        // 현재 먹이갯수가 최대치이면 코루틴을 종료시킨다.
        if (nowFood == ItemFunctionManager.Instance.maxFoodsList[ItemMenuManager.Instance.MaxFoodsLv].value)
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
            if (nowFood >= 1)
            {
                autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);

                // 수집 시간 완료될때까지 대기
                while (elapsed < ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    if (BattleManager.Instance.IsBattleActive)
                    {
                        yield return null;
                        break;
                    }

                    elapsed += Time.deltaTime;
                    autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value);
                    yield return null;
                }

                // 완료되면 먹이 줄이고 고양이 생성
                if (!BattleManager.Instance.IsBattleActive && elapsed >= ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
                {
                    nowFood--;
                    SpawnCat();
                    elapsed = 0f; // 진행 상태 초기화
                }
            }

            // 먹이가 0개면 종료
            if (nowFood == 0)
            {
                isStoppedAutoCoroutine = true;
                yield break;
            }

            yield return null;
        }

        //float elapsed = 0f;
        //// 현재 먹이가 1개 이상일때만 코루틴 진행
        //while(nowFood >= 1)
        //{
        //    autoFillAmountImg.fillAmount = 0f;
        //    while (elapsed < ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value)
        //    {
        //        elapsed += Time.deltaTime;
        //        autoFillAmountImg.fillAmount = Mathf.Clamp01(elapsed / ItemFunctionManager.Instance.autoCollectingList[ItemMenuManager.Instance.AutoCollectingLv].value); // 0 ~ 1 사이로 비율 계산
        //        yield return null; // 다음 프레임까지 대기
        //    }
        //    nowFood--;
        //    SpawnCat();
        //    elapsed = 0f;
        //}

        //if(nowFood == 0)
        //{
        //    StopCoroutine(AutoCollectingTime());
        //    isStoppedAutoCoroutine = true;
        //}
    }


}
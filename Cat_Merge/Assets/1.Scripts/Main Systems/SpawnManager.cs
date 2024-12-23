using UnityEngine;

// 고양이 스폰 Script
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;     // 고양이를 배치할 부모 Transform (UI Panel 등)
    private RectTransform panelRectTransform;           // Panel의 크기 정보 (배치할 범위)
    private GameManager gameManager;                    // GameManager

    private void Start()
    {
        gameManager = GameManager.Instance;
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent가 RectTransform을 가지고 있지 않습니다.");
            return;
        }
    }

    // 고양이 스폰 버튼 클릭
    public void OnClickedSpawn()
    {
        if (gameManager.CanSpawnCat())
        {
            // 업그레이드 시스템 확장성을 위해 변경한 고양이 생성 코드
            Cat catData = GetCatDataForSpawn();
            LoadAndDisplayCats(catData);
            QuestManager.Instance.AddFeedCount();
            gameManager.AddCatCount();
        }
        else
        {
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


}

using UnityEngine;

// 고양이 스폰 버튼 관련 스크립트
public class CatSpawn : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;     // 고양이를 배치할 부모 Transform (UI Panel 등)
    private RectTransform panelRectTransform;           // Panel의 크기 정보 (배치할 범위)
    private GameManager gameManager;                    // GameManager

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent가 RectTransform을 가지고 있지 않습니다.");
            return;
        }
    }

    // 고양이 스폰 버튼
    public void OnClickedSpawn()
    {
        if (gameManager.CanSpawnCat())
        {
            LoadAndDisplayCats(gameManager.AllCatData);
            gameManager.AddCatCount();
        }
        else
        {
            Debug.Log("고양이 최대 보유 갯수에 도달했습니다!");
        }
    }

    // Panel내 랜덤 위치 계산
    Vector3 GetRandomPosition(RectTransform panelRectTransform)
    {
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        Vector3 respawnPos = new Vector3(randomX, randomY, 0f);
        return respawnPos;
    }

    // Panel내 랜덤 위치 배치 : 현재 최하 등급 고양이 스폰 (업그레이드 시스템 도입 시 코드 일부 수정 예정)
    private void LoadAndDisplayCats(Cat[] allCatData)
    {
        GameObject catUIObject = Instantiate(catPrefab, catUIParent);

        CatData catData = catUIObject.GetComponent<CatData>();
        catData.SetCatData(allCatData[0]);

        Vector2 randomPos = GetRandomPosition(panelRectTransform);
        catUIObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
    }


}

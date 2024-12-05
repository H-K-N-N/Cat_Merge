using UnityEngine;

public class CatManager : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;     // 고양이를 배치할 부모 Transform (UI Panel 등)
    private int catCount = 10;                          // 배치할 고양이 수

    private void Start()
    {
        CatMerge catMerge = FindObjectOfType<CatMerge>();
        if (catMerge != null)
        {
            LoadAndDisplayCats(catMerge.AllCatData);
        }
        else
        {
            Debug.LogError("CatMerge를 찾을 수 없습니다.");
        }
    }

    private void LoadAndDisplayCats(Cat[] allCatData)
    {
        // Panel의 크기 정보 (배치할 범위)
        RectTransform panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent가 RectTransform을 가지고 있지 않습니다.");
            return;
        }

        for (int i = 0; i < catCount; i++)
        {
            GameObject catUIObject = Instantiate(catPrefab, catUIParent);

            CatData catData = catUIObject.GetComponent<CatData>();
            catData.SetCatData(allCatData[0]);

            Vector2 randomPos = GetRandomPosition(panelRectTransform);
            catUIObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
        }
    }

    // Panel내 랜덤 위치 계산
    private Vector2 GetRandomPosition(RectTransform panelRectTransform)
    {
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        return new Vector2(randomX, randomY);
    }

}

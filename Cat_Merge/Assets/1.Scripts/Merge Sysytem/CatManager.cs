using UnityEngine;

public class CatManager : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;     // 고양이를 배치할 부모 Transform (UI Panel 등)
    private int catCount = 10;                          // 

    private void Start()
    {
        // CatMerge 스크립트를 찾고 allCatData를 가져옴
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
        // Panel의 RectTransform을 가져옴 (배치할 범위)
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

    // 랜덤 위치 계산 함수 (Panel 내에서)
    private Vector2 GetRandomPosition(RectTransform panelRectTransform)
    {
        // Panel의 너비와 높이를 구함
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        // 랜덤한 X, Y 좌표를 계산 (Panel의 범위 내에서)
        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        return new Vector2(randomX, randomY);
    }

}

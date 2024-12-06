using System.Collections;
using UnityEngine;

public class CatSpawn : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // 고양이 UI 프리팹
    [SerializeField] private Transform catUIParent;     // 고양이를 배치할 부모 Transform (UI Panel 등)
    RectTransform panelRectTransform;                   // Panel의 크기 정보 (배치할 범위)

    // 스폰 버튼 누를 시 스폰
    public void Spawn()
    {
        // 고양이 데이터를 들고 있어서 가져옴
        CatMerge catMerge = FindObjectOfType<CatMerge>();
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent가 RectTransform을 가지고 있지 않습니다.");
            return;
        }

        LoadAndDisplayCats(catMerge.AllCatData);
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

    private void LoadAndDisplayCats(Cat[] allCatData)
    {
        // Panel의 크기 정보 (배치할 범위)
        RectTransform panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent가 RectTransform을 가지고 있지 않습니다.");
            return;
        }

        for (int i = 0; i < 1; i++)
        {
            GameObject catUIObject = Instantiate(catPrefab, catUIParent);

            CatData catData = catUIObject.GetComponent<CatData>();
            catData.SetCatData(allCatData[0]);

            Vector2 randomPos = GetRandomPosition(panelRectTransform);
            catUIObject.GetComponent<RectTransform>().anchoredPosition = randomPos;
        }
    }
}

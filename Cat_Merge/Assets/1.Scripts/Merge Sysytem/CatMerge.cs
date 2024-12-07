using UnityEngine;

public class CatMerge : MonoBehaviour
{
    private Cat[] allCatData;               // 모든 고양이 데이터
    public Cat[] AllCatData => allCatData;

    private void Awake()
    {
        LoadAllCats();
    }

    // 고양이 정보 Load
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // 고양이 Merge 함수
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatId != cat2.CatId)
        {
            //Debug.LogWarning("등급이 다름");
            return null;
        }

        Cat nextCat = GetCatById(cat1.CatId + 1);
        if (nextCat != null)
        {
            Debug.Log($"합성 성공 : {nextCat.CatName}");
            return nextCat;
        }
        else
        {
            //Debug.LogWarning("더 높은 등급의 고양이가 없음");
            return null;
        }
    }

    // 고양이 ID 반환 함수
    public Cat GetCatById(int id)
    {
        foreach (Cat cat in allCatData)
        {
            if (cat.CatId == id)
                return cat;
        }
        return null;
    }


}

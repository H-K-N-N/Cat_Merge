using UnityEngine;

// 고양이 머지 Script
public class CatMerge : MonoBehaviour
{
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
            //Debug.Log($"합성 성공 : {nextCat.CatName}");
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
        GameManager gameManager = GameManager.Instance;

        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatId == id)
                return cat;
        }
        return null;
    }


}

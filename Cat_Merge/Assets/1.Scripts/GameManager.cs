using UnityEngine;

// GameManager
public class GameManager : MonoBehaviour
{
    private Cat[] allCatData;               // 모든 고양이 데이터 보유

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


}

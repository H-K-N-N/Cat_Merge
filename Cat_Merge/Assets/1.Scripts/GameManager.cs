using UnityEngine;
using TMPro;

// GameManager
public class GameManager : MonoBehaviour
{
    // Data
    private Cat[] allCatData;                   // 모든 고양이 데이터 보유
    public Cat[] AllCatData => allCatData;

    private int maxCats = 8;                    // 화면 내 최대 고양이 갯수
    private int currentCatCount = 0;            // 화면 내 고양이 갯수

    // UI
    [SerializeField] private TextMeshProUGUI catCountText;

    private void Awake()
    {
        LoadAllCats();
        UpdateCatCountText();
    }

    // 고양이 정보 Load
    private void LoadAllCats()
    {
        allCatData = Resources.LoadAll<Cat>("Cats");
    }

    // 고양이 수 판별
    public bool CanSpawnCat()
    {
        return currentCatCount < maxCats;
    }

    // 현재 고양이 수 증가
    public void AddCatCount()
    {
        if (currentCatCount < maxCats)
        {
            currentCatCount++;
            UpdateCatCountText();
        }
    }

    // 현재 고양이 수 감소
    public void DeleteCatCount()
    {
        if (currentCatCount > 0)
        {
            currentCatCount--;
            UpdateCatCountText();
        }
    }

    // UI 텍스트 업데이트
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

}

using UnityEngine;
using UnityEngine.UI;

public class CatUI : MonoBehaviour
{
    [SerializeField] private Cat catData;           // 고양이 데이터
    private Image catImage;                         // 고양이 이미지

    private void Awake()
    {
        catImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateCatUI();
    }

    public void UpdateCatUI()
    {
        catImage.sprite = catData.CatImage;
    }

    // Cat 데이터 설정 (GameObject가 활성화될 때 외부에서 호출될 수 있음)
    public void SetCatData(Cat cat)
    {
        catData = cat;

        UpdateCatUI();
    }
}

using UnityEngine;
using UnityEngine.UI;

// 고양이 본인들의 정보
public class CatData : MonoBehaviour
{
    private Cat catData;                        // 고양이 데이터
    private Image catImage;                     // 고양이 이미지

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
        CatDragAndDrop catDragAndDrop = GetComponentInParent<CatDragAndDrop>();

        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = catData;
        }
        catImage.sprite = catData.CatImage;
    }

    // Cat 데이터 설정
    public void SetCatData(Cat cat)
    {
        catData = cat;

        UpdateCatUI();
    }


}

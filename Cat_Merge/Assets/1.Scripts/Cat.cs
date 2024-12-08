using UnityEngine;

// Cat Scriptable Object
[CreateAssetMenu(fileName = "CatData", menuName = "ScriptableObjects/Cat", order = 1)]
public class Cat : ScriptableObject
{
    // 고양이 고유 번호
    [SerializeField] private int catId;
    public int CatId { get => catId; set => catId = value; }

    // 고양이 이름
    [SerializeField] private string catName;
    public string CatName { get => catName; set => catName = value; }

    // 고양이 전투력 (임시로 catID * 50)
    [SerializeField] private int catDamage { get => catId * 50; }
    public int CatDamage { get => catDamage; }

    // 고양이 이미지
    [SerializeField] private Sprite catImage;
    public Sprite CatImage { get => catImage; set => catImage = value; }
    
}

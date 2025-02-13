using UnityEngine;

// 고양이의 정보를 담는 Script
[System.Serializable]
public class Cat
{
    private int catId;           // 고양이 인덱스
    public int CatId { get => catId; set => catId = value; }

    private string catName;      // 고양이 이름
    public string CatName { get => catName; set => catName = value; }

    private int catGrade;        // 고양이 등급
    public int CatGrade { get => catGrade; set => catGrade = value; }

    private int catDamage;       // 고양이 전투력
    public int CatDamage { get => catDamage; set => catDamage = value; }

    private int catGetCoin;      // 고양이 자동 재화 획득량
    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

    private int catHp;           // 고양이 체력
    public int CatHp { get => catHp; set => catHp = value; }

    private Sprite catImage;     // 고양이 이미지
    public Sprite CatImage { get => catImage; set => catImage = value; }

    private string catExplain;   // 고양이 설명
    public string CatExplain { get => catExplain; set => catExplain = value; }

    private int catAttackSpeed;  // 고양이 공격속도
    public int CatAttackSpeed { get => catAttackSpeed; set => catAttackSpeed = value; }

    private int catArmor;        // 고양이 방어력
    public int CatArmor { get => catArmor; set => catArmor = value; }

    private int catMoveSpeed;    // 고양이 이동속도
    public int CatMoveSpeed { get => catMoveSpeed; set => catMoveSpeed = value; }

    public Cat(int catId, string catName, int catGrade, int catDamage, int catGetCoin, int catHp, Sprite catImage, 
        string catExplain, int catAttackSpeed, int catArmor, int catMoveSpeed)
    {
        CatId = catId;
        CatName = catName;
        CatGrade = catGrade;
        CatDamage = catDamage;
        CatGetCoin = catGetCoin;
        CatHp = catHp;
        CatImage = catImage;
        CatExplain = catExplain;
        CatAttackSpeed = catAttackSpeed;
        CatArmor = catArmor;
        CatMoveSpeed = catMoveSpeed;
    }
}
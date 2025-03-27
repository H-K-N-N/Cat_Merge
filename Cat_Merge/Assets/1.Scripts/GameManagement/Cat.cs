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



    private int baseDamage;      // 기본 공격력
    public int BaseDamage { get => baseDamage; set => baseDamage = value; }

    private int baseHp;          // 기본 체력
    public int BaseHp { get => baseHp; set => baseHp = value; }

    private int growthDamage;    // 성장한 공격력
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;        // 성장한 체력
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    // 최종 스탯 계산용 프로퍼티
    public int CatDamage => (int)(GrowthDamage * (BaseDamage * 0.01)) + GrowthDamage; 
    public int CatHp => (int)(GrowthHp * (BaseHp * 0.01)) + GrowthHp;



    private int catGetCoin;      // 고양이 자동 재화 획득량
    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

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
        BaseDamage = catDamage;
        BaseHp = catHp;
        GrowthDamage = 0;
        GrowthHp = 0;
        CatGetCoin = catGetCoin;
        CatImage = catImage;
        CatExplain = catExplain;
        CatAttackSpeed = catAttackSpeed;
        CatArmor = catArmor;
        CatMoveSpeed = catMoveSpeed;
    }

    // 스탯 성장 메서드
    public void GrowStat(int addDamage, int addHp)
    {
        GrowthDamage += addDamage;
        GrowthHp += addHp;
    }

    // 스탯 초기화 메서드
    public void ResetGrowth()
    {
        GrowthDamage = 0;
        GrowthHp = 0;
    }

}

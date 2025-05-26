using UnityEngine;

// 고양이의 정보를 담는 스크립트
[System.Serializable]
public class Cat
{


    #region Basic Properties

    private int catId;          // 고양이 인덱스
    public int CatId { get => catId; set => catId = value; }

    private string catName;     // 고양이 이름
    public string CatName { get => catName; set => catName = value; }

    private int catGrade;       // 고양이 등급
    public int CatGrade { get => catGrade; set => catGrade = value; }

    #endregion


    #region Combat Stats

    private int baseDamage;     // 기본 공격력 (기본값 100%)
    public int BaseDamage { get => baseDamage; set => baseDamage = value; }

    private int baseHp;         // 기본 체력
    public int BaseHp { get => baseHp; set => baseHp = value; }

    private int growthDamage;   // 성장한 공격력
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;       // 성장한 체력
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    private int catAttackSpeed;     // 고양이 공격속도
    public int CatAttackSpeed { get => catAttackSpeed; set => catAttackSpeed = value; }

    private int catArmor;           // 고양이 방어력
    public int CatArmor { get => catArmor; set => catArmor = value; }

    private int catMoveSpeed;       // 고양이 이동속도
    public int CatMoveSpeed { get => catMoveSpeed; set => catMoveSpeed = value; }

    #endregion


    #region Passive Stats

    private float passiveAttackDamage = 1.0f;       // 패시브 공격력 증가 배율 (기본값 1 = 100%)
    public float PassiveAttackDamage { get => passiveAttackDamage; set => passiveAttackDamage = value; }

    private float passiveCoinCollectSpeed = 0f;     // 패시브 재화 수집 속도 증가량 (기본값 0초)
    public float PassiveCoinCollectSpeed { get => passiveCoinCollectSpeed; set => passiveCoinCollectSpeed = value; }

    private float passiveAttackSpeed = 0f;          // 패시브 공격 속도 증가량 (기본값 0초)
    public float PassiveAttackSpeed { get => passiveAttackSpeed; set => passiveAttackSpeed = value; }

    #endregion


    #region Calculated Stats

    // 최종 스탯 계산
    public int CatDamage => (int)((GrowthDamage * (BaseDamage * 0.01) + GrowthDamage) * PassiveAttackDamage);
    public int CatHp => (int)(GrowthHp * (BaseHp * 0.01)) + GrowthHp;

    #endregion


    #region Additional Properties

    private int catGetCoin;         // 고양이 자동 재화 획득량
    public int CatGetCoin { get => catGetCoin; set => catGetCoin = value; }

    private Sprite catImage;        // 고양이 이미지
    public Sprite CatImage { get => catImage; set => catImage = value; }

    private string catExplain;      // 고양이 설명
    public string CatExplain { get => catExplain; set => catExplain = value; }

    private int canOpener;          // 고양이 구매 해금관련
    public int CanOpener { get => canOpener; set => canOpener = value; }

    private int catFirstOpenCash;   // 고양이 첫 획득시 얻는 다이아
    public int CatFirstOpenCash { get => catFirstOpenCash; set => catFirstOpenCash = value; }

    #endregion


    #region Constructor

    // 고양이 객체 생성자
    public Cat(int catId, string catName, int catGrade, int catDamage, int catGetCoin, int catHp, Sprite catImage,
        string catExplain, int catAttackSpeed, int catArmor, int catMoveSpeed, int canOpener, int catFirstOpenCash)
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
        CanOpener = canOpener;
        CatFirstOpenCash = catFirstOpenCash;
    }

    #endregion


    #region Growth Methods

    // 성장 스탯 증가 함수
    public void GrowStat(int addDamage, int addHp)
    {
        GrowthDamage += addDamage;
        GrowthHp += addHp;
    }

    // 성장 스탯 초기화 함수
    public void ResetGrowth()
    {
        GrowthDamage = 0;
        GrowthHp = 0;
    }

    #endregion


    #region Passive Buff Methods

    // 패시브 공격력 증가 함수
    public void AddPassiveAttackDamageBuff(float percentage)
    {
        PassiveAttackDamage += percentage;
    }

    // 패시브 공격력 증가 초기화 함수
    public void ResetPassiveAttackDamageBuff()
    {
        PassiveAttackDamage = 1f;
    }


    // 패시브 재화 수집 속도 증가 함수
    public void AddPassiveCoinCollectSpeedBuff(float seconds)
    {
        PassiveCoinCollectSpeed += seconds;
    }

    // 패시브 공격력 증가 초기화 함수
    public void ResetPassiveCoinCollectSpeedBuff()
    {
        PassiveCoinCollectSpeed = 0f;
    }

    // 패시브 공격 속도 증가 함수
    public void AddPassiveAttackSpeedBuff(float seconds)
    {
        PassiveAttackSpeed += seconds;
    }

    // 패시브 공격 증가 초기화 함수
    public void ResetPassiveAttackSpeedBuff()
    {
        PassiveAttackSpeed = 0f;
    }

    #endregion


}

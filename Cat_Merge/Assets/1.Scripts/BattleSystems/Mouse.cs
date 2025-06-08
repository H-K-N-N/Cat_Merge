using UnityEngine;

// 쥐(보스)의 정보를 담는 스크립트
[System.Serializable]
public class Mouse
{


    #region Mouse Stats

    private int mouseId;                // 쥐 인덱스
    public int MouseId { get => mouseId; set => mouseId = value; }

    private string mouseName;           // 쥐 이름
    public string MouseName { get => mouseName; set => mouseName = value; }

    private int mouseGrade;             // 쥐 등급 (스테이지)
    public int MouseGrade { get => mouseGrade; set => mouseGrade = value; }

    private double mouseDamage;         // 쥐 전투력
    public double MouseDamage { get => mouseDamage; set => mouseDamage = value; }

    private double mouseHp;             // 쥐 체력
    public double MouseHp { get => mouseHp; set => mouseHp = value; }

    private Sprite mouseImage;          // 쥐 이미지
    public Sprite MouseImage { get => mouseImage; set => mouseImage = value; }

    private int numOfAttack;            // 공격 타겟 수
    public int NumOfAttack { get => numOfAttack; set => numOfAttack = value; }

    private int mouseAttackSpeed;       // 쥐 공격속도
    public int MouseAttackSpeed { get => mouseAttackSpeed; set => mouseAttackSpeed = value; }

    private int mouseArmor;             // 쥐 방어력
    public int MouseArmor { get => mouseArmor; set => mouseArmor = value; }

    #endregion


    #region Rewards

    private int clearCashReward;            // 첫 클리어 캐쉬 보상
    public int ClearCashReward { get => clearCashReward; set => clearCashReward = value; }

    private decimal clearCoinReward;        // 첫 클리어 코인 보상
    public decimal ClearCoinReward { get => clearCoinReward; set => clearCoinReward = value; }

    private decimal repeatclearCoinReward;  // 반복 클리어 코인 보상
    public decimal RepeatclearCoinReward { get => repeatclearCoinReward; set => repeatclearCoinReward = value; }

    #endregion


    #region Constructor

    // 쥐 데이터 초기화 생성자
    public Mouse(int mouseId, string mouseName, int mouseGrade, double mouseDamage, double mouseHp,
                Sprite mouseImage, int numOfAttack, int mouseAttackSpeed, int mouseArmor,
                int clearCashReward, decimal clearCoinReward, decimal repeatclearCoinReward)
    {
        MouseId = mouseId;
        MouseName = mouseName;
        MouseGrade = mouseGrade;
        MouseDamage = mouseDamage;
        MouseHp = mouseHp;
        MouseImage = mouseImage;
        NumOfAttack = numOfAttack;
        MouseAttackSpeed = mouseAttackSpeed;
        MouseArmor = mouseArmor;
        ClearCashReward = clearCashReward;
        ClearCoinReward = clearCoinReward;
        RepeatclearCoinReward = repeatclearCoinReward;
    }

    #endregion


}

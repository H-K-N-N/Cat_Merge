using UnityEngine;

// ÁãÀÇ Á¤º¸¸¦ ´ã´Â Script
[System.Serializable]
public class Mouse : MonoBehaviour
{
    private int mouseId;                // Áã ÀÎµ¦½º
    public int MouseId { get => mouseId; set => mouseId = value; }

    private string mouseName;           // Áã ÀÌ¸§
    public string MouseName { get => mouseName; set => mouseName = value; }

    private int mouseGrade;             // Áã µî±Þ (½ºÅ×ÀÌÁö)
    public int MouseGrade { get => mouseGrade; set => mouseGrade = value; }

    private float mouseDamage;            // Áã ÀüÅõ·Â
    public float MouseDamage { get => mouseDamage; set => mouseDamage = value; }

    private float mouseHp;                // Áã Ã¼·Â
    public float MouseHp { get => mouseHp; set => mouseHp = value; }

    private Sprite mouseImage;          // Áã ÀÌ¹ÌÁö
    public Sprite MouseImage { get => mouseImage; set => mouseImage = value; }

    private int numOfAttack;            // °ø°Ý Å¸°Ù ¼ö
    public int NumOfAttack { get => numOfAttack; set => numOfAttack = value; }

    public Mouse(int mouseId, string mouseName, int mouseGrade, float mouseDamage, float mouseHp, Sprite mouseImage, int numOfAttack)
    {
        MouseId = mouseId;
        MouseName = mouseName;
        MouseGrade = mouseGrade;
        MouseDamage = mouseDamage;
        MouseHp = mouseHp;
        MouseImage = mouseImage;
        NumOfAttack = numOfAttack;
    }

}

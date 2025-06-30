using UnityEngine;
using TMPro;

// 데미지 텍스트 오브젝트 스크립트
public class DamageTextObject : MonoBehaviour
{


    #region Variables

    public TextMeshProUGUI text;
    public RectTransform rectTransform;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    #endregion


}

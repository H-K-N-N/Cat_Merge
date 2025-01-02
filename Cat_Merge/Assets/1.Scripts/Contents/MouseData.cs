using UnityEngine;
using UnityEngine.UI;

// 객체가 가지고있는 쥐의 정보를 담는 Script
public class MouseData : MonoBehaviour
{
    public Mouse mouseData;                     // 쥐 데이터
    private Image mouseImage;                   // 쥐 이미지

    private RectTransform rectTransform;        // RectTransform 참조
    private RectTransform parentPanel;          // 부모 패널 RectTransform

    // ======================================================================================================================

    private void Awake()
    {
        mouseImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        parentPanel = rectTransform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateMouseUI(); 
    }

    // ======================================================================================================================

    // MouseUI 최신화하는 함수
    public void UpdateMouseUI()
    {
        mouseImage.sprite = mouseData.MouseImage;
    }

    // Mouse 데이터 설정 함수
    public void SetMouseData(Mouse mouse)
    {
        mouseData = mouse;
        UpdateMouseUI();
    }

    // ======================================================================================================================
    // [전투관련]
    // Mouse Prefab에 MouseData, BossHitbox 스크립트 포함


    // [보스 행동]
    // 히트박스 내 존재하는 고양이 중 N마리 공격하기 (애니메이션은 나중에 추가할 예정)

    // HP가 다 닳으면 사망

    // 


}

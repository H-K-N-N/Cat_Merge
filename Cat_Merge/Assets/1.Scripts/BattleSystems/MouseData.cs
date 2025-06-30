using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// 쥐(보스)의 정보와 행동을 관리하는 스크립트
public class MouseData : MonoBehaviour
{


    #region Variables

    [HideInInspector] public Mouse mouseData;   // 쥐 데이터
    private Image mouseImage;                   // 쥐 이미지
    private RectTransform rectTransform;        // RectTransform 참조

    [Header("---[Damage Text]")]
    [SerializeField] private GameObject damageTextPrefab;   // 데미지 텍스트 프리펩
    private Queue<DamageTextObject> damageTextPool;         // 데미지 텍스트 오브젝트 풀
    private const int POOL_SIZE = 200;                      // 풀 사이즈

    private const float DAMAGE_TEXT_START_Y = 200f;         // 데미지 텍스트 시작 Y 위치
    private const float DAMAGE_TEXT_MOVE_DISTANCE = 50f;    // 데미지 텍스트 이동 거리
    private const float DAMAGE_TEXT_DURATION = 1f;          // 데미지 텍스트 지속 시간

    private readonly Vector3 damageTextMoveOffset = Vector3.up * DAMAGE_TEXT_MOVE_DISTANCE; // 데미지 텍스트 실제 이동 거리

    #endregion


    #region Unity Methods

    private void Awake()
    {
        mouseImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        InitializeDamageTextPool();
    }

    private void Start()
    {
        UpdateMouseUI();
    }

    #endregion


    #region Pool Management

    // 데미지 텍스트 풀 초기화 함수
    private void InitializeDamageTextPool()
    {
        damageTextPool = new Queue<DamageTextObject>();
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject damageTextObj = Instantiate(damageTextPrefab, transform);
            var damageTextComponent = damageTextObj.GetComponent<DamageTextObject>();
            damageTextObj.SetActive(false);
            damageTextPool.Enqueue(damageTextComponent);
        }
    }

    #endregion


    #region Mouse Management

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

    #endregion


    #region Damage Text Management

    // 데미지 텍스트 생성 함수
    public void ShowDamageText(float damage)
    {
        DamageTextObject damageTextObj;
        if (damageTextPool.Count > 0)
        {
            damageTextObj = damageTextPool.Dequeue();
        }
        else
        {
            // 풀이 비어있으면 가장 오래된 텍스트를 재활용
            damageTextObj = transform.GetChild(0).GetComponent<DamageTextObject>();
            transform.GetChild(0).SetSiblingIndex(transform.childCount - 1);
        }

        damageTextObj.gameObject.SetActive(true);

        Vector3 startPosition = rectTransform.anchoredPosition;
        startPosition.y = DAMAGE_TEXT_START_Y;
        damageTextObj.rectTransform.anchoredPosition = startPosition;

        damageTextObj.text.text = GameManager.Instance.FormatNumber((decimal)damage);

        StartCoroutine(AnimateDamageText(damageTextObj));
    }

    // 데미지 텍스트 애니메이션 코루틴
    private IEnumerator AnimateDamageText(DamageTextObject textObj)
    {
        float elapsedTime = 0f;
        Vector3 startPos = textObj.rectTransform.anchoredPosition;
        Vector3 endPos = startPos + damageTextMoveOffset;

        Color originalColor = textObj.text.color;

        while (elapsedTime < DAMAGE_TEXT_DURATION)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / DAMAGE_TEXT_DURATION;

            textObj.rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, progress);

            Color newColor = originalColor;
            newColor.a = 1 - progress;
            textObj.text.color = newColor;

            yield return null;
        }

        textObj.gameObject.SetActive(false);
        damageTextPool.Enqueue(textObj);
    }

    #endregion


}

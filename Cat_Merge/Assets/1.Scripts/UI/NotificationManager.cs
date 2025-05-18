using UnityEngine;
using TMPro;
using System.Collections;

// 알림(예: 재화가 부족합니다!!) 스크립트
public class NotificationManager : MonoBehaviour
{


    #region Variables

    public static NotificationManager Instance { get; private set; }

    [Header("---[UI Components]")]
    [SerializeField] private GameObject notificationPanel;              // Notification Panel
    [SerializeField] private RectTransform panelTransform;              // Panel RectTransform
    [SerializeField] private TextMeshProUGUI notificationText;          // Text (TMP)
    [SerializeField] private CanvasGroup canvasGroup;                   // CanvasGroup for fading

    [Header("---[Position Settings]")]
    private readonly Vector2 startPosition = new Vector2(0, 144);       // Panel 시작위치
    private readonly Vector2 endPosition = new Vector2(0, 96);          // Panel 도착위치

    [Header("---[Animation Settings]")]
    private readonly WaitForSeconds waitNotificationDuration = new WaitForSeconds(0.3f);    // 알림 표시 대기 시간
    private Coroutine currentCoroutine;                         // 현재 실행 중인 코루틴

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        notificationPanel.SetActive(false);
        canvasGroup.alpha = 0;
    }

    #endregion


    #region Notification Control

    // 알림 표시 함수 (string 메시지)
    public void ShowNotification(string message)
    {
        notificationText.text = message;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(NotificationRoutine());
    }

    // 알림 패널 애니메이션 코루틴
    private IEnumerator NotificationRoutine()
    {
        // 알림 패널 활성화
        notificationPanel.SetActive(true);
        panelTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 1;

        // 패널 이동
        float elapsedTime = 0f;
        float moveDuration = 0.2f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            panelTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        // 도착 위치에서 유지
        yield return waitNotificationDuration;

        // 패널 투명화
        elapsedTime = 0f;
        float fadeDuration = 0.5f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }

        // 알림 패널 비활성화
        notificationPanel.SetActive(false);
        currentCoroutine = null;
    }

    #endregion


}

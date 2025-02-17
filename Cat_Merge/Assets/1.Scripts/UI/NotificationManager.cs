using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject notificationPanel;      // Notification Panel
    [SerializeField] private RectTransform panelTransform;      // Panel RectTransform
    [SerializeField] private TextMeshProUGUI notificationText;  // Text (TMP)
    [SerializeField] private CanvasGroup canvasGroup;           // CanvasGroup for fading

    private Vector2 startPosition = new Vector2(0, 144);        // Panel 시작위치
    private Vector2 endPosition = new Vector2(0, 96);           // Panel 도착위치
    private Coroutine currentCoroutine;                         // 현재 실행 중인 코루틴

    // ======================================================================================================================================================================

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

    // ======================================================================================================================================================================

    /// <summary> 알림 표시 함수 (string 메시지) </summary>
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
        yield return new WaitForSeconds(0.3f);

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
}


/*
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject notificationPanel;      // Notification Panel
    [SerializeField] private RectTransform panelTransform;      // Panel RectTransform
    [SerializeField] private TextMeshProUGUI notificationText;  // Text (TMP)
    [SerializeField] private CanvasGroup canvasGroup;           // CanvasGroup for fading

    private Vector2 startPosition = new Vector2(0, 144);        // Panel 시작위치
    private Vector2 endPosition = new Vector2(0, 96);           // Panel 도착위치
    private Coroutine currentCoroutine;                         // 현재 실행 중인 코루틴
    private string currentMessage;                              // 현재 표시 중인 메시지

    // 초기화 시 패널을 비활성화
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

    /// <summary> 알림 표시 함수 (string 메시지) </summary>
    public void ShowNotification(string message)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        if (message == currentMessage)
        {
            currentCoroutine = StartCoroutine(NotificationRoutine(false));
        }
        else
        {
            notificationText.text = message;
            currentMessage = message;
            currentCoroutine = StartCoroutine(NotificationRoutine(true));
        }
    }

    private IEnumerator NotificationRoutine(bool includeMoveAnimation)
    {
        // 알림 패널 활성화
        notificationPanel.SetActive(true);
        panelTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 1;

        // 패널 이동
        float elapsedTime = 0f;
        float moveDuration = 0.2f;
        if (includeMoveAnimation)
        {
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveDuration;
                panelTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
                yield return null;
            }
        }
        else
        {
            panelTransform.anchoredPosition = endPosition;
        }

        // 도착 위치에서 유지
        yield return new WaitForSeconds(0.3f);

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
        currentMessage = null;
    }
}
*/
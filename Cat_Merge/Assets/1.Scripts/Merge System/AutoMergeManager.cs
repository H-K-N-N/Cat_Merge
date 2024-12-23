using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

// 자동 머지 Script
public class AutoMergeManager : MonoBehaviour
{
    [Header("---[AutoMerge System]")]
    [SerializeField] private Button openAutoMergePanelButton;       // 자동 머지 패널 열기 버튼
    [SerializeField] private GameObject autoMergePanel;             // 자동 머지 패널
    [SerializeField] private Button closeAutoMergePanelButton;      // 자동 머지 패널 닫기 버튼
    [SerializeField] private Button autoMergeStateButton;           // 자동 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // 자동 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // 자동 머지 타이머 텍스트
    private int autoMergeCost = 30;

    // ======================================================================================================================

    private float startTime;                            // 자동 머지 시작 시간
    private float autoMergeDuration = 10.0f;            // 자동 머지 기본 지속 시간
    private float currentAutoMergeDuration;             // 현재 자동 머지 지속 시간
    private float plusAutoMergeDuration;                // 자동 머지 추가 시간
    private float autoMergeInterval = 0.5f;             // 자동 머지 간격
    private float moveDuration = 0.2f;                  // 고양이가 이동하는 데 걸리는 시간 (이동 속도)
    private bool isAutoMergeActive = false;             // 자동 머지 활성화 상태

    private HashSet<CatDragAndDrop> mergingCats;        // 머지 중인 고양이 추적

    // ======================================================================================================================

    private void Awake()
    {
        plusAutoMergeDuration = autoMergeDuration;
        currentAutoMergeDuration = autoMergeDuration;

        UpdateAutoMergeCostText();
        openAutoMergePanelButton.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton.onClick.AddListener(StartAutoMerge);
        UpdateAutoMergeTimerVisibility(false);
    }

    private void Start()
    {
        mergingCats = new HashSet<CatDragAndDrop>();
    }

    private void Update()
    {
        if (isAutoMergeActive)
        {
            // 남은 시간 계산
            float remainingTime = currentAutoMergeDuration - (Time.time - startTime);
            remainingTime = Mathf.Max(remainingTime, 0);

            // 타이머 업데이트
            UpdateAutoMergeTimerText((int)remainingTime);

            // 자동 머지 종료 처리
            if (remainingTime <= 0)
            {
                isAutoMergeActive = false;
                UpdateAutoMergeTimerVisibility(false);
            }
        }
    }

    // ======================================================================================================================
    // ---[자동머지 UI]

    // 자동머지 비용 Text 업데이트 함수
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = $"{autoMergeCost}";
        }
    }

    // 자동머지 패널 여는 함수
    private void OpenAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(true);
        }
    }

    // 자동머지 패널 닫는 함수
    private void CloseAutoMergePanel()
    {
        if (autoMergePanel != null)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // 자동머지 시작 함수
    private void StartAutoMerge()
    {
        if (GameManager.Instance.Cash >= autoMergeCost)
        {
            GameManager.Instance.Cash -= autoMergeCost;

            AutoMergeManager autoMergeScript = FindObjectOfType<AutoMergeManager>();
            if (autoMergeScript != null)
            {
                autoMergeScript.OnClickedAutoMerge();
            }
        }
        else
        {
            Debug.Log("Not enough coins to start AutoMerge!");
        }
    }

    // 자동머지 상태에 따라 타이머 텍스트 가시성 업데이트 함수
    public void UpdateAutoMergeTimerVisibility(bool isVisible)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.gameObject.SetActive(isVisible);
        }
    }

    // 자동머지 타이머 업데이트 함수
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}";
        }
    }

    // ======================================================================================================================
    // ---[자동머지 기능]

    // 자동 머지 시작 함수
    public void OnClickedAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            Debug.Log("자동 머지 시작");
            startTime = Time.time;
            isAutoMergeActive = true;
            currentAutoMergeDuration = autoMergeDuration;
            UpdateAutoMergeTimerVisibility(true);
            StartCoroutine(AutoMergeCoroutine());
        }
        else
        {
            Debug.Log($"{plusAutoMergeDuration}초 추가");
            currentAutoMergeDuration += plusAutoMergeDuration;
        }
    }

    // 자동 머지 중인지 확인하는 함수
    public bool IsMerging(CatDragAndDrop cat)
    {
        return mergingCats.Contains(cat);
    }

    // 자동 머지 코루틴
    private IEnumerator AutoMergeCoroutine()
    {
        // 최대 고양이 생성 유지 루프
        StartCoroutine(SpawnCatsWhileAutoMerge());

        while (Time.time - startTime - currentAutoMergeDuration < 0)
        {
            // mergingCats 상태 정리
            mergingCats.RemoveWhere(cat => cat == null || cat.isDragging);

            var allCats = FindObjectsOfType<CatDragAndDrop>().OrderBy(cat => cat.catData.CatGrade).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatGrade).Where(group => group.Count() > 1).ToList();

            if (!groupedCats.Any())
            {
                yield return new WaitForSeconds(autoMergeInterval);
                continue;
            }

            foreach (var group in groupedCats)
            {
                var catsInGroup = group.ToList();

                while (catsInGroup.Count >= 2 && (Time.time - startTime - currentAutoMergeDuration < 0))
                {
                    var cat1 = catsInGroup[0];
                    var cat2 = catsInGroup[1];

                    // 최고 등급 고양이인지 확인
                    if (IsMaxLevelCat(cat1.catData) || IsMaxLevelCat(cat2.catData))
                    {
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    // 고양이 상태 확인
                    if (cat1 == null || cat2 == null || cat1.isDragging || cat2.isDragging ||
                        mergingCats.Contains(cat1) || mergingCats.Contains(cat2))
                    {
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }
                    mergingCats.Add(cat1);
                    mergingCats.Add(cat2);

                    RectTransform parentRect = cat1.rectTransform?.parent?.GetComponent<RectTransform>();
                    if (parentRect == null)
                    {
                        mergingCats.Remove(cat1);
                        mergingCats.Remove(cat2);
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    Vector2 mergePosition = GetRandomPosition(parentRect);

                    StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                    StartCoroutine(MoveCatSmoothly(cat2, mergePosition));

                    yield return new WaitUntil(() =>
                        cat1 == null || cat2 == null ||
                        (!mergingCats.Contains(cat1) && !mergingCats.Contains(cat2)) ||
                        (cat1.rectTransform.anchoredPosition == mergePosition && cat2.rectTransform.anchoredPosition == mergePosition)
                    );

                    if (cat1 != null && cat2 != null && mergingCats.Contains(cat1) && mergingCats.Contains(cat2))
                    {
                        Cat mergedCat = FindObjectOfType<CatMerge>().MergeCats(cat1.catData, cat2.catData);
                        if (mergedCat != null)
                        {
                            cat1.catData = mergedCat;
                            cat1.UpdateCatUI();
                            Destroy(cat2.gameObject);
                        }
                    }

                    mergingCats.Remove(cat1);
                    mergingCats.Remove(cat2);

                    catsInGroup.Remove(cat1);
                    catsInGroup.Remove(cat2);

                    yield return new WaitForSeconds(autoMergeInterval);
                }

                if (Time.time - startTime - currentAutoMergeDuration >= 0) break;
            }

            yield return null;
        }

        isAutoMergeActive = false;
        UpdateAutoMergeTimerVisibility(false);
        Debug.Log("자동 머지 종료");
    }

    // 자동머지중 고양이 최대치로 소환하는 함수
    private IEnumerator SpawnCatsWhileAutoMerge()
    {
        CatSpawn catSpawn = FindObjectOfType<CatSpawn>();
        while (isAutoMergeActive)
        {
            while (GameManager.Instance.CanSpawnCat())
            {
                catSpawn.SpawnCat();
                yield return new WaitForSeconds(0.1f); // 고양이 자동 생성 간격
            }

            yield return null;
        }
    }

    // 부모 RectTransform에서 랜덤 위치 계산 함수
    private Vector2 GetRandomPosition(RectTransform parentRect)
    {
        float panelWidth = parentRect.rect.width;
        float panelHeight = parentRect.rect.height;
        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);
        return new Vector2(randomX, randomY);
    }

    // 고양이가 부드럽게 이동하는 코루틴
    private IEnumerator MoveCatSmoothly(CatDragAndDrop cat, Vector2 targetPosition)
    {
        if (cat == null) yield break;

        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            if (cat == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }
    }

    // 최대 레벨 고양이 확인 함수
    private bool IsMaxLevelCat(Cat catData)
    {
        if (GameManager.Instance == null || GameManager.Instance.AllCatData == null)
        {
            return false;
        }

        return GameManager.Instance.AllCatData.All(cat => cat.CatGrade != catData.CatGrade + 1);
    }

    // 특정 고양이의 mergingCats 상태 제거 함수
    public void StopMerging(CatDragAndDrop cat)
    {
        mergingCats.Remove(cat);
    }


}

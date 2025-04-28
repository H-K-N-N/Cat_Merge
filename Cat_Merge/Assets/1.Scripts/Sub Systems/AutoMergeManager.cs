using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

// 자동머지 관련 스크립트
public class AutoMergeManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static AutoMergeManager Instance { get; private set; }

    [Header("---[UI]")]
    [SerializeField] private GameObject autoMergePanel;             // 자동 머지 패널
    [SerializeField] private Button openAutoMergePanelButton;       // 자동 머지 패널 열기 버튼
    [SerializeField] private Button closeAutoMergePanelButton;      // 자동 머지 패널 닫기 버튼
    [SerializeField] private Button autoMergeStateButton;           // 자동 머지 상태 버튼
    [SerializeField] private TextMeshProUGUI autoMergeCostText;     // 자동 머지 비용 텍스트
    [SerializeField] private TextMeshProUGUI autoMergeTimerText;    // 자동 머지 타이머 텍스트
    [SerializeField] private TextMeshProUGUI explainText;           // 자동 머지 설명 텍스트

    [Header("---[Auto Merge Settings]")]
    private const float MAX_AUTO_MERGE_DURATION = 86400f;                       // 최대 자동 머지 시간 (24시간)
    private const float MOVE_DURATION = 0.3f;                                   // 고양이가 이동하는 데 걸리는 시간 (이동 속도)
    private const float AUTO_MERGE_DURATION = 10.0f;                            // 자동 머지 기본 지속 시간
    private const int AUTO_MERGE_COST = 30;                                     // 자동 머지 비용
    private WaitForSeconds waitAutoMergeInterval = new WaitForSeconds(0.5f);    // 자동 머지 간격
    private WaitForSeconds waitSpawnInterval = new WaitForSeconds(0.1f);        // 소환 간격

    private float startTime;                        // 자동 머지 시작 시간
    private float currentAutoMergeDuration;         // 현재 자동 머지 지속 시간
    private bool isAutoMergeActive = false;         // 자동 머지 활성화 상태
    private bool isPaused = false;                  // 일시정지 상태
    private float pausedTimeRemaining = 0f;         // 일시정지 시점의 남은 시간
    private Coroutine autoMergeCoroutine;           // 자동 머지 코루틴

    private float panelWidth;           // Panel Width
    private float panelHeight;          // Panel Height
    private Vector2 panelHalfSize;      // Panel Size / 2

    private HashSet<DragAndDropManager> mergingCats = new HashSet<DragAndDropManager>();


    private bool isDataLoaded = false;          // 데이터 로드 확인

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
    }

    private void Start()
    {
        InitializeAutoMergeManager();

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            InitializeDefaultValues();
        }
    }

    private void Update()
    {
        if (!isAutoMergeActive && !isPaused) return;

        float remainingTime = isPaused ? pausedTimeRemaining : Mathf.Max(currentAutoMergeDuration - (Time.time - startTime), 0);
        UpdateTimerDisplay((int)remainingTime);

        if (!isPaused && remainingTime <= 0)
        {
            EndAutoMerge();
        }
    }

    #endregion


    #region Initialize

    // 기본값 초기화 함수
    private void InitializeDefaultValues()
    {
        isAutoMergeActive = false;
        isPaused = false;
        currentAutoMergeDuration = 0f;
        UpdateAutoMergeTimerVisibility(false);
        UpdateExplainText(0);
    }

    // 컴포넌트 캐싱 및 UI 초기화
    private void InitializeAutoMergeManager()
    {
        // UI 초기화
        UpdateAutoMergeCostText();

        // 버튼 이벤트 리스너 설정
        InitializeButtonListeners();

        // 패널 크기 캐싱
        CachePanelSize();
    }

    // 패널 크기 캐싱 함수
    private void CachePanelSize()
    {
        DragAndDropManager anyActiveCat = FindObjectOfType<DragAndDropManager>();
        if (anyActiveCat != null)
        {
            RectTransform parentRect = anyActiveCat.rectTransform?.parent?.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                panelWidth = parentRect.rect.width;
                panelHeight = parentRect.rect.height;
                panelHalfSize = new Vector2(panelWidth / 2, panelHeight / 2);
            }
        }
    }

    // UI 버튼 이벤트 리스너 설정 함수
    private void InitializeButtonListeners()
    {
        openAutoMergePanelButton?.onClick.AddListener(OpenAutoMergePanel);
        closeAutoMergePanelButton?.onClick.AddListener(CloseAutoMergePanel);
        autoMergeStateButton?.onClick.AddListener(StartAutoMerge);
    }

    #endregion
    

    #region Auto Merge System

    // 자동머지 시작 함수
    public void StartAutoMerge()
    {
        if (GameManager.Instance.Cash < AUTO_MERGE_COST)
        {
            NotificationManager.Instance.ShowNotification("재화가 부족합니다!!");
            return;
        }

        if (currentAutoMergeDuration + AUTO_MERGE_DURATION > MAX_AUTO_MERGE_DURATION)
        {
            NotificationManager.Instance.ShowNotification("자동머지는 최대 24시간까지 가능합니다!!");
            return;
        }

        GameManager.Instance.Cash -= AUTO_MERGE_COST;
        OnClickedAutoMerge();

        SaveToLocal();
    }

    // 자동머지 버튼 클릭 처리 함수
    public void OnClickedAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            startTime = Time.time;
            isAutoMergeActive = true;
            currentAutoMergeDuration = AUTO_MERGE_DURATION;
            UpdateAutoMergeTimerVisibility(true);
            StartAutoMergeCoroutine();
        }
        else
        {
            currentAutoMergeDuration += AUTO_MERGE_DURATION;
        }
    }

    // 코루틴 시작 전 기존 코루틴 정리
    private void StartAutoMergeCoroutine()
    {
        StopAutoMergeCoroutine();
        autoMergeCoroutine = StartCoroutine(AutoMergeCoroutine());
    }

    // 코루틴 안전하게 중지
    private void StopAutoMergeCoroutine()
    {
        if (autoMergeCoroutine != null)
        {
            StopCoroutine(autoMergeCoroutine);
            autoMergeCoroutine = null;
        }
    }

    // 자동머지 코루틴
    private IEnumerator AutoMergeCoroutine()
    {
        StartCoroutine(SpawnCatsWhileAutoMerge());

        while (Time.time - startTime < currentAutoMergeDuration)
        {
            CleanupMergingCats();

            // 활성화된 고양이를 등급순으로 정렬하여 가져옴
            var allCats = FindObjectsOfType<DragAndDropManager>()
                .Where(cat => cat != null && 
                       cat.gameObject.activeSelf && 
                       !cat.isDragging)
                .OrderBy(cat => cat.catData.CatGrade)
                .ToList();

            bool mergeFound = false;

            // 가장 낮은 등급부터 순차적으로 머지 시도
            for (int i = 0; i < allCats.Count; i++)
            {
                if (allCats[i] == null || !allCats[i].gameObject.activeSelf) continue;

                // 같은 등급의 다른 고양이 찾기
                var sameLevelCats = allCats
                    .Where(cat => cat != null &&
                           cat.gameObject.activeSelf &&
                           cat != allCats[i] &&
                           cat.catData.CatGrade == allCats[i].catData.CatGrade)
                    .ToList();

                if (sameLevelCats.Count > 0)
                {
                    var cat1 = allCats[i];
                    var cat2 = sameLevelCats[0];

                    if (IsValidMergePair(cat1, cat2))
                    {
                        Vector2 mergePosition = GetRandomPosition();
                        yield return ExecuteMerge(cat1, cat2, mergePosition);
                        mergeFound = true;
                        yield return waitAutoMergeInterval;
                        break;
                    }
                }
            }

            if (!mergeFound)
            {
                yield return waitAutoMergeInterval;
            }
        }

        EndAutoMerge();
    }

    // 머지중인 고양이 정리 함수
    private void CleanupMergingCats()
    {
        mergingCats.RemoveWhere(cat => cat == null || !cat.gameObject.activeSelf || cat.isDragging);
    }

    // 유효한 머지인지 확인하는 함수
    private bool IsValidMergePair(DragAndDropManager cat1, DragAndDropManager cat2)
    {
        return cat1 != null && cat2 != null &&
               cat1.gameObject.activeSelf && cat2.gameObject.activeSelf &&
               !cat1.isDragging && !cat2.isDragging &&
               !IsMaxLevelCat(cat1.catData) && !IsMaxLevelCat(cat2.catData) &&
               cat1.catData.CatGrade == cat2.catData.CatGrade;
    }

    // 자동머지 랜덤위치 가져오는 함수
    private Vector2 GetRandomPosition()
    {
        // 활성화된 고양이만 찾도록 수정
        DragAndDropManager anyActiveCat = FindObjectsOfType<DragAndDropManager>().FirstOrDefault(cat => cat != null && cat.gameObject.activeSelf);

        if (anyActiveCat != null)
        {
            RectTransform parentRect = anyActiveCat.rectTransform?.parent?.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                panelWidth = parentRect.rect.width;
                panelHeight = parentRect.rect.height;
                panelHalfSize = new Vector2(panelWidth / 2, panelHeight / 2);
            }
        }

        return new Vector2(
            UnityEngine.Random.Range(-panelHalfSize.x, panelHalfSize.x),
            UnityEngine.Random.Range(-panelHalfSize.y, panelHalfSize.y)
        );
    }

    // 머지 실행 코루틴
    private IEnumerator ExecuteMerge(DragAndDropManager cat1, DragAndDropManager cat2, Vector2 mergePosition)
    {
        if (cat1 == null || cat2 == null) yield break;

        mergingCats.Add(cat1);
        mergingCats.Add(cat2);

        yield return MoveCatsToPosition(cat1, cat2, mergePosition);

        if (cat1 != null && cat2 != null && !cat1.isDragging && !cat2.isDragging)
        {
            CompleteMerge(cat1, cat2);
        }

        mergingCats.Remove(cat1);
        mergingCats.Remove(cat2);
    }

    // 정해진 랜덤위치로 고양이들 이동하는 코루틴
    private IEnumerator MoveCatsToPosition(DragAndDropManager cat1, DragAndDropManager cat2, Vector2 targetPosition)
    {
        StartCoroutine(MoveCatSmoothly(cat1, targetPosition));
        StartCoroutine(MoveCatSmoothly(cat2, targetPosition));

        yield return new WaitUntil(() =>
            cat1 == null || cat2 == null ||
            (!mergingCats.Contains(cat1) && !mergingCats.Contains(cat2)) ||
            (Vector2.Distance(cat1.rectTransform.anchoredPosition, targetPosition) < 0.1f &&
             Vector2.Distance(cat2.rectTransform.anchoredPosition, targetPosition) < 0.1f)
        );
    }

    // 머지 완료 처리 함수
    private void CompleteMerge(DragAndDropManager cat1, DragAndDropManager cat2)
    {
        if (cat1 == null || cat2 == null || !cat1.gameObject.activeSelf || !cat2.gameObject.activeSelf) return;
        if (cat1 == cat2) return;

        cat1.GetComponent<CatData>()?.CleanupCoroutines();
        cat2.GetComponent<CatData>()?.CleanupCoroutines();

        Cat mergedCat = MergeManager.Instance.MergeCats(cat1.catData, cat2.catData);
        if (mergedCat != null)
        {
            SpawnManager.Instance.ReturnCatToPool(cat2.gameObject);
            GameManager.Instance.DeleteCatCount();

            cat1.catData = mergedCat;
            cat1.UpdateCatUI();
            SpawnManager.Instance.RecallEffect(cat1.gameObject);
        }
    }

    // 자동머지 중 고양이 소환하는 코루틴
    private IEnumerator SpawnCatsWhileAutoMerge()
    {
        while (isAutoMergeActive && !isPaused)
        {
            while (GameManager.Instance.CanSpawnCat())
            {
                SpawnManager.Instance.SpawnAutoMergeCat();
                yield return waitSpawnInterval;
            }
            yield return null;
        }
    }

    // 고양이를 부드럽게 이동시키는 코루틴
    private IEnumerator MoveCatSmoothly(DragAndDropManager cat, Vector2 targetPosition)
    {
        if (cat == null) yield break;

        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < MOVE_DURATION)
        {
            if (cat == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / MOVE_DURATION);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);

            yield return null;
        }

        if (cat != null)
        {
            cat.rectTransform.anchoredPosition = targetPosition;
        }
    }

    // 최대레벨 고양이인지 확인하는 함수
    private bool IsMaxLevelCat(Cat catData)
    {
        return GameManager.Instance != null &&
               GameManager.Instance.AllCatData != null &&
               GameManager.Instance.AllCatData.All(cat => cat.CatGrade != catData.CatGrade + 1);
    }

    // 자동머지 종료 함수
    private void EndAutoMerge()
    {
        isAutoMergeActive = false;
        isPaused = false;
        pausedTimeRemaining = 0f;
        UpdateAutoMergeTimerVisibility(false);
        StopAllCoroutines();
        mergingCats.Clear();

        SaveToLocal();
    }

    // 머지중인 고양이 정지 함수
    public void StopMerging(DragAndDropManager cat)
    {
        mergingCats.Remove(cat);
    }

    // 고양이가 머지중인지 확인하는 함수
    public bool IsMerging(DragAndDropManager cat)
    {
        return mergingCats.Contains(cat);
    }

    #endregion


    #region UI System

    // 자동머지 비용 Text 업데이트 함수
    private void UpdateAutoMergeCostText()
    {
        if (autoMergeCostText != null)
        {
            autoMergeCostText.text = AUTO_MERGE_COST.ToString();
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

    // 자동머지 상태에 따라 타이머 텍스트 가시성 업데이트 함수
    public void UpdateAutoMergeTimerVisibility(bool isVisible)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.gameObject.SetActive(isVisible);
        }
    }

    // 타이머 텍스트 업데이트 함수
    private void UpdateTimerDisplay(int remainingTime)
    {
        UpdateAutoMergeTimerText(remainingTime);
        UpdateExplainText(remainingTime);
    }

    // 자동머지 타이머 업데이트 함수
    public void UpdateAutoMergeTimerText(int remainingTime)
    {
        if (autoMergeTimerText != null)
        {
            autoMergeTimerText.text = $"{remainingTime}초";
        }
    }

    // 자동머지 설명 텍스트 업데이트 함수
    private void UpdateExplainText(int remainingTime)
    {
        if (explainText != null)
        {
            int hours = remainingTime / 3600;
            int minutes = (remainingTime % 3600) / 60;
            int seconds = remainingTime % 60;
            explainText.text = $"자동합성 {AUTO_MERGE_DURATION}초 증가\n (타이머 {hours:D2}:{minutes:D2}:{seconds:D2})";
        }
    }

    #endregion


    #region Battle System

    // 자동 머지 일시정지 함수
    public void PauseAutoMerge()
    {
        if (isAutoMergeActive && !isPaused)
        {
            isPaused = true;
            pausedTimeRemaining = currentAutoMergeDuration - (Time.time - startTime);
            StopAllCoroutines();
            mergingCats.Clear();
            DisableAutoMergeUI();

            SaveToLocal();
        }
        else
        {
            openAutoMergePanelButton.interactable = false;
        }
    }

    // 자동 머지 이어하기 함수
    public void ResumeAutoMerge()
    {
        if (isPaused && pausedTimeRemaining > 0)
        {
            isPaused = false;
            startTime = Time.time;
            currentAutoMergeDuration = pausedTimeRemaining;
            EnableAutoMergeUI();
            StartAutoMergeCoroutine();

            SaveToLocal();
        }
        else
        {
            EnableAutoMergeUI();
        }
    }

    // 전투 시작시 자동머지 UI 비활성화 함수
    private void DisableAutoMergeUI()
    {
        openAutoMergePanelButton.interactable = false;
        if (autoMergePanel.activeSelf)
        {
            autoMergePanel.SetActive(false);
        }
    }

    // 전투 종료시 자동머지 UI 활성화 함수
    private void EnableAutoMergeUI()
    {
        openAutoMergePanelButton.interactable = true;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public float remainingTime;              // 남은 시간
    }

    public string GetSaveData()
    {
        float remainingTime = 0f;

        if (isAutoMergeActive)
        {
            remainingTime = Mathf.Max(currentAutoMergeDuration - (Time.time - startTime), 0);
        }

        SaveData data = new SaveData
        {
            remainingTime = remainingTime
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        if (savedData.remainingTime > 0)
        {
            // 남은 시간이 있으면 자동 머지 시작
            this.startTime = Time.time;
            this.currentAutoMergeDuration = savedData.remainingTime;
            this.isAutoMergeActive = true;
            this.isPaused = false;
            UpdateAutoMergeTimerVisibility(true);
            UpdateTimerDisplay((int)savedData.remainingTime);
            StartAutoMergeCoroutine();
        }
        else
        {
            // 남은 시간이 없으면 자동 머지 비활성화
            this.isAutoMergeActive = false;
            this.isPaused = false;
            this.currentAutoMergeDuration = 0f;
            UpdateAutoMergeTimerVisibility(false);
            UpdateTimerDisplay(0);
        }

        isDataLoaded = true;
    }

    private void SaveToLocal()
    {
        string data = GetSaveData();
        string key = this.GetType().FullName;
        GoogleManager.Instance?.SaveToPlayerPrefs(key, data);
    }

    #endregion


}


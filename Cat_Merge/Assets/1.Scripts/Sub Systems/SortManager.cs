using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

// 고양이 정렬 관련 스크립트
public class SortManager : MonoBehaviour
{


    #region Variables

    [Header("---[Sort System]")]
    [SerializeField] private Button sortButton;                     // 정렬 버튼
    [SerializeField] private Transform gamePanel;                   // GamePanel

    private const float MOVEMENT_DELAY = 0.1f;                      // 정렬 시작 전 대기시간
    private const float SORT_COMPLETE_DELAY = 0.2f;                 // 정렬 완료 후 대기시간
    private const float MOVE_DURATION = 0.1f;                       // 각 고양이의 이동 시간
    private const int CATS_PER_ROW = 7;                             // 한 줄에 배치될 고양이 수
    private const float SPACING = 10f;                              // 고양이 간 간격

    private Dictionary<int, List<GameObject>> catsByGrade = new Dictionary<int, List<GameObject>>();  // 등급별 고양이 저장
    private bool isSorting = false;                                 // 정렬 진행 중 여부

    [Header("---[ETC]")]
    private readonly WaitForSeconds waitForMovementDelay = new WaitForSeconds(MOVEMENT_DELAY);
    private readonly WaitForSeconds waitForSortCompleteDelay = new WaitForSeconds(SORT_COMPLETE_DELAY);

    #endregion


    #region Unity Methods

    private void Start()
    {
        InitializeButtonListeners();
    }

    #endregion


    #region Initialize

    // 버튼 리스너 초기화 함수
    private void InitializeButtonListeners()
    {
        sortButton.onClick.AddListener(SortCats);
    }

    #endregion


    #region Sort System

    // 고양이 정렬 시작 함수
    private void SortCats()
    {
        if (isSorting) return;  // 이미 정렬 중이면 무시

        StartCoroutine(SortCatsCoroutine());
    }

    // 고양이들을 순차적으로 배치하는 코루틴
    private IEnumerator SortCatsCoroutine()
    {
        isSorting = true;

        // 모든 고양이 이동 중지
        StopAllCatMovements();
        yield return waitForMovementDelay;

        // 모든 고양이 방향 초기화 (왼쪽 보기)
        ResetAllCatDirections();

        // 등급순 정렬 및 위치 이동
        var sortedCats = GetSortedCats();

        // 정렬이 필요한지 확인
        if (IsSortingNeeded(sortedCats))
        {
            yield return StartCoroutine(MoveCatsToPositions(sortedCats));
            yield return waitForSortCompleteDelay;
        }

        RestoreAutoMoveState();
        isSorting = false;
    }

    // 정렬이 필요한지 확인하는 함수
    private bool IsSortingNeeded(List<GameObject> sortedCats)
    {
        if (sortedCats.Count == 0) return false;

        // 현재 위치와 목표 위치를 비교하여 정렬 필요성 확인
        for (int i = 0; i < sortedCats.Count; i++)
        {
            RectTransform rectTransform = sortedCats[i].GetComponent<RectTransform>();
            Vector2 targetPosition = CalculateTargetPosition(i, rectTransform);

            // 현재 위치와 목표 위치의 차이가 일정 거리 이상이면 정렬 필요
            if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 1f)
            {
                return true;
            }
        }

        return false;
    }

    // 정렬된 고양이들을 순차적으로 이동시키는 코루틴
    private IEnumerator MoveCatsToPositions(List<GameObject> sortedCats)
    {
        var moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < sortedCats.Count; i++)
        {
            moveCoroutines.Add(StartCoroutine(MoveCatToPosition(sortedCats[i], i)));
        }

        foreach (var coroutine in moveCoroutines)
        {
            yield return coroutine;
        }
    }

    // 고양이의 등급을 반환하는 함수
    private int GetCatGrade(GameObject catObject)
    {
        return catObject.GetComponent<CatData>().catData.CatGrade;
    }

    // 개별 고양이 이동 처리 함수(고양이들을 부드럽게 정렬된 위치로 이동시키는 함수)
    private IEnumerator MoveCatToPosition(GameObject catObject, int index)
    {
        // 현재 위치와 목표 위치의 차이를 계산하여 부드럽게 이동
        RectTransform rectTransform = catObject.GetComponent<RectTransform>();
        Vector2 targetPosition = CalculateTargetPosition(index, rectTransform);
        Vector2 initialPosition = rectTransform.anchoredPosition;

        float elapsedTime = 0f;
        while (elapsedTime < MOVE_DURATION)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / MOVE_DURATION);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 이동이 끝났을 때 정확한 목표 위치에 도달
        rectTransform.anchoredPosition = targetPosition;
    }

    // 정렬 위치 계산 함수
    private Vector2 CalculateTargetPosition(int index, RectTransform rectTransform)
    {
        float targetX = (index % CATS_PER_ROW - 3) * (rectTransform.rect.width + SPACING);
        float targetY = (index / CATS_PER_ROW - 3) * (rectTransform.rect.height + SPACING);
        return new Vector2(targetX, -targetY);
    }

    // 둥급순으로 정렬하여 리스트로 반환하는 함수
    private List<GameObject> GetSortedCats()
    {
        List<GameObject> sortedCats = new List<GameObject>();
        catsByGrade.Clear();

        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            // 자동 머지 중인 고양이는 제외
            DragAndDropManager dragManager = child.GetComponent<DragAndDropManager>();
            if (dragManager != null && AutoMergeManager.Instance != null &&
                !AutoMergeManager.Instance.IsMerging(dragManager))
            {
                int grade = GetCatGrade(child.gameObject);

                if (!catsByGrade.ContainsKey(grade))
                {
                    catsByGrade[grade] = new List<GameObject>();
                }
                catsByGrade[grade].Add(child.gameObject);
            }
        }

        // 등급별로 정렬하되, 같은 등급 내에서는 현재 순서 유지
        foreach (var grade in catsByGrade.Keys.OrderByDescending(k => k))
        {
            sortedCats.AddRange(catsByGrade[grade]);
        }

        return sortedCats;
    }

    // 모든 고양이의 자동이동 상태를 설정하는 함수
    private void SetAutoMoveState(bool isEnabled)
    {
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(isEnabled);
            }
        }
    }

    // 모든 고양이의 방향을 초기화하는 함수 (왼쪽을 보도록)
    private void ResetAllCatDirections()
    {
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                // Transform 내의 "Cat Image" 또는 "Image" 오브젝트 찾기
                Transform catImageTransform = child.Find("Cat Image");
                if (catImageTransform == null)
                {
                    catImageTransform = child.Find("Image");
                }

                // 찾은 오브젝트의 방향을 초기화 (Y 회전을 0으로)
                if (catImageTransform != null)
                {
                    catImageTransform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
    }

    #endregion


    #region Move Control

    // 모든 고양이의 이동을 중지시키는 함수
    private void StopAllCatMovements()
    {
        AutoMoveManager.Instance.StopAllCatsMovement();
        StopCatMovementsInPanel();
    }

    // 게임 패널 내 모든 고양이의 이동을 중지하는 함수
    private void StopCatMovementsInPanel()
    {
        foreach (Transform child in gamePanel)
        {
            if (!child.gameObject.activeSelf) continue;

            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.StopAllMovement();
                catData.SetAutoMoveState(false);
            }
        }
    }

    // 자동 이동 상태를 복원하는 함수
    private void RestoreAutoMoveState()
    {
        SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
    }

    #endregion


    #region Battle System

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleSortState()
    {
        SetSortButtonState(false);
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleSortState()
    {
        SetSortButtonState(true);
    }

    // 정렬 버튼의 상태를 설정하는 함수
    private void SetSortButtonState(bool state)
    {
        sortButton.interactable = state;
    }

    #endregion


}

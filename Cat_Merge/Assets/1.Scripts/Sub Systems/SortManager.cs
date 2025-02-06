using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortManager : MonoBehaviour
{
    [Header("---[Sort System]")]
    [SerializeField] private Button sortButton;                     // 정렬 버튼
    [SerializeField] private Transform gamePanel;                   // GamePanel

    // ======================================================================================================================

    private void Awake()
    {
        sortButton.onClick.AddListener(SortCats);
    }

    // ======================================================================================================================

    // 고양이 정렬 함수
    private void SortCats()
    {
        StartCoroutine(SortCatsCoroutine());
    }

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleSortState()
    {
        sortButton.interactable = false;
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleSortState()
    {
        sortButton.interactable = true;
    }

    // 고양이들을 정렬된 위치에 배치하는 코루틴
    private IEnumerator SortCatsCoroutine()
    {
        // 정렬 전 자동 이동 잠시 중지 (현재 자동이동이 진행중인 고양이도 중지 = CatData의 AutoMove가 실행중이어도 중지)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(false); // 자동 이동 비활성화
            }
        }

        // 고양이 객체들을 등급을 기준으로 정렬 (높은 등급이 먼저 오도록)
        List<GameObject> sortedCats = new List<GameObject>();
        foreach (Transform child in gamePanel)
        {
            sortedCats.Add(child.gameObject);
        }

        sortedCats.Sort((cat1, cat2) =>
        {
            int grade1 = GetCatGrade(cat1);
            int grade2 = GetCatGrade(cat2);

            if (grade1 == grade2) return 0;
            return grade1 > grade2 ? -1 : 1;
        });

        // 고양이 이동 코루틴 실행
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < sortedCats.Count; i++)
        {
            GameObject cat = sortedCats[i];
            Coroutine moveCoroutine = StartCoroutine(MoveCatToPosition(cat, i));
            moveCoroutines.Add(moveCoroutine);
        }

        // 모든 고양이가 이동을 마칠 때까지 기다리기
        foreach (Coroutine coroutine in moveCoroutines)
        {
            yield return coroutine;
        }

        // 정렬 후 자동 이동 상태 복구 (정렬이 완료되고 고양이들이 다시 주기마다 자동이동이 가능하게 원래상태로 복구)
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                catData.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
            }
        }
    }

    // 고양이의 등급을 반환하는 함수
    private int GetCatGrade(GameObject catObject)
    {
        int grade = catObject.GetComponent<CatData>().catData.CatGrade;
        return grade;
    }

    // 고양이들을 부드럽게 정렬된 위치로 이동시키는 함수
    private IEnumerator MoveCatToPosition(GameObject catObject, int index)
    {
        RectTransform rectTransform = catObject.GetComponent<RectTransform>();

        // 목표 위치 계산 (index는 정렬된 순서)
        float targetX = (index % 7 - 3) * (rectTransform.rect.width + 10);
        float targetY = (index / 7) * (rectTransform.rect.height + 10);
        Vector2 targetPosition = new Vector2(targetX, -targetY);

        // 현재 위치와 목표 위치의 차이를 계산하여 부드럽게 이동
        float elapsedTime = 0f;
        float duration = 0.1f;          // 이동 시간 (초)

        Vector2 initialPosition = rectTransform.anchoredPosition;

        // 목표 위치로 부드럽게 이동
        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 이동이 끝났을 때 정확한 목표 위치에 도달
        rectTransform.anchoredPosition = targetPosition;
    }
}

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class AutoMerge : MonoBehaviour
{
    private float startTime;                            // 자동 머지 시작 시간
    private float autoMergeDuration = 3.0f;             // 자동 머지 기본 지속 시간
    private float currentAutoMergeDuration;             // 현재 자동 머지 지속 시간
    private float plusAutoMergeDuration;                // 자동 머지 추가 시간
    private float autoMergeInterval = 0.5f;             // 자동 머지 간격
    private float moveDuration = 0.2f;                  // 고양이가 이동하는 데 걸리는 시간 (이동 속도)
    private bool isAutoMergeActive = false;             // 자동 머지 활성화 상태

    private HashSet<CatDragAndDrop> mergingCats = new HashSet<CatDragAndDrop>(); // 머지 중인 고양이 추적

    private void Awake()
    {
        plusAutoMergeDuration = autoMergeDuration;
        currentAutoMergeDuration = autoMergeDuration;
    }

    // 자동 머지 시작
    public void StartAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            Debug.Log("자동머지시작");
            startTime = Time.time;
            currentAutoMergeDuration = autoMergeDuration;
            StartCoroutine(AutoMergeCoroutine());
        }
        else
        {
            Debug.Log($"{plusAutoMergeDuration}초 추가");
            currentAutoMergeDuration += plusAutoMergeDuration;
        }
    }

    // 자동 머지 중인지 확인하는 메서드
    public bool IsMerging(CatDragAndDrop cat)
    {
        return mergingCats.Contains(cat);
    }

    // 자동 머지 코루틴
    private IEnumerator AutoMergeCoroutine()
    {
        isAutoMergeActive = true;

        while (Time.time - startTime - currentAutoMergeDuration < 0)
        {
            var allCats = FindObjectsOfType<CatDragAndDrop>().OrderBy(cat => cat.catData.CatId).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatId).Where(group => group.Count() > 1).ToList();

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

                    // 예외 처리: 드래그 중인 고양이, 이미 머지 중인 고양이 확인
                    if (cat1 == null || cat2 == null || cat1.isDragging || cat2.isDragging ||
                        mergingCats.Contains(cat1) || mergingCats.Contains(cat2))
                    {
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    mergingCats.Add(cat1);
                    mergingCats.Add(cat2);

                    Cat nextCat = FindObjectOfType<CatMerge>().GetCatById(cat1.catData.CatId + 1);
                    if (nextCat == null)
                    {
                        // 예외 처리 : 합성할 고양이 없음
                        mergingCats.Remove(cat1);
                        mergingCats.Remove(cat2);
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        break;
                    }

                    RectTransform parentRect = cat1.rectTransform?.parent?.GetComponent<RectTransform>();
                    if (parentRect == null)
                    {
                        // 예외 처리 : 부모 RectTransform이 없음
                        mergingCats.Remove(cat1);
                        mergingCats.Remove(cat2);
                        catsInGroup.Remove(cat1);
                        catsInGroup.Remove(cat2);
                        continue;
                    }

                    Vector2 mergePosition = GetRandomPosition(parentRect);

                    StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                    StartCoroutine(MoveCatSmoothly(cat2, mergePosition));
                    yield return new WaitUntil(() => cat1 != null && cat2 != null &&
                                                     cat1.rectTransform.anchoredPosition == mergePosition &&
                                                     cat2.rectTransform.anchoredPosition == mergePosition);

                    if (cat1 != null && cat2 != null)
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
        Debug.Log("자동 머지 종료");
    }

    // 부모 RectTransform에서 랜덤 위치 계산
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


}

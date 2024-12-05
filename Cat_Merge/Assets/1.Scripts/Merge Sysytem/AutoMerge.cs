using UnityEngine;
using System.Collections;
using System.Linq;

public class AutoMerge : MonoBehaviour
{
    private float autoMergeDuration = 3.0f;             // 자동 머지 지속 시간
    private float autoMergeInterval = 0.5f;             // 자동 머지 간격
    private float moveDuration = 0.2f;                  // 고양이가 이동하는 데 걸리는 시간 (이동 속도)
    private bool isAutoMergeActive = false;             // 자동 머지 활성화 상태

    // 자동 머지 시작
    public void StartAutoMerge()
    {
        if (!isAutoMergeActive)
        {
            StartCoroutine(AutoMergeCoroutine());
        }
    }

    // 자동 머지 코루틴
    private IEnumerator AutoMergeCoroutine()
    {
        isAutoMergeActive = true;
        float elapsedTime = 0f;

        while (elapsedTime < autoMergeDuration)
        {
            //yield return new WaitForSeconds(autoMergeInterval);

            // 시간 업데이트
            elapsedTime += autoMergeInterval;

            // 고양이들 등급별로 묶기
            var allCats = FindObjectsOfType<CatDragAndDrop>().OrderBy(cat => cat.catData.CatId).ToList();
            var groupedCats = allCats.GroupBy(cat => cat.catData.CatId).Where(group => group.Count() > 1).ToList();

            // 합성할 고양이가 없다면 계속 기다림
            if (!groupedCats.Any())
            {
                continue;
            }

            // 등급별로 고양이 합성
            foreach (var group in groupedCats)
            {
                var catsInGroup = group.ToList();

                // 2개 이상의 고양이가 있으면 합성
                while (catsInGroup.Count >= 2)
                {
                    CatDragAndDrop cat1 = catsInGroup[0];
                    CatDragAndDrop cat2 = catsInGroup[1];

                    // 다음 등급의 고양이 확인
                    Cat nextCat = FindObjectOfType<CatMerge>().GetCatById(cat1.catData.CatId + 1);
                    if (nextCat == null)
                    {
                        Debug.LogWarning("다음 등급의 고양이가 없음");
                        break;
                    }

                    // 합성 위치 계산
                    RectTransform parentRect = cat1.rectTransform.parent.GetComponent<RectTransform>();
                    Vector2 mergePosition = GetRandomPosition(parentRect);

                    // 고양이 이동
                    StartCoroutine(MoveCatSmoothly(cat1, mergePosition));
                    StartCoroutine(MoveCatSmoothly(cat2, mergePosition));

                    // 두 고양이가 모두 도달했을 때 합성 시작
                    yield return new WaitUntil(() => cat1.rectTransform.anchoredPosition == mergePosition && cat2.rectTransform.anchoredPosition == mergePosition);

                    // 합성 처리
                    Cat mergedCat = FindObjectOfType<CatMerge>().MergeCats(cat1.catData, cat2.catData);
                    if (mergedCat != null)
                    {
                        cat1.catData = mergedCat;
                        cat1.UpdateCatUI();
                        Destroy(cat2.gameObject);
                    }

                    // 합성 후 남은 고양이 목록 갱신
                    catsInGroup.RemoveAt(0);
                    catsInGroup.RemoveAt(0);

                    // 합성 후 자동 머지 간격만큼 대기
                    yield return new WaitForSeconds(autoMergeInterval);
                }
            }
        }

        // 자동 머지 종료
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
        Vector2 startPos = cat.rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cat.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, t);
            yield return null;
        }
    }


}

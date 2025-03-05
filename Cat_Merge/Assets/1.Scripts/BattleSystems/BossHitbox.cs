using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 260f;                                 // 히트박스의 너비 (원의 지름 계산용)
    private float height = 420f;                                // 히트박스의 높이 (원의 지름 계산용)

    private RectTransform rectTransform;

    // 고양이의 이미지 때문에 위치 미세 조정
    public Vector3 Position => new Vector3(rectTransform.anchoredPosition.x + 20, rectTransform.anchoredPosition.y - 10, 0);

    // ======================================================================================================================

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // ======================================================================================================================

    // 보스의 히트박스 범위 내에 고양이가 있는지 체크하는 함수
    public bool IsInHitbox(Vector3 targetPosition)
    {
        // 타원 내부에 있는지 확인하기 위해 타원 방정식 사용 : (x/a)^2 + (y/b)^2 <= 1
        float dx = targetPosition.x - Position.x;
        float dy = targetPosition.y - Position.y;
        float normalizedX = dx / (width / 2f);
        float normalizedY = dy / (height / 2f);

        return (normalizedX * normalizedX + normalizedY * normalizedY) <= 1f;
    }

    // 보스 경계 확인 함수
    public bool IsAtBoundary(Vector3 position)
    {
        // 타원 경계에 있는지 확인하기 위해 타원 방정식 사용 : (x/a)^2 + (y/b)^2 <= 1
        float dx = position.x - Position.x;
        float dy = position.y - Position.y;
        float normalizedX = dx / (width / 2f);
        float normalizedY = dy / (height / 2f);

        // 경계 판정을 조금 느슨하게 하기 위한 여유값
        float boundaryTolerance = 0.05f;

        float ellipseEquation = normalizedX * normalizedX + normalizedY * normalizedY;
        return Mathf.Abs(ellipseEquation - 1f) <= boundaryTolerance;
    }

    // 주어진 위치에서 원의 경계까지의 가장 가까운 점을 계산하는 함수
    public Vector3 GetClosestBoundaryPoint(Vector3 position)
    {
        // 보스와 고양이 사이의 방향 계산
        Vector3 direction = (position - Position).normalized;

        // 타원의 반지름을 따라 방향을 조정
        float angle = Mathf.Atan2(direction.y * (width / height), direction.x);
        float closestX = Mathf.Cos(angle) * (width / 2f);
        float closestY = Mathf.Sin(angle) * (height / 2f);

        return Position + new Vector3(closestX, closestY, 0);
    }

}

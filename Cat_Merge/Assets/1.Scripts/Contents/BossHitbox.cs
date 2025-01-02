using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 200f;     // 히트박스의 너비
    private float height = 400f;    // 히트박스의 높이

    public Vector2 Size => new Vector2(width, height);                              // 히트박스 크기
    public Vector3 Position => GetComponent<RectTransform>().anchoredPosition;      // 보스의 위치

    // 보스의 히트박스 범위 내에 고양이가 있는지 체크하는 함수
    public bool IsInHitbox(Vector3 targetPosition)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        // 보스의 위치와 고양이의 위치를 RectTransform을 기준으로 비교
        return Mathf.Abs(targetPosition.x - Position.x) < halfWidth && Mathf.Abs(targetPosition.y - Position.y) < halfHeight;
    }

    // 보스 경계 확인 함수
    public bool IsAtBoundary(Vector3 position)
    {
        // 히트박스 경계 계산
        float left = Position.x - Size.x / 2;
        float right = Position.x + Size.x / 2;
        float top = Position.y + Size.y / 2;
        float bottom = Position.y - Size.y / 2;

        // 고양이가 경계에 위치하는지 확인 (약간의 여유값 추가)
        float boundaryTolerance = 1f;   // 경계 판정을 조금 더 느슨하게 하기 위한 여유값
        bool isAtHorizontalBoundary = Mathf.Abs(position.x - left) <= boundaryTolerance || Mathf.Abs(position.x - right) <= boundaryTolerance;
        bool isAtVerticalBoundary = Mathf.Abs(position.y - top) <= boundaryTolerance || Mathf.Abs(position.y - bottom) <= boundaryTolerance;

        return isAtHorizontalBoundary || isAtVerticalBoundary;
    }

}

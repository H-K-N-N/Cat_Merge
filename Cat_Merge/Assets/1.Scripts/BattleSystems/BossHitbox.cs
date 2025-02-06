using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 200f;     // 히트박스의 너비
    private float height = 400f;    // 히트박스의 높이

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public Vector2 Size => new Vector2(width, height);                          // 히트박스 크기 
    public Vector3 Position => rectTransform.anchoredPosition;                  // 보스의 위치

    // 보스의 히트박스 범위 내에 고양이가 있는지 체크하는 함수
    public bool IsInHitbox(Vector3 targetPosition)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        // 보스의 위치와 고양이의 위치를 RectTransform을 기준으로 비교
        return Mathf.Abs(targetPosition.x - Position.x) < halfWidth && 
            Mathf.Abs(targetPosition.y - Position.y) < halfHeight;
    }

    // 보스 경계 확인 함수
    public bool IsAtBoundary(Vector3 position)
    {
        Vector2 size = Size;
        Vector2 pos = Position;
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        // 경계 판정을 조금 느슨하게 하기 위한 여유값
        const float boundaryTolerance = 1f;

        return Mathf.Abs(position.x - (pos.x - halfWidth)) <= boundaryTolerance ||
               Mathf.Abs(position.x - (pos.x + halfWidth)) <= boundaryTolerance ||
               Mathf.Abs(position.y - (pos.y - halfHeight)) <= boundaryTolerance ||
               Mathf.Abs(position.y - (pos.y + halfHeight)) <= boundaryTolerance;
    }
}

using UnityEngine;

public class BossHitbox : MonoBehaviour
{
    [Header("---[Boss Hitbox]")]
    private float width = 200f;     // 히트박스의 너비
    private float height = 400f;    // 히트박스의 높이

    public Vector2 Size => new Vector2(width, height);                              // 히트박스 크기 (width, height)
    public Vector3 Position => GetComponent<RectTransform>().anchoredPosition;      // 보스의 위치

    // 보스의 히트박스 범위 내에 고양이가 있는지 체크하는 함수
    public bool IsInHitbox(Vector3 targetPosition)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        // 보스의 위치와 고양이의 위치를 RectTransform을 기준으로 비교
        return Mathf.Abs(targetPosition.x - Position.x) < halfWidth && Mathf.Abs(targetPosition.y - Position.y) < halfHeight;
    }

}

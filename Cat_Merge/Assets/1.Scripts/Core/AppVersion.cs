using System;

// "1.10.2" 같은 버전 문자열을 안전하게 비교하는 순수 유틸리티.
// 파싱에 실패하는 값이 들어와도 절대 예외를 던지지 않고, 항상 "통과(fail-open)" 쪽으로 판단한다.
public static class AppVersion
{
    // current가 min 이상이면 true. 어느 한쪽이라도 파싱에 실패하면 무조건 true(통과).
    public static bool IsAtLeast(string current, string min)
    {
        if (!TryParse(current, out int[] currentParts))
        {
            return true;
        }

        if (!TryParse(min, out int[] minParts))
        {
            return true;
        }

        for (int i = 0; i < currentParts.Length; i++)
        {
            if (currentParts[i] != minParts[i])
            {
                return currentParts[i] > minParts[i];
            }
        }

        return true; // 완전히 동일한 버전
    }

    // "1.2.3" -> [1, 2, 3]. 부분 표기("1.2")는 뒤를 0으로 채운다. 실패하면 false.
    private static bool TryParse(string version, out int[] parts)
    {
        parts = new int[3] { 0, 0, 0 };

        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        string[] segments = version.Trim().Split('.');
        if (segments.Length == 0)
        {
            return false;
        }

        int segmentCount = Math.Min(segments.Length, parts.Length);
        for (int i = 0; i < segmentCount; i++)
        {
            if (!int.TryParse(segments[i].Trim(), out parts[i]))
            {
                return false;
            }

            if (parts[i] < 0)
            {
                return false;
            }
        }

        return true;
    }
}

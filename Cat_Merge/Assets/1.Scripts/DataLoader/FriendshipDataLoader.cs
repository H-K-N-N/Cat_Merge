using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int grade; // 등급 (int)
    public int[] expRequirements = new int[5]; // 1~5단계 경험치 요구량
    public int[] rewards = new int[5]; // 1~5단계 보상 (int로 변경)
    public string[] passiveEffects = new string[5]; // 1~5단계 패시브 효과 수치
}

[DefaultExecutionOrder(-10)]
public class FriendshipDataLoader : MonoBehaviour
{

    public static FriendshipDataLoader Instance { get; private set; }

    // 고양이 데이터를 관리할 Dictionary
    public Dictionary<int, List<(int grade, int exp, int reward, string passive)>> dataByGrade = 
        new Dictionary<int, List<(int grade, int exp, int reward, string passive)>>();

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

        LoadCSV("FriendshipDB");

        //PrintLevelData();
    }

    void LoadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + fileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 첫 줄(헤더) 제외
        {
            string line = lines[i].Trim(); // 공백 제거
            if (string.IsNullOrEmpty(line)) continue; // 빈 줄 건너뜀

            string[] values = line.Split(',');

            if (values.Length < 16)
            {
                Debug.LogWarning($"데이터 부족 (라인 {i + 1}): {line}");
                continue;
            }

            LevelData data = new LevelData();

            // grade (등급) 변환
            if (int.TryParse(values[0].Trim(), out int parsedGrade))
            {
                data.grade = parsedGrade;
            }
            else
            {
                Debug.LogError($"등급 파싱 오류 (라인 {i + 1}): {values[0]}");
                continue; // 등급 변환 실패 시 해당 데이터 스킵
            }

            for (int j = 0; j < 5; j++)
            {
                // 경험치 요구량 변환
                if (int.TryParse(values[1 + j].Trim(), out int exp))
                    data.expRequirements[j] = exp;
                else
                    Debug.LogError($"경험치 파싱 오류 (라인 {i + 1}): {values[1 + j]}");

                // 보상 변환 (string → int)
                if (int.TryParse(values[6 + j].Trim(), out int reward))
                    data.rewards[j] = reward;
                else
                    Debug.LogError($"보상 파싱 오류 (라인 {i + 1}): {values[6 + j]}");

                // 패시브 효과 변환
                string passive = values[11 + j].Trim();
                data.passiveEffects[j] = passive;
            }

            // 번호별로 데이터 추가
            if (!dataByGrade.ContainsKey(data.grade))
            {
                dataByGrade[data.grade] = new List<(int grade, int exp, int reward, string passive)>();
            }
            for(int k = 0; k < 5; k++)
            {
                dataByGrade[data.grade].Add((data.grade, data.expRequirements[k], data.rewards[k], data.passiveEffects[k]));             
            }

           
        }
    }

    public List<(int grade, int exp, int reward, string passive)> GetDataByGrade(int grade)
    {
        if (dataByGrade.ContainsKey(grade))
        {
            return dataByGrade[grade];
        }
        else
        {
            Debug.LogWarning($"No data found for number {grade}");
            return null;
        }
    }

    //// 디버깅용
    //private void PrintLevelData()
    //{
    //    foreach (var data in dataByGrade)
    //    {
    //        Debug.Log($"{data.Key} 등급"); // Key = grade

    //        for (int i = 0; i < data.Value.Count; i++) // List 순회
    //        {
    //            var levelInfo = data.Value[i]; // 튜플 데이터 추출
    //            Debug.Log($"{i + 1}단계 - 경험치 요구량: {levelInfo.exp}, 보상: {levelInfo.reward}, 패시브 효과: {levelInfo.passive}");
    //        }
    //    }
    //}

    // Resources 폴더에서 스프라이트 로드 (언젠간 쓰일듯)
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
    //    }
    //    return sprite;
    //}

}


using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int grade;                               // 등급 (int)
    public int[] expRequirements = new int[5];      // 1~5단계 경험치 요구량
    public int[] rewards = new int[5];              // 1~5단계 보상 (int로 변경)
    public string[] passiveEffects = new string[5]; // 1~5단계 패시브 효과 수치
}

// 애정도 데이터를 로드하고 관리하는 스크립트
[DefaultExecutionOrder(-10)]
public class FriendshipDataLoader : MonoBehaviour
{


    #region Variables

    public static FriendshipDataLoader Instance { get; private set; }

    // 고양이 데이터를 관리할 Dictionary
    public Dictionary<int, List<(int grade, int exp, int reward, string passive)>> dataByGrade = 
        new Dictionary<int, List<(int grade, int exp, int reward, string passive)>>();

    private readonly LevelData levelData = new LevelData();
    private readonly List<(int grade, int exp, int reward, string passive)> gradeDataList = 
        new List<(int grade, int exp, int reward, string passive)>(5);

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadCSV("FriendshipDB");

        //PrintLevelData();
    }

    #endregion


    #region Data Loading

    // CSV 파일을 읽고 데이터를 파싱하여 저장하는 함수
    private void LoadCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            //Debug.LogError("CSV 파일을 찾을 수 없습니다: " + fileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length < 16)
            {
                //Debug.LogWarning($"데이터 부족 (라인 {i + 1}): {line}");
                continue;
            }

            if (!ParseLevelData(values, i + 1, levelData)) continue;

            // 번호별로 데이터 추가
            if (!dataByGrade.ContainsKey(levelData.grade))
            {
                gradeDataList.Clear(); // 리스트 재사용
                for (int k = 0; k < 5; k++)
                {
                    gradeDataList.Add((levelData.grade,
                                     levelData.expRequirements[k],
                                     levelData.rewards[k],
                                     levelData.passiveEffects[k]));
                }
                dataByGrade[levelData.grade] = new List<(int, int, int, string)>(gradeDataList);
            }
        }
    }

    // CSV 데이터를 파싱하여 LevelData 객체에 저장하는 함수
    private bool ParseLevelData(string[] values, int lineNumber, LevelData data)
    {
        if (!int.TryParse(values[0].Trim(), out data.grade))
        {
            //Debug.LogError($"등급 파싱 오류 (라인 {lineNumber}): {values[0]}");
            return false;
        }

        for (int j = 0; j < 5; j++)
        {
            if (!int.TryParse(values[1 + j].Trim(), out data.expRequirements[j]))
            {
                //Debug.LogError($"경험치 파싱 오류 (라인 {lineNumber}): {values[1 + j]}");
                return false;
            }

            if (!int.TryParse(values[6 + j].Trim(), out data.rewards[j]))
            {
                //Debug.LogError($"보상 파싱 오류 (라인 {lineNumber}): {values[6 + j]}");
                return false;
            }

            data.passiveEffects[j] = values[11 + j].Trim();
        }

        return true;
    }

    //// 디버깅용 함수
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

    // Resources 폴더에서 스프라이트 로드하는 함수
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
    //    }
    //    return sprite;
    //}

    #endregion


    #region Data Access

    // 특정 등급의 데이터를 반환하는 함수
    public List<(int grade, int exp, int reward, string passive)> GetDataByGrade(int grade)
    {
        if (dataByGrade.ContainsKey(grade))
        {
            return dataByGrade[grade];
        }
        //Debug.LogWarning($"No data found for number {grade}");
        return null;
    }

    #endregion


}

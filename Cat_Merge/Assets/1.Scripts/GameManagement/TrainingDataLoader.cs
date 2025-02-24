using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-3)]  // CatDataLoader와 같은 순서로 실행
public class TrainingDataLoader : MonoBehaviour
{
    // 고양이별 훈련 데이터를 관리할 Dictionary (Key: CatId)
    public Dictionary<int, TrainingData> trainingDictionary = new Dictionary<int, TrainingData>();

    private void Awake()
    {
        LoadTrainingDataFromCSV();
    }

    // CSV 파일을 읽어 TrainingData 객체로 변환 후 Dictionary에 저장
    private void LoadTrainingDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("TrainingDB");
        if (csvFile == null)
        {
            Debug.LogError("Training CSV 파일이 연결되지 않았습니다");
            return;
        }

        StringReader stringReader = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = stringReader.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 1) continue;
            if (lineNumber >= 5) continue;

            // CSV 파싱 - 따옴표 내부의 쉼표는 무시하고 실제 구분자만 처리
            List<string> values = new List<string>();
            bool insideQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (line[i] == ',' && !insideQuotes)
                {
                    string value = line.Substring(startIndex, i - startIndex).Trim();
                    value = value.Trim('"');
                    values.Add(value);
                    startIndex = i + 1;
                }
            }
            // 마지막 값 추가
            string lastValue = line.Substring(startIndex).Trim();
            lastValue = lastValue.Trim('"');
            values.Add(lastValue);

            // 빈 칸을 발견하면 거기까지만 처리
            List<string> validValues = new List<string>();
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    break;
                }
                validValues.Add(value);
            }

            try
            {
                // 데이터 파싱
                int catId = int.Parse(validValues[0]);
                int growthDamage = int.Parse(validValues[2]);
                int growthHp = int.Parse(validValues[3]);

                // TrainingData 객체 생성 및 Dictionary에 추가
                TrainingData trainingData = new TrainingData(growthDamage, growthHp);
                if (!trainingDictionary.ContainsKey(catId))
                {
                    trainingDictionary.Add(catId, trainingData);
                }
                else
                {
                    Debug.LogWarning($"중복된 CatId가 발견되었습니다: {catId}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"라인 {lineNumber}: 데이터 처리 중 오류 발생 - {ex.Message}");
            }
        }
    }
}

// 훈련 데이터를 담는 클래스
public class TrainingData
{
    private int growthDamage;
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    public TrainingData(int growthDamage, int growthHp)
    {
        GrowthDamage = growthDamage;
        GrowthHp = growthHp;
    }
}
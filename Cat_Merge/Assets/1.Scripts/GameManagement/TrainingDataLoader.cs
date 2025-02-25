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
                int catId = int.Parse(validValues[1]);
                int growthDamage = int.Parse(validValues[2]);
                int growthHp = int.Parse(validValues[3]);
                string extraAbilityName = validValues[4];                   // 추가 획득 능력치 이름
                double extraAbilityValue = double.Parse(validValues[5]);    // 추가 획득 능력치 수치
                string extraAbilityUnit = validValues[6];                   // 단위
                string extraAbilitySymbol = validValues[7];                 // 보조 기호
                double trainingCoin = double.Parse(validValues[8]);
                double levelUpCoin = double.Parse(validValues[9]);

                // TrainingData 객체 생성 및 Dictionary에 추가
                TrainingData trainingData = new TrainingData(growthDamage, growthHp, trainingCoin, levelUpCoin,
                    extraAbilityName, extraAbilityValue, extraAbilityUnit, extraAbilitySymbol);
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

    private double trainingCoin;
    public double TrainingCoin { get => trainingCoin; set => trainingCoin = value; }

    private double levelUpCoin;
    public double LevelUpCoin { get => levelUpCoin; set => levelUpCoin = value; }

    // 추가 능력치 관련 변수들
    private string extraAbilityName;    // 추가 획득 능력치 이름
    public string ExtraAbilityName { get => extraAbilityName; set => extraAbilityName = value; }

    private double extraAbilityValue;   // 추가 획득 능력치 수치
    public double ExtraAbilityValue { get => extraAbilityValue; set => extraAbilityValue = value; }

    private string extraAbilityUnit;    // 단위
    public string ExtraAbilityUnit { get => extraAbilityUnit; set => extraAbilityUnit = value; }

    private string extraAbilitySymbol;  // 보조 기호
    public string ExtraAbilitySymbol { get => extraAbilitySymbol; set => extraAbilitySymbol = value; }

    public TrainingData(int growthDamage, int growthHp, double trainingCoin, double levelUpCoin,
        string extraAbilityName, double extraAbilityValue, string extraAbilityUnit, string extraAbilitySymbol)
    {
        GrowthDamage = growthDamage;
        GrowthHp = growthHp;
        TrainingCoin = trainingCoin;
        LevelUpCoin = levelUpCoin;
        ExtraAbilityName = extraAbilityName;
        ExtraAbilityValue = extraAbilityValue;
        ExtraAbilityUnit = extraAbilityUnit;
        ExtraAbilitySymbol = extraAbilitySymbol;
    }
}
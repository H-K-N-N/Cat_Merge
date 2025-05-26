using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 쥐 데이터를 로드하고 관리하는 스크립트
[DefaultExecutionOrder(-10)]
public class MouseDataLoader : MonoBehaviour
{


    #region Variables

    // 쥐 데이터를 관리할 Dictionary
    public Dictionary<int, Mouse> mouseDictionary = new Dictionary<int, Mouse>();

    private readonly List<string> validValues = new List<string>(15);

    #endregion


    #region Unity Methods

    private void Awake()
    {
        LoadMouseDataFromCSV();
    }

    #endregion


    #region Data Loading

    // CSV 파일을 읽어 Mouse 객체로 변환 후 Dictionary에 저장하는 함수
    public void LoadMouseDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MouseDB");
        if (csvFile == null)
        {
            //Debug.LogError("CSV 파일이 연결되지 않았습니다");
            return;
        }

        using (StringReader stringReader = new StringReader(csvFile.text))
        {
            int lineNumber = 0;

            while (true)
            {
                string line = stringReader.ReadLine();
                if (line == null) break;

                lineNumber++;
                if (lineNumber <= 1) continue;

                string[] values = line.Split(',');

                // 빈 칸을 발견하면 거기까지만 처리
                validValues.Clear();
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
                    ParseAndAddMouse(validValues, lineNumber);
                }
                catch (System.Exception ex)
                {
                    //Debug.LogError($"라인 {lineNumber}: 데이터 처리 중 오류 발생 - {ex.Message}");
                }
            }
        }
    }

    // 마우스 데이터를 파싱하고 Dictionary에 추가하는 함수
    private void ParseAndAddMouse(List<string> values, int lineNumber)
    {
        int mouseId = int.Parse(values[0]);
        string mouseName = values[1];
        int mouseGrade = int.Parse(values[2]);
        double mouseDamage = double.Parse(values[3]);
        double mouseHp = double.Parse(values[4]);
        Sprite mouseImage = LoadSprite(values[5]);
        int numOfAttack = int.Parse(values[6]);
        int mouseAttackSpeed = int.Parse(values[7]);
        int mouseArmor = int.Parse(values[8]);
        int clearCashReward = int.Parse(values[9]);
        decimal clearCoinReward = decimal.Parse(values[10]);
        decimal repeatclearCoinReward = decimal.Parse(values[11]);

        Mouse newMouse = new Mouse(mouseId, mouseName, mouseGrade, mouseDamage, mouseHp, mouseImage,
                                 numOfAttack, mouseAttackSpeed, mouseArmor,
                                 clearCashReward, clearCoinReward, repeatclearCoinReward);

        if (!mouseDictionary.ContainsKey(mouseId))
        {
            mouseDictionary.Add(mouseId, newMouse);
        }
        else
        {
            //Debug.LogWarning($"중복된 MouseId가 발견되었습니다: {mouseId}");
        }
    }

    #endregion


    #region Resource Loading

    // Resources 폴더에서 스프라이트 로드하는 함수
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Mouses/" + path);
        if (sprite == null)
        {
            //Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
        }
        return sprite;
    }

    #endregion


}

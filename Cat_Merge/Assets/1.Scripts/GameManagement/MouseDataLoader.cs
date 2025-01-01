using System.Collections.Generic;
using System.IO;
using UnityEngine;

// CatDataLoader Script
[DefaultExecutionOrder(-3)]     // 스크립트 실행 순서 조정 (1번째)
public class MouseDataLoader : MonoBehaviour
{
    // 쥐 데이터를 관리할 Dictionary
    public Dictionary<int, Mouse> mouseDictionary = new Dictionary<int, Mouse>();

    // ======================================================================================================================

    private void Awake()
    {
        LoadMouseDataFromCSV();
    }

    // ======================================================================================================================

    // CSV 파일을 읽어 Mouse 객체로 변환 후 Dictionary에 저장
    public void LoadMouseDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("MouseDB");
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일이 연결되지 않았습니다");
            return;
        }

        StringReader stringReader = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = stringReader.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 2) continue;
            if (lineNumber >= 6) continue;

            string[] values = line.Split(',');

            // 빈 칸을 발견하면 거기까지만 처리
            List<string> validValues = new List<string>();
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    //Debug.Log($"라인 {lineNumber}: 빈 칸 발견 - 해당 칸 이후 데이터를 무시합니다.");
                    break;
                }
                validValues.Add(value);
            }

            try
            {
                // 데이터 파싱
                int mouseId = int.Parse(validValues[0]);
                string mouseName = validValues[1];
                int mouseGrade = int.Parse(validValues[2]);
                int mouseDamage = int.Parse(validValues[3]);
                int mouseHp = int.Parse(validValues[4]);
                Sprite mouseImage = LoadSprite(validValues[5]);
                int numOfAttack = int.Parse(validValues[6]);

                // Mouse 객체 생성 및 Dictionary에 추가
                Mouse newMouse = new Mouse(mouseId, mouseName, mouseGrade, mouseDamage, mouseHp, mouseImage, numOfAttack);
                if (!mouseDictionary.ContainsKey(mouseId))
                {
                    mouseDictionary.Add(mouseId, newMouse);
                }
                else
                {
                    Debug.LogWarning($"중복된 MouseId가 발견되었습니다: {mouseId}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"라인 {lineNumber}: 데이터 처리 중 오류 발생 - {ex.Message}");
            }
        }

        //Debug.Log("쥐 데이터 로드 완료: " + mouseDictionary.Count + "개");
    }

    // Resources 폴더에서 스프라이트 로드
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Mouses/" + path);
        if (sprite == null)
        {
            Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
        }
        return sprite;
    }
}

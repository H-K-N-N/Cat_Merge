using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class FriendshipManager : MonoBehaviour
{
    // 먹이 소환시 경험치 1 획득
    // 동일등급 머지시 경험치 2 획득
    // 구매 소환시 경험치 1 획득
    // 하위등급이 머지가 되어 소환될 때 상위등급 경험치 1획득

    public static FriendshipManager Instance { get; private set; }

    public List<(int[] exp, int[] reward, int[] passive)> listByGrade = new List<(int[] exp, int[] reward, int[] passive)>();

    [SerializeField]
    public TextMeshProUGUI expRequirementText;
    [SerializeField]
    public Slider expGauge;

    //object[] grade = new object[60];

    public int nowExp;
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

        InitializeGradeList();
    }

    private void Update()
    {
         //expRequirementText.text= $"{nowExp} / {FriendshipDataLoader.Instance.levelDataList[0].expRequirements[0]}";
    }

    private void Start()
    {
        nowExp = 0;
        expRequirementText.text= ($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");
        expGauge.value = 0.00f;

        //Debug.Log($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");
    }

    private void InitializeGradeList()
    {
        for (int grade = 1; grade <= 60; grade++)
        {
            var gradeData = FriendshipDataLoader.Instance.GetDataByGrade(grade);

            if (gradeData != null)
            {
                int[] exp = new int[5];
                int[] rewards = new int[5];
                int[] passiveEffects = new int[5];

                // 데이터를 리스트에 채움
                for (int i = 0; i < 5; i++)
                {
                    exp[i] = gradeData[i].exp;
                    rewards[i] = gradeData[i].reward;
                    passiveEffects[i] = gradeData[i].passive;
                }

                // listByGrade에 추가
                listByGrade.Add((exp, rewards, passiveEffects));
            }
            else
            {
                Debug.LogError($"등급 {grade}에 대한 데이터가 없습니다!");
            }
        }
    }
    //private void a()
    //{
    //    // FriendshipDataLoader에서 데이터 사용하기
    //    if (FriendshipDataLoader.Instance != null && FriendshipDataLoader.Instance.levelDataList.Count > 0)
    //    {
    //        // 첫 번째 레벨 데이터 예시로 출력
    //        var firstLevelData = FriendshipDataLoader.Instance.levelDataList[0]; // 첫 번째 데이터 가져오기
    //        Debug.Log($"첫 번째 레벨: {firstLevelData.grade}");
    //        for (int i = 0; i < 5; i++)
    //        {
    //            Debug.Log($"단계 {i + 1} - 경험치 요구량: {firstLevelData.expRequirements[i]}, 보상: {firstLevelData.rewards[i]}, 패시브 효과: {firstLevelData.passiveEffects[i]}");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("FriendshipDataLoader의 데이터가 로드되지 않았습니다!");
    //    }
    //}
}

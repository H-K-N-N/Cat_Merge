using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;


[System.Serializable]
public class CatFriendship
{
    public int catGrade;           // 고양이 등급
    public int currentExp;         // 현재 경험치
    public int currentLevel;       // 현재 레벨 (0-4)
    public bool[] rewardsClaimed;  // 각 레벨별 보상 수령 여부

    public CatFriendship(int grade)
    {
        catGrade = grade;
        currentExp = 0;
        currentLevel = 0;
        rewardsClaimed = new bool[5];
    }

    
}

public class FriendshipManager : MonoBehaviour
{
    // 먹이 소환시 경험치 1 획득
    // 동일등급 머지시 경험치 2 획득
    // 구매 소환시 경험치 1 획득
    // 하위등급이 머지가 되어 소환될 때 상위등급 경험치 1획득

    public static FriendshipManager Instance { get; private set; }

    //public List<(int[] exp, int[] reward, int[] passive)> listByGrade = new List<(int[] exp, int[] reward, int[] passive)>();

    [SerializeField] public TextMeshProUGUI expRequirementText;
    [SerializeField] public Slider expGauge;

    //public int nowExp;
    //public bool[] unlockLv = new bool[5];

    // ----------------------------------------------------

    // 각 고양이별 호감도 정보 저장
    public Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // 기준이 되는 1등급 데이터 캐싱
    public List<(int grade, int exp, int reward, int passive)> baseData;

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

        //InitializeGradeList();
        InitializeCatFriendships();

    }
    private void Start()
    {
        //unlockLv[0] = true;
        //unlockLv[1] = false;
        //unlockLv[2] = false;
        //unlockLv[3] = false;
        //unlockLv[4] = false;

        //expRequirementText.text= ($"{nowExp} / {FriendshipDataLoader.Instance.GetDataByGrade(1)[0].exp}");

        // 1등급 데이터를 기준 데이터로 캐싱
        baseData = FriendshipDataLoader.Instance.GetDataByGrade(1);

        //UpdateFriendshipUI(2);
        expGauge.value = 0.00f;

       
    }

    private void Update()
    {
        //for (int i = 0; i < unlockLv.Length; i++)
        //{
        //    if (unlockLv[i])
        //    {
        //        int requiredExp = FriendshipDataLoader.Instance.GetDataByGrade(1)[i].exp;

        //        if (nowExp >= requiredExp)
        //        {
        //            nowExp = requiredExp;
        //        }

        //        expRequirementText.text = $"{nowExp} / {requiredExp}";

        //        UnlockLevels(1, i + 1); // 최적화한 UnlockLevels 함수 사용
        //        break; // 하나만 실행되도록
        //    }
        //}


        // 각 고양이의 최대 경험치 제한 체크
        foreach (var friendship in catFriendships.Values)
        {
            if (friendship.currentExp >= baseData[4].exp)
            {
                friendship.currentExp = baseData[4].exp;
                UpdateFriendshipUI(friendship.catGrade);
            }
        }
    }

    private void InitializeCatFriendships()
    {
        // Initialize all cat friendships
        for (int i = 1; i <= 60; i++)
        {
            catFriendships[i] = new CatFriendship(i);
        }
    }

    // 경험치 추가
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];

        // 최대 레벨의 경험치를 넘지 않도록 제한
        int maxExp = baseData[4].exp;
        friendship.currentExp = Mathf.Min(friendship.currentExp + expAmount, maxExp);

        // 레벨업 체크
        CheckAndUpdateLevel(catGrade);

        // UI 업데이트
        UpdateFriendshipUI(friendship.catGrade);
    }

    // 레벨 체크 및 업데이트
    private void CheckAndUpdateLevel(int catGrade)
    {
        var friendship = catFriendships[catGrade];

        for (int i = 4; i >= 0; i--)
        {
            if (friendship.currentExp >= baseData[i].exp)
            {
                friendship.currentLevel = i;
                break;
            }
        }
    }

    // 보상 수령 가능 여부 확인
    public bool CanClaimReward(int catGrade)
    {
        var friendship = catFriendships[catGrade];
        return !friendship.rewardsClaimed[friendship.currentLevel];
    }

    // 보상 수령
    public int ClaimReward(int catGrade)
    {
        var friendship = catFriendships[catGrade];
        if (!CanClaimReward(catGrade)) return 0;

        int reward = baseData[friendship.currentLevel].reward;
        friendship.rewardsClaimed[friendship.currentLevel] = true;
        return reward;
    }

    // 특정 고양이의 호감도 정보 가져오기
    public (int currentExp, int currentLevel, int nextLevelExp, int reward, int passive) GetFriendshipInfo(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade))
            return (0, 0, 0, 0, 0);

        var friendship = catFriendships[catGrade];

        List<(int grade, int exp, int reward, int passive)> data = FriendshipDataLoader.Instance.GetDataByGrade(catGrade);
        int nextLevelExp = data[0].exp;

        return (
            friendship.currentExp,
            friendship.currentLevel,
            nextLevelExp,
            baseData[friendship.currentLevel].reward,
            baseData[friendship.currentLevel].passive
        );
    }


    // UI 업데이트
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        // 현재 선택된 고양이가 있고, 그 고양이의 정보를 보고 있다면 해당 고양이의 정보를 업데이트
        int selectedGrade = DictionaryManager.Instance.GetCurrentSelectedCatGrade();

        if (selectedGrade != -1)
        {
            catGrade = selectedGrade;
        }


        var info = GetFriendshipInfo(catGrade);

        if (expRequirementText != null)
        {
            if (info.currentLevel == 0)
            {
                if (info.currentExp >= info.nextLevelExp)
                {
                    //DictionaryManager.Instance.friendshipUnlockButtonss[info.currentLevel].gameObject.transform.Find("LockBG").gameObject.SetActive(false);
                    //DictionaryManager.Instance.friendshipUnlockButtonss[info.currentLevel].gameObject.transform.Find("FirstOpenBG").gameObject.SetActive(true);

                    // DictionaryManager.Instance.characterButtons[catGrade][info.currentLevel].gameObject.transform.Find("LockBG").gameObject.SetActive(false);               
                    // DictionaryManager.Instance.characterButtons[catGrade][info.currentLevel].gameObject.transform.Find("FirstOpenBG").gameObject.SetActive(true);

                    // buttons[info.currentLevel].gameObject.transform.Find("LockBG").gameObject.SetActive(false);
                    //buttons[info.currentLevel].gameObject.transform.Find("FirstOpenBG").gameObject.SetActive(false);

                }
            }

            Debug.Log($"{catGrade}등급의 고양이의 현재 경험치 : {info.currentExp}, 현재 레벨 : {info.currentLevel}");
            expRequirementText.text = $"{info.currentExp} / {info.nextLevelExp}";
        }

        if (expGauge != null)
        {
            expGauge.value = (float)info.currentExp / info.nextLevelExp;
        }
    }

    // 특정 레벨의 보상을 수령할 수 있는지 확인
    public bool CanClaimLevelReward(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.currentExp >= baseData[level].exp && !friendship.rewardsClaimed[level];
    }

    //private void UnlockLevels(int grade, int level)
    //{
    //    if (nowExp < FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp)
    //        return;

    //    nowExp = FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp;

    //    // 버튼, UI 변경
    //    DictionaryManager.Instance.friendshipUnlockButtons[level - 1].interactable = true;
    //    DictionaryManager.Instance.friendshipLockImg[level - 1].SetActive(false);
    //    DictionaryManager.Instance.friendshipGetCrystalImg[level - 1].SetActive(true);

    //    // 총 호감도 레벨 달성 별 표시
    //    RectTransform rectTransform = DictionaryManager.Instance.friendshipStarImg[0].GetComponent<RectTransform>();
    //    Vector2 offset = rectTransform.offsetMax;
    //    offset.x = -168f + (42f * (level - 1)); // level에 따라 위치 자동 변경
    //    rectTransform.offsetMax = offset;

    //    // 개별 호감도 레벨 별 표시
    //    rectTransform = DictionaryManager.Instance.friendshipStarImg[level].GetComponent<RectTransform>();
    //    offset = rectTransform.offsetMax;
    //    offset.x = -168f + (42f * (level - 1));
    //    rectTransform.offsetMax = offset;

    //    unlockLv[level - 1] = false;

    //    //if (nowExp[grade - 1] < FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp)
    //    //    return;

    //    //nowExp[grade - 1] = FriendshipDataLoader.Instance.GetDataByGrade(grade)[level - 1].exp;

    //    //// 버튼, UI 변경
    //    //DictionaryManager.Instance.friendshipUnlockButtons[grade - 1][level - 1].interactable = true;
    //    //DictionaryManager.Instance.friendshipLockImg[grade - 1][level - 1].SetActive(false);
    //    //DictionaryManager.Instance.friendshipGetCrystalImg[grade - 1][level - 1].SetActive(true);

    //    //// 총 호감도 레벨 달성 별 표시
    //    //RectTransform rectTransform = DictionaryManager.Instance.friendshipStarImg[grade - 1][0].GetComponent<RectTransform>();
    //    //Vector2 offset = rectTransform.offsetMax;
    //    //offset.x = -168f + (42f * (level - 1)); // level에 따라 위치 자동 변경
    //    //rectTransform.offsetMax = offset;

    //    //// 개별 호감도 레벨 별 표시
    //    //rectTransform = DictionaryManager.Instance.friendshipStarImg[grade - 1][level].GetComponent<RectTransform>();
    //    //offset = rectTransform.offsetMax;
    //    //offset.x = -168f + (42f * (level - 1));
    //    //rectTransform.offsetMax = offset;

    //    //unlockLv[grade - 1, level - 1] = false;
    //}

    //private void InitializeGradeList()
    //{
    //    for (int grade = 1; grade <= FriendshipDataLoader.Instance.dataByGrade.Count; grade++)
    //    {
    //        var gradeData = FriendshipDataLoader.Instance.GetDataByGrade(grade);

    //        if (gradeData != null)
    //        {
    //            int[] exp = new int[5];
    //            int[] rewards = new int[5];
    //            int[] passiveEffects = new int[5];

    //            // 데이터를 리스트에 채움
    //            for (int i = 0; i < 5; i++)
    //            {
    //                exp[i] = gradeData[i].exp;
    //                rewards[i] = gradeData[i].reward;
    //                passiveEffects[i] = gradeData[i].passive;
    //            }

    //            // listByGrade에 추가
    //            baseData.Add((grade, exp, rewards, passiveEffects));
    //        }
    //        else
    //        {
    //            Debug.LogError($"등급 {grade}에 대한 데이터가 없습니다!");
    //        }
    //    }
    //}

    // ------------------------------------------------------------------------------------------------

    ///////////////////////// 참조 0개

    // 현재 레벨의 패시브 효과 값 반환
    public int GetCurrentPassiveEffect(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return 0;

        var friendship = catFriendships[catGrade];
        return baseData[friendship.currentLevel].passive;
    }

    // 저장/로드 기능 추가 (나중에 구현)
    public void SaveFriendshipData()
    {
        // TODO: 호감도 데이터 저장 구현
    }

    public void LoadFriendshipData()
    {
        // TODO: 호감도 데이터 로드 구현
    }

    // 특정 레벨의 보상 수령
    public int ClaimLevelReward(int catGrade, int level)
    {
        if (!CanClaimLevelReward(catGrade, level)) return 0;

        var friendship = catFriendships[catGrade];
        friendship.rewardsClaimed[level] = true;
        return baseData[level].reward;
    }

    // 특정 레벨의 달성 여부 확인
    public bool IsLevelAchieved(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.currentExp >= baseData[level].exp;
    }

    // 특정 고양이의 현재 레벨 가져오기
    public int GetCurrentLevel(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return 0;
        return catFriendships[catGrade].currentLevel + 1;
    }

    // 특정 고양이의 현재 경험치 가져오기
    public int GetCurrentExp(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return 0;
        return catFriendships[catGrade].currentExp;
    }
}

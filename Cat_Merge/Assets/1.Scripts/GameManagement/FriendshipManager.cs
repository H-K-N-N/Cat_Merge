using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class CatFriendship
{
    public int catGrade;           // 고양이 등급
    public int currentExp;         // 현재 경험치
    public bool[] isLevelUnlocked; // 각 레벨 해금 여부
    public bool[] rewardsClaimed;  // 각 레벨별 보상 수령 여부

    public CatFriendship(int grade)
    {
        catGrade = grade;
        currentExp = 0;
        isLevelUnlocked = new bool[5];
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

    // 레벨별 보상 금액 설정
    private int[] rewardAmounts = new int[] { 5, 10, 15, 20, 25 };
    
    [SerializeField] public TextMeshProUGUI expRequirementText;
    [SerializeField] public Slider expGauge;

    // ======================================================================================================================

    // 각 고양이별 호감도 정보 저장
    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // 레벨별 필요 경험치 데이터
    private List<(int exp, int reward)> levelRequirements;

    // ======================================================================================================================

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

        InitializeCatFriendships();
    }


    private void Start()
    {
        // 레벨별 필요 경험치 데이터 초기화
        levelRequirements = FriendshipDataLoader.Instance.GetDataByGrade(1)
            .Select(data => (data.exp, data.reward))
            .ToList();

        expGauge.value = 0f;
    }


    private void Update()
    {
        // 각 고양이의 최대 경험치 제한 체크
        foreach (var friendship in catFriendships.Values)
        {
            if (friendship.currentExp >= levelRequirements[4].exp)
            {
                friendship.currentExp = levelRequirements[4].exp;
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

    // 경험치 추가 및 레벨 체크
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];
        friendship.currentExp += expAmount;

        // 최대 경험치 제한
        if (friendship.currentExp >= levelRequirements[4].exp)
        {
            friendship.currentExp = levelRequirements[4].exp;
        }

        // 각 레벨 해금 상태 체크
        for (int i = 0; i < 5; i++)
        {
            if (friendship.currentExp >= levelRequirements[i].exp)
            {
                friendship.isLevelUnlocked[i] = true;
            }
        }

        UpdateFriendshipUI(catGrade);
    }

    // UI 업데이트
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];

        // 현재 선택된 고양이가 있고, 그 고양이의 정보를 보고 있을 때만 UI 업데이트
        int selectedGrade = DictionaryManager.Instance.GetCurrentSelectedCatGrade();
        if (selectedGrade != -1 && selectedGrade != catGrade)
        {
            // 선택된 고양이와 다른 고양이의 경험치가 변경된 경우
            // UI 업데이트를 하지 않고 버튼 상태만 업데이트
            DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
            return;
        }

        // 현재 레벨과 다음 레벨 경험치 계산
        int currentLevel = 0;
        int nextLevelExp = levelRequirements[0].exp;

        for (int i = 4; i >= 0; i--)
        {
            if (friendship.currentExp >= levelRequirements[i].exp)
            {
                currentLevel = i;
                nextLevelExp = i < 4 ? levelRequirements[i + 1].exp : levelRequirements[i].exp;
                break;
            }
        }

        // UI 텍스트 업데이트
        if (expRequirementText != null)
        {
            expRequirementText.text = $"{friendship.currentExp} / {nextLevelExp}";
        }

        // 게이지 업데이트
        if (expGauge != null)
        {
            float progress = (float)friendship.currentExp / nextLevelExp;
            expGauge.value = Mathf.Clamp01(progress);
        }

        // 버튼 상태 업데이트
        DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
    }

    // 특정 고양이의 우정도 정보 가져오기
    public (int currentExp, bool[] isUnlocked, bool[] isClaimed) GetFriendshipInfo(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade))
            return (0, new bool[5], new bool[5]);

        var friendship = catFriendships[catGrade];
        return (friendship.currentExp, friendship.isLevelUnlocked, friendship.rewardsClaimed);
    }

    // 보상 수령 가능 여부 확인
    public bool CanClaimLevelReward(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.isLevelUnlocked[level] && !friendship.rewardsClaimed[level];
    }

    // 보상 금액 가져오기
    public int GetRewardAmount(int level)
    {
        if (level >= 0 && level < rewardAmounts.Length)
        {
            return rewardAmounts[level];
        }
        return 0;
    }

    // 보상 수령
    public void ClaimReward(int catGrade, int level)
    {
        if (!CanClaimLevelReward(catGrade, level)) return;

        var friendship = catFriendships[catGrade];
        friendship.rewardsClaimed[level] = true;
        GameManager.Instance.Cash += GetRewardAmount(level);

        UpdateFriendshipUI(catGrade);
    }

}

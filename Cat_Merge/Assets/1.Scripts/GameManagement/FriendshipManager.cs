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

    // 각 고양이별 애정도 정보 저장
    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // 레벨별 필요 경험치 데이터
    private Dictionary<int, List<(int exp, int reward)>> levelByGrade = new Dictionary<int, List<(int exp, int reward)>>();

    // 현재 레벨을 추적하기 위한 변수 추가
    private Dictionary<int, int> currentLevels = new Dictionary<int, int>();

    // FriendshipManager 클래스에 다음 변수 추가
    private Dictionary<int, bool[]> buttonUnlockStatus = new Dictionary<int, bool[]>();

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
        InitializeCurrentLevels();
    }

    private void Start()
    {
        // 모든 등급의 경험치 데이터를 초기화
        for (int i = 0; i < 3; i++)
        {
            levelByGrade[i] = FriendshipDataLoader.Instance.GetDataByGrade(i + 1)
                .Select(data => (data.exp, data.reward))
                .ToList();
        }

        expGauge.value = 0f;
    }

    private void InitializeCatFriendships()
    {
        // Initialize all cat friendships and button states
        for (int i = 1; i <= 60; i++)
        {
            catFriendships[i] = new CatFriendship(i);
            buttonUnlockStatus[i] = new bool[5]; // 각 고양이별로 5개의 버튼 상태 저장
        }
    }

    private void InitializeCurrentLevels()
    {
        // 모든 고양이의 현재 레벨을 0으로 초기화
        for (int i = 1; i <= 60; i++)
        {
            currentLevels[i] = 0;
        }
    }

    // 경험치 추가 및 레벨 체크
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade) || IsMaxLevel(catGrade)) return; // MAX 레벨이면 경험치 획득 중단

        var friendship = catFriendships[catGrade];
        friendship.currentExp += expAmount;

        UpdateFriendshipUI(catGrade);
    }

    // UI 업데이트
    public void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        // 현재 Dictionary에서 선택된 고양이와 업데이트하려는 고양이가 다르면 UI 업데이트 스킵
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() != catGrade &&
            DictionaryManager.Instance.GetCurrentSelectedCatGrade() != -1)
        {
            DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
            return;
        }

        var friendship = catFriendships[catGrade];
        int currentLevel = currentLevels[catGrade];

        // 버튼 클릭으로 인한 보상 획득 처리
        if (DictionaryManager.Instance.buttonClick)
        {
            // 현재 레벨의 경험치만큼 차감
            friendship.currentExp -= levelByGrade[catGrade - 1][currentLevel].exp;

            // 다음 레벨 계산
            currentLevel = Mathf.Min(currentLevel + 1, 4);
            currentLevels[catGrade] = currentLevel;

            DictionaryManager.Instance.buttonClick = false;
        }

        // UI 텍스트 업데이트
        if (expRequirementText != null && DictionaryManager.Instance.GetCurrentSelectedCatGrade() == catGrade)
        {
            if (IsMaxLevel(catGrade))
            {
                expRequirementText.text = "MAX";
                if (expGauge != null)
                {
                    expGauge.value = 1f; // 게이지를 최대로 채움
                }
            }
            else
            {
                int nextExp = GetNextLevelExp(catGrade);
                expRequirementText.text = $"{friendship.currentExp} / {nextExp}";

                // 게이지 업데이트
                if (expGauge != null)
                {
                    float progress = (float)friendship.currentExp / nextExp;
                    expGauge.value = Mathf.Clamp01(progress);
                }
            }
        }

        // 레벨 해금 상태 업데이트
        UpdateLevelUnlockStatus(friendship, catGrade);

        // 버튼 상태 업데이트
        DictionaryManager.Instance.UpdateFriendshipButtonStates(catGrade);
    }

    // UpdateLevelUnlockStatus 메서드 수정
    private void UpdateLevelUnlockStatus(CatFriendship friendship, int catGrade)
    {
        int currentLevel = currentLevels[catGrade];

        for (int i = 0; i < 5; i++)
        {
            // 이미 보상을 받은 레벨은 항상 해금 상태 유지
            if (friendship.rewardsClaimed[i])
            {
                friendship.isLevelUnlocked[i] = true;
                buttonUnlockStatus[catGrade][i] = true;
                continue;
            }

            if (i <= currentLevel)
            {
                // 현재 레벨까지만 경험치 체크
                bool isUnlocked = friendship.currentExp >= levelByGrade[catGrade - 1][i].exp;
                friendship.isLevelUnlocked[i] = isUnlocked;
                buttonUnlockStatus[catGrade][i] = isUnlocked;
            }
            else
            {
                // 이후 레벨은 모두 잠금 (보상을 받지 않은 레벨만)
                friendship.isLevelUnlocked[i] = false;
                buttonUnlockStatus[catGrade][i] = false;
            }
        }
    }

    // 특정 고양이의 애정도 정보 가져오기
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

    // 새로운 메서드 추가
    private int GetNextLevelExp(int catGrade)
    {
        int currentLevel = currentLevels[catGrade];
        return levelByGrade[catGrade - 1][currentLevel].exp;
    }

    // FriendshipManager에 새로운 메서드 추가
    public bool IsMaxLevel(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;
        var friendship = catFriendships[catGrade];
        return friendship.rewardsClaimed.All(claimed => claimed); // 모든 보상을 받았는지 확인
    }

}

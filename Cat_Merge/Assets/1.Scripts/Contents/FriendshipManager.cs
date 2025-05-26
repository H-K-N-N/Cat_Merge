using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

[Serializable]
public class CatFriendship
{
    public int catGrade;                        // 고양이 등급
    public int currentExp;                      // 현재 경험치
    public bool[] isLevelUnlocked;              // 각 레벨 해금 여부
    public bool[] rewardsClaimed;               // 각 레벨별 보상 수령 여부
    public List<string> activePassiveEffects;   // 활성화된 패시브 효과 목록

    public CatFriendship(int grade)
    {
        catGrade = grade;
        currentExp = 0;
        isLevelUnlocked = new bool[5];
        rewardsClaimed = new bool[5];
        activePassiveEffects = new List<string>();
    }
}

// 고양이 애정도 스크립트
[DefaultExecutionOrder(-4)]
public class FriendshipManager : MonoBehaviour, ISaveable
{


    #region Variables

    // 먹이 소환시 경험치 1 획득
    // 동일등급 머지시 경험치 2 획득
    // 구매 소환시 경험치 1 획득
    // 하위등급이 머지가 되어 소환될 때 상위등급 경험치 1획득

    public static FriendshipManager Instance { get; private set; }

    // 레벨별 보상 금액 설정
    private readonly int[] rewardAmounts = new int[] { 5, 10, 15, 20, 25 };

    [Header("---[UI References]")]
    [SerializeField] private Transform friendshipButtonParent;
    [SerializeField] private Button[] friendshipButtonPrefabs;
    [SerializeField] private Transform fullStarImgParent;
    [SerializeField] private GameObject fullStarPrefab;
    [SerializeField] private TextMeshProUGUI expRequirementText;
    [SerializeField] private Slider expGauge;
    [SerializeField] private Transform dictionarySlotParent;

    private Dictionary<int, Button[]> catFriendshipButtons = new Dictionary<int, Button[]>();
    private Dictionary<int, GameObject> fullStars = new Dictionary<int, GameObject>();
    private Button[] activeButtons;
    private bool buttonClick = false;

    private static readonly Vector2 defaultOffset = new Vector2(-210, 0);
    private const float STAR_SPACING = 42f;

    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();                           // 각 고양이별 애정도 정보 저장
    private Dictionary<int, List<(int exp, int reward)>> levelByGrade = new Dictionary<int, List<(int exp, int reward)>>(); // 레벨별 필요 경험치 데이터
    private Dictionary<int, int> currentLevels = new Dictionary<int, int>();                                                // 현재 레벨을 추적하기 위한 변수
    private Dictionary<int, bool[]> buttonUnlockStatus = new Dictionary<int, bool[]>();                                     // FriendshipManager 클래스에 다음 변수


    private bool isDataLoaded = false;          // 데이터 로드 확인

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
        }
    }

    private void Start()
    {
        InitializeFriendshipData();

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            InitializeCatFriendships();
            InitializeCurrentLevels();
        }

        InitializeUI();
    }

    private void OnDestroy()
    {
        CleanupUI();
    }

    #endregion


    #region Initialize

    // 각 고양이별 애정도 데이터 초기화 함수
    private void InitializeCatFriendships()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            catFriendships[i] = new CatFriendship(i);
            buttonUnlockStatus[i] = new bool[5]; // 각 고양이별로 5개의 버튼 상태 저장
        }
    }

    // 모든 고양이의 현재 레벨 초기화 함수
    private void InitializeCurrentLevels()
    {
        // 모든 고양이의 현재 레벨을 0으로 초기화
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            currentLevels[i] = 0;
        }
    }

    // UI 컴포넌트 초기화 함수
    private void InitializeUI()
    {
        InitializeFriendshipButtons();
        InitializeFullStars();
        if (expGauge != null) expGauge.value = 0f;
    }

    // 애정도 버튼 생성 및 초기화 함수
    private void InitializeFriendshipButtons()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            Button[] buttons = new Button[5];
            for (int j = 0; j < 5; j++)
            {
                buttons[j] = Instantiate(friendshipButtonPrefabs[j], friendshipButtonParent);
                buttons[j].gameObject.SetActive(false);

                int level = j;
                int catGrade = i;
                buttons[j].onClick.AddListener(() => OnFriendshipButtonClick(catGrade, level));
            }
            catFriendshipButtons[i] = buttons;
        }
    }

    // fullStar 초기화 함수
    private void InitializeFullStars()
    {
        for (int i = 1; i <= GameManager.Instance.AllCatData.Length; i++)
        {
            GameObject fullStar = Instantiate(fullStarPrefab, fullStarImgParent);
            fullStar.gameObject.SetActive(false);
            fullStars[i] = fullStar;
        }
    }

    // 애정도 데이터 초기화 함수
    private void InitializeFriendshipData()
    {
        // 모든 등급의 경험치 데이터를 초기화
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            levelByGrade[i] = FriendshipDataLoader.Instance.GetDataByGrade(i + 1)
                .Select(data => (data.exp, data.reward))
                .ToList();
        }
    }

    #endregion


    #region UI Management

    // 고양이 선택시 UI 업데이트 함수
    public void OnCatSelected(int catGrade)
    {
        DeactivateCurrentButtons();
        ActivateButtonsForCat(catGrade);
        UpdateFullStarVisibility(catGrade);
        UpdateFriendshipUI(catGrade);
    }

    // 현재 활성화된 버튼들 비활성화 함수
    private void DeactivateCurrentButtons()
    {
        if (activeButtons != null)
        {
            foreach (var button in activeButtons)
            {
                if (button != null)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }

    // 선택된 고양이의 버튼들 활성화 함수
    private void ActivateButtonsForCat(int catGrade)
    {
        if (catFriendshipButtons.TryGetValue(catGrade, out Button[] buttons))
        {
            activeButtons = buttons;
            foreach (var button in buttons)
            {
                button.gameObject.SetActive(true);
                UpdateButtonUI(button, catGrade);
            }
        }
    }

    // 버튼 UI 업데이트 함수
    private void UpdateButtonUI(Button button, int catGrade)
    {
        // 보상 금액 텍스트 설정
        Transform firstOpenBG = button.transform.Find("FirstOpenBG");
        if (firstOpenBG != null)
        {
            TextMeshProUGUI cashText = firstOpenBG.Find("Cash Text")?.GetComponent<TextMeshProUGUI>();
            if (cashText != null)
            {
                int level = System.Array.IndexOf(catFriendshipButtons[catGrade], button);
                int rewardAmount = GetRewardAmount(level);
                cashText.text = $"+ {rewardAmount}";
            }
        }
    }

    // fullStar 상태 업데이트 함수
    private void UpdateFullStarVisibility(int catGrade)
    {
        foreach (var pair in fullStars)
        {
            if (pair.Value != null)
            {
                pair.Value.gameObject.SetActive(pair.Key == catGrade);
            }
        }
    }

    // UI 리셋 함수
    public void ResetUI()
    {
        DeactivateCurrentButtons();
        foreach (var star in fullStars.Values)
        {
            if (star != null)
            {
                star.gameObject.SetActive(false);
            }
        }
        if (expGauge != null) expGauge.value = 0f;
        if (expRequirementText != null) expRequirementText.text = "";
    }

    // UI 요소 정리 함수(OnDestroy)
    private void CleanupUI()
    {
        foreach (var buttons in catFriendshipButtons.Values)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
        }

        foreach (var star in fullStars.Values)
        {
            if (star != null)
            {
                Destroy(star.gameObject);
            }
        }

        catFriendshipButtons.Clear();
        fullStars.Clear();
    }

    #endregion


    #region Friendship System

    // 애정도 버튼 클릭 처리 함수
    private void OnFriendshipButtonClick(int catGrade, int level)
    {
        if (CanClaimLevelReward(catGrade, level))
        {
            ClaimReward(catGrade, level);
            buttonClick = true;
            UpdateFriendshipUI(catGrade);
        }
    }

    // 경험치 추가 및 레벨 체크 함수
    public void AddExperience(int catGrade, int expAmount)
    {
        if (!catFriendships.ContainsKey(catGrade) || IsMaxLevel(catGrade)) return; // MAX 레벨이면 경험치 획득 중단

        var friendship = catFriendships[catGrade];
        friendship.currentExp += expAmount;

        UpdateFriendshipUI(catGrade);
    }

    // 애정도 UI 업데이트 함수
    private void UpdateFriendshipUI(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return;

        var friendship = catFriendships[catGrade];
        int currentLevel = currentLevels[catGrade];

        // 버튼 클릭으로 인한 보상 획득 처리
        if (buttonClick)
        {
            // 현재 레벨의 경험치만큼 차감
            friendship.currentExp -= levelByGrade[catGrade - 1][currentLevel].exp;

            // 다음 레벨 계산
            currentLevel = Mathf.Min(currentLevel + 1, 4);
            currentLevels[catGrade] = currentLevel;

            buttonClick = false;
        }

        // 현재 Dictionary에서 선택된 고양이와 업데이트하려는 고양이가 같을 때만 UI 텍스트 업데이트
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() == catGrade)
        {
            if (expRequirementText != null)
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
        }

        // 레벨 해금 상태 업데이트
        UpdateLevelUnlockStatus(friendship, catGrade);

        // 버튼 상태 업데이트 - 현재 선택된 고양이일 때만
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() == catGrade)
        {
            UpdateFriendshipButtonStates(catGrade);
        }

        // 도감 슬롯의 friendshipNewImage 업데이트
        if (dictionarySlotParent != null)
        {
            Transform slot = dictionarySlotParent.GetChild(catGrade - 1);
            if (slot != null)
            {
                GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;
                if (friendshipNewImage != null)
                {
                    bool hasRewards = HasUnclaimedFriendshipRewards(catGrade);
                    friendshipNewImage.SetActive(hasRewards);

                    // DictionaryManager의 New Image 상태도 업데이트
                    DictionaryManager.Instance.UpdateNewImageStatus();
                }
            }
        }
    }

    // 버튼 상태 업데이트 함수
    private void UpdateFriendshipButtonStates(int catGrade)
    {
        if (!catFriendshipButtons.ContainsKey(catGrade)) return;

        var friendshipInfo = GetFriendshipInfo(catGrade);
        var buttons = catFriendshipButtons[catGrade];
        bool canActivateNextLevel = true;

        // MAX 레벨 체크
        bool isMaxLevel = IsMaxLevel(catGrade);

        // 현재 진행 중인 레벨 찾기
        int currentProgressLevel = 0;
        for (int i = 0; i < friendshipInfo.isClaimed.Length; i++)
        {
            if (!friendshipInfo.isClaimed[i])
            {
                currentProgressLevel = i;
                break;
            }
            if (i == friendshipInfo.isClaimed.Length - 1)
            {
                currentProgressLevel = i;
            }
        }

        // 패시브 효과 데이터 가져오기
        var passiveEffects = FriendshipDataLoader.Instance.GetDataByGrade(catGrade);

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button != null)
            {
                bool isAlreadyClaimed = friendshipInfo.isClaimed[i];

                // Passive Text 업데이트
                Transform passiveTextTr = button.transform.Find("Passive Text");
                if (passiveTextTr != null)
                {
                    TextMeshProUGUI passiveText = passiveTextTr.GetComponent<TextMeshProUGUI>();
                    if (passiveText != null)
                    {
                        passiveText.text = passiveEffects[i].passive;
                    }
                }

                // LockBG 상태 업데이트
                Transform lockBG = button.transform.Find("LockBG");
                if (lockBG != null)
                {
                    bool shouldLock = !isAlreadyClaimed && (!canActivateNextLevel || !friendshipInfo.isUnlocked[i]);
                    lockBG.gameObject.SetActive(shouldLock && !isMaxLevel);
                }

                // FirstOpenBG 상태 업데이트
                Transform firstOpenBG = button.transform.Find("FirstOpenBG");
                if (firstOpenBG != null)
                {
                    bool canShowReward = !isAlreadyClaimed && canActivateNextLevel && friendshipInfo.isUnlocked[i];
                    firstOpenBG.gameObject.SetActive(canShowReward && !isMaxLevel);
                }

                // Star 상태 업데이트
                Transform star = FindDeepChild(button.transform, "Star");
                if (star != null)
                {
                    star.gameObject.SetActive(isAlreadyClaimed);
                }

                // 버튼 상호작용 상태 설정
                button.interactable = !isMaxLevel && !isAlreadyClaimed && canActivateNextLevel && friendshipInfo.isUnlocked[i];

                // 다음 레벨 활성화 조건 체크
                if (i == currentProgressLevel && !friendshipInfo.isClaimed[i] && !isMaxLevel)
                {
                    canActivateNextLevel = false;
                }
            }
        }

        // fullStar 업데이트
        UpdateFullStarUI(catGrade, friendshipInfo);
    }

    // fullStar UI 업데이트 함수
    private void UpdateFullStarUI(int catGrade, (int currentExp, bool[] isUnlocked, bool[] isClaimed) friendshipInfo)
    {
        if (!fullStars.ContainsKey(catGrade)) return;

        GameObject fullStar = fullStars[catGrade];
        if (fullStar != null)
        {
            Transform fullStarBG = fullStar.transform.Find("fullStar");
            if (fullStarBG != null)
            {
                Vector2 newOffsetMax = defaultOffset;
                int claimedCount = friendshipInfo.isClaimed.Count(claimed => claimed);
                newOffsetMax.x = defaultOffset.x + (claimedCount * STAR_SPACING);
                fullStarBG.GetComponent<RectTransform>().offsetMax = newOffsetMax;
            }
        }
    }

    // 레벨 해금 상태 업데이트 함수
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

    #endregion


    #region Friendship Passive Effect System

    // 패시브 효과 종류 리스트
    private readonly List<string> passiveEffectTypes = new List<string>
    {
        "공격력 1% 증가",
        "공격력 2% 증가",
        "공격력 3% 증가",
        "공격력 4% 증가",
        "공격력 5% 증가",
        "고양이 보유 숫자 1 증가",
        "공격 속도 0.05초 증가",
        "젤리 획득 속도 0.05초 증가",
        "무료 다이아 획득량 1 증가",
        "무료 다이아 획득 쿨타임 1초 감소",
        "광고 다이아 획득량 5 증가",
        "광고 다이아 획득 쿨타임 1초 감소",
        "젤리 획득 버프 지속 시간 1초 증가",
        "젤리 획득 버프 쿨타임 1초 감소"
    };

    // 패시브 효과 적용 함수
    private void ApplyPassiveEffect(int catGrade, int level)
    {
        var passiveEffect = FriendshipDataLoader.Instance.GetDataByGrade(catGrade)[level].passive;
        var friendship = catFriendships[catGrade];

        if (!friendship.activePassiveEffects.Contains(passiveEffect))
        {
            friendship.activePassiveEffects.Add(passiveEffect);

            //Debug.Log($"[{catGrade}등급 고양이] 활성화된 패시브 효과 목록: {string.Join(", ", friendship.activePassiveEffects)}");

            // 패시브 효과 매칭 및 적용
            int effectIndex = passiveEffectTypes.IndexOf(passiveEffect);
            if (effectIndex != -1)
            {
                switch (effectIndex)
                {
                    case 0:
                        ApplyAttackDamageBuff(0.01f);
                        break;
                    case 1:
                        ApplyAttackDamageBuff(0.02f);
                        break;
                    case 2:
                        ApplyAttackDamageBuff(0.03f);
                        break;
                    case 3:
                        ApplyAttackDamageBuff(0.04f);
                        break;
                    case 4:
                        ApplyAttackDamageBuff(0.05f);
                        break;
                    case 5:
                        ApplyCatCapacityIncrease();
                        break;
                    case 6:
                        ApplyAttackSpeedBuff();
                        break;
                    case 7:
                        ApplyCoinCollectSpeedBuff();
                        break;
                    case 8:
                        ApplyCashForTimeAmountBuff();
                        break;
                    case 9:
                        ApplyCashForTimeCoolTimeBuff();
                        break;
                    case 10:
                        ApplyCashForAdAmountBuff();
                        break;
                    case 11:
                        ApplyCashForAdCoolTimeBuff();
                        break;
                    case 12:
                        ApplyDoubleCoinForAdDurationBuff();
                        break;
                    case 13:
                        ApplyDoubleCoinForAdCoolTimeBuff();
                        break;
                }
            }
        }
    }

    // 공격력 1~5% 증가 효과 패시브 함수
    private void ApplyAttackDamageBuff(float percentage)
    {
        var allCats = GameManager.Instance.AllCatData;
        for (int i = 0; i < allCats.Length; i++)
        {
            if (allCats[i] != null)
            {
                allCats[i].AddPassiveAttackDamageBuff(percentage);
            }
        }

        var activeCats = SpawnManager.Instance.GetActiveCats();
        foreach (var catObj in activeCats)
        {
            catObj.GetComponent<CatData>().SetCatData(catObj.GetComponent<CatData>().catData);
        }
    }

    // 고양이 보유 숫자 1 증가 패시브 함수
    private void ApplyCatCapacityIncrease() 
    {
        GameManager.Instance.AddPassiveCatCapacity(1);
    }

    // 공격 속도 0.05초 증가 패시브 함수
    private void ApplyAttackSpeedBuff() 
    {
        var allCats = GameManager.Instance.AllCatData;
        for (int i = 0; i < allCats.Length; i++)
        {
            if (allCats[i] != null)
            {
                allCats[i].AddPassiveAttackSpeedBuff(0.05f);
            }
        }

        var activeCats = SpawnManager.Instance.GetActiveCats();
        foreach (var catObj in activeCats)
        {
            catObj.GetComponent<CatData>().SetCatData(catObj.GetComponent<CatData>().catData);
        }
    }

    // 젤리 획득 속도 0.05초 증가 패시브 함수
    private void ApplyCoinCollectSpeedBuff()
    {
        var allCats = GameManager.Instance.AllCatData;
        for (int i = 0; i < allCats.Length; i++)
        {
            if (allCats[i] != null)
            {
                allCats[i].AddPassiveCoinCollectSpeedBuff(0.05f);
            }
        }

        var activeCats = SpawnManager.Instance.GetActiveCats();
        foreach (var catObj in activeCats)
        {
            catObj.GetComponent<CatData>().SetCatData(catObj.GetComponent<CatData>().catData);
        }
    }

    // 무료 다이아 획득량 1 증가 패시브 함수
    private void ApplyCashForTimeAmountBuff()
    {
        ShopManager.Instance.AddPassiveCashForTimeAmount(1);
    }

    // 무료 다이아 획득 쿨타임 1초 감소 패시브 함수
    private void ApplyCashForTimeCoolTimeBuff()
    {
        ShopManager.Instance.AddPassiveCashForTimeCoolTimeReduction(1);
    }

    // 광고 다이아 획득량 5 증가 패시브 함수
    private void ApplyCashForAdAmountBuff()
    {
        ShopManager.Instance.AddPassiveCashForAdAmount(5);
    }

    // 광고 다이아 획득 쿨타임 1초 감소 패시브 함수
    private void ApplyCashForAdCoolTimeBuff()
    {
        ShopManager.Instance.AddPassiveCashForAdCoolTimeReduction(1);
    }

    // 젤리 획득 버프 지속 시간 1초 증가 패시브 함수
    private void ApplyDoubleCoinForAdDurationBuff()
    {
        ShopManager.Instance.AddPassiveDoubleCoinDurationIncrease(1f);
    }

    // 젤리 획득 버프 쿨타임 1초 감소 패시브 함수
    private void ApplyDoubleCoinForAdCoolTimeBuff()
    {
        ShopManager.Instance.AddPassiveDoubleCoinForAdCoolTimeReduction(1);
    }

    #endregion


    #region Data Management

    // 특정 고양이의 애정도 정보 조회 함수
    private (int currentExp, bool[] isUnlocked, bool[] isClaimed) GetFriendshipInfo(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade))
        {
            return (0, new bool[5], new bool[5]);
        }

        var friendship = catFriendships[catGrade];
        return (friendship.currentExp, friendship.isLevelUnlocked, friendship.rewardsClaimed);
    }

    // 특정 등급의 고양이가 받을 수 있는 보상이 있는지 확인하는 함수
    public bool HasUnclaimedFriendshipRewards(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        for (int i = 0; i < friendship.isLevelUnlocked.Length; i++)
        {
            // 해당 레벨이 해금되었고, 아직 보상을 받지 않은 상태라면 true 반환
            if (friendship.isLevelUnlocked[i] && !friendship.rewardsClaimed[i])
            {
                return true;
            }
        }
        return false;
    }

    // 보상 수령 가능 여부 확인 함수
    private bool CanClaimLevelReward(int catGrade, int level)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.isLevelUnlocked[level] && !friendship.rewardsClaimed[level];
    }

    // 보상 금액 조회 함수
    private int GetRewardAmount(int level)
    {
        if (level >= 0 && level < rewardAmounts.Length)
        {
            return rewardAmounts[level];
        }
        return 0;
    }

    // 보상 수령 처리 함수
    private void ClaimReward(int catGrade, int level)
    {
        if (!CanClaimLevelReward(catGrade, level)) return;

        var friendship = catFriendships[catGrade];
        friendship.rewardsClaimed[level] = true;

        // Cash 보상 지급
        GameManager.Instance.Cash += GetRewardAmount(level);

        // 패시브 효과 적용
        ApplyPassiveEffect(catGrade, level);

        UpdateFriendshipUI(catGrade);

        // DictionaryManager의 New Image 상태도 업데이트
        DictionaryManager.Instance.UpdateNewImageStatus();
    }

    // 다음 레벨 필요 경험치 조회 함수
    private int GetNextLevelExp(int catGrade)
    {
        int currentLevel = currentLevels[catGrade];
        return levelByGrade[catGrade - 1][currentLevel].exp;
    }

    // 최대 레벨 도달 여부 확인 함수
    private bool IsMaxLevel(int catGrade)
    {
        if (!catFriendships.ContainsKey(catGrade)) return false;

        var friendship = catFriendships[catGrade];
        return friendship.rewardsClaimed.All(claimed => claimed); // 모든 보상을 받았는지 확인
    }

    #endregion


    #region Utility

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform found = FindDeepChild(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public List<CatFriendshipSaveData> friendshipList = new List<CatFriendshipSaveData>();
    }

    [Serializable]
    private class CatFriendshipSaveData
    {
        public int catGrade;                        // 고양이 등급
        public int currentExp;                      // 현재 경험치
        public int currentLevel;                    // 현재 레벨
        public bool[] isLevelUnlocked;              // 레벨 해금 상태
        public bool[] rewardsClaimed;               // 보상 수령 상태
        public List<string> activePassiveEffects;   // 활성화된 패시브 효과 목록
    }

    public string GetSaveData()
    {
        SaveData saveData = new SaveData();

        foreach (var pair in catFriendships)
        {
            saveData.friendshipList.Add(new CatFriendshipSaveData
            {
                catGrade = pair.Value.catGrade,
                currentExp = pair.Value.currentExp,
                currentLevel = currentLevels[pair.Key],
                isLevelUnlocked = pair.Value.isLevelUnlocked,
                rewardsClaimed = pair.Value.rewardsClaimed,
                activePassiveEffects = pair.Value.activePassiveEffects
            });
        }

        return JsonUtility.ToJson(saveData);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // 데이터 복원
        catFriendships.Clear();
        currentLevels.Clear();
        buttonUnlockStatus.Clear();

        foreach (var savedItem in savedData.friendshipList)
        {
            catFriendships[savedItem.catGrade] = new CatFriendship(savedItem.catGrade)
            {
                currentExp = savedItem.currentExp,
                isLevelUnlocked = savedItem.isLevelUnlocked,
                rewardsClaimed = savedItem.rewardsClaimed,
                activePassiveEffects = savedItem.activePassiveEffects ?? new List<string>()
            };

            currentLevels[savedItem.catGrade] = savedItem.currentLevel;
            buttonUnlockStatus[savedItem.catGrade] = new bool[5];

            // 저장된 패시브 효과들을 다시 적용
            if (savedItem.activePassiveEffects != null)
            {
                foreach (var effect in savedItem.activePassiveEffects)
                {
                    int effectIndex = passiveEffectTypes.IndexOf(effect);
                    if (effectIndex != -1)
                    {
                        switch (effectIndex)
                        {
                            case 0:
                                ApplyAttackDamageBuff(0.01f);
                                break;
                            case 1:
                                ApplyAttackDamageBuff(0.02f);
                                break;
                            case 2:
                                ApplyAttackDamageBuff(0.03f);
                                break;
                            case 3:
                                ApplyAttackDamageBuff(0.04f);
                                break;
                            case 4:
                                ApplyAttackDamageBuff(0.05f);
                                break;
                            case 5:
                                ApplyCatCapacityIncrease();
                                break;
                            case 6:
                                ApplyAttackSpeedBuff();
                                break;
                            case 7:
                                ApplyCoinCollectSpeedBuff();
                                break;
                            case 8:
                                ApplyCashForTimeAmountBuff();
                                break;
                            case 9:
                                ApplyCashForTimeCoolTimeBuff();
                                break;
                            case 10:
                                ApplyCashForAdAmountBuff();
                                break;
                            case 11:
                                ApplyCashForAdCoolTimeBuff();
                                break;
                            case 12:
                                ApplyDoubleCoinForAdDurationBuff();
                                break;
                            case 13:
                                ApplyDoubleCoinForAdCoolTimeBuff();
                                break;
                        }
                    }
                }
            }
        }

        StartCoroutine(UpdateAllFriendshipUI());

        isDataLoaded = true;
    }

    // 데이터 로드 후 애정도 UI 업데이트 코루틴
    private IEnumerator UpdateAllFriendshipUI()
    {
        // 한 프레임 대기
        yield return null;

        // 해금된 모든 고양이의 애정도 상태 업데이트
        if (DictionaryManager.Instance != null)
        {
            for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
            {
                if (DictionaryManager.Instance.IsCatUnlocked(i))
                {
                    // 각 고양이의 UI 업데이트
                    if (dictionarySlotParent != null)
                    {
                        Transform slot = dictionarySlotParent.GetChild(i);
                        if (slot != null)
                        {
                            GameObject friendshipNewImage = slot.transform.Find("Button/New Image")?.gameObject;
                            if (friendshipNewImage != null)
                            {
                                bool hasRewards = HasUnclaimedFriendshipRewards(i + 1);
                                friendshipNewImage.SetActive(hasRewards);
                            }
                        }
                    }
                }
            }
        }

        // 모든 UI 업데이트가 완료된 후 DictionaryManager의 New Image 상태 업데이트
        if (DictionaryManager.Instance != null)
        {
            DictionaryManager.Instance.UpdateNewImageStatus();
        }
    }

    #endregion


}

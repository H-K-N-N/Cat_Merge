using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

[Serializable]
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

// 고양이 애정도 스크립트
public class FriendshipManager : MonoBehaviour, ISaveable
{


    #region Variables

    // 먹이 소환시 경험치 1 획득
    // 동일등급 머지시 경험치 2 획득
    // 구매 소환시 경험치 1 획득
    // 하위등급이 머지가 되어 소환될 때 상위등급 경험치 1획득

    public static FriendshipManager Instance { get; private set; }

    // 레벨별 보상 금액 설정
    private int[] rewardAmounts = new int[] { 5, 10, 15, 20, 25 };

    [Header("---[UI References]")]
    [SerializeField] private Transform friendshipButtonParent;
    [SerializeField] private Button[] friendshipButtonPrefabs;
    [SerializeField] private Transform fullStarImgParent;
    [SerializeField] private GameObject fullStarPrefab;
    [SerializeField] private TextMeshProUGUI expRequirementText;
    [SerializeField] private Slider expGauge;

    private Dictionary<int, Button[]> catFriendshipButtons = new Dictionary<int, Button[]>();
    private Dictionary<int, GameObject> fullStars = new Dictionary<int, GameObject>();
    private Button[] activeButtons;
    private bool buttonClick = false;

    // 각 고양이별 애정도 정보 저장
    private Dictionary<int, CatFriendship> catFriendships = new Dictionary<int, CatFriendship>();

    // 레벨별 필요 경험치 데이터
    private Dictionary<int, List<(int exp, int reward)>> levelByGrade = new Dictionary<int, List<(int exp, int reward)>>();

    // 현재 레벨을 추적하기 위한 변수 추가
    private Dictionary<int, int> currentLevels = new Dictionary<int, int>();

    // FriendshipManager 클래스에 다음 변수 추가
    private Dictionary<int, bool[]> buttonUnlockStatus = new Dictionary<int, bool[]>();


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
        InitializeCatFriendships();
        InitializeCurrentLevels();

        InitializeUI();
        InitializeFriendshipData();

        if (!isDataLoaded)
        {

        }
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

    // UI 요소 정리 (OnDestroy)
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


    #region 애정도 System

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

        // 현재 Dictionary에서 선택된 고양이와 업데이트하려는 고양이가 다르면 UI 업데이트 스킵
        if (DictionaryManager.Instance.GetCurrentSelectedCatGrade() != catGrade &&
            DictionaryManager.Instance.GetCurrentSelectedCatGrade() != -1)
        {
            UpdateFriendshipButtonStates(catGrade);
            return;
        }

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
        UpdateFriendshipButtonStates(catGrade);
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

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            if (button != null)
            {
                bool isAlreadyClaimed = friendshipInfo.isClaimed[i];

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
                Vector2 newOffsetMax = fullStar.GetComponent<RectTransform>().offsetMax;
                newOffsetMax.x = -210;
                newOffsetMax.y = 0;

                int claimedCount = friendshipInfo.isClaimed.Count(claimed => claimed);
                newOffsetMax.x = -210 + (claimedCount * 42);
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
        GameManager.Instance.Cash += GetRewardAmount(level);

        UpdateFriendshipUI(catGrade);
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

    }

    public string GetSaveData()
    {
        SaveData data = new SaveData();



        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);



        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            Debug.Log("구글 저장");
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}

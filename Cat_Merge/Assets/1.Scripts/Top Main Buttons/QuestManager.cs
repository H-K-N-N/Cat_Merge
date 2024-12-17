using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    private GameManager gameManager;                            // GameManager

    [Header("---[QuestManager]")]
    [SerializeField] private ScrollRect questScrollRect;        // 퀘스트의 스크롤뷰

    [SerializeField] private Button questButton;                // 퀘스트 버튼
    [SerializeField] private Image questButtonImage;            // 퀘스트 버튼 이미지

    [SerializeField] private GameObject questMenuPanel;         // 퀘스트 메뉴 Panel
    [SerializeField] private Button questBackButton;            // 퀘스트 뒤로가기 버튼
    private bool isQuestMenuOpen;                               // 퀘스트 메뉴 Panel의 활성화 상태

    [Header("---[Spawn Quest UI]")]
    [SerializeField] private Slider spawnQuestSlider;           // 고양이 스폰 Slider
    [SerializeField] private TextMeshProUGUI spawnCountText;    // "?/?" 텍스트
    [SerializeField] private Button rewardButton;               // 보상 버튼
    [SerializeField] private TextMeshProUGUI plusCashText;      // 보상 재화 개수 Text
    [SerializeField] private GameObject rewardDisabledBG;       // 보상 버튼 비활성화 BG
    private int spawnTargetCount = 1;                           // 목표 스폰 횟수
    private int increaseSpawnTargetCount = 2;                   // 목표 스폰 횟수 증가치
    private int questRewardCash = 5;                            // 퀘스트 보상 캐쉬 재화 개수

    // ======================================================================================================================

    // Start
    private void Start()
    {
        gameManager = GameManager.Instance;

        questMenuPanel.SetActive(false);

        questButton.onClick.AddListener(ToggleQuestMenuPanel);
        questBackButton.onClick.AddListener(CloseDictionaryMenuPanel);

        rewardButton.onClick.AddListener(ReceiveReward);
        rewardButton.interactable = false;
        plusCashText.text = $"+{questRewardCash}";

        ResetScrollPositions();
        UpdateSpawnQuestUI();
    }

    // 초기 스크롤 위치 초기화 함수
    private void ResetScrollPositions()
    {
        questScrollRect.verticalNormalizedPosition = 1f;
    }

    // 퀘스트 Panel 열고 닫는 함수
    private void ToggleQuestMenuPanel()
    {
        isQuestMenuOpen = !isQuestMenuOpen;
        questMenuPanel.SetActive(isQuestMenuOpen);
        UpdateButtonColor(questButtonImage, isQuestMenuOpen);
    }

    // 퀘스트 Panel 닫는 함수 (questBackButton)
    private void CloseDictionaryMenuPanel()
    {
        isQuestMenuOpen = false;
        questMenuPanel.SetActive(false);
        UpdateButtonColor(questButtonImage, false);
    }

    // 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? "#5f5f5f" : "#E2E2E2";
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================

    // 스폰 퀘스트 UI를 업데이트하는 함수
    private void UpdateSpawnQuestUI()
    {
        int currentCount = gameManager.FeedCount;

        // 목표가 10 이상이면 퀘스트 종료 상태 처리
        if (spawnTargetCount >= 10)
        {
            spawnQuestSlider.value = spawnQuestSlider.maxValue;
            spawnCountText.text = "Complete";
            rewardButton.interactable = false;
            rewardDisabledBG.SetActive(true);
            return;
        }

        // Slider 값 설정
        spawnQuestSlider.maxValue = spawnTargetCount;
        spawnQuestSlider.value = currentCount;

        // "?/?" 텍스트 업데이트
        spawnCountText.text = $"{currentCount}/{spawnTargetCount}";

        // 보상 버튼 활성화 조건 체크
        bool isComplete = currentCount >= spawnTargetCount;
        rewardButton.interactable = isComplete;
        rewardDisabledBG.SetActive(!isComplete);
    }

    // 보상 버튼 클릭 시 호출되는 함수
    private void ReceiveReward()
    {
        int currentCount = gameManager.FeedCount;

        // 목표가 10 이상인 경우 더 이상 보상 지급 불가
        if (spawnTargetCount >= 10)
        {
            return;
        }

        if (currentCount >= spawnTargetCount)
        {
            // 초과된 횟수 계산
            int excessCount = currentCount - spawnTargetCount;

            // 목표 스폰 횟수 증가 (퀘스트 난이도 상승)
            spawnTargetCount += increaseSpawnTargetCount;

            // 퀘스트 완료 처리
            gameManager.AddCash(questRewardCash);
            gameManager.ResetFeedCount(excessCount);
            gameManager.UpdateCashText();
            UpdateSpawnQuestUI();
        }
    }

    // 스폰 횟수를 계속 체크하고 UI 업데이트
    private void Update()
    {
        UpdateSpawnQuestUI();
    }

    // ======================================================================================================================


}

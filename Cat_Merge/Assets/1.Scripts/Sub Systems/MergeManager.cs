using UnityEngine;
using UnityEngine.UI;

// 고양이 머지 Script
public class MergeManager : MonoBehaviour
{
    // Singleton Instance
    public static MergeManager Instance { get; private set; }

    [Header("---[Merge On/Off System]")]
    [SerializeField] private Button openMergePanelButton;           // 머지 패널 열기 버튼
    [SerializeField] private GameObject mergePanel;                 // 머지 On/Off 패널
    [SerializeField] private Button closeMergePanelButton;          // 머지 패널 닫기 버튼
    [SerializeField] private Button mergeStateButton;               // 머지 상태 버튼
    private bool isMergeEnabled = true;                             // 머지 활성화 상태
    private bool previousMergeState = true;                         // 이전 상태 저장

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

        UpdateMergeButtonColor();

        openMergePanelButton.onClick.AddListener(OpenMergePanel);
        closeMergePanelButton.onClick.AddListener(CloseMergePanel);
        mergeStateButton.onClick.AddListener(ToggleMergeState);
    }

    // ======================================================================================================================

    // 머지 On/Off 패널 여는 함수
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // 머지 상태 전환 함수
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    // 전투 시작시 버튼 및 기능 비활성화시키는 함수
    public void StartBattleMergeState()
    {
        previousMergeState = isMergeEnabled;
        isMergeEnabled = false;
        openMergePanelButton.interactable = false;

        if (mergePanel.activeSelf == true)
        {
            mergePanel.SetActive(false);
        }
    }

    // 전투 종료시 버튼 및 기능 기존 상태로 되돌려놓는 함수
    public void EndBattleMergeState()
    {
        isMergeEnabled = previousMergeState;
        openMergePanelButton.interactable = true;
    }

    // 머지 버튼 색상 업데이트 함수
    private void UpdateMergeButtonColor()
    {
        if (openMergePanelButton != null)
        {
            string colorCode = !isMergeEnabled ? "#5f5f5f" : "#FFFFFF";
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                openMergePanelButton.GetComponent<Image>().color = color;
            }
        }
    }

    // 머지 패널 닫는 함수
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // 머지 상태 반환 함수
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }

    // ======================================================================================================================

    // 고양이 Merge 함수
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatGrade != cat2.CatGrade)
        {
            //Debug.LogWarning("등급이 다름");
            return null;
        }

        Cat nextCat = GetCatByGrade(cat1.CatGrade + 1);
        if (nextCat != null)
        {
            //Debug.Log($"합성 성공 : {nextCat.CatName}");
            DictionaryManager.Instance.UnlockCat(nextCat.CatGrade - 1);
            QuestManager.Instance.AddMergeCount();
            if(cat1.CatGrade == 1 && cat2.CatGrade == 1)
            {
                DictionaryManager.Instance.friendshipLockImg[0].SetActive(false);
                DictionaryManager.Instance.friendshipStarImg[0].SetActive(true);
            }
            return nextCat;
        }
        else
        {
            //Debug.LogWarning("더 높은 등급의 고양이가 없음");
            return null;
        }
    }

    // 고양이 ID 반환 함수
    public Cat GetCatByGrade(int grade)
    {
        GameManager gameManager = GameManager.Instance;

        foreach (Cat cat in gameManager.AllCatData)
        {
            if (cat.CatId == grade)
                return cat;
        }
        return null;
    }


}

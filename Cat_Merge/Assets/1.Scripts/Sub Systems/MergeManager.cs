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
    private bool isMergeEnabled = true;

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
            QuestManager.Instance.AddCombineCount();
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

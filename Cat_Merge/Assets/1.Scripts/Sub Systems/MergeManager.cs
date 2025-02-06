using UnityEngine;
using UnityEngine.UI;

// ����� ���� Script
public class MergeManager : MonoBehaviour
{
    // Singleton Instance
    public static MergeManager Instance { get; private set; }

    [Header("---[Merge On/Off System]")]
    [SerializeField] private Button openMergePanelButton;           // ���� �г� ���� ��ư
    [SerializeField] private GameObject mergePanel;                 // ���� On/Off �г�
    [SerializeField] private Button closeMergePanelButton;          // ���� �г� �ݱ� ��ư
    [SerializeField] private Button mergeStateButton;               // ���� ���� ��ư
    private bool isMergeEnabled = true;                             // ���� Ȱ��ȭ ����
    private bool previousMergeState = true;                         // ���� ���� ����

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

    // ���� On/Off �г� ���� �Լ�
    private void OpenMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(true);
        }
    }

    // ���� ���� ��ȯ �Լ�
    private void ToggleMergeState()
    {
        isMergeEnabled = !isMergeEnabled;
        UpdateMergeButtonColor();
        CloseMergePanel();
    }

    // ���� ���۽� ��ư �� ��� ��Ȱ��ȭ��Ű�� �Լ�
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

    // ���� ����� ��ư �� ��� ���� ���·� �ǵ������� �Լ�
    public void EndBattleMergeState()
    {
        isMergeEnabled = previousMergeState;
        openMergePanelButton.interactable = true;
    }

    // ���� ��ư ���� ������Ʈ �Լ�
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

    // ���� �г� �ݴ� �Լ�
    public void CloseMergePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
    }

    // ���� ���� ��ȯ �Լ�
    public bool IsMergeEnabled()
    {
        return isMergeEnabled;
    }

    // ======================================================================================================================

    // ����� Merge �Լ�
    public Cat MergeCats(Cat cat1, Cat cat2)
    {
        if (cat1.CatGrade != cat2.CatGrade)
        {
            //Debug.LogWarning("����� �ٸ�");
            return null;
        }

        Cat nextCat = GetCatByGrade(cat1.CatGrade + 1);
        if (nextCat != null)
        {
            //Debug.Log($"�ռ� ���� : {nextCat.CatName}");
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
            //Debug.LogWarning("�� ���� ����� ����̰� ����");
            return null;
        }
    }

    // ����� ID ��ȯ �Լ�
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

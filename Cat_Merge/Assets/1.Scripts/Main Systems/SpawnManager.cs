using UnityEngine;

// ����� ���� Script
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject catPrefab;      // ����� UI ������
    [SerializeField] private Transform catUIParent;     // ����̸� ��ġ�� �θ� Transform (UI Panel ��)
    private RectTransform panelRectTransform;           // Panel�� ũ�� ���� (��ġ�� ����)
    private GameManager gameManager;                    // GameManager

    private void Start()
    {
        gameManager = GameManager.Instance;
        panelRectTransform = catUIParent.GetComponent<RectTransform>();
        if (panelRectTransform == null)
        {
            Debug.LogError("catUIParent�� RectTransform�� ������ ���� �ʽ��ϴ�.");
            return;
        }
    }

    // ����� ���� ��ư Ŭ��
    public void OnClickedSpawn()
    {
        if (gameManager.CanSpawnCat())
        {
            // ���׷��̵� �ý��� Ȯ�强�� ���� ������ ����� ���� �ڵ�
            Cat catData = GetCatDataForSpawn();
            LoadAndDisplayCats(catData);
            QuestManager.Instance.AddFeedCount();
            gameManager.AddCatCount();
        }
        else
        {
            Debug.Log("����� �ִ� ���� ������ �����߽��ϴ�!");
        }
    }

    // �ڵ������� ����� ���� �Լ�
    public void SpawnCat()
    {
        if (!gameManager.CanSpawnCat()) return;

        // ���׷��̵� �ý��� Ȯ�强�� ���� ������ ����� ���� �ڵ�
        Cat catData = GetCatDataForSpawn();
        GameObject newCat = LoadAndDisplayCats(catData);

        DragAndDropManager catDragAndDrop = newCat.GetComponent<DragAndDropManager>();
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = gameManager.AllCatData[0];
            catDragAndDrop.UpdateCatUI();
        }

        gameManager.AddCatCount();
    }

    // �������� �̿�� ��޿� ���� ���� �� ���� (12/26 ���� �ۼ�)
    public void SpawnGradeCat(int grade)
    {
        if (!gameManager.CanSpawnCat()) return;
        Cat catData = gameManager.AllCatData[grade];

        GameObject newCat = LoadAndDisplayCats(catData);

        DragAndDropManager catDragAndDrop = newCat.GetComponent<DragAndDropManager>();
        if (catDragAndDrop != null)
        {
            catDragAndDrop.catData = gameManager.AllCatData[grade];
            catDragAndDrop.UpdateCatUI();
        }

        gameManager.AddCatCount();

    }

    // ����� ���� �����͸� �����ϴ� �Լ� (���׷��̵� �ý��� ���)
    private Cat GetCatDataForSpawn()
    {
        // ����: �������� ����� ����� �����ϰų� ���� ���׷��̵带 ����Ͽ� ����
        // return gameManager.GetRandomCatForSpawn();  // �� �κ��� ���߿� ���׷��̵� �ý��ۿ� �°� ����
        int catGrade = 0;
        DictionaryManager.Instance.UnlockCat(catGrade);
        return gameManager.AllCatData[catGrade];
    }

    // Panel�� ���� ��ġ�� ����� ��ġ�ϴ� �Լ�
    private GameObject LoadAndDisplayCats(Cat catData)
    {
        GameObject catUIObject = Instantiate(catPrefab, catUIParent);

        // CatData ����
        CatData catUIData = catUIObject.GetComponent<CatData>();
        if (catUIData != null)
        {
            catUIData.SetCatData(catData);

            // �ڵ� �̵� ���¸� ����ȭ
            if (gameManager != null)
            {
                catUIData.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
            }
        }

        // ���� ��ġ ����
        Vector2 randomPos = GetRandomPosition(panelRectTransform);
        RectTransform catRectTransform = catUIObject.GetComponent<RectTransform>();
        if (catRectTransform != null)
        {
            catRectTransform.anchoredPosition = randomPos;
        }

        return catUIObject;
    }

    // Panel�� ���� ��ġ ����ϴ� �Լ�
    Vector3 GetRandomPosition(RectTransform panelRectTransform)
    {
        float panelWidth = panelRectTransform.rect.width;
        float panelHeight = panelRectTransform.rect.height;

        float randomX = Random.Range(-panelWidth / 2, panelWidth / 2);
        float randomY = Random.Range(-panelHeight / 2, panelHeight / 2);

        Vector3 respawnPos = new Vector3(randomX, randomY, 0f);
        return respawnPos;
    }


}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemMenuManager : MonoBehaviour
{ 
    [Header("---[ItemMenu]")]
    [SerializeField] private Button bottomItemButton;               // ������ ��ư
    [SerializeField] private Image bottomItemButtonImg;             // ������ ��ư �̹���

    [SerializeField] private GameObject itemMenuPanel;              // ������ �޴� �ǳ�
    private bool isOnToggleItemMenu;                                // ������ �޴� �ǳ� ���
    [SerializeField] private Button itemBackButton;                 // ������ �ڷΰ��� ��ư

    // ������ �޴� ��ư �׷�(�迭�� ������)
    enum EitemMenuButton
    {
        itemMenuButton = 0,
        colleagueMenuButton,
        backgroundMenuButton,
        goldenSomethingMenuButton,
        end,
    }
    // ������ �޴� �ǳڵ� �׷�(�迭�� ������)
    enum EitemMenues
    {
        itemMenues = 0,
        colleagueMenues,
        backgroundMenues,
        goldenMenues,
        end,
    }

    // ������ �޴� �ȿ� ������, ����, ���, Ȳ�ݱ����� ��ư��
    [Header("---[ItemMenues]")]
    [SerializeField] private Button[] itemMenuButtons;                  // itemPanel������ ������ �޴� ��ư��
    [SerializeField] private GameObject[] itemMenues;                   // itemPanel������ �޴� ��ũ��â �ǳ�
    private bool[] isItemMenuButtonsOn = new bool[4];                   // ��ư ���� �����ϱ� ���� boolŸ�� �迭

    // ������ �޴� �ȿ� ������ ���
    [Header("---[ItemMenuList]")]
    [SerializeField] private Button catMaximumIncreaseButton;           // ����� �ִ�ġ ���� ��ư
    [SerializeField] private GameObject disabledBg;                     // ��ư Ŭ�� ���� ���� ���
    private int catMaximumIncreaseFee = 10;                             // ����� �ִ�ġ ���� ���
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseText;    // ����� �ִ�ġ ���� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI catMaximumIncreaseLvText;  // ����� �ִ�ġ ���� ���� �ؽ�Ʈ
    private int catMaximumIncreaseLv = 1;                               // ����� �ִ�ġ ���� ����

    // ======================================================================================================================

    [Header("---[BuyCat]")]
    [SerializeField] private Button bottomBuyCatButton;                                 // ����� ���� ��ư
    [SerializeField] private Image bottomBuyCatButtonImg;                               // ����� ���� ��ư �̹���
    [SerializeField] private GameObject[] buyCatCoinDisabledBg;                           // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject[] buyCatCashDisabledBg;                           // ��ư Ŭ�� ���� ���� ���
    [SerializeField] private GameObject buyCatMenuPanel;                                // ������ �޴� �ǳ�
    private bool isOnToggleBuyCat;                                                      // ������ �޴� �ǳ� ���
    [SerializeField] private Button buyCatBackButton;                                   // ����� ���� �޴� �ڷΰ��� ��ư

    [SerializeField] private Button[] buyCatCoinButtons;                                // ����� ��ȭ�� ���� ��ư
    [SerializeField] private Button[] buyCatCashButtons;                                // ũ����Ż ��ȭ�� ���� ��ư

    [SerializeField] private TextMeshProUGUI[] buyCatCountExplainTexts;                 // ����� ���� Ƚ�� ����â
    [SerializeField] private TextMeshProUGUI[] buyCatCoinFeeTexts;                      // ����� ���� ��� (���)
    [SerializeField] private TextMeshProUGUI[] buyCatCashFeeTexts;                      // ����� ���� ��� (ũ����Ż)

    private int[] buyCatCoinCounts;// = new int[3];   // ����� ���� Ƚ��(����)
    private int[] buyCatCashCounts;// = new int[3];   // ����� ���� Ƚ��(ũ����Ż)
    private int[] buyCatCoinFee;// = new int[3];      // ����� ���� ��� (����)
    private int[] buyCatCashFee;// = new int[3];      // ����� ���� ��� (ũ����Ż)

    // ======================================================================================================================

    private void Awake()
    {

        itemMenues[(int)EitemMenues.itemMenues].SetActive(true);
        if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
        {
            itemMenuButtons[(int)EitemMenuButton.itemMenuButton].GetComponent<Image>().color = parsedColor;
        }

        itemMenuButtons[(int)EitemMenuButton.itemMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.itemMenues]));
        itemMenuButtons[(int)EitemMenuButton.colleagueMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.colleagueMenues]));
        itemMenuButtons[(int)EitemMenuButton.backgroundMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.backgroundMenues]));
        itemMenuButtons[(int)EitemMenuButton.goldenSomethingMenuButton].onClick.AddListener(() => OnButtonClicked(itemMenues[(int)EitemMenues.goldenMenues]));

        for (int i = 1; i < 4; i++)
        {
            isItemMenuButtonsOn[i] = false;
        }
        UpdateIncreaseMaximumFeeText();
        UpdateIncreaseMaximumLvText();

        bottomItemButton.onClick.AddListener(OpenCloseBottomItemMenuPanel);
        catMaximumIncreaseButton.onClick.AddListener(IncreaseCatMaximum);
        itemBackButton.onClick.AddListener(CloseBottomItemMenuPanel);

        // ======================================================================================================================
        buyCatCoinCounts = new int[GameManager.Instance.AllCatData.Length];   // ����� ���� Ƚ��(����)
        buyCatCashCounts = new int[GameManager.Instance.AllCatData.Length];   // ����� ���� Ƚ��(ũ����Ż)
        buyCatCoinFee = new int[GameManager.Instance.AllCatData.Length];      // ����� ���� ��� (����)
        buyCatCashFee = new int[GameManager.Instance.AllCatData.Length];      // ����� ���� ��� (ũ����Ż)
        // ����� ���� �޴� ����
        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            buyCatCoinCounts[i] = 0;
            buyCatCashCounts[i] = 0;
            buyCatCoinFee[i] = i * 5 + 10;
            buyCatCashFee[i] = 5;

            buyCatCountExplainTexts[i].text = $"BuyCount :{buyCatCoinCounts[i]}cnt + {buyCatCashCounts[i]}cnt";
            buyCatCoinFeeTexts[i].text = $"{buyCatCoinFee[i]}";
            buyCatCashFeeTexts[i].text = $"{buyCatCashFee[i]}";
        }
        bottomBuyCatButton.onClick.AddListener(OpenCloseBottomBuyCatMenuPanel);
        buyCatBackButton.onClick.AddListener(CloseBottomBuyCatMenuPanel);

        for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
        {
            
            if (i < buyCatCoinButtons.Length && i < buyCatCashButtons.Length) // �굵 ����
            {
                int index = i; // �갡 �־�� �� �ٵ� ������ ��
                buyCatCoinButtons[index].onClick.AddListener(() => BuyCatCoin(buyCatCoinButtons[index]));
                buyCatCashButtons[index].onClick.AddListener(() => BuyCatCash(buyCatCashButtons[index]));
            }
            else
            {
                Debug.LogWarning($"Index {i} is out of bounds for buyCatCoinButtons array.");
            }
        }
    }

    // ======================================================================================================================

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomItemMenuPanel()
    {
        //Debug.Log("Item Toggle");
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = !isOnToggleItemMenu;
            if (isOnToggleItemMenu)
            {
                //Debug.Log("Item Toggle On");
                itemMenuPanel.SetActive(true);

                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
            else
            {
                //Debug.Log("Item Toggle Off");
                itemMenuPanel.SetActive(false);

                ChangeSelectedButtonColor(isOnToggleItemMenu);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomItemMenuPanel()
    {
        if (itemMenuPanel != null)
        {
            isOnToggleItemMenu = false;
            itemMenuPanel.SetActive(false);

            ChangeSelectedButtonColor(isOnToggleItemMenu);
        }
    }

    // ���� �޴� ��ư ���� ����
    private void ChangeSelectedButtonColor(bool isOnToggle)
    {
        if (isOnToggle)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColor))
            {
                if (itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if (buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
            }
        }
        else
        {
            if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColor))
            {
                if (!itemMenuPanel.activeSelf)
                {
                    bottomItemButtonImg.color = parsedColor;
                }
                if (!buyCatMenuPanel.activeSelf)
                {
                    bottomBuyCatButtonImg.color = parsedColor;
                }
            }
        }
    }

    // ������ �޴��� ������, ����, ���, Ȳ�ݱ��� ��ư ���� ����
    private void ChangeSelectedButtonColor(GameObject menues)
    {
        if (menues.gameObject.activeSelf)
        {
            if (ColorUtility.TryParseHtmlString("#5f5f5f", out Color parsedColorT))
            {
                for (int i = 0; i < (int)EitemMenues.end; i++)
                {
                    if (isItemMenuButtonsOn[i])
                    {
                        itemMenuButtons[i].GetComponent<Image>().color = parsedColorT;
                    }
                    else
                    {
                        if (ColorUtility.TryParseHtmlString("#FFFFFF", out Color parsedColorF))
                        {
                            itemMenuButtons[i].GetComponent<Image>().color = parsedColorF;
                        }
                    }
                }
            }
        }
    }

    // ����� �ִ�ġ ����
    private void IncreaseCatMaximum()
    {
        //Debug.Log("����� �ִ�ġ ���� ��ư Ŭ��");
        if (catMaximumIncreaseButton != null)
        {
            GameManager.Instance.Coin -= catMaximumIncreaseFee;
            GameManager.Instance.MaxCats++;
            catMaximumIncreaseFee *= 2;
            catMaximumIncreaseLv++;
            UpdateIncreaseMaximumFeeText();
            UpdateIncreaseMaximumLvText();

        }
    }

    // ����� �� �ؽ�Ʈ UI ������Ʈ�ϴ� �Լ�
    private void UpdateIncreaseMaximumFeeText()
    {
        if (catMaximumIncreaseText != null)
        {
            if (GameManager.Instance.Coin < catMaximumIncreaseFee)
            {
                catMaximumIncreaseButton.interactable = false;
                disabledBg.SetActive(true);
            }
            else
            {

                catMaximumIncreaseButton.interactable = true;
                disabledBg.SetActive(false);
            }
            catMaximumIncreaseText.text = $"{catMaximumIncreaseFee}";
        }
    }

    // ����� �ִ�ġ ���� ���� UI ������Ʈ �ϴ� �Լ�
    private void UpdateIncreaseMaximumLvText()
    {
        catMaximumIncreaseLvText.text = $"Lv.{catMaximumIncreaseLv}";
    }

    // ��ư Ŭ�� �̺�Ʈ
    private void OnButtonClicked(GameObject activePanel)
    {
        // ��� UI �г� ��Ȱ��ȭ
        for (int i = 0; i < (int)EitemMenues.end; i++)
        {
            itemMenues[i].SetActive(false);
            isItemMenuButtonsOn[i] = false;
        }

        // Ŭ���� ��ư�� UI�� Ȱ��ȭ
        activePanel.SetActive(true);

        for (int i = 0; i < (int)EitemMenues.end; i++)
        {
            if (itemMenues[i].activeSelf)
            {
                isItemMenuButtonsOn[i] = true;
            }
        }
        ChangeSelectedButtonColor(activePanel);
    }

    // ======================================================================================================================

    // ������ �޴� �ǳ� ���� �Լ�
    private void OpenCloseBottomBuyCatMenuPanel()
    {
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = !isOnToggleBuyCat;
            if (isOnToggleBuyCat)
            {
                Debug.Log("Buy Cat Toggle On");
                buyCatMenuPanel.SetActive(true);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
            else
            {
                Debug.Log("Buy Cat Toggle Off");
                buyCatMenuPanel.SetActive(false);
                ChangeSelectedButtonColor(isOnToggleBuyCat);
            }
        }
    }

    // ������ �޴� �ǳ� �ݴ� �Լ�(�ڷΰ��� ��ư)
    private void CloseBottomBuyCatMenuPanel()
    {
        if (buyCatMenuPanel != null)
        {
            isOnToggleBuyCat = false;
            buyCatMenuPanel.SetActive(false);
            ChangeSelectedButtonColor(isOnToggleBuyCat);
        }
    }

    // ����� ���� ��ư �Լ�(�������� ����)
    private void BuyCatCoin(Button b)
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            if(b == buyCatCoinButtons[0])
            {
                Debug.Log("ù��° ����� ���� ��ư Ŭ��(��������)");

                GameManager.Instance.Coin -= buyCatCoinFee[0];
                buyCatCoinCounts[0]++;
                buyCatCoinFee[0] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(0);

                buyCatCountExplainTexts[0].text = $"BuyCount :{buyCatCoinCounts[0]}cnt + {buyCatCashCounts[0]}cnt";
                buyCatCoinFeeTexts[0].text = $"{buyCatCoinFee[0]}";
            }
            else if(b == buyCatCoinButtons[1])
            {              
                Debug.Log("�ι��� ����� ���� ��ư Ŭ��(��������)");

                GameManager.Instance.Coin -= buyCatCoinFee[1];
                buyCatCoinCounts[1]++;
                buyCatCoinFee[1] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(1);

                buyCatCountExplainTexts[1].text = $"BuyCount :{buyCatCoinCounts[1]}cnt + {buyCatCashCounts[1]}cnt";
                buyCatCoinFeeTexts[1].text = $"{buyCatCoinFee[1]}";
            }
            else if (b == buyCatCoinButtons[2])
            {
                Debug.Log("����° ����� ���� ��ư Ŭ��(��������)");

                GameManager.Instance.Coin -= buyCatCoinFee[2];
                buyCatCoinCounts[2]++;
                buyCatCoinFee[2] *= 2;

                QuestManager.Instance.AddPurchaseCatsCount();

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.SpawnGradeCat(2);

                buyCatCountExplainTexts[2].text = $"BuyCount :{buyCatCoinCounts[2]}cnt + {buyCatCashCounts[2]}cnt";
                buyCatCoinFeeTexts[2].text = $"{buyCatCoinFee[2]}";
            }

        }
    }

    // ����� ���� ��ư �Լ�(ũ����Ż�� ����)
    private void BuyCatCash(Button b)
    {
        // ���� ����� ���� ����� �ִ� �� �̸��� ��
        if (GameManager.Instance.CanSpawnCat())
        {
            if(b == buyCatCashButtons[0])
            {
                Debug.Log("ù��° ����� ���� ��ư Ŭ��(ĳ����)");
 
                GameManager.Instance.Cash -= buyCatCashFee[0];
                buyCatCashCounts[0]++;

                QuestManager.Instance.AddPurchaseCatsCount();

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.OnClickedSpawn();

                buyCatCountExplainTexts[0].text = $"BuyCount :{buyCatCoinCounts[0]}cnt + {buyCatCashCounts[0]}cnt";
                buyCatCashFeeTexts[0].text = $"{buyCatCashFee[0]}";

                //UpdateBuyCatUI();
            }
            else if(b == buyCatCashButtons[1])
            {
                Debug.Log("�ι�° ����� ���� ��ư Ŭ��(ĳ����)");

                GameManager.Instance.Cash -= buyCatCashFee[1];
                buyCatCashCounts[1]++;

                QuestManager.Instance.AddPurchaseCatsCount();

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.OnClickedSpawn();

                buyCatCountExplainTexts[1].text = $"BuyCount :{buyCatCoinCounts[1]}cnt + {buyCatCashCounts[1]}cnt";
                buyCatCashFeeTexts[1].text = $"{buyCatCashFee[1]}";

                //UpdateBuyCatUI();
            }
            else if (b == buyCatCashButtons[2])
            {
                Debug.Log("����° ����� ���� ��ư Ŭ��(ĳ����)");

                GameManager.Instance.Cash -= buyCatCashFee[2];
                buyCatCashCounts[2]++;

                QuestManager.Instance.AddPurchaseCatsCount();

                SpawnManager catSpawn = GetComponent<SpawnManager>();
                catSpawn.OnClickedSpawn();

                buyCatCountExplainTexts[2].text = $"BuyCount :{buyCatCoinCounts[2]}cnt + {buyCatCashCounts[2]}cnt";
                buyCatCashFeeTexts[2].text = $"{buyCatCashFee[2]}";


               // UpdateBuyCatUI();
            }

        }
    }

    // ����� ���� ���� UI ������Ʈ �Լ�
    private void UpdateBuyCatUI()
    {
        if (GameManager.Instance.CanSpawnCat())
        {
            for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
            {
                if (GameManager.Instance.Coin < buyCatCoinFee[i])
                {
                    buyCatCoinButtons[i].interactable = false;
                    buyCatCoinDisabledBg[i].SetActive(true);

                }
                else
                {
                    buyCatCoinButtons[i].interactable = true;
                    buyCatCoinDisabledBg[i].SetActive(false);
                }
            }

            for (int i = 0; i < GameManager.Instance.AllCatData.Length; i++)
            {
                if (GameManager.Instance.Cash < buyCatCashFee[i])
                {
                    buyCatCoinButtons[i].interactable = false;
                    buyCatCashDisabledBg[i].SetActive(true);
                }
                else
                {
                    buyCatCoinButtons[i].interactable = true;
                    buyCatCashDisabledBg[i].SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        UpdateBuyCatUI();

    }
}

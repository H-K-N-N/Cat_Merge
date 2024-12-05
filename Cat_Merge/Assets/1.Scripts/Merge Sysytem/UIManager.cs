using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AutoMerge autoMerge;       // AutoMerge ÂüÁ¶

    private void Start()
    {
        if (autoMerge == null)
        {
            autoMerge = FindObjectOfType<AutoMerge>();
        }
    }

    public void OnAutoMergeButtonClicked()
    {
        if (autoMerge != null)
        {
            autoMerge.StartAutoMerge();
        }
    }


}

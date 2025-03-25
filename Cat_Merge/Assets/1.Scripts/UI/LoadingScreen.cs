using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    private Canvas loadingScreenCanvas;
    private const string loadingScreenName = "LoadingScreen";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeLoadingScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLoadingScreen()
    {
        loadingScreenCanvas = GetComponent<Canvas>();
        gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLoadingScreenCamera();
    }

    public void UpdateLoadingScreenCamera()
    {
        if (loadingScreenCanvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                loadingScreenCanvas.worldCamera = mainCamera;
                loadingScreenCanvas.planeDistance = 1f;
            }
        }
    }

    public void Show(bool show)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(show);

            if (!GoogleManager.Instance.isDeletingData)
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

// 옵션 스크립트
public class OptionManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static OptionManager Instance { get; private set; }

    // ======================================================================================================================
    // [옵션 메뉴 UI 요소들]

    [Header("---[OptionManager]")]
    [SerializeField] private Button optionButton;               // 옵션 버튼
    [SerializeField] private Image optionButtonImage;           // 옵션 버튼 이미지
    [SerializeField] private GameObject optionMenuPanel;        // 옵션 메뉴 Panel
    [SerializeField] private Button optionBackButton;           // 옵션 뒤로가기 버튼
    private ActivePanelManager activePanelManager;              // ActivePanelManager

    [SerializeField] private GameObject[] mainOptionMenus;      // 메인 옵션 메뉴 Panels
    [SerializeField] private Button[] subOptionMenuButtons;     // 서브 옵션 메뉴 버튼 배열

    // ======================================================================================================================
    // [서브 메뉴 UI 색상 설정]

    [Header("---[Sub Menu UI Color]")]
    private const string activeColorCode = "#FFCC74";           // 활성화상태 Color
    private const string inactiveColorCode = "#FFFFFF";         // 비활성화상태 Color

    // ======================================================================================================================
    // [토글 버튼 관련 설정]

    private const float onX = 65f, offX = -65f;                 // 핸들 버튼 x좌표
    private const float moveDuration = 0.2f;                    // 토글 애니메이션 지속 시간

    // ======================================================================================================================
    // [Sound]

    // 사운드 컨트롤 내부 클래스
    public class SoundController
    {
        private readonly AudioSource audioSource;
        private readonly Slider volumeSlider;

        public SoundController(GameObject parent, Slider slider, bool loop, AudioClip clip)
        {
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.loop = loop;
            audioSource.clip = clip;
            volumeSlider = slider;

            if (volumeSlider != null)
            {
                volumeSlider.value = 0.1f;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
        }

        // 볼륨 설정 함수
        public void SetVolume(float volume)
        {
            bool isBgm = (audioSource == Instance.bgmSettings.controller.audioSource);
            audioSource.volume = (isBgm && !Instance.bgmSettings.isOn) || (!isBgm && !Instance.sfxSettings.isOn) ? 0f : volume;
            Instance.SetSoundToggleImage(isBgm);
        }

        public void Play() => audioSource.Play();
        public void PlayOneShot()
        {
            if (audioSource != null && audioSource.clip != null && Instance.sfxSettings.isOn)
            {
                audioSource.PlayOneShot(audioSource.clip, Instance.sfxSettings.slider.value);
            }
        }
        public void Stop() => audioSource.Stop();
        public AudioSource GetAudioSource() => audioSource;
    }

    [System.Serializable]
    private class SoundSettings
    {
        public Slider slider;                                   // 볼륨 조절 슬라이더
        public Button toggleButton;                             // 토글 버튼
        public RectTransform handle;                            // 토글 핸들
        public Image onOffImage;                                // On/Off 이미지
        public Image toggleButtonImage;                         // 토글 버튼 이미지
        public SoundController controller;                      // 사운드 컨트롤러
        public Coroutine toggleCoroutine;                       // 토글 애니메이션 코루틴
        public bool isOn = true;                                // 토글 상태
    }

    [Header("---[BGM]")]
    [SerializeField] private SoundSettings bgmSettings = new SoundSettings();

    [Header("---[SFX]")]
    [SerializeField] private SoundSettings sfxSettings = new SoundSettings();

    [Header("---[Common]")]
    private Sprite bgmOnImage;                                  // BGM On 이미지
    private Sprite bgmOffImage;                                 // BGM Off 이미지
    private Sprite sfxOnImage;                                  // SFX On 이미지
    private Sprite sfxOffImage;                                 // SFX Off 이미지

    private const string BGM_ON_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_BGM_v1.9";
    private const string BGM_OFF_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_BGM_v2.9";
    private const string SFX_ON_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_SFX_v1.9";
    private const string SFX_OFF_IMAGE_PATH = "Sprites/UI/I_UI_Option/I_UI_SFX_v2.9";

    // ======================================================================================================================
    // [Display]

    [System.Serializable]
    private class ToggleSettings
    {
        public Button toggleButton;                             // 토글 버튼
        public RectTransform handle;                            // 토글 핸들
        public Image toggleButtonImage;                         // 토글 버튼 이미지
        public Coroutine toggleCoroutine;                       // 토글 애니메이션 코루틴
        public bool isOn = true;                                // 토글 상태
    }

    [Header("---[Effect]")]
    [SerializeField] private ToggleSettings effectSettings = new ToggleSettings();

    [Header("---[Screen Shaking]")]
    [SerializeField] private ToggleSettings shakingSettings = new ToggleSettings();

    [Header("---[Saving Mode]")]
    [SerializeField] private ToggleSettings savingSettings = new ToggleSettings();

    // ======================================================================================================================
    // [System]

    [Header("---[System]")]
    [SerializeField] private Transform slotPanel;                   // 슬롯 버튼들의 부모 패널
    [SerializeField] private Button[] slotButtons;                  // 슬롯 버튼 배열
    [SerializeField] private Button quitButton;                     // 나가기 버튼
    [SerializeField] private GameObject informationPanel;           // 정보 패널들의 부모 패널
    [SerializeField] private GameObject[] informationPanels;        // 정보 패널 배열
    [SerializeField] private Button informationPanelBackButton;     // 정보 패널 뒤로가기 버튼

    private Vector2[] originalButtonPositions;                      // 버튼들의 원래 위치
    private CanvasGroup slotPanelGroup;                             // 슬롯 패널의 CanvasGroup
    private const float systemAnimDuration = 0.5f;                  // 시스템 애니메이션 지속 시간
    private int currentActivePanel = -1;                            // 현재 활성화된 패널 인덱스

    // ======================================================================================================================
    // [옵션 메뉴 타입 정의]

    // Enum으로 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum OptionMenuType
    {
        Sound,                                  // 사운드 메뉴
        Display,                                // 화면 메뉴
        System,                                 // 시스템 메뉴
        End                                     // Enum의 끝
    }
    private OptionMenuType activeMenuType;      // 현재 활성화된 메뉴 타입


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
        optionMenuPanel.SetActive(false);

        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("OptionMenu", optionMenuPanel, optionButtonImage);

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            InitializeOptionManager();
        }
    }

    #endregion


    #region Initialize

    // 모든 OptionManager 시작 함수들 모음
    private void InitializeOptionManager()
    {
        InitializeOptionButton();
        InitializeSubMenuButtons();
        InitializeSoundControllers();
        InitializeDisplayControllers();
        InitializeSystemSettings();
    }

    // OptionButton 초기화 함수
    private void InitializeOptionButton()
    {
        Action handleOptionMenu = () =>
        {
            if (activeMenuType == OptionMenuType.System && currentActivePanel != -1)
            {
                ResetSystemMenu();
            }
        };

        optionButton.onClick.AddListener(() =>
        {
            handleOptionMenu();
            activePanelManager.TogglePanel("OptionMenu");
        });

        optionBackButton.onClick.AddListener(() =>
        {
            handleOptionMenu();
            activePanelManager.ClosePanel("OptionMenu");
        });
    }

    #endregion


    #region 서브 메뉴

    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)OptionMenuType.End; i++)
        {
            int index = i;
            subOptionMenuButtons[index].onClick.AddListener(() =>
            {
                if (activeMenuType == OptionMenuType.System && currentActivePanel != -1)
                {
                    ResetSystemMenu();
                }
                ActivateMenu((OptionMenuType)index);
            });
        }

        ActivateMenu(OptionMenuType.Sound);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(OptionMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainOptionMenus.Length; i++)
        {
            mainOptionMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subOptionMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subOptionMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // 서브 메뉴 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        if (ColorUtility.TryParseHtmlString(isActive ? activeColorCode : inactiveColorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    #endregion


    #region 사운드 설정

    // Sound 초기화 함수
    private void InitializeSoundControllers()
    {
        // Audio 초기화
        AudioClip bgmClip = Resources.Load<AudioClip>("Audios/BGM_Sound");
        AudioClip sfxClip = Resources.Load<AudioClip>("Audios/SFX_Sound");
        bgmSettings.controller = new SoundController(gameObject, bgmSettings.slider, true, bgmClip);
        sfxSettings.controller = new SoundController(gameObject, sfxSettings.slider, false, sfxClip);

        bgmSettings.controller.SetVolume(bgmSettings.slider.value);
        sfxSettings.controller.SetVolume(sfxSettings.slider.value);

        bgmSettings.controller.Play();

        // Image 초기화
        bgmOnImage = Resources.Load<Sprite>(BGM_ON_IMAGE_PATH);
        bgmOffImage = Resources.Load<Sprite>(BGM_OFF_IMAGE_PATH);
        sfxOnImage = Resources.Load<Sprite>(SFX_ON_IMAGE_PATH);
        sfxOffImage = Resources.Load<Sprite>(SFX_OFF_IMAGE_PATH);
        bgmSettings.onOffImage.sprite = bgmOnImage;
        sfxSettings.onOffImage.sprite = sfxOnImage;

        // 토글 버튼 이미지 초기화
        bgmSettings.toggleButtonImage = bgmSettings.toggleButton.GetComponent<Image>();
        sfxSettings.toggleButtonImage = sfxSettings.toggleButton.GetComponent<Image>();
        UpdateToggleButtonImage(bgmSettings.toggleButtonImage, bgmSettings.isOn);
        UpdateToggleButtonImage(sfxSettings.toggleButtonImage, sfxSettings.isOn);

        // 토글 버튼 이벤트 설정
        bgmSettings.toggleButton.onClick.AddListener(() => ToggleSound(true));
        sfxSettings.toggleButton.onClick.AddListener(() => ToggleSound(false));

        UpdateToggleUI(bgmSettings.isOn, true, true);
        UpdateToggleUI(sfxSettings.isOn, false, true);

        // 볼륨 슬라이더 값 변경 시 저장 이벤트 추가
        bgmSettings.slider.onValueChanged.AddListener(_ => {
            SetSoundToggleImage(true);
            GoogleSave();
        });
        sfxSettings.slider.onValueChanged.AddListener(_ => {
            SetSoundToggleImage(false);
            GoogleSave();
        });
    }

    // 사운드 On/Off 토글 함수
    public void ToggleSound(bool isBgm)
    {
        SoundSettings settings = isBgm ? bgmSettings : sfxSettings;
        settings.isOn = !settings.isOn;

        // SFX 토글이고 Off에서 On으로 변경될 때만 소리 재생
        if (!isBgm && settings.isOn)
        {
            if (sfxSettings.controller != null && sfxSettings.controller.GetAudioSource() != null)
            {
                AudioSource audioSource = sfxSettings.controller.GetAudioSource();
                audioSource.volume = sfxSettings.slider.value * 0.5f;
                audioSource.PlayOneShot(audioSource.clip);
            }
        }

        SetSoundToggleImage(isBgm);
        UpdateToggleUI(settings.isOn, isBgm);
        UpdateToggleButtonImage(settings.toggleButtonImage, settings.isOn);

        GoogleSave();
    }

    // 토글 버튼 이미지 업데이트 함수
    private void UpdateToggleButtonImage(Image buttonImage, bool isOn)
    {
        string imagePath = isOn ? "Sprites/UI/I_UI_Option/I_UI_option_on_Frame.9" : "Sprites/UI/I_UI_Option/I_UI_option_off_Frame.9";
        buttonImage.sprite = Resources.Load<Sprite>(imagePath);
    }

    // 사운드 On/Off 이미지 변경 함수
    private void SetSoundToggleImage(bool isBgm)
    {
        SoundSettings settings = isBgm ? bgmSettings : sfxSettings;
        if (isBgm)
        {
            settings.onOffImage.sprite = (!settings.isOn || settings.slider.value == 0) ? bgmOffImage : bgmOnImage;
        }
        else
        {
            settings.onOffImage.sprite = (!settings.isOn || settings.slider.value == 0) ? sfxOffImage : sfxOnImage;
        }
    }

    // 사운드 토글 UI 업데이트 함수
    private void UpdateToggleUI(bool state, bool isBgm, bool instant = false)
    {
        SoundSettings settings = isBgm ? bgmSettings : sfxSettings;
        float targetX = state ? onX : offX;
        float targetVolume = state ? settings.slider.value : 0.0f;

        if (instant)
        {
            settings.handle.anchoredPosition = new Vector2(targetX, settings.handle.anchoredPosition.y);
            settings.controller.SetVolume(targetVolume);
        }
        else
        {
            StopAndStartCoroutine(ref settings.toggleCoroutine, AnimateToggle(settings.handle, targetX, settings.controller, targetVolume));
        }

    }

    // 코루틴 정지 후 실행시키는 함수
    private void StopAndStartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(routine);
    }

    // 사운드 On/Off 버튼 애니메이션 코루틴
    private IEnumerator AnimateToggle(RectTransform handle, float targetX, SoundController controller, float targetVolume)
    {
        yield return AnimateHandle(handle, targetX);
        controller.SetVolume(targetVolume);
    }

    #endregion


    #region 디스플레이 설정

    // Display 초기화 함수
    private void InitializeDisplayControllers()
    {
        InitializeToggle(effectSettings, ToggleEffect);
        InitializeToggle(shakingSettings, ToggleShaking);
        InitializeToggle(savingSettings, ToggleSaving);
    }

    // 토글 초기화 함수
    private void InitializeToggle(ToggleSettings settings, System.Action toggleAction)
    {
        settings.toggleButton.onClick.AddListener(() => toggleAction());
        settings.toggleButtonImage = settings.toggleButton.GetComponent<Image>();
        UpdateToggleButtonImage(settings.toggleButtonImage, settings.isOn);
        UpdateToggleUI(settings.handle, settings.isOn, true);
    }

    // 이펙트 토글 함수
    private void ToggleEffect()
    {
        UpdateToggleState(effectSettings);
    }

    // 화면 흔들림 토글 함수
    private void ToggleShaking()
    {
        UpdateToggleState(shakingSettings);
    }

    // 절전 모드 토글 함수
    private void ToggleSaving()
    {
        UpdateToggleState(savingSettings);
    }

    // 토글 상태 업데이트 함수
    private void UpdateToggleState(ToggleSettings settings)
    {
        settings.isOn = !settings.isOn;
        UpdateToggleUI(settings.handle, settings.isOn);
        UpdateToggleButtonImage(settings.toggleButtonImage, settings.isOn);

        GoogleSave();
    }

    // 토글 UI 업데이트 함수
    private void UpdateToggleUI(RectTransform handle, bool state, bool instant = false)
    {
        float targetX = state ? onX : offX;
        if (instant)
        {
            handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
        }
        else
        {
            StopAndStartCoroutine(ref effectSettings.toggleCoroutine, AnimateHandle(handle, targetX));
        }

    }

    // 토글 핸들 애니메이션 코루틴
    private IEnumerator AnimateHandle(RectTransform handle, float targetX)
    {
        float elapsedTime = 0f;
        float startX = handle.anchoredPosition.x;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            handle.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), handle.anchoredPosition.y);
            yield return null;
        }

        handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
    }

    #endregion


    #region 시스템 설정

    // System 초기화 함수
    private void InitializeSystemSettings()
    {
        // SlotPanel CanvasGroup 초기화
        slotPanelGroup = slotPanel.GetComponent<CanvasGroup>();
        if (slotPanelGroup == null)
        {
            slotPanelGroup = slotPanel.gameObject.AddComponent<CanvasGroup>();
        }

        // InformationPanel 초기 설정
        informationPanel.SetActive(false);

        // 버튼 및 패널 초기화
        originalButtonPositions = new Vector2[slotButtons.Length];
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int buttonIndex = i;
            originalButtonPositions[i] = slotButtons[i].GetComponent<RectTransform>().anchoredPosition;
            slotButtons[i].onClick.AddListener(() => OnSlotButtonClick(buttonIndex));
            informationPanels[i].SetActive(false);

            // 각 버튼에 CanvasGroup 추가
            CanvasGroup buttonGroup = slotButtons[i].gameObject.GetComponent<CanvasGroup>();
            if (buttonGroup == null)
            {
                buttonGroup = slotButtons[i].gameObject.AddComponent<CanvasGroup>();
            }

            // 각 Information Panel의 Content Panel에 RectMask2D와 CanvasGroup 추가
            Transform contentPanel = informationPanels[i].transform.GetChild(0);
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            if (contentRect.GetComponent<RectMask2D>() == null)
            {
                contentRect.gameObject.AddComponent<RectMask2D>();
            }
            if (contentRect.GetComponent<CanvasGroup>() == null)
            {
                contentRect.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 공통 뒤로가기 버튼 이벤트 설정
        informationPanelBackButton.onClick.AddListener(() =>
        {
            if (currentActivePanel != -1)
            {
                StartCoroutine(HideInformationPanel(currentActivePanel));
            }
        });

        // quitButton에 CanvasGroup 추가
        CanvasGroup exitButtonGroup = quitButton.gameObject.GetComponent<CanvasGroup>();
        if (exitButtonGroup == null)
        {
            exitButtonGroup = quitButton.gameObject.AddComponent<CanvasGroup>();
        }

        // informationPanelBackButton에 CanvasGroup 추가
        CanvasGroup backButtonGroup = informationPanelBackButton.gameObject.GetComponent<CanvasGroup>();
        if (backButtonGroup == null)
        {
            backButtonGroup = informationPanelBackButton.gameObject.AddComponent<CanvasGroup>();
        }
        backButtonGroup.alpha = 0f;

        // 시스템 버튼 클릭 이벤트 추가
        subOptionMenuButtons[(int)OptionMenuType.System].onClick.AddListener(() =>
        {
            ActivateMenu(OptionMenuType.System);
            ResetSystemMenu();
        });

        // Quit 버튼 클릭 이벤트 추가
        quitButton.onClick.AddListener(() => { GameManager.Instance.HandleQuitInput(); });
    }

    // 시스템 메뉴 초기화 함수
    private void ResetSystemMenu()
    {
        // 모든 코루틴 정지
        StopAllCoroutines();

        // 현재 활성화된 패널이 있다면 비활성화
        if (currentActivePanel != -1)
        {
            informationPanels[currentActivePanel].SetActive(false);
        }

        slotPanel.gameObject.SetActive(true);
        informationPanel.SetActive(false);

        // 모든 버튼 초기화
        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].gameObject.SetActive(true);
            RectTransform buttonRect = slotButtons[i].GetComponent<RectTransform>();
            buttonRect.anchoredPosition = originalButtonPositions[i];
            CanvasGroup buttonGroup = slotButtons[i].GetComponent<CanvasGroup>();
            buttonGroup.alpha = 1f;
        }

        // Quit 버튼 초기화
        quitButton.gameObject.SetActive(true);
        quitButton.GetComponent<CanvasGroup>().alpha = 1f;

        // back 버튼 초기화
        informationPanelBackButton.GetComponent<CanvasGroup>().alpha = 0f;

        currentActivePanel = -1;
    }

    // 슬롯 버튼 클릭 이벤트 처리 함수
    private void OnSlotButtonClick(int index)
    {
        if (currentActivePanel != -1)
        {
            return;
        }
        currentActivePanel = index;
        StartCoroutine(ShowInformationPanel(index));
    }

    // Information Panel 펼치는 코루틴
    private IEnumerator ShowInformationPanel(int index)
    {
        // 선택된 버튼을 제외한 다른 버튼들 페이드 아웃 및 선택된 버튼 이동 동시 실행
        List<IEnumerator> animations = new List<IEnumerator>();
        animations.Add(MoveButton(slotButtons[index].GetComponent<RectTransform>(), originalButtonPositions[index], new Vector2(0, 465)));
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i != index)
            {
                animations.Add(FadeButton(slotButtons[i].GetComponent<CanvasGroup>(), 1f, 0f));
            }
        }
        animations.Add(FadeButton(quitButton.GetComponent<CanvasGroup>(), 1f, 0f));

        foreach (var anim in animations)
        {
            StartCoroutine(anim);
        }

        yield return new WaitForSeconds(systemAnimDuration);

        // 애니메이션 완료 후 버튼들 비활성화
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i != index)
            {
                slotButtons[i].gameObject.SetActive(false);
            }
        }
        quitButton.gameObject.SetActive(false);

        // 정보 패널 활성화 및 펼치기 애니메이션
        informationPanel.SetActive(true);
        informationPanels[index].SetActive(true);

        // 패널의 초기 크기와 위치 설정
        RectTransform panelRect = informationPanels[index].GetComponent<RectTransform>();
        Transform contentPanel = panelRect.GetChild(0);
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        CanvasGroup contentGroup = contentPanel.GetComponent<CanvasGroup>();

        // 컨텐츠 패널의 Mask 설정
        RectMask2D mask = contentRect.GetComponent<RectMask2D>();
        if (mask == null)
        {
            mask = contentRect.gameObject.AddComponent<RectMask2D>();
        }
        mask.enabled = true;

        // 컨텐츠 패널 초기 알파값 설정
        contentGroup.alpha = 0f;

        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 0);
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 480);
        float elapsedTime = 0f;

        // Back 버튼 페이드 인 시작
        StartCoroutine(FadeButton(informationPanelBackButton.GetComponent<CanvasGroup>(), 0f, 1f));

        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            float currentHeight = Mathf.Lerp(0, 960, t);
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, currentHeight);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, Mathf.Lerp(480, 0, t));
            yield return null;
        }

        panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 960);
        panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 0);

        // 패널이 다 펼쳐진 후 컨텐츠 페이드 인
        elapsedTime = 0f;
        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            contentGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        contentGroup.alpha = 1f;
    }

    // Information Panel 접는 코루틴
    private IEnumerator HideInformationPanel(int index)
    {
        // Back 버튼 페이드 아웃 시작
        StartCoroutine(FadeButton(informationPanelBackButton.GetComponent<CanvasGroup>(), 1f, 0f));

        RectTransform panelRect = informationPanels[index].GetComponent<RectTransform>();
        Transform contentPanel = panelRect.GetChild(0);
        CanvasGroup contentGroup = contentPanel.GetComponent<CanvasGroup>();

        // 먼저 컨텐츠 페이드 아웃
        float elapsedTime = 0f;
        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            contentGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        contentGroup.alpha = 0f;

        // 컨텐츠가 완전히 페이드 아웃된 후 패널 접기
        elapsedTime = 0f;
        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            float currentHeight = Mathf.Lerp(960, 0, t);
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, currentHeight);
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, Mathf.Lerp(0, 480, t));
            yield return null;
        }

        // Mask 비활성화
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        RectMask2D mask = contentRect.GetComponent<RectMask2D>();
        if (mask != null)
        {
            mask.enabled = false;
        }

        informationPanels[index].SetActive(false);
        informationPanel.SetActive(false);

        // 모든 버튼 활성화
        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].gameObject.SetActive(true);
        }
        quitButton.gameObject.SetActive(true);

        // 버튼 이동 및 페이드 인 동시 실행
        List<IEnumerator> animations = new List<IEnumerator>();
        animations.Add(MoveButton(slotButtons[index].GetComponent<RectTransform>(), new Vector2(0, 465), originalButtonPositions[index]));
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i != index)
            {
                animations.Add(FadeButton(slotButtons[i].GetComponent<CanvasGroup>(), 0f, 1f));
            }
        }
        animations.Add(FadeButton(quitButton.GetComponent<CanvasGroup>(), 0f, 1f));

        foreach (var anim in animations)
        {
            StartCoroutine(anim);
        }

        yield return new WaitForSeconds(systemAnimDuration);

        currentActivePanel = -1;
    }

    // 버튼 이동 애니메이션 코루틴
    private IEnumerator MoveButton(RectTransform buttonRect, Vector2 startPos, Vector2 endPos)
    {
        float elapsedTime = 0f;

        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            buttonRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        buttonRect.anchoredPosition = endPos;
    }

    // 버튼 페이드 애니메이션 코루틴
    private IEnumerator FadeButton(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < systemAnimDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / systemAnimDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    #endregion


    #region 버튼에 SFX 추가

    // 씬 로드 이벤트에 리스너 추가
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 씬 로드 이벤트에서 리스너 제거
    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 호출되는 함수
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(AddSFXToAllButtonsDelayed());
    }

    // 지연된 버튼 SFX 추가를 위한 코루틴
    private IEnumerator AddSFXToAllButtonsDelayed()
    {
        // 한 프레임 대기하여 모든 오브젝트가 초기화되도록 기다림
        yield return null;
        AddSFXToAllButtons();
    }

    // 모든 버튼에 SFX 추가하는 함수
    private void AddSFXToAllButtons()
    {
        // 현재 씬의 모든 버튼 찾기 (비활성화된 것 포함)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button button in allButtons)
        {
            // 씬에 있는 실제 게임오브젝트인 경우에만 SFX 등록
            if (button.gameObject.scene.isLoaded)
            {
                RegisterDynamicButton(button);
            }
        }
    }

    // 동적으로 생성된 버튼에 SFX 추가하는 함수
    public void RegisterDynamicButton(Button button)
    {
        if (button != null)
        {
            // 기존 리스너 제거 (중복 방지)
            button.onClick.RemoveListener(PlayButtonClickSound);
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    // 버튼 클릭 시 SFX 재생하는 함수
    public void PlayButtonClickSound()
    {
        if (sfxSettings.controller != null && sfxSettings.isOn)
        {
            sfxSettings.controller.PlayOneShot();
        }
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public bool bgmIsOn;        // BGM 활성화 상태
        public float bgmVolume;     // BGM 볼륨값
        public bool sfxIsOn;        // SFX 활성화 상태
        public float sfxVolume;     // SFX 볼륨값

        public bool effectIsOn;     // 이펙트 활성화 상태
        public bool shakingIsOn;    // 화면 흔들림 활성화 상태
        public bool savingIsOn;     // 절전모드 활성화 상태
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            bgmIsOn = bgmSettings.isOn,
            bgmVolume = bgmSettings.slider.value,
            sfxIsOn = sfxSettings.isOn,
            sfxVolume = sfxSettings.slider.value,

            effectIsOn = effectSettings.isOn,
            shakingIsOn = shakingSettings.isOn,
            savingIsOn = savingSettings.isOn
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        // Sound Settings
        bgmSettings.isOn = savedData.bgmIsOn;
        bgmSettings.slider.value = savedData.bgmVolume;
        sfxSettings.isOn = savedData.sfxIsOn;
        sfxSettings.slider.value = savedData.sfxVolume;

        UpdateToggleUI(bgmSettings.isOn, true, true);
        UpdateToggleUI(sfxSettings.isOn, false, true);
        UpdateToggleButtonImage(bgmSettings.toggleButtonImage, bgmSettings.isOn);
        UpdateToggleButtonImage(sfxSettings.toggleButtonImage, sfxSettings.isOn);
        SetSoundToggleImage(true);
        SetSoundToggleImage(false);

        // Display Settings
        effectSettings.isOn = savedData.effectIsOn;
        shakingSettings.isOn = savedData.shakingIsOn;
        savingSettings.isOn = savedData.savingIsOn;

        UpdateToggleUI(effectSettings.handle, effectSettings.isOn, true);
        UpdateToggleUI(shakingSettings.handle, shakingSettings.isOn, true);
        UpdateToggleUI(savingSettings.handle, savingSettings.isOn, true);
        UpdateToggleButtonImage(effectSettings.toggleButtonImage, effectSettings.isOn);
        UpdateToggleButtonImage(shakingSettings.toggleButtonImage, shakingSettings.isOn);
        UpdateToggleButtonImage(savingSettings.toggleButtonImage, savingSettings.isOn);

        isDataLoaded = true;
    }

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            GoogleManager.Instance.SaveGameState();
        }
    }

    #endregion


}

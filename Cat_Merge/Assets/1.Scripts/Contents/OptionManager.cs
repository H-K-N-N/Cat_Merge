using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// OptionManager Script
public class OptionManager : MonoBehaviour
{
    // Singleton Instance
    public static OptionManager Instance { get; private set; }

    // 사운드 컨트롤 내부 클래스
    public class SoundController
    {
        private AudioSource audioSource;
        private Slider volumeSlider;
        private AudioClip soundClip;

        public SoundController(GameObject parent, Slider slider, bool loop, AudioClip clip)
        {
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.loop = loop;
            volumeSlider = slider;
            soundClip = clip;

            audioSource.clip = soundClip;

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

            if ((isBgm && !Instance.bgmSettings.isOn) || (!isBgm && !Instance.sfxSettings.isOn))
            {
                audioSource.volume = 0f;
            }
            else
            {
                audioSource.volume = volume;
            }

            Instance.SetSoundToggleImage(isBgm);
        }

        // 사운드 재생
        public void Play()
        {
            audioSource.Play();
        }

        // 사운드 정지
        public void Stop()
        {
            audioSource.Stop();
        }

        // AudioSource getter 추가
        public AudioSource GetAudioSource()
        {
            return audioSource;
        }
    }

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
    private string activeColorCode = "#5f5f5f";                 // 활성화상태 Color
    private string inactiveColorCode = "#FFFFFF";               // 비활성화상태 Color

    // ======================================================================================================================
    // [토글 버튼 관련 설정]

    private float onX = 65f, offX = -65f;                       // 핸들 버튼 x좌표
    private float moveDuration = 0.2f;                          // 토글 애니메이션 지속 시간

    // ======================================================================================================================
    // [Sound]

    [System.Serializable]
    private class SoundSettings
    {
        public Slider slider;                                   // 볼륨 조절 슬라이더
        public Button toggleButton;                             // 토글 버튼
        public RectTransform handle;                            // 토글 핸들
        public Image onOffImage;                                // On/Off 이미지
        public SoundController controller;                      // 사운드 컨트롤러
        public Coroutine toggleCoroutine;                       // 토글 애니메이션 코루틴
        public bool isOn = true;                                // 토글 상태
    }

    [Header("---[BGM]")]
    [SerializeField] private SoundSettings bgmSettings = new SoundSettings();

    [Header("---[SFX]")]
    [SerializeField] private SoundSettings sfxSettings = new SoundSettings();

    [Header("---[Common]")]
    private Sprite soundOnImage;                                // 사운드 On 이미지
    private Sprite soundOffImage;                               // 사운드 Off 이미지

    // ======================================================================================================================
    // [Display]

    [System.Serializable]
    private class ToggleSettings
    {
        public Button toggleButton;                             // 토글 버튼
        public RectTransform handle;                            // 토글 핸들
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
        optionMenuPanel.SetActive(false);

        InitializeOptionManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("OptionMenu", optionMenuPanel, optionButtonImage);
    }

    // ======================================================================================================================

    // 모든 OptionManager 시작 함수들 모음
    private void InitializeOptionManager()
    {
        InitializeOptionButton();
        InitializeSubMenuButtons();

        InitializeSoundControllers();
        InitializeDisplayControllers();
    }

    // OptionButton 초기화 함수
    private void InitializeOptionButton()
    {
        optionButton.onClick.AddListener(() => activePanelManager.TogglePanel("OptionMenu"));
        optionBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("OptionMenu"));
    }

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
        soundOnImage = Resources.Load<Sprite>("Sprites/Cats/1");
        soundOffImage = Resources.Load<Sprite>("Sprites/Cats/2");
        bgmSettings.onOffImage.sprite = soundOnImage;
        sfxSettings.onOffImage.sprite = soundOnImage;

        // 토글 버튼 이벤트 설정
        bgmSettings.toggleButton.onClick.AddListener(() => ToggleSound(true));
        sfxSettings.toggleButton.onClick.AddListener(() => ToggleSound(false));

        UpdateToggleUI(bgmSettings.isOn, true, true);
        UpdateToggleUI(sfxSettings.isOn, false, true);
    }

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
        UpdateToggleUI(settings.handle, settings.isOn, true);
    }

    // ======================================================================================================================
    // [서브 메뉴]

    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)OptionMenuType.End; i++)
        {
            int index = i;
            subOptionMenuButtons[index].onClick.AddListener(() => ActivateMenu((OptionMenuType)index));
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
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================
    // [사운드 설정]

    // 사운드 On/Off 토글 함수
    public void ToggleSound(bool isBgm)
    {
        var settings = isBgm ? bgmSettings : sfxSettings;
        settings.isOn = !settings.isOn;
        SetSoundToggleImage(isBgm);
        UpdateToggleUI(settings.isOn, isBgm);
    }

    // 사운드 On/Off 이미지 변경 함수
    private void SetSoundToggleImage(bool isBgm)
    {
        var settings = isBgm ? bgmSettings : sfxSettings;
        Image targetImage = settings.onOffImage;
        targetImage.sprite = (!settings.isOn || settings.slider.value == 0) ? soundOffImage : soundOnImage;
    }

    // 사운드 토글 UI 업데이트 함수
    private void UpdateToggleUI(bool state, bool isBgm, bool instant = false)
    {
        var settings = isBgm ? bgmSettings : sfxSettings;
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

    // ======================================================================================================================
    // [디스플레이 설정]

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

    // ======================================================================================================================



}
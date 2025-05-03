using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;

public class GoogleAdsManager : MonoBehaviour
{


    #region Variables

    // 테스트 광고를 위한 광고 단위 ID 설정
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _adUnitId = "unused";
#endif

    private RewardedInterstitialAd rewardedInterstitialAd;
    private bool isAdWatched = false;       // 광고를 끝까지 시청했는지
    private bool hasEarnedReward = false;   // 보상을 받을 자격이 있는지
    private bool isAdShowing = false;       // 광고가 현재 보여지고 있는지

    private Action onAdCompleteCallback;    // 광고 완료시 실행할 콜백

    #endregion


    #region Unity Methods

    public void Awake()
    {
        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
        });
    }

    public void Start()
    {
        LoadRewardedInterstitialAd();
    }

    #endregion


    #region Load Ad

    public void LoadRewardedInterstitialAd()
    {
        // 새로운 광고를 로드하기 전에 이전 광고 정리
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedInterstitialAd.Load(_adUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogWarning("보상형 전면 광고 로딩 실패. 에러: " + error);
                    return;
                }

                rewardedInterstitialAd = ad;
                RegisterEventHandlers(rewardedInterstitialAd);
            });
    }

    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // 광고가 닫힐 때
        ad.OnAdFullScreenContentClosed += () =>
        {
            HandleAdClosed();
        };

        // 광고 표시 실패시
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogWarning("광고 표시 실패: " + error);
            HandleAdFailure();
        };

        // 광고가 표시될 때
        ad.OnAdFullScreenContentOpened += () =>
        {
            isAdShowing = true;
            isAdWatched = false;
            hasEarnedReward = false;
        };

        // 보상 획득시
        ad.OnAdPaid += (AdValue adValue) =>
        {
            isAdWatched = true;
        };
    }

    #endregion


    #region Show Ad

    // CashForAd 광고 (ShopManager)
    public void ShowRewardedCashForAd()
    {
        ShowAd(() => ShopManager.Instance.OnCashForAdRewardComplete());
    }

    // DoubleCoinForAd 광고 (ShopManager)
    public void ShowRewardedDoubleCoinForAd()
    {
        ShowAd(() => ShopManager.Instance.OnDoubleCoinAdRewardComplete());
    }

    private void ShowAd(Action onComplete)
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            onAdCompleteCallback = onComplete;

            // 광고 시청 전에 전체 게임 데이터 저장
            GameManager.Instance.SaveGame();

            rewardedInterstitialAd.Show((Reward reward) =>
            {
                hasEarnedReward = true;
            });
        }
        else
        {
            Debug.LogWarning("광고를 표시할 수 없습니다. 새로운 광고를 로드합니다.");
            LoadRewardedInterstitialAd();
            ShopManager.Instance.ResetAdState();
        }
    }

    private void HandleAdClosed()
    {
        isAdShowing = false;

        // 광고를 끝까지 보고 보상을 받을 자격이 있는 경우
        if (isAdWatched && hasEarnedReward)
        {
            if (onAdCompleteCallback != null)
            {
                onAdCompleteCallback.Invoke();
            }
        }
        // 광고를 중간에 종료했거나 보상을 받지 못한 경우
        else
        {
            ShopManager.Instance.ResetAdState();
        }

        // 상태 초기화
        isAdWatched = false;
        hasEarnedReward = false;
        onAdCompleteCallback = null;

        // 새로운 광고 로드
        LoadRewardedInterstitialAd();
    }

    private void HandleAdFailure()
    {
        isAdShowing = false;
        isAdWatched = false;
        hasEarnedReward = false;
        onAdCompleteCallback = null;

        ShopManager.Instance.ResetAdState();
        LoadRewardedInterstitialAd();
    }

    #endregion


}
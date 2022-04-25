using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;
using TMPro;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif

public class AdMobManager : MonoBehaviour
{
    private const string kRewardedShowCountText = "REWARDED ADS SHOW COUNT: ";
    private const string kInterstitialSkippedText = "INTERSTITIAL SKIPPED";
    private const string kInterstitialWatchedCompletelyText = "INTERSTITIAL WATCHED 100%";

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    private string adUnitIdRew;
    private string adUnitIdInt;
    private bool isInterstitialSkipped = true;
    private int rewardedAdsShowCount = 0;

    [SerializeField] private Button loadInterstitialButton;
    [SerializeField] private Button showInterstitialButton;
    [SerializeField] private Button loadRewardedButton;
    [SerializeField] private Button showRewardedButton;
    [SerializeField] private TextMeshProUGUI interstitialStatusText;
    [SerializeField] private TextMeshProUGUI rewardedAdsShowCountText;

    private void Start()
    {
#if UNITY_IOS
        // Check the user's consent status.
        // If the status is undetermined, display the request request: 
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
#endif

#if UNITY_ANDROID
        adUnitIdRew = "ca-app-pub-7913894988346674/3281215368";
        adUnitIdInt = "ca-app-pub-7913894988346674/7071506316";
#elif UNITY_IPHONE
        //adUnitIdRew = "ca-app-pub-7913894988346674/4634891145";
        //adUnitIdInt = "ca-app-pub-7913894988346674/9887217825";

        //This is testing ad units
        adUnitIdRew = "ca-app-pub-3940256099942544/1712485313";
        adUnitIdInt = "ca-app-pub-3940256099942544/5135589807";
#else
        adUnitIdRew = "unexpected_platform";
        adUnitIdInt = "unexpected_platform";
#endif

        SetupAdUnits();
        InitInterstitialListeners();
        InitRewardedListeners();
        InitButtonListeners();
        DisableButtons();
    }

    private void InitInterstitialListeners()
    {
        // Called when an ad request has successfully loaded.
        this.interstitialAd.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitialAd.OnAdOpening += HandleOnAdOpening;
        // Called when an ad is closed.
        this.interstitialAd.OnAdClosed += HandleOnAdClosed;
        // Called when an ad is completely watched
        this.interstitialAd.OnPaidEvent += HandleOnPaidEvent;
    }

    private void SetupAdUnits()
    {
        this.interstitialAd = new InterstitialAd(adUnitIdInt);
        this.rewardedAd = new RewardedAd(adUnitIdRew);
    }

    private void InitRewardedListeners()
    {
        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;
    }

    private void InitButtonListeners()
    {
        loadInterstitialButton.onClick.AddListener(LoadInterstitialAd);
        showInterstitialButton.onClick.AddListener(ShowInterstitialAd);
        loadRewardedButton.onClick.AddListener(LoadRewardedAd);
        showRewardedButton.onClick.AddListener(ShowRewardedAd);
    }

    private void DisableButtons()
    {
        loadInterstitialButton.interactable = false;
        showInterstitialButton.interactable = false;
        loadRewardedButton.interactable = false;
        showRewardedButton.interactable = false;
    }

    #region Interstitial Event Handlers
    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLoaded event received");
        showInterstitialButton.interactable = true;
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleFailedToReceiveAd event received with message: " + args.LoadAdError.GetMessage());
    }

    public void HandleOnAdOpening(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpening event received");
        showInterstitialButton.interactable = false;
        isInterstitialSkipped = true;
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleAdClosed event received");
        if (isInterstitialSkipped)
            interstitialStatusText.text = kInterstitialSkippedText;
        else
            interstitialStatusText.text = kInterstitialWatchedCompletelyText;
    }

    private void HandleOnPaidEvent(object sender, AdValueEventArgs e)
    {
        Debug.Log("HandleOnPaidEvent event received");
        isInterstitialSkipped = false;
    }
    #endregion

    #region Rewarded Event Handlers
    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        showRewardedButton.interactable = true;
        Debug.Log("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleRewardedAdFailedToLoad event received with message: " + args.LoadAdError.GetMessage());
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdOpening event received");
        showRewardedButton.interactable = false;
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.Log("HandleRewardedAdFailedToShow event received with message: " + args.AdError.GetMessage());
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleRewardedAdClosed event received");
        rewardedAdsShowCount++;
        rewardedAdsShowCountText.text = kRewardedShowCountText + rewardedAdsShowCount.ToString();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log("HandleRewardedAdRewarded event received for " + amount.ToString() + " " + type);
    }
    #endregion

    public void Init()
    {
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetSameAppKeyEnabled(true).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);
    }

    private void HandleInitCompleteAction(InitializationStatus obj)
    {
        Debug.Log("SDK initialization is complete");
        loadInterstitialButton.interactable = true;
        loadRewardedButton.interactable = true;
    }

    #region Interstitial Ads
    public void LoadInterstitialAd()
    {
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Empty ad request was created.");

        this.interstitialAd.LoadAd(request);
        Debug.Log("The interstitial ad was loaded");
    }

    public void ShowInterstitialAd()
    {
        if (this.interstitialAd.IsLoaded())
        {
            this.interstitialAd.Show();
        }
    }
    #endregion

    #region Rewarded Ads
    public void LoadRewardedAd()
    {
        AdRequest request = new AdRequest.Builder().Build();
        Debug.Log("Empty ad request was created.");

        this.rewardedAd.LoadAd(request);
        Debug.Log("The rewarded ad was loaded");
    }

    public void ShowRewardedAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
    }
    #endregion
}

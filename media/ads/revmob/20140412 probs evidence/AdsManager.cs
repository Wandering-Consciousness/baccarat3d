using Chartboost;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/**
 * Class for managing our various mobile ad networks.
 */
public class AdsManager : MonoBehaviour, IRevMobListener
{
    // Revmob app IDs
    private static readonly Dictionary<String, String> REVMOB_APP_IDS = new Dictionary<String, String> () {
        { "Android", "52611392779d14dad9000123"} ,
        { "IOS", "526113a5519273264a000074" }
    };
    public RevMob revmob;
    public RevMobBanner revmobBanner;

    private AppCCloud appCCloud;

    // How often to display intersitial ads
    public const int NEW_GAME_INTERSITIAL_SHOW_FREQUENCY = 5;

    // Banner ad dimensions relative to the current screen orientation
    private static int BANNER_AD_HEIGHT {
        get {
            if (GUIControls.isPortraitNoConditions) {
                //Debug.Log("[ads]BANNER AD HEIGHT PORTRAIT " +Mathf.RoundToInt(Screen.height * 0.1f));
                return Mathf.RoundToInt (Screen.height * 0.14f);
            } else {
                //Debug.Log("[ads]BANNER AD HEIGHT LANDSCAPE " + Mathf.RoundToInt(Screen.height * 0.05f));
                return Mathf.RoundToInt (Screen.height * 0.075f);
            }
        }
        set {}
    }

    private static int BANNER_AD_WIDTH {
        get {
            if (GUIControls.isPortraitNoConditions) {
                //Debug.Log("[ads]BANNER AD WIDTH PORTRAIT " +Mathf.RoundToInt(Screen.width * 1f));
                return Mathf.RoundToInt (Screen.width * 0.75f);
            } else {
                //Debug.Log("[ads]BANNER AD WIDTH LANDSCAPE " + Mathf.RoundToInt(Screen.width * 0.5f));
                return Mathf.RoundToInt (Screen.width * 0.85f);
            }
        }
        set {}
    }

    private int currentBannerAdWidth;
    private int currentBannerAdHeight;

    // Keep track of whether RebMob banners are showing
    private bool revMobBannerShowing = false;
    private bool revMobSessionStarted = false;

    // Vars for switching between the various ad networks
    private bool showChartboostIntersitial = true; // RevMob <->Chartboost
    private bool showTapForTapBanner = true; // Tap For Tap<->RevMob
    //private bool showRevMobOnNextRound = false;
    private bool showTapForTapFullscreen = false; // Tap For Tap<->RevMob
    private bool showAppCAds = true; // move icons banner vs normal banner

    // Used to trigger on orientation callback
    private bool isCurrentlyPortrait = true;
    private bool isCurrentlyLandscape = false;
    private bool isCurrentlyShowingTapForTapAdView = false;

    // For ads, they need be on an active GameObject
    void Update ()
    {
        if (GameState.Instance.isProEdition)
            return;

        if (isCurrentlyPortrait && !GUIControls.isPortraitNoConditions) {
            // Fire callback when orientaiton changes from portrait->landscape
            Debug.Log ("[ads]Detected orientation change from portrait->landscape");
            OnOrientationChange ();
        }

        if (isCurrentlyLandscape && GUIControls.isPortraitNoConditions) {
            // Fire callback when orientaiton changes from landscape->portrait
            Debug.Log ("[ads]Detected orientation change from landscape->portrait");
            OnOrientationChange ();
        }

        if ((GameState.Instance.camerasManager.birdsEyeSqueezeCamera.enabled ||
             GameState.Instance.tutorialCounter <= 1) &&
                revMobBannerShowing &&
               (Input.deviceOrientation == DeviceOrientation.LandscapeLeft ||
                Input.deviceOrientation == DeviceOrientation.LandscapeRight)) {
            Debug.LogWarning("[ads]Hiding RevMob banner in landscape squeeze/tutorial modes");
            revmobBanner.Hide();
            revmobBanner.Release();
            revmobBanner = null;
            revMobBannerShowing = false;
        }

        // Chartboost
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android) {
            if (Input.GetKeyUp (KeyCode.Escape)) {
                if (CBBinding.onBackPressed ())
                    return;
            }
        }
#endif

    }

    void Start ()
    {
        // Take not of current orientation
        isCurrentlyPortrait = GUIControls.isPortraitNoConditions;
        isCurrentlyLandscape = !GUIControls.isPortraitNoConditions;

        // Register ourselves with GameState
        GameState.Instance.adsManager = this;

        // Initialize Tap for Tap with my API key
        Debug.Log ("[ads]Initializing Tap for Tap");
        TapForTap.initialize ("xxxxxxx");
        TapForTap.PrepareAppWall ();

        // Initialize Tapjoy with my API keys
        Debug.Log ("[ads]Initializing Tapjoy");
        if (Application.platform == RuntimePlatform.Android)
        {
            TapjoyPlugin.RequestTapjoyConnect("bxxxxxxxxxxxxxx", 
                                              "yxxxxxxxxxxxxxxx");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            
            Dictionary<String, String> dict = new Dictionary<String, String>();
            dict.Add("TJC_OPTION_COLLECT_MAC_ADDRESS", TapjoyPlugin.MacAddressOptionOffWithVersionCheck);
			TapjoyPlugin.RequestTapjoyConnect("bxxxxxxxxxxxxxx", 
			                                  "bxxxxxxxxxxxxxx", 
                                              dict);                              
        }

        if (Utils.isJapanese ()) {
            // AppC ads
            Debug.Log ("[ads]Initializing AppC");
            appCCloud = new AppCCloud();
#if UNITY_IPHONE && !UNITY_EDITOR
            appCCloud.SetMK_iOS("xxxxxxxxxxxxxxxxxxxxxxxxxx");
#endif
            appCCloud.Start();
        }
        
        // Check if we're the free or pro version
        if (GameState.Instance.isProEdition) {
            Debug.Log ("[ads]This is PRO edition so not initializing ads");
            return; // none of the code below needs to be initiated for pro version
        } else
            Debug.Log ("[ads]Initializing ad networks for FREE version");
        
        // Initialize random booleans to decide which ad network gets showed first
        /* Random.Range kept returning the minimum so this code doesn't work as is...
        showChartboostIntersitial = UnityEngine.Random.Range (1, 3) == 1 ? true : false; // RevMob <->Chartboost
        Debug.Log("[ads]showChartboostIntersitial? " + showChartboostIntersitial);
        showTapForTapBanner = UnityEngine.Random.Range (1, 3) == 1 ? true : false; // Tap For Tap<->RevMob
        Debug.Log("[ads]showTapForTapBanner? " + showTapForTapBanner);
        showAppCMoveIcons = UnityEngine.Random.Range (1, 3) == 1 ? true : false; // move icons banner vs other
        Debug.Log("[ads]showAppCMoveIcons? " + showAppCMoveIcons);
         */

        // TapForTap stuff only needed in free version
        TapForTap.PrepareInterstitial ();
        StartCoroutine (SetUserLocationCoroutine ());
       
        if (!Utils.isJapanese ()) {
            // Default to English based ads
    
            // Chartboost
            Debug.Log ("[ads]Initializing Chartboost ads");
#if UNITY_ANDROID && !UNITY_EDITOR
            CBBinding.init();
#elif UNITY_IPHONE && !UNITY_EDITOR
            CBBinding.init("xxxxxxx", "xxxxxxxxxxxxxxxxxxxxxx");
#endif
        }

        refresh (1);
    }

    // Refresh AppC ads when in AR... because the MoveIcon view was blocking the gyrocam horizontal scrollbar
    public void refreshAppCAds() {
        if (showAppCAds && !GameState.Instance.isProEdition && Utils.isJapanese()) {
            Debug.Log ("[ads]Refreshing AppC ads for AR view");
            appCCloud.Ad.HideAllViews();
            appCCloud.Ad.ShowMarqueeView (AppCCloud.Vertical.Bottom);
        }
    }

    // Turn off all ads
    public void turnOffAllAds() {
        Debug.Log ("Turning off all ads");
        if (Utils.isJapanese()) {
            if (appCCloud != null && appCCloud.Ad != null)
                appCCloud.Ad.HideAllViews();
            if (isCurrentlyShowingTapForTapAdView) {
                TapForTap.RemoveAdView ();
                isCurrentlyShowingTapForTapAdView = false;
            }
        } else {
            if (isCurrentlyShowingTapForTapAdView) {
                TapForTap.RemoveAdView ();
                isCurrentlyShowingTapForTapAdView = false;
            }
            if (revmobBanner != null) {
                revmobBanner.Hide();
                revmobBanner.Release();
                revmobBanner = null;
                revMobBannerShowing = false;
            }
        }
    }
    
    // Refresh ads
    public void refresh (int roundNumber)
    {
#if UNITY_EDITOR
        return;
#endif
        if (GameState.Instance.isProEdition)
            return;

        if (!revMobSessionStarted) {
            // Revmob
            Debug.Log ("[ads]Initializing RevMob ads");
            revmob = RevMob.Start (REVMOB_APP_IDS, this.gameObject.name);
            revMobSessionStarted = true;
        }
        
        Debug.Log ("[ads]Refreshing ads");
       
        // Refresh banners
        if (Utils.isJapanese ()) {
            if (showAppCAds) {
                // Show Tap for Tap banner
                Debug.Log ("[ads]Showing Tap for Tap banner");
                appCCloud.Ad.HideAllViews ();
                TapForTap.CreateAdView (TapForTapVerticalAlignment.BOTTOM, TapForTapHorizontalAlignment.CENTER);
                isCurrentlyShowingTapForTapAdView = true;
            } else {
                // Show AppC move icon view
                //appCCloud.Ad.HideSimpleView ();
                TapForTap.RemoveAdView ();
                isCurrentlyShowingTapForTapAdView = false;
                if (GUIControls.isPortraitNoConditions && !GameState.Instance.camerasManager.isAR()) {
                    Debug.Log ("[ads]Showing AppC move icon banner");
                    appCCloud.Ad.ShowMoveIconView (AppCCloud.Horizon.Center, AppCCloud.Vertical.Bottom);
                } else {    
                    Debug.Log ("[ads]Showing AppC marquee");
                    appCCloud.Ad.ShowMarqueeView (AppCCloud.Vertical.Bottom);
                }
            }
            showAppCAds = !showAppCAds; // alternate between the two types of AppC banners and Tap for Tap
        } else {
            // Default to English banners
            if (showTapForTapBanner) {
                // Tap for Tap banners
                Debug.Log ("[ads]Showing Tap for Tap banner");
                if (revmobBanner != null) {
                    revmobBannerHide();
                } else {
                    Debug.LogWarning ("[ads]Couldn't remove RevMob ad banner: revmobBanner=" + revmobBanner + ", isShowing?" + revMobBannerShowing);
                }
                TapForTap.CreateAdView (TapForTapVerticalAlignment.BOTTOM, TapForTapHorizontalAlignment.CENTER);
                isCurrentlyShowingTapForTapAdView = true;
                //showRevMobOnNextRound = true;
                currentBannerAdWidth = 0;
                currentBannerAdHeight = 0;
            } else {
                TapForTap.RemoveAdView ();
                isCurrentlyShowingTapForTapAdView = false;
                Debug.Log ("[ads]Showing RevMob banner");
#if UNITY_ANDROID && !UNITY_EDITOR
                if (revmob != null) {
                    currentBannerAdWidth = BANNER_AD_WIDTH;
                    currentBannerAdHeight = BANNER_AD_HEIGHT;
                    if (isCurrentlyPortrait) {
                        Debug.Log ("[ads]Showing Android RevMob banner portrait");
                        revmobBanner = revmob.CreateBanner(RevMob.Position.BOTTOM);//, Screen.width/3, 1, currentBannerAdWidth, currentBannerAdHeight);
                    } else {
                        Debug.Log ("[ads]Showing Android RevMob banner landscape");
                        revmobBanner = revmob.CreateBanner(RevMob.Position.BOTTOM, Screen.width/3 + currentBannerAdWidth/4, 1, currentBannerAdWidth, currentBannerAdHeight);
                    }
                }
#elif UNITY_IPHONE && !UNITY_EDITOR
                if (revmob != null) {
                    currentBannerAdWidth = BANNER_AD_WIDTH;
                    currentBannerAdHeight = BANNER_AD_HEIGHT;
                    if (isCurrentlyPortrait) {
                        Debug.Log ("[ads]Showing iOS RevMob banner portrait");
                        revmobBanner = revmob.CreateBanner();
                    } else {
                        Debug.Log ("[ads]Showing iOS RevMob banner landscape");
                        revmobBanner = revmob.CreateBanner((Screen.width/4)-150, (Screen.height/2)-70, 300, 45, null, null);
                    }
                }
#endif
                Debug.Log ("[ads]RevMob banner debug: Screen.width == " + Screen.width + ", Screen.height == " + Screen.height);
                revmobBannerShow ();
            }
            showTapForTapBanner = !showTapForTapBanner;
        }

        // Show fullscreen ads only every x rounds
        if (roundNumber != 1 && roundNumber % NEW_GAME_INTERSITIAL_SHOW_FREQUENCY == 0) {
            if (Utils.isJapanese ()) {
                // AppC intersitial
                Debug.Log ("[ads]Showing AppC cutin");
                appCCloud.Ad.ShowCutinView();
            } else {
                // Default to English intersitials
                if (showChartboostIntersitial) {
                    // Chartboost intersitial
                    Debug.Log ("[ads]Showing Chartboost intersitial ad");
                    CBBinding.showInterstitial (null);
                } else {
                    // Switch between RevMob and Tap for Tap fullscreeners
                    if (!showTapForTapFullscreen) {
                        // RevMob intersitial
                        Debug.Log ("[ads]Showing RevMob fullscreen ad");
                        if (revmob != null)
                            revmob.ShowFullscreen ();
                    } else {
                        // Tap for Tap intersitial
                        Debug.Log ("[ads]Showing Tap for Tap fullscreen ad");
                        TapForTap.ShowInterstitial ();
                    }
                    showTapForTapFullscreen = !showTapForTapFullscreen;
                }
                showChartboostIntersitial = !showChartboostIntersitial; // alternate between RevMob and Chartboost
            }
        }
    }
    
    // Just more advertising of games
    public void showMoreGames ()
    {
#if UNITY_EDITOR
        GameState.Instance.currentBalance = 100000;
        GameState.Instance.updateGUIText();
        return;
#endif
        Debug.Log ("[ads]Showing more games");

        if (Utils.isJapanese ()) {
            Debug.Log ("[ads]Showing AppC web view");
            appCCloud.Ad.OpenAdView();
        } else {
            // Default to English
            Debug.Log ("[ads]Showing Tap for Tap App Wall");
            TapForTap.ShowAppWall ();
        }
    }

    void OnOrientationChange ()
    {
        isCurrentlyPortrait = GUIControls.isPortraitNoConditions;
        isCurrentlyLandscape = !GUIControls.isPortraitNoConditions;

        if (!isCurrentlyPortrait && showAppCAds && Utils.isJapanese ()) {
            // Make sure we're displaying marquee instead of move icons view for AppC, or if showing Tap for Tap override that because it's too big
            Debug.Log ("[ads]Forcing AppC MoveIcons ads to Marquee");
            appCCloud.Ad.HideAllViews ();
            appCCloud.Ad.ShowMarqueeView (AppCCloud.Vertical.Bottom);
        } else if (isCurrentlyPortrait && showAppCAds && Utils.isJapanese ()) {
            // Make sure we're displaying move icons instead of marquee view for AppC when in portrait
            Debug.Log ("[ads]Forcing AppC Marquee ads to MoveIcons");
            appCCloud.Ad.HideAllViews ();
            appCCloud.Ad.ShowMoveIconView (AppCCloud.Horizon.Center, AppCCloud.Vertical.Bottom);
        }

        if (isCurrentlyShowingTapForTapAdView) {
            Debug.Log ("[ads]Readjusting Tap for Tap banner due to orientation change");
            TapForTap.RemoveAdView();
            TapForTap.CreateAdView (TapForTapVerticalAlignment.BOTTOM, TapForTapHorizontalAlignment.CENTER);
        } else if (!Utils.isJapanese() && !isCurrentlyShowingTapForTapAdView) {
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log ("[ads]Readjusting Android RevMob banner due to orientation change");
            if (revmob != null && revmobBanner != null) {
                revmobBanner.Hide();
                revmobBanner.Release();
                revmobBanner = null;
                revMobBannerShowing = false;
            }
            if (revmob != null) {
                currentBannerAdWidth = BANNER_AD_WIDTH;
                currentBannerAdHeight = BANNER_AD_HEIGHT;
                if (isCurrentlyPortrait) {
                    Debug.Log ("[ads]Showing Android RevMob banner portrait");
                    revmobBanner = revmob.CreateBanner(RevMob.Position.BOTTOM);//, Screen.width/3, 1, currentBannerAdWidth, currentBannerAdHeight);
                } else {
                    Debug.Log ("[ads]Showing Android RevMob banner landscape");
                    revmobBanner = revmob.CreateBanner(RevMob.Position.BOTTOM, Screen.width/3 + currentBannerAdWidth/4, 1, currentBannerAdWidth, currentBannerAdHeight);
                }
            }
#elif UNITY_IPHONE && !UNITY_EDITOR
            Debug.Log ("[ads]Readjusting iOS RevMob banner due to orientation change");
            if (revmobBanner != null) {
                revmobBanner.Hide();
                revmobBanner.Release();
                revmobBanner = null;
                revMobBannerShowing = false;
            }
            if (revmob != null) {
                currentBannerAdWidth = BANNER_AD_WIDTH;
                currentBannerAdHeight = BANNER_AD_HEIGHT;
                if (isCurrentlyPortrait) {
                    Debug.Log ("[ads]Showing iOS RevMob banner portrait");
                    revmobBanner = revmob.CreateBanner();
                } else {
                    Debug.Log ("[ads]Showing iOS RevMob banner landscape");
                    revmobBanner = revmob.CreateBanner((Screen.width/4)-150, (Screen.height/2)-70, 300, 45, null, null);
                }
            }
#endif
            Debug.Log ("[ads]RevMob banner debug: Screen.width == " + Screen.width + ", Screen.height == " + Screen.height);
            revmobBannerShow ();
        }
    }

    void revmobBannerShow() {
        StartCoroutine(revmobBannerShowCoroutine());
    }

    IEnumerator revmobBannerShowCoroutine() {
        yield return new WaitForSeconds(0.5f);
        if (revmobBanner != null)
            revmobBanner.Show ();
    }

    void revmobBannerHide() {
#if UNITY_IPHONE
        revmobBanner.Hide();
        //revmobBanner.Release();
        revmobBanner = null;
        revMobBannerShowing = false;
#elif UNITY_ANDROID
        revmobBanner.Hide();
        revmobBanner.Release();
        revmobBanner = null;
        revMobBannerShowing = false;
#endif
    }
        
    IEnumerator SetUserLocationCoroutine ()
    {
        // We set the user's location for better ad results.
        // Tap for Tap supports this. We don't want to ask for GPS permission in the released app coz that'll
        // turn away some users so instead we can use ipinfodb.com's IP-geolocation info. We do a simple
        // HTTP query and get the lat/long.
        Debug.Log ("[ads]Getting location for ads by IP");
        string url = "http://api.ipinfodb.com/v3/ip-city/?key=7bf137a4e34caf7c43a5101f744ed252ba081073d18c5953ab461b6030a913f0";
        // Sample reply is OK;;221.184.82.107;JP;JAPAN;TOKYO;TOKYO;214-002;35.6149;139.581;+09:00
        WWW www = new WWW (url);
        yield return www;

        // check for errors
        if (www.error == null) {
            Debug.Log ("[ads] IP geolocation response!: " + www.text);
            if (www.data != null && www.text.StartsWith ("OK")) {
                string lat = null, lon = null;
                char[] charArray = www.data.ToCharArray ();
                int colonCount = 0;
                for (int i = 0; i < charArray.Length; i++) {
                    if (charArray [i] == ';')
                        colonCount++;

                    // Latitude starts at the 7th colon
                    if (colonCount == 8) {
                        string substrFrom7thColon = www.text.Substring (i+1);
                        int substrFrom7thColonNextColonIdx = substrFrom7thColon.IndexOf (';');
                        if (substrFrom7thColon != null && substrFrom7thColonNextColonIdx >= 1) {
                            // Extract latitude
                            lat = substrFrom7thColon.Substring (0, substrFrom7thColonNextColonIdx);
                            Debug.Log ("[ads] >>> extracted latitude: " + lat);
                        }
                        string substrFrom8thColon = substrFrom7thColon.Substring (substrFrom7thColonNextColonIdx + 1);
                        int substrFrom8thColonNextColonIdx = substrFrom8thColon.IndexOf (';');
                        if (substrFrom8thColon != null && substrFrom8thColonNextColonIdx >= 1) {
                            // Extract longitude
                            lon = substrFrom8thColon.Substring (0, substrFrom8thColonNextColonIdx-1);
                            Debug.Log ("[ads] >>> extracted longitude: " + lon);
                        }
                        break;
                    }
                }

                // Set ad networks locations
                if (lat != null && lon != null) {
                    double latD = 0, lonD = 0;
                    try {
                        latD = double.Parse (lat);
                        lonD = double.Parse (lon);
                        Debug.Log ("[ads]Setting Tap for Tap coordinates: (" + lat + "," + lon + ") -> (" + latD + "," + lonD + ")");
                        TapForTap.SetLocation (latD, lonD);
                    } catch (Exception e) {
                        Debug.LogWarning ("[ads]Invalid coordinates: (" + lat + "," + lon + ") -> (" + latD + "," + lonD + ")");
                        Debug.LogException(e);
                    }
                } else {
                    Debug.LogWarning ("[ads]Error calculating coordinates");
                }
            } else {
                Debug.Log ("[ads] WWW error getting IP location: " + www.error);
            }
        }
    }

    
    void Awake()
    {
        Debug.Log("[ads]Tapjoy: Awaking and adding Tapjoy Events");
        // Tapjoy Connect Events
        TapjoyPlugin.connectCallSucceeded += HandleTapjoyConnectSuccess;
        TapjoyPlugin.connectCallFailed += HandleTapjoyConnectFailed;
        
        // Tapjoy Virtual Currency Events
        TapjoyPlugin.getTapPointsSucceeded += HandleGetTapPointsSucceeded;
        TapjoyPlugin.getTapPointsFailed += HandleGetTapPointsFailed;
        TapjoyPlugin.spendTapPointsSucceeded += HandleSpendTapPointsSucceeded;
        TapjoyPlugin.spendTapPointsFailed += HandleSpendTapPointsFailed;
        TapjoyPlugin.awardTapPointsSucceeded += HandleAwardTapPointsSucceeded;
        TapjoyPlugin.awardTapPointsFailed += HandleAwardTapPointsFailed;
        TapjoyPlugin.tapPointsEarned += HandleTapPointsEarned;
        
        // Tapjoy Full Screen Ad Events
        TapjoyPlugin.getFullScreenAdSucceeded += HandleGetFullScreenAdSucceeded;
        TapjoyPlugin.getFullScreenAdFailed += HandleGetFullScreenAdFailed;
        
        // Tapjoy Display Ad Events
        TapjoyPlugin.getDisplayAdSucceeded += HandleGetDisplayAdSucceeded;
        TapjoyPlugin.getDisplayAdFailed += HandleGetDisplayAdFailed;
        
        // Tapjoy Video Ad Events
        TapjoyPlugin.videoAdStarted += HandleVideoAdStarted;
        TapjoyPlugin.videoAdFailed += HandleVideoAdFailed;
        TapjoyPlugin.videoAdCompleted += HandleVideoAdCompleted;
        
        // Tapjoy Ad View Closed Events
        TapjoyPlugin.viewOpened += HandleViewOpened;
        TapjoyPlugin.viewClosed += HandleViewClosed;
        
        // Tapjoy Show Offers Events
        TapjoyPlugin.showOffersFailed += HandleShowOffersFailed;
    }

    void OnEnable ()
    {
        EnableChartboostListeners ();
    }

    void OnDisable ()
    {
        DisableChartboostListeners ();

        Debug.Log("[ads]Tapjoy: Disabling and removing Tapjoy Events");
        // Tapjoy Connect Events
        TapjoyPlugin.connectCallSucceeded -= HandleTapjoyConnectSuccess;
        TapjoyPlugin.connectCallFailed -= HandleTapjoyConnectFailed;
        
        // Tapjoy Virtual Currency Events
        TapjoyPlugin.getTapPointsSucceeded -= HandleGetTapPointsSucceeded;
        TapjoyPlugin.getTapPointsFailed -= HandleGetTapPointsFailed;
        TapjoyPlugin.spendTapPointsSucceeded -= HandleSpendTapPointsSucceeded;
        TapjoyPlugin.spendTapPointsFailed -= HandleSpendTapPointsFailed;
        TapjoyPlugin.awardTapPointsSucceeded -= HandleAwardTapPointsSucceeded;
        TapjoyPlugin.awardTapPointsFailed -= HandleAwardTapPointsFailed;
        TapjoyPlugin.tapPointsEarned -= HandleTapPointsEarned;
        
        // Tapjoy Full Screen Ad Events
        TapjoyPlugin.getFullScreenAdSucceeded -= HandleGetFullScreenAdSucceeded;
        TapjoyPlugin.getFullScreenAdFailed -= HandleGetFullScreenAdFailed;
        
        // Tapjoy Display Ad Events
        TapjoyPlugin.getDisplayAdSucceeded -= HandleGetDisplayAdSucceeded;
        TapjoyPlugin.getDisplayAdFailed -= HandleGetDisplayAdFailed;
        
        // Tapjoy Video Ad Events
        TapjoyPlugin.videoAdStarted -= HandleVideoAdStarted;
        TapjoyPlugin.videoAdFailed -= HandleVideoAdFailed;
        TapjoyPlugin.videoAdCompleted -= HandleVideoAdCompleted;
        
        // Tapjoy Ad View Closed Events
        TapjoyPlugin.viewOpened -= HandleViewOpened;
        TapjoyPlugin.viewClosed -= HandleViewClosed;
        
        // Tapjoy Show Offers Events
        TapjoyPlugin.showOffersFailed -= HandleShowOffersFailed;
    }

    #region IRevMobListener implementation

    public void AdDidReceive (string revMobAdType)
    {
        Debug.Log ("[ads]RevMob Ad did receive: " + revMobAdType);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_AD_RECEIVED, new string[] { revMobAdType }, false);
    }

    public void AdDidFail (string revMobAdType)
    {
        Debug.Log ("[ads]RevMob Ad did fail: " + revMobAdType);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_AD_FAILED, new string[] { revMobAdType }, false);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_REVMOB_AD_FAILED, revMobAdType, "AdsManager");
    }

    public void AdDisplayed (string revMobAdType)
    {
        if (revMobAdType != null && revMobAdType == "Banner")
            revMobBannerShowing = true;

        Debug.Log ("[ads]RevMob Ad displayed: " + revMobAdType);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_AD_DISPLAYED, new string[] { revMobAdType }, false);
    }

    public void UserClickedInTheAd (string revMobAdType)
    {
        Debug.Log ("[ads]RevMob Ad clicked: " + revMobAdType);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_AD_CLICKED, new string[] { revMobAdType }, false);
    }

    public void UserClosedTheAd (string revMobAdType)
    {
        if (revMobAdType == "Banner")
            revMobBannerShowing = false;

        Debug.Log ("[ads]RevMob Ad closed:  " + revMobAdType);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_AD_CLOSED, new string[] { revMobAdType }, false);
    }

    public void InstallDidReceive (string s)
    {
        Debug.Log ("[ads]RevMob Install did receive: " + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_INSTALL_RECEIVED, new string[] { s }, false);
    }

    public void InstallDidFail (string s)
    {
        Debug.Log ("[ads]RevMob Install did fail: " + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_REVMOB_INSTALL_FAILED, new string[] { s }, false);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_REVMOB_INSTALL_FAILED, s, "AdsManager");
    }
    #endregion

    #region Chartboost listener implementation

    void EnableChartboostListeners ()
    {
        Debug.Log("[ads]Chartboost: Adding and enabling Chartboost Events");

        // Listen to some interstitial-related events
        CBManager.didFailToLoadInterstitialEvent += didFailToLoadInterstitialEvent;
        CBManager.didDismissInterstitialEvent += didDismissInterstitialEvent;
        CBManager.didCloseInterstitialEvent += didCloseInterstitialEvent;
        CBManager.didClickInterstitialEvent += didClickInterstitialEvent;
        CBManager.didCacheInterstitialEvent += didCacheInterstitialEvent;
        CBManager.didShowInterstitialEvent += didShowInterstitialEvent;
        CBManager.didFailToLoadMoreAppsEvent += didFailToLoadMoreAppsEvent;
        CBManager.didDismissMoreAppsEvent += didDismissMoreAppsEvent;
        CBManager.didCloseMoreAppsEvent += didCloseMoreAppsEvent;
        CBManager.didClickMoreAppsEvent += didClickMoreAppsEvent;
        CBManager.didCacheMoreAppsEvent += didCacheMoreAppsEvent;
        CBManager.didShowMoreAppsEvent += didShowMoreAppsEvent;
    }

    void DisableChartboostListeners ()
    {
        Debug.Log("[ads]Chartboost: Disabling and removing Chartboost Events");

        // Remove event handlers
        CBManager.didFailToLoadInterstitialEvent -= didFailToLoadInterstitialEvent;
        CBManager.didDismissInterstitialEvent -= didDismissInterstitialEvent;
        CBManager.didCloseInterstitialEvent -= didCloseInterstitialEvent;
        CBManager.didClickInterstitialEvent -= didClickInterstitialEvent;
        CBManager.didCacheInterstitialEvent -= didCacheInterstitialEvent;
        CBManager.didShowInterstitialEvent -= didShowInterstitialEvent;
        CBManager.didFailToLoadMoreAppsEvent -= didFailToLoadMoreAppsEvent;
        CBManager.didDismissMoreAppsEvent -= didDismissMoreAppsEvent;
        CBManager.didCloseMoreAppsEvent -= didCloseMoreAppsEvent;
        CBManager.didClickMoreAppsEvent -= didClickMoreAppsEvent;
        CBManager.didCacheMoreAppsEvent -= didCacheMoreAppsEvent;
        CBManager.didShowMoreAppsEvent -= didShowMoreAppsEvent;
    }

    public void didFailToLoadInterstitialEvent (string s)
    {
        Debug.Log ("[ads]Chartboost didFailToLoadInterstitialEvent:" + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_FAIL_LOAD_INTERSITIAL_EVENT, new string[] { s }, false);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_CHARTBOOST_FAIL_LOAD_INTERSITIAL_EVENT, s, "AdsManager");
    }

    public void didDismissInterstitialEvent (string s)
    {
        Debug.Log ("[ads]Chartboost didDismissInterstitialEvent:" + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_DISMISS_INTERSITIAL_EVENT, new string[] { s }, false);
    }

    public void didCloseInterstitialEvent (string s)
    {
        Debug.Log ("[ads]Chartboost didCloseInterstitialEvent:" + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_CLOSE_INTERSITIAL_EVENT, new string[] { s }, false);
    }

    public void didClickInterstitialEvent (string s)
    {
        Debug.Log ("[ads]Chartboost didClickInterstitialEvent:" + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_CLICK_INTERSITIAL_EVENT, new string[] { s }, false);
    }

    public void didCacheInterstitialEvent (string s)
    {
        Debug.Log ("[ads]Chartboost didCacheInterstitialEvent:" + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_CACHE_INTERSITIAL_EVENT, new string[] { s }, false);
    }

    public void didShowInterstitialEvent (string s)
    {
        Debug.Log ("[ads]Chartboost didShowInterstitialEvent:" + s);
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_SHOW_INTERSITIAL_EVENT, new string[] { s }, false);
    }

    public void didFailToLoadMoreAppsEvent ()
    {
        Debug.Log ("[ads]Chartboost didFailToLoadMoreAppsEvent");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_FAIL_LOAD_MORE_APPS_EVENT);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_CHARTBOOST_FAIL_LOAD_MORE_APPS_EVENT, "", "AdsManager");
    }

    public void didDismissMoreAppsEvent ()
    {
        Debug.Log ("[ads]Chartboost didDismissMoreAppsEvent");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_DISMISS_MORE_APPS_EVENT);
    }

    public void didCloseMoreAppsEvent ()
    {
        Debug.Log ("[ads]Chartboost didCloseMoreAppsEvent");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_CLOSE_MORE_APPS_EVENT);
    }

    public void didClickMoreAppsEvent ()
    {
        Debug.Log ("[ads]Chartboost didClickMoreAppsEvent");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_CLICK_MORE_APPS_EVENT);
    }

    public void didCacheMoreAppsEvent ()
    {
        Debug.Log ("[ads]Chartboost didCacheMoreAppsEvent");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_CACHE_MORE_APPS_EVENT);
    }

    public void didShowMoreAppsEvent ()
    {
        Debug.Log ("[ads]Chartboost didShowMoreAppsEvent");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_CHARTBOOST_SHOW_MORE_APPS_EVENT);
    }

    #endregion

    #region Tapjoy Callback Methods (These must be implemented in your own c# script file.)
    
    // CONNECT
    public void HandleTapjoyConnectSuccess()
    {
        Debug.Log("[ads]Tapjoy: HandleTapjoyConnectSuccess");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_CONNECT_SUCCESS);
    }
    
    public void HandleTapjoyConnectFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleTapjoyConnectFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_CONNECT_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_CONNECT_FAILED, "", "AdsManager");
    }
    
    // VIRTUAL CURRENCY
    void HandleGetTapPointsSucceeded(int points)
    {
        Debug.Log("[ads]Tapjoy: HandleGetTapPointsSucceeded: " + points);
        //tapPointsLabel = "Total TapPoints: " + TapjoyPlugin.QueryTapPoints();

        if (points != 0) {
            // Update balance
            GameState.Instance.currentBalance += points;
            GameState.Instance.updateGUIText();
            GameState.Instance.guiControls.displayMessage("+$" + points + " BONUS!");

            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_GET_TAP_POINTS_SUCCEEDED, new string[] { points+"" }, false);

            TapjoyPlugin.ShowDefaultEarnedCurrencyAlert();
            TapjoyPlugin.SpendTapPoints(points);
        }
    }
    
    public void HandleGetTapPointsFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleGetTapPointsFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_GET_TAP_POINTS_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_GET_TAP_POINTS_FAILED, "", "AdsManager");
    }
    
    public void HandleSpendTapPointsSucceeded(int points)
    {
        Debug.Log("[ads]Tapjoy: HandleSpendTapPointsSucceeded: " + points);
        //tapPointsLabel = "Total TapPoints: " + TapjoyPlugin.QueryTapPoints();
        if (points != 0) {
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_SPEND_TAP_POINTS_SUCCEEDED, new string[] { points+"" }, false);
        }
    }
    
    public void HandleSpendTapPointsFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleSpendTapPointsFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_SPEND_TAP_POINTS_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_SPEND_TAP_POINTS_FAILED, "", "AdsManager");
    }
    
    public void HandleAwardTapPointsSucceeded()
    {
        Debug.Log("[ads]Tapjoy: HandleAwardTapPointsSucceeded");
        //tapPointsLabel = "Total TapPoints: " + TapjoyPlugin.QueryTapPoints();

        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_AWARD_TAP_POINTS_SUCCEEDED);
    }
    
    public void HandleAwardTapPointsFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleAwardTapPointsFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_AWARD_TAP_POINTS_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_AWARD_TAP_POINTS_FAILED, "", "AdsManager");
    }
    
    public void HandleTapPointsEarned(int points)
    {
        Debug.Log("[ads]Tapjoy: CurrencyEarned: " + points);
        //tapPointsLabel = "Currency Earned: " + points;

//        if (points != 0) {
//            // Update balance
//            GameState.Instance.currentBalance += points;
//            GameState.Instance.updateGUIText();
//            GameState.Instance.guiControls.displayMessage("+$" + points + " BONUS!");
//
//            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_CURRENCY_EARNED, new string[] { points+"" }, false);
//            
//            TapjoyPlugin.ShowDefaultEarnedCurrencyAlert();
//            TapjoyPlugin.SpendTapPoints(points);
//        }
    }
    
    // FULL SCREEN ADS
    public void HandleGetFullScreenAdSucceeded()
    {
        Debug.Log("[ads]Tapjoy: HandleGetFullScreenAdSucceeded");

        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_GET_FULLSCREEN_AD_SUCCEEDED);
        
        TapjoyPlugin.ShowFullScreenAd();
    }
    
    public void HandleGetFullScreenAdFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleGetFullScreenAdFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_GET_FULLSCREEN_AD_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_GET_FULLSCREEN_AD_FAILED, "", "AdsManager");
    }
    
    // DISPLAY ADS
    public void HandleGetDisplayAdSucceeded()
    {
        Debug.Log("[ads]Tapjoy: HandleGetDisplayAdSucceeded");

        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_GET_DISPLAY_AD_SUCCEEDED);
        
        //if (!openingFullScreenAd)
            TapjoyPlugin.ShowDisplayAd();
    }
    
    public void HandleGetDisplayAdFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleGetDisplayAdFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_GET_DISPLAY_AD_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_GET_DISPLAY_AD_FAILED, "", "AdsManager");
    }
    
    // VIDEO
    public void HandleVideoAdStarted()
    {
        Debug.Log("[ads]Tapjoy: HandleVideoAdStarted");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_HANDLE_VIDEO_AD_STARTED);
    }
    
    public void HandleVideoAdFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleVideoAdFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_HANDLE_VIDEO_AD_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_HANDLE_VIDEO_AD_FAILED, "", "AdsManager");
    }
    
    public void HandleVideoAdCompleted()
    {
        Debug.Log("[ads]Tapjoy: HandleVideoAdCompleted");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_HANDLE_VIDEO_AD_COMPLETED);

        //Debug.Log ("Getting TapJoy points");
        //TapjoyPlugin.GetTapPoints();
    }
    
    // VIEW OPENED  
    public void HandleViewOpened(TapjoyViewType viewType)
    {
        Debug.Log("[ads]Tapjoy: HandleViewOpened of view type " + viewType.ToString());
        //openingFullScreenAd = true;
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_HANDLE_VIEW_TYPE_OPENED, new string[] { viewType+"" }, false);
    }
    
    // VIEW CLOSED  
    public void HandleViewClosed(TapjoyViewType viewType)
    {
        Debug.Log("[ads]Tapjoy: HandleViewClosed of view type " + viewType.ToString());
        //openingFullScreenAd = false;
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_HANDLE_VIEW_TYPE_CLOSED, new string[] { viewType+"" }, false);
       
#if UNITY_IPHONE
        if (viewType != null && viewType == TapjoyViewType.OFFERWALL) {
            Debug.Log ("Getting TapJoy points");
            TapjoyPlugin.GetTapPoints();
        }
#endif
    }
    
    // OFFERS
    public void HandleShowOffersFailed()
    {
        Debug.Log("[ads]Tapjoy: HandleShowOffersFailed");
        FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPJOY_HANDLE_SHOW_OFFERS_FAILED);
        //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPJOY_HANDLE_SHOW_OFFERS_FAILED, "", "AdsManager");
    }
    
    #endregion

    #region TapForTap event listener interfaces

    class TapForTapAdViewListener : ITapForTapAdView
    {
        public void OnTapAd()
        {
            Debug.Log("[ads]TapforTap: Called ad OnTapAd");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_AD_TAPPED);
        }

        public void OnReceiveAd()
        {
            Debug.Log("[ads]TapforTap: Called ad OnReceiveAd");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_AD_RECEIVED);
        }

        public void OnFailToReceiveAd(string reason)
        {
            Debug.Log("[ads]TapforTap: Called ad OnFailToReceiveAd because of " + reason);
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_AD_RECEIVE_FAILED, new string[] { reason }, false);
            //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPFORTAP_AD_RECEIVE_FAILED, reason, "AdsManager");
        }
    }

    class TapForTapAppWallListener : ITapForTapAppWall
    {
        public void OnDismiss()
        {
            Debug.Log("[ads]TapforTap: Called app wall listener OnDismiss");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_APP_WALL_DISMISSED);
        }

        public void OnReceive()
        {
            Debug.Log("[ads]TapforTap: Called app wall listener OnReceive");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_APP_WALL_RECEIVED);
        }

        public void OnShow()
        {
            Debug.Log("[ads]TapforTap: Called app wall listener OnShow");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_APP_WALL_SHOWED);
        }

        public void OnTap()
        {
            Debug.Log("[ads]TapforTap: Called app wall listener OnTap");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_APP_WALL_TAPPED);
        }


        public void OnFail(string s)
        {
            Debug.Log("[ads]TapforTap: Called app wall listener OnFail because: " + s);
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_APP_WALL_FAILED, new string[] { s }, false);
            //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPFORTAP_APP_WALL_FAILED, s, "AdsManager");
        }

    }
    
    class TapForTapInterstitialListener : ITapForTapInterstitial
    {
        public void OnDismiss()
        {
            Debug.Log("[ads]TapforTap: Called interstitial listener OnDismiss: ");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_INTERSITIAL_DISMISSED);
        }

        public void OnReceive()
        {
            Debug.Log("[ads]TapforTap: Called interstitial listener OnReceive");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_INTERSITIAL_RECEIVED);
        }
        
        public void OnShow()
        {
            Debug.Log("[ads]TapforTap: Called interstitial listener OnShow");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_INTERSITIAL_SHOWED);
        }
        
        public void OnTap()
        {
            Debug.Log("[ads]TapforTap: Called interstitial listener OnTap");
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_INTERSITIAL_TAPPED);
        }
        
        
        public void OnFail(string s)
        {
            Debug.Log("[ads]TapforTap: Called interstitial listener OnFail because: " + s);
            FlurryAnalytics.Instance().LogEvent(Consts.FE_ADS_TAPFORTAP_INTERSITIAL_FAILED, new string[] { s }, false);
            //FlurryAnalytics.Instance ().LogError(Consts.FE_ADS_TAPFORTAP_INTERSITIAL_FAILED, s, "AdsManager");
        }
    }

    #endregion

}


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

/**
 * Do game startup stuff like check user preferences (settings) and take action where needed.
 */
public class Startup : MonoBehaviour
{
    public GameObject casino;
    public GameObject dealers;
    public GameObject bgm;

#if UNITY_ANDROID
    private static string BGM_ASSET_BUNDLE_URL = "https://sites.google.com/site/arohacorp/baccarat3d/b3dbgmandroid.unity";
#elif UNITY_IPHONE
    private static string BGM_ASSET_BUNDLE_URL = "https://sites.google.com/site/arohacorp/baccarat3d/b3dbgmios.unity";
#else
    private static string BGM_ASSET_BUNDLE_URL = "https://sites.google.com/site/arohacorp/baccarat3d/b3dbgmpc.unity";
#endif
    // Use this for initialization
    void Start ()
    {
#if !UNITY_WEBPLAYER
        Debug.Log ("Starting up BaccARat 3D!");
#else
        Debug.Log ("Starting up BaccARat 3D WEB PLAYER VERSION!!");
#endif

        // Hide or show the casino room as a background
        ToggleCasinoRoom (Utils.isPreferenceEnabled ("pref_key_graphics_settings_show_casino_room", true));

        // Hide or show the dealers  
        ToggleDealers (Utils.isPreferenceEnabled ("pref_key_graphics_settings_show_dealer", true));

        // Use this as the start trigger to initialize the game state manager
        GameState.Instance.startState ();

		// Download BGM
		StartCoroutine (DownloadBGMAssetBundle ());

        // Initialize Google Play Games
        Debug.Log ("Initializing Google Play Games service (in the cloud)");
//        PlayGamesPlatform.DebugLogEnabled = true;
//        PlayGamesPlatform.Activate();
        if (PlayerPrefs.HasKey("gpg")) {
            if (Application.internetReachability != NetworkReachability.NotReachable) // app wouldn't start if offline
                GameState.Instance.gpgCloudManager.login();
        }

        // Populate AR Treasure Hunt markers list with names of the targets
        // TODO: consider implementing CloudReco
        GameState.Instance.arTreasureHuntMarkers.Add ("SevenStars", new GameState.ARTreasureHuntMarker("SevenStars", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("mevius_lights", new GameState.ARTreasureHuntMarker("mevius_lights", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("mild_sevens", new GameState.ARTreasureHuntMarker("mild_sevens", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("coke", new GameState.ARTreasureHuntMarker("coke", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("malboro_methols", new GameState.ARTreasureHuntMarker("malboro_methols", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("kirin_ichiban_shibori", new GameState.ARTreasureHuntMarker("kirin_ichiban_shibori", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("asahi_super_dry", new GameState.ARTreasureHuntMarker("asahi_super_dry", 0, Dealer.AR_TREASURE_HUNT_BONUS1));
        GameState.Instance.arTreasureHuntMarkers.Add ("prada", new GameState.ARTreasureHuntMarker("prada", 0, Dealer.AR_TREASURE_HUNT_BONUS2));

        // TODO: for testing purposes only!
//        List<FacebookUserInfo> dummys = new List<FacebookUserInfo>();
//        Debug.LogError("POPULATING TEST FACEBOOK USERS! Don't forget to remove me!");
//        FacebookUserInfo item = new FacebookUserInfo();item.id = "100001060023177"; item.userName = "laree.parkinson.9"; item.name = "Lauree Parkinson";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001453204570"; item.userName = "chinatsu.morokoshi"; item.name = "Chinatsu Morokoshi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001487988722"; item.userName = "haruna.morokoshi"; item.name = "Haruna Morokoshi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001513161594"; item.userName = "tomomi.tarushima"; item.name = "Tomomi Tarushima";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001758286472"; item.userName = "mika.shimada.946"; item.name = "Mika Shimada";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001788555308"; item.userName = "miyoko.mccorkindale"; item.name = "Miyoko McCorkindale";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001968546936"; item.userName = "mika.takahashi.1291"; item.name = "Mika Takahashi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001990867457"; item.userName = "hatsuki.izuma"; item.name = "Hatsuki Izuma";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100002285449122"; item.userName = "nanayo.kai"; item.name = "Nanayo Kai";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100002531237737"; item.userName = "itsuka.ogi"; item.name = "Itsuka Ogi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "10000145322204570"; item.userName = "chinat312su.morokoshi"; item.name = "Chinatsu 321312Morokoshi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001487988722"; item.userName = "haruna.morokoshi"; item.name = "Haruna M3213213orokoshi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "1000015131613242594"; item.userName = "tomomi.312312tarushima"; item.name = "Tomo321312mi Tarushima";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001758286432472"; item.userName = "mika.shi2312mada.946"; item.name = "Mika Sh312312imada";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100001788555543308"; item.userName = "miyoko.m31231ccorkindale"; item.name = "Mi12yoko 321312McCorkindale";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "1000019685463223936"; item.userName = "mika.ta312kahashi.1291"; item.name = "Mik3123123a Takahashi";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "10000199086742342357"; item.userName = "hatsuk312312i.izuma"; item.name = "Hatsu123123ki Izuma";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100002231231285449122"; item.userName = "nanayo31231.kai"; item.name = "Nanayo321312 Kai";dummys.Add (item);
//        item = new FacebookUserInfo();item.id = "100002533121237737"; item.userName = "itsuka.ogi"; item.name = "Itsuk312312a Ogi";dummys.Add (item);
        //        GameState.Instance.setFacebookFriendsList(dummys);            Debug.Log ("[ARTreasureHunt] Found special AR target " + tsr.TargetName + ". Bonus time!");


        // TODO: for testing purposes only!
//        List<GameState.Baccarat3DFacebookScoreInfo> dummys2 = new List<GameState.Baccarat3DFacebookScoreInfo>();
//        Debug.LogError("POPULATING TEST FACEBOOK SCORES! Don't forget to remove me!");
//        FacebookScoreInfo item2 = new FacebookScoreInfo();item2.userId = "100004652461530"; item2.userName = "Ewan Hinata Nishi McCorkindale"; item2.score = 19990;GameObject profilePicture = new GameObject();/*profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg"*/;dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        item2 = new FacebookScoreInfo();item2.userId = "43423"; item2.userName = "Tadahiro Yoshii1"; item2.score = 6876;profilePicture = new GameObject();profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg";dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        item2 = new FacebookScoreInfo();item2.userId = "2312"; item2.userName = "Tadahiro Yoshii2"; item2.score = 23123;profilePicture = new GameObject();profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg";dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        item2 = new FacebookScoreInfo();item2.userId = "334243"; item2.userName = "Tadahiro Yoshii3"; item2.score = 444444;profilePicture = new GameObject();profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg";dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        item2 = new FacebookScoreInfo();item2.userId = "4534534"; item2.userName = "Tadahiro Yoshii4"; item2.score = 555556876;profilePicture = new GameObject();profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg";dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        item2 = new FacebookScoreInfo();item2.userId = "32434"; item2.userName = "Tadahiro Yoshii5"; item2.score = 687226;profilePicture = new GameObject();profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg";dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        item2 = new FacebookScoreInfo();item2.userId = "4444"; item2.userName = "Tadahiro Yoshii6"; item2.score = 11111;profilePicture = new GameObject();profilePicture.AddComponent<ProfilePicture>();profilePicture.GetComponent<ProfilePicture>().url = "https://fbcdn-profile-a.akamaihd.net/hprofile-ak-ash4/372715_100004652461530_334827154_q.jpg";dummys2.Add (new GameState.Baccarat3DFacebookScoreInfo(item2, profilePicture));
//        GameState.Instance.facebookScoresList = dummys2;
//        Debug.Log ("Baccarat3DFacebookScoreInfo list size: " + dummys2.Count);
    }

    void ToggleCasinoRoom (bool enable)
    {
        // Hide or show the casino room as a background
        if (enable) {
            Debug.Log ("Toggling casino room display on");
            casino.SetActive (true);
        } else {
            Debug.Log ("Toggling casino room display off");
            casino.SetActive (false);
        }
    }
 
    void ToggleDealers (bool enable)
    {
        // Hide or show the dealer figure        
        if (enable) {
            Debug.Log ("Toggling dealers display on");
            dealers.SetActive (true);
        } else {
            Debug.Log ("Toggling dealers display off");
            dealers.SetActive (false);
        }
    }

    IEnumerator DownloadBGMAssetBundle ()
    {
        Debug.Log ("Downloading/check cache for BGM AssetBundle from: " + BGM_ASSET_BUNDLE_URL);

        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;

        // Load the AssetBundle file from Cache if it exists with the same version or download and store it in the cache
        using (WWW www = WWW.LoadFromCacheOrDownload (BGM_ASSET_BUNDLE_URL, 1)) {
            yield return www;
            if (www.error != null) {
                Debug.LogError ("BGM download error: " + www.error);
            } else if (www.assetBundle == null) {
                Debug.LogError ("BGM download error: www.assetBundle is null!");
            } else {
                Debug.Log ("Instantiating bundle");
                AssetBundle bundle = www.assetBundle;
                if (bundle == null) {
                    Debug.LogError ("BGM download error: bundle is null!");
                } else {
                    Debug.Log ("BGM downloaded/loaded from cache");

                    // Load the object asynchronously
                    Debug.Log ("Loading BaccARat3D_Song1 asynchronously");
                    AssetBundleRequest request = bundle.LoadAssetAsync ("BaccARat3D_Song1", typeof(AudioClip));
                    
                    // Wait for completion
                    yield return request;

                    if (bundle != null) {
                        // Get the reference to the loaded object and instantiate it
                        Debug.Log ("Instantiate (request.asset as AudioClip);");
                        Instantiate (request.asset as AudioClip);
                        Debug.Log ("Instantiate (request.asset as AudioClip); done");

                        // Attach the AudioClip to the BGM GameObject's AudioSource
                        AudioSource audioSource = bgm.GetComponent<AudioSource>();
                        audioSource.clip = (AudioClip) request.asset;
                        if (PlayerPrefs.GetInt("BGM", 1) == 1)
                            audioSource.Play();
                        GameState.Instance.bgm = bgm;
                        bgm = null;

                        // Unload the AssetBundles compressed contents to conserve memory
                        bundle.Unload (false);
                    } else {
                        Debug.LogWarning("Unable to request BGM AssetBundle... request is null");
                    }
                }
                // Frees the memory from the web stream
                www.Dispose ();
            }
        }
    }

	private void OnHideUnity(bool isGameShown)                                                   
	{                                                                                            
		Debug.Log("OnHideUnity");                                                              
		if (!isGameShown)                                                                        
		{                                                                                        
			// pause the game - we will need to hide                                             
			Time.timeScale = 0;                                                                  
		}                                                                                        
		else                                                                                     
		{                                                                                        
			// start the game back up - we're getting focus again                                
			Time.timeScale = 1;                                                                  
		}                                                                                        
	}
}
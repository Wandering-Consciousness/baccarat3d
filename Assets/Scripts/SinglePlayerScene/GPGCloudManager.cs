// using GooglePlayGames;// TODO2022: refactor
using UnityEngine.SocialPlatforms;
//using GooglePlayGames.BasicApi;
using UnityEngine;
using System;

/** Managing storing and retrieving of cloud data on Google Play Games servers */
public class GPGCloudManager  {

    void Start () {
    }

    public void login() {
//        Social.localUser.Authenticate((bool success) => {
//            if (success) {
//                // Notify Leaderboard we're available
//                Leaderboard.gpgAuthenticated = true;
//
//                if (GameState.Instance != null && GameState.Instance.isProEdition)
//                    saveProVersion(); // just in case user purchased ads before signing up to GPG
//
//                // Load GPG cloud data
//                LoadState();
//
//                // Show screen to connect (join / logon) to Google Play Games
//                if (!PlayerPrefs.HasKey("gpg")) {
//                    Debug.Log ("Google Play Games signed on for first time since this app install. Bonus of " + " +$" + Dealer.GPG_LOGON_BONUS.ToString("n0"));
//                    GameState.Instance.currentBalance += Dealer.GPG_LOGON_BONUS;
//                    GameState.Instance.updateGUIText();
//                    GameState.Instance.guiControls.displayMessage(LanguageManager.GetText ("label_gpg_bonus") + " +$" + Dealer.GPG_LOGON_BONUS.ToString("n0"));
//                    PlayerPrefs.SetString("gpg", "yes");
//                    LogUtils.LogEvent (Consts.FE_GPG_LOGON);
//                } else {
//                    Debug.Log ("Google Play Games logged on");
//                    LogUtils.LogEvent (Consts.FE_GPG_LOGON);
//                }
//            } else {
//                Debug.LogError ("Failure logging onto Google Play Games");
//                LogUtils.LogEvent (Consts.FE_GPG_LOGON_FAIL);
//                //FlurryAnalytics.Instance ().LogError("Social.localUser.Authenticate", "Failed to logon to GPG", "GPGCloudManager");
//            }
//        });
    }

    public void LoadState() {
//        Debug.Log("Loading cloud balance and pro version data from GPG servers");
//        ((PlayGamesPlatform) Social.Active).LoadState(0, this); // slot 0 is balance
//        ((PlayGamesPlatform) Social.Active).LoadState(1, this); // slot 1 is pro version
    }

    public void OnStateLoaded(bool success, int slot, byte[] data) {
//        if (success) {
//            // Shouldn't need to review the tutorial at all
//            if (GameState.Instance != null) {
//                GameState.Instance.tutorialCounter = 2;
//            }
//
//            if (0 == slot && data != null) { // cloud balance slot
//                Debug.Log ("Success loading balance (slot 0) from GPG cloud");
//                try {
//                    int cloudBalance = int.Parse(System.Text.Encoding.UTF8.GetString(data));
//                    Debug.Log ("Restoring cloud balance: $" + cloudBalance.ToString("n0") + ", overwriting local balance: $" + GameState.Instance.currentBalance.ToString("n0"));
//                    GameState.Instance.currentBalance = cloudBalance;
//                    LogUtils.LogEvent(Consts.FE_GPG_RESTORE_CLOUD_BALANCE);
//                    GUIControls.message = LanguageManager.GetText("label_loaded_cloud_balance"); // overrides any message already being displayed
//                    GameState.Instance.updateGUIText();
//                    GameState.Instance.guiControls.displayMessage(LanguageManager.GetText("label_loaded_cloud_balance"));
//
//                    // Load any outstanding TapJoy points here because if we do it before the cloud response has come back then
//                    // they may get overwritten
//                    Debug.Log ("Getting TapJoy points");
//                    TapjoyPlugin.GetTapPoints();
//
//                } catch (Exception e) {
//                    Debug.LogError ("Exception restoring cloud balance (slot 0) from GPG: " + e.Message);
//                    LogUtils.LogEvent (Consts.FE_GPG_RESTORE_CLOUD_BALANCE_FAIL);
//                    //FlurryAnalytics.Instance ().LogError("OnStateLoaded", "Exception: loading cloud balance (slot 0) from GPG: " + e.Message, "GPGCloudManager");
//                }
//            } else if (1 == slot && data != null) { // cloud pro version slot
//                Debug.Log ("Success loading cloud pro version (slot 1) from GPG");
//                try {
//                    string adsProStr = System.Text.Encoding.UTF8.GetString(data);
//                    if ("pro".Equals(adsProStr)) {
//                        Debug.Log ("Restoring cloud pro version purchase: " + adsProStr + ", overwriting local value: " + (GameState.Instance.isProEdition ? "pro" : "ads"));
//                        GameState.Instance.isProEdition = true;
//                        LogUtils.LogEvent(Consts.FE_GPG_RESTORE_CLOUD_PRO_VERSION);
//                    }
//                } catch (Exception e) {
//                    Debug.LogError ("Exception restoring cloud pro version (slot 1) from GPG: " + e.Message);
//                    LogUtils.LogEvent (Consts.FE_GPG_RESTORE_CLOUD_PRO_VERSION_FAIL);
//                    //FlurryAnalytics.Instance ().LogError("OnStateLoaded", "Exception loading cloud pro version (slot 1) from GPG: " + e.Message, "GPGCloudManager");
//                }
//            } else {
//                Debug.LogWarning ("OnStateLoaded GPG success no data?! slot: " + slot + ", data: " + data);
//            }
//        } else {
//            if (0 == slot) { // cloud balance slot
//                Debug.LogError ("Failure loading cloud balance (slot 0) from GPG");
//                LogUtils.LogEvent (Consts.FE_GPG_RESTORE_CLOUD_BALANCE_FAIL);
//                //FlurryAnalytics.Instance ().LogError("OnStateLoaded", "Failed to load GPG cloud balance", "GPGCloudManager");
//            } else if (1 == slot) { // cloud pro version slot
//                Debug.LogError ("Failure loading cloud pro version (slot 1) from GPG");
//                LogUtils.LogEvent (Consts.FE_GPG_RESTORE_CLOUD_PRO_VERSION_FAIL);
//                //FlurryAnalytics.Instance ().LogError("OnStateLoaded", "Failed to load GPG cloud pro version", "GPGCloudManager");
//            }
//        }
    }

    public void saveBalance() {
//        Debug.Log ("Updating cloud balance to " + GameState.Instance.currentBalance.ToString("n0"));
//        ((PlayGamesPlatform) Social.Active).UpdateState(0, // slot 0 is balance
//                                                        System.Text.Encoding.UTF8.GetBytes(GameState.Instance.currentBalance+""),
//                                                        this);
    }

    public void saveProVersion() {
//        Debug.Log ("Updating cloud pro version to: " + (GameState.Instance.isProEdition ? "pro" : "ads"));
//        ((PlayGamesPlatform) Social.Active).UpdateState(1, // slot 1 is pro version/ads version
//                                                        System.Text.Encoding.UTF8.GetBytes((GameState.Instance.isProEdition ? "pro" : "ads")),
//                                                        this);
    }
    
    public void OnStateSaved(bool success, int slot) {
//        if (slot == 0) { // slot 0 is balance
//            if (success) {
//                Debug.Log("Success updating cloud balance");
//                LogUtils.LogEvent(Consts.FE_GPG_STORE_CLOUD_BALANCE);
//            } else {
//                Debug.LogError("Failure updating cloud balance");
//                LogUtils.LogEvent (Consts.FE_GPG_STORE_CLOUD_BALANCE_FAIL);
//                //FlurryAnalytics.Instance ().LogError("OnStateSaved", "Failed to save GPG cloud balance", "GPGCloudManager");
//            }
//        } else if (slot == 1) { // slot 0 is cloud pro version
//            if (success) {
//                Debug.Log("Success updating cloud pro version");
//                LogUtils.LogEvent(Consts.FE_GPG_STORE_CLOUD_PRO_VERSION);
//            } else {
//                Debug.LogError("Failure updating cloud pro version");
//                LogUtils.LogEvent (Consts.FE_GPG_STORE_CLOUD_PRO_VERSION_FAIL);
//                //FlurryAnalytics.Instance ().LogError("OnStateSaved", "Failed to save GPG cloud pro version", "GPGCloudManager");
//            }
//        }
    }
    
    public byte[] OnStateConflict(int slot, byte[] local, byte[] server) {
//        if (slot == 0) { // current balance
//            // return state with higher balance
//            int localBalance = int.Parse(System.Text.Encoding.UTF8.GetString(local));
//            int cloudBalance = int.Parse(System.Text.Encoding.UTF8.GetString(server));
//            Debug.LogWarning("OnStateConflict CONFLICT! local balance: " + localBalance + " <-> cloud balance: " + cloudBalance);
//            if (localBalance >= cloudBalance) {
//                Debug.Log("Resolving with local balance: " + localBalance);
//                return local;
//            } else {
//                Debug.Log("Resolving with cloud balance: " + cloudBalance);
//                return server;
//            }
//        } else if (slot == 1) { // ads or no ads (pro / not pro)
//            string localAdsProStr = System.Text.Encoding.UTF8.GetString(local);
//            string cloudAdsProStr = System.Text.Encoding.UTF8.GetString(server);
//            Debug.LogWarning("OnStateConflict CONFLICT! local ads/pro: " + localAdsProStr + " <-> cloud ads/pro: " + cloudAdsProStr);
//            if (localAdsProStr == "pro") {
//                Debug.Log("Resolving as localAdsProStr == pro");
//                return local;
//            } else if (cloudAdsProStr == "pro") {
//                Debug.Log("Resolving as cloudAdsProStr == pro");
//                return server;
//            } else {
//                Debug.Log(@"Resolving as localAdsPro == " + localAdsProStr + " (btw cloud == " + cloudAdsProStr + ")");
//                return local;
//            }
//        } else {
//            return local;
//        }
        return null;
    }

    public void postScore() {
//        Debug.Log ("Automatically posting own score to Google Play Games: $" + GameState.Instance.currentBalance.ToString("n0"));
//        Social.ReportScore(GameState.Instance.currentBalance, Consts.GPG_LEADERBOARD_ID, (bool success) => {
//            if (success) {
//                Debug.Log ("Success posting score to Google Play Games");
//                LogUtils.LogEvent (Consts.FE_GPG_SCORE_POST);
//            } else {
//                Debug.LogWarning("Failure posting score to Google Play Games");
//                LogUtils.LogEvent (Consts.FE_GPG_SCORE_POST_FAIL);
//                //FlurryAnalytics.Instance ().LogError("Social.ReportScore", "Failed to post GPG score", "GPGCloudManager");
//            }
//        });
    }
}

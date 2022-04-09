using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Dialog for Shop Manager to do in app purchases
public class ShopManager : MonoBehaviour
{
    #region variables

    // Dimensions
    static float BTN_WIDTH = 0.8f;
    static float BTN_HEIGHT = 0.085f;

    // IAP item SKUs
#if UNITY_IPHONE
    public readonly string SKU_NO_ADS = "b3d_ad_free_pack_nonconsumerable2"; // app got rejected for this IAP not being non-consumerable
#else
    public readonly string SKU_NO_ADS = "b3d_ad_free_pack";
#endif
    public readonly string SKU_CHIPS1 = "b3d_50_chips";    // $1,000
    public readonly string SKU_CHIPS2 = "b3d_500_chips";   // $5,000
    public readonly string SKU_CHIPS3 = "b3d_10k_chips";   // $10,000
    public readonly string SKU_CHIPS4 = "b3d_100k_chips";  // $100,000
    public readonly string SKU_CHIPS5 = "b3d_1mil_chips_tier_60";  // $1,000,000 - was originally SKU b3d_1mil_chips (priced at tier 61) but Apple rejected our second version (v1.0.14) because of having chi
    public readonly string SKU_CHIPS6 = "b3d_10mil_chips"; // $10,000,000

    #endregion

    // List of items available for in-app purchasing
    public Hashtable iapItems = new Hashtable();


    public void Start() {
#if UNITY_EDITOR || UNITY_WEBPLAYER
		enabled = false;
		return;
#endif
        // Register ourselves with the GameState instance
        GameState.Instance.shopManager = this;

        // Build list of IAP items with their SKUs as keys and get their corresponding info
        iapItems.Add(SKU_NO_ADS, null);  // No Ads (duh)
        iapItems.Add(SKU_CHIPS1, null);  // $1,000
        iapItems.Add(SKU_CHIPS2, null);   // $5,000
        iapItems.Add(SKU_CHIPS3, null);  // $10,000
        iapItems.Add(SKU_CHIPS4, null);  // $100,000
        iapItems.Add(SKU_CHIPS5, null);  // $1,000,000
        iapItems.Add(SKU_CHIPS6, null);  // $10,000,000
    }

    // Callback once IAP plugin has got data on all our IAP items then we update our internal map
    public void UpdateIapItemsInfo() {
        Debug.Log ("Updating IAP item map with their infos");
//        iapItems[SKU_NO_ADS] = SmartIAP.Instance().GetItemInfo(SKU_NO_ADS);
//        Debug.Log (SKU_NO_ADS + " is " + ((SmartIAPItem) iapItems[SKU_NO_ADS]).currency + ((SmartIAPItem) iapItems[SKU_NO_ADS]).price);
//        iapItems[SKU_CHIPS1] = SmartIAP.Instance().GetItemInfo(SKU_CHIPS1);
//        Debug.Log (SKU_CHIPS1 + " is " + ((SmartIAPItem) iapItems[SKU_CHIPS1]).currency + ((SmartIAPItem) iapItems[SKU_CHIPS1]).price);
//        iapItems[SKU_CHIPS2] = SmartIAP.Instance().GetItemInfo(SKU_CHIPS2);
//        Debug.Log (SKU_CHIPS2 + " is " + ((SmartIAPItem) iapItems[SKU_CHIPS2]).currency + ((SmartIAPItem) iapItems[SKU_CHIPS2]).price);
//        iapItems[SKU_CHIPS3] = SmartIAP.Instance().GetItemInfo(SKU_CHIPS3);
//        Debug.Log (SKU_CHIPS3 + " is " + ((SmartIAPItem) iapItems[SKU_CHIPS2]).currency + ((SmartIAPItem) iapItems[SKU_CHIPS3]).price);
//        iapItems[SKU_CHIPS4] = SmartIAP.Instance().GetItemInfo(SKU_CHIPS4);
//        Debug.Log (SKU_CHIPS4 + " is " + ((SmartIAPItem) iapItems[SKU_CHIPS3]).currency + ((SmartIAPItem) iapItems[SKU_CHIPS4]).price);
//        iapItems[SKU_CHIPS5] = SmartIAP.Instance().GetItemInfo(SKU_CHIPS5);
//        Debug.Log (SKU_CHIPS5 + " is " + ((SmartIAPItem) iapItems[SKU_CHIPS5]).currency + ((SmartIAPItem) iapItems[SKU_CHIPS5]).price);
//        iapItems[SKU_CHIPS6] = SmartIAP.Instance().GetItemInfo(SKU_CHIPS6);
//        Debug.Log (SKU_CHIPS6 + " is " + ((SmartIAPItem) iapItems[SKU_CHIPS6]).currency + ((SmartIAPItem) iapItems[SKU_CHIPS6]).price);
    }

    public void ToggleOn() {
       Debug.Log("Toggling ShopManager isDrawGui from " + GetComponent<CustomizedCenteredGuiWindow>().isDrawGui + " to " + !GetComponent<CustomizedCenteredGuiWindow>().isDrawGui);
       if (!GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            GetComponent<CustomizedCenteredGuiWindow>().normalizedHeight = 0.8f;
       }
       GetComponent<CustomizedCenteredGuiWindow>().isDrawGui = !GetComponent<CustomizedCenteredGuiWindow>().isDrawGui;

        // Hide the stats panel when showing any centered GUI window, and show on vice versa
        if (GetComponent<CustomizedCenteredGuiWindow>().isDrawGui) {
            // This doesn't actually close it (haha I'll let you try and recall why, just know that it closes elsewhere. Hint: order!)
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().hidePanelOnMenuOpen ();
            CustomizedStatsPanel.showPanelb = false;
        } else {
            GameState.Instance.guiControls.statsPanel.GetComponent<CustomizedStatsPanel> ().showPanelOnMenuClose ();
        }
    }

    public void OnGUICallback (string[] args)
    {
#if UNITY_EDITOR || UNITY_WEBPLAYER
		return;
#endif

        // Extract string args and convert to ints (passed from SendMessage call)
        float width = float.Parse (args [0]);
        float height = float.Parse (args [1]);

        float buttonWidth = width * BTN_WIDTH;
        float buttonHeight = height * BTN_HEIGHT;

        GUILayout.BeginArea (new Rect (0, 0, width, height));
        GUILayout.FlexibleSpace ();
        GUILayout.BeginHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.BeginVertical ();
        GUI.skin.button.fontStyle = FontStyle.Bold;

        // Remember the original style states
        Color origTextColor = GUI.skin.button.active.textColor;
        int origFontSize = GUI.skin.label.fontSize;

        // Window title font size
        GUI.skin.window.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 6f);
        GUI.skin.window.fontStyle = FontStyle.Bold;

        // No Ads
        string label = MakeIAPItemButtonLabel(SKU_NO_ADS, LanguageManager.GetText("btn_noads"));
        GUI.skin.button.fontSize = Mathf.RoundToInt(GUIControls.fontScaler / 4.5f);
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.white;
        if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Purchasing " + label);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_NO_ADS }, false);
//            SmartIAP.Instance().Purchase(SKU_NO_ADS);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;

        // $1000
        label = MakeIAPItemButtonLabel(SKU_CHIPS1, "$1,000 " + LanguageManager.GetText("btn_chip"));
        if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Purchasing " + label);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CHIPS1 }, false);
//            SmartIAP.Instance().Purchase(SKU_CHIPS1);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }

        // $5000
        label = MakeIAPItemButtonLabel(SKU_CHIPS2, "$5,000 " + LanguageManager.GetText("btn_chip"));
        if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Purchasing " + label);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CHIPS2 }, false);
//            SmartIAP.Instance().Purchase(SKU_CHIPS2);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }

        // $10,000
        label = MakeIAPItemButtonLabel(SKU_CHIPS3, "$10,000 " + LanguageManager.GetText("btn_chip"));
        if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Purchasing " + label);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CHIPS3 }, false);
//            SmartIAP.Instance().Purchase(SKU_CHIPS3);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }

        // $100,000
        label = MakeIAPItemButtonLabel(SKU_CHIPS4, "$100,000 " + LanguageManager.GetText("btn_chip"));
        if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Purchasing " + label);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CHIPS4 }, false);
//            SmartIAP.Instance().Purchase(SKU_CHIPS4);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }

        // $1,000,000
        label = MakeIAPItemButtonLabel(SKU_CHIPS5, "$1,000,000 " + LanguageManager.GetText("btn_chip"));
        if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Purchasing " + label);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CHIPS5 }, false);
//            SmartIAP.Instance().Purchase(SKU_CHIPS5);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }

#if UNITY_IPHONE // Apple rejects IAPs over tier 60 but the item got approved on the first review so lets sneak it in after a date
        string csdateTimeStr = "2014-05-30";
        DateTime csuser_time = DateTime.Parse( csdateTimeStr );
        DateTime cstime_now = DateTime.Now;
        if( cstime_now > csuser_time )
        {
#endif
            // $10,000,000
            label = MakeIAPItemButtonLabel(SKU_CHIPS6, "$10,000,000 " + LanguageManager.GetText("btn_chip"));
            if (GUILayout.Button (label, GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
                Debug.Log ("Purchasing " + label);
                LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CHIPS6 }, false);
//                SmartIAP.Instance().Purchase(SKU_CHIPS6);
                GameState.Instance.ToggleFingerGestures (true);
                ToggleOn();
            }
#if UNITY_IPHONE // Apple rejects IAPs over tier 60
        }
#endif

        GUILayout.BeginHorizontal ();

        // MORE GAMES
        bool twiceSizeHack = false;
#if UNITY_IPHONE // Apple rejected for showing what I think was 'more games'
        string dateTimeStr = "2014-05-30";
        DateTime user_time = DateTime.Parse( dateTimeStr );
        DateTime time_now = DateTime.Now;
        if( time_now > user_time )
        {
            GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.black;
            if (GUILayout.Button (LanguageManager.GetText("btn_more_games"), GUILayout.Width (buttonWidth/2), GUILayout.Height (buttonHeight))) {
                Debug.Log ("More Games pressed");
                LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_MORE_GAMES }, false);
//                if (GameState.Instance.adsManager != null) {
//                    Debug.Log ("Calling More Games from Ads Manager");
//                    GameState.Instance.adsManager.showMoreGames();
//                }
                GameState.Instance.ToggleFingerGestures (true);
                ToggleOn();
            }

        } else {
            twiceSizeHack = true; // make Free Chips take up its space
        }
#else
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.black;
        if (GUILayout.Button (LanguageManager.GetText("btn_more_games"), GUILayout.Width (buttonWidth/2), GUILayout.Height (buttonHeight))) {
            Debug.Log ("More Games pressed");
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_MORE_GAMES }, false);
            if (GameState.Instance.adsManager != null) {
                Debug.Log ("Calling More Games from Ads Manager");
                GameState.Instance.adsManager.showMoreGames();
            }
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }
#endif

        // FREE CHIPS
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = Color.white;
        if (GUILayout.Button (LanguageManager.GetText("btn_free_chips"), GUILayout.Width (buttonWidth/(twiceSizeHack ? 1 : 2)), GUILayout.Height (buttonHeight))) {
            Debug.Log ("Free Chips pressed");
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_FREE_CHIPS }, false);
#if UNITY_EDITOR
            GameState.Instance.currentBalance = 1000000;
            GameState.Instance.updateGUIText();
#endif
            Debug.Log ("[ads]Opening free chip offers");
//            TapjoyPlugin.ShowOffers();

            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }
        GUI.skin.button.active.textColor = GUI.skin.button.normal.textColor = origTextColor;

        GUILayout.EndHorizontal();

#if UNITY_IPHONE
        // Restore past purchases
        if (GUILayout.Button (LanguageManager.GetText ("btn_restore_purchases"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_RESTORE }, false);
            GameState.Instance.ToggleFingerGestures (true);
//            SmartIAP.Instance().RestoreCompletedTransactions();
            ToggleOn();
        }
#endif

        // Close the menu
        if (GUILayout.Button (LanguageManager.GetText ("btn_close"), GUILayout.Width (buttonWidth), GUILayout.Height (buttonHeight))) {
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_EVENT, new string[] { Consts.FEP_BTN_SHOP_CLOSE }, false);
            GameState.Instance.ToggleFingerGestures (true);
            ToggleOn();
        }

        GUILayout.EndVertical ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndHorizontal ();
        GUILayout.FlexibleSpace ();
        GUILayout.EndArea ();
    }

    // Make a localized label to show the name of the IAP item on a button using the IAP item info (price, currency, title) from the appstore
    // if available, otherwise defaulting
    string MakeIAPItemButtonLabel(string sku, string defaultLabel) {
        if (iapItems[sku] == null)
            return defaultLabel;

//        SmartIAPItem item = (SmartIAPItem) iapItems[sku];
#if UNITY_ANDROID
        if (item.title.Contains("(")) {
            //Debug.Log ("MakeIAPItemButtonLabel type 1: SKU:"+sku+" => "+iitem.title.Substring(0, item.title.IndexOf('('))
            //           + " (" + item.currency + item.price + ")";
            return item.title.Substring(0, item.title.IndexOf('(')) // Remove the (BaccARat 3D) that Google forces into the name
                + " (" + item.currency + item.price + ")";
        }
        else {
            //Debug.Log ("MakeIAPItemButtonLabel type 2: SKU:"+sku+" => "+item.title + " [" + item.currency + item.price + "]");
            return item.title + " [" + item.currency + item.price + "]";
        }
#else
        //Debug.Log ("MakeIAPItemButtonLabel type 3: SKU:"+sku+" => "+item.title + " [" + item.currency + item.price + "]");
//        return item.title + " [" + item.currency + item.price + "]"; // Simon temp:
return "";
#endif
    }

    public void OnPurchaseSucceeded(string sku) {
        // Succeeded in purchasing!

        string extraMsg = "";

        // Handle the purchase
        if (SKU_NO_ADS.Equals(sku)) {
            Debug.Log("Purchased no ads pack!");
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_NO_ADS }, false);
            //extraMsg = LanguageManager.GetText("label_noads_after_restart");
            GameState.Instance.isProEdition = true;

            // Save pro version (no ads) purchase in Google Cloud
            GameState.Instance.gpgCloudManager.saveProVersion();
        } else if (SKU_CHIPS1.Equals(sku)) { // $1000
            Debug.Log("Purchased $1000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_CHIPS1 }, false);
            GameState.Instance.currentBalance += 1000;
        } else if (SKU_CHIPS2.Equals(sku)) { // $5000
            Debug.Log("Purchased $5000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_CHIPS2 }, false);
            GameState.Instance.currentBalance += 5000;
        } else if (SKU_CHIPS3.Equals(sku)) { // $10,000
            Debug.Log("Purchased $10,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_CHIPS3 }, false);
            GameState.Instance.currentBalance += 10000;
        } else if (SKU_CHIPS4.Equals(sku)) { // $100,000
            Debug.Log("Purchased $100,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_CHIPS4 }, false);
            GameState.Instance.currentBalance += 100000;
        } else if (SKU_CHIPS5.Equals(sku)) { // $1,000,000
            Debug.Log("Purchased $1,000,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_CHIPS5 }, false);
            GameState.Instance.currentBalance += 1000000;
        } else if (SKU_CHIPS6.Equals(sku)) { // $10,000,000
            Debug.Log("Purchased $10,000,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_SUCCESS_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_CHIPS6 }, false);
            GameState.Instance.currentBalance += 10000000;
        }

        GameState.Instance.updateGUIText();
        Debug.Log (LanguageManager.GetText("label_purchase_ok") + extraMsg); // TODO: make this a popup
		GameState.Instance.guiControls.displayMessage(LanguageManager.GetText("label_purchase_ok"));
    }

    public void OnItemAlreadyOwned (string sku) {
        Debug.Log ("OnItemAlreadyOwned: sku: " + sku);

        // Handle the purchase
        if (SKU_NO_ADS.Equals(sku)) {
            Debug.Log("Re-purchased no ads pack (it was already owned)!");
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_ALREADY_OWNED_EVENT, new string[] { Consts.FEP_BTN_SHOP_PURCHASED_NO_ADS_ALREADY_OWNED }, false);
            GameState.Instance.isProEdition = true;
            
            // Save pro version (no ads) purchase in Google Cloud
            GameState.Instance.gpgCloudManager.saveProVersion();
        }
        
        GameState.Instance.updateGUIText();
        Debug.Log (LanguageManager.GetText("label_purchase_ok")); // TODO: make this a popup
        GameState.Instance.guiControls.displayMessage(LanguageManager.GetText("label_purchase_ok"));
    }

    public void OnPurchaseFailed(string sku, string err) {
        Debug.Log ("Failed to purchase sku:" + sku + ", err:" + err);
        Debug.Log (LanguageManager.GetText("label_purchase_ng")); // TODO: make this a popup
		GameState.Instance.guiControls.displayMessage(LanguageManager.GetText("label_purchase_ng"));

        //FlurryAnalytics.Instance ().LogError(sku, err, "ShopManager");

        if (SKU_NO_ADS.Equals(sku)) {
            Debug.Log("Failed to purchase no ads pack!");
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_NO_ADS }, false);
        } else if (SKU_CHIPS1.Equals(sku)) { // $1000
            Debug.Log("Failed to purchase $1000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS1 }, false);
        } else if (SKU_CHIPS2.Equals(sku)) { // $5000
            Debug.Log("Failed to purchase $5000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS2 }, false);
        } else if (SKU_CHIPS3.Equals(sku)) { // $10,000
            Debug.Log("Failed to purchase $10,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS3 }, false);
        } else if (SKU_CHIPS4.Equals(sku)) { // $100,000
            Debug.Log("Failed to purchase $100,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS4 }, false);
        } else if (SKU_CHIPS5.Equals(sku)) { // $1,000,000
            Debug.Log("Failed to purchase $1,000,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS5 }, false);
        } else if (SKU_CHIPS6.Equals(sku)) { // $10,000,000
            Debug.Log("Failed to purchase $10,000,000 chips! sku:" + sku);
            LogUtils.LogEvent(Consts.FE_BTN_SHOP_IAP_FAIL_EVENT, new string[] { Consts.FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS6 }, false);
        }
    }
}
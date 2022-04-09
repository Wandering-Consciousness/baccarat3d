using System;

public class Consts {
    // Enable especially when deploying to devices as a standalone Unity project (and not merged with Android BaccARat 3D project)
    // as isPreferenceEnabled()'s logic was causing Unity to crash ugly on Android. Disable when merging.
    public static bool DEBUG = true;

    // Debug force AR off/on
    public static bool AR_DEBUG_ON = false;

    // Force Japanese language
    public static bool JAP_DEBUG_ON = false;

    // Facebook credentials
    //public static string facebookAppID    = "662062447141117";
    //public static string facebooAppSecret = "36a36c05b01a1a70def22c4d001c50af";

    // Google Play Game Leaderboard ID
    public static string GPG_LEADERBOARD_ID = "CgkI6prg6b0SEAIQAA";

    // Flurry constants
    public static string FE_ENV_LANGUAGE = "Environment language";
    public static string FE_BTN_MENU_OPEN = "Open Menu pressed";
    public static string FE_BTN_MENU_CLOSE = "Close Menu pressed";
    public static string FE_BTN_MAIN_MENU_EVENT = "Main Menu button presses";
    public static string FEP_BTN_MENU_EXIT_GAME = "Home (Exit)";
    public static string FEP_BTN_MENU_SHOP = "Shop button";
    public static string FEP_BTN_MENU_INVITE_FRIENDS = "Invite Friends";
    public static string FEP_BTN_MENU_LEADERBOARD = "Leaderboard";
    public static string FEP_BTN_MENU_MUSIC = "Music button";
    public static string FEP_BTN_MENU_HELP = "Help button";

    public static string FE_BTN_SHOP_EVENT = "In-App Purchase button presses";
    public static string FEP_BTN_SHOP_NO_ADS = "No Ads Pack";
    public static string FEP_BTN_SHOP_CHIPS1 = "$1000 chips";
    public static string FEP_BTN_SHOP_CHIPS2 = "$5000 chips";
    public static string FEP_BTN_SHOP_CHIPS3 = "$10,000 chips";
    public static string FEP_BTN_SHOP_CHIPS4 = "$100,000 chips";
    public static string FEP_BTN_SHOP_CHIPS5 = "$1,000,000 chips";
    public static string FEP_BTN_SHOP_CHIPS6 = "$10,000,000 chips";
    public static string FEP_BTN_SHOP_MORE_GAMES = "More Games pressed";
    public static string FEP_BTN_SHOP_FREE_CHIPS = "Free Chips pressed";
    public static string FEP_BTN_SHOP_CLOSE = "Close pressed";
    public static string FEP_BTN_SHOP_RESTORE = "Restore Purchases pressed";

    public static string FE_BTN_SHOP_IAP_SUCCESS_EVENT = "Successful In-App purchases";
    public static string FE_BTN_SHOP_IAP_ALREADY_OWNED_EVENT = "In-App purchase already owned";
    public static string FEP_BTN_SHOP_PURCHASED_NO_ADS = "No Ads Pack";
    public static string FEP_BTN_SHOP_PURCHASED_NO_ADS_ALREADY_OWNED = "No Ads Pack [already owned]";
    public static string FEP_BTN_SHOP_PURCHASED_CHIPS1 = "$1000 chips";
    public static string FEP_BTN_SHOP_PURCHASED_CHIPS2 = "$5000 chips";
    public static string FEP_BTN_SHOP_PURCHASED_CHIPS3 = "$10,000 chips";
    public static string FEP_BTN_SHOP_PURCHASED_CHIPS4 = "$100,000 chips";
    public static string FEP_BTN_SHOP_PURCHASED_CHIPS5 = "$1,000,000 chips";
    public static string FEP_BTN_SHOP_PURCHASED_CHIPS6 = "$10,000,000 chips";

    public static string FE_BTN_SHOP_IAP_FAIL_EVENT = "Failed In-App purchases";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_NO_ADS = "No Ads Pack";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS1 = "$1000 chips";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS2 = "$5000 chips";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS3 = "$10,000 chips";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS4 = "$100,000 chips";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS5 = "$1,000,000 chips";
    public static string FEP_BTN_SHOP_FAIL_PURCHASE_CHIPS6 = "$10,000,000 chips";

    public static string FE_BTN_FB_WEBSHOP_EVENT = "In-App Purchase button presses [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CHIPS1 = "$1000 chips [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CHIPS2 = "$5000 chips [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CHIPS3 = "$10,000 chips [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CHIPS4 = "$100,000 chips [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CHIPS5 = "$1,000,000 chips [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CHIPS6 = "$10,000,000 chips [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_CLOSE = "Close pressed [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_RESTORE = "Restore Purchases pressed [Facebook Web Ver.]";
    
    public static string FE_BTN_FB_WEBSHOP_IAP_SUCCESS_EVENT = "Successful In-App purchases [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_PURCHASED_CHIPS = " chips [Facebook Web Ver.]";
    
    public static string FE_BTN_FB_WEBSHOP_IAP_FAIL_EVENT = "Failed In-App purchases [Facebook Web Ver.]";
    public static string FEP_BTN_FB_WEBSHOP_FAIL_PURCHASE_CHIPS = " chips [Facebook Web Ver.]";

    public static string FE_SHOP_IAP_CANCEL_EVENT = "User cancelled In-App purchase";
    public static string FE_SHOP_IAP_COMPLETE_TRANSACTIONS_FAILED_TO_RESTORE = "Complete transactions failed to restore for In-App purchases";
    public static string FE_SHOP_IAP_COMPLETE_TRANSACTIONS_RESTORED = "Complete transactions restored for In-App purchases";

    public static string FE_BTN_LEADERBOARD_FACEBOOK_CONNECT = "Leaderboard -> Facebook Connect pressed";
    public static string FE_BTN_LEADERBOARD_FACEBOOK_CONNECT_CLOSE = "Leaderboard -> Facebook Connect Close pressed";
    public static string FE_BTN_LEADERBOARD_FACEBOOK_POST = "Leaderboard -> Facebook Post pressed";
    public static string FE_BTN_LEADERBOARD_CLOSE = "Leaderboard -> Close pressed";

    public static string FE_BTN_LEADERBOARD_GPG_LOGON = "Leaderboard -> Google Play Games Logon pressed";
    public static string FE_BTN_LEADERBOARD_GPG_LOGON_CLOSE = "Leaderboard -> Google Play Games Close pressed";
    public static string FE_BTN_LEADERBOARD_GPG_LEADERBOARD = "Leaderboard -> Google Play Games Leaderboard pressed";

    
    public static string FE_BTN_HELP_HOW_TO_SQUEEZE = "Help -> How To Squeeze pressed";
    public static string FE_BTN_HELP_VIDEO = "Help -> Video pressed";
    public static string FE_BTN_HELP_TUTORIAL = "Help -> Tutorial pressed";
    public static string FE_BTN_HELP_PLAY_NOW = "Help -> Play Now pressed";

    public static string FE_BTN_CLEAR_CHIP = "Clear Chips pressed";
    public static string FE_BTN_GYROCAM = "Gyrocam pressed";
    public static string FE_BTN_AR = "AR pressed";
    public static string FE_BTN_DEAL = "Deal pressed";
    public static string FE_BTN_3D = "3D pressed";
    public static string FE_BTN_OTHER_CARD_EVENT = "Other Card presses";
    public static string FEP_BTN_LEFT_CARD = "Left";
    public static string FEP_BTN_RIGHT_CARD = "Right";
    public static string FE_BTN_RETURN_CARD = "Return Card pressed";
    public static string FE_BTN_REBET = "Rebet pressed";

    public static string FEP_BTN_REVEAL_EVENT = "Reveal Other Card presseds";
    public static string FEP_BTN_REVEAL_PLAYER_2_CARDS = "First 2 Player cards";
    public static string FEP_BTN_REVEAL_BANKER_2_CARDS = "First 2 Banker cards";
    public static string FEP_BTN_REVEAL_PLAYER_3RD_CARD = "3rd Player card";
    public static string FEP_BTN_REVEAL_BANKER_3RD_CARD = "3rd Banker card";

    public static string FE_TUTORIAL_HELP_SCROLL = "Scroll on Tutorial Help screen";
    public static string FE_GYROCAM_SCROLL = "Scoll on Gyrocam screen";
   
    public static string FE_ROUND_NUMBER = "Number of rounds";
    public static string FE_SHOE_NUMBER = "Number of shoes";

    public static string FE_SHOP_OPEN_LOW_BALANCE = "Open Shop screen because insufficient balance";

    public static string FE_BET_ON_EVENT = "User bet type";
    public static string FEP_BET_ON_PLAYER = "Player";
    public static string FEP_BET_ON_BANKER = "Banker";
    public static string FEP_BET_ON_PLAYER_PAIR = "Player Pair";
    public static string FEP_BET_ON_BANKER_PAIR = "Banker Pair";
    public static string FEP_BET_ON_TIE = "Tie";

    public static string FE_BET_AMOUNT_EVENT = "User bet amount";
    public static string FEP_BET_AMOUNT_PLAYER = "Player";
    public static string FEP_BET_AMOUNT_BANKER = "Banker";
    public static string FEP_BET_AMOUNT_PLAYER_PAIR = "Player Pair";
    public static string FEP_BET_AMOUNT_BANKER_PAIR = "Banker Pair";
    public static string FEP_BET_AMOUNT_TIE = "Tie";

    public static string FE_WIN_EVENT = "Wins";
    public static string FEP_PLAYER_WIN = "Player";
    public static string FEP_BANKER_WIN = "Banker";
    public static string FEP_TIE_WIN = "Tie wins";

    public static string FE_PAIR_EVENT = "Card pairs";
    public static string FEP_PLAYER_PAIR = "Player";
    public static string FEP_BANKER_PAIR = "Banker";

    public static string FE_NATURAL_EVENT = "Card naturals";
    public static string FEP_PLAYER_NATURAL = "Player";
    public static string FEP_BANKER_NATURAL = "Banker";

    public static string FE_BET_CHIP_EVENT = "User bet chips";
    public static string FEP_BET_CHIP1 = "$100";
    public static string FEP_BET_CHIP2 = "$500";
    public static string FEP_BET_CHIP3 = "$1000";
    public static string FEP_BET_CHIP4 = "$10,000";
    public static string FEP_BET_CHIP5 = "$100,000";

    public static string FE_WINLOSE_BET_TYPE_AMOUNT_EVENT = "Amount won/lost (per bet type/bonus)";
    public static string FEP_WINLOSE_PLAYER = "Player";
    public static string FEP_WINLOSE_BANKER = "Banker";
    public static string FEP_WINLOSE_PLAYER_PAIR = "Player Pair";
    public static string FEP_WINLOSE_BANKER_PAIR = "Banker Pair";
    public static string FEP_WINLOSE_TIE = "Tie";
    public static string FEP_WINLOSE_BONUS_AR = "AR bonus";

    public static string FE_WINLOSE_TOTALS_EVENT = "Amount won/lost (totals)";
    public static string FEP_WIN_AMOUNT = "Win";
    public static string FEP_LOSE_AMOUNT = "Lose";

    public static string FE_SQUEEZE_SINGLE_FINGER_DIAGONAL_LEFT = "Squeezed diagonally from left with 1 finger";
    public static string FE_SQUEEZE_SINGLE_FINGER_DIAGONAL_RIGHT = "Squeezed diagonally from right with 1 finger";
    public static string FE_SQUEEZE_SINGLE_FINGER_STRAIGHT = "Squeezed straight with 1 finger";
    public static string FE_SQUEEZE_TWO_FINGERS_LEFT = "Squeezed from left with 2 fingers";
    public static string FE_SQUEEZE_TWO_FINGERS_RIGHT = "Squeezed from right with 2 fingers";
    public static string FE_SQUEEZE_TWO_FINGERS_BOTTOM = "Squeezed straight from bottom-end with 2 fingers";
    public static string FE_SQUEEZE_TWO_FINGERS_TOP = "Squeezed straight from top-end with 2 fingers";
    public static string FE_SQUEEZE_TAP_ROTATE = "Double tap rotation";
    public static string FE_SQUEEZE_TAP_REVEAL = "Long tap reveal";

    public static string FE_THREE_FINGER_SWIPE = "Swipe left 3 fingers";
    public static string FE_TWO_FINGER_ZOOM = "Zoom with 2 fingers";

    public static string FE_TUTORIAL_SCREEN_TOGGLED_ON = "Tutorial help screen toggled on";
    public static string FE_TUTORIAL_SCREEN_TOGGLED_OFF = "Tutorial help screen toggled off";
    public static string FE_TUTORIAL_SHOWED_PLACE_BETS = "Showed 'place bets' tutorial";
    public static string FE_TUTORIAL_SHOWED_TWO_FINGER_ZOOM = "Showed '2 finger zoom' tutorial";
    public static string FE_TUTORIAL_SHOWED_THREE_FINGER_SWIPE = "Showed '3 finger swipe' tutorial";
    public static string FE_TUTORIAL_SHOWED_CLEAR_CHIPS = "Showed 'clear chips' tutorial";
    public static string FE_TUTORIAL_SHOWED_START_DEALING = "Showed 'start dealing' tutorial";
    public static string FE_TUTORIAL_SHOWED_OTHER_CARD = "Showed 'other card' tutorial";
    public static string FE_TUTORIAL_SHOWED_RETURN_CARD = "Showed 'return card' tutorial";
    public static string FE_TUTORIAL_SHOWED_TAP_ROTATE = "Showed 'tap rotate' tutorial";
    public static string FE_TUTORIAL_SHOWED_ONE_FINGER_SQUEEZE = "Showed '1 finger squeeze' tutorial";
    public static string FE_TUTORIAL_SHOWED_TWO_FINGER_SQUEEZE = "Showed '2 finger squeeze' tutorial";
    public static string FE_TUTORIAL_SHOWED_START_SQUEEZE = "Showed 'start squeeze' tutorial";
    public static string FE_TUTORIAL_SHOWED_AR = "Showed AR tutorial";
    public static string FE_TUTORIAL_SHOWED_GYROCAM = "Showed gyrocam tutorial";

    public static string FE_ADS_REVMOB_AD_RECEIVED = "RevMob Ad did receive:";
    public static string FE_ADS_REVMOB_AD_DISPLAYED = "RevMob Ad displayed:";
    public static string FE_ADS_REVMOB_AD_CLICKED = "RevMob Ad clicked:";
    public static string FE_ADS_REVMOB_AD_FAILED = "RevMob Ad did fail:";
    public static string FE_ADS_REVMOB_AD_CLOSED = "RevMob Ad closed:";
    public static string FE_ADS_REVMOB_INSTALL_RECEIVED = "RevMob Install did receive:";
    public static string FE_ADS_REVMOB_INSTALL_FAILED = "RevMob Install did fail:";

    public static string FE_ADS_CHARTBOOST_FAIL_LOAD_INTERSITIAL_EVENT = "Chartboost didFailToLoadInterstitialEvent:";
    public static string FE_ADS_CHARTBOOST_DISMISS_INTERSITIAL_EVENT = "Chartboost didDismissInterstitialEvent:";
    public static string FE_ADS_CHARTBOOST_CLOSE_INTERSITIAL_EVENT = "Chartboost didCloseInterstitialEvent:";
    public static string FE_ADS_CHARTBOOST_CLICK_INTERSITIAL_EVENT = "Chartboost didClickInterstitialEvent:";
    public static string FE_ADS_CHARTBOOST_CACHE_INTERSITIAL_EVENT = "Chartboost didCacheInterstitialEvent:";
    public static string FE_ADS_CHARTBOOST_SHOW_INTERSITIAL_EVENT = "Chartboost didShowInterstitialEvent:";
    public static string FE_ADS_CHARTBOOST_FAIL_LOAD_MORE_APPS_EVENT = "Chartboost didFailToLoadMoreAppsEvent:";
    public static string FE_ADS_CHARTBOOST_DISMISS_MORE_APPS_EVENT = "Chartboost didDismissMoreAppsEvent";
    public static string FE_ADS_CHARTBOOST_CLOSE_MORE_APPS_EVENT = "Chartboost didCloseMoreAppsEvent";
    public static string FE_ADS_CHARTBOOST_CLICK_MORE_APPS_EVENT = "Chartboost didClickMoreAppsEvent";
    public static string FE_ADS_CHARTBOOST_CACHE_MORE_APPS_EVENT = "Chartboost didCacheMoreAppsEvent";
    public static string FE_ADS_CHARTBOOST_SHOW_MORE_APPS_EVENT = "Chartboost didShowMoreAppsEvent";

    public static string FE_ADS_TAPJOY_CONNECT_SUCCESS = "Tapjoy HandleTapjoyConnectSuccess";
    public static string FE_ADS_TAPJOY_CONNECT_FAILED = "Tapjoy HandleTapjoyConnectFailed";
    public static string FE_ADS_TAPJOY_GET_TAP_POINTS_SUCCEEDED = "Tapjoy HandleGetTapPointsSucceeded:";
    public static string FE_ADS_TAPJOY_GET_TAP_POINTS_FAILED = "Tapjoy HandleGetTapPointsFailed";
    public static string FE_ADS_TAPJOY_SPEND_TAP_POINTS_SUCCEEDED = "Tapjoy HandleSpendTapPointsSucceeded:";
    public static string FE_ADS_TAPJOY_SPEND_TAP_POINTS_FAILED = "Tapjoy HandleSpendTapPointsFailed";
    public static string FE_ADS_TAPJOY_AWARD_TAP_POINTS_SUCCEEDED = "Tapjoy HandleAwardTapPointsSucceeded";
    public static string FE_ADS_TAPJOY_AWARD_TAP_POINTS_FAILED = "Tapjoy HandleAwardTapPointsFailed";
    public static string FE_ADS_TAPJOY_CURRENCY_EARNED = "Tapjoy CurrencyEarned:";
    public static string FE_ADS_TAPJOY_GET_FULLSCREEN_AD_SUCCEEDED = "Tapjoy HandleGetFullScreenAdSucceeded";
    public static string FE_ADS_TAPJOY_GET_FULLSCREEN_AD_FAILED = "Tapjoy HandleGetFullScreenAdFailed";
    public static string FE_ADS_TAPJOY_GET_DISPLAY_AD_SUCCEEDED = "Tapjoy HandleGetDisplayAdSucceeded";
    public static string FE_ADS_TAPJOY_GET_DISPLAY_AD_FAILED = "Tapjoy HandleGetDisplayAdFailed";
    public static string FE_ADS_TAPJOY_HANDLE_VIDEO_AD_STARTED = "Tapjoy HandleVideoAdStarted";
    public static string FE_ADS_TAPJOY_HANDLE_VIDEO_AD_FAILED = "Tapjoy HandleVideoAdFailed";
    public static string FE_ADS_TAPJOY_HANDLE_VIDEO_AD_COMPLETED = "Tapjoy HandleVideoAdCompleted";
    public static string FE_ADS_TAPJOY_HANDLE_VIEW_TYPE_OPENED = "Tapjoy HandleViewOpened of view type:";
    public static string FE_ADS_TAPJOY_HANDLE_VIEW_TYPE_CLOSED = "Tapjoy HandleViewClosed of view type:";
    public static string FE_ADS_TAPJOY_HANDLE_SHOW_OFFERS_FAILED = "Tapjoy HandleShowOffersFailed";

    public static string FE_ADS_TAPFORTAP_AD_TAPPED = "TapforTap Called ad OnTapAd";
    public static string FE_ADS_TAPFORTAP_AD_RECEIVED = "TapforTap Called ad OnReceiveAd";
    public static string FE_ADS_TAPFORTAP_AD_RECEIVE_FAILED = "TapforTap Called ad OnFailToReceiveAd because of ";
    public static string FE_ADS_TAPFORTAP_APP_WALL_DISMISSED = "TapforTap Called app wall listener OnDismiss";
    public static string FE_ADS_TAPFORTAP_APP_WALL_RECEIVED = "TapforTap Called app wall listener OnReceive";
    public static string FE_ADS_TAPFORTAP_APP_WALL_SHOWED = "TapforTap Called app wall listener OnShow";
    public static string FE_ADS_TAPFORTAP_APP_WALL_TAPPED = "TapforTap Called app wall listener OnTap";
    public static string FE_ADS_TAPFORTAP_APP_WALL_FAILED = "TapforTap Called app wall listener OnFail because: ";
    public static string FE_ADS_TAPFORTAP_INTERSITIAL_DISMISSED = "TapforTap Called interstitial listener OnDismiss";
    public static string FE_ADS_TAPFORTAP_INTERSITIAL_RECEIVED = "TapforTap Called interstitial listener OnReceive";
    public static string FE_ADS_TAPFORTAP_INTERSITIAL_SHOWED = "TapforTap Called interstitial listener OnShow";
    public static string FE_ADS_TAPFORTAP_INTERSITIAL_TAPPED = "TapforTap Called interstitial listener OnTap";
    public static string FE_ADS_TAPFORTAP_INTERSITIAL_FAILED = "TapforTap Called interstitial listener OnFail";

    public static string FE_FACEBOOK_LOGON = "Facebook logon";
    public static string FE_FACEBOOK_LOGON_FAILED = "Facebook logon failed";
    public static string FE_FACEBOOK_LOGOFF = "Facebook logoff";
    public static string FE_FACEBOOK_POST_MESSAGE = "Facebook post message";
    public static string FE_FACEBOOK_POST_MESSAGE_FAIL = "Facebook post message fail";
    public static string FE_FACEBOOK_POST_SCORE = "Facebook post score";
    public static string FE_FACEBOOK_POST_SCORE_FAIL = "Facebook post score fail";
    public static string FE_FACEBOOK_GET_SCORE = "Facebook get score";
    public static string FE_FACEBOOK_GET_SCORE_FAIL = "Facebook get score fail";
    public static string FE_FACEBOOK_INVITE_FRIENDS = "Facebook invite friends";
    public static string FE_FACEBOOK_INVITE_FRIENDS_FAIL = "Facebook invite friends fail";
    public static string FE_FACEBOOK_GET_USER_DATA = "Facebook get user data";
    public static string FE_FACEBOOK_GET_USER_DATA_FAIL = "Facebook get user data fail";
    public static string FE_FACEBOOK_GET_FRIENDS_DATA = "Facebook get friends data";
    public static string FE_FACEBOOK_GET_FRIENDS_DATA_FAIL = "Facebook get friends data fail";
    public static string FE_FB_UD = "FB UD";
    public static string FE_FB_FD = "FB FD";

    public static string FE_GPG_LOGON = "Google Play Games sign on";
    public static string FE_GPG_LOGON_FAIL = "Google Play Games sign on fail";
    public static string FE_GPG_SCORE_POST = "Google Play Games score post";
    public static string FE_GPG_SCORE_POST_FAIL = "Google Play Games score post fail";
    public static string FE_GPG_STORE_CLOUD_BALANCE = "Store cloud balance";
    public static string FE_GPG_STORE_CLOUD_BALANCE_FAIL = "Store cloud balance fail";
    public static string FE_GPG_STORE_CLOUD_PRO_VERSION = "Store cloud pro version purchase";
    public static string FE_GPG_STORE_CLOUD_PRO_VERSION_FAIL = "Store cloud pro version purchase fail";
    public static string FE_GPG_RESTORE_CLOUD_BALANCE = "Restore cloud balance";
    public static string FE_GPG_RESTORE_CLOUD_BALANCE_FAIL = "Restore cloud balance fail";
    public static string FE_GPG_RESTORE_CLOUD_PRO_VERSION = "Restore cloud pro version purchase";
    public static string FE_GPG_RESTORE_CLOUD_PRO_VERSION_FAIL = "Restore cloud pro version purchase fail";

    public static string FE_USER_GENDER_EVENT = "User gender";
    public static string FEP_USER_GENDER_MALE = "Male";
    public static string FEP_USER_GENDER_FEMALE = "Female";

    public static string FE_USER_YEAR_BORN = "User year born";
}


using UnityEngine;
using System.Collections;

/**
 * Class for managing the display of the roadmap and other info like time elapsed,
 * round number, and number of cards left on the TV monitor.
 */
public class RoadmapManager : MonoBehaviour
{
    public Texture2D japaneseTexture;
    public Texture2D englishTexture;
    public Material displayMaterial;
    public Texture2D canvas;
    public Texture2D[] tieTextures = new Texture2D[8];
    public Texture2D bankerTexture;
    public Texture2D playerTexture;
    public Texture2D bankerPairTexture;
    public Texture2D playerPairTexture;
    public Texture2D naturalTexture;
    public Texture2D _0;
    public Texture2D _1;
    public Texture2D _2;
    public Texture2D _3;
    public Texture2D _4;
    public Texture2D _5;
    public Texture2D _6;
    public Texture2D _7;
    public Texture2D _8;
    public Texture2D _9;
    public Texture2D _colon;

    public class RoadStop
    { // For 1 record (cell) on the Road
        public int x = 0;
        public int y = 0;
        public GameState.BetType type;
        public int numTies = 0;
        public bool hadNatural = false;
        public bool playerPair = false;
        public bool bankerPair = false;
        public int column = 0;
    }

    public class DigitStop
    { // For 1 character on the monitor (digits and :)
        public int x = 0;
        public int y = 0;
        public int number = 0;
        public Texture2D texture;
        public DigitStopType type;
    }

    public enum DigitStopType {
        Player,
        Banker,
        Tie,
        Natural,
        PlayerPair,
        BankerPair,
        CardsRemaining,
        RoundNumber
    }

    public void resetRoadmap() {
        Debug.Log ("Resetting Roadmap Manager");
        bigRoad.Clear();
        digitStops.Clear();
        currentRow = 0;
        currentColumn = 1;
        consecutiveTies = 0;
        numCards = 417; // 8 decks x 52 cards = 416 cards, + 1 because we minus 1 automatically initially
        numCards = 417; // 8 decks x 52 cards = 416 cards, + 1 because we minus 1 automatically initially
        numRounds = -1;
        numPlayerWins = -1;
        numBankerWins = -1;
        numTieWins = -1;
        numNaturals = -1;
        numPlayerPairs = -1;
        numBankerPairs = -1;
        DestroyImmediate(displayMaterial.mainTexture);
        DestroyImmediate(canvas);
        Start ();

        // TODO: ::TESTING::
        //StartCoroutine(DoTests());
    }

    // Internals
    private ArrayList bigRoad = new ArrayList ();
    private Hashtable digitStops = new Hashtable ();

    private RoadStop lastBigRoadStop {
        get {
            // Return the last stop on the Big Road
            if (bigRoad != null && bigRoad.Count >= 1) {
                return (RoadStop)bigRoad [bigRoad.Count - 1];
            }
            //Debug.LogWarning ("Returning empty RoadStop item because the Big Road is empty");
            return new RoadStop ();
        }
    }

    // Roadmap stuff
    private static readonly int BIG_ROAD_START_X = 63;
    private static readonly int BIG_ROAD_START_Y = 268;
    private static readonly int BIG_ROAD_STEP_X = 51;
    private static readonly int BIG_ROAD_STEP_Y = 45;
    private static readonly int BIG_ROAD_ROWS = 6;
    private static readonly int BIG_ROAD_COLUMNS = 18;
    private int currentRow = 0;
    private int currentColumn = 1;
    private int consecutiveTies = 0;

    private static readonly int CARDS_START_X = 686;
    private static readonly int CARDS_START_Y = 360;
    private int numCards = 417; // 8 decks x 52 cards = 416 cards, + 1 because we minus 1 automatically initially

    private static readonly int ROUND_START_X = 686;
    private static readonly int ROUND_START_Y = 315;
    private int numRounds = -1;

    private static readonly int PLAYER_START_X = 390;
    private static readonly int PLAYER_START_Y = 457;
    private int numPlayerWins = -1;

    private static readonly int BANKER_START_X = 390;
    private static readonly int BANKER_START_Y = 430;
    private int numBankerWins = -1;

    private static readonly int TIE_START_X = 390;
    private static readonly int TIE_START_Y = 400;
    private int numTieWins = -1;

    private static readonly int NATURAL_START_X = 390;
    private static readonly int NATURAL_START_Y = 370;
    private int numNaturals = -1;

    private static readonly int PPAIR_START_X = 390;
    private static readonly int PPAIR_START_Y = 340;
    private int numPlayerPairs = -1;

    private static readonly int BPAIR_START_X = 390;
    private static readonly int BPAIR_START_Y = 310;
    private int numBankerPairs = -1;


    // Use this for initialization
    void Start ()
    {
        // Set the background texture based on the locale
        if (Utils.isJapanese ()) {
            Debug.Log ("Setting Japanese background texture for the roadmap display");
            canvas = new Texture2D (japaneseTexture.width, japaneseTexture.height);
            canvas.SetPixels (japaneseTexture.GetPixels ());
            canvas.Apply ();
            displayMaterial.mainTexture = canvas;
        } else {
            Debug.Log ("Setting English background texture for the roadmap display");
            canvas = new Texture2D (englishTexture.width, englishTexture.height);
            canvas.SetPixels (englishTexture.GetPixels ());
            canvas.Apply ();
            displayMaterial.mainTexture = canvas;
        }

        // Register ourselves with the Game State
        GameState.Instance.roadmapManager = this;

        setRoundNumber(1);
        setCardsRemaining();
        setPlayerWins();
        setBankerWins();
        setTieWins();
        setNaturals();
        setPlayerPairs();
        setBankerPairs();

        redrawCanvas ();
    }

    #region test
    IEnumerator DoTests() {
        // TODO: ::TESTING CODE::
        //return;
        yield return new WaitForSeconds(4f);

        for (int i = 0; i < 420; i++) {
            setLatestResult((Random.Range (0, 2) == 1 ? GameState.BetType.Player : GameState.BetType.Banker), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  
            yield return new WaitForSeconds(0f);
        }

        float wait = 0.0f;
        // >> BEGIN 1st FULL SCREEN
        /*
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        // overhang

        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        
        // << EOF 1st FULL SCREEN
        //
        //
        // *********************************************************************
        //
        // >> BEGIN 2nd FULL SCREEN

        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
       
        // overhang

        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Banker, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        setLatestResult(GameState.BetType.Player, (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false), (Random.Range (0, 2) == 1 ? true : false));  yield return new WaitForSeconds(wait);
        */

        // << EOF 2nd FULL SCREEN
        
        // <<< EOF TESTING
    }
    #endregion

    /**
     * Set the latest stop on the road.
     */
    public void setLatestResult (GameState.BetType betType, bool hadNatural, bool playerPair, bool bankerPair)
    {
        Debug.Log ("Setting latest result on roadmaps to: " + betType);

        // Package this latest result in a class that represents a single result on the roadmap (I call it a RoadStop)
        RoadStop stop = new RoadStop ();
        stop.hadNatural = hadNatural;
        stop.playerPair = playerPair;
        stop.bankerPair = bankerPair;
        stop.type = betType;

        // TODO: ::TESTING CODE::
        /*
        tstCnt++;
        if (tstCnt < 8)
            stop.type = GameState.BetType.Player;
         */
        bool shift = true;
        if (betType == GameState.BetType.Tie)
            shift = false;
        if (currentRow < BIG_ROAD_ROWS-1 && 
            betType == lastBigRoadStop.type)
            shift = false; // because we just go down, not along
        if (currentColumn == BIG_ROAD_COLUMNS-1) //if (currentColumn % (BIG_ROAD_COLUMNS-1) == 0)
            shift = false; // last column (most right one) wasn't getting filled with stops sometimes

        if (shift)
            leftShiftBigRoad ();

        updateBigRoad (stop);

        // TODO: implement other roads like small road, cockroach road etc...
    }

    /**
     * Set the number of cards remaining
     */
    public void setCardsRemaining() {
        Debug.Log ("Setting number of remaining cards on TV display to: " + (this.numCards-1));
        if (!digitStops.Contains(DigitStopType.CardsRemaining)) {
            DigitStop cardsRemainingDigitStop = new DigitStop();
            cardsRemainingDigitStop.number = --this.numCards;
            cardsRemainingDigitStop.x = CARDS_START_X;
            cardsRemainingDigitStop.y = CARDS_START_Y;
            cardsRemainingDigitStop.type = DigitStopType.CardsRemaining;
            updateDigits(cardsRemainingDigitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.CardsRemaining]).number = --this.numCards;
            updateDigits(((DigitStop) digitStops[DigitStopType.CardsRemaining]));
        }
        redrawCanvas();
    }

    /**
     * Set the round number (number of games)
     */
    public void setRoundNumber(int roundNumber) {
        Debug.Log ("Setting round number on TV display to: " + roundNumber);
        LogUtils.LogEvent(Consts.FE_ROUND_NUMBER, new string[] { roundNumber+"" }, false);
        if (!digitStops.Contains(DigitStopType.RoundNumber)) {
            DigitStop roundNumberDigitStop = new DigitStop();
            roundNumberDigitStop.number = roundNumber;
            roundNumberDigitStop.x = ROUND_START_X;
            roundNumberDigitStop.y = ROUND_START_Y;
            roundNumberDigitStop.type = DigitStopType.RoundNumber;
            updateDigits(roundNumberDigitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.RoundNumber]).number = roundNumber;
            updateDigits((DigitStop) digitStops[DigitStopType.RoundNumber]);
        }
        this.numRounds = roundNumber;
        redrawCanvas();
    }

    /**
     * Set the number of player wins
     */
    public void setPlayerWins() {
        Debug.Log ("Setting number of player wins on TV display to: " + (numPlayerWins+1));
        if (!digitStops.Contains(DigitStopType.Player)) {
            DigitStop digitStop = new DigitStop();
            digitStop.number = ++numPlayerWins;
            digitStop.x = PLAYER_START_X;
            digitStop.y = PLAYER_START_Y;
            digitStop.type = DigitStopType.Player;
            updateDigits(digitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.Player]).number = ++numPlayerWins;
            updateDigits(((DigitStop) digitStops[DigitStopType.Player]));
        }
        redrawCanvas();
    }

    /**
     * Set the number of banker wins
     */
    public void setBankerWins() {
        Debug.Log ("Setting number of banker wins on TV display to: " + (numBankerWins+1));
        if (!digitStops.Contains(DigitStopType.Banker)) {
            DigitStop digitStop = new DigitStop();
            digitStop.number = ++numBankerWins;
            digitStop.x = BANKER_START_X;
            digitStop.y = BANKER_START_Y;
            digitStop.type = DigitStopType.Banker;
            updateDigits(digitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.Banker]).number = ++numBankerWins;
            updateDigits((DigitStop) digitStops[DigitStopType.Banker]);
        }
        redrawCanvas();
    }

    /**
     * Set the number of tie wins
     */
    public void setTieWins() {
        Debug.Log ("Setting number of tie wins on TV display to: " + (numTieWins+1));
        if (!digitStops.Contains(DigitStopType.Tie)) {
            DigitStop digitStop = new DigitStop();
            digitStop.number = ++numTieWins;
            digitStop.x = TIE_START_X;
            digitStop.y = TIE_START_Y;
            digitStop.type = DigitStopType.Tie;
            updateDigits(digitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.Tie]).number = ++numTieWins;
            updateDigits((DigitStop) digitStops[DigitStopType.Tie]);
        }
        redrawCanvas();
    }

    /**
     * Set the number of naturals
     */
    public void setNaturals() {
        Debug.Log ("Setting number of naturals on TV display to: " + (numNaturals+1));
        if (!digitStops.Contains(DigitStopType.Natural)) {
            DigitStop digitStop = new DigitStop();
            digitStop.number = ++numNaturals;
            digitStop.x = NATURAL_START_X;
            digitStop.y = NATURAL_START_Y;
            digitStop.type = DigitStopType.Natural;
            updateDigits(digitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.Natural]).number = ++numNaturals;
            updateDigits(((DigitStop) digitStops[DigitStopType.Natural]));
        }
        redrawCanvas();
    }

    /**
     * Set the number of player pairs
     */
    public void setPlayerPairs() {
        Debug.Log ("Setting number of player pairs on TV display to: " + (numPlayerPairs+1));
        if (!digitStops.Contains(DigitStopType.PlayerPair)) {
            DigitStop digitStop = new DigitStop();
            digitStop.number = ++numPlayerPairs;
            digitStop.x = PPAIR_START_X;
            digitStop.y = PPAIR_START_Y;
            digitStop.type = DigitStopType.PlayerPair;
            updateDigits(digitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.PlayerPair]).number = ++numPlayerPairs;
            updateDigits((DigitStop) digitStops[DigitStopType.PlayerPair]);
        }
        redrawCanvas();
    }

    /**
     * Set the number of banker pairs
     */
    public void setBankerPairs() {
        Debug.Log ("Setting number of banker pairs on TV display to: " + (numBankerPairs+1));
        if (!digitStops.Contains(DigitStopType.BankerPair)) {
            DigitStop digitStop = new DigitStop();
            digitStop.number = ++numBankerPairs;
            digitStop.x = BPAIR_START_X;
            digitStop.y = BPAIR_START_Y;
            digitStop.type = DigitStopType.BankerPair;
            updateDigits(digitStop);
        } else {
            ((DigitStop) digitStops[DigitStopType.BankerPair]).number = ++numBankerPairs;
            updateDigits((DigitStop) digitStops[DigitStopType.BankerPair]);
        }
        redrawCanvas();
    }

    private int tstCnt = 0;

    // Scroll the Big Road left if we've run out of columns
    private void leftShiftBigRoad ()
    {
        if (currentColumn < BIG_ROAD_COLUMNS - 1)
            return; // don't need to left shift until all columns have been populated

        Debug.Log ("Left-shifting the Big Road");
        ArrayList stopsToRemove = new ArrayList ();
        foreach (RoadStop stop in bigRoad) {
            stop.x -= (BIG_ROAD_START_X-12);
            stop.column--;

            // If the RoadStop has left-shifted off the left-side of the roadmap, then remove it
            if (stop.x < BIG_ROAD_START_X)
                stopsToRemove.Add (stop);
        }
        foreach (RoadStop stopToRemove in stopsToRemove) {
            bigRoad.Remove (stopToRemove);
        }
    }

    void updateBigRoad (RoadStop stop)
    {
        //Debug.Log ("Updating big roadmap to: " + stop.type);
        currentRow++;

        //if (stop.type == GameState.BetType.Banker)
        //    Debug.Log ("Im testing");

        if (bigRoad.Count == 0) {
            // This is the first round of this shoe
            Debug.Log ("First stop on the Big Road");
            stop.x = BIG_ROAD_START_X;
            stop.y = BIG_ROAD_START_Y;
            stop.column = 1;
        } else {
            // Position of the next stop depends on the last result
            if (stop.type == GameState.BetType.Tie) {
                // Tie goes in the same place as the last stop
                stop.x = lastBigRoadStop.x;
                stop.y = lastBigRoadStop.y;
                stop.column = currentColumn;
                currentRow--;
            } else if (numPlayerWins == 0 && numBankerWins == 0 && lastBigRoadStop.type == GameState.BetType.Tie) {
                // Tie goes in the same place as the last stop
                stop.x = lastBigRoadStop.x;
                stop.y = lastBigRoadStop.y;
                stop.column = currentColumn;
                currentRow--;
            } else {
                if (stop.type == lastBigRoadStop.type) {
                    if (currentRow >= BIG_ROAD_ROWS) {
                        // If we've gone off the bottom of the grid, then continue on right along the bottom row
                        stop.x = lastBigRoadStop.x + BIG_ROAD_STEP_X;
                        stop.y = lastBigRoadStop.y;
                        currentColumn++;
                        stop.column = currentColumn;
                        currentRow = BIG_ROAD_ROWS;
                    } else {
                        // Move down one cell if win type is the same as the last round
                        stop.x = lastBigRoadStop.x;
                        stop.y = lastBigRoadStop.y - BIG_ROAD_STEP_Y;
                        stop.column = currentColumn;
                    }
                } else {
                    // Move to the right one cell if win type is different to the last round
                    stop.x = lastBigRoadStop.x + BIG_ROAD_STEP_X;
                    currentColumn++;
                    stop.column = currentColumn;
                    stop.y = BIG_ROAD_START_Y;

                    // This result is different to the last, so we've moved back to the top row
                    currentRow = 0;
                }
            }
        }

        if (stop.type == GameState.BetType.Tie) {
            consecutiveTies++;
            stop.numTies = consecutiveTies;

            if (stop.numTies > 8) // only record up to 8 consecutive tie wins... hardly any chance of more than that. Did so because it was easier to prepare 8 rotated textures than rotate a Texture2D dynamically
                stop.numTies = 8;
        } else {
            consecutiveTies = 0;
        }

        // Add the road to the roadmap array
        bigRoad.Add (stop);

        redrawCanvas ();
    }

    /**
     * It's not the most efficient to redraw every single symbol/character after each game,
     * but it was easier to redraw the canvas before applying it as a texture otherwise the UV
     * stuff made it hard to align where the next RoadStop should be placed (as it scales/skews stuff)
     * once the whole texture is applied to the 3D world.
     */
    void redrawCanvas ()
    {
        // Cleanup
        DestroyImmediate(displayMaterial.mainTexture);
        DestroyImmediate(canvas);

        if (Utils.isJapanese ()) {
            if (numRounds <= 1) Debug.Log ("Setting Japanese background texture for the roadmap display");
            canvas = new Texture2D (japaneseTexture.width, japaneseTexture.height);
            canvas.SetPixels (japaneseTexture.GetPixels ());
        } else {
            if (numRounds <= 1) Debug.Log ("Setting English background texture for the roadmap display");
            canvas = new Texture2D (englishTexture.width, englishTexture.height);
            canvas.SetPixels (englishTexture.GetPixels ());
        }

        foreach (RoadStop stop in bigRoad) {
            // Draw the corresponding symbol for player, banker or tie
            if (stop.type == GameState.BetType.Player) {
                //Debug.Log ("Adding Player to roadmap; offset X: " + stop.x + ", offset Y: " + stop.y);
                addTexture (playerTexture, stop.x, stop.y);
            } else if (stop.type == GameState.BetType.Banker) {
                //Debug.Log ("Adding Banker to roadmap; offset X: " + stop.x + ", offset Y: " + stop.y);
                addTexture (bankerTexture, stop.x, stop.y);
            } else if (stop.type == GameState.BetType.Tie) {
                //Debug.Log ("Adding Tie to roadmap; offset X: " + stop.x + ", offset Y: " + stop.y);
                addTexture (tieTextures [stop.numTies - 1], stop.x, stop.y);
            }
    
            // Add extra info like if we had pairs or were a natural
            if (stop.hadNatural) {
                //Debug.Log ("Adding Natural to roadmap; offset X: " + stop.x + ", offset Y: " + stop.y);
                addTexture (naturalTexture, stop.x, stop.y);
            }
            if (stop.playerPair) {
                //Debug.Log ("Adding PlayerPair to roadmap; offset X: " + stop.x + ", offset Y: " + stop.y);
                addTexture (playerPairTexture, stop.x, stop.y);
            }
            if (stop.bankerPair) {
                //Debug.Log ("Adding BankerPair to roadmap; offset X: " + stop.x + ", offset Y: " + stop.y);
                addTexture (bankerPairTexture, stop.x, stop.y);
            }
        }

        // And the digits for the various stats on the screen...
        foreach (DictionaryEntry de in this.digitStops) {
            // Debug.Log("Adding " + digitStop.number + " to TV display");
            DigitStop digitStop = (DigitStop) de.Value;
            addTexture(digitStop.texture, digitStop.x, digitStop.y);
        }

        // Update results in scene
        canvas.Apply ();
        displayMaterial.mainTexture = canvas;
    }

    // Insert a picture into the main canvas picture at the specified coordinates
    private void addTexture (Texture2D tex2d, int offx, int offy)
    {
        for (int h = 1; h <= tex2d.height; h++) {
            for (int w = 1; w <= tex2d.width; w++) {
                Color pix = tex2d.GetPixel (w, h);
                if (pix.a != 0) // don't overwrite original pixels if this is a transparent pixel
                    canvas.SetPixel (offx + w, offy + h, pix);
            }
        }
    }

    // Convert integer to actual textures by making a combination of our 0-9 texture
    // images and add them to be drawn on the canvas
    private void updateDigits(DigitStop digitStop) {
        int number = digitStop.number;
        Debug.Log ("Converting and inserting " + number + " to digit texture");

        // Count how many digits, and store each right-most digit as we count them
        int i = 1;
        ArrayList digits = new ArrayList ();
        if (number >= 1) {
            for (; number >= 1; i++, number /= 10) {
               //Debug.Log ("Right most digit extraction: " + number + " and " + number % 10);
               digits.Add (number % 10);
            }
        } else {
            i++;
            digits.Add (0);
        }

        // Build the combined textures, e.g. 1+2+3+4 == number of 1234
        digits.Reverse ();
        Texture2D currentDigitTexture, newTotalNumberTexture;
        newTotalNumberTexture = new Texture2D((i-1)*_0.width, _0.height);
        int j = 0;
        foreach (int digit in digits) {
            switch (digit) {
            case 0:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _0;
                break;
            case 1:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _1;
                break;
            case 2:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _2;
                break;
            case 3:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _3;
                break;
            case 4:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _4;
                break;
            case 5:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _5;
                break;
            case 6:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _6;
                break;
            case 7:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _7;
                break;
            case 8:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _8;
                break;
            default:
            case 9:
                //Debug.Log ("j: "+j+", digit: "+digit);
                currentDigitTexture = _9;
                break;
            }

            // Add the single digit texture to the big texture we create with all digits (to express the whole number)
            for (int h = 0; h <= currentDigitTexture.height; h++) {
                for (int w = 0; w <= currentDigitTexture.width; w++) {
                    Color pix = currentDigitTexture.GetPixel (w, h);
                    newTotalNumberTexture.SetPixel (j*_0.width + w, h, pix);
                }
            }
            j++;
        }
        newTotalNumberTexture.Apply();

        // Add the current digit's corresponding texture to be rendered on the canvas
        digitStop.texture = newTotalNumberTexture;
        this.digitStops[digitStop.type] = digitStop;
    }
}
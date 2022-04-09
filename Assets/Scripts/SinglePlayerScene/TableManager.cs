using UnityEngine;
using System.Collections;

public class TableManager : MonoBehaviour
{
    const float NO_NO_FLASH_INTERVAL = 0.3f; // secs
    const int NO_NO_FLASH_TIMES = 3;
    const float YES_YES_FLASH_INTERVAL = 0.05f; // secs
    const int YES_YES_FLASH_TIMES = 3;

    // Reference to the table top plane so we can change the texture
    public GameObject table;
    public Texture tieTexture;
    public Texture playerTexture;
    public Texture playerPairTexture;
    public Texture bankerTexture;
    public Texture bankerPairTexture;
    public Texture normalTexture;
    public Texture noTextTexture;

    // Private variables for doing yes yes or no no texture flashes
    TableTexture prevTexture = TableTexture.Normal;
    TableTexture nextTexture = TableTexture.Normal;
    bool noNoFlash = false;
    bool yesYesFlash = false;
    float lastFlashTime = 0;
    int flashCount = 0;

    public enum TableTexture
    {
        Normal,
        Player,
        Banker,
        Tie,
        BankerPair,
        PlayerPair,
        NoText
    }

    // Use this for initialization
    void Start ()
    {
        Debug.Log ("Initializing Table Manager");

        // Register ourselves with the game state manager
        GameState.Instance.tableManager = this;
    }
 
    // Update is called once per frame
    void Update ()
    {
        // Do texture flashing logic
        if (yesYesFlash) {
            if (flashCount > YES_YES_FLASH_TIMES * 2) {
                nextTexture = TableTexture.Normal;
                setTableTexture (TableTexture.Normal);
                yesYesFlash = false;
                lastFlashTime = 0;
                flashCount = 0;
            } else {
                if (lastFlashTime == 0) {
                    // First flash
                    setTableTexture (nextTexture);
                    flashCount++;
                    lastFlashTime = Time.time;
                } else if ((Time.time - lastFlashTime) > YES_YES_FLASH_INTERVAL) {
                    if (flashCount % 2 == 0) {
                        // Flash "off"
                        setTableTexture (TableTexture.Normal);
                    } else {
                        // Flash again
                        setTableTexture (nextTexture);
                    }
                    flashCount++;
                    lastFlashTime = Time.time;
                }
            }
        } else if (noNoFlash) {
            if (flashCount > NO_NO_FLASH_TIMES * 2) {
                nextTexture = TableTexture.Normal;
                setTableTexture (TableTexture.Normal);
                noNoFlash = false;
                lastFlashTime = 0;
                flashCount = 0;
            } else {
                if (lastFlashTime == 0) {
                    // First flash
                    setTableTexture (nextTexture);
                    flashCount++;
                    lastFlashTime = Time.time;
                } else if ((Time.time - lastFlashTime) > NO_NO_FLASH_INTERVAL) {
                    if (flashCount % 2 == 0) {
                        // Flash "off"
                        setTableTexture (TableTexture.Normal);
                    } else {
                        // Flash again
                        setTableTexture (nextTexture);
                    }
                    flashCount++;
                    lastFlashTime = Time.time;
                }
            }
        }
 
    }

    // Set the table texture
    public void setTableTexture (TableTexture texture)
    {
        if (GameState.Instance.getCurrentBetType() == GameState.BetType.Tie
            && texture != TableTexture.NoText
            && GameState.Instance.camerasManager.tiePlayerCamera.activeSelf
            && GameState.Instance.camerasManager.tieBankerCamera.activeSelf) {
            // Hack to prevent an early call of set the normal table texture overwriting the
            // no text texture set while the Tie cameras are active
            return;
        }
        switch (texture) {
        case TableTexture.Player:
            table.GetComponent<Renderer>().material.mainTexture = playerTexture;
            break;
        case TableTexture.PlayerPair:
            table.GetComponent<Renderer>().material.mainTexture = playerPairTexture;
            break;
        case TableTexture.Banker:
            table.GetComponent<Renderer>().material.mainTexture = bankerTexture;
            break;
        case TableTexture.BankerPair:
            table.GetComponent<Renderer>().material.mainTexture = bankerPairTexture;
            break;
        case TableTexture.Tie:
            table.GetComponent<Renderer>().material.mainTexture = tieTexture;
            break;
        case TableTexture.NoText:
            table.GetComponent<Renderer>().material.mainTexture = noTextTexture;
            break;
        case TableTexture.Normal:
        default:
            table.GetComponent<Renderer>().material.mainTexture = normalTexture;
            break;
        }
    }

    // Do a "no, no!" type flash effect for the texture that corresponds to the specified bet type
    public void doNoNoFlash (GameState.BetType textureToFlash)
    {
        switch (textureToFlash) {
        case GameState.BetType.Player:
        case GameState.BetType.PlayerPair:
            nextTexture = TableTexture.Player;
            break;
        case GameState.BetType.Banker:
        case GameState.BetType.BankerPair:
            nextTexture = TableTexture.Banker;
            break;
        default:
            break;
        }
        noNoFlash = true;
    }

    // Do a "yes, yes!" type flash effect for the texture that corresponds to the specified bet type
    public void doYesYesFlash (GameState.BetType textureToFlash)
    {
        switch (textureToFlash) {
        case GameState.BetType.Player:
            nextTexture = TableTexture.Player;
            break;
        case GameState.BetType.PlayerPair:
            nextTexture = TableTexture.PlayerPair;
            break;
        case GameState.BetType.Banker:
            nextTexture = TableTexture.Banker;
            break;
        case GameState.BetType.BankerPair:
            nextTexture = TableTexture.BankerPair;
            break;
        case GameState.BetType.Tie:
            nextTexture = TableTexture.Tie;
            break;
        default:
            break;
        }
        yesYesFlash = true;
    }
}

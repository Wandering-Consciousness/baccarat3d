using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class manages the state of all the chips
public class ChipsManager : MonoBehaviour
{
    public ArrayList chipsOnTable = new ArrayList ();

    // References to the colliders used to detect where a chip was dropped
    public Collider bankerCollider;
    public Collider bankerPairCollider;
    public Collider playerCollider;
    public Collider playerPairCollider;
    public Collider tieCollider;
    public Collider troffCollider;
    public Collider[] colliders;

    // Troff are the chips that sit in the troff on top of the stack, i.e. the chips players move on to the table
    ArrayList troffChips = new ArrayList ();
    public Chip troffChip1; // $100
    public Chip troffChip2; // $500
    public Chip troffChip3; // $1000
    public Chip troffChip4; // $10,000
    public Chip troffChip5; // $100,000
    public Vector3 lastForcedChangeBetChipPos = new Vector3 (0, 0, 0);

    public GameObject prefabChip1; // $100
    public GameObject prefabChip2; // $500
    public GameObject prefabChip3; // $1000
    public GameObject prefabChip4; // $10,000
    public GameObject prefabChip5; // $100,000

    // For when we rebet the same chips
    public ArrayList clearedChipsList = new ArrayList();
    public int clearedChipsTotalAmount = 0;

    // Use this for initialization
    void Start ()
    {
        Debug.Log ("Initializing Chips Manager");

        // Turn colliders off until there is chip dragging
        //collidersOff ();

        // Initialize the chips at the top of the stack in the troff which are the ones that can be moved
        initTroffChips ();

        // Register ourselves with the game state manager
        GameState.Instance.chipsManager = this;

        // Populate array of colliders
        colliders = new Collider[5];
        colliders[0] = playerCollider;
        colliders[1] = playerPairCollider;
        colliders[2] = bankerCollider;
        colliders[3] = bankerPairCollider;
        colliders[4] = tieCollider;

        // Make copy of troff collider chips so those instances can be used
        // to instantiate new chips for when making new chips to return banker
        // wins which don't include the commission
        prefabChip1 = Instantiate (troffChip1.gameObject,
                        troffChip1.gameObject.transform.position,
                        troffChip1.gameObject.transform.rotation) as GameObject;
                    prefabChip1.transform.parent = troffChip1.transform.parent;
                    prefabChip1.transform.localScale = troffChip1.gameObject.transform.localScale;
        prefabChip2 = Instantiate (troffChip2.gameObject,
                        troffChip2.gameObject.transform.position,
                        troffChip2.gameObject.transform.rotation) as GameObject;
                    prefabChip2.transform.parent = troffChip2.transform.parent;
                    prefabChip2.transform.localScale = troffChip2.gameObject.transform.localScale;
        prefabChip3 = Instantiate (troffChip3.gameObject,
                        troffChip3.gameObject.transform.position,
                        troffChip3.gameObject.transform.rotation) as GameObject;
                    prefabChip3.transform.parent = troffChip3.transform.parent;
                    prefabChip3.transform.localScale = troffChip3.gameObject.transform.localScale;
        prefabChip4 = Instantiate (troffChip4.gameObject,
                        troffChip4.gameObject.transform.position,
                        troffChip4.gameObject.transform.rotation) as GameObject;
                    prefabChip4.transform.parent = troffChip4.transform.parent;
                    prefabChip4.transform.localScale = troffChip4.gameObject.transform.localScale;
        prefabChip5 = Instantiate (troffChip5.gameObject,
                        troffChip5.gameObject.transform.position,
                        troffChip5.gameObject.transform.rotation) as GameObject;
                    prefabChip5.transform.parent = troffChip5.transform.parent;
                    prefabChip5.transform.localScale = troffChip5.gameObject.transform.localScale;
        prefabChip1.name += "BankerWinPrefab";
        prefabChip2.name += "BankerWinPrefab";
        prefabChip3.name += "BankerWinPrefab";
        prefabChip4.name += "BankerWinPrefab";
        prefabChip5.name += "BankerWinPrefab";
        prefabChip1.SetActive(false);
        prefabChip2.SetActive(false);
        prefabChip3.SetActive(false);
        prefabChip4.SetActive(false);
        prefabChip5.SetActive(false);
    }

    // Find the closest chip better area collider to the specified chip
    public Collider FindClosestCollider(GameObject srcChip) {
        //Debug.Log ("srcChip: " + srcChip.name);
        Collider closestCollider = null;
        foreach (Collider hit in colliders) {
            if (hit.name.Equals("BankerPairCollider") || hit.name.Equals("PlayerPairCollider")) {
                // I decided to disable the pair colliders because when a chip was placed just out of bounds on either side of the banker
                // area, it would be best to goto the center of the banker area but instead the banker and player pairs would be calculated
                // as been closer.
                continue;
            }
            //Debug.DrawLine(srcChip.collider.bounds.center, hit.ClosestPointOnBounds(srcChip.transform.position), Color.cyan, 3000f, false);
            //Debug.Log ("hit: " + hit.name);
            if (hit.GetComponent<Collider>() == srcChip.GetComponent<Collider>()) {
                // We're hitting ourselves so ignore
                continue;
            }
            if (!closestCollider) {
               closestCollider = hit;
            }
            // Compares distances
            //Debug.Log ("Vector3.Distance(srcChip.transform.position, hit.transform.position) : " + Vector3.Distance(srcChip.transform.position, hit.transform.position)
            //    + ", Vector3.Distance(srcChip.transform.position, closestCollider.transform.position): " + Vector3.Distance(srcChip.transform.position, closestCollider.transform.position));
            float currentDistance = Vector3.Distance(srcChip.transform.position, closestCollider.ClosestPointOnBounds(srcChip.transform.position));
            float newDistance = Vector3.Distance(srcChip.transform.position, hit.ClosestPointOnBounds(srcChip.transform.position));
            //if (Vector3.Distance(srcChip.transform.position, hit.transform.position) <= Vector3.Distance(srcChip.transform.position, closestCollider.transform.position))
            if (newDistance <= currentDistance)
            {
               closestCollider = hit;
            }
        }
        Debug.Log (closestCollider.name + " is the closest collider to " + srcChip.name);

        // Logic to make sure we don't return the collider that is opposite to the current side being bet on (can't position the chip on player side
        // if there's already another bet on a banker or vice versa)
        if (closestCollider == playerCollider
            && GameState.Instance.getCurrentBetType() == GameState.BetType.Banker) {
            // Force to banker side
             Debug.Log("Returning banker collider as the closest one because that's the current side betted on");
             return bankerCollider;
        } else if (closestCollider == bankerCollider
            && GameState.Instance.getCurrentBetType() == GameState.BetType.Player) {
            // Force to player side
            Debug.Log("Returning player collider as the closest one because that's the current side betted on");
            return playerCollider;
        }

        return closestCollider;
    }

    // Update is called once per frame
    void Update ()
    {
 
    }

    // Display the troff chips
    void initTroffChips ()
    {
        troffChip1.chipsManager = this;
        troffChip1.isTroffChip = true;
        troffChips.Add (troffChip1);
        troffChip2.chipsManager = this;
        troffChip2.isTroffChip = true;
        troffChips.Add (troffChip2);
        troffChip3.chipsManager = this;
        troffChip3.isTroffChip = true;
        troffChips.Add (troffChip3);
        troffChip4.chipsManager = this;
        troffChip4.isTroffChip = true;
        troffChips.Add (troffChip4);
        troffChip5.chipsManager = this;
        troffChip5.isTroffChip = true;
        troffChips.Add (troffChip5);
    }

    // Replace a troff chip. This method can be called after a troff chip has been moved onto the table
    // and its cloned a new one to replace it on the troff (chips on the top of the chip stack in the troff).
    public void replaceTroffChip (Chip oldChip, Chip newChip)
    {
        int indx = troffChips.IndexOf (oldChip);
        if (indx == -1) {
            Debug.LogError ("Cannot replace troff chip! Returned index was -1 for old chip");
        }
        oldChip.isTroffChip = false;
        newChip.isTroffChip = true;
        chipsOnTable.Add (troffChips [indx]); // old troff chip is now a table chip
        troffChips [indx] = newChip;
    }

    // Method to turn colliders on when dragging a chip onto the table.
    // Not used anymore. Was originally for when moving chips but we solved the slow chip moving problem!
    public void collidersOn ()
    {
        bankerCollider.enabled = true;
        playerCollider.enabled = true;
        playerPairCollider.enabled = true;
        bankerPairCollider.enabled = true;
        tieCollider.enabled = true;
        troffCollider.enabled = true;
    }

    // Method to turn off colliders when not dragging a chip onto the table.
    // Not used anymore. Was originally for when moving chips but we solved the slow chip moving problem!
    public void collidersOff ()
    {
        bankerCollider.enabled = false;
        playerCollider.enabled = false;
        playerPairCollider.enabled = false;
        bankerPairCollider.enabled = false;
        tieCollider.enabled = false;
        troffCollider.enabled = false;
    }

    // Rebet the chips from the last round
    public void rebet() {
        Debug.Log ("Rebetting " + clearedChipsList.Count + " chip(s) from last round");
        foreach (GameObject chipGO in clearedChipsList) {
            chipGO.SetActive(true);
            Chip chip = chipGO.GetComponent<Chip>();
            chip.betPlaced = true;
            chip.droppedOnce = true;
            Debug.Log ("Rebetting chip on " + chip.currentBetType + " for $" + chip.chipValue.ToString("n0") +
                ", success bet: " + GameState.Instance.bet(chip.currentBetType, chip.chipValue));
            chipsOnTable.Add(chip);
        }
    }

    // Iterate over all chips currently on the table, subtract their value from the total bet and destroy them
    public void clearAllChipsOnTable ()
    {
        if (chipsOnTable != null) {
            Debug.Log ("Clearing all chips off table");

            foreach (Chip chip in chipsOnTable) {
                if (chip != null) {
                    chip.killSelf (false);
                }
            }

            chipsOnTable.Clear ();
        }

        GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);

        lastForcedChangeBetChipPos = new Vector3 (0, 0, 0);
    }

    // Iterate over all chips currently on the table and destroy the ones excluding the array of winners
    public void clearChipsExcludingWinners (List<GameState.BetType> winners)
    {
        if (chipsOnTable != null) {
            Debug.Log ("Clearing all chips off table excluding winners");

            foreach (Chip chip in chipsOnTable) {
                if (chip != null ) {
                    if (winners.Contains (chip.currentBetType)) {
                        // Break loop if the current chip is placed on the current round's winner
                        continue;
                    }
                    // Otherwise...lets commit seppuku
                    chip.killSelf (false);
                }
            }
        }
    }

    // Enable/disable placing of bets
    public void setPlaceBetsAllowed (bool enabled)
    {
        // Setup the clear and deal buttons
        if (enabled) {
            // Show the clear/deal buttons when placing bets
            // Now they're shown when a bet is placed on the table
//            GameState.Instance.guiControls.clearButtonState = GUIControls.ClearButtonState.Clear;
//            GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Deal;
        } else {
            // Hide the clear/deal buttons when not placing bets
            GameState.Instance.guiControls.clearButtonState = GUIControls.ClearButtonState.Hide;
            GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Hide;
        }

        // Enable/disable gestures
        this.gameObject.GetComponent<DragRecognizer> ().enabled = enabled;

        // Don't allow any new bets
        foreach (Chip troffChip in troffChips) {
            if (troffChip != null) {
                troffChip.enabled = enabled;
            }
        }

        // Don't allow chips on the table already to be moved
        foreach (Chip chip in chipsOnTable) {
            if (chip != null) {
                chip.enabled = enabled;
            }
        }
    }

    public void clearChipsForSqueezing (bool clear)
    {
        if (clear) {
            // Remove any chips kept in memory for rebetting
            foreach (GameObject chip in clearedChipsList) {
                if (chip != null) {
                    //DestroyImmediate(chip.GetComponent<MeshFilter>().mesh);
                    //Destroy(chip);
                }
            }
            clearedChipsList.Clear();
            clearedChipsTotalAmount = 0;
        }

        foreach (Chip chip in chipsOnTable) {
            if (chip == null)
                continue; // skip chips that have been destroyed but are still in the array

            if (clear) {
                // Save the chips about to be cleared so we can rebet them
                // We do so but instantiating a new one with the same position and value
                GameObject clone;
                float heightLift = 0.0f; // don't need anymore as we replace the chips where they were without dropping them
                if (chip.chipValue == 100000) {
                    clone = Instantiate (chip.gameObject,
                        chip.transform.position + new Vector3(0, heightLift, 0),
                        chip.transform.rotation) as GameObject;
                    clone.transform.parent = chip.transform.parent;
                    clone.transform.localScale = chip.gameObject.transform.localScale;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(false);
                    clone.GetComponent<Chip>().currentBetType = chip.currentBetType;
                    clone.GetComponent<Chip>().droppedOnce = true;
                    clone.GetComponent<Chip>().enabled = true;
                    clearedChipsList.Add(clone);
                    clearedChipsTotalAmount += 100000;
                } else if (chip.chipValue == 10000) {
                    clone = Instantiate (chip.gameObject,
                        chip.transform.position + new Vector3(0, heightLift, 0),
                        chip.transform.rotation) as GameObject;
                    clone.transform.parent = chip.transform.parent;
                    clone.transform.localScale = chip.gameObject.transform.localScale;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(false);
                    clone.GetComponent<Chip>().currentBetType = chip.currentBetType;
                    clone.GetComponent<Chip>().droppedOnce = true;
                    clone.GetComponent<Chip>().enabled = true;
                    clearedChipsList.Add(clone);
                    clearedChipsTotalAmount += 10000;
                } else if (chip.chipValue == 1000) {
                    clone = Instantiate (chip.gameObject,
                        chip.transform.position + new Vector3(0, heightLift, 0),
                        chip.transform.rotation) as GameObject;
                    clone.transform.parent = chip.transform.parent;
                    clone.transform.localScale = chip.gameObject.transform.localScale;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(false);
                    clone.GetComponent<Chip>().currentBetType = chip.currentBetType;
                    clone.GetComponent<Chip>().droppedOnce = true;
                    clone.GetComponent<Chip>().enabled = true;
                    clearedChipsList.Add(clone);
                    clearedChipsTotalAmount += 1000;
                } else if (chip.chipValue == 500) {
                    clone = Instantiate (chip.gameObject,
                        chip.transform.position + new Vector3(0, heightLift, 0),
                        chip.transform.rotation) as GameObject;
                    clone.transform.parent = chip.transform.parent;
                    clone.transform.localScale = chip.gameObject.transform.localScale;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(false);
                    clone.GetComponent<Chip>().currentBetType = chip.currentBetType;
                    clone.GetComponent<Chip>().droppedOnce = true;
                    clone.GetComponent<Chip>().enabled = true;
                    clearedChipsList.Add(clone);
                    clearedChipsTotalAmount += 500;
                } else if (chip.chipValue == 100) {
                    clone = Instantiate (chip.gameObject,
                        chip.transform.position + new Vector3(0, heightLift, 0),
                        chip.transform.rotation) as GameObject;
                    clone.transform.parent = chip.transform.parent;
                    clone.transform.localScale = chip.gameObject.transform.localScale;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(false);
                    clone.GetComponent<Chip>().currentBetType = chip.currentBetType;
                    clone.GetComponent<Chip>().droppedOnce = true;
                    clone.GetComponent<Chip>().enabled = true;
                    clearedChipsList.Add(clone);
                    clearedChipsTotalAmount += 100;
                }
            }

            if (chip != null) {
                chip.gameObject.SetActive(!clear);
            }
        }
    }

    // Increase the number of chips on the table to show the winnings
    public void chipsMadness (GameState.BetType winner, int paysout, int wonAmount, int commission)
    {
       ArrayList newChipsOnTable = new ArrayList ();

        if (winner == GameState.BetType.Banker) {
            // As a 5% commission is taken by the house when the banker wins we can't get
            // away with just multiplying the number of chips by the payout value, we need
            // to work out the difference and replace one of the larger chips with a number
            // of chips in smaller denomination to make it realistically look like the 5%
            // was taken away.

            // Create a bunch of new chips.
            // Say the amount won (excluding the 5% commission) is $95.
            // Then we create 9x$10 chips and 5x$100 chip
            int sum = 0;
            GameObject clone = null;
            Debug.Log ("Creating chips for " + winner + " win minus commission");
            Debug.Log ("wonAmount: " + wonAmount + ", commission: " + commission);
            Vector3 victor =  new Vector3 (0.0f, 0.2f, 0.0f);
            Vector3 center = bankerCollider.bounds.center;
            while (sum < wonAmount) {
                victor.y += 0.002f;
                if (wonAmount - sum >= 100000) {
                    clone = Instantiate (prefabChip5.gameObject,
                        center + victor,
                        prefabChip5.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip5.transform.parent;
                    clone.transform.localScale = prefabChip5.gameObject.transform.localScale;
                    sum += 100000;
                    Debug.Log("+$100k chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
                } else if (wonAmount - sum >= 10000) {
                    clone = Instantiate (prefabChip4.gameObject,
                        center + victor,
                        prefabChip4.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip4.transform.parent;
                    clone.transform.localScale = prefabChip4.gameObject.transform.localScale;
                    sum += 10000;
                    Debug.Log("+$10k chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
                } else if (wonAmount - sum >= 1000) {
                    clone = Instantiate (prefabChip3.gameObject,
                        center + victor,
                        prefabChip3.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip3.transform.parent;
                    clone.transform.localScale = prefabChip3.gameObject.transform.localScale;
                    sum += 1000;
                    Debug.Log("+$1000 chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
                } else if (wonAmount - sum >= 500) {
                    clone = Instantiate (prefabChip2.gameObject,
                        center + victor,
                        prefabChip2.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip2.transform.parent;
                    clone.transform.localScale = prefabChip2.gameObject.transform.localScale;
                    sum += 500;
                    Debug.Log("+$500 chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
               } else if (wonAmount - sum >= 100) {
                    clone = Instantiate (prefabChip1.gameObject,
                        center + victor,
                        prefabChip1.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip1.transform.parent;
                    clone.transform.localScale = prefabChip1.gameObject.transform.localScale;
                    sum += 100;
                    Debug.Log("+$100 chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
                } else if (wonAmount - sum >= 10) {
                    clone = Instantiate (prefabChip1.gameObject,
                        center + victor,
                        prefabChip1.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip1.transform.parent;
                    clone.transform.localScale = prefabChip1.gameObject.transform.localScale;
                    clone.GetComponent<Renderer>().materials[1].color = new Color (0xff/255f, 0xd8/255f, 0x12/255f);
                    sum += 10;
                    Debug.Log("+$10 chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
                } else if (wonAmount - sum >= 1) {
                    clone = Instantiate (prefabChip1.gameObject,
                        center + victor,
                        prefabChip1.gameObject.transform.rotation) as GameObject;
                    clone.transform.parent = prefabChip1.transform.parent;
                    clone.transform.localScale = prefabChip1.gameObject.transform.localScale;
                    clone.GetComponent<Renderer>().materials[1].color = new Color (0xd1/255f, 0x0/255f, 0x3f/255f);
                    sum += 1;
                    Debug.Log("+$1 chip: sum = " + sum);
                    clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationY;
                    clone.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
                    clone.GetComponent<Rigidbody>().useGravity = true;
                    clone.SetActive(true);
                    newChipsOnTable.Add(clone.GetComponent<Chip> ());
                } else {
                    break;
                }
            }
            Debug.Log ("Final sum of new banker chips (minus commission): $" + sum.ToString("n0"));
        } else {
            // No banker-win so needn't worry about commission, we can just multiply the chips already on the table
            foreach (Chip chip in chipsOnTable) {
                if (chip != null && chip.currentBetType == winner) {
                    for (int i = 1; i <= paysout; i++) {
                        GameObject clone = Instantiate (chip.gameObject,
                            chip.gameObject.transform.position + new Vector3 (0.0f, 0.01f, 0.01f),
                            chip.gameObject.transform.rotation) as GameObject;
                        clone.transform.parent = chip.gameObject.transform.parent;
                        clone.transform.localScale = chip.gameObject.transform.localScale;
                        clone.name += "Winnings" + i;
                        newChipsOnTable.Add (clone.GetComponent<Chip> ());
                    }
                }
            }
        }

        chipsOnTable.AddRange(newChipsOnTable);
    }

    // Clear all the chips that didn't win anything by animating them to the dealer
    public void clearLoses (List<GameState.BetType> winners)
    {
        foreach (Chip chip in chipsOnTable) {
            if (!winners.Contains(chip.currentBetType)) {
                chip.onLoss ();
            }
        }
    }
}

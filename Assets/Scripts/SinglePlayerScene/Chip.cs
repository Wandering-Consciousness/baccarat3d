using UnityEngine;
using System.Collections;

// This class handles moving a chip onto the table using a drag gesture.
public class Chip : MonoBehaviour
{
    public int chipValue; // id to identify the value of this chip
    public Collider DragPlaneCollider;      // collider used when dragPlaneType is set to DragPlaneType.UseCollider
    public float DragPlaneOffset = 0.0f;    // distance between dragged object and drag constraint plane
    public Camera RaycastCamera;
    public Material material;
    public ChipsManager chipsManager;
    public static bool isShowingTutorials = false;

    // What this chip is currently betting on, banker, player, tie etc...
    public GameState.BetType currentBetType = GameState.BetType.Undefined;
    private GameState.BetType secondGuessCurrentBetType = GameState.BetType.Undefined;

    // are we being dragged?
    bool dragging = false;
    FingerGestures.Finger draggingFinger = null;
    GestureRecognizer gestureRecognizer;
    Vector3 physxDragMove = Vector3.zero;
    public bool isTroffChip = false;
    bool isFlying = false;

    // The next chip to go on top of the stack after this one has been betted
    GameObject clone;
    Material disabledColorMaterial = null;

    // State controlling booleans
    bool dropping = false;
    public bool dropped = false;
    public bool droppedOnce = false;
    bool canBeDestroyed = false;
    bool destroyChipOnFixedUpdate = false;
    bool disabledColorShowing = false;
    public bool betPlaced = false;
    bool doYesYesFlash = false;
    bool doNoNoFlashCozOppositeHand = false;
    bool doNoNoFlashCozOutOfBounds = false;
    float disableColor = 60.0f / 255.0f; // dark blackish
    float enableColor = 167.0f / 255.0f; // light grey
    AudioSource[] audioSources = null;
    public bool ignoreThisDragThisTime = false;
    bool setLastForcedChangeBetChipPos = false;
//    GameObject lastCollidedChip = null;

    public bool Dragging {
        get { return dragging; }
        private set {
            if (dragging != value) {
                dragging = value;

                if (dragging) {
                    GetComponent<Rigidbody>().useGravity = false;
                } else {
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
            }
        }
    }
    ContinuousGesturePhase gesturePhase = ContinuousGesturePhase.None;

    public enum DragPlaneType
    {
        Camera, // drag along a plane parallal to the camera/screen screen (XY)
        UseCollider, // project on the collider specified by dragPlaneCollider
    }

    void Start ()
    {
       // Get the list of attached audio source components.
        // Note changing the order they are attached will effect which sound is played.
        // It doesn't look like Unity has name support for attached components.
        audioSources = gameObject.GetComponents<AudioSource> ();

        enabledChipColor ();

        // Localize our texture if necessary
        if (gameObject.GetComponent<LocalizeChipTextures> () != null) {
            Debug.Log ("Localizing dynamic chip " + gameObject.name + " texture");
            gameObject.GetComponent<LocalizeChipTextures> ().localizeChipTextures ();
        }
    }

    void disabledChipColor ()
    {
        if (material) {
            // Darken the chips color
            disabledColorMaterial = new Material (material);
            disabledColorMaterial.SetColor ("_Color", new Color (disableColor, disableColor, disableColor, 1.0f));
            this.GetComponent<Renderer>().material = disabledColorMaterial;
            disabledColorShowing = true;
        }
    }

    void enabledChipColor ()
    {
        if (material) {
            // Normal the chips color
            material.SetColor ("_Color", new Color (enableColor, enableColor, enableColor, 1.0f));
            this.GetComponent<Renderer>().material = material;
            disabledColorShowing = false;
        }
    }

    // converts a screen-space position to a world-space position constrained to the current drag plane type
    // returns false if it was unable to get a valid world-space position
    public bool ProjectScreenPointOnDragPlane (Vector3 refPos, Vector2 screenPos, out Vector3 worldPos)
    {
       //if (!RaycastCamera)
        RaycastCamera = GameState.Instance.camerasManager.raycastCamera;

        worldPos = refPos;

        if (DragPlaneCollider) {
            Ray ray = RaycastCamera.ScreenPointToRay (screenPos);
            RaycastHit hit;

            if (!DragPlaneCollider.Raycast (ray, out hit, float.MaxValue))
                return false;

            worldPos = hit.point + DragPlaneOffset * hit.normal;
        } else { // DragPlaneType.Camera
            Transform camTransform = RaycastCamera.transform;

            // create a plane passing through refPos and facing toward the camera
            Plane plane = new Plane (-camTransform.forward, refPos);

            Ray ray = RaycastCamera.ScreenPointToRay (screenPos);

            float t = 0;
            if (!plane.Raycast (ray, out t))
                return false;

            worldPos = ray.GetPoint (t);
        }
               
        return true;
    }

    void HandleDrag (DragGesture gesture)
    {
        if (!enabled)
            return;

        if (Dragging && isTroffChip)
            return;

        if (isShowingTutorials) {
            Debug.LogWarning ("Can't move chips while isShowingTutorials == true");
            return;
        }

        if (gesture.Phase == ContinuousGesturePhase.Started) {
            gesturePhase = ContinuousGesturePhase.Started;
            // Hide the 'place bet tutorial'
            GameState.Instance.tutorialHelpManager.placeBets(false);

            // Check that we're in the PlaceBets game state which is when we're allowed to put chips on the table
            if (GameState.Instance.currentState != GameState.State.PlaceBets) {
                Debug.LogWarning ("Cannot move chips because not in PlaceBets state");
                ignoreThisDragThisTime = true;
                return;
            }

            // Check that the player has at least this chips value in their balance before being allowed to move
            // Also check that against any other bets currently on the table
            if (!droppedOnce) {
                if (GameState.Instance.currentBalance < chipValue ||
                    chipValue + GameState.Instance.getCurrentBetValue () > GameState.Instance.currentBalance) {
                    Debug.LogWarning ("Not enough money left to place this $" + chipValue + " chip on the table");
                    Debug.LogWarning ("GameState.Instance.currentBalance: " + GameState.Instance.currentBalance
                                      + ", chipValue: " + chipValue + ", GameState.Instance.getCurrentBetValue: " + GameState.Instance.getCurrentBetValue());
                    GameState.Instance.guiControls.displayMessage (LanguageManager.GetText ("label_insufficient_balance"));
                    GameState.Instance.dealer.playInsuffcientFundsSound();
#if !UNITY_WEBPLAYER
                    Handheld.Vibrate ();
#endif
                    //reset (); // Commented out coz was trying to prevent putting chips on table when weren't supposed to be able to, but it had no effect
                    ignoreThisDragThisTime = true;
                    return;
                }
            }

            // The real calculation of a chips bet isn't done until it actually lands on the table but I found
            // if you quickly throw a big chips onto the table you can bypass the balance-check logic and bet
            // more money than you have. This is a temporary way to add the chips value as a bet as soon
            // as the chip is touched.
            if (!droppedOnce)
                ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount += chipValue;

            // Check that the player isn't exceeding the max bet with this chip
            //if (!droppedOnce || isTroffChip) {
                if (GameState.Instance.getCurrentBetValue () > GameState.MAX_BET) {
                    Debug.LogWarning ("Can't bet more than MAX BET ($" + GameState.MAX_BET +") with $" + chipValue + " chip");
                    GameState.Instance.guiControls.displayMessage (LanguageManager.GetText ("label_max_bet") + " $" + GameState.MAX_BET);
#if !UNITY_WEBPLAYER
					Handheld.Vibrate ();
#endif                    
					ignoreThisDragThisTime = true;
                    // Remove it's temporary bet
                    ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                    if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    return;
                }
            //}

            //Debug.Log ("Started dragging a $" + chipValue + " chip");

            // We don't want funny floating/rotating action happening while dragging which looks like the chip is in outer space
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            // Reset dropped variables in case we were picked up again
            dropping = false;
            dropped = false;
            isFlying = false;
            
            isTroffChip = false;

            // If this chip has already been betted once, we temporarily remove its bet from the pool
            // until its dropped somewhere again
            if (betPlaced) {
                GameState.Instance.unbet (currentBetType, chipValue);
                betPlaced = false;
            }

            if (!droppedOnce) {
                // Make a duplicate of this chip to be added to the stack of chips once this one has been dropped
                // on the table
                //Debug.Log ("Making clone of " + this.gameObject.name);
                clone = Instantiate (this.gameObject, this.gameObject.transform.position, this.gameObject.transform.rotation) as GameObject;
                clone.name = this.gameObject.name;
                clone.transform.parent = this.gameObject.transform.parent;
                clone.transform.localScale = this.gameObject.transform.localScale;
                clone.SetActive (false); // hide until ready
                //clone.GetComponent<Chip>().reset (); // Commented out coz was trying to prevent putting chips on table when weren't supposed to be able to, but it had no effect
                chipsManager.replaceTroffChip (this, clone.GetComponent<Chip> ());
            }

            Dragging = true;
            draggingFinger = gesture.Fingers [0];

            playPickUpChipSound ();

            // Reset current bet type in case the chip has been moved to a different area
            currentBetType = GameState.BetType.Undefined;

            // Raise the chip vertically when player starts dragging it so as to get it out of the troff.
            // Assumption is the chip game object is a rigidbody.
            GetComponent<Rigidbody>().useGravity = false;
            if (!droppedOnce) gameObject.transform.Rotate(new Vector3(-20f, 0, 0));
            GetComponent<Rigidbody>().MovePosition (GetComponent<Rigidbody>().position + new Vector3 (0.0f, 0.035f, (droppedOnce ? .0f : .035f)));
            //physxDragMove += new Vector3 (0.0f, 0.035f, (droppedOnce ? .0f : .055f));
        } else if (gesture.Phase == ContinuousGesturePhase.Ended) {
            gesturePhase = ContinuousGesturePhase.Ended;
            if (!ignoreThisDragThisTime && !isTroffChip) {
                //Debug.Log ("Ended dragging a $" + chipValue + " chip");

                // Switch on useGravity when the drag has finished so it drops down onto the table
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                //rigidbody.isKinematic = false;
                //Debug.Log ("Dragging ended for of the $" + chipValue + " chip");
        
                if (!droppedOnce && clone != null) {
                    // Show the new clone in the troff (the chip that replaces this one)
                    clone.SetActive (true);
                }
        
                // Set the flag to know we're dropping (gravity's doing the work for us)
                dropping = true;
            } else {
                ignoreThisDragThisTime = false;
            }
        } else if (Dragging) {
            if (!ignoreThisDragThisTime) {
                // make sure this is the finger we started dragging with
                if (gesture.Fingers [0] != draggingFinger)
                    return;
    
                if (gesture.Phase == ContinuousGesturePhase.Updated) {
                    Transform tf = transform;
    
                    // figure out our previous screen space finger position
                    Vector3 fingerPos3d, prevFingerPos3d;
    
                    // convert these to world-space coordinates, and compute the amount of motion we need to apply to the object
                    if (ProjectScreenPointOnDragPlane (tf.position, draggingFinger.PreviousPosition, out prevFingerPos3d) &&
                        ProjectScreenPointOnDragPlane (tf.position, draggingFinger.Position, out fingerPos3d)) {
                        Vector3 move = fingerPos3d - prevFingerPos3d;

                        // When using the Gryo or AR cameras the distance between finger and chip was very great
                        if (GameState.Instance.camerasManager.isAR ()) {
                            float fac = 2f;
                            move.x /= fac;
                            move.y /= fac;
                            move.z /= fac;
                        }
                        physxDragMove += move; // this will be used in FixedUpdate() to properly move the rigidbody
                    }
                } else {
                    Dragging = false;
                }
            }
        }

        if (gesture.Phase == ContinuousGesturePhase.Updated)
            gesturePhase = ContinuousGesturePhase.Updated;
        else if (gesture.Phase == ContinuousGesturePhase.None)
            gesturePhase = ContinuousGesturePhase.None;
    }

    // Callback for when iTween shaking a chip placed on an area it cant bet, has finished
    public void flyToPlayerCollider ()
    {
        Debug.Log(gameObject.name + " flying to player collider");
        iTween.MoveTo (gameObject, iTween.Hash ("position", chipsManager.playerCollider.transform.position + new Vector3 (0f, 0.1f, 0f),
                        "time", 0.2f, "onComplete", "enableGravity"));
//        iTween.MoveTo (gameObject, iTween.Hash ("position",
//            (chipsManager.lastForcedChangeBetChipPos == new Vector3 (0, 0, 0) ?
//                (chipsManager.playerCollider.transform.position + new Vector3 (0f, 0.1f, 0f))
//                : (chipsManager.lastForcedChangeBetChipPos + new Vector3 (0f, 0.0f, 0f))),
//                        "time", 0.2f, "onComplete", "enableGravity"));
    }

    // Callback for when iTween shaking a chip placed on an area it cant bet, has finished
    public void flyToBankerCollider ()
    {
        Debug.Log(gameObject.name + " flying to banker collider");
         iTween.MoveTo (gameObject, iTween.Hash ("position", chipsManager.bankerCollider.transform.position + new Vector3 (0f, 0.1f, 0f),
                        "time", 0.2f, "onComplete", "enableGravity"));
//        iTween.MoveTo (gameObject, iTween.Hash ("position",
//            (chipsManager.lastForcedChangeBetChipPos == new Vector3 (0, 0, 0) ?
//                (chipsManager.bankerCollider.transform.position + new Vector3 (0f, 0.1f, 0f))
//                : (chipsManager.lastForcedChangeBetChipPos + new Vector3 (0f, 0.0f, 0f))),
//                        "time", 0.2f, "onComplete", "enableGravity"));
    }

    // Callback for when iTween shaking a chip placed on an area it cant bet, has finished
    public void flyToClosestCollider ()
    {
        isFlying = true;
        Collider closestCollider = chipsManager.FindClosestCollider (gameObject);
        Debug.Log(gameObject.name + " flying to " + closestCollider.name);

        iTween.MoveTo (gameObject, iTween.Hash ("position", closestCollider.transform.position + new Vector3 (0f, 0.1f, 0f),
                        "time", 0.2f, "onComplete", "enableGravity"));

        // Update this chip's current bet type relative to the collider it was forced to
        if ("PlayerCollider".Equals (closestCollider.name)) {
            currentBetType = GameState.BetType.Player;
        } else if ("BankerCollider".Equals (closestCollider.name)) {
            currentBetType = GameState.BetType.Banker;
        } else if ("PlayerPairCollider".Equals (closestCollider.name)) {
            currentBetType = GameState.BetType.PlayerPair;
        } else if ("BankerPairCollider".Equals (closestCollider.name)) {
            currentBetType = GameState.BetType.BankerPair;
        } else if ("TieCollider".Equals (closestCollider.name)) {
            currentBetType = GameState.BetType.Tie;
        }
    }

    public void enableGravity ()
    {
        Debug.Log ("Enabling gravity on chip $"+chipValue);
        GetComponent<Rigidbody>().useGravity = true;
        setLastForcedChangeBetChipPos = true;
        dropping = true;
    }

    void FixedUpdate ()
    {
        if (isTroffChip) {
            return;
        }

        if (dropped && doNoNoFlashCozOppositeHand) { // ALREADY A CHIP DROPPED ON THE OTHER HAND: CAN'T BET BANKER AND PLAYER TOGETHER
            // Give a visual indication that this action is not allowed by showing where the chip should've been placed
            if (GameState.Instance.tableManager != null) {

                // Shake our booty
#if !UNITY_WEBPLAYER
				Handheld.Vibrate ();
#endif                
				GetComponent<Rigidbody>().useGravity = false;
                if (currentBetType == GameState.BetType.Banker) {
                    // Chip was dropped on banker but should've gone on player (or tie etc.)
                    GameState.Instance.tableManager.doNoNoFlash (GameState.BetType.Player);

                    // "Shake" then "Fly" the chip to the center of the collider on the other side
                    iTween.ShakePosition (gameObject, iTween.Hash ("amount", new Vector3 (0.01f, 0.0f, 0.01f), "time", 0.3f,
                        "onComplete", "flyToPlayerCollider"));
                    currentBetType = GameState.BetType.Player;
                } else if (currentBetType == GameState.BetType.Player) {
                    // Chip was dropped on player but should've gone on banker (or tie etc.)
                    GameState.Instance.tableManager.doNoNoFlash (GameState.BetType.Banker);

                    // "Shake" then "Fly" the chip to the center of the collider on the other side
                    iTween.ShakePosition (gameObject, iTween.Hash ("amount", new Vector3 (0.01f, 0.0f, 0.01f), "time", 0.3f,
                        "onComplete", "flyToBankerCollider"));
                    currentBetType = GameState.BetType.Banker;
                }
            }
            doNoNoFlashCozOppositeHand = false;
        } else if (dropped && doNoNoFlashCozOutOfBounds) { // CHIP WAS PLACED IN OUT OF BOUNDS AREA: MOVE TO THE CLOSEST COLLIDER
            if (!isTroffChip) {
                // Give a visual indication that this action is not allowed by showing where the chip should've been placed
                // Shake our booty
                Debug.Log (gameObject.name + " is out of bounds");
                //Handheld.Vibrate ();
                GetComponent<Rigidbody>().useGravity = false;

                // "Shake" then "Fly" the chip to the center of the closest collider
                iTween.ShakePosition (gameObject, iTween.Hash ("amount", new Vector3 (0.01f, 0.0f, 0.01f), "time", 0.3f,
                    "onComplete", "flyToClosestCollider"));
                doNoNoFlashCozOutOfBounds = false;
            }
        } else if (dropped && doYesYesFlash) {
            // Give a visual indication that this action was allowed
            if (GameState.Instance.tableManager != null) {
                GameState.Instance.tableManager.doYesYesFlash (currentBetType);
            }
            doYesYesFlash = false;
        }

        // Determine if we've been dropped in place
        if (dropping && GetComponent<Rigidbody>().velocity.y <= 0) {
            dropping = false;
            isFlying = false;
            dropped = true;
            droppedOnce = true;

            // Remember the position of chips that were dropped after being forced onto the right bet
            if (setLastForcedChangeBetChipPos) {
                chipsManager.lastForcedChangeBetChipPos = gameObject.transform.position;
                setLastForcedChangeBetChipPos = false;
            }

            // Commit suicide if we were told to do so
            if (destroyChipOnFixedUpdate) {
                Debug.Log ("Killing myself ($" + chipValue + ") on FixedUpdate() call!");
                //Debug.Log ("DEBUG0: chipValue " + betPlaced == " + betPlaced");
                //Debug.Log ("DEBUG1: Current bet value == " + GameState.Instance.getCurrentBetValue());
                //Debug.Log ("DEBUG2: Current bet type == " + GameState.Instance.getCurrentBetType());

                if (betPlaced) {
                    GameState.Instance.unbet (currentBetType, chipValue);
                    betPlaced = false;
                } else { //if (!droppedOnce) {
                    // Remove it's temporary bet
                    Debug.LogWarning ("Removing InAir bet of " + chipValue);
//                    Debug.LogWarning ("isFlying = > " +isFlying + "," +
//                                      "dropped = > " +dropped + "," + 
//                                      "droppedOnce = > " +droppedOnce + "," +
//                                      "dropping = > " +dropping + "," +
//                                      "Dragging = > " +Dragging + "," +
//                                      "gesturePhase = > " +gesturePhase.ToString() + "," +
//                                      "isTroffChip = > " + isTroffChip);
                    // Remove it's temporary bet
                    Debug.Log ("Removing disabled chip's temporary bet... Num bets on table: " + GameState.Instance.betsOnTableHash.Count
                               + ", current bet value is: " + GameState.Instance.getCurrentBetValue());
                    ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                    if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    Debug.Log ("...Removed disabled chip's temporary bet... Num bets on table: " + GameState.Instance.betsOnTableHash.Count
                               + ", current bet value is: " + GameState.Instance.getCurrentBetValue());                
                }
                    
                //Debug.Log ("DEBUG3: Current bet value == " + GameState.Instance.getCurrentBetValue());
                //Debug.Log ("DEBUG4: Current bet type == " + GameState.Instance.getCurrentBetType());

                // Bug fix debugging:
                /*
                Debug.Log ("DEBUG5: betsOnTableHash.Count="+GameState.Instance.betsOnTableHash.Count);
                foreach (GameState.BetType bt in GameState.Instance.betsOnTableHash.Keys) {
                    Debug.Log ("betsOnTableHash has " + bt.ToString() + ": val: " + ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[bt]).amount);
                }
                //*/

                DestroyImmediate (this.GetComponent<MeshFilter>().mesh);
                Destroy (this.gameObject);
                return;
            }

            // Notify game state of where this chip was betted on
            if (currentBetType != GameState.BetType.Undefined && !betPlaced) {
                GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
                if (GameState.Instance.bet (currentBetType, chipValue)) {
                    // Bet was successfully placed down
                    betPlaced = true;
                    Debug.Log ("Bet placed on " + currentBetType);
                    doYesYesFlash = true;
                    // Remove it's temporary bet
                    ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                    if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                } else {
                    // Bet was not successfully placed down
                    doNoNoFlashCozOppositeHand = true;
                }
            } else if (!betPlaced) {
                // Here we apply a rudimentary check to find where the chip was dropped (player, banker pair, tie etc.).
                // We first check to see whether we can detect what collider the chip was in, which has the best detection
                // rate, and if we can't detect it via that method we go to the second best guess which was taken note
                // of on collision triggers.
                GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
                if (IsInside (chipsManager.playerCollider, gameObject.transform.position)) {
                    Debug.Log ("Chip is on player");
                    currentBetType = GameState.BetType.Player;
                    if (GameState.Instance.bet (currentBetType, chipValue)) {
                        betPlaced = true;
                        Debug.Log ("Bet placed on " + currentBetType);
                        doYesYesFlash = true;
                        // Remove it's temporary bet
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                        if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                            ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    } else {
                        // Bet not currently allowed here
                        betPlaced = false;
                        doNoNoFlashCozOppositeHand = true;
                    }
                } else if (IsInside (chipsManager.bankerCollider, gameObject.transform.position)) {
                    Debug.Log ("Chip is on banker");
                    currentBetType = GameState.BetType.Banker;
                    if (GameState.Instance.bet (currentBetType, chipValue)) {
                        betPlaced = true;
                        Debug.Log ("Bet placed on " + currentBetType);
                        doYesYesFlash = true;
                        // Remove it's temporary bet
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                        if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                            ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    } else {
                        // Bet not currently allowed here
                        betPlaced = false;
                        doNoNoFlashCozOppositeHand = true;
                    }
                } else if (IsInside (chipsManager.playerPairCollider, gameObject.transform.position)) {
                    Debug.Log ("Chip is on player pair");
                    currentBetType = GameState.BetType.PlayerPair;
                    if (GameState.Instance.bet (currentBetType, chipValue)) {
                        betPlaced = true;
                        Debug.Log ("Bet placed on " + currentBetType);
                        doYesYesFlash = true;
                        // Remove it's temporary bet
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                        if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                            ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    } else {
                        // Bet not currently allowed here
                        betPlaced = false;
                        doNoNoFlashCozOppositeHand = true;
                    }
                } else if (IsInside (chipsManager.bankerPairCollider, gameObject.transform.position)) {
                    Debug.Log ("Chip is on banker pair");
                    currentBetType = GameState.BetType.BankerPair;
                    if (GameState.Instance.bet (currentBetType, chipValue)) {
                        betPlaced = true;
                        Debug.Log ("Bet placed on " + currentBetType);
                        doYesYesFlash = true;
                        // Remove it's temporary bet
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                        if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                            ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    } else {
                        // Bet not currently allowed here
                        betPlaced = false;
                        doNoNoFlashCozOppositeHand = true;
                    }
                } else if (IsInside (chipsManager.tieCollider, gameObject.transform.position)) {
                    Debug.Log ("Chip is on tie");
                    currentBetType = GameState.BetType.Tie;
                    if (GameState.Instance.bet (currentBetType, chipValue)) {
                        betPlaced = true;
                        Debug.Log ("Bet placed on " + currentBetType);
                        doYesYesFlash = true;
                        // Remove it's temporary bet
                        ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount -= chipValue;
                        if (((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount < 0)
                            ((GameState.BetOnTable) GameState.Instance.betsOnTableHash[GameState.BetType.InAir]).amount = 0;
                    } else {
                        // Bet not currently allowed here
                        betPlaced = false;
                        doNoNoFlashCozOppositeHand = true;
                    }
                } else {
//                    Debug.LogWarning ("Failed in first attempt to get chip's location, resorting to second guessing...");
//                    if (secondGuessCurrentBetType != GameState.BetType.Undefined) {
//                        currentBetType = secondGuessCurrentBetType;
//                        if (GameState.Instance.bet (currentBetType, chipValue)) {
//                            betPlaced = true;
//                            Debug.Log ("Second guess was bet was placed on " + currentBetType);
//                            doYesYesFlash = true;
//                        } else {
//                            // Bet not currently allowed here
//                            betPlaced = false;
//                            doNoNoFlashCozOppositeHand = true;
//                        }
//                    } else {
                    // Bet not currently allowed here
                    betPlaced = false;
                    doNoNoFlashCozOutOfBounds = true;
//                    }
                }
            }
        }

        if (Dragging) {
            // use MovePosition() for physics objects
            GetComponent<Rigidbody>().MovePosition (GetComponent<Rigidbody>().position + physxDragMove);

            // reset the accumulated drag amount value 
            physxDragMove = Vector3.zero;
        }

//        if (dropped && lastCollidedChip != null) {
//            // Autostack chips
//            iTween.MoveTo (gameObject, lastCollidedChip.transform.position, 0.1f);
//            lastCollidedChip = null;
//        }
    }


    // Check if point is inside collider
    // Ref: http://answers.unity3d.com/questions/163864/test-if-point-is-in-collider.html
    static public bool IsInside (Collider test, Vector3 point)
    {
        Vector3 center;
        Vector3 direction;
        Ray ray;
        RaycastHit hitInfo;
        bool hit;

        // Use collider bounds to get the center of the collider. May be inaccurate
        // for some colliders (i.e. MeshCollider with a 'plane' mesh)
        center = test.bounds.center;
    
        // Cast a ray from point to center
        direction = center - point;
        ray = new Ray (point, direction);
        hit = test.Raycast (ray, out hitInfo, direction.magnitude);

        // If we hit the collider, point is outside. So we return !hit
        return !hit;
    }

    // Commit suicide if we were told to do so after a few seconds
    IEnumerator SuicideTimerCoroutine ()
    {
        Debug.Log ("Killing myself ($" + chipValue + ")!");
        if (betPlaced) {
            GameState.Instance.unbet (currentBetType, chipValue);
            betPlaced = false;
        }

        yield return new WaitForSeconds(2f);
        GameState.Instance.chipsManager.chipsOnTable.Remove(this);
        DestroyImmediate (this.GetComponent<MeshFilter>().mesh);
        Destroy (this.gameObject);
    }

    void OnDrag (DragGesture gesture)
    {
        HandleDrag (gesture);
    }

    void OnDisable ()
    {
        // if this gets disabled while dragging, make sure we cancel the drag operation
        if (Dragging)
            Dragging = false;
    }

    // Called when this chip was lost and goes to the dealer
    public void onLoss ()
    {
        try {
        if (gameObject != null)
            iTween.MoveTo (gameObject, iTween.Hash (
                        "position", GameState.Instance.dealer.DealersChips.transform.position + new Vector3 (0f, 0f, 0.1f), // plus a little to end at the chip tray in front of dealer
                        "delay", Dealer.dealSpeed,
                        "time", Dealer.dealSpeed,
                        "onComplete", "killSelf"));
        } catch (MissingReferenceException e) {
            Debug.LogWarning ("Chip onLoss exception: " + e.Message);
        }
    }

    public void killSelf ()
    {
        killSelf (false);
    }

    public void killSelf (bool onFixedUpdate)
    {
        // Reset the table texture to normal so as to indicate we're currently not betting on anything
        GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);

        // Disable this chips collider so it doesn't effect any other bodies on its way out
        this.gameObject.GetComponent<Collider>().enabled = false;

        if (onFixedUpdate) {
            // Bool to indicate chip should kill itself on the next FixedUpdate call
            destroyChipOnFixedUpdate = true;
        } else {
            // Destroy after we've hit the floor
            if (!gameObject.activeSelf)
                gameObject.SetActive(true); // need to be true to run the coroutine
            StartCoroutine (SuicideTimerCoroutine ());
        }
    }

    void OnTriggerStay (Collider other)
    {
        //Debug.Log ("OnTriggerStay call for "+other.name+"!!");

        // Do second guess check if the chip was placed on the banker, banker pair, player, player pair or tie area
        if (other.name == "BankerCollider") {
            //Debug.Log ("Collided on Banker!");
            secondGuessCurrentBetType = GameState.BetType.Banker;
            //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Banker);
        } else if (other.name == "PlayerCollider") {
            //Debug.Log ("Collided on Player!");
            secondGuessCurrentBetType = GameState.BetType.Player;
            //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Player);
        } else if (other.name == "BankerPairCollider") {
            //Debug.Log ("Collided on BankerPairCollider!");
            secondGuessCurrentBetType = GameState.BetType.BankerPair;
            //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.BankerPair);
        } else if (other.name == "PlayerPairCollider") {
            //Debug.Log ("Collided on PlayerPair!");
            secondGuessCurrentBetType = GameState.BetType.PlayerPair;
            //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.PlayerPair);
        } else if (other.name == "TieCollider") {
            //Debug.Log ("Collided on Tie!");
            secondGuessCurrentBetType = GameState.BetType.Tie;
            //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Tie);
        }

//        if (other.name.ToLower().Contains("chip") && (dropped || dropping)) {
//                Debug.Log ("LANDED on another chip: "+other.name+"!");
//        }
    }

    void OnTriggerEnter (Collider other)
    {
//        if (other.name != null && other.name.ToLower().Contains("chip")) {
//            // Take note of the last collided chip so we can autostack
//            lastCollidedChip = other.gameObject;
//        } else
        if (other.name == "TroffCollider") {
            // The first time the chip passes through the troff collider marks its step over the threshold.
            if (!canBeDestroyed && dragging) {
                //// Commented out because decided to do away with the troff collider to cancel chips for the meantime
                Debug.Log ("Chip can now be destroyed");
                canBeDestroyed = true;
            } else if (dragging && !isFlying) {
                // If it returns now to the troff now, it'll be killed.
                //// Commented out because decided to do away with the troff collider to cancel chips for the meantime
                GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
                disabledChipColor ();
                killSelf (true);
            }
        }
        //else if (other.name.Replace ("Collider", "").Contains (currentBetType.ToString ())) {
        //Debug.Log ("Entering " + other.name);
        //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
        //currentBetType = GameState.BetType.Undefined;
        //}
    }

    void OnTriggerExit (Collider other)
    {
//        if (other.name != null && other.name.ToLower().Contains("chip")) {
//            // If we moved away from a chip collider, we won't autostack
//            lastCollidedChip = null;
//        }

        // Change current bet type to unknown if chip has left one of the colliders
        // into an unknown area on the table. We only do this in the case where the chip
        // is leaving a collider that has already been marked as the current bet type.
        // This is to avoid the potential order where the chip enters it's final collider
        // but then we get a subsequent exit trigger from an old collider that would
        // falsely reset the final collider/current bet type to undefined.
        //if (other.name.Replace ("Collider", "").Contains (currentBetType.ToString ())) {
        //Debug.Log ("Exiting " + other.name);
        //GameState.Instance.tableManager.setTableTexture (TableManager.TableTexture.Normal);
        //currentBetType = GameState.BetType.Undefined;
        //}

    }

    void OnCollisionEnter (Collision collision)
    {
        if (collision.gameObject.name.ToLower ().Contains ("felt") || collision.gameObject.name.ToLower ().Contains ("Chip")) {
            // Play a collision sound if hitting other chips or table
            if (chipValue == 100000 && GameState.Instance.currentState == GameState.State.PlaceBets) {
                // Show some umphiness if we dropped a large bet
                //Handheld.Vibrate (); // Vibrating was a bit too much
                playChipThumpSound ();
            } else if (GameState.Instance.currentState == GameState.State.PlaceBets) {
                playChipCollidedSound ();
            }
        }


        // Code to find relative position of collision/collider
//        Vector3 relativePosition = transform.InverseTransformPoint(collision.contacts[0].point);
//
//        if (relativePosition.x > 0) {
//
//        Debug.Log ("The object is to the right");
//
//        } else {
//
//        Debug.Log ("The object is to the left");
//
//        }
//
//        if (relativePosition.y > 0) {
//
//        Debug.Log ("The object is above.");
//
//        } else {
//
//        print ("The object is below.");
//
//        }
//
//        if (relativePosition.z > 0) {
//
//        Debug.Log ("The object is in front.");
//
//        } else {
//
//        Debug.Log ("The object is behind.");
//
//        }
    }

    void playPickUpChipSound ()
    {
        if (audioSources == null) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [0] != null) {
            audioSources [0].Play ();
        }
    }

    float timeLastPlayedChipCollidedSound = 0;
    void playChipCollidedSound ()
    {
        // Avoid a not-so-nice chained together continuous playing of many chips colliding together at the same time
        if (Time.time - timeLastPlayedChipCollidedSound < 0.5f) {
            //Debug.LogWarning ("Avoiding playing too many chip collided sounds at once");
            return;
        }

        if (audioSources == null) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [1] != null) {
            audioSources [1].Play ();
            timeLastPlayedChipCollidedSound = Time.time;
        }
    }

    void playChipThumpSound ()
    {
        if (audioSources == null) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [2] != null && !audioSources [2].isPlaying) {
            audioSources [2].Play ();
        }
    }

    // Reset ourselves to a default state
    public void reset ()
    {
        Debug.Log ("Resetting " + gameObject.name + "'s state");
        dragging = false;
        physxDragMove = Vector3.zero;
        clone = null;
        dropping = false;
        dropped = false;
        droppedOnce = false;
        canBeDestroyed = false;
        destroyChipOnFixedUpdate = false;
        disabledColorShowing = false;
        betPlaced = false;
        doYesYesFlash = false;
        doNoNoFlashCozOppositeHand = false;
        doNoNoFlashCozOutOfBounds = false;
        ignoreThisDragThisTime = false;
        setLastForcedChangeBetChipPos = false;
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This class handles drawing the mesh and textures for a card as well as the methods for squeezing it
public class Card : MonoBehaviour
{
    public CardDef Definition;
    public Collider DragPlaneCollider;      // collider used when dragPlaneType is set to DragPlaneType.UseCollider
    public float DragPlaneOffset = 1.0f;    // distance between dragged object and drag constraint plane
    public Camera RaycastCamera;
    public GameState.BetType cardType;
    AudioSource[] audioSources = null;
    public bool isCuttingCard = false;
    public bool isCutCard = false;
    public GameObject rightBoundaryCardObj = null;
    public Collider lastCutCardCollider = null;
    public GameObject Baccarat3DPlayingCard = null;
    public bool detectCutCardHit = false;
    public bool readyToReturn = false;
    MegaBendWarp squeezeWarper;
    Vector3 squeezeWarperOrigPos = Vector3.zero;
    Vector3 squeezeWarperOrigRot = Vector3.zero;
    static float scalingFactor = 41f;
    float zFactor = 0.001f;

    enum FingerQuadPos
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Unknown
    };

    // Getter/setter for making this card the cut card
    public bool CutCard {
        get { return Definition.markedAsCutCard; }
        set {
            Definition.markedAsCutCard = value;
        }
        /* COMMENT OUT: TODO: re-visit when implement CutCards logic (revive rather)
        get { return isCutCard; }
        set {
            if (value) {
                Debug.Log ("Setting " + Definition.Symbol + " " + Definition.Text + " as the card to move and cut with");

                isCuttingCard = value;
            }
            isCutCard = value;
        }
        */
    }

    CardAtlas Atlas {
        get { return (Definition != null) ? Definition.Atlas : null; }
    }

    CardStock Stock {
        get { return (Definition != null) ? Definition.Stock : null; }
    }

    void Start ()
    {
        if (!RaycastCamera)
            RaycastCamera = Camera.main;

        // Define megafier modifier references
        modObj = this.gameObject.GetComponent<MegaModifyObject> ();
        bendMod = this.gameObject.GetComponent<MegaBend> ();
        //modObj.MeshUpdated();   // Call this after all modifers have been added

        // Now we use a space bend warp for squeezing, not the MegaBend above
        squeezeWarper = this.gameObject.GetComponentInChildren<MegaBendWarp> ();
        MegaWarpBind warpBind = this.gameObject.GetComponent<MegaWarpBind> ();
        if (warpBind != null)
                warpBind.SourceWarpObj = squeezeWarper.gameObject;

        // Get the list of attached audio source components.
        // Note changing the order they are attached will effect which sound is played.
        // It doesn't look like Unity has name support for attached components.
        audioSources = gameObject.GetComponents<AudioSource> ();

        // As Card is instantiated from Prefab we need to get a reference to the box collider on this object
        DragPlaneCollider = this.gameObject.GetComponent<BoxCollider> ();

        // Debug cubes for squeezing
        if (Consts.DEBUG) {
            c1 = GameObject.Find ("Cube1");
            c2 = GameObject.Find ("Cube2");
            cm = GameObject.Find ("CubeM");
            dc1 = GameObject.Find ("DCube1");
            dc2 = GameObject.Find ("DCube2");
        } else {
            c1 = GameObject.Find ("Cube1");
            if (c1 != null)
                c1.gameObject.transform.parent.gameObject.SetActive (false);
        }
    }

    // are we being dragged?
    bool dragging = false;
    FingerGestures.Finger draggingFinger1 = null;
    FingerGestures.Finger draggingFinger2 = null;
    GestureRecognizer gestureRecognizer;
    MegaModifyObject    modObj;
    MegaBend            bendMod;
    float               angle = 0.0f;
    bool m_built = false;
    public bool revealed = false;
    bool meshIsNew = false;
    Vector3 physx1 = Vector3.zero, physx2 = Vector3.zero, physx3 = Vector3.zero;
    Vector3 physxDragMove = Vector3.zero;
    Vector3 cutCardOrigPos = Vector3.zero;
    bool showSymbolsText = false;

    // Start the tutorials
    public void StartSqueezeTutorial ()
    {
        Debug.Log ("Showing the squeeze tutorials with hand gestures");
        if (GameState.Instance.tutorialCounter > 1)
            return;

        // COMMENTING OUT because it wasn't doing much
        // Reset squeeze camera to its original height
        //GameState.Instance.camerasManager.setTutorialSqueezePos(true);

        // Don't allow squeezing (finger gesture input) while showing tutorials
        GameState.Instance.ToggleFingerGestures (false);

        StartCoroutine (StartSqueezeTutorialCoroutine ());
    }

    // Tutorial variables
    readonly float TUTORIAL_SPEED = 3f;
    float squeezeTutorialOrgFloatHolder1 = 0f;
    Vector3 squeezeTutorialOrigPosition = Vector3.zero;
    Vector3 squeezeTutorialOrigRotation = Vector3.zero;
    Vector3 tapRotateSqueezeHandOrigPosition = Vector3.zero;
    Vector3 tapRotateSqueezeHandOrigRotation = Vector3.zero;
    Vector3 singleFingerSqueezeHandOrigPosition = Vector3.zero;
    Vector3 singleFingerSqueezeHandOrigRotation = Vector3.zero;
    Vector3 doubleFingerSqueezeHandOrigPosition = Vector3.zero;
    Vector3 doubleFingerSqueezeHandOrigRotation = Vector3.zero;
    Color squeezeHandOriginalColor;

    // More squeezing variables
    Vector3 cardTop, cardBottom, cardLeft, cardRight;
    bool swapFingers = true;
    bool firstDrag = true;
    float squeezeWarperLastPosVal = 0;
    bool startedLeftSide = false;
    bool startedRightSide = false;
    // Debug cubes
    GameObject c1;
    GameObject c2;
    GameObject cm;
    GameObject dc1;
    GameObject dc2;

    IEnumerator StartSqueezeTutorialCoroutine ()
    {
        // Tutorial stuff
        if (GameState.Instance.tutorialCounter > 1)
            yield break;
        else {
            // We needs this camera to show the speech bubbles
            GameState.Instance.camerasManager.ToggleSpeechBubbleCamera (true);

            isShowingTutorials = true;

            // Cache squeeze warper and hands& original tranforms so we can revert back to them after squeeze tutorial is done
            squeezeTutorialOrigRotation = squeezeWarper.gameObject.transform.rotation.eulerAngles;
            squeezeTutorialOrigPosition = squeezeWarper.gameObject.transform.position;
            tapRotateSqueezeHandOrigPosition = GameState.Instance.dealer.tapRotateSqueezeHand.transform.position;
            tapRotateSqueezeHandOrigRotation = GameState.Instance.dealer.tapRotateSqueezeHand.transform.rotation.eulerAngles;
            singleFingerSqueezeHandOrigPosition = GameState.Instance.dealer.singleFingerSqueezeHand.transform.position;
            singleFingerSqueezeHandOrigRotation = GameState.Instance.dealer.singleFingerSqueezeHand.transform.rotation.eulerAngles;
            doubleFingerSqueezeHandOrigPosition = GameState.Instance.dealer.doubleFingerSqueezeHand.transform.position;
            doubleFingerSqueezeHandOrigRotation = GameState.Instance.dealer.doubleFingerSqueezeHand.transform.rotation.eulerAngles;

            yield return new WaitForSeconds(1); // wait for previous tutorials and clear them
            GameState.Instance.tutorialHelpManager.otherCard (false);
            GameState.Instance.tutorialHelpManager.returnCard (false);

            // START!
            startTapRotateSqueezeTutorial ();
        }
    }

    // Show using tap to rotate the card
    public void startTapRotateSqueezeTutorial ()
    {
        // Using one finger to tap and rotate
        float lobHeight = 0.075f, lobTime = 0.75f;
        GameState.Instance.tutorialHelpManager.doubleTapRotate (true);
        GameState.Instance.dealer.tapRotateSqueezeHand.SetActive (true);
        GameState.Instance.dealer.tapRotateSqueezeHand.GetComponentInChildren<Renderer> ().enabled = true;
        squeezeHandOriginalColor = GameState.Instance.dealer.tapRotateSqueezeHand.GetComponentInChildren<Renderer> ().material.color;
        GameState.Instance.dealer.tapRotateSqueezeHand.GetComponentInChildren<Renderer> ().material.SetColor ("_Color", Color.white);

        // Tap the finger twice
        iTween.MoveBy (GameState.Instance.dealer.tapRotateSqueezeHand, iTween.Hash ("y", lobHeight));
        iTween.MoveBy (GameState.Instance.dealer.tapRotateSqueezeHand, iTween.Hash ("y", lobHeight, "time", lobTime / 4, "easeType", iTween.EaseType.easeOutQuad));
        iTween.MoveBy (GameState.Instance.dealer.tapRotateSqueezeHand, iTween.Hash ("y", -lobHeight, "time", lobTime / 4, "delay", lobTime / 4, "easeType", iTween.EaseType.easeInCubic));
        iTween.MoveBy (GameState.Instance.dealer.tapRotateSqueezeHand, iTween.Hash ("y", lobHeight, "time", lobTime / 4, "delay", 2 * lobTime / 4, "easeType", iTween.EaseType.easeOutQuad));
        iTween.MoveBy (GameState.Instance.dealer.tapRotateSqueezeHand, iTween.Hash ("y", -lobHeight, "time", lobTime / 4, "delay", 3 * lobTime / 4, "easeType", iTween.EaseType.easeInCubic,
            "oncomplete", "squeezeTutorialTapRotateOnComplete", "oncompletetarget", gameObject));
    }

    public void squeezeTutorialTapRotateOnComplete ()
    {
        StartCoroutine (squeezeTutorialTapRotateOnCompleteCoroutine ());
    }

    IEnumerator squeezeTutorialTapRotateOnCompleteCoroutine ()
    {
        // Rotate the card
        iTween.RotateAdd (gameObject, new Vector3 (0, 0, 180), 1f);

        // End showing the tap rotate squeeze tutorial
        yield return new WaitForSeconds(1);
        squeezeWarper.gameObject.transform.RotateAround (gameObject.GetComponent<Collider>().bounds.center, Vector3.up, 180); //  and return the squeeze warper back to its original rotation
        GameState.Instance.tutorialHelpManager.doubleTapRotate (false);
        GameState.Instance.dealer.tapRotateSqueezeHand.GetComponentInChildren<Renderer> ().enabled = false;
        GameState.Instance.dealer.tapRotateSqueezeHand.GetComponentInChildren<Renderer> ().material.SetColor ("_Color", squeezeHandOriginalColor);
        iTween.MoveTo (GameState.Instance.dealer.tapRotateSqueezeHand.gameObject, tapRotateSqueezeHandOrigPosition, 0f);
        iTween.RotateTo (GameState.Instance.dealer.tapRotateSqueezeHand.gameObject, tapRotateSqueezeHandOrigRotation, 0f);
        iTween.MoveTo (squeezeWarper.gameObject, squeezeTutorialOrigPosition, 0f);
        squeezeTutorialOrgFloatHolder1 = 0;
        if (stopTutorials) {
            iTween.Stop ();
            GameState.Instance.tutorialCounter = 2;
            GameState.Instance.tutorialEndedThisSession = true;
            GameState.Instance.camerasManager.ToggleSpeechBubbleCamera (false);
            GameState.Instance.ToggleFingerGestures (true);
            stopTutorials = false;
            isShowingTutorials = false;
        } else {
            // Next tutorial
            yield return new WaitForSeconds(1);
            startSingleFingerSqueezeTutorial ();
        }
    }

    // Show using single fingers to squeeze the card
    public void startSingleFingerSqueezeTutorial ()
    {
        // Using one finger to push and squeeze
        GameState.Instance.tutorialHelpManager.singleFingerSqueeze (true);
        GameState.Instance.dealer.singleFingerSqueezeHand.SetActive (true);
        GameState.Instance.dealer.singleFingerSqueezeHand.GetComponentInChildren<Renderer> ().enabled = true;
        squeezeHandOriginalColor = GameState.Instance.dealer.singleFingerSqueezeHand.GetComponentInChildren<Renderer> ().material.color;
        GameState.Instance.dealer.singleFingerSqueezeHand.GetComponentInChildren<Renderer> ().material.SetColor ("_Color", Color.white);
        iTween.ValueTo (gameObject, iTween.Hash ("from", 0.0f, "to", 0.004f, "time", 3, "delay", 1,
            "onupdate", "squeezeTutorialSingleFingerOnUpdate", "onupdatetarget", gameObject,
            "oncomplete", "squeezeTutorialSingleFingerOnComplete", "oncompletetarget", gameObject
            , "ignoretimescale", true
            ));
    }

    public void squeezeTutorialSingleFingerOnUpdate (float val)
    {
        if (stopTutorials)
            squeezeTutorialSingleFingerOnComplete ();

        // Move the finger
        if (squeezeTutorialOrgFloatHolder1 == 0f) {
            squeezeTutorialOrgFloatHolder1 = GameState.Instance.dealer.singleFingerSqueezeHand.transform.position.z;
        }
        GameState.Instance.dealer.singleFingerSqueezeHand.GetComponent<Rigidbody>().MovePosition (GameState.Instance.dealer.singleFingerSqueezeHand.transform.position + new Vector3 (0, 0, val / 1.75f));

        // Squeeze the card
        squeezeWarper.GetComponent<Rigidbody>().MovePosition (squeezeWarper.transform.position + new Vector3 (0, 0, val / 2));
    }

    public void squeezeTutorialSingleFingerOnComplete ()
    {
        StartCoroutine (squeezeTutorialSingleFingerOnCompleteCoroutine ());
    }

    IEnumerator squeezeTutorialSingleFingerOnCompleteCoroutine ()
    {
        // End showing the single finger squeeze tutorial
        yield return new WaitForSeconds(1);
        GameState.Instance.tutorialHelpManager.singleFingerSqueeze (false);
        GameState.Instance.dealer.singleFingerSqueezeHand.GetComponentInChildren<Renderer> ().enabled = false;
        GameState.Instance.dealer.singleFingerSqueezeHand.GetComponentInChildren<Renderer> ().material.SetColor ("_Color", squeezeHandOriginalColor);
        iTween.MoveTo (GameState.Instance.dealer.singleFingerSqueezeHand.gameObject, singleFingerSqueezeHandOrigPosition, 0f);
        iTween.RotateTo (GameState.Instance.dealer.singleFingerSqueezeHand.gameObject, singleFingerSqueezeHandOrigRotation, 0f);
        iTween.MoveTo (squeezeWarper.gameObject, squeezeTutorialOrigPosition, 0f);
        squeezeTutorialOrgFloatHolder1 = 0;
        if (stopTutorials) {
            iTween.Stop ();
            GameState.Instance.tutorialCounter = 2;
            GameState.Instance.tutorialEndedThisSession = true;
            GameState.Instance.camerasManager.ToggleSpeechBubbleCamera (false);
            GameState.Instance.ToggleFingerGestures (true);
            stopTutorials = false;
            isShowingTutorials = false;
        } else {
            // Next tutorial
            startDoubleFingerSqueezeTutorial ();
        }
    }

    // Show using two fingers to squeeze the card
    public void startDoubleFingerSqueezeTutorial ()
    {
        GameState.Instance.dealer.doubleFingerSqueezeHand.SetActive (true);
        Renderer[] handsRenderers = GameState.Instance.dealer.doubleFingerSqueezeHand.GetComponentsInChildren<Renderer> ();
        handsRenderers [0].enabled = true; // hand 1
        handsRenderers [1].enabled = true; // hand 2
        handsRenderers [0].material.SetColor ("_Color", Color.white);
        handsRenderers [1].material.SetColor ("_Color", Color.white);

        // Rotate the squeeze warper so we can squeeze from the long side
        iTween.RotateBy (squeezeWarper.gameObject, new Vector3 (0, -90f / 360f, 0), 0f);
        iTween.MoveBy (squeezeWarper.gameObject, new Vector3 (0, 0, 0.1f), 0f);

        // Using one finger to push and squeeze
        GameState.Instance.tutorialHelpManager.doubleFingerSqueeze (true);
        iTween.ValueTo (gameObject, iTween.Hash ("from", 0.0f, "to", 0.0015f, "time", 3, "delay", 0.5f,
            "onupdate", "squeezeTutorialDoubleFingerOnUpdate", "onupdatetarget", gameObject,
            "oncomplete", "squeezeTutorialDoubleFingerOnComplete", "oncompletetarget", gameObject
            , "ignoretimescale", true
            ));
    }

    public void squeezeTutorialDoubleFingerOnUpdate (float val)
    {
        if (stopTutorials)
            squeezeTutorialDoubleFingerOnComplete ();

        // Move the fingers
        GameState.Instance.dealer.doubleFingerSqueezeHand.GetComponent<Rigidbody>().MovePosition (GameState.Instance.dealer.doubleFingerSqueezeHand.transform.position + new Vector3 (-val * 2f, 0, 0));

        // Squeeze the card
        squeezeWarper.GetComponent<Rigidbody>().MovePosition (squeezeWarper.transform.position + new Vector3 (-val, 0, 0));
    }

    public void squeezeTutorialDoubleFingerOnComplete ()
    {
        StartCoroutine (squeezeTutorialDoubleFingerOnCompleteCoroutine ());
    }

    IEnumerator squeezeTutorialDoubleFingerOnCompleteCoroutine ()
    {
        // End showing the double finger squeeze tutorial
        yield return new WaitForSeconds(2);
        GameState.Instance.tutorialHelpManager.doubleFingerSqueeze (false);

        Renderer[] handsRenderers = GameState.Instance.dealer.doubleFingerSqueezeHand.GetComponentsInChildren<Renderer> ();
        handsRenderers [0].enabled = false; // hand 1
        handsRenderers [1].enabled = false; // hand 2
        handsRenderers [0].material.SetColor ("_Color", squeezeHandOriginalColor);
        handsRenderers [1].material.SetColor ("_Color", squeezeHandOriginalColor);

        iTween.MoveTo (GameState.Instance.dealer.doubleFingerSqueezeHand.gameObject, doubleFingerSqueezeHandOrigPosition, 0f);
        iTween.RotateTo (GameState.Instance.dealer.doubleFingerSqueezeHand.gameObject, doubleFingerSqueezeHandOrigRotation, 0f);

        // Move squeeze warper back to its original position
        iTween.MoveTo (squeezeWarper.gameObject, squeezeTutorialOrigPosition, 0f);
        iTween.RotateTo (squeezeWarper.gameObject, squeezeTutorialOrigRotation, 0f);
        squeezeTutorialOrgFloatHolder1 = 0;

        // Tell the player to try and squeeze themself
        GameState.Instance.tutorialHelpManager.startSqueezing(true);

        // End of all how-to-squeeze tutorials
        GameState.Instance.tutorialCounter = 2; // 3 & above effectively ends tutorials until the user presses BEGINNERS MODE again
        GameState.Instance.tutorialEndedThisSession = true;
        GameState.Instance.camerasManager.ToggleSpeechBubbleCamera (false);
        GameState.Instance.ToggleFingerGestures (true);
        stopTutorials = false;
        isShowingTutorials = false;
    }

    private bool stopTutorials = false;
    public static bool isShowingTutorials = false;

    public void StopSqueezeTutorial ()
    {
        Debug.Log ("Stopping any squeeze tutorials in progress");
        stopTutorials = true;
        isShowingTutorials = false;

        // COMMENTING OUT because it wasn't doing much
        // Reset squeeze camera to its original position
        //GameState.Instance.camerasManager.setTutorialSqueezePos(false);

        // Re-enable finger gestures
        GameState.Instance.ToggleFingerGestures (true);
    }

    class SubMesh
    {
        public List<Vector3> VertexList = new List<Vector3> ();
        public List<int> IndexList = new List<int> ();
        public List<Vector2> TexCoords = new List<Vector2> ();
        public List<Color> Colors = new List<Color> ();

        public SubMesh ()
        {
            VertexList = new List<Vector3> ();
            IndexList = new List<int> ();
        }

        public void AddVertex (Vector3 v, Vector2 uv, Color color)
        {
            VertexList.Add (v);
            TexCoords.Add (uv);
            Colors.Add (color);
        }

        public void AddTriangle (int a, int b, int c)
        {
            IndexList.Add (a);
            IndexList.Add (b);
            IndexList.Add (c);
        }
    }

    class CardMesh
    {
        public List<Material> Materials = new List<Material> ();
        public List<SubMesh> MeshList = new List<SubMesh> ();

        public SubMesh NewSubMesh (Material mat)
        {
            SubMesh mesh = new SubMesh ();
            MeshList.Add (mesh);
            Materials.Add (mat);
            return mesh;
        }

        public List<Vector3> GetCombinedVertices ()
        {
            if (MeshList.Count > 0) {
                List<Vector3> combined = new List<Vector3> ();
                foreach (SubMesh m in MeshList) {
                    combined.AddRange (m.VertexList);
                }
                return combined;
            }
            return MeshList [0].VertexList;
        }

        public List<Vector2> GetCombinedTexCoords ()
        {
            if (MeshList.Count > 0) {
                List<Vector2> combined = new List<Vector2> ();
                foreach (SubMesh m in MeshList) {
                    combined.AddRange (m.TexCoords);
                }
                return combined;
            }
            return MeshList [0].TexCoords;
        }
     
        public List<Color> GetCombinedColors ()
        {
            if (MeshList.Count > 0) {
                List<Color> combined = new List<Color> ();
                foreach (SubMesh m in MeshList) {
                    combined.AddRange (m.Colors);
                }
                return combined;
            }
            return MeshList [0].Colors;
        }
    }
 
    Vector2 UV (CardShape shape, float tu, float tv)
    {
        float u = Mathf.Lerp (shape.Min.x, shape.Max.x, tu);
        float v = Mathf.Lerp (shape.Min.y, shape.Max.y, tv);
        return new Vector2 (u, v);
    }
 
    void BuildCorner3 (SubMesh data, CardShape shape, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        CardStock Stock = Definition.Stock;
        int smooth = Stock.Smooth;
     
        int vbase = data.VertexList.Count;
        this.VertexUV (data, shape, v0, UV (shape, 0, 1));
        this.VertexUV (data, shape, v1, UV (shape, 0, 1));
        this.VertexUV (data, shape, v2, UV (shape, 1, 0));
     
        float deltaAngle = 0.5f * Mathf.PI / (smooth + 1);
        //int prev = vbase + 1;
        Vector3 vy = v1 - v0;
        Vector3 vx = v2 - v0;
        for (int i=0; i<=smooth; ++i) {
            if (i < smooth) {
                float angle = (i + 1) * deltaAngle;
                float tu = Mathf.Sin (angle);
                float tv = Mathf.Cos (angle);
                Vector3 xyz = v0 + tu * vx + tv * vy;
                Vector2 uv = UV (shape, tu, tv);
                data.AddVertex (xyz, uv, ((isCutCard || Definition.markedAsCutCard) ? Color.red : Color.white));
            }
            int vn = vbase + 3 + i;
            int vprev = (i == 0) ? vbase + 1 : vn - 1;
            int vnext = (i < smooth) ? vn : vbase + 2;
            data.AddTriangle (vbase, vprev, vnext);
            //prev = vn;
        }
    }
 
    void VertexUV (SubMesh data, CardShape shape, Vector3 pos, Vector2 uv)
    {
        data.AddVertex (pos, uv, Color.white);
    }

    void Square (SubMesh data, CardShape shape, Vector3 pos, Vector2 size, Color color, bool flip)
    {
        int vi = data.VertexList.Count;
        // originals:
        //Vector2 t0 = shape.UV0;
        //Vector2 t2 = shape.UV1;
        //Vector2 t4 = shape.UV2;
        //Vector2 t6 = shape.UV3;

        // Texture coordinates:
        // method 1:
        // t0     t1     t2
        //
        // t7     t8     t3
        //
        // t6     t5     t4
        // method 2:
        // t4     t5     t6
        //
        // t3     t8     t7
        //
        // t2     t1     t0
        // t0->t4, t1->t5, t2->t6, t7->t3, t8->t8, t3->t7, t6->t2, t5->t1, t4->t0

        // Modified by Simon 2013/04:
        // Calculate extra texcoords so we can texture one image over multiple triangles
        Vector2 t0, t2, t4, t6;
        Vector2 t1, t3, t5, t7, t8;
        t0 = shape.UV0;
        t2 = shape.UV1;
        t4 = shape.UV2;
        t6 = shape.UV3;
        t1.x = (t0.x + t2.x) / 2f;
        t1.y = t0.y;
        t3.y = (t2.y + t4.y) / 2f;
        t3.x = t2.x;
        t5.x = (t4.x + t6.x) / 2f;
        t5.y = t4.y;
        t7.y = (t0.y + t6.y) / 2f;
        t7.x = t0.x;
        t8.x = (t0.x + t2.x) / 2f;
        t8.y = (t0.y + t6.y) / 2f;

        if (!flip) {
            data.AddVertex (pos + new Vector3 (-size.x, +size.y, 0), t0, color); // top-left
            data.AddVertex (pos + new Vector3 (0, +size.y, 0), t1, color); // top-center
            data.AddVertex (pos + new Vector3 (+size.x, +size.y, 0), t2, color); // top-right
            data.AddVertex (pos + new Vector3 (+size.x, 0, 0), t3, color); // right-center
            data.AddVertex (pos + new Vector3 (+size.x, -size.y, 0), t4, color); // bottom-right
            data.AddVertex (pos + new Vector3 (0, -size.y, 0), t5, color); // bottom-center
            data.AddVertex (pos + new Vector3 (-size.x, -size.y, 0), t6, color); // bottom-left
            data.AddVertex (pos + new Vector3 (-size.x, 0, 0), t7, color); // left-center
            data.AddVertex (pos + new Vector3 (0, 0, 0), t8, color); // center
        } else {
            data.AddVertex (pos + new Vector3 (-size.x, +size.y, 0), t4, color); // top-left
            data.AddVertex (pos + new Vector3 (0, +size.y, 0), t5, color); // top-center
            data.AddVertex (pos + new Vector3 (+size.x, +size.y, 0), t6, color); // top-right
            data.AddVertex (pos + new Vector3 (+size.x, 0, 0), t7, color); // right-center
            data.AddVertex (pos + new Vector3 (+size.x, -size.y, 0), t0, color); // bottom-right
            data.AddVertex (pos + new Vector3 (0, -size.y, 0), t1, color); // bottom-center
            data.AddVertex (pos + new Vector3 (-size.x, -size.y, 0), t2, color); // bottom-left
            data.AddVertex (pos + new Vector3 (-size.x, 0, 0), t3, color); // left-center
            data.AddVertex (pos + new Vector3 (0, 0, 0), t8, color); // center
        }

        data.AddTriangle (vi, vi + 1, vi + 8); // 1
        data.AddTriangle (vi, vi + 8, vi + 7); // 2
        data.AddTriangle (vi + 1, vi + 2, vi + 3); // 3
        data.AddTriangle (vi + 1, vi + 3, vi + 8); // 4
        data.AddTriangle (vi + 8, vi + 3, vi + 4); // 5
        data.AddTriangle (vi + 8, vi + 4, vi + 5); // 6
        data.AddTriangle (vi + 7, vi + 8, vi + 5); // 7
        data.AddTriangle (vi + 7, vi + 5, vi + 6); // 8

    }

    // Draw square with X polygons wide, Y polygons high and calculate the correct texture coords as well
    void SquareXYUp (SubMesh data, CardShape shape, Vector3 pos, Vector2 size, Color color, float X, float Y, bool hackAlignmenting)
    {
        // Alignment hacks
        if (hackAlignmenting)
            pos.y -= 3 * size.y;

        Vector3 orgPos = new Vector3 (pos.x, pos.y, pos.z);
        int vi = data.VertexList.Count;

        // Bring the face textures just slightly above the white card
        float zv = -zFactor;

//        Debug.Log ("pos.x:"+pos.x+", pos.y:"+pos.y);

        // Center the texture in alignment to the card paper back
//        pos.x -= size.x*X;
//        pos.y -= size.y*Y;

        float ddx = (shape.UV1.x - shape.UV0.x) / X;
        float ddy = (shape.UV1.y - shape.UV2.y) / Y;
        Vector2 t0, t2, t4, t6;
        Vector2 t1, t3, t5, t7, t8;
        t0 = new Vector2 (shape.UV0.x, shape.UV2.y + ddy); // top-left
        t2 = new Vector2 (shape.UV0.x + ddx, shape.UV2.y + ddy); // top-right
        t4 = new Vector2 (shape.UV0.x + ddx, shape.UV2.y); // bottom-right
        t6 = new Vector2 (shape.UV3.x, shape.UV2.y); // bottom-left
        float tdx = 0;
        float tdy = 0;
//        Debug.Log ("shape.UV0.x:"+shape.UV0.x+", shape.UV0.y:"+shape.UV0.y);
//        Debug.Log ("shape.UV1.x:"+shape.UV1.x+", shape.UV1.y:"+shape.UV1.y);
//        Debug.Log ("shape.UV2.x:"+shape.UV2.x+", shape.UV2.y:"+shape.UV2.y);
//        Debug.Log ("shape.UV3.x:"+shape.UV3.x+", shape.UV3.y:"+shape.UV3.y);
//        Debug.Log ("ddx:"+ddx+", ddy:"+ddy);
        for (int i = 1; i <= Y; i++) {
            if (i == 1) {
                pos = new Vector3 (pos.x, pos.y, 0);
            }

            for (int j = 1; j <= X; j++) {
                t0 = new Vector2 (t0.x + tdx, t0.y); // top-left
                t2 = new Vector2 (t2.x + tdx, t2.y); // top-right
                t4 = new Vector2 (t4.x + tdx, t4.y); // bottom-right
                t6 = new Vector2 (t6.x + tdx, t6.y); // bottom-left
//                Debug.Log ("t0.x:"+t0.x+", t2.x:"+t2.x+", t4.x:"+t4.x+", t6.x:"+t6.x+  ", t0.y:"+t0.y+", t2.y:"+t2.y+", t4.y:"+t4.y+", t6.y:"+t6.y);
                tdx = ddx;
                t1.x = (t0.x + t2.x) / 2f;
                t1.y = t0.y;
                t3.y = (t2.y + t4.y) / 2f;
                t3.x = t2.x;
                t5.x = (t4.x + t6.x) / 2f;
                t5.y = t4.y;
                t7.y = (t0.y + t6.y) / 2f;
                t7.x = t0.x;
                t8.x = (t0.x + t2.x) / 2f;
                t8.y = (t0.y + t6.y) / 2f;

                Vector3 nv;
                int tt = i;
                nv = pos + new Vector3 (0, 2 * size.y, zv); // top-left
                data.AddVertex (nv, t0, (i == tt ? color : Color.red));
//                Debug.Log ("nv1:"+nv);
                nv = pos + new Vector3 (size.x, 2 * size.y, zv); // top-center
                data.AddVertex (nv, t1, (i == tt ? color : Color.red)); // top-center
//                Debug.Log ("nv2:"+nv);
                nv = pos + new Vector3 (2 * size.x, 2 * size.y, zv); // top-right
                data.AddVertex (nv, t2, (i == tt ? color : Color.red)); // top-right
//                Debug.Log ("nv3:"+nv);
                nv = pos + new Vector3 (2 * size.x, size.y, zv); // right-center
                data.AddVertex (nv, t3, (i == tt ? color : Color.red)); // right-center
//                Debug.Log ("nv4:"+nv);
                nv = pos + new Vector3 (2 * size.x, 0, zv); // bottom-right
                data.AddVertex (nv, t4, (i == tt ? color : Color.red)); // bottom-right
//                Debug.Log ("nv5:"+nv);
                nv = pos + new Vector3 (size.x, 0, zv); // bottom-center
                data.AddVertex (nv, t5, (i == tt ? color : Color.red)); // bottom-center
//                Debug.Log ("nv6:"+nv);
                nv = pos + new Vector3 (0, 0, zv); // bottom-left
                data.AddVertex (nv, t6, (i == tt ? color : Color.red)); // bottom-left
//                Debug.Log ("nv7:"+nv);
                nv = pos + new Vector3 (0, size.y, zv); // left-center
                data.AddVertex (nv, t7, (i == tt ? color : Color.red)); // left-center
//                Debug.Log ("nv8:"+nv);
                nv = pos + new Vector3 (size.x, size.y, zv); // center
                data.AddVertex (nv, t8, (i == tt ? color : Color.red)); // center
//                Debug.Log ("nv9:"+nv);

                data.AddTriangle (vi, vi + 1, vi + 8); // 1
                data.AddTriangle (vi, vi + 8, vi + 7); // 2
                data.AddTriangle (vi + 1, vi + 2, vi + 3); // 3
                data.AddTriangle (vi + 1, vi + 3, vi + 8); // 4
                data.AddTriangle (vi + 8, vi + 3, vi + 4); // 5
                data.AddTriangle (vi + 8, vi + 4, vi + 5); // 6
                data.AddTriangle (vi + 7, vi + 8, vi + 5); // 7
                data.AddTriangle (vi + 7, vi + 5, vi + 6); // 8

                vi += 9;
                pos -= new Vector3 (-2 * size.x, 0, 0);
            }
            pos = new Vector3 (orgPos.x, orgPos.y + (i * 2 * size.y), 0);
//            Debug.LogWarning ("**** tdy:"+tdy+", i:"+i+", ddy:"+ddy);
            tdy = ddy;
//            Debug.LogError ("**** tdy:"+tdy+", i:"+i+", ddy:"+ddy);
            tdx = 0;
            t0 = new Vector2 (shape.UV0.x, t0.y + tdy); // top-left
            t2 = new Vector2 (shape.UV0.x + ddx, t2.y + tdy); // top-right
            t4 = new Vector2 (shape.UV0.x + ddx, t4.y + tdy); // bottom-right
            t6 = new Vector2 (shape.UV3.x, t6.y + tdy); // bottom-left
        }
    }

    // Draw square with X polygons wide, Y polygons high and calculate the correct texture coords as well
    void SquareXYDown (SubMesh data, CardShape shape, Vector3 pos, Vector2 size, Color color, float X, float Y, bool hackAlignmenting)
    {
        // Alignment hacks
        if (hackAlignmenting)
            pos.y += 2 * size.y;

        Vector3 orgPos = new Vector3 (pos.x, pos.y, pos.z);
        int vi = data.VertexList.Count;

        // Bring the face textures just slightly above the white card
        float zv = -zFactor;

//        Debug.Log ("pos.x:"+pos.x+", pos.y:"+pos.y);

        // Center the texture in alignment to the card paper back
//        pos.x -= size.x*X;
//        pos.y -= size.y*Y;

        float ddx = (shape.UV1.x - shape.UV0.x) / X;
        float ddy = (shape.UV1.y - shape.UV2.y) / Y;
        Vector2 t0, t2, t4, t6;
        Vector2 t1, t3, t5, t7, t8;
        t0 = new Vector2 (shape.UV0.x, shape.UV2.y + ddy); // top-left
        t2 = new Vector2 (shape.UV0.x + ddx, shape.UV2.y + ddy); // top-right
        t4 = new Vector2 (shape.UV0.x + ddx, shape.UV2.y); // bottom-right
        t6 = new Vector2 (shape.UV3.x, shape.UV2.y); // bottom-left
        float tdx = 0;
        float tdy = 0;
//        Debug.Log ("shape.UV0.x:"+shape.UV0.x+", shape.UV0.y:"+shape.UV0.y);
//        Debug.Log ("shape.UV1.x:"+shape.UV1.x+", shape.UV1.y:"+shape.UV1.y);
//        Debug.Log ("shape.UV2.x:"+shape.UV2.x+", shape.UV2.y:"+shape.UV2.y);
//        Debug.Log ("shape.UV3.x:"+shape.UV3.x+", shape.UV3.y:"+shape.UV3.y);
//        Debug.Log ("ddx:"+ddx+", ddy:"+ddy);
        for (int i = (int)Y; i >= 1; i--) {
            // The bottom row of the pip was half-centered to the right for some reason we we readjust it here
            if (i == Y) {
                pos = new Vector3 (pos.x, pos.y, 0);
            }

            for (int j = 1; j <= X; j++) {
                t0 = new Vector2 (t0.x + tdx, t0.y); // top-left
                t2 = new Vector2 (t2.x + tdx, t2.y); // top-right
                t4 = new Vector2 (t4.x + tdx, t4.y); // bottom-right
                t6 = new Vector2 (t6.x + tdx, t6.y); // bottom-left
//                Debug.Log ("t0.x:"+t0.x+", t2.x:"+t2.x+", t4.x:"+t4.x+", t6.x:"+t6.x+  ", t0.y:"+t0.y+", t2.y:"+t2.y+", t4.y:"+t4.y+", t6.y:"+t6.y);
                tdx = ddx;
                t1.x = (t0.x + t2.x) / 2f;
                t1.y = t0.y;
                t3.y = (t2.y + t4.y) / 2f;
                t3.x = t2.x;
                t5.x = (t4.x + t6.x) / 2f;
                t5.y = t4.y;
                t7.y = (t0.y + t6.y) / 2f;
                t7.x = t0.x;
                t8.x = (t0.x + t2.x) / 2f;
                t8.y = (t0.y + t6.y) / 2f;

                Vector3 nv;
                int tt = i;
                nv = pos + new Vector3 (0, 2 * size.y, zv); // top-left
                data.AddVertex (nv, t6, (i == tt ? color : Color.red));
//                Debug.Log ("nv1:"+nv);
                nv = pos + new Vector3 (size.x, 2 * size.y, zv); // top-center
                data.AddVertex (nv, t5, (i == tt ? color : Color.red)); // top-center
//                Debug.Log ("nv2:"+nv);
                nv = pos + new Vector3 (2 * size.x, 2 * size.y, zv); // top-right
                data.AddVertex (nv, t4, (i == tt ? color : Color.red)); // top-right
//                Debug.Log ("nv3:"+nv);
                nv = pos + new Vector3 (2 * size.x, size.y, zv); // right-center
                data.AddVertex (nv, t3, (i == tt ? color : Color.red)); // right-center
//                Debug.Log ("nv4:"+nv);
                nv = pos + new Vector3 (2 * size.x, 0, zv); // bottom-right
                data.AddVertex (nv, t2, (i == tt ? color : Color.red)); // bottom-right
//                Debug.Log ("nv5:"+nv);
                nv = pos + new Vector3 (size.x, 0, zv); // bottom-center
                data.AddVertex (nv, t1, (i == tt ? color : Color.red)); // bottom-center
//                Debug.Log ("nv6:"+nv);
                nv = pos + new Vector3 (0, 0, zv); // bottom-left
                data.AddVertex (nv, t0, (i == tt ? color : Color.red)); // bottom-left
//                Debug.Log ("nv7:"+nv);
                nv = pos + new Vector3 (0, size.y, zv); // left-center
                data.AddVertex (nv, t7, (i == tt ? color : Color.red)); // left-center
//                Debug.Log ("nv8:"+nv);
                nv = pos + new Vector3 (size.x, size.y, zv); // center
                data.AddVertex (nv, t8, (i == tt ? color : Color.red)); // center
//                Debug.Log ("nv9:"+nv);

                data.AddTriangle (vi, vi + 1, vi + 8); // 1
                data.AddTriangle (vi, vi + 8, vi + 7); // 2
                data.AddTriangle (vi + 1, vi + 2, vi + 3); // 3
                data.AddTriangle (vi + 1, vi + 3, vi + 8); // 4
                data.AddTriangle (vi + 8, vi + 3, vi + 4); // 5
                data.AddTriangle (vi + 8, vi + 4, vi + 5); // 6
                data.AddTriangle (vi + 7, vi + 8, vi + 5); // 7
                data.AddTriangle (vi + 7, vi + 5, vi + 6); // 8

                vi += 9;
                pos -= new Vector3 (-2 * size.x, 0, 0);
            }
            pos = new Vector3 (orgPos.x, (orgPos.y - 2.5f * Y * size.y) + (i * 2 * size.y), 0);
//            Debug.LogWarning ("**** tdy:"+tdy+", i:"+i+", ddy:"+ddy);
            tdy = ddy;
//            Debug.LogError ("**** tdy:"+tdy+", i:"+i+", ddy:"+ddy);
            tdx = 0;
            t0 = new Vector2 (shape.UV0.x, t0.y + tdy); // top-left
            t2 = new Vector2 (shape.UV0.x + ddx, t2.y + tdy); // top-right
            t4 = new Vector2 (shape.UV0.x + ddx, t4.y + tdy); // bottom-right
            t6 = new Vector2 (shape.UV3.x, t6.y + tdy); // bottom-left
        }
    }

    void Square4 (SubMesh data, CardShape shape, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Color color,
        bool vflip, bool hflip)
    {
        int vi = data.VertexList.Count;
        Vector2 t0 = shape.UV3;
        Vector2 t1 = shape.UV2;
        Vector2 t2 = shape.UV1;
        Vector2 t3 = shape.UV0;

        if (!vflip) {
            if (hflip) {
                data.AddVertex (v0, t2, color);
                data.AddVertex (v1, t3, color);
                data.AddVertex (v2, t0, color);
                data.AddVertex (v3, t1, color);
            } else {
                data.AddVertex (v0, t3, color);
                data.AddVertex (v1, t2, color);
                data.AddVertex (v2, t1, color);
                data.AddVertex (v3, t0, color);
            }
        } else {
            if (hflip) {
                data.AddVertex (v0, t1, color);
                data.AddVertex (v1, t0, color);
                data.AddVertex (v2, t3, color);
                data.AddVertex (v3, t2, color);
            } else {
                data.AddVertex (v0, t0, color);
                data.AddVertex (v1, t1, color);
                data.AddVertex (v2, t2, color);
                data.AddVertex (v3, t3, color);
            }
        }
        data.AddTriangle (vi, vi + 1, vi + 2);
        data.AddTriangle (vi, vi + 2, vi + 3);
    }

    void Square5 (SubMesh data, CardShape shape, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color color, bool flip)
    {
        int vi = data.VertexList.Count;
        Vector2 t0 = shape.UV0;
        Vector2 t1 = shape.UV1;
        Vector2 t2 = shape.UV2;
        Vector2 t3 = shape.UV3;
        Vector2 t4 = (t0 + t3) / 2;
        if (!flip) {
            data.AddVertex (v0, t3, color);
            data.AddVertex (v1, t2, color);
            data.AddVertex (v2, t1, color);
            data.AddVertex (v3, t0, color);
            data.AddVertex (v4, t4, color);
        } else {
            data.AddVertex (v0, t0, color);
            data.AddVertex (v1, t1, color);
            data.AddVertex (v2, t2, color);
            data.AddVertex (v3, t3, color);
            data.AddVertex (v4, t4, color);
        }
        data.AddTriangle (vi + 4, vi + 0, vi + 1);
        data.AddTriangle (vi + 4, vi + 1, vi + 2);
        data.AddTriangle (vi + 4, vi + 2, vi + 3);
    }
 
    static int[] patternBits = new int []
    {
     0,
     128,
     2 + 8192,
     2 + 128 + 8192,
     1 + 4 + 4096 + 16384,
     1 + 4 + 128 + 4096 + 16384,
     1 + 4 + 64 + 256 + 4096 + 16384,
     1 + 4 + 16 + 64 + 256 + 4096 + 16384,
     1 + 4 + 16 + 64 + 256 + 1024 + 4096 + 16384,
     1 + 4 + 8 + 32 + 128 + 512 + 2048 + 4096 + 16384,
     1 + 4 + 8 + 32768 + 32 + 512 + 65536 + 2048 + 4096 + 16384,
    };
    static Vector2[] patternPos = new Vector2[]
    {
     new Vector2 (0.25f, 0.2f), // 1 top left
     new Vector2 (0.5f, 0.2f),  // 2 top mid
     new Vector2 (0.75f, 0.2f), // 3 top right
     new Vector2 (0.25f, 0.4f), // 4
     new Vector2 (0.5f, 0.3f),  // 5
     new Vector2 (0.75f, 0.4f), // 6
     new Vector2 (0.25f, 0.5f), // 7 center line
     new Vector2 (0.5f, 0.5f),  // 8
     new Vector2 (0.75f, 0.5f), // 9
     new Vector2 (0.25f, 0.6f), // 10 line
     new Vector2 (0.5f, 0.7f),  // 11
     new Vector2 (0.75f, 0.6f), // 12
     new Vector2 (0.25f, 0.8f), // 13 line
     new Vector2 (0.5f, 0.8f),  // 14
     new Vector2 (0.75f, 0.8f), // 15
     new Vector2 (0.5f, 0.25f), // 16 10 up-mid
     new Vector2 (0.5f, 0.75f), // 17 10 down-mid
    };
 
    public bool IsValid ()
    {
        if (Definition != null) {
            if (Definition.Atlas != null) {
                if (Definition.Stock != null && Definition.Stock.DefaultMaterial != null) {
                    return true;
                }
            }
        }
        return false;
    }
 
    static Color GetSymbolColor (string shape)
    {
        return (shape == "Heart" || shape == "Diamond") ? Color.red : Color.black;
    }
 
    // Cache material so that render batching includes multiple cards.
    static Material LastMat;
 
    // Optional support for multiple atlases or textures
    SubMesh GetMesh (CardMesh card, Dictionary<Texture2D,SubMesh> table, CardShape shape)
    {
        if (shape != null) {
            if (table.ContainsKey (shape.Image)) {
                return table [shape.Image];
            }
            Material mat = LastMat;
            if (LastMat == null || LastMat.mainTexture != shape.Image || LastMat.shader != Stock.DefaultMaterial.shader) {
                mat = new Material (Stock.DefaultMaterial); //new Material(Shader.Find("Diffuse"));
                mat.mainTexture = shape.Image;
                LastMat = mat;
            }
            SubMesh newMesh = card.NewSubMesh (mat);
            table [shape.Image] = newMesh;
            return newMesh;
        }
        return null;
    }
 
    public void Rebuild ()
    {
        if (!IsValid ()) {
            Debug.LogError ("The card definition is not valid.");
            return;
        }

        Stock.Validate ();
     
        Dictionary<Texture2D,SubMesh> table = new Dictionary<Texture2D, SubMesh> ();
         
        CardMesh card = new CardMesh ();
        CardShape paper = Definition.Atlas.FindById (Definition.Stock.Paper);
        if (paper == null) {
            Debug.LogError ("Paper does not exist in atlas = " + Definition.Atlas.name + "::" + Definition.Stock.Paper);
            return;
        }
        SubMesh data = GetMesh (card, table, paper);
     
        float x = Stock.Size.x / 2;
        float y = Stock.Size.y / 2;

        float cx = (x - Stock.Border.x) / scalingFactor;
        float cy = (y - Stock.Border.y) / scalingFactor;

//        // Modifications by Simon Apr 2013.
//        // Increase number of polygons in middle square for blend modifier so
//        // we can get a smoother and rounder rollover effect for card squeezing.
//
        Vector3 v0 = new Vector2 (-cx, +cy); // middle
        Vector3 v1 = new Vector2 (+cx, +cy);
        Vector3 v2 = new Vector2 (+cx, -cy);
        Vector3 v3 = new Vector2 (-cx, -cy);
//
//        //   4  5
//        // B 0  1 6
//        //   C  D
//        // A 3  2 7
//        //   9  8
        Vector3 vC = new Vector2 (-cx, 0); // mid
        Vector3 vD = new Vector2 (+cx, 0);
        Vector2 textSize = new Vector2
            (0.175f / scalingFactor, 0.175f / scalingFactor);
        Vector2 symSize = new Vector2 (0.265f, 0.265f);
        float symW = symSize.x * 0.5f;
        float rimX = Mathf.Max (textSize.x, symW);

        CardShape symbol = Atlas.FindById (Definition.Symbol);
        float numPolysWide = 22;
        float numPolysHigh = 33;
        if (symbol == null && !string.IsNullOrEmpty (Definition.Symbol)) {
            Debug.LogError (string.Format ("Symbol shape '{0}' is not defined in atlas.", Definition.Symbol));
        }
        CardShape fullImage = Definition.FullImage ? Atlas.FindById (Definition.Image) : null;
        CardShape halfImage = !Definition.FullImage ? Atlas.FindById (Definition.Image) : null;
        if (fullImage != null) { // image back pieces, e.g. e.g. king, queen, jack
            SubMesh core = GetMesh (card, table, fullImage);
            //SquareXY (core, fullImage, new Vector3((v1.x+v0.x)/2, (v1.y+v2.y)/2), new Vector2((v1.x-v0.x)/2/numPolysWide, (v1.y-v2.y)/2/numPolysHigh), Color.white, numPolysWide, numPolysHigh);
            SquareXYUp (core, fullImage, new Vector2 (v3.x, v3.y), new Vector2 ((v1.x - v0.x) / 2 / numPolysWide, (v1.y - v2.y) / 2 / numPolysHigh), Color.white, numPolysWide, numPolysHigh, false);
            //original:Square4 (core, fullImage, v0, v1, v2, v3, Color.white, false, false);
        } else if (halfImage != null) {
            SubMesh core = GetMesh (card, table, halfImage);
            Vector3 lift = new Vector3 (0, 0, -zFactor);
            Square4 (core, halfImage, v0 + lift, v1 + lift, vD + lift, vC + lift, Color.white, false, false);
            Square4 (core, halfImage, vC + lift, vD + lift, v2 + lift, v3 + lift, Color.white, true, true);
        } else if (Definition.Pattern != 0 && symbol != null) {
            if (Definition.Pattern >= 1 && Definition.Pattern < patternBits.Length) {
                SubMesh core = GetMesh (card, table, symbol);
                Vector2 ssize = symSize / scalingFactor;
                int bits = patternBits [Definition.Pattern];
                float x0 = (-x + Stock.Border.x) / scalingFactor;
                float x1 = (+x - Stock.Border.x) / scalingFactor;
                float y0 = (+y - Stock.Border.y / 3) / scalingFactor;
                float y1 = (-y + Stock.Border.y / 3) / scalingFactor;
                float scalex = 4f;//(Definition.Pattern == 1) ? 1.5f : 1;
                float scaley = 4f;
                //Debug.Log("x0:"+x0+", x1:"+x1+", y0:"+y0+", y1:"+y1);
                for (int b=0; b<17; b++) { // Draw all the symbols on the pip side for 2-10, including A(??)
                    if ((bits & (1 << b)) != 0) {
                        float px = Mathf.Lerp (x0, x1, patternPos [b].x);
                        float py = Mathf.Lerp (y0, y1, patternPos [b].y);
                        //Debug.LogWarning("b:"+b+", px:"+px+", py:"+py);
                        //Square (core, symbol, new Vector3 (px, py, -0.02f), ssize, Color.white, py >= 0 ? false : true);
                        if (py >= 0)
                            SquareXYUp (core, symbol, new Vector3 (px - ssize.x, (px == 0 && py != 0) ? py - (/*mid pip adjustment*/2 * (ssize.y / scaley)/**/) : py, -0.03f), new Vector2 (ssize.x / scalex, ssize.y / scaley), Color.white, scalex, scaley, true);
                        else
                            SquareXYDown (core, symbol, new Vector3 (px - ssize.x, py, -0.03f), new Vector2 (ssize.x / scalex, ssize.y / scaley), Color.white, scalex, scaley, true);
                    }
                }
            } else {
                Debug.LogError (string.Format ("Pattern value '{0}' is not valid.", Definition.Pattern));
            }
        }

        CardShape text = Atlas.FindById (Definition.Text);
        if (text == null && !string.IsNullOrEmpty (Definition.Text)) {
            Debug.LogError (string.Format ("Text shape '{0}' is not defined in atlas.", Definition.Text));
        }
        float xm = 0.5f;
        if (text != null && showSymbolsText) { // side text
            SubMesh sub = GetMesh (card, table, text);
            float x0 = (-x + (Stock.Border.x + rimX) * xm) / scalingFactor;
            float x1 = (+x - (Stock.Border.x + rimX) * xm) / scalingFactor;
            float y0 = (+y - Stock.Border.y / 2 - 0.25f) / scalingFactor;
            float y1 = (-y + Stock.Border.y / 2 + 0.25f) / scalingFactor;
            Color color = GetSymbolColor (Definition.Symbol);
            Square (sub, text, new Vector3 (x0, y0, -zFactor), textSize, color, false);
            Square (sub, text, new Vector3 (x1, y1, -zFactor), textSize, color, true);
        }
        if (symbol != null && showSymbolsText) { // side symbols
            SubMesh sub = GetMesh (card, table, symbol);
            Vector2 ssize = symSize * 0.5f;
            float gapY = ssize.y * 3.5f;
            float x0 = (-x + (Stock.Border.x + rimX) * xm) / scalingFactor;
            float x1 = (+x - (Stock.Border.x + rimX) * xm) / scalingFactor;
            float y0 = (+y - Stock.Border.y / 2 - textSize.y - gapY - ssize.y) / scalingFactor;
            float y1 = (-y + Stock.Border.y / 2 + textSize.y + gapY + ssize.y) / scalingFactor;
            Color color = GetSymbolColor (Definition.Symbol);
            Square (sub, symbol, new Vector3 (x0, y0, -zFactor), ssize / scalingFactor, color, false);
            Square (sub, symbol, new Vector3 (x1, y1, -zFactor), ssize / scalingFactor, color, true);
        }

        Mesh mesh = SetupMesh ();
        mesh.vertices = card.GetCombinedVertices ().ToArray ();
        mesh.triangles = data.IndexList.ToArray ();
        mesh.uv = card.GetCombinedTexCoords ().ToArray ();
        mesh.colors = card.GetCombinedColors ().ToArray ();

        if (card.MeshList.Count > 1) {
            mesh.subMeshCount = card.MeshList.Count;
            int vbase = 0;
            for (int i=1; i<card.MeshList.Count; ++i) {
                SubMesh sub = card.MeshList [i];
                int [] tris = sub.IndexList.ToArray ();
                vbase += card.MeshList [i - 1].VertexList.Count;
                for (int t=0; t<tris.Length; ++t) {
                    tris [t] += vbase;
                }
                mesh.SetTriangles (tris, i);
            }
        }
     
        mesh.RecalculateBounds ();
        ;
        mesh.RecalculateNormals ();
     
        this.GetComponent<Renderer>().sharedMaterials = card.Materials.ToArray ();


        // We now combine the mesh procedurally created above with a prefab mesh and materials
        // of the playing card paper and back pattern
        Baccarat3DPlayingCard.gameObject.SetActive (true);
        CombineMeshes cm = GetComponent<CombineMeshes> ();
        cm.Combine ();
        Baccarat3DPlayingCard.gameObject.SetActive (false);

        // Commented out because we fixed the scaling elsewhere...
        /*
        // Hack to uniformly scale the cards. There was an issue with cards after being created looking distorted when they were rotated onto their
        // side for squeezing so here we make all x y z scale factors the same.
        Vector3 adjustedScale = new Vector3 (gameObject.transform.localScale.z, gameObject.transform.localScale.z, gameObject.transform.localScale.z);
        gameObject.transform.localScale = adjustedScale;
        */

        // If we're marked as the cut card when cutting cards remove the back texture and make the plane red
        if (isCutCard) {
            this.GetComponent<Renderer>().materials [2].mainTexture = null;
            this.GetComponent<Renderer>().materials [2].color = Color.red;
        }
    }

    Mesh SetupMesh ()
    {
        if (this.GetComponent<MeshRenderer> () == null) {
            this.gameObject.AddComponent (typeof(MeshRenderer));
        }
        MeshFilter mf = this.GetComponent<MeshFilter> ();
        if (mf == null) {
            Debug.Log ("Instantiating new mesh filter for card");
            mf = this.gameObject.AddComponent (typeof(MeshFilter)) as MeshFilter;
        } else {
//            Debug.Log ("Mesh filter already instantiated for card");

            // Destroy old mesh to avoid memory leaks (Simon was testing this pre-release Jan 2014)
            DestroyImmediate (mf.mesh);
        }
        Mesh mesh = new Mesh ();
        mf.mesh = mesh;
        meshIsNew = true;
        return mesh;
    }
 
    public void TryBuild ()
    {
        if (!m_built) {
            Rebuild ();
            m_built = true;
        }
    }
 
    // Update is called once per frame
    void Update ()
    {
        TryBuild ();

        if (meshIsNew) {
            // Tell Megafier the card's using a new mesh
            if (modObj != null) {
                modObj.MeshUpdated ();
                meshIsNew = false;
            }
        }

        // COMMENT OUT coz decided we didn't need this kind of squeeze feature
        /*
        if (squeezeWarper != null) {
            if (GUIControls.isFaceDown) {
                // If facedown and squeezing then set mega warp bend angle to 180 so we can see from birds eye (top down)
                squeezeWarper.angle = 180;
            } else {
                // If facedown and squeezing then set mega warp bend angle to 90 for a vertical squeeze effect
                squeezeWarper.angle = 90;
            }
        }
        */
    }

    public bool Dragging {
        get { return dragging; }
        private set {
            if (dragging != value) {
                dragging = value;
            }
        }
    }

    public enum DragPlaneType
    {
        Camera, // drag along a plane parallal to the camera/screen screen (XY)
        UseCollider, // project on the collider specified by dragPlaneCollider
    }

    // converts a screen-space position to a world-space position constrained to the current drag plane type
    // returns false if it was unable to get a valid world-space position
    public bool ProjectScreenPointOnDragPlane (Vector3 refPos, Vector2 screenPos, out Vector3 worldPos)
    {
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
        if (!enabled || revealed)
            return;

        // Extra check logic to ensure we're in the right game state
        if (!isCuttingCard && GameState.Instance.currentState != GameState.State.SqueezeCards) {
//            Debug.LogWarning ("Cannot squeeze card " + gameObject.name + " when not in SqueezeCards game state");
            return;
        }

        if (gesture.Phase == ContinuousGesturePhase.Started) {
            Dragging = true;
            draggingFinger1 = gesture.Fingers [0];
            if (gesture.Fingers.Count > 1) {
                draggingFinger2 = gesture.Fingers [1];

                // After finally getting all the code above to work nicely I still found that sometimes on the very
                // first double finger squeeze for a card that we'd still need to swap fingers so here's a hack at that...
                //if ((finger3d1QuadPos == FingerQuadPos.Unknown || finger3d2QuadPos == FingerQuadPos.Unknown) && firstDrag)
                //if (firstDrag)
                //    swapFingers = true;
                //if (firstDrag)
                //    firstDrag = false;

            } else
                draggingFinger2 = null;

            if (!isCutCard && !Definition.markedAsCutCard) {
                playSqueezingSound ();
            } else {

            }

            // Set the raycast camera to the squeeze camera for smoother squeezing results
            RaycastCamera = Camera.allCameras [0];
        } else if (gesture.Phase == ContinuousGesturePhase.Ended) {
            if (isCuttingCard) {
                // Use let go of cut card, end cutting of cards
                isCuttingCard = false;
                detectCutCardHit = true;
                GameState.Instance.dealer.endCardCut ();
            }
            firstDrag = true;
            startedLeftSide = false;
            startedRightSide = false;
            squeezeWarperLastPosVal = squeezeWarper.transform.position.z;
            Dragging = false;
        } else if (Dragging) {

            //////////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////// BEGIN SQUEEZE... LETS SQUEEZING! ////////////////////////////
            //////////////////////////////////////////////////////////////////////////////////////////

            // make sure this is the finger we started dragging with
            // Commented out because we now offer one and two finger squeezing! Wow, amazing... (I just watched SGU1's Frozen.. A-ma-zing!)
//            if (gesture.Fingers [0] != draggingFinger)
//                return;

            // Rememer the squeeze warpers original position if it's not set so we can reference it for deciding limits
            // and when to return the card after revealing enough
            if (squeezeWarperOrigPos == Vector3.zero) {
                squeezeWarperOrigPos = squeezeWarper.gameObject.transform.position;
                squeezeWarperOrigRot = squeezeWarper.gameObject.transform.rotation.eulerAngles;
            }

            if (gesture.Phase == ContinuousGesturePhase.Updated) {
                Transform tf = transform;

                // figure out our previous screen space finger position
                Vector3 fingerPos3d1, prevFingerPos3d1;
                Vector3 fingerPos3d2, prevFingerPos3d2;

                // convert these to world-space coordinates, and compute the amount of motion we need to apply to the object
                if (ProjectScreenPointOnDragPlane (tf.position, draggingFinger1.PreviousPosition, out prevFingerPos3d1) &&
                    ProjectScreenPointOnDragPlane (tf.position, draggingFinger1.Position, out fingerPos3d1)) {
                    Vector3 move = fingerPos3d1 - prevFingerPos3d1;

                    // If we are squeezing with two fingers, calculate the angle between the two point vectors and rotate
                    // the squeeze space bend warper thing to match
                    if (draggingFinger2 != null
                        && ProjectScreenPointOnDragPlane (tf.position, draggingFinger2.PreviousPosition, out prevFingerPos3d2)
                        && ProjectScreenPointOnDragPlane (tf.position, draggingFinger2.Position, out fingerPos3d2)) {
                        Vector3 move2 = fingerPos3d2 - prevFingerPos3d2;
                        //move2 /= 2;

                         // Determine what quadrant of the screen the fingers are and extrapolate the anticipated side from whence thoust
                        // will squeeze.
                        // In laymen's terms; divide the screen into 4 sections, and if for eg. the two fingers are in the bottom  two
                        // squares we can assume the user's doing a squeeze from the bottom end of the card. If the two fingers are in both
                        // the right-hand side squares, then assume a squeeze from the right-hand long side. etc.
                        if (firstDrag) {
                            Vector3 center = GetComponent<Collider>().bounds.center;
                            Vector3 cardDimens = GetComponent<Collider>().bounds.size;
                            cardTop = cardBottom = cardLeft = cardRight = center;
                            cardTop.z += cardDimens.z / 2;
                            cardBottom.z -= cardDimens.z / 2;
                            cardLeft.x -= cardDimens.x / 2;
                            cardRight.x += cardDimens.x / 2;
                            // Find where the first finger touched the card
                            FingerQuadPos finger3d1QuadPos = FingerQuadPos.Unknown;
                            if (fingerPos3d1.x >= cardLeft.x && fingerPos3d1.x <= center.x
                                 && fingerPos3d1.z >= cardBottom.z && fingerPos3d1.z <= center.z) {
                                finger3d1QuadPos = FingerQuadPos.BottomLeft;
                            } else if (fingerPos3d1.x >= cardLeft.x && fingerPos3d1.x <= center.x
                                 && fingerPos3d1.z > center.z && fingerPos3d1.z <= cardTop.z) {
                                finger3d1QuadPos = FingerQuadPos.TopLeft;
                            } else if (fingerPos3d1.x > center.x && fingerPos3d1.x <= cardRight.x
                                 && fingerPos3d1.z >= cardBottom.z && fingerPos3d1.z <= center.z) {
                                finger3d1QuadPos = FingerQuadPos.BottomRight;
                            } else if (fingerPos3d1.x >= center.x && fingerPos3d1.x <= cardRight.x
                                 && fingerPos3d1.z > center.z && fingerPos3d1.z <= cardTop.z) {
                                finger3d1QuadPos = FingerQuadPos.TopRight;
                            }
                            // Find where the second finger touched the card
                            FingerQuadPos finger3d2QuadPos = FingerQuadPos.Unknown;
                            if (fingerPos3d2.x >= cardLeft.x && fingerPos3d2.x <= center.x
                                 && fingerPos3d2.z >= cardBottom.z && fingerPos3d2.z <= center.z) {
                                finger3d2QuadPos = FingerQuadPos.BottomLeft;
                            } else if (fingerPos3d2.x >= cardLeft.x && fingerPos3d2.x <= center.x
                                 && fingerPos3d2.z > center.z && fingerPos3d2.z <= cardTop.z) {
                                finger3d2QuadPos = FingerQuadPos.TopLeft;
                            } else if (fingerPos3d2.x > center.x && fingerPos3d2.x <= cardRight.x
                                 && fingerPos3d2.z >= cardBottom.z && fingerPos3d2.z <= center.z) {
                                finger3d2QuadPos = FingerQuadPos.BottomRight;
                            } else if (fingerPos3d2.x >= center.x && fingerPos3d2.x <= cardRight.x
                                 && fingerPos3d2.z > center.z && fingerPos3d2.z <= cardTop.z) {
                                finger3d2QuadPos = FingerQuadPos.TopRight;
                            }
                            // Swap fingers based on where the first finger landed as sometimes it would cause the card to
                            // be squeezed from the other side
                            if (finger3d2QuadPos == FingerQuadPos.BottomLeft && finger3d1QuadPos == FingerQuadPos.BottomRight) {
                                // Start from bottom
                                Debug.Log (Consts.FE_SQUEEZE_TWO_FINGERS_BOTTOM);
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_TWO_FINGERS_BOTTOM);
                                swapFingers = true;
                            } else if (finger3d1QuadPos == FingerQuadPos.TopLeft && finger3d2QuadPos == FingerQuadPos.TopRight) {
                                // Start from top
                                Debug.Log (Consts.FE_SQUEEZE_TWO_FINGERS_TOP);
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_TWO_FINGERS_TOP);
                                swapFingers = true;
                            } else if (finger3d2QuadPos == FingerQuadPos.TopLeft && finger3d1QuadPos == FingerQuadPos.BottomLeft) {
                                // Start from left
                                Debug.Log (Consts.FE_SQUEEZE_TWO_FINGERS_LEFT);
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_TWO_FINGERS_LEFT);
                                swapFingers = true;
                            } else if (finger3d2QuadPos == FingerQuadPos.BottomRight && finger3d1QuadPos == FingerQuadPos.TopRight) {
                                // Start from right
                                Debug.Log (Consts.FE_SQUEEZE_TWO_FINGERS_RIGHT);
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_TWO_FINGERS_RIGHT);
                                swapFingers = true;
                            } else if (finger3d2QuadPos == FingerQuadPos.BottomRight && finger3d1QuadPos == FingerQuadPos.BottomRight) {
                                return; // don't allow two finger squeezing from same quadrant
                            } else if (finger3d2QuadPos == FingerQuadPos.BottomLeft && finger3d1QuadPos == FingerQuadPos.BottomLeft) {
                                return; // don't allow two finger squeezing from same quadrant
                            } else if (finger3d2QuadPos == FingerQuadPos.TopRight && finger3d1QuadPos == FingerQuadPos.TopRight) {
                                return; // don't allow two finger squeezing from same quadrant
                            } else if (finger3d2QuadPos == FingerQuadPos.TopLeft && finger3d1QuadPos == FingerQuadPos.TopLeft) {
                                return; // don't allow two finger squeezing from same quadrant
                            } else {
                                swapFingers = false;
                            }
                        }

                        if (firstDrag)
                            firstDrag = false;

                        if (swapFingers) {
                            physx1 = fingerPos3d1;
                            physx2 = fingerPos3d2;
                        } else {
                            physx2 = fingerPos3d1;
                            physx1 = fingerPos3d2;
                        }

                        Vector3 midway = Vector3.Lerp (fingerPos3d1, fingerPos3d2, 0.5f);
                        physx3 = midway;
                    } else { // Single finger squeeze


                        if (firstDrag) {
                            // If we detect a single finger diagonal squeeze we determine which side it started from to prevent
                            // the angle switching to the other side if the finger moves all the way over there in one gesture.
                            // I.e. for one start->end gesture, it should continue to squeeze in the same direction.
                            if (fingerPos3d1.x >= gameObject.GetComponent<Collider>().bounds.center.x - gameObject.GetComponent<Collider>().bounds.size.x/2 // left hand side
                                && fingerPos3d1.x <= gameObject.GetComponent<Collider>().bounds.center.x - gameObject.GetComponent<Collider>().bounds.size.x*1/4)
                            {
                                startedLeftSide = true;
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_SINGLE_FINGER_DIAGONAL_LEFT);
                            }
                            else if (fingerPos3d1.x <= gameObject.GetComponent<Collider>().bounds.center.x + gameObject.GetComponent<Collider>().bounds.size.x/2 // right hand side
                                && fingerPos3d1.x >= gameObject.GetComponent<Collider>().bounds.center.x + gameObject.GetComponent<Collider>().bounds.size.x*1/4)
                            {
                                startedRightSide = true;
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_SINGLE_FINGER_DIAGONAL_RIGHT);
                            } else {
                                LogUtils.LogEvent(Consts.FE_SQUEEZE_SINGLE_FINGER_STRAIGHT);
                            }
                        }

                        if (firstDrag) {
                            firstDrag = false;
                            squeezeWarperLastPosVal = squeezeWarper.transform.position.z;
                        }

                        //move /= 2;

                        // Only move squeeze warper forwards
                        if (squeezeWarper.gameObject.transform.position.z + move.z >= squeezeWarperOrigPos.z) {
                            physx1 = new Vector3 (0, 0, move.z);
                            physx2 = Vector3.zero;
                            physx3 = Vector3.zero;

                            // Some basic trig for diagonal squeeze!
                            float adj = squeezeWarper.gameObject.GetComponent<Collider>().bounds.center.z - fingerPos3d1.z;
                            float opp = fingerPos3d1.x - squeezeWarper.gameObject.GetComponent<Collider>().bounds.center.x;
                            if (adj < 0) {
                                adj *= -1;
                            }
                            if (adj != 0) { // avoid /0
                                float angle = Mathf.Atan(opp/adj) * Mathf.Rad2Deg;
                                if (angle < 0) {
                                    angle += 180;
                                }

                                // Only squeeze diagonally if finger is on outter edges of card
                                if (startedLeftSide && fingerPos3d1.x >= gameObject.GetComponent<Collider>().bounds.center.x - gameObject.GetComponent<Collider>().bounds.size.x/2 // left hand side
                                    && fingerPos3d1.x <= gameObject.GetComponent<Collider>().bounds.center.x - gameObject.GetComponent<Collider>().bounds.size.x*1/4)
                                {
                                    physx3.x = angle;
                                }
                                else if (startedRightSide && fingerPos3d1.x <= gameObject.GetComponent<Collider>().bounds.center.x + gameObject.GetComponent<Collider>().bounds.size.x/2 // right hand side
                                    && fingerPos3d1.x >= gameObject.GetComponent<Collider>().bounds.center.x + gameObject.GetComponent<Collider>().bounds.size.x*1/4)
                                {
                                    physx3.x = angle;
                                }
                            }
                        }

                        // Return card after the magic number distance has been squeezed
                        if (squeezeWarper.gameObject.transform.position.z > (squeezeWarperOrigPos.z + 0.23f))
                            readyToReturnToDealer();
                    }

                    if (isCuttingCard) {
                        // Move the cutting card left to right with user swipe gestures
//                        Debug.Log ("gameObject.transform.position.x + move.x: " + (gameObject.transform.position.x + move.x) +
//                            ", rightBoundaryCardObj.transform.position.x: " + rightBoundaryCardObj.transform.position.x);
                        if (cutCardOrigPos.x <= (gameObject.transform.position.x + move.x)  // left most X boundary
                            && rightBoundaryCardObj != null
                            && (gameObject.transform.position.x + move.x) <= rightBoundaryCardObj.transform.position.x) { // right most boundary
//                            ) {
                            physxDragMove += move;
                            gameObject.transform.Translate (Vector3.right * physxDragMove.x, Space.World);
                            physxDragMove = Vector3.zero;
                        } else {
                            physxDragMove = Vector3.zero;
                        }
                    }
                }
            } else {
                Dragging = false;
            }
        }
    }

    void FixedUpdate ()
    {
        if (doRotate) { // we get here from DoubleTap gestures
            // Rotate the card
            doRotate = false;
            squeezeWarper.gameObject.transform.RotateAround (gameObject.GetComponent<Collider>().bounds.center, Vector3.up, -90);
            iTween.RotateAdd (gameObject, iTween.Hash ("amount", new Vector3 (0, 0, 90), "time", 1f, "oncomplete", "rotateSqueezeWarperComplete", "oncompletetarget", gameObject));
            physx1 = physx2 = physx3 = Vector3.zero;
            startedLeftSide = startedRightSide = false;
            firstDrag = true;
            return;
        }

        if (Dragging && GetComponent<Rigidbody>()) {
            //
            // Squeeze the card
            //
            if (physx1 != Vector3.zero && physx2 != Vector3.zero) { // Two finger squeeze

                if (Consts.DEBUG) {
                    //c1.rigidbody.MovePosition (physx1);
                    //c2.rigidbody.MovePosition (physx2);
                }

                Vector3 targetDir;
                Vector3 forward = Vector3.back;
                float a;

                if (physx1.x < physx2.x) {
                    targetDir = physx1 - physx2;
                    a = Vector3.Angle (targetDir, forward);

                    if (Consts.DEBUG) {
                       // dc1.rigidbody.MovePosition (physx3);
                       // dc2.rigidbody.MovePosition (Vector3.zero);
                    }
                    a -= 180;
                } else {
                    targetDir = physx2 - physx1;
                    a = Vector3.Angle (targetDir, forward);
                    if (Consts.DEBUG) {
                       // dc2.rigidbody.MovePosition (physx3);
                       // dc1.rigidbody.MovePosition (Vector3.zero);
                    }
                }

                squeezeWarper.GetComponent<Rigidbody>().MovePosition (new Vector3 (physx3.x, squeezeWarper.gameObject.transform.position.y, physx3.z/* - a/3000*/));
                squeezeWarper.GetComponent<Rigidbody>().MoveRotation (Quaternion.Euler (new Vector3 (0, a, 0)));

                // Return card after the magic number distance has been squeezed
                if (squeezeWarper.gameObject.transform.position.z > (squeezeWarperOrigPos.z + 0.24f))
                    readyToReturnToDealer();

            } else if (physx1 != Vector3.zero) { // Single finger squeeze
                //if (physx3.x > 65 && physx3.x < 115) // limit the angles to allow a nicer straight forward squeeze with no angle rotation
                if (physx3.x > 45 && physx3.x < 135) // limit the angles to allow a nicer straight forward squeeze with no angle rotation
                    squeezeWarper.GetComponent<Rigidbody>().MoveRotation (Quaternion.Euler (new Vector3 (0, physx3.x, 0)));
                squeezeWarper.GetComponent<Rigidbody>().MovePosition (new Vector3 (squeezeWarper.gameObject.transform.position.x,
                    squeezeWarper.gameObject.transform.position.y, squeezeWarper.gameObject.transform.position.z + physx1.z));

                // Reset back to "firstDrag" status so if we squeeze-reveal, then reverse back to hide the card, we can start
                // squeezing from another angle
                if (squeezeWarper.gameObject.transform.position.z < squeezeWarperLastPosVal) {
                    firstDrag = true;
                }
                squeezeWarperLastPosVal = squeezeWarper.gameObject.transform.position.z;
            }
            physx1 = physx2 = physx3 = Vector3.zero;
        }
    }

    void OnDrag (DragGesture gesture)
    {
        if (gesture.Selection != gameObject) {
            return;
        }

        HandleDrag (gesture);
    }

    void OnTwoFingerDrag (DragGesture gesture)
    {
        /** COMMENTED OUT because we removed the FaceDown/FaceUp squeeze feature
        if (!GUIControls.isFaceDown)
            return;
            */

        //Debug.Log ("Two finger drag detected!");
        HandleDrag (gesture);
    }

    // Enable/disable finger gesture detectors/recognizers
    public void setGestureRecognizerStates (bool state)
    {
        if (GetComponent<FingerDownDetector> () != null)
            GetComponent<FingerDownDetector> ().enabled = state;

        if (GetComponent<DragRecognizer> () != null)
            GetComponent<DragRecognizer> ().enabled = state;

        if (GetComponent<TwistRecognizer> () != null)
            GetComponent<TwistRecognizer> ().enabled = state;

        if (GetComponent<TapRecognizer> () != null)
            GetComponent<TapRecognizer> ().enabled = state;

        if (GetComponent<LongPressRecognizer> () != null)
            GetComponent<LongPressRecognizer> ().enabled = state;

        // Swipe isn't currently implemented. The idea was to be able to "swipe-return" a card to the dealer.
//        if (GetComponent<SwipeRecognizer> () != null)
//            GetComponent<SwipeRecognizer> ().enabled = state;
    }

    void OnFingerDown (FingerDownEvent evt)
    {
        // Prevent button presses from activating us
        if (GUIControls.returnCardPressed)
            return; // doesnt work, check fingerdown is lower priority than GUIControls in script execution order settings

        if (evt.Selection == gameObject) {
            GameState.Instance.dealer.cardDisplaying = gameObject;

            // Commented out these lines because original logic was to hide them after user started squeezing
//            GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Hide;
//            GameState.Instance.guiControls.clearButtonState = GUIControls.ClearButtonState.Hide;


            //Debug.Log ("FingerDownEvent: " + evt.Name + " on " + gameObject.name);
            // Move the squeeze closer to ourselves for better clarity while squeezing
            if (gameObject.name != null && gameObject.name.Contains ("1")) {
                GameState.Instance.camerasManager.moveSqueezeCamera ("left");
            } else if (gameObject.name != null && gameObject.name.Contains ("2")) {
                GameState.Instance.camerasManager.moveSqueezeCamera ("right");
            } else {
                GameState.Instance.camerasManager.moveSqueezeCamera ("center");
            }
        }
    }

    void OnSwipe (SwipeGesture gesture)
    {
        if (gesture.Selection != gameObject) {
            return;
        }

        Debug.Log ("Swipe gesture");
        //endSqueezing();
    }

    void OnLongPress (LongPressGesture gesture)
    {
        if (gesture.Selection != gameObject || revealed) {
            return;
        }

        Debug.Log ("Long press gesture");
        LogUtils.LogEvent (Consts.FE_SQUEEZE_TAP_REVEAL);
        revealSelf ();
    }

    bool doRotate = false;

    void OnDoubleTap (TapGesture gesture)
    {
        if (gesture.Selection != gameObject || Rotating || revealed) {
            return;
        }

        Debug.Log ("Double tap gesture to rotate");
        LogUtils.LogEvent (Consts.FE_SQUEEZE_TAP_ROTATE);

        // Do the rotate in FixedUpdate
        doRotate = true;
    }

    public void rotateSqueezeWarperComplete() {
        StartCoroutine(rotateSqueezeWarperCompleteCoroutine());
    }

    IEnumerator rotateSqueezeWarperCompleteCoroutine() {
        /* COMMMENTED OUT coz not working well
        // Need to wait for the previous transforms to complete before we rotate and reposition the squeeze warper.
        // The idea of this code here is; the user can peek at the card from an angle (or straight) when the card is
        // upright and if they then double tap to rotate the card we lay the card flat again. If we don't have this
        // code below then the card is rotated as is (i.e. half squeezed).
        yield return new WaitForSeconds(0.01f);
        float diffLen = squeezeWarper.transform.position.z - squeezeWarperOrigPos.z;
        //Debug.LogError("squeezeWarper.transform.position.z: " + squeezeWarper.transform.position.z);
        //Debug.LogError("squeezeWarperOrigPos.z: " + squeezeWarperOrigPos.z);
        //Debug.LogError("DIFFLEN: " + diffLen);
        squeezeWarper.rigidbody.MovePosition (new Vector3 (
            squeezeWarper.transform.position.x,
            squeezeWarper.transform.position.y,
            squeezeWarper.transform.position.z - diffLen*1.2f
           ));
            */
        yield break;
    }

    // Update the megafier bend's gizmo rotation after the card has been rotated so as to keep the angle
    // we squeeze at in line.
    void UpdateGizmoRot (Hashtable paramz)
    {
        iTween.RotateBy (squeezeWarper.gameObject, iTween.Hash ("y", -(float)paramz ["angle"], "time", 0.0f));
        return;
        // ***
        // Old code before we starting using space bend warpers for squeezing cards.
        // ***
//        float r = gameObject.transform.eulerAngles.y % 360f;
//        if (r >= 337.5f || r < 22.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 0;
//            minGizmoRotz = -45f;
//            maxGizmoRotz = 45f;
//        } else if (r >= 22.5f && r < 67.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 315;
//            minGizmoRotz = 270f;
//            maxGizmoRotz = 360f;
//        } else if (r >= 67.5f && r < 112.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 270;
//            minGizmoRotz = 225f;
//            maxGizmoRotz = 315f;
//        } else if (r >= 112.5f && r < 157.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 225;
//            minGizmoRotz = 180f;
//            maxGizmoRotz = 270f;
//        } else if (r >= 157.5f && r < 202.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 180;
//            minGizmoRotz = 135f;
//            maxGizmoRotz = 225f;
//        } else if (r >= 202.5f && r < 247.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 135;
//            minGizmoRotz = 90f;
//            maxGizmoRotz = 180f;
//        } else if (r >= 247.5f && r < 292.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 90;
//            minGizmoRotz = 45f;
//            maxGizmoRotz = 135f;
//        } else if (r >= 292.5f && r < 337.5f) {
//            bendMod.axis = MegaAxis.Z;
//            bendMod.gizmoRot.z = 45;
//            minGizmoRotz = 0f;
//            maxGizmoRotz = 90f;
//        } else {
//            Debug.LogWarning ("LOGIC ERROR with calculating UpdateGizmoRot!");
//            minGizmoRotz = -45f;
//            maxGizmoRotz = 360f;
//            return;
//        }
    }

    bool rotating = false;

    bool Rotating {
        get { return rotating; }
        set {
            if (rotating != value) {
                rotating = value;
            }
        }
    }

    // Rotate card by the amount squeezed parallel to table plane
    void OnTwist (TwistGesture gesture)
    {
        /** COMMENTED OUT because we removed the FaceDown/FaceUp squeeze feature
        // Only allow twisting to rotate when not in facedown otherwise the two 2 finger gestures clash
        if (GUIControls.isFaceDown)
            return;
        */

        /** COMMENTED OUT because OnTwist was interferring with double finger squeeze drag
        if (gesture.Selection != gameObject) {
            return;
        }

        if (gesture.Phase == ContinuousGesturePhase.Started) {
            Rotating = true;
        } else if (gesture.Phase == ContinuousGesturePhase.Updated) {
            if (Rotating) {
                // apply a rotation around the Z axis by rotationAngleDelta degrees on our target object
                float rotationDelta = -(gesture.DeltaRotation///2///) / 360f;
                Hashtable nextParams = new Hashtable();
                nextParams.Add("angle", rotationDelta);
                iTween.RotateBy (gameObject, iTween.Hash ("amount", new Vector3 (0f, 0f, rotationDelta), "time", 0.0f,
                    "onComplete", "UpdateGizmoRot", "onCompleteTarget", gameObject, "onCompleteParams", nextParams));
            }
        } else {
            if (Rotating) {
                Rotating = false;
            }
        }
        */
    }

    void OnTriggerEnter (Collider other)
    {
        if (isCuttingCard && other.name.Contains ("CutCard")) {
            playCardTapSound ();
        }
    }

    void OnTriggerStay (Collider other)
    {
        if (detectCutCardHit && other.name.Contains ("CutCard")) {
//            Debug.Log ("Last cut card collider was " + other.name);
            lastCutCardCollider = other;
        }
    }

    void OnDisable ()
    {
        // if this gets disabled while dragging, make sure we cancel the drag operation
        if (Dragging)
            Dragging = false;
    }

    // Callback method to call when squeezing of this card is done
    void readyToReturnToDealer ()
    {
        readyToReturn = true;
        this.gameObject.GetComponent<MegaWarpBind> ().ModEnabled = false; // reset squeezing for this card
        GameState.Instance.dealer.squeezing = true; // hack coz sometimes it was false preventing us from ending squeezing
        GameState.Instance.dealer.endSqueezing ();
    }

    // Show our face
    public void revealSelf ()
    {
        revealSelf (null, null, null, false);
    }

    // Called to reveal ourselves when we're a 3rd card and need to call a method after we've finished revealing
    public void revealSelf (GameObject gameObject, string callbackMethodName)
    {
        revealSelf (gameObject, callbackMethodName, null, false);
    }

    // Reveal self and then invoke the callback to do something to the next one after it
    // revealFromSqueeze not used anymore!
    public void revealSelf (GameObject nextOnCompleteTarget, string nextOnCompleteCallbackMethodName, Hashtable nextOnCompleteParamObj, bool revealFromSqueeze)
    {
        if ("BankerCard3".Equals(this.gameObject.name))
            // Hide the button to reveal the banker's 3rd card
            GameState.Instance.guiControls.ToggleRevealOtherButton(false);

        if (revealed) {
            Debug.LogWarning ("Card " + name + " already revealed");
            // If we've been revealed already then skip the revealing stuff and just move onto the next callback method if one (most likely returning
            // ourselves to the dealer -- I think)
            if (nextOnCompleteTarget != null && nextOnCompleteCallbackMethodName != null && nextOnCompleteParamObj != null) {
                nextOnCompleteTarget.SendMessage (nextOnCompleteCallbackMethodName, nextOnCompleteParamObj);
            } else if (nextOnCompleteTarget != null && nextOnCompleteCallbackMethodName != null && nextOnCompleteParamObj == null) {
                nextOnCompleteTarget.SendMessage (nextOnCompleteCallbackMethodName);
            }
            return;
        }

        // Once we've been revealed we can show the corner symbol and text
        showSymbolsText = true;
        Rebuild ();

        Debug.Log ("Revealing card " + gameObject.name);
        playRevealingSound ();

        /* Commented out because we fixed the root cause of the cards being skewed funny... do you remember what it was?!
        // For some reason we appear skewed after being dealt, most likely after being malformed with megafier for squeezing,
        // so here's a hack to unskew ourselves
        MegaSkew skewer = gameObject.GetComponent<MegaSkew>();
        if (skewer != null) {
            //skewer.amount = -0.44f; // a tried and tested number
        }
        */

        // Reset any squeeziness
        this.gameObject.GetComponent<MegaWarpBind> ().ModEnabled = false;

        // Reset any bend modifications done to ourselves through squeezing
        if (bendMod != null) {
            bendMod.ResetOffset ();
            bendMod.ResetGizmoPos ();
//            bendMod.ResetGizmoRot ();
//            bendMod.ResetGizmoScale ();
            bendMod.angle = 0;
            bendMod.dir = 0;
        }

        // Flip ourselves over and invoke callback for next card if specified
        if (nextOnCompleteTarget != null && nextOnCompleteCallbackMethodName != null && nextOnCompleteParamObj != null) {
            float waitDelay = Dealer.dealSpeed * 4;
            if (nextOnCompleteParamObj.Contains ("delay"))
                nextOnCompleteParamObj ["delay"] = waitDelay;
            else
                nextOnCompleteParamObj.Add ("delay", waitDelay);
            iTween.RotateBy (gameObject, iTween.Hash ("x", 0.5f, "y", 0f, "time", 0.0f,
                    "onCompleteTarget", nextOnCompleteTarget,
                    "onComplete", nextOnCompleteCallbackMethodName,
                    "onCompleteParams", nextOnCompleteParamObj));
        } else if (nextOnCompleteTarget != null && nextOnCompleteCallbackMethodName != null && nextOnCompleteParamObj == null) {
            // Reveal ourselves and call callback with no params, currently used for callbacks after revealing a 3rd card
            iTween.RotateBy (gameObject, iTween.Hash ("y", (revealed ? 0 : .5),
                "time", 0.3f,
                "onCompleteTarget", nextOnCompleteTarget,
                "onComplete", nextOnCompleteCallbackMethodName));
        } else {
            // Just reveal self with no callback
            iTween.RotateBy (gameObject, iTween.Hash ("y", 0.5f, "time", 0.3f));
        }

        revealed = true;

        // Update our value on the status bar
        GameState.Instance.guiControls.AddCardValue (cardType, Definition.getValue ());
    }

    // Function for iTween to call and update the bendMod.gizmoPos.y variable
    public void TweenedBendModGizmoPosY (float val)
    {
        if (bendMod != null) {
            bendMod.gizmoPos.y = val;
        }
    }

    // Function for iTween to call and update the bendMod.gizmoPos.z variable
    public void TweenedBendModGizmoPosZ (float val)
    {
        if (bendMod != null) {
            bendMod.gizmoPos.z = val;
        }
    }

    public void playSlidingSound ()
    {
        if (audioSources == null || audioSources.Length < 1) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [0] != null) {
            audioSources [0].Play ();
        }
    }

    public void playRevealingSound ()
    {
        if (audioSources == null || audioSources.Length < 2) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [1] != null) {
            audioSources [1].Play ();
        }
    }

    public void playSqueezingSound ()
    {
        if (audioSources == null || audioSources.Length < 3) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [2] != null) {
            audioSources [2].Play ();
        }
    }

    public void playReturningSound ()
    {
        if (audioSources == null || audioSources.Length < 4) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [3] != null) {
            audioSources [3].Play ();
        }
    }

    public void playCardTapSound ()
    {
        if (audioSources == null || audioSources.Length < 5) {
            audioSources = gameObject.GetComponents<AudioSource> ();
        }
        if (audioSources [4] != null) {
            audioSources [4].Play ();
        }
    }

    public bool isReturning = false;

    // Return ourselves to the dealer using iTween and the specified parameters
    public void returnSelf (Hashtable table)
    {
        if (isReturning)
            return;

        isReturning = true;

        // Hide return card button if we're player or banker 3
        if (gameObject.name.Contains ("3")) {
            GameState.Instance.guiControls.dealButtonState = GUIControls.DealButtonState.Hide;
        }

        // Shrink back to normal size after enlargened for squeezing
        ToggleEnlarge ();

        // Reset card back to its original portrait position
        iTween.RotateTo (gameObject, iTween.Hash ("y", squeezeWarperOrigRot.y / 360f, "time", 0.2f));

        if (table != null && table.ContainsKey ("delay") && table.Contains ("dealerCallback")) {
            StartCoroutine (playReturnSoundCoroutine ((float)table ["delay"], (string)table ["dealerCallback"]));
        } else if (table != null && table.ContainsKey ("delay") && !table.Contains ("dealerCallback")) {
            StartCoroutine (playReturnSoundCoroutine ((float)table ["delay"], null));
        } else {
            playReturningSound ();
        }
        iTween.MoveTo (gameObject, table);
    }

    IEnumerator playReturnSoundCoroutine (float delay, string dealerCallbackMethod)
    {
        yield return new WaitForSeconds(delay);
        playReturningSound ();

        // Squeezing can put the camera in up or down positions, so we reset to left/right so otherside call below works
        // COMMENTING OUT because camera was switching to left after returning second card and there was nothing there,
        // so it was like a pointless camera move
        /*
        if (gameObject.name.Contains("1")) {
            GameState.Instance.camerasManager.moveSqueezeCamera("left");
        } else if (gameObject.name.Contains("2")) {
            GameState.Instance.camerasManager.moveSqueezeCamera("right");
        }
        */

        // Callbacks to get the dealer to do the next step
        if (dealerCallbackMethod == "startCalculate2CardTotalsCoroutine") {
            GameState.Instance.dealer.startCalculate2CardTotalsCoroutine ();
        } else if (dealerCallbackMethod == "draw3rdBankerCard") {
            //GameState.Instance.camerasManager.ToggleSqueezeCamera(false);
            GameState.Instance.dealer.draw3rdBankerCard ();
        } else if (dealerCallbackMethod == "calculate3CardsTotals") {
            GameState.Instance.dealer.calculate3CardsTotals ();
        } else {
            // This is the right time to switch the camera to the other side when we were the first card squeezed out of the first two.
            // That's the only time this cooroutine should be called is when the first card of the 1st and 2nd cards dealt is this one
            // and we've just revealed ourselves before returning to the dealer.

            // Allow returning of the other card when the 1st/2nd has been returned already
            if ("PlayerCard2" == gameObject.name && !((GameObject) GameState.Instance.dealer.dealtCards["PlayerCard1"]).GetComponent<Card>().isReturning) {
                Debug.Log ("Hack to allow first player card to be returned second");
                GameState.Instance.camerasManager.moveSqueezeCamera("left");
            } else if ("BankerCard2" == gameObject.name && !((GameObject) GameState.Instance.dealer.dealtCards["BankerCard1"]).GetComponent<Card>().isReturning) {
                Debug.Log ("Hack to allow first banker card to be returned second");
                GameState.Instance.camerasManager.moveSqueezeCamera("left");
            } else if ("PlayerCard1" == gameObject.name && !((GameObject) GameState.Instance.dealer.dealtCards["PlayerCard2"]).GetComponent<Card>().isReturning) {
                Debug.Log ("Hack to allow second player card to be returned first");
                GameState.Instance.camerasManager.moveSqueezeCamera("right");
            } else if ("BankerCard1" == gameObject.name && !((GameObject) GameState.Instance.dealer.dealtCards["BankerCard2"]).GetComponent<Card>().isReturning) {
                Debug.Log ("Hack to allow second banker card to be returned first");
                GameState.Instance.camerasManager.moveSqueezeCamera("right");
            }
            if ("PlayerCard2" == gameObject.name) {
                GameState.Instance.guiControls.hideLeftRightCardButtons();
            } else if ("BankerCard2" == gameObject.name) {
                GameState.Instance.guiControls.hideLeftRightCardButtons();
            } else if ("PlayerCard1" == gameObject.name) {
                GameState.Instance.guiControls.hideLeftRightCardButtons();
            } else if ("BankerCard1" == gameObject.name) {
                GameState.Instance.guiControls.hideLeftRightCardButtons();
            }
        }
    }

    // Slide ourselves to a point using iTween and the specified parameters
    public void slideSelf (Hashtable table)
    {
        playSlidingSound ();
        iTween.MoveTo (gameObject, table);

        // Commented out coz these hacks aren't needed any more...
        /*i
        // Hack to straighten (straight up and down, face down from the user's perspective) the rotation of the cards after they've slided
        //iTween.RotateTo (gameObject, iTween.Hash ("y", -5f, "time", table["time"]));

        // For some reason we appear skewed after being dealt
        MegaSkew skewer = gameObject.GetComponent<MegaSkew>();
        if (skewer != null) {
            //skewer.amount = 0.44f; // a tried and tested number
        }
        */
    }

    public static float CARD_ENLARGEN_SCALEFX = 2.342215f;
    public static float CARD_ENLARGEN_SCALEFY = 2.807938f;
    public static float CARD_ENLARGEN_SCALEFZ = 2.807938f;
    public static float CARD_ENLARGEN_SCALEF_TIME = 0.5f;
    private bool isEnlarged = false;

    public void ToggleEnlarge ()
    {
        // Scale size up a bit to avoid jitter/shakiness when squeezing
        //if (!GameState.Instance.camerasManager.isAR()) {
        Debug.Log ((isEnlarged ? "Shrinking" : "Enlargening") + " card " + gameObject.name + " for squeezing");
        iTween.ScaleAdd (gameObject,
                iTween.Hash ("amount",
                new Vector3 (
                    isEnlarged ? -CARD_ENLARGEN_SCALEFX : CARD_ENLARGEN_SCALEFX,
                    isEnlarged ? -CARD_ENLARGEN_SCALEFY : CARD_ENLARGEN_SCALEFY,
                    isEnlarged ? -CARD_ENLARGEN_SCALEFZ : CARD_ENLARGEN_SCALEFZ),
                "time", CARD_ENLARGEN_SCALEF_TIME
                , "delay", (isEnlarged ? 4 * Dealer.dealSpeed : Dealer.dealSpeed)
            ));
        isEnlarged = !isEnlarged;
        if (isEnlarged)
            gameObject.layer = 9; // "SqueezeCard"; // this hides the card from the pic-in-pic cameras while squeezing
        else
            gameObject.layer = 0; // "Default";
        //}
    }

    // This is a callback function.
    public void startCutCardMoving ()
    {
        // If we're the cut card take not of its current position before the user
        // starts to move it. This can then act as the left-hand boundary.
        cutCardOrigPos = transform.position;

        DragPlaneCollider = GameState.Instance.tableManager.table.GetComponent<Collider>();
    }
}
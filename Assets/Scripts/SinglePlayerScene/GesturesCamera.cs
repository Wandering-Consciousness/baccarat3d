using UnityEngine;
using System.Collections;

// Class to handle swipe and pinch gestures to rotate and zoom the camera. Usually logic for both could be separated,
// as in the original example scripts that came with FingerGesture (Scripts/Toolbox/Camera/TBDragView and TBPinchZoom),
// but we combine them here to add logic to make them mutually exclusive - i.e. while pinching/zooming no swiping/rotating
// will occur, and vice versa.
[RequireComponent( typeof( Camera ) )]
[RequireComponent( typeof( PinchRecognizer ) )]
[RequireComponent( typeof( DragRecognizer ) )]
public class GesturesCamera : MonoBehaviour
{

    public bool allowUserInput = true;  // set this to false to prevent the user from dragging the view and zooming
    public bool unrestrictedMovement = false;  // restricted (default) gives mins/maxs on how far user can rotate/zoom

    void Start()
    {
        // Check whether steering controls are enabled at all before proceeding any further
        if (Utils.isPreferenceEnabled("pref_key_graphics_settings_steering_controls", (Consts.DEBUG ? true : false))) {
            Debug.Log ("Steering controls are enabled");
            allowUserInput = true;
        } else {
            Debug.Log ("Steering controls are disabled");
            allowUserInput = false;
            return;
        }

        // Whether or not to allow unrestricted movement (i.e. more relaxing the min/max angles/distances)
        if (Utils.isPreferenceEnabled("pref_key_graphics_settings_unrestricted_movement", (Consts.DEBUG ? false : false))) {
            Debug.Log ("Unrestricted movement is enabled");
            unrestrictedMovement = true;
        } else {
            Debug.Log ("Unrestricted movement is disabled");
            unrestrictedMovement = false;
        }

        // >>> Zoom code

        if( !GetComponent<PinchRecognizer>() )
        {
            Debug.LogWarning( "No pinch recognizer found on " + this.name + ". Disabling TBPinchZoom." );
            enabled = false;
        }

        SetDefaults();

        // <<< EOF Zoom code


        // >>> Rotation code

        // Whether or not to reverse direction of steering controls
        if (Utils.isPreferenceEnabled("pref_key_graphics_settings_reverse_steering", true)) {
            Debug.Log ("Steering controls are reversed");
            reverseControls = true;
        } else {
            Debug.Log ("Steering controls are not reversed");
            reverseControls = false;
        }

        IdealRotation = cachedTransform.rotation;

        // sanity check
        if( !GetComponent<DragRecognizer>() )
        {
            Debug.LogWarning( "No drag recognizer found on " + this.name + ". Disabling TBDragView." );
            enabled = false;
        }

        // <<< EOF Rotation code
    }

    //
    // >>> Zoom methods and fields
    //

    public enum ZoomMethod
    {
        // move the camera position forward/backward
        Position,

        // change the field of view of the camera, or projection size for orthographic cameras
        FOV,
    }

    public ZoomMethod zoomMethod = ZoomMethod.Position;
    public float zoomSpeed = 0.01f;
    public float minZoomAmount = -2;
    public float maxZoomAmount = 1.0f;

    Vector3 defaultPos = Vector3.zero;
    float defaultFov = 0;
    float defaultOrthoSize = 0;
    float zoomAmount = 0;
    PinchGesture pinchGesture;

    public Vector3 DefaultPos
    {
        get { return defaultPos; }
        set { defaultPos = value; }
    }

    public float DefaultFov
    {
        get { return defaultFov; }
        set { defaultFov = value; }
    }

    public float DefaultOrthoSize
    {
        get { return defaultOrthoSize; }
        set { defaultOrthoSize = value; }
    }

    public float ZoomAmount
    {
        get { return zoomAmount; }
        set
        {
            // check to restrict the distance we can zoom or not
            if (unrestrictedMovement) {
                zoomAmount = value;
            } else {
                zoomAmount = Mathf.Clamp( value, minZoomAmount, maxZoomAmount );
            }

            switch( zoomMethod )
            {
                case ZoomMethod.Position:
                    transform.position = defaultPos + zoomAmount * transform.forward;
                    break;

                case ZoomMethod.FOV:
                    if( GetComponent<Camera>().orthographic )
                        GetComponent<Camera>().orthographicSize = Mathf.Max( defaultOrthoSize - zoomAmount, 0.1f );
                    else
                        GetComponent<Camera>().fieldOfView = Mathf.Max( defaultFov - zoomAmount, 0.1f );
                    break;
            }
        }
    }

    public float ZoomPercent
    {
        get { return ( ZoomAmount - minZoomAmount ) / ( maxZoomAmount - minZoomAmount ); }
    }

    public void SetDefaults()
    {
        DefaultPos = transform.position;
        DefaultFov = GetComponent<Camera>().fieldOfView;
        DefaultOrthoSize = GetComponent<Camera>().orthographicSize;
    }

    public bool Pinching
    {
        get { return pinchGesture != null; }
    }

    // Handle the pinch event
    void OnPinch( PinchGesture gesture )
    {
        // Don't allow pinch-zoom when in AR and squeeze mode because it interfers with the two finger card squeezing
        //if (GameState.Instance.camerasManager.arCamera.activeSelf && GameState.Instance.dealer.squeezing) {
        if (GameState.Instance.camerasManager.isAR() && GameState.Instance.dealer.squeezing) { // testing both AR and gyrocam mode disable
            Debug.LogWarning ("Pinch-zooming not allowed in during squeezing in AR mode");
            return;
        }

        if (gesture.Phase == ContinuousGesturePhase.Started)
            LogUtils.LogEvent(Consts.FE_TWO_FINGER_ZOOM);

        // Take note of when we're been pinched and set a flag
        // so dragging can be disabled during these times.
        if( gesture.Phase != ContinuousGesturePhase.Ended )
        {
            pinchGesture = gesture;
        }
        else
        {
            pinchGesture = null;
        }

        if (!allowUserInput) {
            return;
        }

        ZoomAmount += zoomSpeed * gesture.Delta;
    }

    //
    // <<< EOF Zoom methods and fields
    //


    //
    // >>> Rotation methods and fields
    //

    public float sensitivity = 3.0f;
    public float dragAcceleration = 6.5f;
    public float dragDeceleration = 2.5f;
    public bool reverseControls = true;
    public float minPitchAngle = 0.0f;
    public float maxPitchAngle = 37.5f;
    public float minYawAngle = -30.0f;
    public float maxYawAngle = 60.0f;
    public float idealRotationSmoothingSpeed = 7.0f; // set to 0 to disable smoothing when rotating toward ideal direction

    Transform cachedTransform;
    Vector2 angularVelocity = Vector2.zero;
    Quaternion idealRotation;
    bool useAngularVelocity = false;

    DragGesture dragGesture;

    void Awake()
    {
        cachedTransform = this.transform;
    }

    public bool Dragging
    {
        get { return dragGesture != null; }
    }

    // Handle Gesture Event (sent by the DragRecognizer component)
    void OnDrag( DragGesture gesture )
    {
        if( gesture.Phase != ContinuousGesturePhase.Ended )
            dragGesture = gesture;
        else
            dragGesture = null;

        if (gesture.Phase == ContinuousGesturePhase.Started)
            LogUtils.LogEvent(Consts.FE_THREE_FINGER_SWIPE);
    }

    void Update()
    {
        // Do nothing if we're not allowed to receive user input
        if (!allowUserInput) {
            return;
        }

        // Pinching/zooming takes precendence over dragging to rotate the camera
        if (Pinching)
        {
            return;
        }

        if (Dragging)
            useAngularVelocity = true;

        if( useAngularVelocity )
        {
            Vector3 localAngles = transform.localEulerAngles;
            Vector2 idealAngularVelocity = Vector2.zero;

            float accel = dragDeceleration;

            if( Dragging )
            {
                idealAngularVelocity = sensitivity * dragGesture.DeltaMove;
                accel = dragAcceleration;
            }

            angularVelocity = Vector2.Lerp( angularVelocity, idealAngularVelocity, Time.deltaTime * accel );
            Vector2 angularMove = Time.deltaTime * angularVelocity;

            if( reverseControls )
                angularMove = -angularMove;

            // pitch angle
            if (unrestrictedMovement) {
                localAngles.x -= angularMove.y;
            } else {
                // Normal pitch - the default
                localAngles.x = Mathf.Clamp( NormalizePitch( localAngles.x - angularMove.y ), minPitchAngle, maxPitchAngle );
            }

            // yaw angle
            if (unrestrictedMovement) {
                localAngles.y += angularMove.x;
            } else {
                // Normal yaw - the default
                localAngles.y = Mathf.Clamp( NormalizeYaw( localAngles.y + angularMove.x ), minYawAngle, maxYawAngle );
            }

            // apply
            transform.localEulerAngles = localAngles;
        }
//        else
//        {
//            if( idealRotationSmoothingSpeed > 0 )
//                cachedTransform.rotation = Quaternion.Slerp( cachedTransform.rotation, IdealRotation, Time.deltaTime * idealRotationSmoothingSpeed );
//            else
//                cachedTransform.rotation = idealRotation;
//        }
    }

    static float NormalizePitch( float angle )
    {
        if( angle > 180.0f )
            angle -= 360.0f;

        return angle;
    }

    static float NormalizeYaw( float angle )
    {
        if( angle > 180.0f )
            angle -= 360.0f;

        return angle;
    }

    public Quaternion IdealRotation
    {
        get { return idealRotation; }
        set
        {
            idealRotation = value;
            useAngularVelocity = false;
        }
    }

    // Point the camera at the target point
    public void LookAt( Vector3 pos )
    {
        IdealRotation = Quaternion.LookRotation( pos - cachedTransform.position );
    }

    //
    // <<< EOF rotation methods and fields
    //
}

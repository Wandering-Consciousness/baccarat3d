using UnityEngine;

using System.Collections;

using System.Collections.Generic;

// For detecting blows in mic, and fluttering the registered rigidbodies up and ripple effecting them
// Referenced http://forum.unity3d.com/threads/118215-Blow-detection-(Using-iOS-Microphone)
public class iBlow : MonoBehaviour
{
    private const int FREQUENCY = 48000;    // Wavelength, I think.
    private const int SAMPLECOUNT = 1024;   // Sample Count.
    private const float REFVALUE = 0.1f;    // RMS value for 0 dB.
    private const float THRESHOLD = 0.02f;  // Minimum amplitude to extract pitch (recieve anything)
    private const float ALPHA = 0.05f;      // The alpha for the low pass filter (I don't really understand this).

    public int recordedLength = 50;    // How many previous frames of sound are analyzed.
    public int requiedBlowTime = 2;    // How long a blow must last to be classified as a blow (and not a sigh for instance).
    public int clamp = 160;            // Used to clamp dB (I don't really understand this either).

    public List<BlowableRippleableRigidBody> rigidBodiesToMove; // The rigidbodies to move on blowing

    private bool running = false;

    private float rmsValue;            // Volume in RMS
    private float dbValue;             // Volume in DB
    private float pitchValue;          // Pitch - Hz (is this frequency?)
    private int blowingTime;           // How long each blow has lasted
    private float lowPassResults;      // Low Pass Filter result
    private float peakPowerForChannel; //

    private float[] samples;           // Samples
    private float[] spectrum;          // Spectrum

    private List<float> dbValues;      // Used to average recent volume.
    private List<float> pitchValues;   // Used to average recent pitch.

    Vector3 physxDragMove = Vector3.zero;

    AudioSource audioSrc;

    public void Start ()
    {
        samples = new float[SAMPLECOUNT];
        spectrum = new float[SAMPLECOUNT];
        dbValues = new List<float> ();
        pitchValues = new List<float> ();

        rigidBodiesToMove = new List<BlowableRippleableRigidBody>();

        audioSrc = gameObject.AddComponent<AudioSource>();

        //StartMicListener (); // TODO: undo
    }

    public void Update ()
    {
        if (running) {
            // If the audio has stopped playing, this will restart the mic play the clip.
            if (!audioSrc.isPlaying) {
                StartMicListener ();
            }
    
            // Gets volume and pitch values
            AnalyzeSound ();
    
            // Runs a series of algorithms to decide whether a blow is occuring.
            DeriveBlow ();
    
            // Update the meter display.
//            GUIControls.message = "RMS: " + rmsValue.ToString ("F2") + " (" + dbValue.ToString ("F1") + " dB)\n" + "Low pass: " + lowPassResults + "\nPitch: " + pitchValue.ToString ("F0") + " Hz";
        }
    }

    /// Starts the Mic, and plays the audio back in (near) real-time.
    private void StartMicListener ()
    {
        audioSrc.clip = Microphone.Start (null, false, 5, FREQUENCY);
        //audioSrc.mute = true;
        // HACK - Forces the function to wait until the microphone has started, before moving onto the play function.
        while (!(Microphone.GetPosition(null) > 0)) {
        }
        audioSrc.Play ();
    }

    /// Credits to aldonaletto for the function, http://goo.gl/VGwKt
    /// Analyzes the sound, to get volume and pitch values.
    private void AnalyzeSound ()
    {
        // Get all of our samples from the mic.
        audioSrc.GetOutputData (samples, 0);

        // Sums squared samples
        float sum = 0;
        for (int i = 0; i < SAMPLECOUNT; i++) {
            sum += Mathf.Pow (samples [i], 2);
        }

        // RMS is the square root of the average value of the samples.
        rmsValue = Mathf.Sqrt (sum / SAMPLECOUNT);
        dbValue = 20 * Mathf.Log10 (rmsValue / REFVALUE);

        // Clamp it to {clamp} min
        if (dbValue < -clamp) {
            dbValue = -clamp;
        }

        // Gets the sound spectrum.
        audioSrc.GetSpectrumData (spectrum, 0, FFTWindow.BlackmanHarris);
        float maxV = 0;
        int maxN = 0;

        // Find the highest sample.
        for (int i = 0; i < SAMPLECOUNT; i++) {
            if (spectrum [i] > maxV && spectrum [i] > THRESHOLD) {
                maxV = spectrum [i];
                maxN = i; // maxN is the index of max
            }
        }

        // Pass the index to a float variable
        float freqN = maxN;

        // Interpolate index using neighbours
        if (maxN > 0 && maxN < SAMPLECOUNT - 1) {
            float dL = spectrum [maxN - 1] / spectrum [maxN];
            float dR = spectrum [maxN + 1] / spectrum [maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }

        // Convert index to frequency
        pitchValue = freqN * 24000 / SAMPLECOUNT;
    }

    private void DeriveBlow ()
    {
        UpdateRecords (dbValue, dbValues);
        UpdateRecords (pitchValue, pitchValues);

        // Find the average pitch in our records (used to decipher against whistles, clicks, etc).
        float sumPitch = 0;
        foreach (float num in pitchValues) {
            sumPitch += num;
        }

        sumPitch /= pitchValues.Count;

        // Run our low pass filter.
        lowPassResults = LowPassFilter (dbValue);

        // Decides whether this instance of the result could be a blow or not.
        if (lowPassResults > 0) {
//         if (lowPassResults >= -30) {
            blowingTime += 1;

        } else {

            blowingTime = 0;

        }

        // Once enough successful blows have occured over the previous frames (requiredBlowTime), the blow is triggered.
        // This example says "blowing", or "not blowing", and also blows up a sphere.
        float scalef = 0.01f;

        if (blowingTime > requiedBlowTime) {
//            GUIControls.bankerCardsValueText = "Blowing";
//      balloon.transform.localScale *= scalef;
            float res = pitchValue / 100000;
            if (res < 0) res = 0;
            physxDragMove += new Vector3 (0f, res, 0f);
        } else {
//            GUIControls.bankerCardsValueText = "Not blowing";
//      float t = 0.3270419f;
//      if (balloon.transform.localScale.x > t && balloon.transform.localScale.y > t && balloon.transform.localScale.z > t)
//            balloon.transform.localScale *= 0.999f;
        }
    }

    // Updates a record, by removing the oldest entry and adding the newest value (val).
    private void UpdateRecords (float val, List<float> record)
    {
        if (record.Count > recordedLength) {
            record.RemoveAt (0);
        }
        record.Add (val);
    }

    /// Gives a result (I don't really understand this yet) based on the peak volume of the record
    /// and the previous low pass results.
    private float LowPassFilter (float peakVolume)
    {
        return ALPHA * peakVolume + (1.0f - ALPHA) * lowPassResults;
    }

    void FixedUpdate ()
    {
        // use MovePosition() for physics objects
        if (rigidBodiesToMove != null) {
            foreach (BlowableRippleableRigidBody rb in rigidBodiesToMove) {
                rb.rigidbody.MovePosition (rb.rigidbody.position + physxDragMove);
//                rb.megaRipple.Offset.x += physxDragMove.x;
            }
        }

        // reset the accumulated drag amount value
        physxDragMove = Vector3.zero;
    }

    // Start iBlowing.
    // Starts detection for blows and animates the registered rigidbodies
    public void start(params GameObject[] rigidbodies) {
        if (running)
            return;

        Debug.Log ("Starting iBlow detection");

        running = true;
        foreach (GameObject rb in rigidbodies) {
            Debug.Log ("iBlow " + rb.name);

            // Package rigidbody's gameobject and megafier references in easily accessed class
            BlowableRippleableRigidBody br = new BlowableRippleableRigidBody();
            br.rigidbody = rb.GetComponent<Rigidbody>();
//            br.megaRipple = rb.gameObject.GetComponent<MegaRipple> ();
//            br.megaRipple.amp = 0.15f;
//            rb.GetComponent<MegaModifyObject>().MeshUpdated();
            rigidBodiesToMove.Add (br);
        }
    }

    // End iBlowing
    // Stops blow detection and clears registered rigidbodies
    public void end() {
        if (!running)
            return;

        Debug.Log ("Ending iBlow");

        // Reset moved/animation stuff
        foreach (BlowableRippleableRigidBody rb in rigidBodiesToMove) {
//            rb.megaRipple.ResetOffset();
//            rb.megaRipple.ResetGizmoPos();
//            rb.megaRipple.ResetGizmoRot();
//            rb.megaRipple.ResetGizmoScale();
//            rb.megaRipple.amp = 0.0f;
        }


        running = false;
        rigidBodiesToMove.Clear();
        Microphone.End(null);
    }

    public class BlowableRippleableRigidBody {
        public Rigidbody rigidbody;
        public MegaRipple megaRipple;
    }
}
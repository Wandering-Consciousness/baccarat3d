#pragma strict

/** Script for displaying tutorial text in speech bubbles */

var englishText : String;
var japaneseText : String;
var displayTime : int;
var triangle : int; // Added by Simon: 1 = draw bottom triangle on left, 2 - draw it right, anything else - center

private var speech : Speech;    // this entity's speech reference

var speechHolder : Transform;    // position where speech bubble will stay, preferably right above a head

function Start () {
    SetSpeech();
}

function SetSpeech()
{
    speech = Instantiate(SpeechManager.bubble, Vector3.zero, Quaternion.identity).GetComponent(Speech);
    speech.holder = speechHolder; // pass holder
    speech.triangle = triangle; // Added by Simon
    speech.Stop();
}

function Update()
{
    // Hack so we can get feedback from C# scripts to stop showing the speech bubble
    if ("d".Equals(gameObject.tag)) {
        speech.Stop();
        gameObject.SetActive(false);
    } else if ("sa".Equals(gameObject.tag)) {
        // Say text depending on whether Japanese or English
        if (isJapanese()) {
            speech.SayNewLine(japaneseText, displayTime);
        } else {
            speech.SayNewLine(englishText, displayTime);
        }
        gameObject.tag = "x";
    }
}

// Because the Speech Bubble asset we bought from the store is in JavaScript we couldn't use the Utils.cs / Language Manager all the other scripts use.
// So instead each game object in the screen that is setup for a speech bubble needs to have its corresponding English and Japanese arrays filled in
// manually.
function isJapanese() {
    var lang : String;
    lang = Application.systemLanguage.ToString();
    return lang.Equals("Japanese");
}
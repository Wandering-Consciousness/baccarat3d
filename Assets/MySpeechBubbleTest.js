#pragma strict

private var speech : Speech;    // this entity's speech reference

var speechHolder : Transform;    // position where speech bubble will stay, preferably right above a head

function SetSpeech()
{
 speech = Instantiate(SpeechManager.bubble, Vector3.zero, Quaternion.identity).GetComponent(Speech);
 speech.holder = speechHolder; // pass holder
 speech.Stop();
}

function Start () {
    SetSpeech();
    speech.Say("Hello");
}

function Update () {

}
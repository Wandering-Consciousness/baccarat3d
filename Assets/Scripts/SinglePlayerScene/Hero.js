#pragma strict

private var speech : Speech;    // this entity's speech reference
public var speechManagerGO : GameObject;
var speechHolder : Transform;    // position where speech bubble will stay, preferably right above a head

function Start () {
    //var sm : SpeechManager;
    //sm = speechManagerGO.GetComponent(SpeechManager);
    //sm.cam = Camera.main;
    //sm.camT = Camera.main.transform;
    SetSpeech();
    speech.Say("HELLO");
}

function Update () {

}

function SetSpeech()
{
 speech = Instantiate(SpeechManager.bubble).GetComponent(Speech);
 speech.holder = speechHolder; // pass holder
 speech.Stop();
}

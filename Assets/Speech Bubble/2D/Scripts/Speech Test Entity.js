#pragma strict

private var speech : Speech; 	// this entity's speech reference

var speechHolder : Transform; 	// position where speech bubble will stay, preferably right above a head

function Start () 
{
	SetSpeech();
}

function SetSpeech()
{
	speech = Instantiate(SpeechManager.bubble, Vector3.zero, Quaternion.identity).GetComponent(Speech);	
	speech.holder = speechHolder; // pass holder
	speech.Stop();
}

//
// below is not essential, only to demonstrate Say() functions
// (coded for PC not mobile just for testing)

private var string : String = "What to say";
private var fadeInDuration : String = "0.25";
private var fadeOutDuration : String = "0.25";

function OnGUI()
{
	string = GUI.TextField (Rect (25, 15, 120, 30), string, 250);
	fadeInDuration = GUI.TextField (Rect (25, 245, 35, 30), fadeInDuration, 250);
	fadeOutDuration = GUI.TextField (Rect (25, 277, 35, 30), fadeOutDuration, 250);
	
	var result : float;
	float.TryParse(fadeInDuration, result);
	GameObject.Find("Speech Manager").GetComponent(SpeechManager).fadeInDuration = (result < 0)? 0 : result;
	
	float.TryParse(fadeOutDuration, result);
	GameObject.Find("Speech Manager").GetComponent(SpeechManager).fadeOutDuration = (result < 0)? 0 : result;
	
	GUI.Label(Rect(65,242,150,35), "Fade in duration\n(in second)");
	GUI.Label(Rect(65,274,160,35), "Fade out duration\n(0 sec won't start fade process)");
	
	GUI.Label(Rect(25,50,150,30), "Current new line is #");
		
	if (GUI.Button(Rect(25,75,120,30), "Say for 3 seconds!"))
		if(string != String.Empty)
			speech.Say(string, 3); // say for 3 seconds
			
	if (GUI.Button(Rect(25,105,120,30), "Just Say! (infinity)"))
		if(string != String.Empty)
			speech.Say(string); // Say() without duration can stop by calling Stop() or call Say() with duration again
			
	if (GUI.Button(Rect(25,135,120,30), "3 sec (new line)"))
		if(string != String.Empty)
			speech.SayNewLine(string, 3); // New line process is heavier
			
	if (GUI.Button(Rect(25,165,120,30), "endless (new line)"))
		if(string != String.Empty)
			speech.SayNewLine(string); // New line process is heavier
			
	if (GUI.Button(Rect(25,205,120,30), "Stop"))
		speech.Stop();
		
	if (GUI.Button(Rect(150,15,120,30), "Name Print"))
		speech.Print("Treefriend\n< NPC >");
}
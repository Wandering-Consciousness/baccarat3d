#pragma strict

private var speech : SpeechBatched; 	// this entity's speech reference

var speechHolder : Transform; 	// position where speech bubble will stay, preferably right above a head

function Start () 
{
	SetSpeech();
	
	StartCoroutine(UpdateManaged());
}

function UpdateManaged()
{
	while(1){
	
		var interval : int = Random.Range(1, 5);
	
		yield WaitForSeconds (interval);
		
		speech.SayNewLine(Bulabula());
	}	
}

function Bulabula() : String
{
	switch(Random.Range(0, 10)){
	case 0 : return "Huh?";
	case 1 : return "What are you looking at!";
	case 2 : return "Hello?#Is anybody there?";
	case 3 : return "Who's there!#I know you are watching there!#Come out this instance!";
	case 4 : return "I don't know maybe it's just me#but whenever I say something#there's a speech bubble above my head";
	case 5 : return "Get this speech bubble off of me!#You want a piece of me?!?!";
	case 6 : return "This is so loud#You guys need to quiet down";
	case 7 : return "This is hilarious";
	case 8 : return "I can't see you with all these cubes!#Where are you!";
	case 9 : return "I'm here!#here!";
	}
}

function SetSpeech()
{
	speech = Instantiate(SpeechBatchedManager.bubble, speechHolder.localPosition, Quaternion.identity).GetComponent(SpeechBatched);	
	speech.holder = speechHolder; // pass holder
	speech.Stop();
}
/*
private var string : String = String.Empty;
function OnGUI()
{
	string = GUI.TextField (Rect (25, 15, 120, 30), string, 250);
	
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
}*/
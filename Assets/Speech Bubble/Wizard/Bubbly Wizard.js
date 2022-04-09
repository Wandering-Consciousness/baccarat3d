#pragma strict

import System.IO;
import System.Collections.Generic;

// change this path to point to the right Bubble Width.txt if you place it somewhere else
private var filePathWidth : String = "/Speech Bubble/Wizard/Resources/Data/Bubble Width.txt";

private var sw : StreamWriter;
private var speech : Speech; 	// this entity's speech reference
private var windowRect : Rect;
private var popupRect : Rect;
private var progressRect : Rect;
private var index : int;
private var ascii : char;
private var asciiMin : int = 32;
private var asciiMax : int = 126;
private var asciiDec : int = asciiMin;
private var phase = new List.<Function>();
private var width = new List.<int>();
private var newWidth : String = "";
private var intervalString : String = "";
private var incremntAddString : String = "";
private var incremntMaxString : String = "";
private var increment : int = 1;
private var interval : float = 0.1; // of increasing the number of characters
private var incrementAdd : int = 5; // how many to increment chars
private var incrementMax : int = 60;
private var isDone : boolean;

var manager : SpeechManager;
var speechHolder : Transform; 	// position where speech bubble will stay, preferably right above a head
var gage : GUIStyle[];

function Start () 
{
	SetSpeech();
	SetGageColor();
			
	phase.Add(StartUp);
	phase.Add(Width);
	phase.Add(Result);
	
	intervalString 		= interval.ToString();
	incremntAddString 	= incrementAdd.ToString();
	incremntMaxString 	= incrementMax.ToString();
	
	StartCoroutine(Interval());
	
	manager.fadeInDuration =
	manager.fadeOutDuration = 0; // no fade
}

function Update()
{
	windowRect.width = 250;
	windowRect.height = 100;
	windowRect.x = Screen.width / 2 - windowRect.width / 2;
	windowRect.y = Screen.height - windowRect.height;
	
	popupRect.width = 118;
	popupRect.height = 126;
		
	progressRect.width = 118;
	progressRect.height = 45;
	progressRect.x = Screen.width - progressRect.width;
}

function SetGageColor()
{
    var texture : Texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);

    texture.SetPixel(0, 0, Color.white);
    texture.SetPixel(1, 0, Color.white);
    texture.SetPixel(0, 1, Color.white);
    texture.SetPixel(1, 1, Color.white);

    texture.Apply();

    gage[1].normal.background = texture;
    
    texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

    texture.SetPixel(0, 0, Color.black);
    texture.SetPixel(1, 0, Color.black);
    texture.SetPixel(0, 1, Color.black);
    texture.SetPixel(1, 1, Color.black);

    texture.Apply();

    gage[0].normal.background = texture;
}

function SetSpeech()
{
	speech = Instantiate(SpeechManager.bubble, Vector3.zero, Quaternion.identity).GetComponent(Speech);	
	speech.holder = speechHolder; // pass holder
	speech.Stop();
}

function Interval()
{
	while(1){
		
		yield WaitForSeconds(interval);
		
		if(index == 1)
			speech.Say(GetTextIncreasedWidth(increment));
		
		increment += incrementAdd;
		if(increment > incrementMax)
			increment = 1;
	}
}

function GetTextIncreasedWidth(count : int) : String
{
	var buf : String;
	
	if(asciiDec == asciiMin) // for space
		buf = "[";
	
	for(var i : int = 0; i < count; i++)
		buf += ascii;
		
	if(asciiDec == asciiMin)
		buf += "]";
		
	return buf;
}

function DoMyWindow (windowID : int) 
{		
	if(index < phase.Count)
		phase[index]();
}

function DoMyPopup (windowID : int) 
{		
	GUILayout.BeginVertical();
	
	GUILayout.BeginHorizontal();	
	GUILayout.Label("Interval : ");
	intervalString 		= GUILayout.TextField(intervalString, 4);
	intervalString 		= Regex.Replace(intervalString, "[^0-9. ]", ""); // character limit
	GUILayout.EndHorizontal();
	
	GUILayout.BeginHorizontal();	
	GUILayout.Label("Add : ");
	incremntAddString 	= GUILayout.TextField(incremntAddString, 2);
	incremntAddString 	= Regex.Replace(incremntAddString, "[^0-9 ]", ""); // character limit
	GUILayout.EndHorizontal();
	
	GUILayout.BeginHorizontal();	
	GUILayout.Label("Max : ");
	incremntMaxString 	= GUILayout.TextField(incremntMaxString, 3);
	incremntMaxString 	= Regex.Replace(incremntMaxString, "[^0-9 ]", ""); // character limit
	GUILayout.EndHorizontal();
	
	if(GUILayout.Button("Ok")){
		interval 		= ParseFloat(intervalString);
		incrementAdd 	= ParseInt(incremntAddString);
		incrementMax 	= ParseInt(incremntMaxString);
	}
	
	GUILayout.EndVertical();	
}

function DoMyProgress (windowID : int) 
{		
	GUILayout.BeginVertical();
		
	for(var i : int = 0; i < gage.length; i++)
		GUILayout.Label("", gage[i]);
		
	var buf : float = (asciiDec - asciiMin) + 1;
	gage[1].fixedWidth = buf / manager.characterWidth.length * gage[0].fixedWidth;
		
	GUILayout.EndVertical();	
}

function OnGUI ()
{
	GUI.Window (0, windowRect, DoMyWindow, "Bubbly Wizard");
	
	if(index == 1 || index == 2)	
		GUI.Window (1, popupRect, DoMyPopup, "Settings");
		
	if(index == 1)	
		GUI.Window (2, progressRect, DoMyProgress, (asciiDec - asciiMin + 1) + " / " + manager.characterWidth.length);
}

function StartUp()
{
	GUILayout.BeginVertical();
	
	GUILayout.Label("I'll help you find widths of about 90 characters in no time! (3D does not need this)");
	GUILayout.FlexibleSpace();
	
	GUILayout.BeginHorizontal();	
	
	GUILayout.FlexibleSpace();
	
	if(GUILayout.Button("Proceed")){
		index++;
		SetExistingWidth();		
	}
			
	GUILayout.FlexibleSpace();
	
	GUILayout.EndHorizontal();
	
	GUILayout.EndVertical();	
}

function SetExistingWidth()
{
	if(asciiDec - asciiMin < manager.characterWidth.length)
		newWidth = manager.characterWidth[asciiDec - asciiMin].ToString();
}

function Width()
{
	ascii = asciiDec;

	GUILayout.BeginVertical();
		
	GUILayout.Label("Let's set width of < " + ascii + " >" + ((ascii == asciiMin)? " (space)" : ""));
	GUILayout.FlexibleSpace();
	
	newWidth = GUILayout.TextField(newWidth, 2);
	newWidth = Regex.Replace(newWidth, "[^0-9 ]", ""); // character limit
	
	manager.characterWidth[asciiDec - asciiMin] = ParseInt(newWidth);
	
	GUILayout.BeginHorizontal();	
	
	GUILayout.FlexibleSpace();
	
	if(GUILayout.Button("Set"))
		Next();		
				
	GUILayout.FlexibleSpace();
	
	GUILayout.EndHorizontal();
	
	GUILayout.EndVertical();	
}

function Next()
{
	width.Add(ParseInt(newWidth)); // chosen new width
		
	if(++asciiDec > asciiMax)
		index++;
			
	SetExistingWidth();
		
	increment = 1;
}

function Result()
{
	if(!isDone)
		WriteFileWidth();

	GUILayout.BeginVertical();
	
	GUILayout.Label("Done! <Bubble Width.txt> have been generated and replaced the old ones successfully! Cya!");
	GUILayout.FlexibleSpace();
	
	GUILayout.EndVertical();	
	
	speech.SayNewLine("Yay! Finished!#Don't forget to reimport the text file~!");
}

function ParseInt(string : String) : int
{
	var result : int;
	int.TryParse(string, result);
	
	return result;
}

function ParseFloat(string : String) : float
{
	var result : float;
	float.TryParse(string, result);
	
	return result;
}

function WriteFileWidth()
{
    sw = new StreamWriter(Application.dataPath + filePathWidth);

	for(var i : int = 0; i < width.Count; i++){
		ascii = i + asciiMin;
		
		if(i < width.Count - 1)
    		sw.WriteLine(width[i] + "\t// " + ascii);
    	else
    		sw.Write(width[i] + "\t// " + ascii);
    }

    sw.Close();
    
    isDone = true;
}
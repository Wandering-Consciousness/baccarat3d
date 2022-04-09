#pragma strict

import System.IO;

enum Bubble { Top, Bottom, Text, Total }
enum BubbleSkin { White, Black, Gradient1, Gradient2, Gradient3, Chocolate }

static var reference : SpeechManager;
static var bubble : GameObject; // speech ballon gameobject to be instantiated

@HideInInspector var font : Font;
@HideInInspector var fontMat : Material;
@HideInInspector var is4G : boolean;
@HideInInspector var characterWidth : int[]; // characters' widths read on start up
@HideInInspector var initialAlpha : float[]; // of 3 gui elements
@HideInInspector var lineHeight : int; 		 // used for new line process (will be doubled for 4g)
@HideInInspector var asciiMin : int = 32;

public var cam : Camera;				// camera to process speech bubbles with (main camera or UI camera can be one)
public var camT : Transform;			// the camera's transform cached
var helperSkin : GUISkin;	

var skin : BubbleSkin;			
var textColor : Color;
var fontName : String;			// that is in resources folder
var visibleRange : int; 		// speech will not be activated if out of range even if Say() is called (unit)
var newLineChar : String;		// chararacter to start a new line, usually speical character, set it to only one character
var fadeInDuration : float;		// how long it will take as it fades in or out in seconds
var fadeOutDuration : float;	// put zero if you don't want fade feature
var heightRevision : float;		// if bubble can't follow text's height, increase this value (it happens when font's line height ignores floating point)
var test4G : boolean;			// force doubling UI

function Awake()
{
	reference = this;

	Settings();
	Load();
	GetWidth();	
}

function Settings()
{
	is4G = (Is4G() || test4G);
	
	if(is4G)
		heightRevision *= 3f; // Change from 2, by Simon
	
	initialAlpha = new float[Bubble.Total];
}

function SetInitialAlpha(top : float, bottom : float, text : float)
{
	initialAlpha[Bubble.Top] 		= top;
	initialAlpha[Bubble.Bottom] 	= bottom;
	initialAlpha[Bubble.Text] 		= text;
}

function Load()
{
    //fontName = fontName + ((is4G)? " x2" : String.Empty); // Disabled by Simon coz using customized font file
    //Debug.LogError("fontName: "+fontName);
	font = Resources.Load("Font/" + fontName, Font);
	fontMat = font.material;
	fontMat.SetColor ("_Color", textColor);

    helperSkin.label.font = font;

	lineHeight = helperSkin.label.lineHeight;
	
	bubble = Resources.Load("2D/Skin/" + skin + "/Bubble" + ((is4G)? " x2" : String.Empty), GameObject);
}

function GetWidth()
{	
	var textfile : TextAsset = Resources.Load("Data/Bubble Width", TextAsset);
    
    var line : String[] = textfile.text.Split("\n"[0]);
    
    characterWidth = new int[line.length];
    
    for(var i : int = 0; i < characterWidth.length; i++){ // double it if 4g
    	
    	var result : int;
    	var buf : String[] = line[i].Split('\t'[0]);
					
		if(int.TryParse(buf[0], result))
			characterWidth[i] = result * ((is4G)? 2 : 1);
   	}
    
    textfile = null; // lose reference to be collected
}

function Is4G()
{
	// ipod 4g?
	//return ((Application.platform == RuntimePlatform.IPhonePlayer &&
	//	Screen.width == 960 && Screen.height == 640));

    // Modified by Simon
    return Screen.width >= Speech.four4width;
}
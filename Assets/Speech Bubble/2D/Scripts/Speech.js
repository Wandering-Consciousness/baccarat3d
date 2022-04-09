#pragma strict

@HideInInspector var holder : Transform; // position where it will stick to, preferably right above a head
@HideInInspector var isPrinting : boolean;	// npc

private var manager : SpeechManager;
private var startTime : float;  	// for timer purpose
private var width : int;			// width of speech bubble set
private var second : int; 			// say duration
private var isInfinity : boolean; 	// say without duration
private var initialBubbleHeight : int;
private var curTextCol : Color;		// used for fade
private var preTextAlpha : float;
private var lineCount : int;

var t : Transform;
var object : GameObject;
var string : GUIText; 		// GUI Text cached
var gui : GUITexture[];		// UI elements cached
var triangle : int; // Added by Simon: 1 = draw bottom triangle on left, 2 - draw it right, anything else - center

function Awake()
{
	manager = SpeechManager.reference;

	Settings();		
}

function Update()
{	
	// transform is behind a camera? or time has passed?	
	if(!IsPresentable() || (Time.time - startTime > second && !isInfinity))
		Stop();
		
	Fade();
}

function Fade()
{
	// fade out override fade in
	if(Time.time - startTime > second - manager.fadeOutDuration && !isInfinity){
		if(manager.fadeOutDuration)	
			FadeOut();
	}
	else{
		if(manager.fadeInDuration && curTextCol.a != preTextAlpha)	
			FadeIn();
	}
}

// Added by Simon
static var four4width = 460;

function Is4G()
{
 // ipod 4g?
 //return ((Application.platform == RuntimePlatform.IPhonePlayer &&
 //  Screen.width == 960 && Screen.height == 640));

    // Modified by Simon
#if UNITY_IPHONE
    return true;
#elif UNITOR_EDITOR
    return true;
#else
    return Screen.width >= Speech.four4width;
#endif
}

function Settings()
{
	initialBubbleHeight = gui[Bubble.Top].pixelInset.height;
	
	string.font	= manager.font;
	string.material = manager.fontMat;
    string.fontSize = (Is4G() ? 34 : 22); // Added by Simon

	if(!manager.initialAlpha[0]) // pass in its alphas initially
		manager.SetInitialAlpha(gui[Bubble.Top].color.a, 
								gui[Bubble.Bottom].color.a, 
								manager.textColor.a);
	ResetColor();
}

function GetLongestLine() : String
{
	string.text = Regex.Replace(string.text, manager.newLineChar, '\n');
			
	var buf : String[] = string.text.Split('\n'[0]);
	
	lineCount = buf.length;
	
	var longest : String = buf[0];	
	for(var i : int = 1; i < buf.length; i++) {
        if (buf[i].Contains("        ")) {
            continue;
        }
		if(longest.length < buf[i].length) {
			longest = buf[i]; // get the longest line
        }
    }

	return longest;
}

function GetWidth(newline : boolean) : int
{
	width = 0;
	
	var ascii : int;	
	var chars : char[] = ((newline)? GetLongestLine() : string.text).ToCharArray();
	
	for(var j : int = 0; j < chars.length; j++){
		ascii = chars[j];
	
		// sum all character's width to determine object's width
		//if(ascii - manager.asciiMin < 0 || ascii - manager.asciiMin >= manager.characterWidth.length)
		//	width += manager.characterWidth[7]; // just apply default width for special characters
		//else
		//	width += manager.characterWidth[ascii - manager.asciiMin]; // got from text

        // Widths hacked by Simon for custom font using English and Japanese
        if (isJapanese())
            width += (Is4G() ? 2 : 1) * 20;
       else
            width += (Is4G() ? 2 : 1) * 9;
	}	
	
	return width;
}

// Because the Speech Bubble asset we bought from the store is in JavaScript we couldn't use the Utils.cs / Language Manager all the other scripts use.
// So instead each game object in the screen that is setup for a speech bubble needs to have its corresponding English and Japanese arrays filled in
// manually.
function isJapanese() {
    var lang : String;
    lang = Application.systemLanguage.ToString();
    return lang.Equals("Japanese");
}

function AdjustSize(newline : boolean)
{	
	width = GetWidth(newline);
	
		var heightRevision : float = lineCount * manager.heightRevision;
	
	// lengthen or shrink the bubble's width
	gui[Bubble.Top].pixelInset.x 		= -width / 2 - manager.characterWidth[2];
	gui[Bubble.Top].pixelInset.width 	= width + manager.characterWidth[2] * 2;	
	
	gui[Bubble.Top].pixelInset.height 	+= (lineCount - 1) * manager.lineHeight + manager.characterWidth[2] / 10 + heightRevision;

    // Added by Simon
    // Shift the speech bubble triangle left (1) or right (2) or center (any other value)
    if (triangle == 1) {
        gui[Bubble.Bottom].pixelInset.x -= (width/2 - width*0.1f); // left
        triangle = 3; // only adjust size once
    }
    else if (triangle == 2) {
        gui[Bubble.Bottom].pixelInset.x += (width/2 - width*0.1f); // right
        triangle = 3; // only adjust size once
    }
    // else: Center: default: need nothing.
}

function OnSayEnter(text : String)
{
	string.alignment = TextAlignment.Left;
	
	string.text = text;	
	gui[Bubble.Top].pixelInset.height 
		= initialBubbleHeight;
		
	lineCount = 1;
		
	isPrinting = false;
}

function Say(text : String, second : float)
{
	if(IsPresentable()){

		isInfinity = false;
		startTime = Time.time;
		this.second = second;

		OnSayEnter(text);
		AdjustSize(false);
	
		Present(); // now present
	}
}

function Say(text : String)
{
	if(IsPresentable()){
	
		isInfinity = true; // infinity state stops when Stop() is called manually or another Say() with duration is called
	
		OnSayEnter(text);
		AdjustSize(false);
	
		Present(); // will need to call Stop() manually since the timer didn't started
	}
}

// new line process involves somewhat heavy string operation
// if you don't need new line call Say() instead
function SayNewLine(text : String, second : float)
{
	if(IsPresentable()){

		isInfinity = false;
		startTime = Time.time;
		this.second = second;

		OnSayEnter(text);
		AdjustSize(true);
	
		Present(); // now present
	}
}

function SayNewLine(text : String)
{
	if(IsPresentable()){
		
		isInfinity = true; // infinity state stops when Stop() is called manually or another Say() with duration is called
	
		OnSayEnter(text);
		AdjustSize(true);
	
		Present(); // will need to call Stop() manually since the timer didn't started
	}
}

function Print(text : String)
{	
	if(IsPresentable()){	
	
		string.alignment = TextAlignment.Center;
	
		object.SetActiveRecursively(false);
	
		isPrinting =
		isInfinity = true; // infinity state stops when Stop() is called manually or another Say() with duration is called
	
		string.text = text;	
		gui[Bubble.Top].pixelInset.height 
			= initialBubbleHeight;
	
		if(manager.fadeInDuration)
			OnFadeInEnter(); // start fade in?
		else
			ResetAlphas(); // need alpha reset?
		
		object.active = true;
	}
}

function Present()
{
	if(manager.fadeInDuration)
		OnFadeInEnter(); // start fade in?
	else
		ResetAlphas(); // need alpha reset?
	
	object.SetActiveRecursively(true);
}

function Stop()
{
	second = 0; // stops timer
	isPrinting =
	isInfinity = false;	
	
	if(t.position.z > 0 && manager.fadeOutDuration && curTextCol.a > 0)	
		return; // fade out first if wanted to stop	but only if infront of cam

	if(object.active)
		object.SetActiveRecursively(false);
}

function IsPresentable() : boolean
{
	t.position = manager.cam.WorldToViewportPoint (holder.position);	
	
	// see if the entity is infront of camera and also in visible range
	return (t.position.z > 0 && (t.position - manager.camT.position).sqrMagnitude < manager.visibleRange * manager.visibleRange);	
}

function ResetColor()
{
	// initialize alphas
	for(var i : int = 0; i < gui.length; i++)
		gui[i].color.a = 0;
		
	var col : Color = Color(manager.textColor.r, 
							manager.textColor.g, 
							manager.textColor.b, 
							0);
		
	string.material.SetColor ("_Color", col);
		
	curTextCol = col;	
}

function OnFadeInEnter()
{
	ResetColor();
	
	FadeIn();
}

function FadeIn()
{
	// determine when to stop
	preTextAlpha = curTextCol.a;
	
	for(var i : int = 0; i < gui.length; i++){ // GUI Textures
		
		gui[i].color.a += manager.initialAlpha[i] * Time.deltaTime / manager.fadeInDuration;
		
		if(gui[i].color.a > manager.initialAlpha[i])
			gui[i].color.a = manager.initialAlpha[i];
	}
	
	// GUI Text
	curTextCol.a += manager.initialAlpha[Bubble.Text] * Time.deltaTime / manager.fadeInDuration;
	
	string.material.SetColor ("_Color", curTextCol);
		
	if(curTextCol.a > manager.initialAlpha[Bubble.Text]){
		curTextCol.a = manager.initialAlpha[Bubble.Text];
		string.material.SetColor ("_Color", curTextCol);
	}
}

function FadeOut()
{
	for(var i : int = 0; i < gui.length; i++){ // GUI Textures
		gui[i].color.a -= manager.initialAlpha[i] * Time.deltaTime / manager.fadeOutDuration;
		
		if(gui[i].color.a < 0)
			gui[i].color.a = 0;
	}
	
	// GUI Text
	curTextCol.a -= manager.initialAlpha[Bubble.Text] * Time.deltaTime / manager.fadeOutDuration;
	
	if(curTextCol.a < 0)
		curTextCol.a = 0;
	
	string.material.SetColor ("_Color", curTextCol);
}

function ResetAlphas()
{
	for(var i : int = 0; i < gui.length; i++)
		gui[i].color.a = manager.initialAlpha[i];
	
	curTextCol.a = manager.initialAlpha[Bubble.Text];
	
	string.material.SetColor ("_Color", curTextCol);
}
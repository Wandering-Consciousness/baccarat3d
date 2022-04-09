#pragma strict

enum BubbleShape { Simple, Round, Total }

static var reference : SpeechBatchedManager;
static var bubble : GameObject; // speech ballon gameobject to be instantiated

@HideInInspector var initialVerts : Vector3[];		// initial vertexes' positions of top bubble saved
@HideInInspector var topVerts : int[];			
@HideInInspector var leftVerts : int[];			
@HideInInspector var rightVerts : int[];	
@HideInInspector var font : Font;
@HideInInspector var camPerspectiveT : Transform;	// perspective
@HideInInspector var camOrthographicT : Transform;	// orthographic
@HideInInspector var lineHeight : int;				// used for new line process
@HideInInspector var asciiMin : int = 32;

var camPerspective : Camera;	// perspective
var camOrthographic : Camera;	// orthographic
var bubbleMat : Material;
var textMat : Material;	
var helperSkin : GUISkin;	

var shape : BubbleShape;
var isPixelPerfect : boolean;	// bubbles should be pixel perfect based on Size even when the size of screen changes?
var bubbleColor : Color;
var textColor : Color;
var fontName : String;			// that is in resources folder
var fontTexture : Texture2D;	// font's texture
var size : float;				// of bubble
var visibleRange : int; 		// speech will not be activated if out of range even if Say() is called (unit)
var newLineChar : String;		// chararacter to start a new line, usually speical character, set it to only one character	
var targetToFollow : Transform;	// has a hero that moves around?
var test4G : boolean;			// force doubling UI

function Awake () 
{
	reference = this;
	
	Load();
	SetFontColor();
	SetBubbleColor();
	
	camPerspectiveT = camPerspective.transform;
	camOrthographicT = camOrthographic.transform;
}

function Update()
{	
	if(isPixelPerfect){ // keep it the same size no matter what?
		camOrthographic.orthographicSize = Screen.height / 2;	
		camOrthographic.orthographicSize /= size;
		
		if(Is4G() || test4G)
			camOrthographic.orthographicSize /= 2; // twice bigger
	}
		
	if(targetToFollow)
		camOrthographicT.position = targetToFollow.position;
}

function Load()
{
	font = Resources.Load("Font/" + fontName, Font);	
	bubble = Resources.Load("3D/Shape/" + shape + "/Bubble Batched", GameObject);
}

function SetFontColor()
{
	textMat.SetColor ("_Color", textColor);
	textMat.mainTexture = fontTexture;
	
	helperSkin.label.font = font;
	
	lineHeight = helperSkin.label.lineHeight;
}

function SetBubbleColor()
{
    var texture : Texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);

    texture.SetPixel(0, 0, bubbleColor);
    texture.SetPixel(1, 0, bubbleColor);
    texture.SetPixel(0, 1, bubbleColor);
    texture.SetPixel(1, 1, bubbleColor);

    texture.Apply();

    bubbleMat.mainTexture = texture;
}

function SetInitialVerts(mesh : Mesh)
{
	initialVerts = mesh.vertices;
	
	if(shape == BubbleShape.Simple) Simple();
	else							Round();
}

function Simple()
{
	topVerts = new int[3];
	
	topVerts[0] = 0;
    topVerts[1] = 3;
    topVerts[2] = 7;
    
    rightVerts = new int[2];
    
    rightVerts[0] = 0;
    rightVerts[1] = 2;
    
    leftVerts = new int[2];
    
    leftVerts[0] = 7;
    leftVerts[1] = 6;
}

function Round()
{
	topVerts = new int[7];
	
	topVerts[0] = 9;
    topVerts[1] = 11;
    topVerts[2] = 12;
    topVerts[3] = 13;
    topVerts[4] = 14;
    topVerts[5] = 15;
    topVerts[6] = 3;

	rightVerts = new int[6];
	
	rightVerts[0] = 0;
    rightVerts[1] = 2;
    rightVerts[2] = 8;
    rightVerts[3] = 9;
    rightVerts[4] = 12;
    rightVerts[5] = 13;

	leftVerts = new int[6];

    leftVerts[0] = 6;
    leftVerts[1] = 7;
    leftVerts[2] = 10;
    leftVerts[3] = 11;
    leftVerts[4] = 14;
    leftVerts[5] = 15;
}

function Is4G()
{
	// ipod 4g?
	//return ((Application.platform == RuntimePlatform.IPhonePlayer &&
	//	Screen.width == 960 && Screen.height == 640));

    // Modified by Simon
    return Screen.width >= Speech.four4width;
}
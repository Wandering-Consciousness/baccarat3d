#pragma strict

class Text{
	
	var object : GameObject;
	var mesh : TextMesh;
	var renderer : Renderer;
}

@HideInInspector var holder : Transform; // position where it will stick to, preferably right above a head
@HideInInspector var isPrinting : boolean;	// npc

private var manager : SpeechBatchedManager;
private var isInfinity : boolean; 	// say without duration
private var startTime : float;  	// for timer purpose
private var second : int; 			// say duration
private var mesh : Mesh;			// of bubble
private var width : int;			// width of speech bubble set

var t : Transform;
var object : GameObject;
var meshFilter : MeshFilter;		// of bubble
var text : Text;					// text mesh

function Awake () 
{
	manager = SpeechBatchedManager.reference;

	Settings();		
}

function Update () 
{
	// transform is behind a camera? or time has passed?	
	if(!IsPresentable() || (Time.time - startTime > second && !isInfinity))
		Stop();
}

/* deprecated
function Simulate2D()
{
	t.LookAt(manager.camPerspectiveT.position + manager.camPerspectiveT.forward * -Mathf.Infinity);
	
	var distance : float = Vector3.Distance(manager.camPerspectiveT.position, t.position) / 27.75;
	var angle : float = Vector3.Angle(manager.camPerspectiveT.right, t.position - manager.camPerspectiveT.position);
			
	if(angle > 90) angle = 90 - (angle - 90); // keep under sin scope
	
	var close : float = distance * manager.sinTable[angle];
	
	t.localScale = Vector3(close, close, close);
}*/

function Settings()
{
	mesh = meshFilter.mesh;
	
	text.mesh.font = manager.font;
	
	if(!manager.initialVerts.length) // only one time on start up
		manager.SetInitialVerts(mesh);
}

function NewLineProcess(height : float)
{
	var vertices : Vector3[] = mesh.vertices;
	
	for(var i : int = 0; i < manager.topVerts.length; i++)
    	vertices[manager.topVerts[i]].y += height;

    mesh.vertices = vertices;
}

function NewWidthProcess(width : float)
{
	var vertices : Vector3[] = mesh.vertices;
	
	for(var i : int = 0; i < manager.leftVerts.length; i++)
    	vertices[manager.leftVerts[i]].x -= width;
    	
    for(i = 0; i < manager.rightVerts.length; i++)
    	vertices[manager.rightVerts[i]].x += width;

    mesh.vertices = vertices;
}

function AdjustSize(newline : boolean)
{
	if(newline)
		text.mesh.text = Regex.Replace(text.mesh.text, manager.newLineChar, '\n');
		
	text.object.active = true; // activated beforehand
	
	// lengthen or shrink the bubble's width
	NewWidthProcess(text.renderer.bounds.extents.x * 0.6855); // factor of .6855
	NewLineProcess(text.renderer.bounds.extents.y * 0.4963 - 0.35); // factor of .4963 and revision of .35
	
	mesh.RecalculateBounds();
}

function OnSayEnter(string : String)
{
	text.mesh.alignment = TextAlignment.Left;
	
	text.mesh.text = string;	
		
	var vertices : Vector3[] = manager.initialVerts;

    mesh.vertices = vertices;
    
    isPrinting = false;
}

function Say(string : String, second : float)
{
	if(IsPresentable()){	
	
		isInfinity = false;
		startTime = Time.time;
		this.second = second;

		OnSayEnter(string);
		AdjustSize(false);
	
		Present(); // now present
	}
}

function Say(string : String)
{
	if(IsPresentable()){	
		
		isInfinity = true; // infinity state stops when Stop() is called manually or another Say() with duration is called
	
		OnSayEnter(string);
		AdjustSize(false);
	
		Present(); // will need to call Stop() manually since the timer didn't started
	}
}

// new line process involves somewhat heavy string operation
// if you don't need new line call Say() instead
function SayNewLine(string : String, second : float)
{
	if(IsPresentable()){	
	
		isInfinity = false;
		startTime = Time.time;
		this.second = second;

		OnSayEnter(string);
		AdjustSize(true);
	
		Present(); // now present
	}
}

function SayNewLine(string : String)
{
	if(IsPresentable()){	
	
		isInfinity = true; // infinity state stops when Stop() is called manually or another Say() with duration is called
	
		OnSayEnter(string);
		AdjustSize(true);
	
		Present(); // will need to call Stop() manually since the timer didn't started
	}
}

function Print(string : String)
{	
	if(IsPresentable()){	
	
		text.mesh.alignment = TextAlignment.Center;
	
		object.SetActiveRecursively(false);
	
		isPrinting =
		isInfinity = true; // infinity state stops when Stop() is called manually or another Say() with duration is called
	
		text.mesh.text = string;	
		
		object.active = true;
//		text.active = true; // Simon commented out to upgrade to unity 2017
	}
}

function Present()
{
	object.SetActiveRecursively(true);
}

function Stop()
{
	second = 0; // stops timer
	isPrinting =
	isInfinity = false;	
	
	if(object.active)
		object.SetActiveRecursively(false);
}

function IsPresentable() : boolean
{
	var screen : Vector3 = manager.camPerspective.WorldToScreenPoint(holder.position);
	t.position = manager.camOrthographic.ScreenToWorldPoint(new Vector3(Mathf.Round(screen.x), Mathf.Round(screen.y), (screen.z)));
	
	// see if the entity is infront of camera and also in visible range
	return ((t.position - manager.camPerspectiveT.position).sqrMagnitude < manager.visibleRange * manager.visibleRange);	
}
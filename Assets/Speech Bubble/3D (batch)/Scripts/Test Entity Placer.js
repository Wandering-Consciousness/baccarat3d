#pragma strict

private var count : int;

var entity : GameObject;

function Start () {

}

function OnGUI()
{
	if (GUI.Button(Rect(25,35,120,30), "New entity (" + count + ")")){		
		Instantiate(entity, Vector3(Random.Range(0, 40), 0, Random.Range(0, 40)), Quaternion.identity);	
		count++;
	}
}
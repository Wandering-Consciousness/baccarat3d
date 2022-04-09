#pragma strict

private var vel : float;

@HideInInspector var velStatic : float;

var t : Transform;
var target : Transform;
var speed : float;
var offset : Vector3; // offset of camera
var lookOffset : Vector3;

function Update()
{
	vel -= speed * Time.deltaTime;
	
	t.LookAt( target.position + lookOffset ); 	
	t.position = target.position + GetVectorRotated(offset.z, vel + velStatic) + Vector3(0, offset.y, 0); // rotate while looking at target	
}

function GetVectorRotated(range : float, angle : float) : Vector3
{	
	var vector : Vector3 = Vector3(0, 0, range);

	var r : float = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
	var theta : float = Mathf.Atan(vector.z / vector.x);

	var rad : float = angle * Mathf.Deg2Rad;

	// to rectangular coordinates again with extra radian thrown in it
	vector.x = (r * Mathf.Cos(theta + rad));
	vector.z = (r * Mathf.Sin(theta + rad));

	//Debug.Log(vector);

	return vector;
}
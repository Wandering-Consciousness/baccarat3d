using UnityEngine;
using UnityEditor;

public static class GameObjectUtils
{
	private static Vector3 GetCreationPosition()
	{
		Camera editorCamera = SceneView.lastActiveSceneView.camera;
		if (editorCamera == null)
			return Vector3.zero;
		
		RaycastHit hit;
		
		if (Physics.Raycast(editorCamera.transform.position, editorCamera.transform.forward, out hit))
			return hit.point;
		else
			return Vector3.zero;
	}		
	
	private static void EndCreateGameObject(GameObject _gameObject)
	{
		_gameObject.transform.position = GetCreationPosition();
		
		MeshRenderer meshRenderer = (MeshRenderer)_gameObject.AddComponent<MeshRenderer>();
				
		Material defaultMaterial = new Material(Shader.Find("Diffuse"));
		meshRenderer.material = defaultMaterial;		
		
		Undo.RegisterCreatedObjectUndo(_gameObject, "Create " + _gameObject.name);
		
		GameObject[] gameObjs = { _gameObject };			
		Selection.objects = gameObjs;	
	}
	
	
	public static void CreateSphere(string _name, float _fRadius, int _iSubdivision)
	{
		GameObject newSphere = new GameObject(_name);
		                           
		Mesh mesh;
		GeometryUtils.CreateSphere(out mesh, _iSubdivision, _fRadius);
		mesh.Optimize();
		                           
		MeshFilter meshFilter = (MeshFilter)newSphere.AddComponent<MeshFilter>();		
		meshFilter.mesh = mesh;	
		
		SphereCollider sphereCollider = (SphereCollider)newSphere.AddComponent<SphereCollider>();
		sphereCollider.radius = _fRadius;		
		
		EndCreateGameObject(newSphere);
	}
		
	public static void CreateSlope(string _name, Vector2 _planeDims, Vector2 _heights, Vector2 _uvTiling, Vector2 _uvOffset)
	{
		GameObject newSlope = new GameObject(_name);
			
		Mesh mesh;
		GeometryUtils.CreateSlopeMesh(out mesh, _planeDims, _heights, _uvTiling, _uvOffset);
		mesh.Optimize();
		
		MeshFilter meshFilter = (MeshFilter)newSlope.AddComponent<MeshFilter>();		
		meshFilter.mesh = mesh;	
		
		MeshCollider meshCollider = (MeshCollider)newSlope.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = mesh;
		meshCollider.convex = true;
		
		EndCreateGameObject(newSlope);
	}
	
	public static void CreatePlane(string _name, Vector2 _planeDims, Vector2 _uvTiling, Vector2 _uvOffset)
	{
		GameObject newPlane = new GameObject (_name);
		
		Mesh mesh;
		GeometryUtils.CreatePlaneMesh(out mesh, _planeDims, _uvTiling, _uvOffset);
		mesh.Optimize();
		                           
		MeshFilter meshFilter = (MeshFilter)newPlane.AddComponent<MeshFilter>();		
		meshFilter.mesh = mesh;	
		
		//Create an "thin" box as a collider, it seem to work better		
		BoxCollider boxCollider = (BoxCollider)newPlane.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(_planeDims.x, 0.02f, _planeDims.y);
		
		EndCreateGameObject(newPlane);	
	}
	
	public static void CreateCylinder(string _name, 
	                                      int _iRenderSubdivisionCount, 
	                                      int _iCollisionSubdivisionCount, 
	                                      float _fTopRadius, 
	                                      float _fBottomRadius, 
	                                      float _fHeight, 
	                                      bool _bSmoothNormal, 
	                                      Vector2 _uvTiling,
	                                      Vector2 _uvOffset)
	{
		GameObject newCylinder = new GameObject (_name);
		
		Mesh mesh;
		GeometryUtils.CreateCylinderMesh(out mesh, _iRenderSubdivisionCount, _fTopRadius, _fBottomRadius, _fHeight, _bSmoothNormal, _uvTiling, _uvOffset);
		mesh.Optimize();
		                           
		MeshFilter meshFilter = (MeshFilter)newCylinder.AddComponent<MeshFilter>();		
		meshFilter.mesh = mesh;	
		
		MeshCollider meshCollider = (MeshCollider)newCylinder.AddComponent<MeshCollider>();
		meshCollider.convex = true;
		
		if (_iRenderSubdivisionCount != _iCollisionSubdivisionCount)
		{
			Mesh collisionMesh;
			GeometryUtils.CreateCylinderMesh(out collisionMesh, _iCollisionSubdivisionCount, _fTopRadius, _fBottomRadius, _fHeight, false, _uvTiling, _uvOffset);
			meshCollider.sharedMesh = collisionMesh;
		}
		else
		{
			meshCollider.sharedMesh = mesh;
		}
		
		EndCreateGameObject(newCylinder);	
	}
	
	public static void CreateBox(string _name, Vector3 _boxDims, Vector2 _uvTiling, Vector2 _uvOffset)
	{
		GameObject newBox = new GameObject ("NewBox");
		
		BoxCollider boxCollider = (BoxCollider)newBox.AddComponent<BoxCollider>();		
		boxCollider.size = _boxDims;
		boxCollider.center = new Vector3(0.0f, _boxDims.y * 0.5f, 0.0f);
		
		Mesh mesh;
		GeometryUtils.CreateBoxMesh(out mesh, _boxDims, _uvTiling, _uvOffset);
		mesh.Optimize();
		
		MeshFilter meshFilter = (MeshFilter)newBox.AddComponent<MeshFilter>();		
		meshFilter.mesh = mesh;	
		
		EndCreateGameObject(newBox);
	}
		
	
}
	
	
	
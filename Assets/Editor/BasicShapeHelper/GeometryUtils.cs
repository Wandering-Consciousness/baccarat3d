using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

static class GeometryUtils
{
	public static void CreateBoxMesh(out Mesh _mesh, Vector3 _boxDims, Vector2 _uvTiling, Vector2 _uvOffset)
	{		
		Vector2 planeDims = new Vector2(_boxDims.x, _boxDims.z);
		Vector2 heights = new Vector2(_boxDims.y, _boxDims.y);
		
		CreateSlopeMesh(out _mesh, planeDims, heights, _uvTiling, _uvOffset);
		                
		_mesh.name = "BoxMesh";	
	}
	
	public static void CreateSlopeMesh(out Mesh _mesh, Vector2 _planeDims, Vector2 _heights, Vector2 _uvTiling, Vector2 _uvOffset)
	{		
		_mesh = new Mesh();
		_mesh.name = "Slope";
		
		Vector3[] baseVertices = new Vector3[8];
		
		Vector2 planeHalfDims = _planeDims;
		planeHalfDims *= 0.5f;
		
		//TO DO: handle this better
		//Remove duplicate vertices if one of the height is 0
		if (_heights.x < 0.01f)			
			_heights.x = 0.01f;
		
		if (_heights.y < 0.01f)
			_heights.y = 0.01f;
					
		//  7------6
		// /      /|
		// 4------5|
		// |      |  
		//
		//  3------2
		// /      /
		// 0------1
		
		baseVertices[0] = new Vector3(-planeHalfDims.x, 0.0f, -planeHalfDims.y);
		baseVertices[1] = new Vector3(planeHalfDims.x, 0.0f, -planeHalfDims.y);
		baseVertices[2] = new Vector3(planeHalfDims.x, 0.0f, planeHalfDims.y);
		baseVertices[3] = new Vector3(-planeHalfDims.x, 0.0f, planeHalfDims.y);
		
		baseVertices[4] = new Vector3(-planeHalfDims.x, _heights.x, -planeHalfDims.y);
		baseVertices[5] = new Vector3(planeHalfDims.x, _heights.y, -planeHalfDims.y);
		baseVertices[6] = new Vector3(planeHalfDims.x, _heights.y, planeHalfDims.y);
		baseVertices[7] = new Vector3(-planeHalfDims.x, _heights.x, planeHalfDims.y);
		
		const int iTotalVertexCount = 24;
		
		Vector3[] vertices = new Vector3[iTotalVertexCount];
		
		//Y-
		vertices[0] = baseVertices[1];
		vertices[1] = baseVertices[2];
		vertices[2] = baseVertices[3];
		vertices[3] = baseVertices[0];
		
		//Y+
		vertices[4] = baseVertices[4];
		vertices[5] = baseVertices[7];
		vertices[6] = baseVertices[6];
		vertices[7] = baseVertices[5];
		
		//X-
		vertices[8] = baseVertices[0];
		vertices[9] = baseVertices[3];
		vertices[10] = baseVertices[7];
		vertices[11] = baseVertices[4];
		
		//X+
		vertices[12] = baseVertices[2];
		vertices[13] = baseVertices[1];
		vertices[14] = baseVertices[5];
		vertices[15] = baseVertices[6];	
	
		//Z-
		vertices[16] = baseVertices[1];
		vertices[17] = baseVertices[0];
		vertices[18] = baseVertices[4];
		vertices[19] = baseVertices[5];		
		
		//Z+
		vertices[20] = baseVertices[3];
		vertices[21] = baseVertices[2];
		vertices[22] = baseVertices[6];
		vertices[23] = baseVertices[7];
		
		
				
		Vector3[] baseNormals = new Vector3[6];
		
		baseNormals[0] = new Vector3(0.0f, -1.0f, 0.0f);		
		baseNormals[1] = new Vector3(0.0f, 1.0f, 0.0f);
		baseNormals[2] = new Vector3(-1.0f, 0.0f, 0.0f);
		baseNormals[3] = new Vector3(1.0f, 0.0f, 0.0f);
		baseNormals[4] = new Vector3(0.0f, 0.0f, -1.0f);		
		baseNormals[5] = Vector3.Cross(baseVertices[4] - baseVertices[5], baseVertices[6] - baseVertices[5]);
		baseNormals[5].Normalize();
		
		Vector3[] normals = new Vector3[iTotalVertexCount];
		
		for (int iVertexIdx = 0; iVertexIdx < iTotalVertexCount; iVertexIdx++)
		{
			int iBaseNormalIndex = iVertexIdx / 4;
			normals[iVertexIdx] = baseNormals[iBaseNormalIndex];
			        
		}
		
		Vector2 uvStart = _uvOffset;
		Vector2 uvEnd = uvStart + _uvTiling;		
		
		Vector2[] baseUVs = new Vector2[4];
		baseUVs[0] = new Vector2(uvStart.x, uvStart.y);
		baseUVs[1] = new Vector2(uvEnd.x, uvStart.y);		
		baseUVs[2] = new Vector2(uvEnd.x, uvEnd.y);		
		baseUVs[3] = new Vector2(uvStart.x, uvEnd.y);

		Vector2[] UVs = new Vector2[iTotalVertexCount];		
		
		for (int iVertexIdx = 0; iVertexIdx < iTotalVertexCount; iVertexIdx++)
		{
			int iBaseUVIndex = iVertexIdx & 0x03;
			UVs[iVertexIdx] = baseUVs[iBaseUVIndex];			        
		}	
		
	
		int[] triangles = new int[12 * 3];
		
		//Left-handed!
		int iCurrentVertexIndex = 0;
		for (int iFaceIdx = 0; iFaceIdx < 6; iFaceIdx++)
		{	
			int iCurrentFaceIndexStart = iFaceIdx * 3 * 2;
			
			triangles[iCurrentFaceIndexStart]     = iCurrentVertexIndex;
			triangles[iCurrentFaceIndexStart + 1] = iCurrentVertexIndex + 1; 
			triangles[iCurrentFaceIndexStart + 2] = iCurrentVertexIndex + 2; 
					
			triangles[iCurrentFaceIndexStart + 3] = iCurrentVertexIndex;
			triangles[iCurrentFaceIndexStart + 4] = iCurrentVertexIndex + 2;
			triangles[iCurrentFaceIndexStart + 5] = iCurrentVertexIndex + 3;
			
			iCurrentVertexIndex += 4;
		}
		
		_mesh.vertices = vertices;
		_mesh.normals = normals;
		_mesh.uv = UVs;
		_mesh.triangles = triangles;	
		
		_mesh.RecalculateBounds();	
		_mesh.name = "Box";
	}
	
	public static void CreatePlaneMesh(out Mesh _mesh, Vector2 _planeDims, Vector2 _uvTiling, Vector2 _uvOffset)
	{
		_mesh = new Mesh();
		_mesh.name = "BoxMesh";
		
		_mesh.vertices = new Vector3[4];
		
		Vector2 planeHalfDims = _planeDims;
		planeHalfDims *= 0.5f;
						
		//  3------2
		// /      /
		// 0------1
		
		Vector3[] vertices = new Vector3[4];
		
		vertices[0] = new Vector3(-planeHalfDims.x, 0.0f, -planeHalfDims.y);
		vertices[1] = new Vector3( planeHalfDims.x, 0.0f, -planeHalfDims.y);
		vertices[2] = new Vector3( planeHalfDims.x, 0.0f,  planeHalfDims.y);
		vertices[3] = new Vector3(-planeHalfDims.x, 0.0f,  planeHalfDims.y);
		
		Vector2 uvStart = _uvOffset;
		Vector2 uvEnd = uvStart + _uvTiling;
		
		Vector2[] UVs = new Vector2[4];
		UVs[0] = new Vector2(uvStart.x, uvStart.y);
		UVs[1] = new Vector2(uvEnd.x, uvStart.y);		
		UVs[2] = new Vector2(uvEnd.x, uvEnd.y);		
		UVs[3] = new Vector2(uvStart.x, uvEnd.y);
		
		Vector3[] normals = new Vector3[4];		
		normals[0] = normals[1] = normals[2] = normals[3] = new Vector3(0.0f, 1.0f, 0.0f);			
		
		int[] triangles = new int[6];
		
		triangles[0] = 0;
		triangles[1] = 2; 
		triangles[2] = 1; 
					
		triangles[3] = 0;
		triangles[4] = 3;
		triangles[5] = 2;
		
		_mesh.vertices = vertices;
		_mesh.normals = normals;
		_mesh.uv = UVs;
		_mesh.triangles = triangles;	
		
		_mesh.RecalculateBounds();	
		_mesh.name = "Plane";	
	}	

	public static void CreateCylinderMesh(out Mesh _mesh, 
	                                      int _iSubdivisionCount, 
	                                      float _fTopRadius, 
	                                      float _fBottomRadius, 
	                                      float _fHeight, 
	                                      bool _bSmoothNormal, 
	                                      Vector2 _uvTiling,
	                                      Vector2 _uvOffset)
	{			
		_iSubdivisionCount = Mathf.Clamp(_iSubdivisionCount, 3, 128);
		
		//If the top radius is too small, dont create a top cap
		bool bCreateTopCap = _fTopRadius > 0.001f;
		
		int iTotalVertexCount = 0;
		int iSideVertexCount;
			
		//We need to duplicate the last vertex for the UV to interpolate correctly
		if (_bSmoothNormal)
			iSideVertexCount = _iSubdivisionCount + 1;
		else
			iSideVertexCount = _iSubdivisionCount * 2;
		
		iTotalVertexCount += (iSideVertexCount * 2);
				
		
		//Determine vertex count and create arrays
		int iCapVertexCount = _iSubdivisionCount;
		
		if (bCreateTopCap)
			iTotalVertexCount += (iCapVertexCount * 2);
		else
			iTotalVertexCount += iCapVertexCount;		
		
		Vector3[] vertices = new Vector3[iTotalVertexCount];
		Vector3[] normals = new Vector3[iTotalVertexCount];
		Vector2[] UVs = new Vector2[iTotalVertexCount];
		
		
		//Init circle vertex
		Vector3[] circleVertices = new Vector3[_iSubdivisionCount];		
		float fCircleSliceSize = 1.0f / (float)_iSubdivisionCount;
		
		for (int iSubDivisionIdx = 0; iSubDivisionIdx < _iSubdivisionCount; iSubDivisionIdx++)
		{
			float fAngle = ((float)iSubDivisionIdx / (float)_iSubdivisionCount) * (Mathf.PI * 2.0f);
			circleVertices[iSubDivisionIdx] = new Vector3(Mathf.Sin(fAngle), 0.0f, Mathf.Cos(fAngle));
		}		
		
		
		//Set side vertices (bottom and top)
		for (int iSubDivisionIdx = 0; iSubDivisionIdx < _iSubdivisionCount; iSubDivisionIdx++)
		{			
			if (_bSmoothNormal)
			{
				int iSideBottomIndex = iSubDivisionIdx;
				int iSideTopIndex = iSideBottomIndex + iSideVertexCount;	
								
				Vector2 uvBottom = new Vector2((iSubDivisionIdx * fCircleSliceSize) * _uvTiling.x + _uvOffset.x, _uvOffset.y);
				Vector2 uvTop = new Vector2(uvBottom.x, _uvTiling.y + _uvOffset.y);
				
				vertices[iSideBottomIndex] = circleVertices[iSubDivisionIdx] * _fBottomRadius;			
				UVs[iSideBottomIndex] = uvBottom;
				
				//Top
				Vector3 topVertex = circleVertices[iSubDivisionIdx];
				topVertex *= _fTopRadius;
				topVertex.y = _fHeight;			
				
				vertices[iSideTopIndex] = topVertex;				
				UVs[iSideTopIndex] = uvTop;
			}
			else
			{
				int iSideBottomIndex = iSubDivisionIdx * 2;
				int iSideTopIndex = iSideBottomIndex + iSideVertexCount;	
								
				Vector2 uvStart = new Vector2((iSubDivisionIdx * fCircleSliceSize) * _uvTiling.x + _uvOffset.x, _uvOffset.y);				
				Vector2 uvEnd = new Vector2(((iSubDivisionIdx + 1) * fCircleSliceSize) * _uvTiling.x + _uvOffset.x, _uvTiling.y + _uvOffset.y);				
								
				//Bottom 
				vertices[iSideBottomIndex] = circleVertices[iSubDivisionIdx] * _fBottomRadius;			
				vertices[iSideBottomIndex + 1] = circleVertices[(iSubDivisionIdx + 1) % _iSubdivisionCount] * _fBottomRadius;			
				UVs[iSideBottomIndex] = new Vector2(uvStart.x, uvStart.y);
				UVs[iSideBottomIndex + 1] = new Vector2(uvEnd.x, uvStart.y);

				//Top
				Vector3 topVertex0 = circleVertices[iSubDivisionIdx];
				topVertex0 *= _fTopRadius;
				topVertex0.y = _fHeight;
				vertices[iSideTopIndex] = topVertex0;
				
				Vector3 topVertex1 = circleVertices[(iSubDivisionIdx + 1) % _iSubdivisionCount];
				topVertex1 *= _fTopRadius;
				topVertex1.y = _fHeight;			
				
				vertices[iSideTopIndex + 1] = topVertex1;
				UVs[iSideTopIndex] = new Vector2(uvStart.x, uvEnd.y);
				UVs[iSideTopIndex + 1] = new Vector2(uvEnd.x, uvEnd.y);
			}
		}
		
		
		//Bottom and top cap
		int iBottomCapVertexStart = iSideVertexCount * 2;
		int iTopCapVertexStart = iBottomCapVertexStart + _iSubdivisionCount;
		
		for (int iSubDivisionIdx = 0; iSubDivisionIdx < _iSubdivisionCount; iSubDivisionIdx++)
		{	
			int iBottomIndex = iBottomCapVertexStart + iSubDivisionIdx;
			
			vertices[iBottomIndex] = circleVertices[iSubDivisionIdx] * _fBottomRadius;			
			normals[iBottomIndex] = -Vector3.up;
			UVs[iBottomIndex] = new Vector2(((circleVertices[iSubDivisionIdx].x + 1.0f) * 0.5f) * _uvTiling.x + _uvOffset.x, 
			                                ((circleVertices[iSubDivisionIdx].z + 1.0f) * 0.5f) * _uvTiling.y + _uvOffset.y);
						
			if (bCreateTopCap)
			{
				int iTopIndex = iTopCapVertexStart + iSubDivisionIdx;
				
				Vector3 topVertex0 = circleVertices[iSubDivisionIdx];
				topVertex0 *= _fTopRadius;
				topVertex0.y = _fHeight;
				vertices[iTopIndex] = topVertex0;				
				normals[iTopIndex] = Vector3.up;
				UVs[iTopIndex] = UVs[iBottomIndex];
			}				
		}		
		
		for (int iSubDivisionIdx = 0; iSubDivisionIdx < _iSubdivisionCount; iSubDivisionIdx++)
		{			
			int iSideBottomIndex;
			if (_bSmoothNormal)
				iSideBottomIndex = iSubDivisionIdx;
			else
				iSideBottomIndex = iSubDivisionIdx * 2;						
				
			if (_bSmoothNormal)
			{
				//Ignore the duplicate vertex at the end
				int iNextBottomIndex;
				if (iSideBottomIndex == (_iSubdivisionCount - 1))
					iNextBottomIndex = 0;
				else
					iNextBottomIndex = iSideBottomIndex + 1;
				
				Vector3 vCurrent0 = vertices[iNextBottomIndex] - vertices[iSideBottomIndex];
				Vector3 vCurrent1 = vertices[iSideBottomIndex + iSideVertexCount] - vertices[iSideBottomIndex];
				Vector3 normalCurrentFace = Vector3.Cross(vCurrent0, vCurrent1);
				
				//Ignore the duplicate vertex at the end
				int iPreviousBottomIndex;
				if (iSideBottomIndex == 0)
					iPreviousBottomIndex = _iSubdivisionCount - 1;
				else
					iPreviousBottomIndex = (iSideBottomIndex - 1);

				Vector3 vPrevious0 = vertices[iSideBottomIndex] - vertices[iPreviousBottomIndex];
				Vector3 vPrevious1 = vertices[iPreviousBottomIndex + iSideVertexCount] - vertices[iPreviousBottomIndex];
			
				Vector3 normalPreviousFace = Vector3.Cross(vPrevious0, vPrevious1);			
				
				Vector3 averageNormal = (normalCurrentFace + normalPreviousFace) * 0.5f;
				averageNormal.Normalize();
				
				normals[iSideBottomIndex] = averageNormal;
				normals[iSideBottomIndex + iSideVertexCount] = averageNormal;				
			}
			else
			{
				//Just take the normal of the current face				
				Vector3 vCurrent0 = vertices[(iSideBottomIndex + 1) % iSideVertexCount] - vertices[iSideBottomIndex];
				Vector3 vCurrent1 = vertices[iSideBottomIndex + iSideVertexCount] - vertices[iSideBottomIndex];
				Vector3 normalCurrentFace = Vector3.Cross(vCurrent0, vCurrent1);
				
				normalCurrentFace.Normalize();			
				normals[iSideBottomIndex] = normalCurrentFace;
				normals[iSideBottomIndex + 1] = normalCurrentFace;
				
				normals[iSideBottomIndex + iSideVertexCount] = normalCurrentFace;
				normals[iSideBottomIndex + iSideVertexCount + 1] = normalCurrentFace;
			}
		}		
		
		//Duplicate the first vertex for the UV to interpolate correctly
		if (_bSmoothNormal)
		{
			int iBottomLastIndex = iSideVertexCount - 1;
			int iTopLastIndex = (iSideVertexCount * 2) - 1;
			
			vertices[iBottomLastIndex] = vertices[0];			
			normals[iBottomLastIndex] = normals[0];
			UVs[iBottomLastIndex] = new Vector2(_uvOffset.x + _uvTiling.x, _uvOffset.y);
			
			vertices[iTopLastIndex] = vertices[iSideVertexCount];
			normals[iTopLastIndex] = normals[iSideVertexCount];
			UVs[iTopLastIndex] = new Vector2(_uvOffset.x + _uvTiling.x, _uvOffset.y + _uvTiling.y);
		}
	
	
		int iSideTriangleIndexCount = (iSideVertexCount * 2) * 3;
		int iCapTriangleIndexCount = (_iSubdivisionCount - 2) * 3;
		
		int[] triangles = new int[iSideTriangleIndexCount + (iCapTriangleIndexCount * 2)];		
		for (int iSubDivisionIdx = 0; iSubDivisionIdx < _iSubdivisionCount; iSubDivisionIdx++)
		{
			int iCurrentFaceIndex = iSubDivisionIdx * 2 * 3;
			
			int iSideVertexStart;
				
			if (_bSmoothNormal)
				iSideVertexStart = iSubDivisionIdx;		
			else
				iSideVertexStart = iSubDivisionIdx * 2;		
			
			triangles[iCurrentFaceIndex]     = iSideVertexStart;
			triangles[iCurrentFaceIndex + 1] = iSideVertexStart + 1;
			triangles[iCurrentFaceIndex + 2] = iSideVertexStart + iSideVertexCount;
					
			triangles[iCurrentFaceIndex + 3] = triangles[iCurrentFaceIndex + 1];
			triangles[iCurrentFaceIndex + 4] = triangles[iCurrentFaceIndex + 1] + iSideVertexCount;
			triangles[iCurrentFaceIndex + 5] = triangles[iCurrentFaceIndex + 2];
		}
		
		int iBottomCapStartTrianglesIndex = iSideTriangleIndexCount;
		int iBottomCapStartVerticesIndex = iSideVertexCount * 2;
		
		int iTopCapStartTrianglesIndex = iSideTriangleIndexCount + iCapTriangleIndexCount;
		int iTopCapStartVerticesIndex = iBottomCapStartVerticesIndex + _iSubdivisionCount ;
			
		for (int iSubDivisionIdx = 0; iSubDivisionIdx < _iSubdivisionCount - 2; iSubDivisionIdx++)
		{
			int iCurrentFaceIndex = iSubDivisionIdx * 3;
			triangles[iBottomCapStartTrianglesIndex + iCurrentFaceIndex]     = iBottomCapStartVerticesIndex;
			triangles[iBottomCapStartTrianglesIndex + iCurrentFaceIndex + 1] = iBottomCapStartVerticesIndex + iSubDivisionIdx + 2;
			triangles[iBottomCapStartTrianglesIndex + iCurrentFaceIndex + 2] = iBottomCapStartVerticesIndex + iSubDivisionIdx + 1;			
			
			if (bCreateTopCap)
			{
				triangles[iTopCapStartTrianglesIndex + iCurrentFaceIndex]     = iTopCapStartVerticesIndex;
				triangles[iTopCapStartTrianglesIndex + iCurrentFaceIndex + 1] = iTopCapStartVerticesIndex + iSubDivisionIdx + 1;
				triangles[iTopCapStartTrianglesIndex + iCurrentFaceIndex + 2] = iTopCapStartVerticesIndex + iSubDivisionIdx + 2;			
			}
		}
		 
		_mesh = new Mesh();
		_mesh.vertices = vertices;
		_mesh.normals = normals;
		_mesh.uv = UVs;
		_mesh.triangles = triangles;
		
		_mesh.RecalculateBounds();
	}
	
	//Yeah it's slow, can be optimize
	public static int AddVertex(ref List<Vector3> _vertexArray, ref Vector3 _vertex)
	{
		for (int iVertexIndex = 0; iVertexIndex < _vertexArray.Count; iVertexIndex++)
		{
			//Found it?  Return the index then
			if (_vertexArray[iVertexIndex].IsSameWith(ref _vertex, 0.001f))
				return iVertexIndex;	
		}
		
		//Not found?  Add the vertex to the array
		_vertexArray.Add(_vertex);
		return _vertexArray.Count - 1;
	}
	
	public static void CreateSphere(out Mesh _mesh, int _iSubdivision, float _fRadius/*, Vector2 _uvTiling, Vector2 _uvOffset*/)
	{	
		float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;		
		
		//Create base icosahedron then normalize the vertex on the unit sphere
		List<Vector3> vertexArray = new List<Vector3>();
		List<MathUtils.Triangle> triangleArray = new List<MathUtils.Triangle>();
		
		vertexArray.Add(new Vector3(-1.0f,  t,  0.0f));
		vertexArray.Add(new Vector3( 1.0f,  t,  0.0f));
		vertexArray.Add(new Vector3(-1.0f, -t,  0.0f));
		vertexArray.Add(new Vector3( 1.0f, -t,  0.0f));
		
		vertexArray.Add(new Vector3( 0.0f, -1.0f,  t));
		vertexArray.Add(new Vector3( 0.0f,  1.0f,  t));
		vertexArray.Add(new Vector3( 0.0f, -1.0f, -t));
		vertexArray.Add(new Vector3( 0.0f,  1.0f, -t));
		 
		vertexArray.Add(new Vector3( t,  0.0f, -1.0f));
		vertexArray.Add(new Vector3( t,  0.0f,  1.0f));
		vertexArray.Add(new Vector3(-t,  0.0f, -1.0f));
		vertexArray.Add(new Vector3(-t,  0.0f,  1.0f));		
		
        
        triangleArray.Add(new MathUtils.Triangle(0, 11, 5));
        triangleArray.Add(new MathUtils.Triangle(0, 5, 1));
        triangleArray.Add(new MathUtils.Triangle(0, 1, 7));
        triangleArray.Add(new MathUtils.Triangle(0, 7, 10));
        triangleArray.Add(new MathUtils.Triangle(0, 10, 11));

        triangleArray.Add(new MathUtils.Triangle(1, 5, 9));
        triangleArray.Add(new MathUtils.Triangle(5, 11, 4));
        triangleArray.Add(new MathUtils.Triangle(11, 10, 2));
        triangleArray.Add(new MathUtils.Triangle(10, 7, 6));
        triangleArray.Add(new MathUtils.Triangle(7, 1, 8));
        
        triangleArray.Add(new MathUtils.Triangle(3, 9, 4));
        triangleArray.Add(new MathUtils.Triangle(3, 4, 2));
        triangleArray.Add(new MathUtils.Triangle(3, 2, 6));
        triangleArray.Add(new MathUtils.Triangle(3, 6, 8));
        triangleArray.Add(new MathUtils.Triangle(3, 8, 9));
         
        triangleArray.Add(new MathUtils.Triangle(4, 9, 5));
        triangleArray.Add(new MathUtils.Triangle(2, 4, 11));
        triangleArray.Add(new MathUtils.Triangle(6, 2, 10));
        triangleArray.Add(new MathUtils.Triangle(8, 6, 7));
        triangleArray.Add(new MathUtils.Triangle(9, 8, 1));
		
		
		for (int iRecursionLevel = 0; iRecursionLevel < _iSubdivision; iRecursionLevel++)
		{
			List<MathUtils.Triangle> newTriangleArray = new List<MathUtils.Triangle>();
						
			foreach (MathUtils.Triangle tri in triangleArray)
			{
				Vector3 middlePoint1 = (vertexArray[tri.iVertexIndex1] + vertexArray[tri.iVertexIndex2]) * 0.5f;
				Vector3 middlePoint2 = (vertexArray[tri.iVertexIndex2] + vertexArray[tri.iVertexIndex3]) * 0.5f;
				Vector3 middlePoint3 = (vertexArray[tri.iVertexIndex3] + vertexArray[tri.iVertexIndex1]) * 0.5f;				
				
				int a = AddVertex(ref vertexArray, ref middlePoint1);
				int b = AddVertex(ref vertexArray, ref middlePoint2);
				int c = AddVertex(ref vertexArray, ref middlePoint3);				                                            			                                            
				                                            
                newTriangleArray.Add(new MathUtils.Triangle(tri.iVertexIndex1, a, c));
                newTriangleArray.Add(new MathUtils.Triangle(tri.iVertexIndex2, b, a));
                newTriangleArray.Add(new MathUtils.Triangle(tri.iVertexIndex3, c, b));
                newTriangleArray.Add(new MathUtils.Triangle(a, b, c));
            }

			triangleArray = newTriangleArray;
		}		
		
		//Convert to unity mesh triangle
		int[] meshTriangles = new int[triangleArray.Count * 3];
		
		int iCurrentTriangleIndex = 0;
		foreach (MathUtils.Triangle tri in triangleArray)
		{
			meshTriangles[iCurrentTriangleIndex++] = tri.iVertexIndex1;
			meshTriangles[iCurrentTriangleIndex++] = tri.iVertexIndex2;
			meshTriangles[iCurrentTriangleIndex++] = tri.iVertexIndex3;
		}
		
		
		Vector3[] normals = new Vector3[vertexArray.Count];
		Vector3[] meshVertices = new Vector3[vertexArray.Count];
		Vector2[] UVs = new Vector2[vertexArray.Count];

		for (int iVertexIndex = 0; iVertexIndex < vertexArray.Count; iVertexIndex++)		
		{			
			Vector3 vertex = vertexArray[iVertexIndex];
			vertex.Normalize();
				
			UVs[iVertexIndex] = new Vector2((Mathf.Asin(vertex.x) / Mathf.PI + 0.5f), (Mathf.Asin(vertex.y) / Mathf.PI + 0.5f));
			
			normals[iVertexIndex] = vertex;
			
			vertex *= _fRadius;
			meshVertices[iVertexIndex] = vertex;			
		}
		
		_mesh = new Mesh();		
		_mesh.vertices = meshVertices;
		_mesh.normals = normals;	
		_mesh.uv = UVs;
		_mesh.triangles = meshTriangles;
	}	
	
	
	
}

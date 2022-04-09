using UnityEngine;
using System.Collections;

static class MathUtils
{
	public struct Triangle
	{
        public int iVertexIndex1;
        public int iVertexIndex2;
        public int iVertexIndex3;

        public Triangle(int _iVertexIndex1, int _iVertexIndex2, int _iVertexIndex3)
        {
            iVertexIndex1 = _iVertexIndex1;
            iVertexIndex2 = _iVertexIndex2;
            iVertexIndex3 = _iVertexIndex3;
        }
	}
	
	
	
	public static bool IsSameWith(this Vector3 _this, ref Vector3 _other, float _fEpsilon)
    {
		if (Mathf.Abs(_this.x - _other.x) > _fEpsilon)			
			return false;
		
		if (Mathf.Abs(_this.y - _other.y) > _fEpsilon)
			return false;
		
		if (Mathf.Abs(_this.z - _other.z) > _fEpsilon)
			return false;			     	
		     	
		return true;	     
    }
}
	
	
	
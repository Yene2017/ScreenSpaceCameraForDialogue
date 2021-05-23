using UnityEngine;

namespace vcf
{
	namespace tools
	{
	    public static class Vector3Extensions
	    {
	        public static Vector2 projectX(this Vector3 v) { return new Vector2(v.z, v.y); }
	        public static Vector2 projectY(this Vector3 v) { return new Vector2(v.x, v.z); }
	        public static Vector2 projectZ(this Vector3 v) { return new Vector2(v.y, v.x); }

		    public static void UnprojectX(ref Vector3 v, Vector2 w)
		    {
			    v.z = w.x;
			    v.y = w.y;
		    }

	        public static void UnprojectY(ref Vector3 v, Vector2 w)
		    {
			    v.x = w.x;
			    v.z = w.y;
		    }

	        public static void UnprojectZ(ref Vector3 v, Vector2 w)
		    {
			    v.y = w.x;
			    v.x = w.y;
		    }

	        public static void Negate(ref Vector3 v)
	        {
	            for (int i = 0; i < 3; i++) v[i] = -v[i];
	        }

	        //public static Vector3 operator ^(Vector3 u, Vector3 v)
	        //{
	        //    return Vector3.Cross(u, v);
	        //}

		    public static Vector3 perpendicular(this Vector3 v)
		    {
			    Vector3 res;
			    if( v.z == 0 ) // do the same as in 2d
			    {
				    res = new Vector3(-v.y,v.x,v.z);
			    }
			    else
			    {
				    // satisfy the formula xx' + yy' + zz' = 0
				    // by fixing 2 coords, then computing the 3rd
				    // by using the dot product of the 2d sub-vectors
				    Vector2 v2 = new Vector2(v.y,v.z);
	                Vector2 proj = v.projectZ();
	                float p = Vector3.Dot(proj, v);
				    res = new Vector3(v2.x,v2.y,-p/v.z);
			    }
			    res.Normalize();
			    return res;
		    }

	        public static radian angle(this Vector3 u, Vector3 v)
	        {
	            return new radian( Vector3.Angle(u, v) * Mathf.Deg2Rad );
	        }

	        public static radian directedAngle(this Vector3 u, Vector3 v, Vector3 normal)
		    {
			    Matrix3 m = new Matrix3();
			    m.swapRow(0, u);
			    m.swapRow(1, v);
			    m.swapRow(2, normal);
			    double det = -m.getDeterminant(); // inversed wrt a right handed system

			    radian a = u.angle(v);

			    if( det > 0 )
				    return a;
			    else
				    return -a;
		    }
	    }
	}
}
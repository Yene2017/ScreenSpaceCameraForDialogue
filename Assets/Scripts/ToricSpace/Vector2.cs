using UnityEngine;

namespace vcf
{
	namespace tools
	{
	    public static class Vector2Extensions
	    {
	        public static Vector2 rotated90(this Vector2 v) { return new Vector2(-v.y, v.x); }

	        public static Vector2 rotated180(this Vector2 v) { return new Vector2(-v.x, -v.y); }

	        public static Vector2 rotated270(this Vector2 v) { return new Vector2(v.y, -v.x); }

	        public static Vector2 rotated(this Vector2 v, radian theta)
			{
				Vector2 res = new Vector2(v.x,v.y);
				res.x = (float) ( Math.Cos(theta) * v.x - Math.Sin(theta) * v.y );
				res.y = (float) ( Math.Sin(theta) * v.x + Math.Cos(theta) * v.y );
				return res;
			}

	        public static void Rotate90(ref Vector2 v)
			{
				float tmp = v.x;
				v.x = -v.y; 
				v.y = tmp; 
			}

	        public static void Rotate180(ref Vector2 v) { Negate(ref v); }

	        public static void Rotate270(ref Vector2 v) { Rotate90(ref v); Rotate180(ref v); }
	        
	        public static void Rotate(ref Vector2 v, radian theta)
			{
				float
	                x = (float) ( Math.Cos(theta) * v.x - Math.Sin(theta) * v.y ),
	                y = (float) ( Math.Sin(theta) * v.x + Math.Cos(theta) * v.y );
				v.x = x;
				v.y = y;
			}

	        public static void Negate(ref Vector2 v)
	        {
	            v.x = -v.x;
	            v.y = -v.y;
	        }
	    }
	}
}
using UnityEngine;
namespace vcf
{
	namespace tools
	{
	    public static class Math
	    {
	        public static float PI = Mathf.PI;
	        public static float PI_2 = 2 * PI;

	        public static void Assert(bool b)
	        {
	            if (!b)
	            {
	                throw new System.Exception();
	            }
	        }

	        public static T Clamp<T>(T val, T min, T max) where T : System.IComparable<T>
	        {
	            if (val.CompareTo(min) < 0) return min;
	            if (val.CompareTo(max) > 0) return max;
	            return val;
	        }

	        public static bool Within<T>(T val, T min, T max) where T : System.IComparable<T>
	        {
	            return val.CompareTo(min) > 0 && val.CompareTo(max) < 0;
	        }

	        public static float Cos (radian r) { return Mathf.Cos (r.valueRadians());    }
	        public static float Acos(radian r) { return Mathf.Acos(r.valueRadians());    }
	        public static float Sin (radian r) { return Mathf.Sin (r.valueRadians());    }
	        public static float Asin(radian r) { return Mathf.Asin(r.valueRadians());    }
	        public static float Tan (radian r) { return Mathf.Tan (r.valueRadians());    }
	        public static float Atan(radian r) { return Mathf.Atan(r.valueRadians());    }

	        public static float Square(float x) { return x * x; }

            public static T Min<T>(T t1, T t2) where T : System.IComparable<T>
            {
                if (t1.CompareTo(t2) < 0) return t1;
                else return t2;
            }

            public static T Max<T>(T t1, T t2) where T : System.IComparable<T>
            {
                if (t1.CompareTo(t2) > 0) return t1;
                else return t2;
            }
	    }
	}
}
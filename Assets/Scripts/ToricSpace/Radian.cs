namespace vcf
{
	namespace tools
	{
#pragma warning disable CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
#pragma warning disable CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
        public class radian : System.IComparable<radian>
#pragma warning restore CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
#pragma warning restore CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
        {
			private float m_value;
				
			public radian(float val)
			{
		        m_value = val;
		        modulo(ref m_value);
			}

		    public radian(radian r)
		    {
		        m_value = r.m_value;
		    }

		    public static implicit operator radian(radian2 r) // implicit conversion
		    {
		        radian res = new radian(r.valueRadians());
		        return res;
		    }
				
			public static implicit operator radian(degree d) // implicit conversion
		    {
		        radian res = new radian( d.valueRadians() );  
		        return res;
		    }

		    public static explicit operator radian(float d) // implicit conversion
		    {
		        radian res = new radian(d);  
		        return res;
		    }
				
			// return the angle in the range [-Pi,+Pi]
		    private static void modulo(ref float d)
			{
		        if( System.Math.Abs(d) - Math.PI < 1e-5 ) return;
		    		
				int n1 = 0, n2 = 0;
		        if (d < -Math.PI)
				{
		            n1 = (int)-System.Math.Floor((d + Math.PI) / Math.PI_2);
		            d = d + (n1 * Math.PI_2);
				}

		        if (d > Math.PI)
				{
		            n2 = (int)System.Math.Ceiling((d - Math.PI) / Math.PI_2);
		            d = d - (n2 * Math.PI_2);
				}
			}

		    public float valueRadians() { return m_value; }
			public float valueDegrees() { return m_value * UnityEngine.Mathf.Rad2Deg; }

		    public static radian operator -(radian r)
		    {
		        radian res = new radian(-r.m_value);
		        return res;
		    }

		    public static radian operator +(radian r1, radian r2)
		    {
		        radian res = new radian(r1.m_value + r2.m_value);
		        return res;
		    }

		    public static radian operator -(radian r1, radian r2)
		    {
		        radian res = new radian(r1.m_value - r2.m_value);
		        return res;
		    }

		    public static radian operator *(radian r, float s)
		    {
		        radian res = new radian(r.m_value * s);
		        return res;
		    }

		    public static radian operator /(radian r, float s)
		    {
		        radian res = new radian(r.m_value / s);
		        return res;
		    }

			public static bool operator< (radian r1, radian r2) { return r1.m_value < r2.m_value; }
			public static bool operator> (radian r1, radian r2) { return r1.m_value > r2.m_value; }
			public static bool operator<=(radian r1, radian r2) { return r1.m_value <= r2.m_value; }
			public static bool operator>=(radian r1, radian r2) { return r1.m_value >= r2.m_value; }
			public static bool operator==(radian r1, radian r2) { return r1.m_value == r2.m_value; }
			public static bool operator!=(radian r1, radian r2) { return r1.m_value != r2.m_value; }

		    public int CompareTo(radian other)
		    {
		        if (this < other) return -1;
		        if (this > other) return +1;
		        return 0;
		    }
		}

#pragma warning disable CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
#pragma warning disable CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
        public class radian2 : System.IComparable<radian2>
#pragma warning restore CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
#pragma warning restore CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
        {
		    private float m_value;

		    public radian2(float val)
		    {
		        m_value = val;
		        modulo(ref m_value);
		    }

		    public radian2(radian2 r)
		    {
		        m_value = r.m_value;
		    }

		    public static explicit operator radian2(degree d) // explicit conversion
		    {
		        radian2 res = new radian2(d.valueRadians());  
		        return res;
		    }

		    public static explicit operator radian2(radian r) // explicit conversion
		    {
		        radian2 res = new radian2(r.valueRadians());  
		        return res;
		    }

		    public static explicit operator radian2(float d) // implicit conversion
		    {
		        radian2 res = new radian2(d);  
		        return res;
		    }

		    // return the angle in the range [0,+2Pi]
		    private static void modulo(ref float d)
		    {
		        if (d > -1e-5 && d < Math.PI_2 + 1e-5) return;

				int n1 = 0, n2 = 0;
				if( d < 0 )
				{
		            n1 = (int)System.Math.Ceiling(-d / Math.PI_2);
		            d = d + (n1 * Math.PI_2);
				}

		        if (d > Math.PI_2)
				{
		            n2 = (int)System.Math.Floor(d / Math.PI_2);
		            d = d - (n2 * Math.PI_2);
				}
		    }

		    public float valueRadians() { return m_value; }
		    public float valueDegrees() { return m_value * UnityEngine.Mathf.Rad2Deg; }

		    public static radian2 operator -(radian2 r)
		    {
		        radian2 res = new radian2(-r.m_value);
		        return res;
		    }

		    public static radian2 operator +(radian2 r1, radian2 r2)
		    {
		        radian2 res = new radian2(r1.m_value + r2.m_value);
		        return res;
		    }

		    public static radian2 operator -(radian2 r1, radian2 r2)
		    {
		        radian2 res = new radian2(r1.m_value - r2.m_value);
		        return res;
		    }

		    public static radian2 operator *(radian2 r, float s)
		    {
		        radian2 res = new radian2(r.m_value * s);
		        return res;
		    }

		    public static radian2 operator /(radian2 r, float s)
		    {
		        radian2 res = new radian2(r.m_value / s);
		        return res;
		    }

			public static bool operator< (radian2 r1, radian2 r2) { return r1.m_value < r2.m_value; }
			public static bool operator> (radian2 r1, radian2 r2) { return r1.m_value > r2.m_value; }
			public static bool operator<=(radian2 r1, radian2 r2) { return r1.m_value <= r2.m_value; }
			public static bool operator>=(radian2 r1, radian2 r2) { return r1.m_value >= r2.m_value; }
			public static bool operator==(radian2 r1, radian2 r2) { return r1.m_value == r2.m_value; }
			public static bool operator!=(radian2 r1, radian2 r2) { return r1.m_value != r2.m_value; }

		    public int CompareTo(radian2 other)
		    {
		        if (this < other) return -1;
		        if (this > other) return +1;
		        return 0;
		    }
		}
	}
}
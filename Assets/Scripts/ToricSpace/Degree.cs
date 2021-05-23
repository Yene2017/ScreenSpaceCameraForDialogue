namespace vcf
{
	namespace tools
	{
		public struct degree : System.IComparable<degree>
		{
			private float m_value;

			public degree(float val)
			{
		        m_value = val;
		        modulo(ref m_value);
			}
				
			public degree(degree d)
			{
				m_value = d.m_value;
			}

		    public static implicit operator degree(radian r) // implicit conversion
		    {
		        degree res = new degree(r.valueDegrees());  
		        return res;
		    }

		    public static explicit operator degree(float d) // implicit conversion
		    {
		        degree res = new degree(d);  
		        return res;
		    }

		    // return the angle in the range [-180,+180]
			private static void modulo(ref float d)
			{
		        if (System.Math.Abs(d) - 180 < 1e-5) return;

		        int n1 = 0, n2 = 0;
		        if (d < -180)
		        {
		            n1 = (int)-System.Math.Floor((d + 180) / 360);
		            d = d + (n1 * 360);
		        }

		        if (d > 180)
		        {
		            n2 = (int)System.Math.Ceiling((d - 180) / 360);
		            d = d - (n2 * 360);
		        }
			}

		    public float valueRadians() { return m_value * UnityEngine.Mathf.Deg2Rad; }
			public float valueDegrees() { return m_value; }

		    public static degree operator -(degree d) 
		    {
		        degree res = new degree(-d.m_value);
		        return res; 
		    }

		    public static degree operator +(degree d1, degree d2)
		    { 
		        degree res = new degree(d1.m_value + d2.m_value);
		        return res;
		    }

		    public static degree operator -(degree d1, degree d2)
		    {
		        degree res = new degree(d1.m_value - d2.m_value);
		        return res;
		    }

		    public static degree operator *(degree d, float s)
		    {
		        degree res = new degree(d.m_value * s);
		        return res;
		    }

		    public static degree operator /(degree d, float s)
		    {
		        degree res = new degree(d.m_value / s);
		        return res;
		    }

		    public static bool operator <(degree d1, degree d2) { return d1.m_value < d2.m_value; }
		    public static bool operator >(degree d1, degree d2) { return d1.m_value > d2.m_value; }
		    public static bool operator <=(degree d1, degree d2) { return d1.m_value <= d2.m_value; }
		    public static bool operator >=(degree d1, degree d2) { return d1.m_value >= d2.m_value; }
		    public static bool operator ==(degree d1, degree d2) { return d1.m_value == d2.m_value; }
		    public static bool operator !=(degree d1, degree d2) { return d1.m_value != d2.m_value; }

		    public int CompareTo(degree other)
		    {
		        if (this < other) return -1;
		        if (this > other) return +1;
		        return 0;
		    }
		}
	}
}
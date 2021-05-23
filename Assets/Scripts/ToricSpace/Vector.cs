/************************************************************************/
/* Vector: represents any vector in Cartesian Space                     */
/************************************************************************/
using UnityEngine;
namespace vcf
{
	namespace tools
	{
		public class Vector
		{
			protected float[] m_vec;

	        public Vector(int size)
	        {
	            m_vec = new float[size];
	        }

	        public Vector(int length, float val)
	        {
	            m_vec = new float[length];
	            for(int i = 0; i < length; i++) m_vec[i] = 0;
	        }

	        public Vector(Vector v)
	        {
	            m_vec = new float[v.length];
	            for (int i = 0; i < length; i++) this[i] = v[i];
	        }

	        public int length { get { return m_vec.Length; } }

	        public float[] val { get { return m_vec; } set { m_vec = value; } }

	        public float this[int i] { get { return m_vec[i]; } set { m_vec[i] = value; } }

			public float norm2()
			{
	            return DotProduct(this, this);
			}

			public float norm()
			{ 
				return Mathf.Sqrt(norm2());
			}

			public Vector normalize()
			{
				float n = norm();
				if(n > 0)
				{
					for(int i=0;i<length;i++) m_vec[i] /= n;
				}
	            return this;
			}

	        public Vector normalized()
	        {
	            Vector res = new Vector(this);
	            res.normalize();
	            return res;
	        }

	        public static Vector operator-(Vector v)
			{
				Vector res = new Vector(v);
				res.negate();
				return res;
			}

			public void negate()
			{
				for(int i=0; i < length; i++)
					m_vec[i] = -m_vec[i];
			}

			public static Vector operator+(Vector u, Vector v)
			{
				Vector res = new Vector(u);
				res.add(v);
				return res;
			}

			public void add(Vector v)
			{
				for(int i = 0; i < length; i++)
					m_vec[i] += v.m_vec[i];
			}

			public static Vector operator-(Vector u, Vector v)
			{
				Vector res = new Vector(u);
				u.subtract(v);
	            return res;
			}

			public void subtract(Vector v)
			{
				for(int i = 0; i < length; i++)
					m_vec[i] -= v.m_vec[i];
			}

			public static Vector operator*(Vector v, float s)
			{
				Vector res = new Vector(v);
				res.multiply(s);
				return res;
			}

			public void multiply(float s) 
			{
				for(int i = 0; i < length; i++)
					m_vec[i] *= s;
			}

	        /// multiply element by element
			public Vector multiplied(Vector v)
			{
				Vector res = new Vector(this);
				res.multiply(v);
	            return res;
			}

			/// multiply element by element
			public void multiply(Vector v)
			{
				for(int i = 0; i < length; i++)
					m_vec[i] *= v.m_vec[i];
			}

	        /// divide element by element
			public Vector divided(Vector v)
			{
				Vector res = new Vector(this);
	            res.divide(v);
				return res;
			}

			/// divide element by element
			public void divide(Vector v)
			{
				for(int i = 0; i < length; i++)
					m_vec[i] /= v.m_vec[i];
			}
	        
			/// dot product
			public static float operator*(Vector u, Vector v)
			{
				return DotProduct(u,v);
			}

			/** \brief Scalar division */
	        /*
			inline Vector<SIZE, ValueType> operator/ (ValueType const & v) const
			{
				Vector<SIZE, ValueType> result ;
				for(int cpt=0 ; cpt<SIZE ; cpt++)
					result[cpt] = m_vec[cpt]/v ;
				return result ;			
			}

			inline void operator/= (ValueType const & v)
			{
				for(int cpt=0 ; cpt<SIZE ; cpt++)
					m_vec[cpt] /= v ;		
			}

			inline bool operator==(const Vector<SIZE,ValueType> & v) const
			{
				for(int i=0;i<SIZE;i++)
				{
					if( m_vec[i] != v.m_vec[i] )
						return false;
				}
				return true;
			}

			inline bool operator!=(const Vector<SIZE,ValueType> & v) const
			{
				return !operator==(v);
			}
	        */

			/** \brief Comparison operator (lexicographical order) */
			public static bool operator<(Vector u, Vector v)
			{
	            if(u.length < v.length) return true;

				for(int i=0 ; i<u.length ; i++)
				{
					if( u[i] == v[i] ) continue ;
					return u[i] < v[i] ;
				}
				return false ;
			}

	        public static bool operator >(Vector u, Vector v)
	        {
	            if (u.length > v.length) return true;

	            for (int i = 0; i < u.length; i++)
	            {
	                if (u[i] == v[i]) continue;
	                return u[i]> v[i];
	            }
	            return false;
	        }

	        public static float DotProduct(Vector u, Vector v)
	        {
	            float p = 0;
	            for (int i = 0; i < u.length; i++)
	                p += u[i] * v[i];
	            return p;
	        }

	        public static radian Angle(Vector u, Vector v)
	        {
	            float lenProduct = u.norm() * v.norm();

	            // Divide by zero check
	            if (lenProduct < 1e-6f)
	                lenProduct = 1e-6f;

	            float f = DotProduct(u, v) / lenProduct;

	            f = Math.Clamp(f, -1.0f, +1.0f);
				
				u.normalize();
				v.normalize();

	            if (f < 0.0f)
	                return new radian(Math.PI - 2.0f * Mathf.Asin((-v - u).norm() / 2.0f));
	            else
	                return new radian(2.0f * Mathf.Asin((v - u).norm() / 2.0f));
	        }
		};
	}
}
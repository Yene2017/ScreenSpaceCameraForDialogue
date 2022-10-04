using UnityEngine;

namespace vcf
{
	namespace tools
	{
		public class Matrix3 : Matrix
		{
		    public Matrix3() : base(3,3,0) {}

		    public Matrix3(Matrix3 m3x3) : base(m3x3) {}

			public float getDeterminant()
	        {
	            float fCofactor00 = m_mat[1,1]*m_mat[2,2] -
	                m_mat[1,2]*m_mat[2,1];
	            float fCofactor10 = m_mat[1,2]*m_mat[2,0] -
	                m_mat[1,0]*m_mat[2,2];
	            float fCofactor20 = m_mat[1,0]*m_mat[2,1] -
	                m_mat[1,1]*m_mat[2,0];

	            float fDet =
	                m_mat[0,0]*fCofactor00 +
	                m_mat[0,1]*fCofactor10 +
	                m_mat[0,2]*fCofactor20;

	            return fDet;
	        }

	        public void swapRow(int i, Vector3 row)
	        {
	            for(int j=0; j<numColumns; j++)
	                m_mat[i, j] = row[j];
	        }

	        public void swapColumn(int j, Vector3 col)
			{
				for(int i=0;i<numRows;i++)
					m_mat[i,j] = col[i];
			}

	        //public Vector3 getRow(int i)    { return (Vector3)base.getRow(i);    }
	        //public Vector3 getColumn(int j) { return (Vector3)base.getColumn(j); }

	        public static Vector3 operator *(Matrix3 m, Vector3 v)
	        {
	            Vector vec = new Vector(3);
	            for (int i = 0; i < 3; i++) vec[i] = v[i];
	            Matrix mbase = m;
	            vec = mbase * vec;
	            return new Vector3((float)vec[0], (float)vec[1], (float)vec[2]);
	        }

	        public void transpose()
	        {
	            for (int i = 0; i < numRows; i++)
	            {
	                for (int j = i + 1; j < numColumns; j++)
	                {
	                    float tmp = this[i, j];
	                    this[i, j] = this[j, i];
	                    this[j, i] = tmp;
	                }
	            }
	        }

#pragma warning disable CS0108 // 成员隐藏继承的成员；缺少关键字 new
            public Matrix3 transposed()
#pragma warning restore CS0108 // 成员隐藏继承的成员；缺少关键字 new
            {
	            Matrix3 m = new Matrix3(this);
	            m.transpose();
	            return m;
	        }

			public Matrix3 inverse(){
				Matrix3 inverted = new Matrix3();
				float det = getDeterminant();
				inverted[0,0] = (this[2,2]*this[1,1]-this[2,1]*this[1,2])/det;
				inverted[1,0] = -(this[2,2]*this[1,0]-this[2,0]*this[1,2])/det;
				inverted[2,0] = (this[2,1]*this[1,0]-this[2,0]*this[1,1])/det;

				inverted[0,1] = -(this[2,2]*this[0,1]-this[2,1]*this[0,2])/det;
				inverted[1,1] = (this[2,2]*this[0,0]-this[2,0]*this[0,2])/det;
				inverted[2,1] = -(this[2,1]*this[0,0]-this[2,0]*this[0,1])/det;

				inverted[0,2] = (this[1,2]*this[0,1]-this[1,1]*this[0,2])/det;
				inverted[1,2] = -(this[1,2]*this[0,0]-this[1,0]*this[0,2])/det;
				inverted[0,2] = (this[1,1]*this[0,0]-this[1,0]*this[0,1])/det;
				return inverted;
			}
		};
	}
}
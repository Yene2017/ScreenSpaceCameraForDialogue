using UnityEngine;
namespace vcf
{
	namespace tools
	{
		public class Matrix
		{
	        protected float[,] m_mat;

	        public Matrix(int rows, int cols)
	        {
	            m_mat = new float[rows,cols];
	        }

	        public Matrix(Pair<int,int> dim)
	        {
	            m_mat = new float[dim.First,dim.Second];
	        }

	        public Matrix(int rows, int cols, float val)
	        {
	            m_mat = new float[rows,cols];
	            for(int i=0;i<rows;i++)
		    	    for(int j=0;j<cols;j++)
					    m_mat[i,j] = val;
			}
			
			public Matrix(Matrix4x4 mat)
			{
				m_mat = new float[4,4];
				for(int i=0;i<4;i++)
					for(int j=0;j<4;j++)
						m_mat[i,j] = mat[i,j];
			}

		    public Matrix(Matrix m)
		    {
	            m_mat = new float[m.numRows,m.numColumns];
			    for(int i=0;i<numRows;i++)
		    		for(int j=0;j<numColumns;j++)
						m_mat[i,j] = m.m_mat[i,j];
			}

			public Pair<int,int> dimensions { get{ return new Pair<int,int>(numRows,numColumns); } }

	        public int numRows    { get { return m_mat.GetLength(0); } }
	        public int numColumns { get { return m_mat.GetLength(1); } }

	        public float this[int i, int j]
	        {
	            get { return m_mat[i, j]; }
	            set { m_mat[i, j] = value; }
	        }

			public Vector getColumn(int  j)
			{
				Vector col = new Vector(numRows);
				for(int i=0;i<numRows;i++)
					col.val[i] = m_mat[i,j];
				return col;
			}
			
			public Vector getRow(int i)
			{
				Vector row = new Vector(numColumns);
				for(int j=0;j<numColumns;j++)
					row.val[j] = m_mat[i,j];
				return row;
			}

			public float getRowSum(int i) 
			{
				float sum=0;
				for(int j=0;j<numColumns;j++)
					sum += m_mat[i, j];
				return sum;
			}

			public float getColumnSum(int j) 
			{
				float sum=0;
				for(int i=0;i<numRows;i++)
	                sum += m_mat[i, j];
				return sum;
			}

			public void normalizeRows()
			{
				for(int i = 0; i < numRows; i++)
				{
					float sum = getRowSum(i);
					if( sum != 0 )
					{
						for(int j = 0; j < numColumns; j++)
							m_mat[i,j] /= sum;
					}
					else
					{
						for(int j = 0; j < numColumns; j++)
							m_mat[i,j] = 1.0F / numColumns;
					}
				}
			}

			public void normalizeColumns()
			{
	            for (int j = 0; j < numColumns; j++)
				{
					float sum = getColumnSum(j);
					if( sum != 0 )
					{
						for(int i=0; i<numRows; i++)
	                        m_mat[i, j] /= sum;
					}
					else
					{
						for(int i=0; i<numRows; i++)
	                        m_mat[i, j] = 1.0F / numRows;
					}
				}
			}

			public Matrix transposed()
			{
				Matrix res = new Matrix(numColumns,numRows);
				for(int i=0;i<numRows;i++) 
					for(int j=0;j<numColumns;j++)
	                    res.m_mat[j, i] = m_mat[i, j];
				return res;
			}

	        public static Matrix operator*(Matrix a, Matrix b)
	        {
	            Math.Assert(a.numColumns == b.numRows);
				Matrix res = new Matrix(a.numRows,b.numColumns,0);
				for(int i=0;i<a.numRows;i++) // for each row of our matrix m
	            {
	                for(int j=0;j<b.numColumns;j++) // and each column of the passed matrix m'
					{
						// process the res[i][j] value by multiplying the ith row of m
						// and the jth column of m'
						for(int k=0;k<a.numColumns;k++)
							res.m_mat[i,j] += (a.m_mat[i,k] * b.m_mat[k,j]) ;
					}
	            }
				return res;
			}

			public static Vector operator*(Matrix m, Vector v) // v = a column matrix with v.numRows = m.numColumns
			{
	            Math.Assert(v.length == m.numColumns);

				Vector res = new Vector(m.numRows,0); // matrix RowSize x 1

				for(int i=0;i<m.numRows;i++) // for each row of our matrix m
					for(int j=0;j<m.numColumns;j++)
						res.val[i] += (m.m_mat[i,j] * v.val[j]) ;
				return res;
			}

			public static Matrix operator+(Matrix a, Matrix b)
			{
	            Math.Assert(a.dimensions == b.dimensions);
				Matrix res = new Matrix(a.dimensions);
				for(int i=0;i<a.numRows;i++) // for each row of both matrices m
					for(int j=0;j<a.numColumns;j++) // and each column of both matrices
						// make the addition
						res.m_mat[i,j] = (a.m_mat[i,j] + b.m_mat[i,j]) ;
				return res;
			}

			public static Matrix operator-(Matrix a, Matrix b)
	        {
	            Math.Assert(a.dimensions == b.dimensions);
	            Matrix res = new Matrix(a.dimensions);
	            for (int i = 0; i < a.numRows; i++) // for each row of both matrices m
	                for (int j = 0; j < a.numColumns; j++) // and each column of both matrices
						// make the addition
						res.m_mat[i,j] = (a.m_mat[i,j] - b.m_mat[i,j]) ;
				return res;
			}
		};
	}
}
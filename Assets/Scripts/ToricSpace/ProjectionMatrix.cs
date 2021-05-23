using UnityEngine;

namespace vcf
{
	namespace tools
	{
		class ProjectionMatrix
	{
        private Vector3 m_position;
		private Matrix3 m_rotation, m_transposed;
		private float m_Sx, m_Sy;

		public static void ComputeScale(radian fovX, float aspect, out float Sx, out float Sy)
	    {
		    // angle of view computation within [-1;+1]:
		    // -----------------------------------------
		    //				 d/2
		    //	tan(fov/2) = ---
		    //				  S
		    //
		    // with d the size of half the camera plane (here 1 both for width and height)
		    // and  S the distance between the viewer and the camera plane
		    //

		    float tanX = Math.Tan(fovX/2);
		    Sx = 1 / tanX;
		    Sy = aspect * Sx;
	    }

	    public ProjectionMatrix(radian fovX, float aspect, Vector3 p, Matrix3 m)
	    {
            m_position = p;
            m_rotation = new Matrix3(m);
            m_transposed = m_rotation.transposed();
		    ComputeScale(fovX, aspect, out m_Sx, out m_Sy);
	    }

        public ProjectionMatrix(radian fovX, float aspect, Vector3 p, Quaternion q)
		{
		    m_position = p;
            m_rotation = GetMatrix(q);
            m_transposed = m_rotation.transposed();
            ComputeScale(fovX, aspect, out m_Sx, out m_Sy);
	    }

        public void setPosition(Vector3 p)
	    {
		    m_position = p; 
	    }

	    public void setOrientation(Quaternion q)
	    {
            m_rotation = GetMatrix(q);
            m_transposed = m_rotation.transposed();
	    }

	    public Vector3 project(Vector3 worldCoords) 
	    {
            Vector3 screenCoords = worldCoords;
		    screenCoords -= m_position; // world-oriented coordinate system with origin at the camera's position

		    screenCoords = (m_rotation * screenCoords); // camera coordinate system: x=right, y=up, z=depth
		
		    screenCoords.x = ( screenCoords.x * (float)m_Sx ) / screenCoords.z; // x in [-1;+1] if in the frame
		    screenCoords.y = ( screenCoords.y * (float)m_Sy ) / screenCoords.z; // y in [-1;+1] if in the frame
		    //screenCoords.z() = screenCoords.z(); // depth

		    return screenCoords; // x,y normalized coordinates, i.e. projection into the screen space
	    }

	    public Vector3 inverseProjection(Vector3 screenCoords) 
	    {
		    float
			    x = ( screenCoords.x * screenCoords.z ) / m_Sx,
			    y = ( screenCoords.y * screenCoords.z ) / m_Sy,
			    z = screenCoords.z;
            Vector3 worldCoords = new Vector3((float)x, (float)y, (float)z); // camera coordinate system

		    worldCoords = (m_transposed * worldCoords); // world-oriented coordinate system with origin at the camera's position

		    worldCoords += m_position; // world coordinates

		    return worldCoords;
	    }

        //Matrix3 GetMatrix(Vector3 right, Vector3 forward, Vector3 up)
        //{
        //    Matrix3 m = new Matrix3();

        //    m.swapRow(0, right);
        //    m.swapRow(1, up);
        //    m.swapRow(2, forward);

        //    return m;
        //}

	    private static Matrix3 GetMatrix(Quaternion q)
	    {
		    //Vector3	r, f, u; // right, forward, up
		    //q.toAxes(out r, out f, out u);
            //return GetMatrix(r, f, u);

            Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
            Matrix3 m3 = new Matrix3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    m3[i, j] = m[i, j];
                }
            }
            m3.transpose();
            return m3; // transposed rotation matrix
	    }
	}
	}
}
using System.Collections.Generic;
using UnityEngine;

namespace vcf
{
	namespace tools
	{
		public class ToricManifold
		{
			public class Equation
			{
				public Vector3 m_A, m_B;
				public radian m_alpha;
				
				/// build the equation of a solution torus around the line segment [AB]
				public Equation(Vector3 wposA, Vector3 wposB, radian alpha)
				{
					m_A = wposA;
					m_B = wposB;
					m_alpha = alpha;
				}
				
				float value(Vector3 p)
				{
					Vector3 PA = (m_A - p), PB = (m_B - p);
					return ( PA.angle(PB) - m_alpha ).valueRadians();
				}
				
				float absValue(Vector3 p)
				{
					return System.Math.Abs( value(p) );
				}
			};
			
			private Equation m_equation;
			private radian m_alpha;
			private Quaternion m_q;
			
			public radian getAlpha() { return m_alpha; }
			
			public ToricManifold(radian fov, float aspect, Vector2 pA, Vector2 pB, Vector3 wposA, Vector3 wposB)
			{
				create(fov, aspect, pA, pB, wposA, wposB);
			}
			
			public void create(radian fov, float aspect, Vector2 pA, Vector2 pB, Vector3 wposA, Vector3 wposB)
			{
				float Sx, Sy;
				ProjectionMatrix.ComputeScale(fov, aspect, out Sx, out Sy);
				
				Vector3 pA3 = new Vector3(pA.x / (float)Sx, pA.y / (float)Sy, 1); // projects at pA on screen
				Vector3 pB3 = new Vector3(pB.x / (float)Sx, pB.y / (float)Sy, 1); // projects at pB on screen
				m_alpha = new radian(Mathf.Clamp((pA3.angle(pB3)).valueRadians(),0.001f, Mathf.PI-0.001f));
				
				Vector3 A = wposA, B = wposB;
				
				m_equation = new Equation(A, B, m_alpha);
				
				Vector3 fwd, up = new Vector3(0, 1, 0); // right, forward, up
				// remark: we here use vectors that will be suitable when cast into Ogre space
				{
					pA3.Normalize(); pB3.Normalize();
					
					fwd = pA3 + pB3; fwd.Normalize(); // forward (look-at)
					up = Vector3.Cross(pA3, pB3);   up.Normalize(); // up
				}
				
				m_q = Quaternion.LookRotation(fwd, up);
			}
			
			public Vector3 computePosition(radian2 theta, radian phi)
			{
				// NB: theta should be (strictly) between 0 and 2 * (pi - alpha)
				
				Toric3 t = new Toric3(m_alpha, theta, phi);
				return Toric3.ToWorldPosition(m_equation.m_A, m_equation.m_B, t);
			}
			
			public Vector3 computePosition(float thetaRatio, radian phi)
			{
				// NB: theta should be (strictly) between 0 and 2 * (pi - alpha)
				radian2 theta =  new radian2(Mathf.Min(Mathf.Max(thetaRatio,0.001f),0.999f) * 2*(Mathf.PI-m_alpha.valueRadians()));
				Toric3 t = new Toric3(m_alpha, theta, phi);
				return Toric3.ToWorldPosition(m_equation.m_A, m_equation.m_B, t);
			}
			

			
			public Vector3 computeProjection(Vector3 position, float minDist = 0.0001f)
			{
				// NB: theta should be (strictly) between 0 and 2 * (pi - alpha)
				radian phi = Toric3.FromWorldPosition(m_equation.m_A, m_equation.m_B, position).phi;
				
				Vector3 center = computeCenter(phi);
				
				
				Vector3 perp = Vector3.Cross(m_equation.m_A - center, m_equation.m_B - center);
				Vector3 perp2 = Vector3.Cross(perp, m_equation.m_B - center);
				float theta = (new degree(Vector3.Angle(position-center, m_equation.m_B-center))).valueRadians();
				if(Vector3.Dot(perp2, center-position)>0)
					theta = 2.0f*Mathf.PI-theta;
                float thresholdAngle = Mathf.Max(Constraints.ThetaFromDistance((m_equation.m_A-m_equation.m_B).magnitude,m_alpha,minDist),0.0001f);
                theta = Mathf.Clamp(theta, thresholdAngle, 2 * (Mathf.PI - m_alpha.valueRadians()) - thresholdAngle);
								
				Toric3 t = new Toric3(m_alpha, new radian2(theta), phi);
				Vector3 pos = Toric3.ToWorldPosition(m_equation.m_A, m_equation.m_B, t);

				return pos;
			}
			
			public Vector3[] computePositions(int N, int M)
			{
				Vector3[] res = new Vector3[N*M];
				
				int cpt=0;
				for(int i=0; i<M; i++)
				{
					radian phi = new radian((Math.PI_2 * i) / M);
					for(int j=0; j<N; j++)
					{
						float ratio = (float)(j+1) / (N+1);
						radian2 theta = Toric3.ComputeTheta(ratio,m_alpha);
						res[cpt++] = computePosition(theta,phi);
					}
				}
				
				return res;
			}
			
			public Quaternion computeOrientation(Vector3 position)
			{
				Vector3 fwd, up = new Vector3(0,1,0); // right-forward-up coord system of the camera
				
				Vector3 PA = (m_equation.m_A-position); PA.Normalize();
				Vector3 PB = (m_equation.m_B-position); PB.Normalize();
				
				fwd = PA + PB;
				fwd.Normalize();
				
				//up = Vector3.Cross(PA, PB);
				//up.Normalize();
				
				Quaternion q = Quaternion.LookRotation(fwd,up);
				
				return q * Quaternion.Inverse(m_q);
			}
			
			public Vector3 computeCenter(radian phi){
				Vector3 vecAB = m_equation.m_B-m_equation.m_A;
				float AB = vecAB.magnitude;
				
				Vector3 n = vecAB;
				// process z such that z.y() = 0 (warning: the following code will not work if n.x() and n.z() are both 0)
				Vector2 n2 = n.projectY(); Vector2Extensions.Rotate90(ref n2);
				Vector3 z = new Vector3(0,0,0); Vector3Extensions.UnprojectY(ref z, n2);
				
				Vector3 vecABnorm = n.normalized;
				Quaternion qP = Quaternion.AngleAxis((float)(new radian(phi)).valueDegrees(), vecABnorm);
				
				
				radian angle = new radian(Mathf.PI/2.0f - m_alpha.valueRadians());
				
				Vector3 t = Vector3.Cross(z, n);
				
				Quaternion qT = Quaternion.AngleAxis((float)angle.valueDegrees(), t);
				float d = AB/(2.0f*Mathf.Sin(m_alpha.valueRadians()));
				
				Vector3 res = qP * qT * vecABnorm * (float) d;
				res += m_equation.m_A;
				
				return res;
			}
			
		}
		
		public class Toric3
		{
			public radian  alpha { get; set; }
			public radian2 theta { get; set; }
			public radian  phi   { get; set; }
			
			public Toric3(radian mAlpha, radian2 mTheta, radian mPhi)
			{
				set(mAlpha, mTheta, mPhi);
			}
			
			public Toric3(radian mAlpha, float mThetaRatio, radian mPhi)
			{
				set(mAlpha, ComputeTheta(mThetaRatio,mAlpha), mPhi);
			}
			
			public void set(radian mAlpha, radian2 mTheta, radian mPhi)
			{
				Math.Assert(Math.Within(mAlpha.valueRadians(), 0, Math.PI));
				Math.Assert(Math.Within(mTheta.valueRadians(), 0, 2 * (Math.PI - mAlpha.valueRadians())));
				
				alpha = mAlpha;
				theta = mTheta;
				phi   = mPhi;
			}
			
			// wposP represents the camera position
			public static Toric3 FromWorldPosition(Vector3 wposA, Vector3 wposB, Vector3 wposP)
			{
				Vector3 AB = wposB-wposA; 
				Vector3 AP = wposP-wposA;
				
				Vector3 n = Vector3.Cross(AP, AB);
				// process z such that z.y() = 0 (warning: the following code will not work if n.x() and n.z() are both 0)
				Vector2 n2 = AB.projectY(); Vector2Extensions.Rotate270(ref n2);
				Vector3 z = new Vector3(0,0,0); Vector3Extensions.UnprojectY(ref z, n2);
				
				radian beta = AB.angle(AP);
				radian alpha = new radian(Mathf.Clamp(Constraints.ComputeAlpha(AB.magnitude, beta, AP.magnitude).valueRadians(),0.001f, Mathf.PI-0.001f));
				radian2 theta = new radian2(Mathf.Clamp(beta.valueRadians() * 2, 0.001f,2*(Mathf.PI-alpha.valueRadians())-0.001f));
				radian phi = (radian)(Math.PI / 2 - z.directedAngle(n, AB).valueRadians());
				
				return new Toric3(alpha, theta, phi);
			}
			
			public static Vector3 ToWorldPosition(Vector3 wposA, Vector3 wposB, Toric3 t3)
			{
				Vector3 vecAB = wposB-wposA;
				float AB = vecAB.magnitude;
				
				Vector3 n = vecAB;
				// process z such that z.y() = 0 (warning: the following code will not work if n.x() and n.z() are both 0)
				Vector2 n2 = n.projectY(); Vector2Extensions.Rotate90(ref n2);
				Vector3 z = new Vector3(0,0,0); Vector3Extensions.UnprojectY(ref z, n2);
				
				Vector3 vecABnorm = n.normalized;
				Quaternion qP = Quaternion.AngleAxis((float)t3.phi.valueDegrees(), vecABnorm);
				radian beta = t3.theta / 2;
				
				Vector3 t = Vector3.Cross(z, n);
				
				Quaternion qT = Quaternion.AngleAxis((float)beta.valueDegrees(), t);
				float d = Constraints.ComputeDistanceFromA(AB, t3.alpha, t3.theta);
				
				Vector3 res = qP * qT * vecAB;
				res *= (float) d;
				res /= (float) AB;
				res += wposA;
				
				return res;
			}
			
			public static float ComputeThetaRatio(radian2 theta, radian alpha)
			{
				//assert( theta() < 2 * (PI-alpha()) );
				return theta.valueRadians() / ( 2 * (Math.PI - alpha.valueRadians()) );
			}
			
			public static radian2 ComputeTheta(float theta_r, radian alpha)
			{
				//assert( 0 < theta_r && theta_r < 1 );
				return (radian2) theta_r * ( 2 * (Math.PI - alpha.valueRadians()) );
			}
		}
	}
}
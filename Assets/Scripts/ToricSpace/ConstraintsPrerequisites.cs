using UnityEngine;
using vcf.tools;

namespace vcf
{
	namespace tools
	{
		public class Constraints
		{
			public static float ComputeDistanceFromA(float AB, radian alpha, radian2 theta)
			{
				return ( AB * Math.Sin(alpha+theta/2.0f) ) / Math.Sin(alpha);
			}
			
			public static float ComputeBetaFromA(float AB, radian alpha, float distanceFromA)
			{
				return Mathf.PI-Mathf.Asin(distanceFromA * Math.Sin(alpha)/AB)-alpha.valueRadians();
			}
			
			public static float ComputeDistanceFromB(float AB, radian alpha, radian2 theta)
			{
				return Mathf.PI-( AB * Math.Sin(theta/2.0f) ) / Math.Sin(alpha);
			}
			
			public static float ComputeBetaFromB(float AB, radian alpha, float distanceFromB)
			{
				return Mathf.Asin(distanceFromB * Math.Sin(alpha)/AB);
			}
			
			public static float ComputeDistanceFromA(Vector3 A, Vector3 B, Toric3 t)
			{
				return ComputeDistanceFromA((B-A).magnitude, t.alpha, t.theta);
			}
			
			public static float ComputeDistanceFromB(Vector3 A, Vector3 B, Toric3 t)
			{
				return ComputeDistanceFromB((B-A).magnitude, t.alpha, t.theta);
			}
			
			public static radian ComputeAlpha(float AB, radian beta, float d)
			{
				float num = d - AB * Math.Cos(beta);
				float den = Mathf.Sqrt( Math.Square(d) + Math.Square(AB) - 2.0f * d * AB * Math.Cos(beta) );
				
				float val = Math.Clamp(num / den, -1.0f, +1.0f);
				radian alpha = (radian) System.Math.Acos( val );
				
				return alpha;
			}
            public static void ComputeCirclesIntersection(float AB, radian alpha, float r0, bool nearB, out float radius, out float distanceFromA)
            {
                float r1 = AB / (2.0f * Math.Sin(alpha));
                float h = Mathf.Sqrt(r1 * r1 - AB * AB / 4.0f);
                float Dist = Mathf.Sqrt(AB * AB / 4.0f + h * h);
                float a = nearB ? AB : 0;
                float b = 0;
                float c = AB / 2.0f;
                float d = -h;
                Vector2 center = new Vector2(c, d);

                float sigma = Mathf.Sqrt((Dist + r0 + r1) * (Dist + r0 - r1) * (Dist - r0 + r1) * (-Dist + r0 + r1)) / 4.0f;

                float x1 = (a + c) / 2.0f + (c - a) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) + 2.0f * (b - d) * sigma / (Dist * Dist);
                float x2 = (a + c) / 2.0f + (c - a) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) - 2.0f * (b - d) * sigma / (Dist * Dist);
                float y1 = (b + d) / 2.0f + (d - b) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) - 2.0f * (a - c) * sigma / (Dist * Dist);
                float y2 = (b + d) / 2.0f + (d - b) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) + 2.0f * (a - c) * sigma / (Dist * Dist);

                Vector2 pos;
                Vector2 vec1 = new Vector2(x1, y1);
                Vector2 vec2 = new Vector2(x2, y2);
                /*
                if(Vector2.Dot(Vector2.up, vec1) > 0.0f){
                    angle = new degree(Vector2.Angle(Vector2.right, vec2)).valueRadians();
                    distance = vec2.magnitude;
                }else{
                    angle = new degree(Vector2.Angle(Vector2.right, vec1)).valueRadians();
                    distance = vec1.magnitude;
                }*/
                /**/
                if (vec1.y > 0.0f)
                    pos = vec2;
                else if (vec2.y > 0.0f)
                    pos = vec1;
                else if (nearB)
                    pos = (vec1 - new Vector2(AB, 0.0f)).magnitude < vec1.magnitude ? vec1 : vec2;
                else
                    pos = (vec1 - new Vector2(AB, 0.0f)).magnitude < vec1.magnitude ? vec2 : vec1;
                //Debug.Log(pos.magnitude);

                if (pos.x == AB / 2.0f)
                {
                    radius = pos.y;
                    distanceFromA = AB / 2.0f;
                }
                else if (pos.y == d)
                {
                    radius = -1.0f;
                    distanceFromA = (center - pos).magnitude;
                }
                else
                {
                    float aeq = (pos.y - center.y) / (pos.x - center.x);
                    float beq = pos.y - aeq * pos.x;
                    distanceFromA = -beq / aeq;
                    Vector2 centerApprox = new Vector2(distanceFromA, 0.0f);
                    radius = (centerApprox - pos).magnitude;
                }

            }
		
			public static float ThetaRatioFromDistance(float AB, radian alpha, float r0){
                float r1 = AB / (2.0f * Math.Sin(alpha));
                if (r0 > 2 * r1)
                    return 3.14f / (2*(Mathf.PI - alpha.valueRadians()));
				/*float h = Mathf.Sqrt(r1*r1-AB*AB/4.0f);
				float Dist = Mathf.Sqrt(AB*AB/4.0f+h*h);
				float a = 0;
				float b = 0;
				float c = AB/2.0f;
				float d = -h;
				Vector2 center = new Vector2(c,d);
                if (r0 > 2* r1)
                    return 3.14f/(Mathf.PI-2*alpha.valueRadians());
				
				float sigma = Mathf.Sqrt((Dist+r0+r1)*(Dist+r0-r1)*(Dist-r0+r1)*(-Dist+r0+r1))/4.0f;
				
				float x1 = (a+c)/2.0f + (c-a)*(r0*r0-r1*r1)/(2.0f*Dist*Dist) + 2.0f*(b-d)*sigma/(Dist*Dist);
				float x2 = (a+c)/2.0f + (c-a)*(r0*r0-r1*r1)/(2.0f*Dist*Dist) - 2.0f*(b-d)*sigma/(Dist*Dist);
				float y1 = (b+d)/2.0f + (d-b)*(r0*r0-r1*r1)/(2.0f*Dist*Dist) - 2.0f*(a-c)*sigma/(Dist*Dist);
				float y2 = (b+d)/2.0f + (d-b)*(r0*r0-r1*r1)/(2.0f*Dist*Dist) + 2.0f*(a-c)*sigma/(Dist*Dist);

				Vector2 vec1 = new Vector2(x1,y1);
				Vector2 vec2 = new Vector2(x2,y2);

                float angle1 = Vector2.Angle(-center, vec1-center);
                float angle2 = Vector2.Angle(-center, vec2-center);
                radian angle = new radian(angle1);
                if (angle1 < 0 || angle1 > 360 - 2 * alpha.valueDegrees())
                    angle = new radian(angle2 * Mathf.PI / 180);
                if (angle1 < 0 || angle1 > 360 - 2 * alpha.valueDegrees())
                    angle = new radian(angle1 * Mathf.PI / 180);
                else
                    angle = new radian(Mathf.Min(angle1, angle2) * Mathf.PI / 180);
                */
                float angle3 = Mathf.Asin(r0 / (2 * r1));
                float angle4 = Mathf.PI - angle3;
                angle3 = angle3 < 0.0f ? 2 * Mathf.PI + angle3 : angle3;
                angle4 = angle4 < 0.0f ? 2 * Mathf.PI + angle4 : angle4;

                float angle = Mathf.Min(angle3, angle4);
                angle = 2*(Mathf.PI-alpha.valueRadians()) - 2.0f * angle;
				return angle/(2*(Mathf.PI-alpha.valueRadians()));
			}
            public static float ThetaFromDistance(float AB, radian alpha, float r0)
            {
                float r1 = AB / (2.0f * Math.Sin(alpha));
                if (r0 > 2 * r1)
                    return 3.14f;
                /*float h = Mathf.Sqrt(r1 * r1 - AB * AB / 4.0f);
                float Dist = Mathf.Sqrt(AB * AB / 4.0f + h * h);
                float a = 0;
                float b = 0;
                float c = AB / 2.0f;
                float d = -h;
                Vector2 center = new Vector2(c, d);
                if (r0 > 2 * r1)
                    return 3.14f / (Mathf.PI - 2 * alpha.valueRadians());

                float sigma = Mathf.Sqrt((Dist + r0 + r1) * (Dist + r0 - r1) * (Dist - r0 + r1) * (-Dist + r0 + r1)) / 4.0f;

                float x1 = (a + c) / 2.0f + (c - a) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) + 2.0f * (b - d) * sigma / (Dist * Dist);
                float x2 = (a + c) / 2.0f + (c - a) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) - 2.0f * (b - d) * sigma / (Dist * Dist);
                float y1 = (b + d) / 2.0f + (d - b) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) - 2.0f * (a - c) * sigma / (Dist * Dist);
                float y2 = (b + d) / 2.0f + (d - b) * (r0 * r0 - r1 * r1) / (2.0f * Dist * Dist) + 2.0f * (a - c) * sigma / (Dist * Dist);

                Vector2 vec1 = new Vector2(x1, y1);
                Vector2 vec2 = new Vector2(x2, y2);

                float angle1 = Vector2.Angle(-center, vec1 - center);
                float angle2 = Vector2.Angle(-center, vec2 - center);
                radian angle = new radian(angle1);
                if (angle1 < 0 || angle1 > 360 - 2 * alpha.valueDegrees())
                    angle = new radian(angle2 * Mathf.PI / 180);
                if (angle1 < 0 || angle1 > 360 - 2 * alpha.valueDegrees())
                    angle = new radian(angle1 * Mathf.PI / 180);
                else
                    angle = new radian(Mathf.Min(angle1, angle2) * Mathf.PI / 180);

                return angle.valueRadians();
                */
                float angle3 = Mathf.Asin(r0/(2*r1));
                float angle4 = Mathf.PI - angle3;
                angle3 = angle3 < 0.0f ? 2 * Mathf.PI + angle3 : angle3;
                angle4 = angle4 < 0.0f ? 2 * Mathf.PI + angle4 : angle4;

                return 2 * (Mathf.PI - alpha.valueRadians()) - Mathf.Min(2.0f * angle3, 2.0f * angle4);
            }
        };
	}
}

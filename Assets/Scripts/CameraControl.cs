using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using vcf.tools;

public class CameraControlDebuger
{
    public CameraControl target;

    public void OnSceneGUI(SceneView sv)
    {
        var c = target.cam;
        var pc = target.cam.transform.position;
        var pa = target.wposA;
        var pb = target.wposB;
        var sa = c.WorldToViewportPoint(pa);
        var sb = c.WorldToViewportPoint(pb);
        Handles.Label(pa, string.Format("{0:F2} {1:F2} {2:F2}", sa.x, sa.y, sa.z));
        Handles.Label(pb, string.Format("{0:F2} {1:F2} {2:F2}", sb.x, sb.y, sb.z));
    }

    public void Dispose()
    {

    }
}
public class CameraControl : MonoBehaviour
{
    [System.Serializable]
    public struct ActorInfo
    {
        public Transform head;
        public Collider collider;
    }
    public enum Strategy
    {
        None,
        RandomFix,
        SimulatedAnnealing,
        GradientDescend,
    };

    [Header("Toric Space Attributes")]
    // Desired positions on screen
    public Vector2 pA, pB;
    [Range(phiMin, phiMax)]
    public float originalPhi = Mathf.PI;
    [Range(thetaMin, thetaMax)]
    public float originalTheta = Mathf.PI;

    public ActorInfo actor1, actor2;
    public List<Vector3> rayCastSamplePoints;
    public List<GameObject> obstacles;

    [Header("Camera Control Parameters")]
    public Strategy strategy = Strategy.RandomFix;
    public float cameraSpeed = 1f;

    // Toric Space parameters
    public const float thetaMin = 0.001f;
    public const float thetaMax = 2 * (Mathf.PI - 0.57f);
    public const float phiMin   = 0.001f;
    public const float phiMax   = 2 * Mathf.PI;
    [Range(phiMin, phiMax)]
    public float phi;
    [Range(thetaMin, thetaMax)]
    public float theta;
    public float alpha;

    [Header("Camera Attributes")]
    public float fovY;
    public float fovX;
    public Vector3 wposA, wposB;

    public Camera cam;
    private radian fov;
    private ToricManifold toric;
    CameraControlDebuger debuger;
    // Use this for initialization
    // No need to call ctor in Unity
    void Start () 
    {
        debuger = new CameraControlDebuger();
        debuger.target = this;
        SceneView.onSceneGUIDelegate += debuger.OnSceneGUI;
        actor1.head = GameObject.FindGameObjectWithTag("Actor1").transform;
        actor1.collider = actor1.head.GetComponent<Collider>();
        actor2.head = GameObject.FindGameObjectWithTag("Actor2").transform;
        actor2.collider = actor2.head.GetComponent<Collider>();
        obstacles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Obstacle"));

        // Set up camera in Toric space
        cam  = gameObject.GetComponent<Camera>();
        fovY = cam.fieldOfView * Mathf.Deg2Rad;
        fovX = 2.0f * Mathf.Atan(Mathf.Tan(fovY / 2.0f) * cam.aspect);

        wposA = actor1.head.position;
        wposB = actor2.head.position;

        toric = new ToricManifold(new radian(fovY),
                                      cam.aspect, 
                                      pA, 
                                      pB,
                                      wposA,
                                      wposB);

        alpha = toric.getAlpha().valueRadians();
        theta = originalTheta;
        phi   = originalPhi;

        // Generate sample points
        rayCastSamplePoints = new List<Vector3>();
        Vector3 pos1 = actor1.collider.transform.position;
        Vector3 pos2 = actor2.collider.transform.position;
        rayCastSamplePoints.Add(pos1);
        rayCastSamplePoints.Add(pos2);
        Vector3[] deltas = {
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(0, -1, 0),
            new Vector3(-1, 0, 0),
        };
        float r = 0.15f;
        for (int i = 0; i < 6; ++i)
        {
            Vector3 p1 = pos1 + deltas[i] * r;
            Vector3 p2 = pos2 + deltas[i] * r;
            rayCastSamplePoints.Add(p1);
            rayCastSamplePoints.Add(p2);
            rayCastSamplePoints.Add(pos1 + deltas[i] * r * 0.5f);
            rayCastSamplePoints.Add(pos2 + deltas[i] * r * 0.5f);
        }
	}

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= debuger.OnSceneGUI;
        debuger.Dispose();
        debuger = null;
    }

    // Update is called once per frame
    void Update () 
    {
        wposA = actor1.head.position;
        wposB = actor2.head.position;

        toric = new ToricManifold(new radian(fovY),
                                      cam.aspect,
                                      pA,
                                      pB,
                                      wposA,
                                      wposB);

        alpha = toric.getAlpha().valueRadians();
        // Display potential occulsions
        GameObject obstacle;

        IsOcculuded(cam.transform.position, actor1, out obstacle);
        if (obstacle == null)
            IsOcculuded(cam.transform.position, actor2, out obstacle);

        switch (strategy) {
            case Strategy.RandomFix:
                if (obstacle != null)
                    RandomFix();
                break;
            case Strategy.SimulatedAnnealing:
                if (obstacle != null)
                    SimulateAnnealing();
                break;
            case Strategy.GradientDescend:
                GradientDescend();
                break;
        }
        

        UpdateCamera(cam.transform, theta, phi);
    }

    void UpdateCamera(Transform cameraTransform, float newTheta, float newPhi)
    {
        // Update camara position and rotation using toric space values
        Vector3 newPos = toric.computePosition(new radian2(newTheta), new radian(newPhi));
        Quaternion newOri = toric.computeOrientation(newPos);
        cameraTransform.SetPositionAndRotation(newPos, newOri);
    }

    // Determine whether actor is occluded by some specific obstacle
    // Leveraging the Unity physics engine
    bool IsOcculuded(Vector3 camPos, ActorInfo actor, out GameObject obstacle) 
    {
        foreach (GameObject ob in obstacles) {
            Collider obstacleCollider = ob.GetComponent<Collider>();
            RaycastHit[] hits = {};

            if (obstacleCollider.GetType() == typeof(CapsuleCollider)) 
            {
                CapsuleCollider capsule = (CapsuleCollider)obstacleCollider;
                Vector3[] dirs = {
                    new Vector3(1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 1),
                };
                Vector3 dir = dirs[capsule.direction];
                Vector3 point1 = capsule.center + (capsule.height / 2 - capsule.radius) * dir;
                Vector3 point2 = capsule.center - (capsule.height / 2 - capsule.radius) * dir;
                float radius = capsule.radius;

                // Convert to world space values
                point1 = capsule.transform.TransformPoint(point1);
                point2 = capsule.transform.TransformPoint(point2);
                radius = Vector3.Magnitude(
                    point1 - capsule.transform.TransformPoint(
                        capsule.center + capsule.height / 2 * dir
                    )
                );

                // Do the capsule cast
                hits = Physics.CapsuleCastAll(
                    point1, 
                    point2, 
                    radius, 
                    capsule.transform.position - camPos
                );
            } 
            else if (obstacleCollider.GetType() == typeof(SphereCollider)) 
            {
                SphereCollider sphere = (SphereCollider)obstacleCollider;

                // Convert to world space
                Vector3 worldCenter = sphere.transform.TransformPoint(sphere.center);
                float worldRadius = Mathf.Max(
                    new float[] {
                        sphere.transform.lossyScale.x,
                        sphere.transform.lossyScale.y,
                        sphere.transform.lossyScale.z,
                    }
                );

                hits = Physics.SphereCastAll(
                    camPos,
                    worldRadius,
                    worldCenter - camPos
                );
            }
            else
            {
                Debug.LogWarning("Obstacle Collider Type Not Supported");
            }

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.tag == actor.head.tag)
                {
                    float distOb = (hit.collider.transform.position 
                                   - camPos).magnitude;
                    float distAct = (actor.head.position - camPos).magnitude;
                    if (distOb > distAct)
                    {
                        continue;
                    }

                    obstacle = ob;
                    return true;
                }
            }
        }
        obstacle = null;
        return false;
    }

    // Randomly find a new phi and theta value to avoid occlusion
    void RandomFix()
    {
        // The next position vector in Toric space
        // We don't change alpha in this optimization
        // theta in [0, 2*(PI-0.57)]
        // phi   in [0, 2*PI]
        Vector2 origin = new Vector2(originalTheta, originalPhi);
        Vector2 curr = new Vector2(thetaMin, phiMin);

        // Take random samples, 
        // choose the one with smallest distance and maintain visibility
        int samples = 10;
        for (int i = 0; i < samples; ++i) 
        {
            Vector2 next = new Vector2
            {
                x = Random.Range(thetaMin, thetaMax),
                y = Random.Range(phiMin, phiMax)
            };

            if (CompareSolution(origin, curr, next) < 0)
            {
                curr = next;
            }
        }

        // Set new values in Toric Space
        theta = curr.x;
        phi   = curr.y;
    }

    // Use simulate annealing to find a place with no occulusion
    // The search space is (theta, phi)
    // The object is :
    // 1. No occlusion
    // 2. Minimize the magnitude of (delta_theta, delta_phi)
    void SimulateAnnealing()
    {
        // The next position vector in Toric space
        // We don't change alpha in this optimization
        // theta in [0, 2*(PI-0.57)]
        // phi   in [0, 2*PI]
        Vector2 origin = new Vector2(originalTheta, originalPhi);
        Vector2 prev = new Vector2(theta, phi);
        Vector2 curr = new Vector2(thetaMin, phiMin);

        // Take random samples, 
        // choose the one with smallest distance and maintain visibility
        int samples = 10;
        for (int i = 0; i < samples; ++i)
        {
            Vector2 next = new Vector2
            {
                x = Random.Range(thetaMin, thetaMax),
                y = Random.Range(phiMin, phiMax)
            };

            if (CompareSolution(origin, curr, next) < 0)
            {
                curr = next;
            }
        }

        // Use the best sample as initial values to start annealing
        float temperature = 100;
        for (int i = 0; i < 100; ++i)
        {
            Vector2 next = new Vector2
            {
                x = curr.x + Random.Range(-0.3f, 0.3f),
                y = curr.y + Random.Range(-0.3f, 0.3f)
            };

            // Adjust solution if out-of-range
            if (next.x < thetaMin)
            {
                next.x += thetaMax;
            }
            if (next.x > thetaMax) 
            {
                next.x -= thetaMax - 0.001f;
            }
            if (next.y < phiMin)
            {
                next.y += 2 * Mathf.PI;
            }
            if (next.y > phiMax)
                next.y -= 2 * Mathf.PI - 0.001f;

            if (CompareSolution(origin, curr, next) < 0)
            {
                curr = next;
            }
            else
            {
                float p = Mathf.Exp(-(curr-next).magnitude / temperature);
                float rand = Random.Range(0f, 1f);

                if (rand < p)
                {
                    curr = next;
                }
            }

            temperature--;
        }

        // Set new values in Toric Space
        Vector2 change = (curr - prev).normalized;
        if ((curr - prev).magnitude > Time.deltaTime * cameraSpeed)
        {
            change *= cameraSpeed * Time.deltaTime;
        }
        else
        {
            change = curr - prev;
        }
        theta += change.x;
        phi += change.y;
        //theta = curr.x;
        //phi = curr.y;
    }

    // Compare two solution candidates
    // 1 means s1 better than s2, -1 means s1 worse than s2, 0 means equal
    internal int CompareSolution(Vector2 origin, Vector2 s1, Vector2 s2)
    {
        GameObject obstacle;

        float dist1 = (s1 - origin).magnitude;
        float dist2 = (s2 - origin).magnitude;

        UpdateCamera(cam.transform, s1.x, s1.y);
        bool visible1 = !IsOcculuded(cam.transform.position, actor1, out obstacle)
                     && !IsOcculuded(cam.transform.position, actor2, out obstacle);

        UpdateCamera(cam.transform, s2.x, s2.y);
        bool visible2 = !IsOcculuded(cam.transform.position, actor1, out obstacle)
                     && !IsOcculuded(cam.transform.position, actor2, out obstacle);

        if (visible1 && !visible2)
        {
            return 1;
        }
        else if (visible2 && !visible1)
        {
            return -1;
        }
        else if (!visible1 && !visible2)
        {
            return 0;
        }
        else
        {
            return dist1 < dist2 ? 1 :
                (dist1 > dist2 ? -1 : 0);
        }
    }

    void GradientDescend()
    {   
        // The next position vector in Toric space
        // We don't change alpha in this optimization
        // theta in [0, 2*(PI-0.57)]
        // phi   in [0, 2*PI]
        Vector2 origin = new Vector2(originalTheta, originalPhi);
        Vector2 prev = new Vector2(theta, phi);
        Vector2 curr = new Vector2(theta, phi);

        for (int i = 0; i < 10; ++i)
        {
            // Gradient directions
            List<Vector2> direction = new List<Vector2>();
            for (int j = 0; j < 10; ++j) 
            {
                direction.Add(new Vector2(
                    Mathf.Cos(Mathf.Deg2Rad * j * 36),
                    Mathf.Sin(Mathf.Deg2Rad * j * 36)
                ));
            }

            Vector2 next = curr;
            int currentVisibility = Visibility(curr.x, curr.y);
            foreach (Vector2 dir in direction) 
            {
                Vector2 next2 = curr + dir * 0.1f;
                int gra = Visibility(next2.x, next2.y);
                if (gra < currentVisibility)
                {
                    currentVisibility = gra;
                    next = next2;
                }
                else if (gra == currentVisibility)
                {
                    if (gra == 0 && (next2 - origin).magnitude < (next - origin).magnitude)
                    {
                        next = next2;   
                    }
                }
            }

            curr = next;
        }

        // Set new values in Toric Space
        Vector2 change = (curr - prev).normalized;
        if ((curr - prev).magnitude > Time.deltaTime * cameraSpeed) {
            change *= cameraSpeed * Time.deltaTime;
        } else {
            change = curr - prev;
        }
        theta += change.x;
        phi += change.y;
        //theta = curr.x;
        //phi = curr.y;
    }

    int Visibility(float th, float ph)
    {
        int result = 0;

        UpdateCamera(cam.transform, th, ph);

        foreach (Vector3 p in rayCastSamplePoints)
        {
            RaycastHit[] hits;

            hits = Physics.RaycastAll(cam.transform.position, p - cam.transform.position);

            foreach (RaycastHit hit in hits) {
                if (hit.collider.tag == "Obstacle") {
                    float hitDist = (hit.transform.position - cam.transform.position).magnitude;
                    float sampleDist = (p - cam.transform.position).magnitude;
                    if (hitDist < sampleDist)
                    {
                        result++;
                    }
                }
            }
        }

        return result;
    }

    public static void Shuffle(List<Vector3> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    private void OnDrawGizmos()
    {
        //foreach(Vector3 point in rayCastSamplePoints) {
            //Gizmos.DrawSphere(point, 0.1f);
        //}
    }

    /****GUI Hook Functions****/
    public void ResetCamera()
    {
        phi = Mathf.PI;
        theta = Mathf.PI;
        UpdateCamera(cam.transform, Mathf.PI, Mathf.PI);
    }

    public void ChangeCameraControlMethod(UnityEngine.UI.Dropdown change)
    {
        switch (change.value) {
            case 0:
                strategy = Strategy.RandomFix;
                break;
            case 1:
                strategy = Strategy.SimulatedAnnealing;
                break;
            case 2:
                strategy = Strategy.GradientDescend;
                break;
        }
    }
}

#if HasOdin
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CameraSolver
{
    [ExecuteInEditMode]
    public partial class ToricSpaceCameraSolver : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public bool debug;

#if HasOdin
        [OnValueChanged(nameof(RecalculateAll))]
#endif
        [Range(1f, 120)]
        public int vFov = 30;

#if HasOdin
        [OnValueChanged(nameof(RecalculateAll), true)]
#endif
        public ViewportTarget A;

#if HasOdin
        [OnValueChanged(nameof(RecalculateAll), true)]
#endif
        public ViewportTarget B;

        public ControlAxis controlAxis;

#if HasOdin
        [OnValueChanged(nameof(UpdateCamera))]
        [DisableIf("@controlAxis == ControlAxis.AutoDutch")]
#endif
        [Range(-89, 89)]
        public int dutch = 0;

#if HasOdin
        [OnValueChanged(nameof(UpdateCamera))]
        [DisableIf("@controlAxis == ControlAxis.AutoYaw")]
#endif
        [Range(1, 179)]
        public int yaw = 1;

#if HasOdin
        [OnValueChanged(nameof(UpdateCamera))]
        [DisableIf("@controlAxis == ControlAxis.AutoPitch")]
#endif
        [Range(-89, 89)]
        public int pitch = 0;

#if HasOdin
        [Button]
#endif
        [ContextMenu(nameof(Generate))]
        void Generate() => GeneratePrevData();

        private void OnEnable()
        {
            SceneView.duringSceneGui -= OnScneGUI;
            SceneView.duringSceneGui += OnScneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnScneGUI;
        }

        private void Update()
        {
#if !HasOdin
            if (!IsValid()) return;
            if (prs.Count < 1 && controlAxis != ControlAxis.AutoDutch) return;
             _camera.fieldOfView = vFov;
            toric.aspect = _camera.aspect;
            if (toric == null) toric = new Toric();
            UpdateToric();
            UpdateCamera();
#endif
        }

        void RecalculateAll()
        {
            if (!IsValid()) return;
            _camera.fieldOfView = vFov;
            toric.aspect = _camera.aspect;
            if (toric == null) toric = new Toric();
            UpdateToric();
            GeneratePrevData();
            UpdateCamera();
        }

        void UpdateToric()
        {
            toric.diffY = A.Y - B.Y;
            toric.diffX = A.X - B.X;

            toric.vertTan = Mathf.Tan(vFov * 0.5f * Mathf.Deg2Rad);
            toric.horiTan = toric.vertTan * toric.aspect;
            toric.hFov = Mathf.Atan(toric.horiTan) * 2 * Mathf.Rad2Deg;

            var tvt = toric.vertTan * toric.diffY / 2;
            var tht = toric.horiTan * toric.diffX / 2;
            toric.fullTan = Mathf.Sqrt(tvt * tvt + tht * tht);

            toric.radianHalf = Mathf.Atan(toric.fullTan);
            toric.angleHalf = Mathf.Rad2Deg * toric.radianHalf;
            toric.circleRadian = toric.radianHalf * 4;
            toric.center = (A.WPos + B.WPos) / 2;
            toric.halfFB = (A.WPos - B.WPos).magnitude / 2;
            toric.radius = toric.halfFB / Mathf.Sin(toric.radianHalf * 2);
            toric.circleHalfTan = Mathf.Tan(toric.radianHalf * 2);

            toric.AB = (A.WPos - B.WPos).normalized;
            var axisFwd = Vector3.zero;
            if (Mathf.Approximately(toric.AB.y, 0))
            {
                axisFwd = Vector3.Cross(worldUp, toric.AB).normalized;
            }
            else
            {
                var projeV = Vector3.Project(toric.AB, worldUp).normalized;
                axisFwd = Vector3.Cross(toric.AB, projeV).normalized;
            }
            toric.zeroAxisUp = Vector3.Cross(toric.AB, axisFwd).normalized;
        }

        private void UpdateCamera()
        {
            if (!IsValid()) return;
            AutoBalance();
            Calculate(yaw, pitch, out var pos, out var rot);
            _camera.transform.position = pos;
            _camera.transform.rotation = rot;
            transform.position = pos;
            transform.rotation = rot;
        }

        bool IsValid()
        {
            if (!_camera) return false;
            if (!A.target) return false;
            if (!B.target) return false;
            return true;
        }

        private void AutoBalance()
        {
            var min = 8100f;//90*90
            switch (controlAxis)
            {
                case ControlAxis.AutoDutch:
                    Calculate(yaw, pitch, out _, out var rot);
                    var rAxis = rot * Vector3.right; rAxis.Normalize();
                    var projV = rAxis;
                    projV.y = 0;
                    var angle = Vector3.Angle(rAxis, projV);
                    if (rAxis.y < 0) angle *= -1;
                    dutch = (int)angle;
                    break;
                case ControlAxis.AutoPitch:
                    foreach (var pr in prs)
                    {
                        if (pr.axis.x != yaw) continue;
                        var t = (pr.dutch - dutch)  * (pr.dutch - dutch);
                        if (t > min) continue;
                        min = t;
                        pitch = pr.axis.y;
                    }
                    break;
                case ControlAxis.AutoYaw:
                    foreach (var pr in prs)
                    {
                        if (pr.axis.y != pitch) continue;
                        var t = (pr.dutch - dutch)  * (pr.dutch - dutch);
                        if (t > min) continue;
                        min = t;
                        yaw = pr.axis.x;
                    }
                    break;
            }
        }

        void Calculate(int yaw, int pitch, out Vector3 position, out Quaternion rotation)
        {
            // 以A为锚点，如果A横向坐标小于B会导致相机颠倒
            var axisFwd = Vector3.Cross(toric.zeroAxisUp, toric.AB).normalized;
            if (A.X < B.X) axisFwd *= -1;
            // 根据pitch值旋转圆所在平面
            var axisUp = Quaternion.AngleAxis(pitch, toric.AB) * toric.zeroAxisUp;
            //基础圆所在平面的圆心
            var zeroCenter = toric.center + axisFwd * toric.halfFB / toric.circleHalfTan;
            //根据yaw换算以A为半径的线段的旋转角度
            var hAngle = (360 - toric.circleRadian * Mathf.Rad2Deg) * yaw / 180;
            //基础圆所在平面的旋转结果
            var point2d = zeroCenter + Quaternion.AngleAxis(hAngle, toric.zeroAxisUp) * (A.WPos - zeroCenter);
            //pitch后所在平面的旋转结果
            var vector2d = point2d - toric.center;
            var vector3d = Quaternion.AngleAxis(pitch, toric.AB) * vector2d;
            var point3d = toric.center + vector3d;

            //获取在相机局部空间的CA，CB的加向量与叉乘向量的四元数
            var pivotRot = GetPivotRot();
            var pA = (A.WPos - point3d).normalized;
            var pB = (B.WPos - point3d).normalized;

            //根据当前世界空间的CA，CB的加向量与叉乘向量的四元数
            var up = Vector3.Cross(pA, pB).normalized;
            rotation = Quaternion.LookRotation((pA + pB).normalized, up);
            //四元数的差异既是相机矩阵的四元数
            rotation *= Quaternion.Inverse(pivotRot);
            position = point3d;

            //Debug
            var arcCenter = Quaternion.AngleAxis(pitch, toric.AB) * (zeroCenter - toric.center);
            calcStruct = new CalcStruct(axisFwd, axisUp, zeroCenter, arcCenter, hAngle, point2d, point3d);
        }

        Quaternion GetPivotRot()
        {
            // projects at A on screen
            Vector3 pA = new Vector3(A.X * toric.horiTan, A.Y * toric.vertTan, 1);
            // projects at B on screen
            Vector3 pB = new Vector3(B.X * toric.horiTan, B.Y * toric.vertTan, 1);
            pA.Normalize(); pB.Normalize();
            var fwd = pA + pB; fwd.Normalize(); // forward (look-at)
            var up = Vector3.Cross(pA, pB); up.Normalize(); // up
            //if (up.y < 0) up = -up;
            return Quaternion.LookRotation(fwd, up);
        }

#region DefineType

        [System.Serializable]
        public class ViewportTarget
        {
            public Transform target;
            [Range(-1, 1)]
            public float compositionX = 0;
            [Range(-1, 1)]
            public float compositionY = 0;
            public Vector3 WPos => target.position;
            public float X => compositionX;
            public float Y => compositionY;
        }

        [System.Serializable]
        public class Toric
        {
            public float hFov;
            public float diffX;
            public float diffY;
            public float vertTan;
            public float horiTan;
            public float fullTan;
            public float circleHalfTan;
            public float radianHalf;
            public float angleHalf;
            public float halfFB;
            public float radius;
            public Vector3 AB;
            public float circleRadian;
            public Vector3 center;
            public Vector3 zeroAxisUp;
            public int splitCount = 36;
            public float aspect;
        }

        public enum ControlAxis
        {
            AutoDutch,
            AutoYaw,
            AutoPitch,
        }

        [System.Serializable]
        public class ArcPlane
        {
            public Vector3 center;
            public Vector3 axisUp;
            public Vector3 axisRight;
        }

        [System.Serializable]
        public struct PR
        {
            public Vector2Int axis;
            public float dutch;
        }

        [System.Serializable]
        public struct CalcStruct
        {
            public Vector3 axisFwd;
            public Vector3 axisUp;
            public Vector3 zeroCenter;
            public Vector3 arcCenter;
            public float hAngle;
            public Vector3 point2d;
            public Vector3 point3d;

            public CalcStruct(Vector3 axisFwd, Vector3 axisUp, Vector3 zeroCenter, Vector3 arcCenter, float hAngle, Vector3 point2d, Vector3 point3d)
            {
                this.axisFwd = axisFwd;
                this.axisUp = axisUp;
                this.zeroCenter = zeroCenter;
                this.arcCenter = arcCenter;
                this.hAngle = hAngle;
                this.point2d = point2d;
                this.point3d = point3d;
            }
        }

#endregion DefineType

#region PrivateField

        List<PR> prs = new List<PR>();
        ArcPlane[] arcs = new ArcPlane[0];
        Toric toric = new Toric();
        CalcStruct calcStruct = new CalcStruct();
        Texture2D img;
        Vector2Int texPos;
        Vector3 worldUp => Vector3.up;

#endregion PrivateField

    }
}

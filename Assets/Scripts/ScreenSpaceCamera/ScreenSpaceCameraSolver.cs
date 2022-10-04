using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CameraSolver
{
    [System.Serializable, ExecuteInEditMode]
    public partial class ScreenSpaceCameraSolver : MonoBehaviour
    {
        [System.Serializable]
        public class ViewportTarget
        {
            public Transform target;
            [Range(0, 1)]
            public float compositionX = 0.33f;
            [Range(-0.5f, 0.5f)]
            public float compositionY = 0;
        }

        [SerializeField] private Camera _camera;
        public ViewportTarget front;
        public ViewportTarget back;
        [Range(-90, 90)]
        public float yaw = -30;
        [Range(0.1f, 75)]
        public float fov = 30;
        [Range(-90, 90)]
        public float dutch = 0;
        [Range(1, 40)]
        public float itrpPerDutch = 10;

        public double bCompX => back.compositionX;
        public double bCompY => back.compositionY;
        public double fCompX => front.compositionX;
        public double fCompY => front.compositionY;

        public Vector3 wbPosition => back.target.position;
        public Vector3 wfPosition => front.target.position;
        private Transform _calcTarget => _camera.transform;

        private double _aspect;//屏幕长宽比
        private double _overScale;//wb所在相机深度和wf所在相机深度的投影面大小比
        private Vector3 _lookCenter;
        private Vector3 _bp;//wb在相机水平面的投影点
        private Vector3 _fp;//wf在相机水平面的投影点
        private double _sinC;//偏航角的sin值
        private double _cosC;//偏航角的cos值
        private double _tanHalfHorizonFov;//横向fov的一半的tan值

        private void Update()
        {
            Calculate();
        }

        public Transform Calculate()
        {
            if (!_calcTarget) return null;
            if (!Valid()) return null;
            CalculateCameraPos();
            _camera.fieldOfView = fov;
            return _calcTarget;
        }

        private void CalculateCameraPos()
        {
            if (bCompX == 0 || fCompX == 0) return;
            if (bCompX + fCompX == 1) return;

            #region Constant
            var cAngle = Mathf.Abs(yaw);
            _sinC = Mathf.Sin(Mathf.Deg2Rad * cAngle);
            _cosC = Mathf.Cos(Mathf.Deg2Rad * cAngle);
            _aspect = _camera.aspect;
            var tanHalfVerticalFov = Mathf.Tan(Mathf.Deg2Rad * fov / 2);
            _tanHalfHorizonFov = tanHalfVerticalFov * _aspect;
            #endregion

            #region Solve
            //c-相机，wb-背景目标，wf-前景目标，l相机中心线和fb的交点
            //f-前景目标在相机水平面投影，b-背景目标在相机水平面投影
            var clPbl = _sinC / _tanHalfHorizonFov / bCompX - _cosC;
            var clPfl = _sinC / _tanHalfHorizonFov / fCompX + _cosC;
            var blPfb = clPfl / (clPfl + clPbl);
            var flPfb = clPbl / (clPfl + clPbl);
            _overScale = (flPfb / fCompX) / (blPfb / bCompX);
            var x = _sinC / fCompX * 2 / _aspect;
            var clPdh2 = clPfl * clPfl / (abs(fCompY - bCompY * _overScale) * x * x);
            var clPfb = clPbl * clPfl / (clPfl + clPbl);
            var clPfb2 = clPfb * clPfb;
            var clPwfwb2 = clPfb2 * clPdh2 / (clPfb2 + clPdh2);
            var wfwb = Vector3.Distance(wbPosition, wfPosition);
            var cl = wfwb * sqrt(clPwfwb2);
            var fl = cl / clPfl;
            var bl = cl / clPbl;
            var bS = bl * _sinC / bCompX / _aspect * 2;
            var fS = fl * _sinC / fCompX / _aspect * 2;
            var fY = fS * fCompY;
            var bY = bS * bCompY;
            #endregion

            #region Itr
            var i = 0;
            do
            {
                i++;
                var upAxis = _camera.transform.up;
                _bp = wbPosition - upAxis * (float)(bY);
                _fp = wfPosition - upAxis * (float)(fY);
                _lookCenter = _bp * (float)flPfb + _fp * (float)blPfb;
                var cameraFwd = Quaternion.AngleAxis(-yaw, upAxis) * (_bp - _fp).normalized;
                _calcTarget.transform.position = _lookCenter -
                    (float)(cl) * cameraFwd;
                _calcTarget.transform.LookAt(_lookCenter, Quaternion.Euler(0, 0, dutch) * Vector3.up);
            }
            while (i < dutch / itrpPerDutch);

            //var right = Quaternion.AngleAxis(90 + yaw, upAxis) * (wbPosition - wfPosition) + upAxis * (float)(fY - bY);
            //fwd = Quaternion.AngleAxis(-yaw, upAxis) * (wbPosition - upAxis * bY) - (wfPosition - upAxis * fY)
            //fwd = Quaternion.AngleAxis(-yaw, upAxis) * (wbPosition - wfPosition + upAxis * (fY - bY))
            //right = Quaternion.AngleAxis(90 + yaw, upAxis) * (wbPosition - wfPosition + upAxis * (fY - bY))
            //ca1 = projectDeltaLine = wbPosition - wfPosition
            //ca2 = fY - bY
            //right = Quaternion.AngleAxis(90 + yaw, upAxis) * ca1 + upAxis * ca2
            #endregion
        }

        private bool Valid()
        {
            bool isValid = _calcTarget && !PositionEquals(wfPosition, wbPosition);
            if (!isValid)
            {
                Debug.LogError($"Solver not valid {(_calcTarget == null)} ftPos: {wfPosition} btPos: {wbPosition}");
            }
            return isValid;
        }

        private bool PositionEquals(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < 0.01f;
        }

        double abs(double x)
        {
            return x < 0 ? -x : x;
        }

        double sqrt(double x)
        {
            return Mathf.Sqrt((float)x);
        }

    }
}

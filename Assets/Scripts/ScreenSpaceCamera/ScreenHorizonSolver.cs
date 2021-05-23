using System;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pangu.Tools
{
    [System.Serializable, ExecuteInEditMode]
    public class ScreenHorizonSolver : MonoBehaviour
    {
        public enum LookAtBone
        {
            foot = 0,
            spine = 1,
            waist = 2,
            chest = 3,
            neck = 4,
            head = 5,
        }

        public Transform bTarget;
        public Transform fTarget;
        [Range(0,0.5f)]
        public float bCompositionX = 0.33f;
        [Range(0,0.5f)]
        public float fCompositionX = 0.25f;
        [Range(-90,90)]
        public float yaw = -30;
        [Range(0.1f, 75)]
        public float fov = 30;
        public float aspect;

        public Vector3 btPosition { get { return bTarget.position; } }
        public Vector3 ftPosition { get { return fTarget.position; } }


        [SerializeField] private double focus;
        [SerializeField] private double cl;
        [SerializeField] private Camera _camera;

        //[Header("Calc")]
        private Vector3 _fPos;
        private Vector3 _bPos;
        private Vector3 _lookCenter;
        private float _cAngle;
        private double _fbDistance;
        private double _sinC;
        private double _cosC;
        private double _tanHalfVerticalFov;
        private double _tanHalfHorizonFov;
        private double _bWidthToEdge;
        private double _fWidthToEdge;
        private double _btProjector;
        private double _ftProjector;

        private void Update()
        {
            Calculate();
        }

        public Transform Calculate()
        {
            if (!_camera)
            {
                return null;
            }
            if (!Valid())
            {
                return null;
            }
            CalcConstance();
            CalcClassicCameraPos();
            ApplyCamera();
            return _camera.transform;
        }

        private void CalcConstance()
        {
            aspect = _camera.aspect;
            _fPos = ftPosition;
            _bPos = btPosition;

            _cAngle = Mathf.Abs(yaw);
            _fbDistance = Vector3.Distance(_bPos, _fPos);
            _sinC = Mathf.Sin(Mathf.PI / 180 * _cAngle);
            _cosC = Mathf.Cos(Mathf.PI / 180 * _cAngle);
            _tanHalfVerticalFov = Mathf.Tan(Mathf.PI / 180 * fov / 2);
        }

        private void CalcClassicCameraPos()
        {
            double bCompsition = 1f - bCompositionX * 2;
            double fComposition = 1f - fCompositionX * 2;
            if (bCompsition == 0 || fComposition == 0)
            {
                return;
            }
            if (bCompsition + fComposition == 0)
            {
                return;
            }
            _tanHalfHorizonFov = _tanHalfVerticalFov * aspect;
            //bx/bl/(bx/cx)-(xl/bl)=cx/bl-xl/bl=cl/bl
            double clPbl = _sinC / _tanHalfHorizonFov / bCompsition - _cosC;
            double clPfl = _sinC / _tanHalfHorizonFov / fComposition + _cosC;
            //cl/fl/(cl*(bl+fl)/(fl*bl))=(fl*bl)/(fl*(fl+bl))=bl/(fl+bl)
            focus = clPfl / (clPfl + clPbl);
            if (focus != 0)
            {
                cl = _fbDistance * focus * clPbl;
            }
            _bWidthToEdge = (cl + _fbDistance * focus * _cosC) * _tanHalfHorizonFov;
            _fWidthToEdge = (cl - _fbDistance * (1 - focus) * _cosC) * _tanHalfHorizonFov;
            _btProjector = _fbDistance * focus * _sinC;
            _ftProjector = _fbDistance * (1 - focus) * _sinC;
        }

        private void ApplyCamera()
        {
            _lookCenter = _bPos * (float)(1 - focus) + _fPos * (float)focus;
            _camera.transform.position = _lookCenter + (float)(cl)* (Quaternion.Euler(0, -yaw, 0) * (_fPos - _bPos)).normalized;
            _camera.transform.LookAt(_lookCenter);
            _camera.fieldOfView = fov;
        }

        private bool Valid()
        {
            bool isValid = _camera && !PositionXZEquals(ftPosition, btPosition);
            if (!isValid)
            {
                Debug.LogError("Dialogue Camera Solver not valid " + (_camera == null) + " ftPos: " + ftPosition + " btPos: " + btPosition);
            }
            return isValid;
        }

        private bool PositionXZEquals(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b) < 0.01f;
        }

#if UNITY_EDITOR

        #region Debug

        private void OnDrawGizmosSelected()
        {
            if (!Valid()) { return; }
            DrawTarget();
            DrawCameraLine();
            DrawFrameOnTargetPlane();
        }

        private void DrawFrameOnTargetPlane()
        {
            var cDir = yaw < 0 ? 1 : -1;
            var bCenter = _bPos + cDir * _camera.transform.right * (float)_btProjector;
            var fCenter = _fPos - cDir * _camera.transform.right * (float)_ftProjector;
            var bEdge = bCenter - cDir * _camera.transform.right * (float)_bWidthToEdge;
            var fEdge = fCenter + cDir * _camera.transform.right * (float)_fWidthToEdge;
            Handles.color = Color.yellow;
            Handles.DrawLine(_bPos, bEdge);
            Handles.DrawLine(_fPos, fEdge);
            Handles.DrawLine(_camera.transform.position, bEdge);
            Handles.DrawLine(_camera.transform.position, fEdge);
            Handles.color = Color.green;
            Handles.DrawLine(_bPos, bCenter);
            Handles.DrawLine(_fPos, fCenter);
            Handles.DrawLine(_bPos, _camera.transform.position);
            Handles.DrawLine(_fPos, _camera.transform.position);
            Handles.color = Color.white;
        }

        private void DrawTarget()
        {
            var cDir = yaw < 0 ? 1 : -1;
            var bCenter = _bPos + cDir * _camera.transform.right * (float)_btProjector;
            var fCenter = _fPos - cDir * _camera.transform.right * (float)_ftProjector;
            Handles.Label(bCenter, "B'");
            Handles.Label(fCenter, "F'");
            Handles.Label(ftPosition, "F");
            Handles.Label(btPosition, "B");
            Handles.Label(_camera.transform.position, "C");
            Handles.Label(_lookCenter, "L");
            Handles.DrawLine(_fPos - (_fPos - _bPos) * 2, _bPos + (_fPos - _bPos) * 2);
        }

        private void DrawCameraLine()
        {
            Handles.color = Color.red;
            var p = _lookCenter * 2 - _camera.transform.position;
            Handles.DrawLine(_camera.transform.position, p);
            Handles.color = Color.white;
        }

        #endregion

#endif

    }
}

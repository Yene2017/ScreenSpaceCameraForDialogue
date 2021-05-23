#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Pangu.Tools
{
    public partial class ScreenSpaceCameraSolver
    {
        int drawMode = 0;

        private void DebugInfo()
        {
            var size = 0.02f;
            Gizmos.DrawSphere(_lookCenter, size);
            Gizmos.DrawSphere(_bp, size);
            Gizmos.DrawSphere(_fp, size);
        }

        private void OnDrawGizmos()
        {
            if (!Valid()) { return; }
            drawMode = 1;
            DrawTarget();
            DrawResultPoint(_camera, wbPosition, Color.red, out var bvp);
            DrawResultPoint(_camera, wfPosition, Color.green, out var fvp);
            DrawLine(bvp, fvp);
            DebugInfo();
        }

        private void DrawResultPoint(Camera camera, Vector3 position, Color color, out Vector3 nearPos)
        {
            var vp = camera.WorldToViewportPoint(position);
            var cp = camera.transform.position;
            var depth = vp.z;
            var interval = camera.nearClipPlane / depth;
            SetColor(color);
            #region NearPlane
            nearPos = Vector3.Lerp(cp, position, interval);
            var nearCenter = cp + camera.transform.forward * camera.nearClipPlane;
            #endregion
            #region PosPlane
            var posCenter = cp + camera.transform.forward * depth;
            var ppp = posCenter + Vector3.ProjectOnPlane(position - posCenter, camera.transform.up);
            DrawLine(posCenter, cp);
            DrawLine(posCenter, ppp);
            DrawLine(position, ppp);
            #endregion
            SetColor(Color.white);
            Handles.Label(_bp, "B");
            Handles.Label(_fp, "F");
            Handles.Label(wbPosition, "WB");
            Handles.Label(wfPosition, "WF");
            Handles.Label(_lookCenter, "L");
            Handles.Label(cp, "C");
            Handles.Label((ppp + posCenter) / 2, $"{Mathf.Abs(vp.x * 2 - 1):F2}");
            Handles.Label((ppp + position) / 2, $"{(vp.y - 0.5f):F2}");
            DrawLine(nearCenter, cp);
            DrawLine(wbPosition, wbPosition + _fp - _bp);
        }

        private void DrawTarget()
        {
            DrawLine(wfPosition, wbPosition);
            DrawLine(_bp, _fp);
            var size = 80f;
            var sv = SceneView.currentDrawingSceneView;
            var camera = _camera;
            if (sv)
            {
                camera = sv.camera;
            }
            Handles.BeginGUI();
            var fsp = camera.WorldToScreenPoint(wfPosition);
            var bsp = camera.WorldToScreenPoint(wbPosition);
            var bSize = size / bsp.z;
            var fSize = bSize / (float)_overSacle;
            EditorGUI.DrawRect(new Rect(fsp.x - fSize, camera.pixelHeight - fsp.y - 2 * fSize,
                fSize * 2, fSize * 2), Color.blue * 0.4f);
            EditorGUI.DrawRect(new Rect(bsp.x - bSize, camera.pixelHeight - bsp.y - 2 * bSize,
                bSize * 2, bSize * 2), Color.blue * 0.4f);
            Handles.EndGUI();
        }

        private void DrawLine(Vector3 p1, Vector3 p2)
        {
            switch (drawMode)
            {
                case 0:
                    Handles.DrawLine(p1, p2);
                    break;
                case 1:
                    Gizmos.DrawLine(p1, p2);
                    break;
            }
        }

        private void SetColor(Color color, float alpha = 1)
        {
            color.a = alpha;
            Gizmos.color = color;
            Handles.color = color;
        }

    }
}

#endif

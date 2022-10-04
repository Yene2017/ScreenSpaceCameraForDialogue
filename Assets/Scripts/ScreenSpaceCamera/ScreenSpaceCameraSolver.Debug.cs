#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CameraSolver
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
            DrawLine(wfPosition, wbPosition);
            DrawLine(_bp, _fp);
            DrawTargetRect();
            DrawResultPoint(_camera, wbPosition, _bp, Color.red, out var bvp);
            DrawResultPoint(_camera, wfPosition, _fp, Color.green, out var fvp);
            DrawLine(bvp, fvp);
            SetColor(Color.white);
            Handles.Label(_bp, "B");
            Handles.Label(_fp, "F");
            Handles.Label(wbPosition, "WB");
            Handles.Label(wfPosition, "WF");
            Handles.Label(_lookCenter, "L");
            DrawLine(wbPosition, wbPosition + _fp - _bp);
            DebugInfo();
        }

        private void DrawResultPoint(Camera camera, Vector3 worldPos, Vector3 projectPos, Color color, out Vector3 nearPos)
        {
            var vp = camera.WorldToViewportPoint(worldPos);
            var cp = camera.transform.position;
            var depth = vp.z;
            var interval = camera.nearClipPlane / depth;
            SetColor(color);
            #region NearPlane
            nearPos = Vector3.Lerp(cp, worldPos, interval);
            var nearCenter = cp + camera.transform.forward * camera.nearClipPlane;
            #endregion
            #region PosPlane
            var centerOnDepth = cp + camera.transform.forward * depth;
            var ppp = centerOnDepth + Vector3.ProjectOnPlane(worldPos - centerOnDepth, camera.transform.up);
            DrawLine(centerOnDepth, cp);
            DrawLine(centerOnDepth, ppp);
            DrawLine(worldPos, ppp);
            #endregion
            #region vertical
            var a = Vector3.ProjectOnPlane(centerOnDepth - _lookCenter, Vector3.up);
            var b = Vector3.ProjectOnPlane(centerOnDepth - projectPos, Vector3.up);
            DrawLine(_lookCenter, _lookCenter + a);
            DrawLine(projectPos, projectPos + b);
            DrawLine(centerOnDepth, _lookCenter + a);
            DrawLine(centerOnDepth, projectPos + b);
            var angleB = Mathf.Acos(Vector3.Dot(b.normalized, (centerOnDepth - projectPos).normalized)) * Mathf.Rad2Deg;
            Handles.Label((projectPos + projectPos + b + centerOnDepth) / 3, $"{angleB:F1}°");
            #endregion
            SetColor(Color.white);
            Handles.Label(cp, "C");
            Handles.Label((ppp + centerOnDepth) / 2, $"{Mathf.Abs(vp.x * 2 - 1):F2}");
            Handles.Label((ppp + worldPos) / 2, $"{(vp.y - 0.5f):F2}");
            DrawLine(nearCenter, cp);
        }

        private void DrawTargetRect()
        {
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
            var fSize = bSize / (float)_overScale;
            EditorGUI.DrawRect(new Rect(fsp.x - fSize, camera.pixelHeight - fsp.y - 2 * fSize,
                fSize * 2, fSize * 2), Color.blue * 0.2f);
            EditorGUI.DrawRect(new Rect(bsp.x - bSize, camera.pixelHeight - bsp.y - 2 * bSize,
                bSize * 2, bSize * 2), Color.blue * 0.2f);
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

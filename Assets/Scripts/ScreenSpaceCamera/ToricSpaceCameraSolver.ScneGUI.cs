#if UNITY_EDITOR
#endif

using UnityEditor;
using UnityEngine;

namespace CameraSolver
{
    public partial class ToricSpaceCameraSolver
    {
        private void OnScneGUI(SceneView sv)
        {
            if (!debug) return;
            if (!IsValid()) return;
            var angle = 360 - toric.circleRadian * Mathf.Rad2Deg;
            foreach (var arc in arcs)
            {
                Handles.DrawWireArc(arc.center, arc.axisUp, A.WPos - arc.center, angle, toric.radius);
            }

            Handles.color = Color.red;
            Handles.DrawWireArc(calcStruct.arcCenter, calcStruct.axisUp, A.WPos - calcStruct.arcCenter, calcStruct.hAngle, toric.radius, 4);

            Handles.color = Color.green;
            Handles.SphereHandleCap(0, calcStruct.point3d, Quaternion.identity, 0.1f, EventType.Repaint);
            Handles.DrawLine(calcStruct.point3d, A.WPos);
            Handles.DrawLine(calcStruct.point3d, B.WPos);
            Handles.DrawWireArc(toric.center, toric.AB,
                calcStruct.point2d - toric.center, pitch, (calcStruct.point2d - toric.center).magnitude);

            Handles.color = Color.white;


            if (!img) return;
            Handles.BeginGUI();
            var r = new Rect(0, 0, 360, 360);
            r.size = new Vector2(img.width, img.height);
            var texR = r;
            EditorGUI.DrawTextureTransparent(texR, img);
            var e = Event.current;
            r.y += img.height;
            if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
            {
                var mouse = e.mousePosition;
                if (texR.Contains(mouse))
                {
                    var y = 178 - mouse.y;
                    yaw = (int)(mouse.x) + 1;
                    pitch = (int)(y) - 89;
                    texPos.x = (int)(mouse.x);
                    texPos.y = (int)(y);
                    UpdateCamera();
                    e.Use();
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.BeginArea(r);
            GUILayout.Button("Dutch Map");
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Button("X");
                texPos.x = EditorGUILayout.IntField(texPos.x);
                GUILayout.Button("Yaw");
                EditorGUILayout.IntField(texPos.x + 1);
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Button("Y");
                texPos.y = EditorGUILayout.IntField(texPos.y);
                GUILayout.Button("Pitch");
                EditorGUILayout.IntField(texPos.y - 89);
            }
            GUILayout.EndArea();
            Handles.EndGUI();
        }


        void GeneratePrevData()
        {
            if (!IsValid()) return;
            prs.Clear();
            img = new Texture2D(178, 178);
            for (var x = 0; x < 178; x++)
            {
                for (var y = 0; y < 178; y++)
                {
                    var tyaw = x + 1;
                    var tpitch = y - 89;
                    Calculate(tyaw, tpitch, out _, out var rot);
                    var rAxis = rot * Vector3.right; rAxis.Normalize();
                    var projV = rAxis;
                    projV.y = 0;
                    var angle = Vector3.Angle(rAxis, projV);
                    var h = angle / 90;
                    h = 1 - h;
                    if (rAxis.y < 0) angle *= -1;
                    var color = new Color(h, h, h);
                    img.SetPixel(x, y, color);

                    prs.Add(new PR()
                    {
                        axis = new Vector2Int(tyaw, tpitch),
                        dutch = angle
                    });
                }
            }
            img.Apply();

            arcs = new ArcPlane[toric.splitCount];
            var splitAngle = 360 / toric.splitCount;
            for (var i = 0; i < toric.splitCount; i++)
            {
                var arc = new ArcPlane();
                arc.axisUp = Quaternion.AngleAxis(i * splitAngle, toric.AB) * toric.zeroAxisUp;
                arc.axisRight = Vector3.Cross(arc.axisUp, toric.AB).normalized;
                arc.center = toric.center + arc.axisRight * toric.halfFB / toric.circleHalfTan;
                arcs[i] = arc;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Policy.UI
{
    /// <summary>
    /// Procedural rounded rectangle — no sprites needed.
    /// Draws a filled quad with rounded corners via vertex mesh.
    /// Drop this on any UI GameObject instead of Image.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class RoundedImage : Graphic
    {
        [Range(0f, 200f)] public float cornerRadius = 18f;
        [Range(4, 32)]    public int   cornerSegments = 8;
        public bool outline;
        public Color outlineColor = new(0.87f, 0.85f, 0.82f, 1f);
        [Range(0f, 8f)] public float outlineWidth = 1f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect  = GetPixelAdjustedRect();
            float r   = Mathf.Min(cornerRadius, Mathf.Min(rect.width, rect.height) * 0.5f);
            int   segs = cornerSegments;

            // Build corner centres
            Vector2[] centres = {
                new(rect.xMin + r, rect.yMin + r), // BL
                new(rect.xMax - r, rect.yMin + r), // BR
                new(rect.xMax - r, rect.yMax - r), // TR
                new(rect.xMin + r, rect.yMax - r), // TL
            };
            float[] startAngles = { 180f, 270f, 0f, 90f };

            // Centre vertex
            AddVert(vh, rect.center, color);
            int vertIdx = 1;

            for (int c = 0; c < 4; c++)
            {
                for (int s = 0; s <= segs; s++)
                {
                    float angle = (startAngles[c] + s * 90f / segs) * Mathf.Deg2Rad;
                    var   p     = centres[c] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
                    AddVert(vh, p, color);
                    if (s > 0)
                        vh.AddTriangle(0, vertIdx - 1, vertIdx);
                    vertIdx++;
                }
            }
            // Close the fan
            vh.AddTriangle(0, vertIdx - 1, 1);

            if (outline && outlineWidth > 0)
                DrawOutline(vh, centres, startAngles, r, segs, vertIdx);
        }

        private static void AddVert(VertexHelper vh, Vector2 pos, Color col)
        {
            var uiv = UIVertex.simpleVert;
            uiv.position = pos;
            uiv.color    = col;
            vh.AddVert(uiv);
        }

        private void DrawOutline(VertexHelper vh, Vector2[] centres, float[] startAngles, float r, int segs, int baseIdx)
        {
            int start = baseIdx;
            for (int c = 0; c < 4; c++)
            {
                for (int s = 0; s <= segs; s++)
                {
                    float angle  = (startAngles[c] + s * 90f / segs) * Mathf.Deg2Rad;
                    var   pOuter = centres[c] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
                    var   pInner = centres[c] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (r - outlineWidth);
                    AddVert(vh, pOuter, outlineColor);
                    AddVert(vh, pInner, outlineColor);
                }
            }

            int count = (segs + 1) * 4;
            for (int i = 0; i < count - 1; i++)
            {
                int a = start + i * 2;
                int b = start + i * 2 + 1;
                int c2 = start + (i + 1) * 2;
                int d = start + (i + 1) * 2 + 1;
                vh.AddTriangle(a, b, c2);
                vh.AddTriangle(b, d, c2);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}

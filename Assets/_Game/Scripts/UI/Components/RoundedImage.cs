using UnityEngine;
using UnityEngine.UI;

namespace Policy.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RoundedImage : Graphic
    {
        [Range(0f, 200f)] public float cornerRadius   = 18f;
        [Range(3, 24)]    public int   cornerSegments = 8;
        public bool  outline;
        public Color outlineColor = new(0.87f, 0.85f, 0.82f, 1f);
        [Range(0f, 8f)] public float outlineWidth = 1f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect  rect = GetPixelAdjustedRect();
            float r    = Mathf.Min(cornerRadius, Mathf.Min(rect.width, rect.height) * 0.499f);
            int   segs = Mathf.Max(3, cornerSegments);

            Vector2[] cc = {
                new(rect.xMin + r, rect.yMin + r),
                new(rect.xMax - r, rect.yMin + r),
                new(rect.xMax - r, rect.yMax - r),
                new(rect.xMin + r, rect.yMax - r),
            };
            float[] startDeg = { 180f, 270f, 0f, 90f };

            // ── filled body ──
            // vertex 0 = centre
            AddVert(vh, rect.center, color);
            int vi = 1;
            for (int c = 0; c < 4; c++)
            {
                for (int s = 0; s <= segs; s++)
                {
                    float a = (startDeg[c] + s * 90f / segs) * Mathf.Deg2Rad;
                    AddVert(vh, cc[c] + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, color);
                    if (vi > 1) vh.AddTriangle(0, vi - 1, vi);
                    vi++;
                }
            }
            vh.AddTriangle(0, vi - 1, 1); // close

            if (!outline || outlineWidth <= 0f) return;

            // ── outline ring ──
            // Build two rings: outer (r) and inner (r - outlineWidth)
            int ringBase = vi; // first outline vertex
            float rI = Mathf.Max(0f, r - outlineWidth);

            for (int c = 0; c < 4; c++)
            {
                for (int s = 0; s <= segs; s++)
                {
                    float a = (startDeg[c] + s * 90f / segs) * Mathf.Deg2Rad;
                    var   dir2 = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    AddVert(vh, cc[c] + dir2 * r,  outlineColor); // outer
                    AddVert(vh, cc[c] + dir2 * rI, outlineColor); // inner
                    vi += 2;
                }
            }

            // Connect quads around the ring
            int steps = (segs + 1) * 4;
            for (int i = 0; i < steps - 1; i++)
            {
                int o0 = ringBase + i * 2;
                int i0 = o0 + 1;
                int o1 = o0 + 2;
                int i1 = o0 + 3;
                vh.AddTriangle(o0, i0, o1);
                vh.AddTriangle(i0, i1, o1);
            }
            // Close the ring
            int last   = ringBase + (steps - 1) * 2;
            vh.AddTriangle(last, last + 1, ringBase);
            vh.AddTriangle(last + 1, ringBase + 1, ringBase);
        }

        private static void AddVert(VertexHelper vh, Vector2 pos, Color col)
        {
            var v = UIVertex.simpleVert;
            v.position = new Vector3(pos.x, pos.y, 0f);
            v.color    = col;
            vh.AddVert(v);
        }

#if UNITY_EDITOR
        protected override void OnValidate() { base.OnValidate(); SetVerticesDirty(); }
#endif
    }
}

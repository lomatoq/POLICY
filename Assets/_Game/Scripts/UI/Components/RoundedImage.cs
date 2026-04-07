using UnityEngine;
using UnityEngine.UI;

namespace Policy.UI
{
    /// <summary>
    /// Procedural rounded rectangle — no sprites needed.
    /// Draws a filled rounded rect via corner fan triangles.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class RoundedImage : Graphic
    {
        [Range(0f, 200f)] public float cornerRadius   = 18f;
        [Range(3, 24)]    public int   cornerSegments = 8;

        // Kept for serialization compatibility but not used for rendering
        public bool  outline;
        public Color outlineColor = new(0.87f, 0.85f, 0.82f, 1f);
        [Range(0f, 8f)] public float outlineWidth = 1f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect  rect = GetPixelAdjustedRect();
            float r    = Mathf.Min(cornerRadius,
                             Mathf.Min(rect.width, rect.height) * 0.499f);
            int   segs = Mathf.Max(3, cornerSegments);

            // Centre vertex (index 0)
            AddVert(vh, rect.center);

            // Corner arc centres
            Vector2[] cc = {
                new(rect.xMin + r, rect.yMin + r), // BL
                new(rect.xMax - r, rect.yMin + r), // BR
                new(rect.xMax - r, rect.yMax - r), // TR
                new(rect.xMin + r, rect.yMax - r), // TL
            };
            float[] startDeg = { 180f, 270f, 0f, 90f };

            int vi = 1; // next vertex index

            for (int c = 0; c < 4; c++)
            {
                for (int s = 0; s <= segs; s++)
                {
                    float a = (startDeg[c] + s * 90f / segs) * Mathf.Deg2Rad;
                    AddVert(vh, cc[c] + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r);
                    if (vi > 1)
                        vh.AddTriangle(0, vi - 1, vi);
                    vi++;
                }
            }
            // Close the last gap back to first perimeter vertex
            vh.AddTriangle(0, vi - 1, 1);
        }

        private void AddVert(VertexHelper vh, Vector2 pos)
        {
            var v = UIVertex.simpleVert;
            v.position = new Vector3(pos.x, pos.y, 0f);
            v.color    = color;
            vh.AddVert(v);
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

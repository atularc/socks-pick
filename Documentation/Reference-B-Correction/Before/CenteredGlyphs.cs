using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    // Centre the visible numeral geometry, not the font's advance width / line box.
    // Runs only when the text mesh changes (including recycled map rows).
    [RequireComponent(typeof(Text))]
    public sealed class CenteredGlyphs : BaseMeshEffect
    {
        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || mesh.currentVertCount == 0) return;
            var vertex = UIVertex.simpleVert;
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i);
                min = Vector2.Min(min, vertex.position);
                max = Vector2.Max(max, vertex.position);
            }
            Vector2 shift = graphic.rectTransform.rect.center - (min + max) * .5f;
            for (int i = 0; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i);
                vertex.position += (Vector3)shift;
                mesh.SetUIVertex(vertex, i);
            }
        }
    }
}

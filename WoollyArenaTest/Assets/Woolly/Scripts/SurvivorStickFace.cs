using UnityEngine;
using UnityEngine.UI;

namespace WoollyArena
{
    // A quiet, flat touch control leaves the arena and spawn warnings readable underneath.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SurvivorStickFace : MaskableGraphic
    {
        public bool knob;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var center = rectTransform.rect.center;
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * (knob ? .47f : .44f);
            Disc(mesh, center, radius, knob ? new Color(.12f, .16f, .11f, .8f) : new Color(.08f, .1f, .08f, .16f));
            Ring(mesh, center, radius, knob ? 2.5f : 2, new Color(.75f, .88f, .62f, knob ? .65f : .25f));
            if (knob) Disc(mesh, center, radius * .26f, new Color(.75f, .88f, .62f, .75f));
        }
        static void Disc(VertexHelper mesh, Vector2 center, float radius, Color color)
        {
            int first = mesh.currentVertCount; mesh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i <= 48; i++)
            {
                float angle = i * Mathf.PI / 24;
                mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, color, Vector2.zero);
                if (i > 0) mesh.AddTriangle(first, first + i, first + i + 1);
            }
        }
        static void Ring(VertexHelper mesh, Vector2 center, float radius, float width, Color color)
        {
            for (int i = 0; i < 48; i++)
            {
                float a = i * Mathf.PI / 24, b = (i + 1) * Mathf.PI / 24; int first = mesh.currentVertCount;
                var x = new Vector2(Mathf.Cos(a), Mathf.Sin(a)); var y = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                mesh.AddVert(center + x * radius, color, Vector2.zero); mesh.AddVert(center + y * radius, color, Vector2.zero);
                mesh.AddVert(center + y * (radius - width), color, Vector2.zero); mesh.AddVert(center + x * (radius - width), color, Vector2.zero);
                mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
            }
        }
    }
}

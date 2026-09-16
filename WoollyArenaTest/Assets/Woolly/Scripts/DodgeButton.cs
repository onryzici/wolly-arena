using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WoollyArena
{
    // Vector UI shares the arena's blue controls and needs no runtime texture allocations.
    public sealed class DodgeButton : MaskableGraphic, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public ArenaPlayer player;
        public TMP_Text label;
        int pointer = int.MinValue;
        float displayedCooldown = -1;
        bool wasReady;

        void Update()
        {
            if (!player || !player.Dodge) return;
            bool ready = player.CanDodge;
            float remaining = player.Dodge.CooldownRemaining;
            if (ready != wasReady || !Mathf.Approximately(remaining, displayedCooldown))
            {
                wasReady = ready;
                displayedCooldown = remaining;
                SetVerticesDirty();
            }
            label.text = remaining > 0 ? remaining.ToString("F1") + "s" : Application.isMobilePlatform ? "KAÇ" : "SPACE";
            label.color = ready ? new Color(.85f, .96f, 1) : new Color(.6f, .72f, .85f);
            transform.localScale = Vector3.one * (pointer != int.MinValue && ready ? .94f : 1);
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .5f;
            var center = rectTransform.rect.center;
            bool ready = player && player.CanDodge;
            Disc(mesh, center, radius, new Color(.055f, .14f, .29f, .94f));
            Disc(mesh, center + Vector2.up * 2, radius - 4, ready ? new Color(.13f, .4f, .68f, .92f) : new Color(.12f, .21f, .34f, .9f));
            float fill = player && player.Dodge ? 1 - player.Dodge.CooldownRemaining / Mathf.Max(.1f, player.Dodge.cooldown) : 1;
            Ring(mesh, center, radius - 2, Mathf.Clamp01(fill), ready ? new Color(.47f, .84f, 1, 1) : new Color(.4f, .6f, .83f, .7f));
            var ink = ready ? new Color(.8f, .94f, 1) : new Color(.36f, .46f, .6f);
            for (int i = 0; i < 2; i++)
            {
                var p = center + new Vector2(-18 + i * 22, 6);
                Quad(mesh, p + new Vector2(-5, 16), p + new Vector2(3, 16), p + new Vector2(17, 0), p + new Vector2(9, 0), ink);
                Quad(mesh, p + new Vector2(9, 0), p + new Vector2(17, 0), p + new Vector2(3, -16), p + new Vector2(-5, -16), ink);
            }
        }

        static void Disc(VertexHelper mesh, Vector2 center, float radius, Color color)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i <= 64; i++)
            {
                float a = i * Mathf.PI * 2 / 64;
                mesh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color, Vector2.zero);
                if (i > 0) mesh.AddTriangle(first, first + i, first + i + 1);
            }
        }

        static void Ring(VertexHelper mesh, Vector2 center, float radius, float fill, Color color)
        {
            for (int i = 0; i < 64; i++)
            {
                float a = Mathf.Min(i / 64f, fill) * Mathf.PI * 2;
                float b = Mathf.Min((i + 1) / 64f, fill) * Mathf.PI * 2;
                if (a >= b) break;
                var x = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
                var y = new Vector2(Mathf.Sin(b), Mathf.Cos(b));
                Quad(mesh, center + x * radius, center + y * radius, center + y * (radius - 3), center + x * (radius - 3), color);
            }
        }

        static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(a, color, Vector2.zero); mesh.AddVert(b, color, Vector2.zero);
            mesh.AddVert(c, color, Vector2.zero); mesh.AddVert(d, color, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || pointer != int.MinValue || !player || !player.RequestDodge()) return;
            pointer = data.pointerId;
        }
        public void OnPointerUp(PointerEventData data) { if (pointer == data.pointerId) pointer = int.MinValue; }
        public void OnPointerExit(PointerEventData data) { if (pointer == data.pointerId) pointer = int.MinValue; }
        protected override void OnDisable() { pointer = int.MinValue; transform.localScale = Vector3.one; base.OnDisable(); }
        void OnApplicationFocus(bool focused) { if (!focused) pointer = int.MinValue; }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WoollyArena
{
    public sealed class LobbyButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        float target = 1;
        bool hovered;
        Button button;
        void Awake() { button = GetComponent<Button>(); }
        void Update()
        {
            float value = button && !button.interactable ? 1 : target;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * value, 1 - Mathf.Exp(-22 * Time.unscaledDeltaTime));
        }
        void OnDisable() { target = 1; hovered = false; transform.localScale = Vector3.one; }
        public void OnPointerEnter(PointerEventData e) { hovered = true; target = 1.035f; }
        public void OnPointerExit(PointerEventData e) { hovered = false; target = 1; }
        public void OnPointerDown(PointerEventData e) { target = .95f; }
        public void OnPointerUp(PointerEventData e) { target = hovered ? 1.035f : 1; }
    }
}

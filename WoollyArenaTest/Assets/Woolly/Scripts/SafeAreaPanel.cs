using UnityEngine;
namespace WoollyArena {
[RequireComponent(typeof(RectTransform))]public sealed class SafeAreaPanel:MonoBehaviour {
 Rect last;Vector2 size;
 void Update(){var safe=Screen.safeArea;var screen=new Vector2(Screen.width,Screen.height);if(safe==last&&size==screen)return;last=safe;size=screen;if(screen.x<=0||screen.y<=0)return;var r=(RectTransform)transform;r.anchorMin=safe.min/screen;r.anchorMax=safe.max/screen;r.offsetMin=r.offsetMax=Vector2.zero;}
}
}

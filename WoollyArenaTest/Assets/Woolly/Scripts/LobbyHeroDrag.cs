using UnityEngine;
using UnityEngine.EventSystems;
namespace WoollyArena {
// Only gestures begun on the character rotate it; menu buttons retain their normal input.
public sealed class LobbyHeroDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler {
 public LobbyScreen lobby;
 int pointer=int.MinValue;
 public void OnPointerDown(PointerEventData e){if(pointer==int.MinValue&&lobby&&lobby.CanRotateHero)pointer=e.pointerId;}
 public void OnDrag(PointerEventData e){
  if(e.pointerId!=pointer||!lobby||!lobby.CanRotateHero)return;
  var rect=(RectTransform)transform;
  if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var current)&&RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position-e.delta,e.pressEventCamera,out var previous))
   lobby.RotateHero(-(current.x-previous.x)/Mathf.Max(1,rect.rect.width)*360);
 }
 public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer)pointer=int.MinValue;}
 public void OnEndDrag(PointerEventData e){OnPointerUp(e);}
 void OnDisable(){pointer=int.MinValue;}
}
}

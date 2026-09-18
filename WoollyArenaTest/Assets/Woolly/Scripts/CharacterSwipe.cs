using UnityEngine;
using UnityEngine.EventSystems;
namespace WoollyArena {
 // Selection swipes change pages; rotation remains exclusive to the main lobby.
 public sealed class CharacterSwipe:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler {
  public LobbyScreen lobby;
  Vector2 start;int pointer=int.MinValue;float horizontal;
  public void OnBeginDrag(PointerEventData e){
   if(pointer!=int.MinValue||!lobby)return;
   pointer=e.pointerId;horizontal=0;
   RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.pressPosition,e.pressEventCamera,out start);
  }
  public void OnDrag(PointerEventData e){
   if(e.pointerId!=pointer||!lobby)return;
   if(RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var current)){
    var delta=current-start;horizontal=Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?delta.x:0;lobby.DragCharacterPage(horizontal);
   }
  }
  public void OnEndDrag(PointerEventData e){if(e.pointerId!=pointer)return;pointer=int.MinValue;if(lobby)lobby.ReleaseCharacterPage(horizontal);horizontal=0;}
  void OnDisable(){pointer=int.MinValue;horizontal=0;if(lobby)lobby.CancelCharacterSwipe();}
 }
}

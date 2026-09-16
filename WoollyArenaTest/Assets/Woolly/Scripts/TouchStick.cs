using UnityEngine;
using UnityEngine.EventSystems;
namespace WoollyArena {
public sealed class TouchStick:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler {
 public bool buttonOnly;public RectTransform knob; public float radius=70,deadZone=.13f;
 public Vector2 Value {get;private set;} public bool Held=>pointer!=int.MinValue;int pointer=int.MinValue;bool pressed;
 public bool ConsumePress(){bool result=pressed;pressed=false;return result;}
 public void OnPointerDown(PointerEventData e){if(Held)return;pointer=e.pointerId;pressed=buttonOnly;OnDrag(e);}
 public void OnDrag(PointerEventData e){if(e.pointerId!=pointer)return;if(buttonOnly){Value=Vector2.zero;knob.anchoredPosition=Vector2.zero;return;}RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform,e.position,e.pressEventCamera,out var local);var v=Vector2.ClampMagnitude(local/radius,1);Value=v.magnitude<deadZone?Vector2.zero:v;knob.anchoredPosition=v*radius;}
 public void OnPointerUp(PointerEventData e){if(e.pointerId==pointer){bool tap=pressed;ResetInput();pressed=tap;}}
 public void ResetInput(){pointer=int.MinValue;pressed=false;Value=Vector2.zero;if(knob)knob.anchoredPosition=Vector2.zero;}
 void OnDisable()=>ResetInput();void OnApplicationFocus(bool focused){if(!focused)ResetInput();}
}
}

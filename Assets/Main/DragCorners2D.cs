using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragCorners2D : MonoBehaviour, IPointerDownHandler {

    public Image markerLT, markerLB, markerRT, markerRB;
    public RectTransform localRect;

    DragCorners2D dragCorners2D;

    Image draggedMarker;

    void Start()
    {
    }

    void Update()
    {
        NudgeDraggedMarker();
    }

    void NudgeDraggedMarker()
    {
        if (draggedMarker == null) return;

        Vector3 currentPosition = draggedMarker.transform.localPosition;

        if (Input.GetKey(KeyCode.UpArrow))
            currentPosition.y += .5f;

        if (Input.GetKey(KeyCode.DownArrow))
            currentPosition.y -= .5f;

        if (Input.GetKey(KeyCode.LeftArrow))
            currentPosition.x -= .5f;

        if (Input.GetKey(KeyCode.RightArrow))
            currentPosition.x += .5f;

        draggedMarker.transform.localPosition = currentPosition;

    }

    public bool isDirty = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(localRect, eventData.position, eventData.pressEventCamera, out localPosition))
        {
            if ( localPosition.x < 0 || localPosition.y > 0 || localPosition.x >= 512 || localPosition.y <= -424 )
            {
                return;
            }

            if (localPosition.x >= 256)
            {
                if (localPosition.y < -212) { draggedMarker = markerRT; }
                else { draggedMarker = markerRB; }
            }
            else
            {
                if (localPosition.y < -212) { draggedMarker = markerLT; }
                else { draggedMarker = markerLB; }
            }

            if (draggedMarker != null)
            {
                draggedMarker.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0.0f);
                isDirty = true;
            }
        }
    }

}

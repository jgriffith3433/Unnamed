using UnityEngine;
using UnityEngine.EventSystems;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Window Resize Handle")]
    public class UIWindowResizeHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("Direction of Resize")]
        public UIWindow.ResizeDirection Direction = UIWindow.ResizeDirection.TOP;
        [Tooltip("Set cursor on MouseOver?")]
        public bool SetCursor = true;
        [Tooltip("Window to resize")]
        public UIWindow myWindow = null;

        private int CursorCallerID = 9043;
        private int CursorLockedPriority = 9001;
        private bool isMouseDown = false;
        private bool isOver = false;
        private UICursor.CursorSelection Cursor
        {
            get {
                if (UICursor.Instance != null)
                {
                    switch (Direction)
                    {
                        case UIWindow.ResizeDirection.TOP:
                        case UIWindow.ResizeDirection.BOTTOM:
                            return UICursor.Instance.ResizeVerticalCursor;
                        case UIWindow.ResizeDirection.LEFT:
                        case UIWindow.ResizeDirection.RIGHT:
                            return UICursor.Instance.ResizeHorizontalCursor;
                        case UIWindow.ResizeDirection.TOP_LEFT:
                        case UIWindow.ResizeDirection.BOTTOM_RIGHT:
                            return UICursor.Instance.ResizeDiagonalTLCursor;
                        case UIWindow.ResizeDirection.TOP_RIGHT:
                        case UIWindow.ResizeDirection.BOTTOM_LEFT:
                            return UICursor.Instance.ResizeDiagonalTRCursor;
                    }
                }
                return new UICursor.CursorSelection();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (myWindow == null)
                return;
            myWindow.BeginResize();
            myWindow.BringToFront();
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (myWindow == null)
                return;
            myWindow.UpdateResize(Direction, eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (myWindow == null)
                return;
            myWindow.EndResize();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isOver = true;
            if (SetCursor)
            {
                UICursor.Set(Cursor, CursorCallerID);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            isOver = false;
            if (!isMouseDown && SetCursor)
            {
                UICursor.Clear(CursorCallerID);
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            isMouseDown = true;
            if (myWindow)
                myWindow.BringToFront();
            if (SetCursor)
            {
                UICursor.Set(Cursor.cursor, CursorCallerID, CursorLockedPriority);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            isMouseDown = false;
            if (SetCursor)
            {
                if (!isOver)
                {
                    UICursor.Clear(CursorCallerID);
                }
                else
                {
                    UICursor.Set(Cursor, CursorCallerID);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.UI
{
    [AddComponentMenu("Procedural Worlds/Storm/UI/Window")]
    public class UIWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public enum ResizeDirection
        {
            TOP = 0x01,
            LEFT = 0x02,
            RIGHT = 0x04,
            BOTTOM = 0x08,
            TOP_LEFT = 0X10 | TOP | LEFT,
            TOP_RIGHT = 0X10 | TOP | RIGHT,
            BOTTOM_LEFT = 0X10 | BOTTOM | LEFT,
            BOTTOM_RIGHT = 0X10 | BOTTOM | RIGHT,
        }
        const string WindowPositionSettingHeader = "STORM_WINDOW_";
        public string WindowName = "";
        public RectTransform TransformToMove = null;
        public bool KeepOnScreen = true;
        public Vector2 MinSize = new Vector2(100, 100);
        public Vector2 MaxSize = new Vector2(9999, 9999);

        private Vector2 beginingMin = Vector2.zero;
        private Vector2 beginingMax = Vector2.zero;
        private Vector3 moveOffset = Vector3.zero;
        private Vector3 startPosition = Vector3.zero;

        private void Start()
        {
            if (TransformToMove == null)
            {
                TransformToMove = transform.parent as RectTransform;
                if (TransformToMove == null)
                    TransformToMove = transform as RectTransform;
            }
            Load();
        }

        public void BringToFront()
        {
            if (TransformToMove != null)
            {
                if (TransformToMove.parent == null || TransformToMove.GetSiblingIndex() == TransformToMove.parent.childCount - 1)
                {
                    // already in front, don't waste time
                    return;
                }
                TransformToMove.SetAsLastSibling();
            }
        }
        public void BeginResize()
        {
            Vector2 Size = (TransformToMove.parent as RectTransform).rect.size;
            beginingMin = new Vector2(TransformToMove.offsetMin.x + Size.x * TransformToMove.anchorMin.x, TransformToMove.offsetMin.y + Size.y * TransformToMove.anchorMin.y);
            beginingMax = new Vector2(TransformToMove.offsetMax.x + Size.x * TransformToMove.anchorMax.x, TransformToMove.offsetMax.y + Size.y * TransformToMove.anchorMax.y);
        }
        public void UpdateResize(ResizeDirection direction, PointerEventData eventData)
        {
            RectTransform restrict = TransformToMove.parent as RectTransform;
            Canvas c = GetCanvasOf(restrict);
            Vector2 restrictSize = restrict.rect.size;
            bool isTop = (direction & ResizeDirection.TOP) == ResizeDirection.TOP;
            bool isBot = (direction & ResizeDirection.BOTTOM) == ResizeDirection.BOTTOM;
            bool isLft = (direction & ResizeDirection.LEFT) == ResizeDirection.LEFT;
            bool isRgt = (direction & ResizeDirection.RIGHT) == ResizeDirection.RIGHT;
            Vector2 dragged = eventData.position - eventData.pressPosition;
            if (c && c.scaleFactor != 0f)
                dragged /= c.scaleFactor;
            Vector2 myMin = beginingMin;
            Vector2 myMax = beginingMax;
            if (isTop)
                myMax.y += dragged.y;
            if (isBot)
                myMin.y += dragged.y;
            if (isRgt)
                myMax.x += dragged.x;
            if (isLft)
                myMin.x += dragged.x;
            if (KeepOnScreen)
            {
                if (isTop && myMax.y >= restrictSize.y)
                {
                    myMax.y = restrictSize.y;
                }
                if (isBot && myMin.y <= 0)
                {
                    myMin.y = 0;
                }
                if (isRgt && myMax.x >= restrictSize.x)
                {
                    myMax.x = restrictSize.x;
                }
                if (isLft && myMin.x <= 0)
                {
                    myMin.x = 0;
                }
            }
            Vector2 newMax = new Vector2(Mathf.Clamp(myMax.x, myMin.x + MinSize.x, myMin.x + MaxSize.x), Mathf.Clamp(myMax.y, myMin.y + MinSize.y, myMin.y + MaxSize.y));
            Vector2 newMin = new Vector2(Mathf.Clamp(myMin.x, myMax.x - MaxSize.x, myMax.x - MinSize.x), Mathf.Clamp(myMin.y, myMax.y - MaxSize.y, myMax.y - MinSize.y));
            if (isTop)
                TransformToMove.offsetMax = new Vector2(TransformToMove.offsetMax.x, newMax.y - restrictSize.y * TransformToMove.anchorMax.y);
            if (isBot)
                TransformToMove.offsetMin = new Vector2(TransformToMove.offsetMin.x, newMin.y - restrictSize.y * TransformToMove.anchorMin.y);
            if (isRgt)
                TransformToMove.offsetMax = new Vector2(newMax.x - restrictSize.x * TransformToMove.anchorMax.x, TransformToMove.offsetMax.y);
            if (isLft)
                TransformToMove.offsetMin = new Vector2(newMin.x - restrictSize.x * TransformToMove.anchorMin.x, TransformToMove.offsetMin.y);
        }
        public void EndResize()
        {
            Save();
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            moveOffset = Vector3.zero;
            if (TransformToMove != null)
            {
                BringToFront();
                startPosition = TransformToMove.anchoredPosition;
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (TransformToMove != null)
            {
                Vector2 dragAmt = eventData.delta;
                Canvas c = GetCanvasOf(TransformToMove);
                if (c && c.scaleFactor != 0f)
                    dragAmt /= c.scaleFactor;
                DoMoveTransform(dragAmt);
            }
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            moveOffset = Vector3.zero;
            Save();
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public void DoMoveTransform(Vector2 moveBy)
        {
            moveOffset.x += moveBy.x;
            moveOffset.y += moveBy.y;
            TransformToMove.anchoredPosition = startPosition + moveOffset;
            Vector2 parentSize = ((RectTransform)TransformToMove.parent as RectTransform).rect.size;
            Vector2 TopRight = new Vector2(TransformToMove.offsetMax.x + parentSize.x * TransformToMove.anchorMax.x, TransformToMove.offsetMax.y + parentSize.y * TransformToMove.anchorMax.y);
            Vector2 BottomLeft = new Vector2(TransformToMove.offsetMin.x + parentSize.x * TransformToMove.anchorMin.x, TransformToMove.offsetMin.y + parentSize.y * TransformToMove.anchorMin.y);

            Vector2 NudgeAmount = Vector3.zero;
            if (KeepOnScreen)
            {
                if (0 > BottomLeft.x)
                {
                    NudgeAmount.x += -BottomLeft.x;
                }
                if (0 > BottomLeft.y)
                {
                    NudgeAmount.y += -BottomLeft.y;
                }
                if (parentSize.x < TopRight.x)
                {
                    NudgeAmount.x += parentSize.x - TopRight.x;
                }
                if (parentSize.y < TopRight.y)
                {
                    NudgeAmount.y += parentSize.y - TopRight.y;
                }
            }
            TransformToMove.anchoredPosition += NudgeAmount;
        }
        public static Canvas GetCanvasOf(Transform transform)
        {
            if (transform == null)
                return null;
            int count = 0;
            do
            {
                ++count;
                Canvas c = transform.GetComponent<Canvas>();
                if (c != null || transform.parent == null)
                    return c;
                transform = transform.parent;
            } while (count < 100);
            return null;
        }

        public void Save()
        {
            if (WindowName == "")
                return;
            //Debug.LogWarning("Save(" + WindowName + "): " + TransformToMove.name);
            if (TransformToMove == null)
                TransformToMove = transform as RectTransform;
            EnsureIsOnScreen();
            string saveStr = PositionToString();
            PlayerPrefs.SetString(WindowPositionSettingHeader + WindowName, saveStr);
            PlayerPrefs.Save();
        }
        public void Load()
        {
            if (WindowName == "")
                return;
            if (TransformToMove == null)
                TransformToMove = transform as RectTransform;

            string Settings = PlayerPrefs.GetString(WindowPositionSettingHeader + WindowName, "");

            if (string.IsNullOrEmpty(Settings))
                return;
            if (!StringToPosition(Settings))
            {
                Debug.LogWarning("Failed to load position from string \"" + Settings + "\".");
                return;
            }
            //enforce maximum/minimum size
            Vector2 winSize = TransformToMove.rect.size;
            if (winSize.x < MinSize.x || winSize.y < MinSize.y || winSize.x > MaxSize.x || winSize.y > MaxSize.y)
            {
                TransformToMove.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(winSize.x, MinSize.x, MaxSize.x));
                TransformToMove.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(winSize.y, MinSize.y, MaxSize.y));
            }
            EnsureIsOnScreen();
        }

        public string PositionToString()
        {
            if (TransformToMove == null)
                return "";
            RectTransform Parent = TransformToMove.parent as RectTransform;
            Vector2 ParentSize = Parent.rect.size;
            float Top = TransformToMove.offsetMax.y + ParentSize.y * TransformToMove.anchorMax.y;
            float Bottom = TransformToMove.offsetMin.y + ParentSize.y * TransformToMove.anchorMin.y;
            float Middle = Bottom + ((Top - Bottom) * 0.5f);
            float Left = TransformToMove.offsetMin.x + ParentSize.x * TransformToMove.anchorMin.x;
            float Right = TransformToMove.offsetMax.x + ParentSize.x * TransformToMove.anchorMax.x;
            float Center = Left + ((Right - Left) * 0.5f);

            float BotOff = Bottom;
            float MidOff = Middle - (ParentSize.y * 0.5f);
            float TopOff = Top - ParentSize.y;
            float BotDist = Mathf.Abs(BotOff);
            float MidDist = Mathf.Abs(MidOff);
            float TopDist = Mathf.Abs(TopOff);

            float LeftOff = Left;
            float CenterOff = Center - (ParentSize.x * 0.5f);
            float RightOff = Right - ParentSize.x;
            float LeftDist = Mathf.Abs(LeftOff);
            float CenterDist = Mathf.Abs(CenterOff);
            float RightDist = Mathf.Abs(RightOff);

            string retStr = "";
            if (MidDist < TopDist && MidDist < BotDist)
                retStr += "M" + MidOff; //middle is Closest
            else if (BotDist < TopDist)
                retStr += "B" + BotOff; //bottom is Closest
            else
                retStr += "T" + TopOff; //Top is Closest

            if (CenterDist < RightDist && CenterDist < LeftDist)
                retStr += ";C" + CenterOff; //middle is Closest
            else if (LeftDist < RightDist)
                retStr += ";L" + LeftOff; //bottom is Closest
            else
                retStr += ";R" + RightOff; //Top is Closest

            return retStr + ";" + (Top - Bottom) + ";" + (Right - Left);
        }
        public bool StringToPosition(string str)
        {
            if (TransformToMove == null)
                return false;
            float XPos;
            float YPos;
            float Height;
            float Width;
            string[] split = str.Split(';');
            if (split.Length != 4)
            {
                Debug.LogWarning("passed string \"" + str + "\" has invalid length. " + split.Length + " parts found (split with ';'), expecting 4.");
                return false;
            }
            if (split[0].Length < 2)
            {
                Debug.LogWarning("YPosition length is too small \"" + split[0] + "\"");
                return false;
            }
            char YType = split[0][0];
            if (YType == 'T' || YType == 'B' || YType == 'M')
            {
                if (!float.TryParse(split[0].Substring(1), out YPos))
                {
                    Debug.LogWarning("YPosition value is not a number '" + split[0].Substring(1) + "'.");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("YPosition type is invalid '" + YType + "'. Expecting 'T', 'M', or 'B'.");
                return false;
            }
            if (split[1].Length < 2)
            {
                Debug.LogWarning("XPosition length is too small \"" + split[1] + "\"");
                return false;
            }
            char XType = split[1][0];
            if (XType == 'L' || XType == 'R' || XType == 'C')
            {
                if (!float.TryParse(split[1].Substring(1), out XPos))
                {
                    Debug.LogWarning("XPosition value is not a number '" + split[1].Substring(1) + "'.");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("XPosition type is invalid '" + XType + "'. Expecting 'L', 'C', or 'R'.");
                return false;
            }
            if (!float.TryParse(split[2], out Height))
            {
                Debug.LogWarning("Height value is not a number '" + split[2] + "'.");
                return false;
            }
            if (!float.TryParse(split[3], out Width))
            {
                Debug.LogWarning("Width value is not a number '" + split[3] + "'.");
                return false;
            }

            Vector2 newPivot = Vector2.zero;
            if (YType == 'T')
            {
                newPivot.y = 1;
            }
            else if (YType == 'M')
            {
                newPivot.y = 0.5f;
            }
            if (XType == 'R')
            {
                newPivot.x = 1;
            }
            else if (XType == 'C')
            {
                newPivot.x = 0.5f;
            }
            TransformToMove.pivot = newPivot;
            TransformToMove.anchorMin = newPivot;
            TransformToMove.anchorMax = newPivot;
            TransformToMove.anchoredPosition = new Vector2(XPos, YPos);
            TransformToMove.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Clamp(Width, MinSize.x, MaxSize.x));
            TransformToMove.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(Height, MinSize.y, MaxSize.y));
            return true;
        }
        void EnsureIsOnScreen()
        {
            if (KeepOnScreen)
            {
                if (TransformToMove.parent == null || TransformToMove == null)
                    return; //this should never really happen
                Vector2 restrictSize = (TransformToMove.parent as RectTransform).rect.size;
                Vector2 myMin = new Vector2(TransformToMove.offsetMin.x + restrictSize.x * TransformToMove.anchorMin.x, TransformToMove.offsetMin.y + restrictSize.y * TransformToMove.anchorMin.y);
                Vector2 myMax = new Vector2(TransformToMove.offsetMax.x + restrictSize.x * TransformToMove.anchorMax.x, TransformToMove.offsetMax.y + restrictSize.y * TransformToMove.anchorMax.y);

                if (myMin.x < 0f)
                {
                    myMax.x -= myMin.x;
                    myMin.x = 0f;
                }
                if (myMin.y < 0)
                {
                    myMax.y -= myMin.y;
                    myMin.y = 0;
                }
                if (myMax.x > restrictSize.x)
                {
                    myMin.x -= myMax.x - restrictSize.x;
                    myMax.x = restrictSize.x;
                }
                if (myMax.y > restrictSize.y)
                {
                    myMin.y += myMax.y - (restrictSize.y);
                    myMax.y = restrictSize.y;
                }
                TransformToMove.offsetMin = new Vector2(myMin.x - restrictSize.x * TransformToMove.anchorMin.x, myMin.y - restrictSize.y * TransformToMove.anchorMin.y);
                TransformToMove.offsetMax = new Vector2(myMax.x - restrictSize.x * TransformToMove.anchorMax.x, myMax.y - restrictSize.y * TransformToMove.anchorMax.y);
            }
        }
    }
}
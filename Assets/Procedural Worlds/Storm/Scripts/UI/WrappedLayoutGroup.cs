using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("Layout/Wrapping Layout Group")]
public class WrappedLayoutGroup : LayoutGroup
{
    #region Types
    class ChildData
    {
        public Vector2 size = Vector2.zero;
        public Vector2 pos = Vector2.zero;
        public int LineNum = 0;
    }
    public enum WrapDir
    {
        Up,
        Down
    }
    #endregion
    #region Variables
    [SerializeField]
    protected Vector2 m_spacing = new Vector2();
    [SerializeField]
    protected WrapDir m_wrapDirection = WrapDir.Down;
    [SerializeField]
    protected bool m_forceChildExpandHeight = false;
    [SerializeField]
    protected bool m_forceChildExpandWidth = false;

    float m_preferredHeight = -1;
    List<ChildData> Children = new List<ChildData>();
    #endregion
    #region Properties
    public override float preferredHeight
    {
        get {
            if (m_preferredHeight < 0)
                return GetTotalPreferredSize(1);
            return m_preferredHeight;
        }
    }
    public Vector2 Spacing
    {
        get {
            return m_spacing;
        }
        set {
            SetProperty(ref m_spacing, value);
        }
    }
    public WrapDir WrapDirection
    {
        get {
            return m_wrapDirection;
        }
        set {
            SetProperty(ref m_wrapDirection, value);
        }
    }
    public bool ForceChildExpandHeight
    {
        get {
            return m_forceChildExpandHeight;
        }
        set {
            SetProperty(ref m_forceChildExpandHeight, value);
        }
    }
    public bool ForceChildExpandWidth
    {
        get {
            return m_forceChildExpandWidth;
        }
        set {
            SetProperty(ref m_forceChildExpandWidth, value);
        }
    }
    public override float flexibleWidth
    {
        get {
            return m_forceChildExpandWidth ? 1f : 0f;
        }
    }
    public override float flexibleHeight
    {
        get {
            return m_forceChildExpandHeight ? 1f : 0f;
        }
    }
    #endregion
    #region Internal Functions
    void CalcChildHorizontalSize()
    {
        float myWidth = (transform as RectTransform).rect.width - padding.horizontal;
        int curLineNum = 0;
        float curLinePos = padding.left;
        List<float> lineEmptySpace = new List<float>();
        //calculate each child's width and set X Position
        foreach (RectTransform child in rectChildren)
        {
            ChildData info = new ChildData();
            info.size.x = Mathf.Max(Mathf.Min(LayoutUtility.GetPreferredWidth(child), myWidth), LayoutUtility.GetMinWidth(child));
            float nextLinePos = curLinePos + info.size.x;
            if (nextLinePos > myWidth)
            {
                lineEmptySpace.Add(myWidth - curLinePos + m_spacing.x);
                curLineNum++;
                curLinePos = padding.left;
                nextLinePos = padding.left + info.size.x;
            }
            nextLinePos += m_spacing.x;
            info.LineNum = curLineNum;
            info.pos.x = curLinePos;
            curLinePos = nextLinePos;
            Children.Add(info);
        }
        lineEmptySpace.Add(myWidth - curLinePos + m_spacing.x);
        if (ForceChildExpandWidth)
        {
            float[] totalFlex = new float[lineEmptySpace.Count];
            int[] totalChildren = new int[lineEmptySpace.Count];
            float[] flexes = new float[rectChildren.Count];
            for (int c = 0; c < Children.Count; ++c)
            {
                float flex = LayoutUtility.GetFlexibleWidth(rectChildren[c]);
                flexes[c] = flex;
                int line = Children[c].LineNum;
                ++totalChildren[line];
                if (flex > 0)
                    totalFlex[line] += flex;
            }
            float[] usedFlex = new float[lineEmptySpace.Count];
            for (int c = 0; c < Children.Count; ++c)
            {
                int line = Children[c].LineNum;
                float flexToAdd = 0f;
                if (totalFlex[line] <= 0f)
                {
                    flexToAdd = lineEmptySpace[line] / (float)totalChildren[line];
                }
                else if (flexes[c] > 0)
                {
                    flexToAdd = lineEmptySpace[line] * flexes[c] / totalFlex[line];
                }
                Children[c].size.x += flexToAdd; //add the extra size
                Children[c].pos.x += usedFlex[line];
                usedFlex[line] += flexToAdd;
            }
        }
        else
        {
            if (m_ChildAlignment == TextAnchor.LowerCenter || m_ChildAlignment == TextAnchor.MiddleCenter || m_ChildAlignment == TextAnchor.UpperCenter)
            {
                foreach (ChildData ci in Children)
                    ci.pos.x += lineEmptySpace[ci.LineNum] * 0.5f;
            }
            else if (m_ChildAlignment == TextAnchor.LowerRight || m_ChildAlignment == TextAnchor.MiddleRight || m_ChildAlignment == TextAnchor.UpperRight)
            {
                foreach (ChildData ci in Children)
                    ci.pos.x += lineEmptySpace[ci.LineNum];
            }
        }
    }
    #endregion
    #region Interface Implementation
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        Children.Clear();
        float myWidth = transform.parent == null ? float.MaxValue : (transform.parent as RectTransform).rect.width - padding.horizontal;
        SetLayoutInputForAxis(0f, myWidth, 9999, 0);
    }
    public override void CalculateLayoutInputVertical()
    {
        //first need to see if we did the LayoutHorizontal yet. if not, need to figure out line numbers. (this happens a lot in the editor)
        if (Children.Count == 0)
            CalcChildHorizontalSize();
        //first need to calc total height of each line
        List<float> lineHeight = new List<float>();
        List<float> linePos = new List<float>();
        List<float> childHeight = new List<float>();
        for (int c = 0; c < rectChildren.Count; ++c)
        {
            RectTransform child = rectChildren[c];
            int lineNum = Children[c].LineNum;
            float cHeight = LayoutUtility.GetPreferredHeight(child);
            while (lineHeight.Count < lineNum + 1)
                lineHeight.Add(0f);
            if (lineHeight[lineNum] < cHeight)
            {
                lineHeight[lineNum] = cHeight;
            }
            childHeight.Add(cHeight);
        }
        float TotalHeight = padding.vertical - Spacing.y;
        foreach (float h in lineHeight)
            TotalHeight += h + Spacing.y;
        bool isDown = m_wrapDirection == WrappedLayoutGroup.WrapDir.Down;
        float WrapDir = isDown ? 1f : -1f;
        while (linePos.Count < lineHeight.Count)
        {
            linePos.Add(linePos.Count == 0 ? (isDown ? padding.top : TotalHeight - lineHeight[0] - padding.bottom) : linePos[linePos.Count - 1] + ((lineHeight[linePos.Count - (isDown ? 1 : 0)] + m_spacing.y) * WrapDir));
        }

        //set each child's Y Position
        for (int c = 0; c < rectChildren.Count; ++c)
        {
            float childPosY = linePos[Children[c].LineNum];
            if (!m_forceChildExpandHeight)
            {
                if (m_ChildAlignment == TextAnchor.MiddleCenter || m_ChildAlignment == TextAnchor.MiddleLeft || m_ChildAlignment == TextAnchor.MiddleRight)
                {
                    childPosY = childPosY + (lineHeight[Children[c].LineNum] - childHeight[c]) * 0.5f;
                }
                else if (m_ChildAlignment == TextAnchor.LowerCenter || m_ChildAlignment == TextAnchor.LowerLeft || m_ChildAlignment == TextAnchor.LowerRight)
                {
                    childPosY = childPosY + (lineHeight[Children[c].LineNum] - childHeight[c]);
                }
            }
            Children[c].pos.y = childPosY;
            Children[c].size.y = m_forceChildExpandHeight ? lineHeight[Children[c].LineNum] : childHeight[c];
        }
        SetLayoutInputForAxis(0f, TotalHeight, -1, 1);
    }
    public override void SetLayoutHorizontal()
    {
        CalcChildHorizontalSize();
        for (int c = 0; c < Children.Count; ++c)
        {
            RectTransform child = rectChildren[c];
            ChildData ci = Children[c];
            SetChildAlongAxis(child, 0, ci.pos.x, ci.size.x);
        }
    }
    public override void SetLayoutVertical()
    {
        for (int c = 0; c < rectChildren.Count; ++c)
        {
            RectTransform child = rectChildren[c];
            ChildData ci = Children[c];
            SetChildAlongAxis(child, 1, ci.pos.y, ci.size.y);
        }
    }
    #endregion
}
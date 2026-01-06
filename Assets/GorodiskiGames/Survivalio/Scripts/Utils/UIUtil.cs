using System;
using  UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

namespace Utilities
{
    public static class UIUtil
    {
        public static Color GetDisabledColor(Color c)
        {
            return new Color(c.r, c.g, c.b, .5f);
        }

        public static Vector2 GetContentSize(RectTransform content, GridLayoutGroup layoutGroup)
        {
            var items = content.childCount;
            var itemsPerRow = layoutGroup.constraintCount;
            var rows = (int)Mathf.Ceil((float)items / itemsPerRow);

            var paddingTop = layoutGroup.padding.top;
            var paddingBottom = layoutGroup.padding.bottom;
            var spacing = layoutGroup.spacing.y;

            var ySlotSize = layoutGroup.cellSize.y;
            var ySize = paddingTop + (rows * ySlotSize) + ((rows - 1) * spacing) + paddingBottom;

            return new Vector2(content.sizeDelta.x, ySize);
        }

        public static Vector2 GetContentSize(RectTransform content, GridLayoutGroup layoutGroup, Constraint constraint, int count)
        {
            var childCount = content.childCount;
            var cellSize = layoutGroup.cellSize;

            var spacing = layoutGroup.spacing;
            var paddingTop = layoutGroup.padding.top;
            var paddingBottom = layoutGroup.padding.bottom;
            var paddingLeft = layoutGroup.padding.left;
            var paddingRight = layoutGroup.padding.right;

            var columns = 0;
            var rows = 0;

            if (constraint == Constraint.FixedColumnCount)
            {
                columns = count;
                rows = Mathf.CeilToInt((float)childCount / columns);
            }
            else if (constraint == Constraint.FixedRowCount)
            {
                rows = count;
                columns = Mathf.CeilToInt((float)childCount / rows);
            }

            var width = columns * cellSize.x + (columns - 1) * spacing.x + paddingLeft + paddingRight;
            var height = rows * cellSize.y + (rows - 1) * spacing.y + paddingTop + paddingBottom;

            return new Vector2(width, height);
        }
    }
}
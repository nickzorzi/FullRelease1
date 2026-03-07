using UnityEngine;

public static class RectTransformExtensions
{
    // Use this method like this: someRectTransform.GetWorldRect();
    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        var localRect = rectTransform.rect;

        return new Rect
        {
            min = rectTransform.TransformPoint(localRect.min),
            max = rectTransform.TransformPoint(localRect.max)
        };
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utils {

    public static Vector2 SwitchToRectTransform(RectTransform from, RectTransform to) {
        Vector2 localPoint;
        Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
        Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
        screenP += fromPivotDerivedOffset;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out localPoint);
        Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
        return to.anchoredPosition + localPoint - pivotDerivedOffset;
    }

    public static T ChangeAlpha<T>(this T g, float newAlpha)
        where T : Graphic {
        var color = g.color;
        color.a = newAlpha;
        g.color = color;
        return g;
    }
    //
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        System.Random rnd = new System.Random();
        while (n > 1) {
            int k = (rnd.Next(0, n) % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
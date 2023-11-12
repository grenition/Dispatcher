using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void MoveItemAtIndexToFront<T>(this List<T> list, int index)
    {
        T item = list[index];
        for (int i = index; i > 0; i--)
            list[i] = list[i - 1];
        list[0] = item;
    }
    public static void Swap<T>(this List<T> list, int index1, int index2)
    {
        T temp = list[index1];
        list[index1] = list[index2];
        list[index2] = temp;
    }
    public static void CycleMoveUp<T>(this List<T> list)
    {
        if (list.Count <= 1)
            return;
        T temp = list[0];
        for(int i = 1; i < list.Count; i++)
            list[i - 1] = list[i];
        list[list.Count - 1] = temp;
    }
    public static void CycleMoveDown<T>(this List<T> list)
    {
        if (list.Count <= 1)
            return;
        T temp = list[list.Count - 1];
        for (int i = list.Count - 1; i > 0; i--)
            list[i] = list[i - 1];
        list[0] = temp;
    }
}

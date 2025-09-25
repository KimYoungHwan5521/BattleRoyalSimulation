using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class CustomExtentions
{
    public static Vector2 Rotate(this Vector2 origin, float degree)
    {
        float angleRadians = degree * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);

        float newX = origin.x * cos - origin.y * sin;
        float newY = origin.x * sin + origin.y * cos;

        return new Vector2(newX, newY);
    }

    public static string GetFileName(this string path)
    {
        if (path == null || path.Length == 0) return "";
        string newPath = "";
        bool findSpace = false;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] != ' ')
            {
                if (findSpace)
                {
                    newPath += char.ToUpper(path[i]);
                    findSpace = false;
                }
                else
                {
                    newPath += path[i];
                }
            }
            else
            {
                findSpace = true;
            }
        }
        return newPath[(path.LastIndexOf("/") + 1)..];
    }

    public static List<T> Shuffle<T>(this List<T> origin)
    {
        List<T> list = new();
        List<T> copyOrigin = origin.ToList();
        int length = origin.Count;
        for(int i=0; i<length; i++)
        {
            int index = Random.Range(0, copyOrigin.Count);
            list.Add(copyOrigin[index]);
            copyOrigin.RemoveAt(index);
        }
        return list;
    }

    public static T[] Shuffle<T>(this T[] origin)
    {
        List<T> list = origin.ToList<T>();
        list = list.Shuffle();
        return list.ToArray();
    }
}
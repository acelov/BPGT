using UnityEngine;


public static class VectorUtils
{
    public static Vector3 SnapZ(this Vector3 v)
    {
        return new Vector3(v.x, v.y, 0);
    }

    public static Vector3 SnapZCamera(this Vector3 v)
    {
        return new Vector3(v.x, v.y, -10);
    }

    public static Vector2Int SnapRow(this Vector2Int v)
    {
        if (v.y % 2 == 0)
        {
            v.x -= 1;
        }

        return v;
    }

    private static Vector2Int GetNext(this Vector2Int v, Vector2Int dir)
    {
        if (dir.y != 0)
        {
            if (v.y % 2 == 0 && dir.x < 0)
            {
                dir.x = 0;
            }
            else if (v.y % 2 == 1 && dir.x > 0)
            {
                dir.x = 0;
            }
        }

        return v + dir;
    }

    public static Vector2Int GetNext(this Vector2Int v, int x, int y)
    {
        return v.GetNext(new Vector2Int(x, y));
    }

    public static Vector2Int ToV2Int(this Vector2 v)
    {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public static Vector2Int ToV2Int(this Vector3 v)
    {
        return new Vector2Int((int)v.x, (int)v.y);
    }
}
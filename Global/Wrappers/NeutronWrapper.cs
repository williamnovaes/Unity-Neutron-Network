using UnityEngine;

public static class NeutronWrapper
{
    public static SerializableVector3 ToVector3(this Vector3 refVector3)
    {
        return new SerializableVector3(refVector3.x, refVector3.y, refVector3.z);
    }

    public static Vector3 ToVector3(this SerializableVector3 refVector3)
    {
        return new Vector3(refVector3.x, refVector3.y, refVector3.z);
    }

    public static Vector2 ToVector2(this SerializableVector3 refVector2)
    {
        return new Vector2(refVector2.x, refVector2.y);
    }
}
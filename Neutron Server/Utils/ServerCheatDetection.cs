using UnityEngine;

public class ServerCheatDetection
{
    public static bool enabled = false;
    public static bool AntiTeleport(Vector3 oldPosition, Vector3 newPosition, float tolerance)
    {
        if (Mathf.Abs(Vector3.Distance(oldPosition, newPosition)) > tolerance)
        {
            return true;
        }
        return false;
    }

    public static bool AntiSpeedHack(float callTime, float tolerance)
    {
        if (callTime > tolerance)
        {
            return true;
        }
        else return false;
    }
}

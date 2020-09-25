using UnityEngine;

/// <summary>
/// Note: Do not modify this class, create your custom validation class and call Validation.
/// </summary>
public static class CustomServerValidation
{
    public static void ServerMovementValidation(NeutronReader paramsReader, Player mSocket)
    {
        if (!CheatDetection.enabled) CheatDetection.enabled = true;
        if (CheatDetection.enabled)
        {
            Vector3 newPosition = paramsReader.ReadVector3();
            Quaternion newRotation = paramsReader.ReadQuaternion();
            //Vector3 newVelocity = paramsReader.ReadVector3();
            //Vector3 newAngularVelocity = paramsReader.ReadVector3();
            //===========================================================\\
            PlayerState statePlayer = mSocket.GetStateObject();
            if (statePlayer != null)
            {
                statePlayer.lastPosition = newPosition;
                statePlayer.lastRotation = newRotation;
                statePlayer.mFrequency++;
            }
        }
    }
}

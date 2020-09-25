using UnityEngine;

public class ProcessEvents : NeutronBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 90;
    }

    private void Update()
    {
        Neutron.Dequeue(ref NeutronConstants.monoBehaviourActions, 5);
        Neutron.Dequeue(ref NeutronConstants.monoBehaviourRPCActions, 5);
    }

    private void OnApplicationQuit()
    {
        Neutron.Disconnect();
    }

    public override void OnFailed(Packet packet, System.String errorMessage)
    {
        Debug.LogError(packet + ":-> " + errorMessage);
    }

    public override void OnDisconnected(string reason)
    {
        Neutron.Disconnect();

        Debug.Log("You Have Disconnected from server -> [" + reason + "]");
    }
}

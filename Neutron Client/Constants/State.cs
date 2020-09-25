using System;
using System.Net.Sockets;

[Serializable]
public class NavMeshResyncProps
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
}
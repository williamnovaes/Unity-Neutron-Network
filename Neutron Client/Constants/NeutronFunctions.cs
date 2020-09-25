using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;

public class NeutronFunctions : NeutronConstants
{
    public static void Logger(object message)
    {
        NeutronServerFunctions.Logger(message);
    }

    public static void LoggerError(object message)
    {
        NeutronServerFunctions.LoggerError(message);
    }

    protected static void ResponseRPC(int executeID, byte[] mArray)
    {
        object[] _array = mArray.DeserializeObject<object[]>();
        int propertyID = (int)_array[0];
        object[] parameters = (object[])_array[1];
        //=====================================================================================================================//
        NeutronObject obj = neutronObjects[propertyID];
        //=====================================================================================================================//
        RPCBehaviour[] scriptComponents = obj.GetComponentsInChildren<RPCBehaviour>();
        //=====================================================================================================================//
        for (int i = 0; i < scriptComponents.Length; i++)
        {
            RPCBehaviour mInstance = scriptComponents[i];
            MethodInfo Invoker = mInstance.HasRPC(executeID, out string message);
            if (Invoker != null)
            {
                Invoker.Invoke(mInstance, new object[] { new NeutronReader((byte[])parameters[0]) });
                break;
            }
            else if (message != string.Empty) LoggerError(message);
        }
    }

    protected static void Send(byte[] buffer, ProtocolType protocolType = ProtocolType.Tcp)
    {
        buffer = buffer.Compress(COMPRESSION_MODE);
        switch (protocolType)
        {
            case ProtocolType.Tcp:
                SendTCP(buffer);
                break;
            case ProtocolType.Udp:
                SendUDP(buffer);
                break;
        }
    }

    protected static void SendTCP(byte[] buffer)
    {
        try
        {
            NetworkStream networkStream = _TCPSocket.GetStream();
            //======================================================\\
            networkStream.BeginWrite(buffer, 0, buffer.Length, EndWrite, networkStream);
        }
        catch (Exception ex) { LoggerError(ex.Message); }
    }

    static void EndWrite(IAsyncResult e)
    {
        NetworkStream endStream = (NetworkStream)e.AsyncState;
        //======================================================\\
        endStream.EndWrite(e);
    }

    protected static void SendUDP(byte[] message)
    {
        try
        {
            _UDPSocket.BeginSend(message, message.Length, _IEPSend, (e) =>
            {
                int data = ((UdpClient)(e.AsyncState)).EndSend(e);
                if (data > 0)
                { }
            }, _UDPSocket);
        }
        catch (Exception ex) { LoggerError(ex.Message); }
    }

    //============================================================================================================//
    protected static bool InitConnect()
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.Connected);
            Send(writer.GetBuffer());
        }
        return true;
    }
    protected static void SendRPC(NeutronObject mThis, int RPCID, ValidationPacket validationType, object[] parameters, SendTo sendTo, bool cached, ProtocolType protocolType, Broadcast broadcast)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            object[] bArray = { mThis.Infor.ownerID, parameters };
            //==========================================================================================================//
            writer.WritePacket(Packet.RPC);
            writer.WritePacket(broadcast);
            writer.Write(RPCID);
            writer.WritePacket(sendTo);
            writer.WritePacket(validationType);
            writer.Write(cached);
            writer.Write(bArray.Serialize());
            //==========================================================================================================//
            Send(writer.GetBuffer(), protocolType);
        }
    }
    //============================================================================================================//
    protected static void HandleConnected(string status, int uniqueID)
    {
        Logger(status);
        //=============================================
        Neutron.myPlayer = new Player(uniqueID, null);
        //=============================================
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onNeutronConnected, new object[] { true }), ref monoBehaviourActions);
    }
    protected static void HandleDisconnect(string reason)
    {
        Neutron.Fire(Neutron.onNeutronDisconnected, new object[] { reason });
    }
    protected static void HandleSendChat(string message, byte[] sender)
    {
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onMessageReceived, new object[] { message, sender.DeserializeObject<Player>() }), ref monoBehaviourActions);
    }
    protected static void HandleInstantiate(Vector3 pos, Quaternion rot, string playerPrefab, byte[] mPlayer)
    {
        //================================================================================================================================\\
        Player playerInstantiated = mPlayer.DeserializeObject<Player>();
        //================================================================================================================================\\
        Neutron.Enqueue(() =>
        {
            GameObject playerPref = Resources.Load(playerPrefab, typeof(GameObject)) as GameObject;
            if (playerPref != null)
            {
                Neutron.onPlayerInstantiated(playerInstantiated, pos, rot, playerPref);
            }
            else LoggerError($"CLIENT: -> Unable to load prefab {playerPrefab}");
        }, ref monoBehaviourActions);
    }
    protected static void HandleSendInput(byte[] mInput)
    {
        SerializableInput nInput = mInput.DeserializeObject<SerializableInput>();
        //===================================================================
        SerializableVector3 nVelocity = nInput.Vector;
        //===================================================================
        Vector3 velocity = new Vector3(nVelocity.x, nVelocity.y, nVelocity.z);
        //===================================================================
        //Neutron.Enqueue(() => playerRB.velocity = velocity);
    }
    protected static void HandleRPC(int id, byte[] parameters)
    {
        Neutron.Enqueue(() => ResponseRPC(id, parameters), ref monoBehaviourRPCActions);
    }
    protected static void HandleDatabase(Packet packet, object[] response)
    {
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onDatabasePacket, new object[] { packet, response }), ref monoBehaviourActions);
    }
    protected static void HandleGetChannels(byte[] mChannels)
    {
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onChannelsReceived, new object[] { mChannels.DeserializeObject<Channel[]>() }), ref monoBehaviourActions);
    }
    protected static void HandleJoinChannel(int ID, byte[] Player)
    {
        Player playerJoined = Player.DeserializeObject<Player>();
        //===============================================================================================================================
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onPlayerJoinedChannel, new object[] { playerJoined, ID }), ref monoBehaviourActions);
    }
    protected static void HandleCreateRoom(Room room, NeutronReader options)
    {
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onCreatedRoom, new object[] { room, options }), ref monoBehaviourActions);
    }
    protected static void HandleGetRooms(Room[] room, NeutronReader[] options)
    {
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onRoomsReceived, new object[] { room, options }), ref monoBehaviourActions);
    }
    protected static void HandleFail(Packet packet, string error)
    {
        Neutron.Fire(Neutron.onFailed, new object[] { packet, error });
    }
    protected static void HandleJoinRoom(int roomID, byte[] player)
    {
        Player playerJoined = player.DeserializeObject<Player>();
        //=================================================================================================================================
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onPlayerJoinedRoom, new object[] { playerJoined, roomID }), ref monoBehaviourActions);
    }
    protected static void HandleDestroyPlayer()
    {
        Neutron.Enqueue(() => Neutron.Fire(Neutron.onDestroyed, new object[] { }), ref monoBehaviourActions);
    }
    protected static void HandleVoiceChat(int offset, byte[] bufferAud)
    {

    }
    protected static void HandlePlayerDisconnected(byte[] player)
    {
        Player playerDisconnected = player.DeserializeObject<Player>();
        //===================================================================================\\
        NeutronObject obj = neutronObjects[playerDisconnected.ID];
        //===================================================================================\\
        Neutron.Enqueue(() => Destroy(obj.gameObject), ref monoBehaviourActions);
        //===================================================================================\\
        neutronObjects.Remove(obj.Infor.ownerID);
    }
    protected static void HandleNavMeshAgent(int ownerID, Vector3 inputPoint)
    {
        NeutronObject obj = neutronObjects[ownerID];
        if (obj.agent != null)
        {
            Neutron.Enqueue(() => obj.agent.SetDestination(inputPoint), ref monoBehaviourActions);
        }
    }
    protected static void HandleNavMeshResync(int ownerID, Vector3 pos, Vector3 rot)
    {
        NeutronObject obj = neutronObjects[ownerID];
        if (obj.agent != null)
        {
            obj.navMeshResync.position = pos.ToVector3();
            obj.navMeshResync.rotation = rot.ToVector3();
        }
    }
    protected static void HandleJsonProperties(int ownerID, string properties)
    {
        NeutronObject obj = neutronObjects[ownerID];
        //======================================================================\\
        if (obj != null)
        {
            if (obj.myProperties != null) JsonUtility.FromJsonOverwrite(properties, obj.myProperties);
            else Debug.LogError("Unable to find NeutronSyncBehaviour object!");
        }
    }
}

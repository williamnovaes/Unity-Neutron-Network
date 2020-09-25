using System;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class NeutronServerFunctions : NeutronServerValidation
{
    public static NeutronServerFunctions _singleton;
    //=============================================================================================================
    public static event ServerEvents.OnPlayerDisconnected onPlayerDisconnected;
    public static event ServerEvents.OnPlayerInstantiated onPlayerInstantiated;
    public static event ServerEvents.OnPlayerDestroyed onPlayerDestroyed;
    public static event ServerEvents.OnPlayerJoinedChannel onPlayerJoinedChannel;
    public static event ServerEvents.OnPlayerLeaveChannel onPlayerLeaveChannel;
    public static event ServerEvents.OnPlayerJoinedRoom onPlayerJoinedRoom;
    public static event ServerEvents.OnPlayerLeaveRoom onPlayerLeaveRoom;
    public static ServerEvents.OnPlayerPropertiesChanged onChanged;
    public static ServerEvents.OnCheatDetected onCheatDetected;
    //=============================================================================================================
    public void Awake()
    {
        _singleton = this;
        //===============================\\
        Application.targetFrameRate = 90;
    }
    private void Update()
    {
        timeDelta = Time.deltaTime;
        //====================================
        Dequeue(ref monoBehaviourActions, 50);
    }
    public void Dequeue(ref ConcurrentQueue<Action> cQueue, int count)
    {
        try
        {
            for (int i = 0; i < count && cQueue.Count > 0; i++)
            {
                if (cQueue.TryDequeue(out Action action))
                {
                    action.Invoke();
                }
            }
        }
        catch { Logger("Falha ao Inicializar Servidor"); }
    }
    public void Enqueue(Action action, ref ConcurrentQueue<Action> cQueue)
    {
        cQueue.Enqueue(action);
    }
    protected int GetUniqueID(IPEndPoint endPoint)
    {
        return Math.Abs(endPoint.GetHashCode());
    }
    protected bool GetPlayer(TcpClient mSocket, out Player mSender)
    {
        if (tcpPlayers.TryGetValue(mSocket, out Player value))
        {
            mSender = value;
            //===============\\
            return true;
        }
        else
        {
            mSender = value;
            //===============\\
            return false;
        }
    }
    protected bool GetPlayer(IPEndPoint pEndPoint, out Player mSender)
    {
        bool contains = tcpPlayers.Values.Any(x => x.tcpClient.RemoteEndPoint().Equals(pEndPoint));
        //========================================================================================================\\
        if (contains) mSender = tcpPlayers.Values.First(x => x.tcpClient.RemoteEndPoint().Equals(pEndPoint));
        else mSender = new Player();
        //========================================================================================================\\
        return contains;
    }
    protected bool AddPlayer(Player mPlayer)
    {
        return tcpPlayers.TryAdd(mPlayer.tcpClient, mPlayer);
    }
    protected bool RemovePlayer(Player mPlayer)
    {
        if (tcpPlayers.TryRemove(mPlayer.tcpClient, out Player removedPlayer))
        {
            if (playersState.TryRemove(removedPlayer.tcpClient, out PlayerState removedState))
            {
                Enqueue(() =>
                {
                    Destroy(removedState.gameObject);
                }, ref monoBehaviourActions);
            }
            return true;
        }
        else return false;
    }
    protected bool isLoggedin(TcpClient mSocket)
    {
        return IDS.ContainsKey(mSocket);
    }
    protected bool Instantiate(Player mPlayer, Vector3 pos, Quaternion rot, string prefabName)
    {
        GameObject playerPref = Resources.Load(prefabName, typeof(GameObject)) as GameObject;
        if (playerPref != null)
        {
            GameObject obj = Instantiate(playerPref, pos, rot);
            //======================================================================================\\
            obj.GetComponent<Renderer>().material.color = Color.red;
            //======================================================================================\\
            int layerMask = LayerMask.NameToLayer("ServerObject");
            if (layerMask > -1) obj.layer = layerMask;
            else
            {
                LoggerError("\"ServerObject\" layer not exist, create it");
                return false;
            }
            //======================================================================================\\
            obj.name = "[SERVER OBJECT]: " + mPlayer.Nickname;
            //=======================================================================================\\
            obj.AddComponent<PlayerState>();
            //=======================================================================================\\
            PlayerState _nPState = obj.GetComponent<PlayerState>();
            _nPState._Player = mPlayer;
            _nPState.lastPosition = pos;
            _nPState._prefabName = prefabName;
            //=======================================================================================\\
            onPlayerInstantiated(mPlayer);
            //=======================================================================================\\
            return playersState.TryAdd(mPlayer.tcpClient, _nPState);
        }
        else return false;
    }
    public static void SendProperties(Player player, NeutronSyncBehaviour properties, SendTo sendTo, Broadcast broadcast)
    {
        NeutronSyncBehaviour _properties = properties;
        //=======================================================\\
        string props = JsonUtility.ToJson(_properties);
        //=======================================================\\
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.playerProps);
            writer.Write(player.ID);
            writer.Write(props);
            player.Send(sendTo, writer.GetBuffer(), broadcast, null, ProtocolType.Tcp, null, null);
        }
    }
    public static void SendErrorMessage(Player mSocket, Packet packet, string message)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.Fail);
            writer.WritePacket(packet);
            writer.Write(message);
            //========================================================\\
            mSocket.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
        }
    }
    public static void SendDisconnect(Player mSocket, string reason)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.Disconnected);
            writer.Write(reason);
            //======================================================================================\\
            mSocket.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
        }
    }
    private void HandlePlayerDisconnected(Player mSender)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.PlayerDisconnected);
            writer.Write(mSender.Serialize());
            mSender.Send(SendTo.Others, writer.GetBuffer(), Broadcast.Channel, null, ProtocolType.Tcp, null, null);
        }
    }
    /// <summary>
    /// Send the response from client or all clients.
    /// </summary>
    /// <param name="mSocket">TcpClient of player</param>
    /// <param name="buffer">Message stream</param>
    public static IAsyncResult TCP(TcpClient mSocket, SendTo sendTo, byte[] buffer, Player[] ToSend, object state, AsyncCallback endSend)
    {
        switch (sendTo)
        {
            case SendTo.Only:
                return mSocket.BeginWrite(buffer, endSend, state);
            case SendTo.All:
                for (int i = 0; i < ToSend.Length; i++)
                {
                    Player To = ToSend[i];
                    //===============================================//
                    To.tcpClient.BeginWrite(buffer, endSend, state);
                }
                break;
            case SendTo.Others:
                for (int i = 0; i < ToSend.Length; i++)
                {
                    Player To = ToSend[i];

                    if (To.tcpClient == mSocket) continue;

                    To.tcpClient.BeginWrite(buffer, endSend, state);
                }
                break;
            default:
                return null;
        }
        return null;
    }
    public static IAsyncResult UDP(SendTo sendTo, byte[] buffer, Player[] ToSend, IPEndPoint onlyEndPoint, object state, AsyncCallback endSend)
    {
        switch (sendTo)
        {
            case SendTo.Only:
                return _UDPSocket.BeginSend(buffer, buffer.Length, onlyEndPoint, endSend, state);
            case SendTo.All:
                lock (lockerUDPEndPoints)
                {
                    foreach (var _ip in udpEndPoints)
                    {
                        if (ToSend.Any(x => x.tcpClient.RemoteEndPoint().Equals(_ip)))
                        {
                            _UDPSocket.BeginSend(buffer, buffer.Length, _ip, endSend, state);
                        }
                    }
                }
                break;
            case SendTo.Others:
                lock (lockerUDPEndPoints)
                {
                    foreach (var _ip in udpEndPoints)
                    {
                        if (_ip.Equals(onlyEndPoint)) continue;

                        if (ToSend.Any(x => x.tcpClient.RemoteEndPoint().Equals(_ip)))
                        {
                            _UDPSocket.BeginSend(buffer, buffer.Length, _ip, endSend, state);
                        }
                    }
                }
                break;
            default:
                return null;
        }
        return null;
    }
    public static bool IsHeadlessMode()
    {
        return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;
    }
    public static void Logger(object message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#endif
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
        Console.WriteLine(message);
#endif
    }
    public static void LoggerError(object message)
    {
#if UNITY_EDITOR
        Debug.LogError(message);
#endif
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
        Console.WriteLine(message);
#endif
    }
    //============================================================================================================//
    protected void HandleDisconnect(TcpClient mSocket)
    {
        void HandleEndPoint()
        {
            lock (lockerUDPEndPoints)
            {
                udpEndPoints.RemoveAll(x => x.Equals(mSocket.RemoteEndPoint()));
            }
            lock (lockerUDPEndPointsVoices)
            {
                udpEndPointsVoices.RemoveAll(x => x.Address.Equals(mSocket.RemoteEndPoint().Address));
            }
        }

        if (GetPlayer(mSocket, out Player playerHandled))
        {
            if (RemovePlayer(playerHandled))
            {
                IDS.TryRemove(mSocket, out int value);
                //=====================================\\
                HandleEndPoint();
                HandlePlayerDisconnected(playerHandled);
                //=====================================\\
                onPlayerDisconnected(playerHandled);
            }
            mSocket.Close();
        }
    }
    protected void HandleConfirmation(Player mSocket, Packet mCommand)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(mCommand);
            writer.Write("Status: OK");
            writer.Write(mSocket.ID);
            mSocket.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, (ia) =>
            {
                if (mSocket.EndSend(ia))
                { }
            });
        }
    }
    protected void HandleNickname(Player mSender, string Nickname)
    {
        if (tcpPlayers.TryGetValue(mSender.tcpClient, out Player oldValue))
        {
            mSender.Nickname = Nickname;
            if (tcpPlayers.TryUpdate(mSender.tcpClient, mSender, oldValue))
            {
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(Packet.Nickname);
                    //======================================================================================\\
                    mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                }
            }
            else SendErrorMessage(mSender, Packet.Nickname, "ERROR: Failed to Change Nickname");
        }
    }
    protected void HandleSendChat(Player mSender, Packet mCommand, Broadcast broadcast, string message)
    {
        try
        {
            using (NeutronWriter writer = new NeutronWriter())
            {
                Player pSender = mSender;
                writer.WritePacket(mCommand);
                writer.Write(message);
                writer.Write(pSender.Serialize());
                //==============================================================================================\\
                mSender.Send(SendTo.All, writer.GetBuffer(), broadcast, null, ProtocolType.Tcp, null, null);
            }
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleInstantiatePlayer(Player mSender, Packet mCommand, Broadcast broadcast, Vector3 position, Quaternion rotation, string playerPrefab)
    {
        try
        {
            if (mSender.IsInChannel())
            {
                Enqueue(() =>
                {
                    if (Instantiate(mSender, position, rotation, playerPrefab))
                    {
                        using (NeutronWriter writer = new NeutronWriter())
                        {
                            writer.WritePacket(mCommand);
                            writer.Write(position);
                            writer.Write(rotation);
                            writer.Write(playerPrefab);
                            writer.Write(mSender.Serialize());
                            //============================================================================================
                            mSender.Send(SendTo.All, writer.GetBuffer(), broadcast, null, ProtocolType.Tcp, null, null);
                        }
                    }
                    else SendErrorMessage(mSender, Packet.InstantiatePlayer, "SERVER: -> Failed to instantiate Player, unable to load prefab");

                }, ref monoBehaviourActions);
            }
            else SendErrorMessage(mSender, mCommand, "ERROR: You are not on a channel/room.");
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleSendInput(Player mSender, Packet mCommand, byte[] Input)
    {
        try
        {
            if (mSender.IsInChannel())
            {
                Player playerToMove = mSender;
                //============================================================================================\\
                SerializableInput authInput = Input.DeserializeObject<SerializableInput>();
                //============================================================================================\\
                Rigidbody rbPlayer = playersState[playerToMove.tcpClient].rigidBody;
                //============================================================================================\\
                Vector3 newPos = new Vector3(authInput.Horizontal, 0, authInput.Vertical);
                //============================================================================================\\
                Enqueue(() =>
                {
                    rbPlayer.velocity = (newPos * MOVE_SPEED * Time.deltaTime);
                    //============================================================================================\\
                    Vector3 velocity = rbPlayer.velocity;
                    //============================================================================================\\
                    SerializableInput nInput = new SerializableInput(authInput.Horizontal, authInput.Vertical, new SerializableVector3(velocity.x, velocity.y, velocity.z));
                    //============================================================================================\\
                    using (NeutronWriter writer = new NeutronWriter())
                    {
                        writer.WritePacket(mCommand);
                        writer.Write(nInput.Serialize());
                        //============================================================================================\\
                        //SendTCP(mSender.tcpClient, SendTo.Only, writer.GetBuffer());
                    }
                }, ref monoBehaviourActions);
            }
            else SendErrorMessage(mSender, Packet.SendChat, "ERROR: You are not on a channel/room.");
        }
        catch
        {
            Debug.LogError("Corrupted Bytes");
        }
    }
    protected void HandleRPC(Player mSender, Packet mCommand, Broadcast broadcast, int executeID, SendTo sendMode, ValidationPacket vType, bool Cached, byte[] parameters, IPEndPoint onlyEndPoint)
    {
        try
        {
            if (mSender.IsInChannel())
            {
                object[] _array = parameters.DeserializeObject<object[]>();
                object[] mParameters = (object[])_array[1];

                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(mCommand);
                    writer.Write(executeID);
                    writer.Write(parameters);

                    ExecuteValidation(vType, new NeutronReader((byte[])mParameters[0]), mSender);

                    if (onlyEndPoint != null)
                    {
                        mSender.Send(sendMode, writer.GetBuffer(), broadcast, onlyEndPoint, ProtocolType.Udp, null, (ia) =>
                        {
                            _UDPSocket.EndSend(ia);
                        });
                    }
                    else mSender.Send(sendMode, writer.GetBuffer(), broadcast, null, ProtocolType.Tcp, null, (ia) =>
                    {
                        mSender.EndSend(ia);
                    });
                }
            }
            else SendErrorMessage(mSender, Packet.SendChat, "ERROR: You are not on a channel/room.");
        }
        catch
        {
            Debug.LogError("Corrupted Bytes: lenght" + parameters.Length);
        }
    }
    protected void HandleOnCustomPacket(Player mSender, Packet mCommand, Packet packet, byte[] customParams)
    {
        try
        {
            Packet customPacket = packet;
            byte[] customPacketParams = customParams;
            using (NeutronWriter writer = new NeutronWriter())
            {
                writer.WritePacket(mCommand);
                writer.WritePacket(customPacket);
                writer.Write(customPacketParams);

                object[] validationParameters = customPacketParams.DeserializeObject<object[]>();

                //PacketValidation(customPacket, new NeutronReader((byte[])validationParameters[0]), mSender);

                //SendTCP(mSender.tcpClient, SendTo.Only, writer.GetBuffer());
            }
        }
        catch
        {
            Debug.LogError("Corrupted Bytes");
        }
    }
    protected void HandleGetChannels(Player mSender, Packet mCommand)
    {
        try
        {
            if (!mSender.IsInChannel())
            {
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(mCommand);
                    writer.Write(serverChannels.ToArray().Serialize());
                    mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                }
            }
            else SendErrorMessage(mSender, mCommand, "WARNING: You are trying to get channels from within a channel, this function is not necessarily prohibited, you can change the behavior on the server, but it is not recommended to obtain the list of channels within a channel, in order to save bandwidth.");
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleJoinChannel(Player mSender, Packet mCommand, int channel)
    {
        try
        {
            if (!mSender.IsInChannel())
            {
                mSender.currentChannel = channel;
                //=====================================================//
                tcpPlayers[mSender.tcpClient] = mSender;
                //====================================================//
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(mCommand);
                    writer.Write(channel);
                    writer.Write(mSender.Serialize());
                    mSender.Send(SendTo.All, writer.GetBuffer(), Broadcast.Channel, null, ProtocolType.Tcp, null, null);
                }
                onPlayerJoinedChannel(mSender);
            }
            else SendErrorMessage(mSender, mCommand, "ERROR: You are already joined to a channel.");
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleCreateRoom(Player mSender, Packet mCommand, string roomName, int maxPlayers, string Password, bool isVisible, bool JoinOrCreate, byte[] options)
    {
        try
        {
            if (mSender.IsInChannel() && !mSender.IsInRoom())
            {
                Room nRoom = new Room(roomID, roomName, maxPlayers, string.IsNullOrEmpty(Password), isVisible, options);
                //=======================================================================================================
                void CreateRoom()
                {
                    using (NeutronWriter writer = new NeutronWriter())
                    {
                        writer.WritePacket(mCommand);
                        writer.Write(nRoom.Serialize());
                        mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                    }
                    //=========================================================
                    serverChannels[mSender.currentChannel]._rooms.Add(nRoom);
                    //=========================================================
                    Player _player = tcpPlayers[mSender.tcpClient];
                    _player.currentRoom = roomID;
                    tcpPlayers[mSender.tcpClient] = _player;
                    //=========================================================
                    roomID++;
                }
                if (!JoinOrCreate)
                {
                    CreateRoom();
                }
                else
                {
                    if (!serverChannels[mSender.currentChannel]._rooms.Any(x => x.roomName == roomName))
                    {
                        CreateRoom();
                    }
                    else HandleJoinRoom(mSender, Packet.JoinRoom, roomID);
                }
            }
            else SendErrorMessage(mSender, mCommand, "ERROR: You cannot create a room by being inside one. Call LeaveRoom or you not within a channel");
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleGetCached(Player mSender, CachedPacket packetToSendCache)
    {
        List<byte[]> iP = new List<byte[]>();
        //========================================================\\
        if (packetToSendCache == CachedPacket.ResyncInstantiate)
        {
            foreach (var _ps in playersState.Values)
            {
                if (_ps._Player.Equals(mSender)) continue;
                if (_ps._Player.currentChannel == mSender.currentChannel)
                {
                    using (NeutronWriter writer = new NeutronWriter())
                    {
                        writer.Write(_ps.lastPosition);
                        writer.Write(_ps.lastRotation);
                        writer.Write(_ps._prefabName);
                        writer.Write(_ps._Player.Serialize());
                        //========================================================\\
                        iP.Add(writer.GetBuffer());
                    }
                }
                else continue;
            }
            OnEndBuffer();
        }

        void OnEndBuffer()
        {
            if (iP.Count > 0)
            {
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(CachedPacket.ResyncInstantiate);
                    writer.Write(iP.ToArray().Serialize());
                    //=============================================================================\\
                    mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                }
            }
        }
    }
    protected void HandleGetRooms(Player mSender, Packet mCommand)
    {
        try
        {
            if (mSender.IsInChannel() && !mSender.IsInRoom())
            {
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(mCommand);
                    writer.Write(serverChannels[mSender.currentChannel]._rooms.ToArray().Serialize());
                    mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                }
            }
            else SendErrorMessage(mSender, mCommand, "WARNING: You already in channel?/or/You are trying to get rooms from within a room, this function is not necessarily prohibited, you can change the behavior on the server, but it is not recommended to obtain the list of rooms within a room, in order to save bandwidth.");
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleJoinRoom(Player mSender, Packet mCommand, int roomID)
    {
        try
        {
            if (mSender.IsInChannel() && !mSender.IsInRoom())
            {
                onPlayerJoinedRoom(mSender);
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(mCommand);
                    writer.Write(roomID);
                    writer.Write(mSender.Serialize());
                    //================================================================================================\\
                    mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                }
                Player _player = tcpPlayers[mSender.tcpClient];
                _player.currentRoom = roomID;
                tcpPlayers[mSender.tcpClient] = _player;
            }
            else SendErrorMessage(mSender, mCommand, "WARNING: You already in channel?/or/You are trying to get rooms from within a room, this function is not necessarily prohibited, you can change the behavior on the server, but it is not recommended to obtain the list of rooms within a room, in order to save bandwidth.");
        }
        catch { Debug.LogError("Corrupted Bytes"); }
    }
    protected void HandleDestroyPlayer(Player mSender, Packet mCommand)
    {
        if (playersState.TryRemove(mSender.tcpClient, out PlayerState removedState))
        {
            PlayerState obj = removedState;
            //=======================================================\\
            Enqueue(() => Destroy(obj.gameObject), ref monoBehaviourActions);
            //=======================================================\\
            using (NeutronWriter writer = new NeutronWriter())
            {
                writer.WritePacket(mCommand);
                mSender.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
            }
            onPlayerDestroyed(mSender);
        }
        else SendErrorMessage(mSender, mCommand, "ERROR: Player already destroyed!");
    }
    protected void HandleVoiceChat(Player mSender, Packet mCommand, int lastPos, byte[] buffer, IPEndPoint onlyEndPoint)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(mCommand);
            writer.Write(lastPos);
            writer.Write(buffer);
            //SendUDP(SendTo.Others, writer.GetBuffer(), mSender.SendToChannel(), onlyEndPoint);
        }
    }
    protected void HandleNavMeshAgent(Player mSender, Packet mCommand, Vector3 inputPoint)
    {
        Enqueue(() =>
        {
            var nMa = playersState[mSender.tcpClient].navAgent;
            if (nMa.CalculatePath(inputPoint, nMa.path))
            {
                if (nMa.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    nMa.destination = inputPoint;
                    //======================================================================================================
                    using (NeutronWriter writer = new NeutronWriter())
                    {
                        writer.WritePacket(mCommand);
                        writer.Write(mSender.ID);
                        writer.Write(inputPoint);
                        //===================================================================================================
                        mSender.Send(SendTo.All, writer.GetBuffer(), Broadcast.Channel, null, ProtocolType.Tcp, null, null);
                    }
                }
                else SendErrorMessage(mSender, mCommand, "ERROR: Invalid Path. Solution: Clear And Re-Bake IA Path. the scenes between client and server must be identical.");
            }
            else SendErrorMessage(mSender, mCommand, "ERROR: Invalid Path. Solution: Clear And Re-Bake IA Path. the scenes between client and server must be identical.");
        }, ref monoBehaviourActions);
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using _rnd = UnityEngine.Random;

public class Neutron : UDPManager
{
    public static NeutronObject NeutronObject { get; set; }

    public static bool _Connected { get; set; }

    /// <summary>
    /// Returns to the local player's instance.
    /// </summary>
    public static Player myPlayer;
    static string _Nickname;
    /// <summary>
    /// The nickname of the player that will be used to be displayed to other players.
    /// This function must be assigned before the Connect method;
    /// </summary>
    public static string Nickname { get => _Nickname; set => SetNickname(value); }
    //==================================================================================================================================
    public static Events.OnTCPData onTCPData { get; set; }
    /// <summary>
    /// This event is called when your connection to the server is established or fails.
    /// This call cannot perform functions that inherit from MonoBehaviour.
    /// </summary>
    public static Events.OnNeutronConnected onNeutronConnected { get; set; }
    public static Events.OnNeutronDisconnected onNeutronDisconnected { get; set; }
    /// <summary>
    /// This event is called when you receive a message from yourself or other players.
    /// </summary>
    public static Events.OnMessageReceived onMessageReceived { get; set; }
    /// <summary>
    /// This function is called when processing database actions on the server.
    /// </summary>
    public static Events.OnDatabasePacket onDatabasePacket { get; set; }
    public static Events.OnPlayerInstantiated onPlayerInstantiated { get; set; }
    public static Events.OnChannelsReceived onChannelsReceived { get; set; }
    public static Events.OnRoomsReceived onRoomsReceived { get; set; }
    /// <summary>
    /// This event is triggered when you or other players join the channel.
    /// </summary>
    public static Events.OnPlayerJoinedChannel onPlayerJoinedChannel { get; set; }
    public static Events.OnPlayerJoinedRoom onPlayerJoinedRoom { get; set; }
    public static Events.OnCreatedRoom onCreatedRoom { get; set; }
    public static Events.OnFailed onFailed { get; set; }
    public static Events.OnDestroyed onDestroyed { get; set; }
    public static Events.OnNicknameChanged onNicknameChanged { get; set; }
    /// <summary>
    /// This function will trigger the OnPlayerConnected callback.
    /// Callbacks are inherited from NeutronBehaviour.
    /// Do not call this function in Awake.
    /// </summary>
    /// <param name="Async">Indicates whether this function should be performed asynchronously.</param>
    /// <returns></returns>
    public static void Connect(bool Async = false)
    {
        if (!NeutronServerFunctions.IsHeadlessMode())
        {
            if (!_Connected)
            {
                try
                {
                    if (!Async)
                    {
                        _TCPSocket.Connect(_IEPSend);
                        if (InitConnect()) TCPListenThread();
                        StartUDP();
                        _Connected = true;
                    }
                    else
                    {
                        Logger("Connecting.........");
                        _TCPSocket.BeginConnect(_IEPSend.Address, _IEPSend.Port, (e) =>
                        {
                            TcpClient socket = (TcpClient)e.AsyncState;
                            if (socket.Connected)
                            {
                                if (InitConnect()) TCPListenThread();
                                StartUDP();
                                _Connected = true;
                            }
                            else if (!_TCPSocket.Connected)
                            {
                                LoggerError("Enable to connect to the server");
                                onNeutronConnected(false);
                            }
                            socket.EndConnect(e);
                        }, _TCPSocket);
                    }
                }
                catch (SocketException ex)
                {
                    LoggerError($"The connection to the server failed {ex.ErrorCode}");
                    onNeutronConnected(false);
                }
                EventProcessor();
            }
            else LoggerError("Connection Refused!");
        }
        else LoggerError("- Client Mode Disabled");
    }

    static void EventProcessor()
    {
        if (GameObject.FindObjectOfType<ProcessEvents>() == null)
        {
            GameObject eObject = new GameObject("EventProcessor");
            eObject.AddComponent<ProcessEvents>();
            DontDestroyOnLoad(eObject);
        }
    }

    public static void StartUDP()
    {
        Thread _thread = new Thread(new ThreadStart(() =>
        {
            try
            {
                _UDPSocket.BeginReceive(OnUDPReceive, null);
            }
            catch (Exception ex) { LoggerError(ex.Message); }
        }));
        _thread.IsBackground = true;
        _thread.Start();
    }

    static void TCPListenThread()
    {
        Thread _thread = new Thread(new ThreadStart(() =>
        {
            BufferConfig stateObject = new BufferConfig();
            //==========================================
            stateObject.tcpClient = _TCPSocket;
            stateObject.networkStream = stateObject.tcpClient.GetStream();
            //===============================================================================
            stateObject.networkStream.BeginRead(stateObject.buffer, 0, BufferConfig.BUFFER_SIZE, OnResponseReceived, stateObject);
        }));
        _thread.IsBackground = true;
        _thread.Start();
    }

    public static void Dequeue(ref ConcurrentQueue<Action> cQueue, int count)
    {
        for (int i = 0; i < count && cQueue.Count > 0; i++)
        {
            if (cQueue.TryDequeue(out Action action))
            {
                action.Invoke();
            }
        }
    }

    public static void Enqueue(Action action, ref ConcurrentQueue<Action> cQueue)
    {
        cQueue.Enqueue(action);
    }

    public static bool GetNetworkStats(out long Ping, out double PcktLoss, float delay)
    {
        tNetworkStatsDelay += Time.deltaTime;
        if (tNetworkStatsDelay >= delay)
        {
            pingAmount++;
            using (System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping())
            {
                pingSender.PingCompleted += (object sender, PingCompletedEventArgs e) =>
                {
                    if (e.Reply.Status == IPStatus.Success)
                    {
                        ping = e.Reply.RoundtripTime;
                    }
                    else packetLoss += 1;
                };
                pingSender.SendAsync(_IEPSend.Address, null);
            }
            tNetworkStatsDelay = 0;
        }
        Ping = ping;
        PcktLoss = (packetLoss / pingAmount) * 100;

        return true;
    }

    public static bool IsMine(NeutronObject mObj)
    {
        return mObj.Infor.ownerID == myPlayer.ID;
    }

    public static bool IsMine(Player mPlayer)
    {
        return mPlayer.Equals(myPlayer);
    }

    public static void SetMine(Player mPlayer, GameObject playerObject)
    {
        if (mPlayer.Equals(myPlayer))
        {
            if (playerObject.TryGetComponent<NeutronObject>(out NeutronObject obj))
            {
                NeutronObject = obj;
            }
            else NeutronObject = playerObject.transform.root.GetComponent<NeutronObject>();

            if (NeutronObject == null) Debug.LogError("SetMine not found NeutronObject. Try Add");
        }
    }

    public static object Fire(Delegate @delegate, object[] parameters)
    {
        return @delegate.DynamicInvoke(parameters);
    }

    static void OnResponseReceived(IAsyncResult ia)
    {
        try
        {
            BufferConfig stateObject = (BufferConfig)ia.AsyncState;
            //==================================================================\\
            int dataLength = stateObject.networkStream.EndRead(ia);
            //==================================================================\\
            if (!stateObject.tcpClient.IsConnected())
            {
                HandleDisconnect("Server indisponible!");
            }
            else
            {
                stateObject.networkStream.BeginRead(stateObject.buffer, 0, BufferConfig.BUFFER_SIZE, OnResponseReceived, stateObject);
                //======================================================================================================================\\
                if (dataLength > 0)
                {
                    byte[] stateBuffer = stateObject.buffer;
                    //==================================================================================\\
                    byte[] decompressedBuffer = stateBuffer.Decompress(COMPRESSION_MODE, dataLength);
                    //==================================================================================\\
                    byte[] bufferCopy = new byte[decompressedBuffer.Length];
                    //==================================================================================\\
                    Buffer.BlockCopy(decompressedBuffer, 0, bufferCopy, 0, decompressedBuffer.Length);
                    //==================================================================================\\
                    dataLength = bufferCopy.Length;
                    //==================================================================================\\
                    using (NeutronReader mReader = new NeutronReader(bufferCopy))
                    {
                        Packet mCommand = mReader.ReadPacket<Packet>();
                        switch (mCommand)
                        {
                            case Packet.Connected:
                                HandleConnected(mReader.ReadString(), mReader.ReadInt32());
                                break;
                            case Packet.Disconnected:
                                HandleDisconnect(mReader.ReadString());
                                break;
                            case Packet.SendChat:
                                HandleSendChat(mReader.ReadString(), mReader.ReadBytes(dataLength));
                                break;
                            case Packet.InstantiatePlayer:
                                HandleInstantiate(mReader.ReadVector3(), mReader.ReadQuaternion(), mReader.ReadString(), mReader.ReadBytes(dataLength));
                                break;
                            case Packet.SendInput:
                                HandleSendInput(mReader.ReadBytes(dataLength));
                                break;
                            case Packet.RPC:
                                HandleRPC(mReader.ReadInt32(), mReader.ReadBytes(dataLength));
                                break;
                            case Packet.Database:
                                Packet dbPacket = mReader.ReadPacket<Packet>();
                                object[] dbResponse = mReader.ReadBytes(dataLength).DeserializeObject<object[]>();
                                //=================================================================================//
                                HandleDatabase(dbPacket, dbResponse);
                                break;
                            case Packet.GetChannels:
                                HandleGetChannels(mReader.ReadBytes(dataLength));
                                break;
                            case Packet.JoinChannel:
                                HandleJoinChannel(mReader.ReadInt32(), mReader.ReadBytes(dataLength));
                                break;
                            case Packet.Fail:
                                HandleFail(mReader.ReadPacket<Packet>(), mReader.ReadString());
                                break;
                            case Packet.CreateRoom:
                                Room roomCreated = mReader.ReadBytes(dataLength).DeserializeObject<Room>();
                                HandleCreateRoom(roomCreated, new NeutronReader(roomCreated.options));
                                break;
                            case Packet.GetRooms:
                                Room[] rooms = mReader.ReadBytes(dataLength).DeserializeObject<Room[]>();
                                NeutronReader[] options = rooms.Select(x => new NeutronReader(x.options)).ToArray();
                                HandleGetRooms(rooms, options);
                                break;
                            case Packet.JoinRoom:
                                int roomID = mReader.ReadInt32();
                                byte[] playerJoined = mReader.ReadBytes(dataLength);
                                HandleJoinRoom(roomID, playerJoined);
                                break;
                            case Packet.DestroyPlayer:
                                HandleDestroyPlayer();
                                break;
                            case Packet.PlayerDisconnected:
                                HandlePlayerDisconnected(mReader.ReadBytes(dataLength));
                                break;
                            case Packet.SendMousePosition:
                                HandleNavMeshAgent(mReader.ReadInt32(), mReader.ReadVector3());
                                break;
                            case Packet.navMeshResync:
                                HandleNavMeshResync(mReader.ReadInt32(), mReader.ReadVector3(), mReader.ReadVector3());
                                break;
                            case Packet.playerProps:
                                HandleJsonProperties(mReader.ReadInt32(), mReader.ReadString());
                                break;
                            case Packet.Nickname:
                                onNicknameChanged();
                                break;
                            default:
                                using (NeutronReader mReaderCached = new NeutronReader(bufferCopy))
                                {
                                    CachedPacket cachedCommand = mReaderCached.ReadPacket<CachedPacket>();
                                    switch (cachedCommand)
                                    {
                                        case CachedPacket.ResyncInstantiate:
                                            byte[][] playersInstatiated = mReader.ReadBytes(dataLength).DeserializeObject<byte[][]>();
                                            foreach (var pI in playersInstatiated)
                                            {
                                                using (NeutronReader reader = new NeutronReader(pI))
                                                {
                                                    Vector3 pos = reader.ReadVector3();
                                                    Quaternion rot = reader.ReadQuaternion();
                                                    string pPrefab = reader.ReadString();
                                                    byte[] player = reader.ReadBytes(dataLength);
                                                    //========================================================================\\
                                                    HandleInstantiate(pos, rot, pPrefab, player);
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
        catch (SocketException ex) { LoggerError(ex.Message + ":" + ex.ErrorCode); }
    }

    public static void Disconnect()
    {
        _TCPSocket.Close();
        _UDPSocket.Close();
    }

    public static void SendVoice(byte[] buffer, int lastPos)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.VoiceChat);
            writer.Write(lastPos);
            writer.Write(buffer);
            Send(writer.GetBuffer(), ProtocolType.Udp);
        }
    }

    public static void SendChat(string mMessage, Broadcast broadcast)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.SendChat);
            writer.WritePacket(broadcast);
            writer.Write(mMessage);
            Send(writer.GetBuffer());
        }
    }

    static void SetNickname(string nickname)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.Nickname);
            writer.Write(nickname);
            Send(writer.GetBuffer());
        }
        _Nickname = nickname;
    }

    public static void JoinChannel(int ChannelID)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.JoinChannel);
            writer.Write(ChannelID);
            Send(writer.GetBuffer());
        }
    }

    public static void JoinRoom(int RoomID)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.JoinRoom);
            writer.Write(RoomID);
            Send(writer.GetBuffer());
        }
    }

    public static void CreateRoom(string roomName, int maxPlayers, string password, NeutronWriter options, bool visible = true, bool JoinOrCreate = false)
    {
        byte[] buffer = options != null ? options.GetBuffer() : new byte[] { };
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.CreateRoom);
            writer.Write(roomName);
            writer.Write(maxPlayers);
            writer.Write(password ?? string.Empty);
            writer.Write(visible);
            writer.Write(JoinOrCreate);
            writer.Write(buffer);
            Send(writer.GetBuffer());
        }
    }

    public static void GetChachedPackets(CachedPacket packet)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.GetChached);
            writer.WritePacket(packet);
            Send(writer.GetBuffer());
        }
    }

    public static void GetChannels()
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.GetChannels);
            Send(writer.GetBuffer());
        }
    }

    public static void GetRooms()
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.GetRooms);
            Send(writer.GetBuffer());
        }
    }

    public static void MoveWithMousePointer(Vector3 inputPosition)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.SendMousePosition);
            writer.Write(inputPosition);
            Send(writer.GetBuffer());
        }
    }

    /// <summary>
    /// Creates an instance of this player on the server.
    /// </summary>
    public static void NeutronInstantiate(Vector3 position, Quaternion rotation, GameObject prefabPlayer, Broadcast broadcast)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.InstantiatePlayer);
            writer.WritePacket(broadcast);
            writer.Write(position);
            writer.Write(rotation);
            writer.Write(prefabPlayer.name);
            Send(writer.GetBuffer());
        }
    }

    public static void DestroyPlayer()
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.DestroyPlayer);
            Send(writer.GetBuffer());
        }
    }

    /// <summary>
    /// Creates an instance of this player on the local client.
    /// </summary>
    /// <param name="mPlayer">The Player to be instantiated, returned by the OnPlayerInstantiated callback.</param>
    /// <param name="prefabPlayer">Player prefab to be instantiated</param>
    /// <returns></returns>
    public static GameObject NeutronInstantiate(Player mPlayer, Vector3 position, Quaternion rotation, GameObject prefabPlayer)
    {
        try
        {
            if (prefabPlayer.TryGetComponent(typeof(NeutronObject), out Component Comp))
            {
                GameObject obj = Instantiate(prefabPlayer, position, rotation);
                obj.name = mPlayer.Nickname;
                //=============================================================//
                NeutronObject NETOBJ = obj.GetComponent<NeutronObject>();
                NETOBJ.Infor.ownerID = mPlayer.ID;
                neutronObjects.Add(NETOBJ.Infor.ownerID, NETOBJ);
                //=============================================================//
                return obj;
            }
            LoggerError("The Object does not contain the NeutronObject component, Try add.");
            return null;
        }
        catch
        {
            LoggerError("Failed Instantiate Prefab");
            return null;
        }
    }

    public static void SendInput(float delay, ProtocolType protocolType = ProtocolType.Tcp)
    {
        tInputDelay += Time.deltaTime;
        if (tInputDelay >= delay)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                using (NeutronWriter writer = new NeutronWriter())
                {
                    SerializableInput authoritativeInput = new SerializableInput(horizontal, vertical, new SerializableVector3(0, 0, 0));
                    //================================================================================//
                    writer.WritePacket(Packet.SendInput);
                    writer.Write(authoritativeInput.Serialize());
                    //================================================================================//
                    Send(writer.GetBuffer(), protocolType);
                }
            }
            tInputDelay = 0;
        }
    }

    static bool RPCTimer(int RPCID, float Delay)
    {
        if (timeRPC.ContainsKey(RPCID))
        {
            timeRPC[RPCID] += Time.deltaTime;
            if (timeRPC[RPCID] >= Delay)
            {
                timeRPC[RPCID] = 0;
                return true;
            }
            return false;
        }
        else
        {
            timeRPC.Add(RPCID, 0);
            return true;
        }
    }

    public static void RPC(NeutronObject mThis, int RPCID, ValidationPacket validationType, float secondsDelay, NeutronWriter parametersStream, SendTo sendTo, bool cached, Broadcast broadcast, ProtocolType protocolType = ProtocolType.Tcp)
    {
        if (RPCTimer(RPCID, secondsDelay))
        {
            SendRPC(mThis, RPCID, validationType, new object[] { parametersStream.GetBuffer() }, sendTo, cached, protocolType, broadcast);
        }
    }

    public static void SendCustomPacket(object[] parameters, Packet customPacket, ProtocolType protocolType = ProtocolType.Tcp)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.OnCustomPacket);
            writer.WritePacket(customPacket);
            writer.Write(parameters.Serialize());
            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    Send(writer.GetBuffer(), ProtocolType.Tcp);
                    break;
                case ProtocolType.Udp:
                    Send(writer.GetBuffer(), ProtocolType.Udp);
                    break;
            }
        }
    }

    /// <summary>
    /// Notifies you of removing an item from the interface.
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    /// <param name="mParent">Transform parent of interface objects</param>
    /// <param name="mArray">The object that will generate the objects in the interface</param>
    /// <param name="mCacheArray">The list that stores the interface objects generated by mArray</param>
    /// <param name="mDestroy">Indicates whether to remove or destroy the interface object</param>
    /// <returns></returns>
    public static bool? NotifyDestroy<T>(Transform mParent, T[] mArray, ref List<T> mCacheArray, IEqualityComparer<T> comparer, bool mDestroy = true) where T : INotify
    {
        if (mParent.childCount > mArray.Length)
        {
            var exceptList = mCacheArray.Except(mArray, comparer);
            int indexRemove = 0;
            foreach (var _ in exceptList.ToList())
            {
                indexRemove = mCacheArray.RemoveAll(x => x.ID == _.ID);
                foreach (Transform _child in mParent)
                {
                    int ID = int.Parse(new string(_child.name.Where(x => char.IsNumber(x)).ToArray()));
                    if (ID == _.ID)
                    {
                        if (mDestroy) Destroy(_child.gameObject);
                        else _child.gameObject.SetActive(false);
                    }
                }
                //if (onNotify) onNotifyDestroy(_.ID, ((NOTIFY)new System.Diagnostics.StackFrame(1).GetMethod().GetCustomAttributes(typeof(NOTIFY), false).FirstOrDefault()).eventName);
            }
            //===========================================================================================
            return Convert.ToBoolean(indexRemove);
        }
        return null;
    }
    /// <summary>
    /// Indicates whether to remove or destroy the interface object
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    /// <param name="mArray">The list that generated the objects in the interface</param>
    /// <param name="mObject">The expression that will be used to identify whether the objects are the same or not; x => x.ID == ..........</param>
    /// <returns></returns>
    public static bool NotifyExist<T>(List<T> mArray, Func<T, bool> mObject)
    {
        return mArray.Any(mObject);
    }
    public static void NotifyClear<T>(Transform mParent, List<T> mArray, bool destroy = true)
    {
        foreach (Transform _p in mParent)
        {
            if (destroy) Destroy(_p.gameObject);
            else _p.gameObject.SetActive(false);
            mArray.Clear();
        }
    }

    // Database Manager //

    public static void Login(string Username, string Password)
    {
        Send(DBLogin(Username, Password), ProtocolType.Tcp);
    }
}

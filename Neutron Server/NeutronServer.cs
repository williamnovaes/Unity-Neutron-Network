using System.Net.Sockets;
using System;
using System.Threading;
using System.Net;
using UnityEngine;

public class NeutronServer : ServerUDP
{
    private static void CenterText(string text)
    {
        Console.Write(new string(' ', (Console.WindowWidth - text.Length) / 2));
        _singleton.Logger(text);
    }

    void Initilize()
    {
        _TCPSocket.Start(LISTEN_MAX);
        //====================================\\
        TCPListen();
        StartUDP();
        StartUDPVoice();
        //====================================\\
        if (!Application.isEditor)
        {
            for (int i = 0; i < 100; i++)
            {
                Logger("\r\n");
            }
            CenterText("Welcome to Neutron");
        }
        Logger("- TCP Initialized");
        Logger("- UDP Initialized");
    }

    void StartUDP()
    {
        Thread _Thread = new Thread(new ThreadStart(() =>
        {
            _UDPSocket.Client.ReceiveBufferSize = 4096; // maximum size of the DGRAM that can be received.
            _UDPSocket.Client.SendBufferSize = 4096; // maximum size of the DGRAM that can be sended.
            _UDPSocket.BeginReceive(OnUDPReceive, null);
        }));
        _Thread.Start();
    }

    void StartUDPVoice()
    {
        Thread _Thread = new Thread(new ThreadStart(() =>
        {
            _UDPVoiceSocket.Client.ReceiveBufferSize = 4096; // maximum size of the DGRAM that can be received.
            _UDPVoiceSocket.Client.SendBufferSize = 4096; // maximum size of the DGRAM that can be sended.
            _UDPVoiceSocket.BeginReceive(OnUDPVoiceReceive, null);
        }));
        _Thread.Start();
    }

    void TCPListen()
    {
        Thread _Thread = new Thread(new ThreadStart(() =>
        {
            _TCPSocket.BeginAcceptTcpClient(OnClientAccepted, _TCPSocket);
        }));
        _Thread.Start();
    }

    void OnClientAccepted(IAsyncResult ia)
    {
        try
        {
            TcpClient clientAccepted = ((TcpListener)ia.AsyncState).EndAcceptTcpClient(ia);
            //================================================================================\\
            //clientAccepted.NoDelay = true;
            //================================================================================\\
            BufferConfig BufferConfig = new BufferConfig();
            //================================================================================\\
            BufferConfig.tcpClient = clientAccepted;
            BufferConfig.networkStream = BufferConfig.tcpClient.GetStream();
            //================================================================================\\
            Player nPlayer = new Player(GetUniqueID((IPEndPoint)BufferConfig.tcpClient.Client.RemoteEndPoint), BufferConfig.tcpClient);
            if (AddPlayer(nPlayer))
            {
                Logger($"Client Connected -> [{BufferConfig.tcpClient.RemoteEndPoint()}]");
                //=========================================
                BufferConfig.networkStream.BeginRead(BufferConfig.buffer, 0, BufferConfig.BUFFER_SIZE, OnTCPReceived, BufferConfig);
            }
            else Logger("Fail to client accept");
        }
        catch (SocketException ex) { LoggerError($"Failed to Client Accept {ex.Message} {ex.ErrorCode}"); }
        TCPListen(); // Wait for new connenction.
    }

    void OnTCPReceived(IAsyncResult ia)
    {
        BufferConfig BufferConfig = (BufferConfig)ia.AsyncState;
        try
        {
            int dataLength = BufferConfig.networkStream.EndRead(ia);
            //==============================================================================================\\
            if (!BufferConfig.tcpClient.IsConnected())
            {
                HandleDisconnect(BufferConfig.tcpClient); // Disconnects a client and frees up its resources.
            }
            else
            {
                BufferConfig.networkStream.BeginRead(BufferConfig.buffer, 0, BufferConfig.BUFFER_SIZE, OnTCPReceived, BufferConfig);
                //==================================================================================================================\\
                if (dataLength > 0)
                {
                    byte[] stateBuffer = BufferConfig.buffer;
                    //==================================================================================
                    byte[] decompressedBuffer = stateBuffer.Decompress(COMPRESSION_MODE, dataLength);
                    //==================================================================================
                    byte[] bufferCopy = new byte[decompressedBuffer.Length];
                    //==================================================================================
                    Buffer.BlockCopy(decompressedBuffer, 0, bufferCopy, 0, decompressedBuffer.Length);
                    //==================================================================================
                    ProcessClientData(BufferConfig.tcpClient, bufferCopy, bufferCopy.Length, null);
                }
                else HandleDisconnect(BufferConfig.tcpClient);
            }
        }
        catch (SocketException ex)
        {
            HandleDisconnect(BufferConfig.tcpClient);
            //==========================================================//
            LoggerError($"Failed to Receive Message {ex.Message}");
        }
    }

    void ProcessClientData(TcpClient mSocket, byte[] bufferCopy, int length, IPEndPoint endPoint)
    {
        try
        {
            using (NeutronReader mReader = new NeutronReader(bufferCopy))
            {
                Packet mCommand = mReader.ReadPacket<Packet>();
                if (GetPlayer(mSocket, out Player Sender))
                {
                    switch (mCommand)
                    {
                        case Packet.Connected:
                            HandleConfirmation(Sender, mCommand);
                            break;
                        case Packet.Nickname:
                            HandleNickname(Sender, mReader.ReadString());
                            break;
                        case Packet.SendChat:
                            HandleSendChat(Sender, mCommand, mReader.ReadPacket<Broadcast>(), mReader.ReadString());
                            break;
                        case Packet.InstantiatePlayer:
                            HandleInstantiatePlayer(Sender, mCommand, mReader.ReadPacket<Broadcast>(), mReader.ReadVector3(), mReader.ReadQuaternion(), mReader.ReadString());
                            break;
                        case Packet.SendInput:
                            HandleSendInput(Sender, mCommand, mReader.ReadBytes(length));
                            break;
                        case Packet.RPC:
                            HandleRPC(Sender, mCommand, mReader.ReadPacket<Broadcast>(), mReader.ReadInt32(), mReader.ReadPacket<SendTo>(), mReader.ReadPacket<ValidationPacket>(), mReader.ReadBoolean(), mReader.ReadBytes(length), endPoint);
                            break;
                        case Packet.OnCustomPacket:
                            HandleOnCustomPacket(Sender, mCommand, mReader.ReadPacket<Packet>(), mReader.ReadBytes(length));
                            break;
                        case Packet.Database:
                            Packet dbPacket = mReader.ReadPacket<Packet>();
                            switch (dbPacket)
                            {
                                case Packet.Login:
                                    if (!isLoggedin(mSocket))
                                    {
                                        string username = mReader.ReadString();
                                        string passsword = mReader.ReadString();
                                        Enqueue(() => StartCoroutine(Login(Sender, username, passsword)), ref monoBehaviourActions);
                                    }
                                    else SendErrorMessage(Sender, mCommand, "You are already logged in.");
                                    break;
                            }
                            break;
                        case Packet.GetChannels:
                            HandleGetChannels(Sender, mCommand);
                            break;
                        case Packet.JoinChannel:
                            HandleJoinChannel(Sender, mCommand, mReader.ReadInt32());
                            break;
                        case Packet.GetChached:
                            HandleGetCached(Sender, mReader.ReadPacket<CachedPacket>());
                            break;
                        case Packet.CreateRoom:
                            HandleCreateRoom(Sender, mCommand, mReader.ReadString(), mReader.ReadInt32(), mReader.ReadString(), mReader.ReadBoolean(), mReader.ReadBoolean(), mReader.ReadBytes(length));
                            break;
                        case Packet.GetRooms:
                            HandleGetRooms(Sender, mCommand);
                            break;
                        case Packet.JoinRoom:
                            HandleJoinRoom(Sender, mCommand, mReader.ReadInt32());
                            break;
                        case Packet.DestroyPlayer:
                            HandleDestroyPlayer(Sender, mCommand);
                            break;
                        case Packet.SendMousePosition:
                            HandleNavMeshAgent(Sender, mCommand, mReader.ReadVector3());
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggerError($"Failed to Response Client {ex.Message}");
        }
    }

    void Start()
    {
        serverChannels.Add(new Channel(0, "Canal 1", 100));
        serverChannels.Add(new Channel(1, "Canal 2", 100));
        serverChannels.Add(new Channel(2, "Canal 3", 100));
        //===============================================================
        serverChannels[0]._rooms.Add(new Room(1001, "Sala do servidor", 40, false, true, new byte[] { }));
        //===============================================================
        Initilize();
    }

    void DisableLoggger()
    {
#if UNITY_EDITOR
        UnityEngine.Debug.unityLogger.logEnabled = true;
#endif
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
        UnityEngine.Debug.unityLogger.logEnabled = false;
#endif
    }

    private void OnApplicationQuit()
    {
        foreach (var sP in tcpPlayers)
        {
            sP.Value.tcpClient.Close();
        }
        _TCPSocket.Stop();
        _UDPSocket.Close();
    }
}

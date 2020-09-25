using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NeutronConstants : MonoBehaviour
{
    protected const Compression COMPRESSION_MODE = Compression.Deflate; // OBS: Compression.None change to BUFFER_SIZE in StateObject to 4092 or 9192.
    //==============================================================\\
    public static IPEndPoint _IEPRef = new IPEndPoint(IPAddress.Any, 0);
    public static IPEndPoint _IEPListen = new IPEndPoint(IPAddress.Any, new System.Random().Next(0, 1000));
    public static IPEndPoint _IEPSend = new IPEndPoint(/*IPAddress.Parse("145.14.134.106")*/ IPAddress.Loopback, 5055); // IP and Port that the Neutron will use to connect.
    //==============================================================\\
    public static ConcurrentQueue<Action> monoBehaviourActions = new ConcurrentQueue<Action>();
    public static ConcurrentQueue<Action> monoBehaviourRPCActions = new ConcurrentQueue<Action>();
    //==============================================================\\
    protected static TcpClient _TCPSocket = new TcpClient(_IEPListen);
    protected static UdpClient _UDPSocket = new UdpClient(_IEPListen);
    //==============================================================\\
    protected static float tKeepAlive = 0f;
    protected static float tInputDelay = 0f;
    protected static float tNetworkStatsDelay = 0f;
    //==============================================================\\
    public const float navMeshTolerance = 15f;
    //==============================================================\\
    protected static long ping = 0;
    protected static double packetLoss = 0;
    protected static int pingAmount = 0;
    //==============================================================\\
    protected static Dictionary<int, float> timeRPC = new Dictionary<int, float>();
    protected static Dictionary<int, NeutronObject> neutronObjects = new Dictionary<int, NeutronObject>();
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NeutronServerConstants : MonoBehaviour
{
    public const Compression COMPRESSION_MODE = Compression.Deflate; // Level of compression of the bytes.
    //==============================================================\\
    protected static IPEndPoint _IEPRef = new IPEndPoint(IPAddress.Any, 0);
    protected static IPEndPoint _IEPRefVoice = new IPEndPoint(IPAddress.Any, 0);
    private static IPEndPoint _EndPoint = new IPEndPoint(IPAddress.Any, 5055); // Server IP Address and Port. Note: Providers like Amazon, Google, Azure, etc ... require that the ports be released on the VPS firewall and In Server Port Management, servers that have routers, require the same process.
    //==============================================================\\
    protected const int SIO_UDP_CONNRESET = -1744830452;
    protected static byte[] inValue = new byte[] { 0 };
    protected static byte[] outValue = new byte[] { 0 };
    //==============================================================\\
    protected const int LISTEN_MAX = 100; // Maximum size of the acceptance queue for simultaneous clients.
    protected const float MOVE_SPEED = 1200f; // Velocity of Player Move.

    public const float TELEPORT_TOLERANCE = 15f; // Kick player 5M > Teleported.
    public const float SPEEDHACK_TOLERANCE = 10f; // 0.1 = 0.1 x 1000 = 100 -> 1000/100 = 10 pckts per seconds.

    public const string URL_LOGIN = "http://127.0.0.1/Network/login.php";
    //===============================================================\\
    protected static int roomID = 0;
    protected float timeDelta = 0; // Time.DeltaTime from server.
    //=================================================================\\
    public ConcurrentQueue<Action> monoBehaviourActions = new ConcurrentQueue<Action>(); // All shares that inherit from MonoBehaviour must be processed in this space.
    public ConcurrentDictionary<TcpClient, Player> tcpPlayers = new ConcurrentDictionary<TcpClient, Player>(); // List of players who are currently in play.
    public ConcurrentDictionary<TcpClient, PlayerState> playersState = new ConcurrentDictionary<TcpClient, PlayerState>();
    protected ConcurrentDictionary<TcpClient, int> IDS = new ConcurrentDictionary<TcpClient, int>(); // Logged Players Database ID
    //==================================================================================================================================\\
    protected List<Channel> serverChannels = new List<Channel>(); // Server channels. This function must be assigned in the Neutron Start method.
    //==================================================================================================================================\\
    protected static List<IPEndPoint> udpEndPoints = new List<IPEndPoint>();
    protected static List<IPEndPoint> udpEndPointsVoices = new List<IPEndPoint>();
    //==================================================================================================================================\\
    protected static TcpListener _TCPSocket = new TcpListener(_EndPoint);
    protected static UdpClient _UDPSocket = new UdpClient(_EndPoint);
    protected static UdpClient _UDPVoiceSocket = new UdpClient(new IPEndPoint(IPAddress.Any, 5056));
    //=====================================================================\\
    protected static object lockerUDPEndPoints = new object();
    protected static object lockerUDPEndPointsVoices = new object();
    //=====================================================================\\
}

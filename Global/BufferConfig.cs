using System.Net.Sockets;
using UnityEngine;

public class BufferConfig : MonoBehaviour
{
    public TcpClient tcpClient = null;
    public const int BUFFER_SIZE = 8192;
    public byte[] buffer = new byte[BUFFER_SIZE];
    public NetworkStream networkStream;
}

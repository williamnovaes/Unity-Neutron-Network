using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public static class Extesions
{
    public static string Deserialize(this byte[] buffer)
    {
        return Encoding.UTF8.GetString(buffer);
    }
    public static byte[] Serialize(this string message)
    {
        return Encoding.UTF8.GetBytes(message);
    }
    public static byte[] Serialize(this object message)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream mStream = new MemoryStream())
        {
            formatter.Serialize(mStream, message);
            return mStream.GetBuffer();
        }
    }
    public static byte[] Compress(this byte[] data, Compression compressionType)
    {
        if (compressionType == Compression.Deflate)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
                {
                    dstream.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }
        else if (compressionType == Compression.GZip)
        {
            if (data == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs,
                 CompressionMode.Compress), 64 * 1024))
                {
                    gzs.Write(data, 0, data.Length);
                }
                return compressIntoMs.ToArray();
            }
        }
        else return data;
    }
    public static byte[] Decompress(this byte[] data, Compression compressionType, int length)
    {
        if (compressionType == Compression.Deflate)
        {
            using (MemoryStream input = new MemoryStream(data, 0, length))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        dstream.CopyTo(output);
                    }
                    return output.ToArray();
                }
            }
        }
        else if (compressionType == Compression.GZip)
        {
            if (data == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressedMs = new MemoryStream(data))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                     CompressionMode.Decompress), 64 * 1024))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }
        else return data;
    }
    public static T DeserializeObject<T>(this byte[] message)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        try
        {
            using (MemoryStream mStream = new MemoryStream(message))
            {
                T obj = (T)formatter.Deserialize(mStream);
                return obj;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Falha ao deserilizar {ex.Message}");
            return default;
        }
    }
    public static bool IsConnected(this TcpClient socket)
    {
        try
        {
            return !(socket.Client.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (SocketException) { return false; }
    }
    public static IAsyncResult Send(this Player mSender, SendTo sendTo, byte[] buffer, Broadcast broadcast, IPEndPoint endPoint, ProtocolType protocolType, object state, AsyncCallback endSend)
    {
        buffer = buffer.Compress(NeutronServerConstants.COMPRESSION_MODE);
        switch (protocolType)
        {
            case ProtocolType.Tcp:
                return NeutronServerFunctions.TCP(mSender.tcpClient, sendTo, buffer, mSender.SendBroadcast(broadcast), state, endSend);
            case ProtocolType.Udp:
                return NeutronServerFunctions.UDP(sendTo, buffer, mSender.SendBroadcast(broadcast), endPoint, state, endSend);
            default:
                return null;
        }
    }
    public static IAsyncResult BeginWrite(this TcpClient mSocket, byte[] buffer, AsyncCallback asyncCallback, object state)
    {
        NetworkStream stream = mSocket.GetStream();
        //==================================================================
        return stream.BeginWrite(buffer, 0, buffer.Length, asyncCallback, state);
    }
    public static bool EndSend(this TcpClient mSocket, IAsyncResult ia)
    {
        NetworkStream stream = mSocket.GetStream();
        //==================================================================
        stream.EndWrite(ia);
        //==================================================================
        return true;
    }
    public static bool EndSend(this Player mSocket, IAsyncResult ia)
    {
        NetworkStream stream = mSocket.tcpClient.GetStream();
        //==================================================================
        stream.EndWrite(ia);
        //==================================================================
        return true;
    }
    public static bool IsInChannel(this Player _player)
    {
        return _player.currentChannel != -1;
    }
    public static bool IsInRoom(this Player _player)
    {
        return _player.currentRoom != -1;
    }
    public static PlayerState GetStateObject(this Player _player)
    {
        if (NeutronServerFunctions._singleton.playersState.TryGetValue(_player.tcpClient, out PlayerState value))
        {
            return value;
        }
        else return null;
    }
    public static IPEndPoint RemoteEndPoint(this TcpClient socket)
    {
        return (IPEndPoint)socket.Client.RemoteEndPoint;
    }
    private static Player[] SendToRoomAndInstantiated(this Player _player)
    {
        return NeutronServerFunctions._singleton.tcpPlayers.Values.Where(x => x.currentRoom == _player.currentRoom).Where(y => NeutronServerFunctions._singleton.playersState.ContainsKey(y.tcpClient)).ToArray();
    }
    private static Player[] SendToChannel(this Player channelID)
    {
        return NeutronServerFunctions._singleton.tcpPlayers.Values.Where(x => x.currentChannel == channelID.currentChannel).ToArray();
    }
    private static Player[] SendToRoom(this Player roomID)
    {
        return NeutronServerFunctions._singleton.tcpPlayers.Values.Where(x => x.currentRoom == roomID.currentRoom).ToArray();
    }
    private static Player[] SendToServer()
    {
        return NeutronServerFunctions._singleton.tcpPlayers.Values.ToArray();
    }

    public static MethodInfo HasRPC(this RPCBehaviour mThis, int executeID, out string Error)
    {
        MethodInfo[] infor = mThis.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < infor.Length; i++)
        {
            RPC RPC = infor[i].GetCustomAttribute<RPC>();
            if (RPC != null)
            {
                ParameterInfo[] pInfor = infor[i].GetParameters();
                if (pInfor.Length == 1)
                {
                    if (pInfor[0].ParameterType != typeof(NeutronReader))
                    {
                        Error = "RPC Parameters Invalid";
                        return null;
                    }
                    else
                    {
                        if (RPC.ID == executeID)
                        {
                            Error = null;
                            return infor[i];
                        }
                    }
                }
                else
                {
                    Error = "RPC Lenght > 1 or 0";
                    return null;
                }
            }
            else continue;
        }
        Error = string.Empty;
        return null;
    }

    public static TRoom[] Zipped(this Room[] rooms, NeutronReader[] options)
    {
        return rooms.Zip(options, (r, o) => new TRoom(r, o)).ToArray();
    }

    private static Player[] SendBroadcast(this Player mPlayer, Broadcast broadcast)
    {
        switch (broadcast)
        {
            case Broadcast.All:
                return SendToServer();
            case Broadcast.Channel:
                return mPlayer.SendToChannel();
            case Broadcast.Room:
                return mPlayer.SendToRoom();
            case Broadcast.Instantiated:
                return mPlayer.SendToRoomAndInstantiated();
            default:
                return null;
        }
    }
}
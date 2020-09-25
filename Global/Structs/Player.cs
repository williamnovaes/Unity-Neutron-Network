using System;
using System.Collections.Generic;
using System.Net.Sockets;

[Serializable]
public struct Player : IEquatable<Player>, INotify, IEqualityComparer<Player>
{
    public int ID { get; set; }
    public string Nickname;
    public int currentChannel;
    public int currentRoom;
    [NonSerialized] public TcpClient tcpClient;

    public Player(int ID, TcpClient tcpClient)
    {
        this.ID = ID;
        this.tcpClient = tcpClient;
        this.Nickname = null;
        this.currentChannel = -1;
        this.currentRoom = -1;
    }

    public Boolean Equals(Player other)
    {
        return this.ID == other.ID;
    }

    public Boolean Equals(Player x, Player y)
    {
        if (object.ReferenceEquals(x, y))
        {
            return true;
        }
        if (object.ReferenceEquals(x, null) ||
            object.ReferenceEquals(y, null))
        {
            return false;
        }
        return x.ID == y.ID;
    }

    public Int32 GetHashCode(Player obj)
    {
        return obj.ID.GetHashCode();
    }
}
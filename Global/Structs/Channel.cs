using System;
using System.Collections.Generic;

[Serializable]
public struct Channel : IEquatable<Channel>, INotify, IEqualityComparer<Channel>
{
    public int ID { get; set; }
    public string Name;
    public int maxPlayers;
    [NonSerialized] public List<Room> _rooms;

    public Channel(int ID, string Name, int maxPlayers)
    {
        this.ID = ID;
        this.Name = Name;
        this.maxPlayers = maxPlayers;
        this._rooms = new List<Room>();
    }

    public Boolean Equals(Channel other)
    {
        return this.ID == other.ID;
    }

    public Boolean Equals(Channel x, Channel y)
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

    public Int32 GetHashCode(Channel obj)
    {
        return obj.ID.GetHashCode();
    }
}

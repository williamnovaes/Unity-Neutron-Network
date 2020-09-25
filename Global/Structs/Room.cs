using System;
using System.Collections.Generic;

[Serializable]
public struct Room : IEquatable<Room>, INotify, IEqualityComparer<Room>
{
    public int ID { get; set; }
    public string roomName;
    public int maxPlayers;
    public bool hasPassword;
    public bool isVisible;
    public byte[] options;

    public Room(int ID, string roomName, int maxPlayers, bool hasPassword, bool isVisible, byte[] options)
    {
        this.ID = ID;
        this.roomName = roomName;
        this.maxPlayers = maxPlayers;
        this.hasPassword = hasPassword;
        this.isVisible = isVisible;
        this.options = options;
    }

    public Boolean Equals(Room other)
    {
        return this.ID == other.ID;
    }

    public Boolean Equals(Room x, Room y)
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

    public Int32 GetHashCode(Room obj)
    {
        return obj.ID.GetHashCode();
    }
}
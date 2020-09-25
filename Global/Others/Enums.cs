using System;

public enum SendTo
{
    /// <summary>
    /// Broadcast data to all Players, including you.
    /// </summary>
    All,
    /// <summary>
    /// Broadcast data only you.
    /// </summary>
    Only,
    /// <summary>
    /// Broadcast data to all Players, except you.
    /// </summary>
    Others,
}

public enum Packet
{
    Nickname,
    OnCustomPacket,
    KeepAlive,
    Connected,
    Disconnected,
    RPC,
    SendChat,
    SendInput,
    InstantiatePlayer,
    Database,
    Login,
    GetChannels,
    GetChached,
    JoinChannel,
    JoinRoom,
    CreateRoom,
    GetRooms,
    Fail,
    DestroyPlayer,
    VoiceChat,
    PlayerDisconnected,
    SendMousePosition,
    navMeshResync,
    playerProps,
    //======================================================
    // - CUSTOM PACKETS ADD HERE
    //======================================================
}

public enum CachedPacket
{
    /// <summary>
    /// Used to instantiate other players on this client.
    /// </summary>
    ResyncInstantiate,
    //======================================================
    // - CUSTOM PACKETS ADD HERE
    //======================================================
}

public enum ValidationPacket
{
    /// <summary>
    /// Used to validate the player's movement on the server.
    /// </summary>
    Movement,
    None,
    //======================================================
    // - CUSTOM PACKETS ADD HERE
    //======================================================
}

[Flags]
public enum WhenChanging
{
    Position = 1,
    Rotation = 2,
    Velocity = 4,
}

public enum Protocol
{
    /// <summary>
    /// Broadcast data using TCP Protocol.
    /// Recommended for reliable data, for example -> Points, Money, Kills, Death, data that is guaranteed to be delivered without needing to be in real time.
    /// </summary>
    Tcp = 6,
    /// <summary>
    /// Broadcast data using Udp Protocol.
    /// Recommended for unreliable data, example, real-time data -> shots, damage, movement, animation. 
    /// </summary>
    Udp = 17,
}

public enum Compression
{
    /// <summary>
    /// Compress data using deflate mode.
    /// </summary>
    Deflate,
    /// <summary>
    /// Compress data using GZip mode.
    /// </summary>
    GZip,
    /// <summary>
    /// Disable data compression.
    /// </summary>
    None,
}

public enum Broadcast
{
    /// <summary>
    /// Broadcast data on the server.
    /// </summary>
    All,
    /// <summary>
    /// Broadcast data on the channel.
    /// </summary>
    Channel,
    /// <summary>
    /// Broadcast data on the room.
    /// </summary>
    Room,
    /// <summary>
    /// Broadcast data on the room, except for those in the lobby/waiting room.
    /// that is only for players who are instantiated and in the same room.
    /// </summary>
    Instantiated,
    /// <summary>
    /// Broadcast data on the same group.
    /// </summary>
    Group,
    /// <summary>
    /// None broadcast. Used to SendTo.Only.
    /// </summary>
    None,
}
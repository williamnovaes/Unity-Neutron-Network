using UnityEngine;

public class Events
{
    /// <summary>
    /// This event is called when your connection to the server is established or fails.
    /// This call cannot perform functions that inherit from MonoBehaviour.
    /// </summary>
    /// <param name="success"></param>
    public delegate void OnNeutronConnected(bool success);
    public delegate void OnNeutronDisconnected(string reason);
    /// <summary>
    /// This event is called when you receive a message from yourself or other players.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sender"></param>
    public delegate void OnMessageReceived(string message, Player sender);
    public delegate GameObject OnPlayerInstantiated(Player player, Vector3 pos, Quaternion rot, GameObject playerPrefab);
    public delegate void OnTCPData(byte[] buffer);
    public delegate void OnUDPData(byte[] buffer);
    public delegate void OnDatabasePacket(Packet packet, object[] response);
    public delegate void OnChannelsReceived(Channel[] channels);
    public delegate void OnRoomsReceived(Room[] rooms, NeutronReader[] options);
    public delegate void OnNicknameChanged();
    /// <summary>
    /// This event is triggered when you or other players join the channel.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="Channel"></param>
    public delegate void OnPlayerJoinedChannel(Player player, int channel);
    public delegate void OnPlayerJoinedRoom(Player player, int room);
    public delegate void OnCreatedRoom(Room room, NeutronReader options);
    public delegate void OnFailed(Packet packet, string errorMessage);
    /// <summary>
    /// This
    /// </summary>
    public delegate void OnDestroyed();
}

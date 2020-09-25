using UnityEngine;

public abstract class NeutronBehaviour : MonoBehaviour
{
    /// <summary>
    /// This event is called when your connection to the server is established or fails.
    /// This call cannot perform functions that inherit from MonoBehaviour.
    /// </summary>
    /// <param name="success"></param>
    public virtual void OnConnected(bool success) { }
    public virtual void OnDisconnected(string reason) { }
    /// <summary>
    /// This event is called when you receive a message from yourself or other players.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="sender"></param>
    public virtual void OnMessageReceived(string message, Player sender) { }
    public virtual void OnDatabasePacket(Packet packet, object[] response) { }
    public virtual void OnChannelsReceived(Channel[] channels) { }
    public virtual void OnRoomsReceived(Room[] rooms, NeutronReader[] options) { }
    public virtual GameObject OnPlayerInstantiated(Player player, Vector3 pos, Quaternion rot, GameObject playerPrefab) { return null; }
    /// <summary>
    /// This event is triggered when you or other players join the channel.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="Channel"></param>
    public virtual void OnPlayerJoinedChannel(Player player, int channel) { }
    public virtual void OnPlayerJoinedRoom(Player player, int room) { }
    public virtual void OnCreatedRoom(Room room, NeutronReader options) { }
    public virtual void OnFailed(Packet packet, string errorMessage) { }
    public virtual void OnDestroyed() { }
    public virtual void OnNicknameChanged() { }
    public void OnEnable()
    {
        Neutron.onNeutronConnected += OnConnected;
        Neutron.onNeutronDisconnected += OnDisconnected;
        Neutron.onMessageReceived += OnMessageReceived;
        Neutron.onPlayerInstantiated += OnPlayerInstantiated;
        Neutron.onDatabasePacket += OnDatabasePacket;
        Neutron.onChannelsReceived += OnChannelsReceived;
        Neutron.onPlayerJoinedChannel += OnPlayerJoinedChannel;
        Neutron.onFailed += OnFailed;
        Neutron.onCreatedRoom += OnCreatedRoom;
        Neutron.onRoomsReceived += OnRoomsReceived;
        Neutron.onPlayerJoinedRoom += OnPlayerJoinedRoom;
        Neutron.onDestroyed += OnDestroyed;
        Neutron.onNicknameChanged += OnNicknameChanged;
    }

    public void OnDisable()
    {
        Neutron.onNeutronConnected -= OnConnected;
        Neutron.onNeutronDisconnected -= OnDisconnected;
        Neutron.onMessageReceived -= OnMessageReceived;
        Neutron.onPlayerInstantiated -= OnPlayerInstantiated;
        Neutron.onDatabasePacket -= OnDatabasePacket;
        Neutron.onChannelsReceived -= OnChannelsReceived;
        Neutron.onPlayerJoinedChannel -= OnPlayerJoinedChannel;
        Neutron.onFailed -= OnFailed;
        Neutron.onCreatedRoom -= OnCreatedRoom;
        Neutron.onRoomsReceived -= OnRoomsReceived;
        Neutron.onPlayerJoinedRoom -= OnPlayerJoinedRoom;
        Neutron.onDestroyed -= OnDestroyed;
        Neutron.onNicknameChanged -= OnNicknameChanged;
    }
}

using UnityEngine;

public class NeutronSyncBehaviour : MonoBehaviour
{
    private Player _player;
    public Player Player {
        get {
            if (_player.ID == 0)
            {
                _player = GetComponent<PlayerState>()._Player;
                return _player;
            }
            else return _player;
        }
        set {
            _player = value;
        }
    }

    public void Start()
    {
        Player = GetComponent<PlayerState>()._Player;
    }

    protected void OnNotifyChange(NeutronSyncBehaviour syncBehaviour, string propertiesName, Broadcast broadcast)
    {
        NeutronServerFunctions.onChanged(Player, syncBehaviour, propertiesName, broadcast);
    }
}
using UnityEngine;

public class NeutronSyncBehaviour : MonoBehaviour
{
    private PlayerState state;
    private Player _player;

    public void Start()
    {
        state = GetComponent<PlayerState>();
        //==========================================//
        if (state != null) _player = state._Player;
    }

    protected void OnNotifyChange(NeutronSyncBehaviour syncBehaviour, string propertiesName, Broadcast broadcast)
    {
        if (state != null) NeutronServerFunctions.onChanged(_player, syncBehaviour, propertiesName, broadcast);
    }
}
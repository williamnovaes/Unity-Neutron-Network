using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OnCollisionEvents : MonoBehaviour
{
    private PlayerState StatePlayer;

    public static event ServerEvents.OnPlayerCollision onPlayerCollision;
    public static event ServerEvents.OnPlayerTrigger onPlayerTrigger;

    [SerializeField] private string objectIdentifier;

    private void Start()
    {
        if (TryGetComponent(out PlayerState state))
        {
            StatePlayer = state;
        }
        else
        {
            StatePlayer = GetComponentInParent<PlayerState>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (StatePlayer == null) return;
        onPlayerCollision(StatePlayer._Player, collision, objectIdentifier);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (StatePlayer == null) return;
        onPlayerTrigger(StatePlayer._Player, other, objectIdentifier);
    }
}

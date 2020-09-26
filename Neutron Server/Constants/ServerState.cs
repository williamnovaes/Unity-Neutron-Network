using System.Net.Sockets;
using UnityEngine;
using UnityEngine.AI;

public class PlayerState : Config
{
    public NeutronSyncBehaviour neutronSyncBehaviour;
    //===============================================\\
    public Player _Player;

    private void Awake()
    {
        neutronSyncBehaviour = GetComponent<NeutronSyncBehaviour>();
    }

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Controller = GetComponent<CharacterController>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (navAgent == null)
        {
            if (ServerCheatDetection.enabled) OnCheat();
        }
        else if (enableNavResync) OnAgent();
    }

    void OnAgent()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        try
        {
            navResyncTime += Time.deltaTime;
            if (navResyncTime > 2f)
            {
                using (NeutronWriter writer = new NeutronWriter())
                {
                    writer.WritePacket(Packet.navMeshResync);
                    writer.Write(_Player.ID);
                    writer.Write(transform.position);
                    writer.Write(transform.eulerAngles);
                    //=========================================================================================\\
                    _Player.Send(SendTo.Only, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
                }
                navResyncTime = 0;
            }
        }
        catch
        {
            enabled = false;
        }
    }
    void OnCheat()
    {
        if (ServerCheatDetection.AntiTeleport(transform.position, lastPosition, NeutronServerConstants.TELEPORT_TOLERANCE))
        {
            NeutronServerFunctions.onCheatDetected(_Player, "Teleport");
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, lastPosition, 8f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, lastRotation, 8f * Time.deltaTime);
        }

        frequencyTime += Time.deltaTime;
        if (frequencyTime >= 1f)
        {
            if (ServerCheatDetection.AntiSpeedHack(mFrequency, NeutronServerConstants.SPEEDHACK_TOLERANCE))
            {
                NeutronServerFunctions.onCheatDetected(_Player, "SpeedHack");
            }
            mFrequency = 0;
            frequencyTime = 0;
        }
    }
}

public class Config : PlayerComponents
{
    public Vector3 lastPosition;
    public Quaternion lastRotation;
    public string _prefabName;
    //=======================================================
    public float mFrequency;
    [SerializeField] protected bool enableNavResync = true;
    //========================================================
    protected float frequencyTime = 0;
    protected float navResyncTime = 0;
}

public class PlayerComponents : MonoBehaviour
{
    public Rigidbody rigidBody;
    public CharacterController Controller;
    public NavMeshAgent navAgent;
}
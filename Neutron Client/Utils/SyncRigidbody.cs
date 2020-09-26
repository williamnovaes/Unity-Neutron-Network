using System.Net.Sockets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SyncRigidbody : RPCBehaviour
{
    public WhenChanging whenChanging;

    [SerializeField] private Protocol protocolType;

    [SerializeField] private float syncTime = 0.1f;
    [SerializeField] private float LerpTime = 8f;

    private Rigidbody GetRigidbody;

    [SerializeField] private SendTo sendTo;

    void Start()
    {
        if (Neutron.IsServer(gameObject)) Destroy(this);
        //==============================================//
        GetRigidbody = GetComponent<Rigidbody>();
    }

    Vector3 oldVelocity, oldRotation, oldPosition;
    Vector3 newPosition, newVelocity, newAngularVelocity;
    Quaternion newRotation;

    void Update()
    {
        if (Neutron.IsMine)
        {
            switch (whenChanging)
            {
                case (WhenChanging)default:
                    RPC();
                    break;
                case WhenChanging.Position:
                    if (transform.position != oldPosition)
                    {
                        RPC();
                        oldPosition = transform.position;
                    }
                    break;
                case WhenChanging.Rotation:
                    if (transform.eulerAngles != oldRotation)
                    {
                        RPC();
                        oldRotation = transform.eulerAngles;
                    }
                    break;
                case WhenChanging.Velocity:
                    if (GetRigidbody.velocity != oldVelocity)
                    {
                        RPC();
                        oldVelocity = GetRigidbody.velocity;
                    }
                    break;
                case (WhenChanging.Position | WhenChanging.Rotation):
                    if (transform.position != oldPosition || transform.eulerAngles != oldRotation)
                    {
                        RPC();
                        oldPosition = transform.position;
                        oldRotation = transform.eulerAngles;
                    }
                    break;
                case (WhenChanging.Velocity | WhenChanging.Position):
                    if (transform.position != oldPosition || GetRigidbody.velocity != oldVelocity)
                    {
                        RPC();
                        oldPosition = transform.position;
                        oldVelocity = GetRigidbody.velocity;
                    }
                    break;
                case (WhenChanging.Velocity | WhenChanging.Rotation):
                    if (transform.eulerAngles != oldRotation || GetRigidbody.velocity != oldVelocity)
                    {
                        RPC();
                        oldRotation = transform.eulerAngles;
                        oldVelocity = GetRigidbody.velocity;
                    }
                    break;
                case (WhenChanging.Velocity | WhenChanging.Rotation | WhenChanging.Position):
                    AnyProperty();
                    break;
                default:
                    AnyProperty();
                    break;
            }
        }
        else
        {
            if (newPosition != Vector3.zero && transform.position != newPosition) transform.position = Vector3.Lerp(transform.position, newPosition, LerpTime * Time.deltaTime);
            if (newPosition != Vector3.zero && transform.position != newPosition) transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, LerpTime * Time.deltaTime);
        }
    }

    void AnyProperty()
    {
        if (transform.eulerAngles != oldRotation || GetRigidbody.velocity != oldVelocity || transform.position != oldPosition)
        {
            RPC();
            oldRotation = transform.eulerAngles;
            oldVelocity = GetRigidbody.velocity;
            oldPosition = transform.position;
        }
    }

    void RPC()
    {
        using (NeutronWriter streamParams = new NeutronWriter())
        {
            streamParams.Write(transform.position);
            streamParams.Write(transform.rotation);
            streamParams.Write(GetRigidbody.velocity);
            streamParams.Write(GetRigidbody.angularVelocity);
            //======================================================================================================================================
            Neutron.RPC(Neutron.NeutronObject, 255, ValidationPacket.Movement, syncTime, streamParams, sendTo, false, Broadcast.Channel, (ProtocolType)(int)protocolType);
        }
    }

    [RPC(255)]
    void Sync(NeutronReader streamReader)
    {
        using (streamReader)
        {
            newPosition = streamReader.ReadVector3();
            newRotation = streamReader.ReadQuaternion();
            newVelocity = streamReader.ReadVector3();
            newAngularVelocity = streamReader.ReadVector3();
        }
    }

    private void FixedUpdate()
    {
        if (!Neutron.IsMine)
        {
            GetRigidbody.velocity = newVelocity;
            GetRigidbody.angularVelocity = newAngularVelocity;
        }
    }
}

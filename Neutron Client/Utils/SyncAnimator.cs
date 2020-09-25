using System.Net.Sockets;
using UnityEngine;

public class SyncAnimator : RPCBehaviour
{
    [SerializeField] private Protocol protocolType;

    [SerializeField] private float syncTime = 0.3f;

    private Animator GetAnimator;

    void Start()
    {
        GetAnimator = GetComponent<Animator>();
    }

    bool GetParameters(out object[] mParams)
    {
        object[] parametersToSend = new object[GetAnimator.parameterCount];

        for (int i = 0; i < parametersToSend.Length; i++)
        {
            AnimatorControllerParameter parameter = GetAnimator.GetParameter(i);
            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
                parametersToSend[i] = GetAnimator.GetBool(parameter.name);
            }
            else if (parameter.type == AnimatorControllerParameterType.Float)
            {
                parametersToSend[i] = GetAnimator.GetFloat(parameter.name);
            }
            else if (parameter.type == AnimatorControllerParameterType.Int)
            {
                parametersToSend[i] = GetAnimator.GetInteger(parameter.name);
            }
        }
        mParams = parametersToSend;
        //===========================//
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Neutron.IsMine(isMine))
        {
            if (GetParameters(out object[] parameters))
            {
                using (NeutronWriter streamParams = new NeutronWriter())
                {
                    streamParams.Write(parameters.Serialize());
                    //======================================================================================================================================
                    Neutron.RPC(isMine, 254, ValidationPacket.None, syncTime, streamParams, SendTo.Others, false, Broadcast.Channel, (ProtocolType)(int)protocolType);
                }
            }
        }
    }

    [RPC(254)]
    void Sync(NeutronReader streamReader)
    {
        using (streamReader)
        {
            object[] parameters = streamReader.ReadBytes(8192).DeserializeObject<object[]>();
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = GetAnimator.GetParameter(i);
                if (parameter.type == AnimatorControllerParameterType.Bool)
                {
                    GetAnimator.SetBool(parameter.name, (bool)parameters[i]);
                }
                else if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    GetAnimator.SetFloat(parameter.name, (float)parameters[i]);
                }
                else if (parameter.type == AnimatorControllerParameterType.Int)
                {
                    GetAnimator.SetInteger(parameter.name, (int)parameters[i]);
                }
            }
        }
    }
}

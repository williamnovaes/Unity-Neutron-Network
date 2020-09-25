using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class RPCBehaviour : NeutronBehaviour
{
    protected NeutronObject isMine;

    public void Awake()
    {
        if (TryGetComponent<NeutronObject>(out NeutronObject obj))
        {
            isMine = obj;
        }
        else isMine = transform.root.GetComponent<NeutronObject>();

        if (isMine == null && Neutron.NeutronObject == null) Debug.LogError("RPC Behaviour depends it NeutronObject. Try Add");
    }
}

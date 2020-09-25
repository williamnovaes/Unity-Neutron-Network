using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableComponent : RPCBehaviour
{
    [SerializeField] private Component[] components;
    new void Awake()
    {
        base.Awake();

        if (isMine == null)
        {
            if (!Neutron.IsMine(Neutron.NeutronObject))
            {
                Disable();
            }
            return;
        }
        if (!Neutron.IsMine(isMine)) Disable();
    }

    void Disable()
    {
        if(components.Length == 0) gameObject.SetActive(false);
        else
        {
            for(int i=0; i < components.Length; i++)
            {
                components[i].gameObject.SetActive(false);
            }
        }
    }
}

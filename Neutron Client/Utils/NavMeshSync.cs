using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshSync : RPCBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Neutron.IsMine(isMine))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 500))
                {
                    if (hit.transform.CompareTag("ground"))
                    {
                        Neutron.MoveWithMousePointer(hit.point);
                    }
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class NavSync : RPCBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //==============================================//
        if (Application.isEditor) agent.radius = 0.01f;
        //==============================================//
        agent.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        //==============================================//
        if (Neutron.IsServer(gameObject)) Destroy(this);
    }

    void Update()
    {
        if (Neutron.IsMine)
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
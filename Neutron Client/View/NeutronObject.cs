using System;
using UnityEngine;
using UnityEngine.AI;

public class NeutronObject : MonoBehaviour
{
    [NonSerialized] public NavMeshAgent agent;
    public NeutronProperty Infor;
    public NavMeshResyncProps navMeshResync;
    public NeutronSyncBehaviour myProperties;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void LateUpdate()
    {
        if (agent != null)
        {
            if (Vector3.Distance(transform.position, navMeshResync.position.ToVector3()) > NeutronConstants.navMeshTolerance)
            {
                transform.position = navMeshResync.position.ToVector3();
                //======================================================//
                agent.ResetPath();
                //======================================================//
                agent.SetDestination(transform.position);
            }
        }
    }
}

[Serializable]
public class NeutronProperty : IEquatable<NeutronProperty>
{
    public int ownerID = -1;

    public Boolean Equals(NeutronProperty other)
    {
        return this.ownerID == other.ownerID;
    }
}

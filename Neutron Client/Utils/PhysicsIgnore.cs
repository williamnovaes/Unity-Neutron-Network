using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsIgnore : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (Application.isEditor)
        {
            int layerMask = LayerMask.NameToLayer("ClientObject");
            int layerMask2 = LayerMask.NameToLayer("ServerObject");
            //======================================================//
            Physics.IgnoreLayerCollision(layerMask, layerMask2);
        }
    }
}

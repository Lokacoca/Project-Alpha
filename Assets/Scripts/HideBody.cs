using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class HideBody : MonoBehaviourPun
{
   
    void Start()
    {
        if (photonView.IsMine)
        {
       
            Renderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (Renderer rend in renderers)
            {
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                renderer.receiveShadows = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        // Disable shadow casting
        foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}

   


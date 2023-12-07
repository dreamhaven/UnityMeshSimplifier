using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper component that forces a LOD mesh's lightmaps to overlay LOD0
[ExecuteInEditMode]
public class LightmappedLOD : MonoBehaviour
{

    public LODGroup m_lodGroup;
    // Blackrazor edit - made renderer assignable in inspector
    public MeshRenderer m_currentRenderer;

    // Get mesh renderer on host
    private void Awake()
    {
        // Blackrazor edit - only get component if one wasn't assigned
        if (m_currentRenderer == null)
        {
            m_currentRenderer = gameObject.GetComponent<MeshRenderer>();
        }
        RendererInfoTransfer();
    }

    // In editor evaluate RendererInfoTransfer() each time there is a LOD switch
#if UNITY_EDITOR
    void OnBecameVisible()
    {
        if(!Application.isPlaying)
            RendererInfoTransfer();
    }
#endif

    // Blackrazor Edit - make public so we can call this after Bakery ftLightmapsStorage initialization (which potentially happens after our Awake())
    public void RendererInfoTransfer()
    {
        if (m_lodGroup == null || m_currentRenderer == null)
        {
            return;
        }

        //Gather LODs
        var lods = m_lodGroup.GetLODs();
        int currentRendererLodIndex = -1;

        //Find which LOD is the current renderer part of
        for (int i = 0; i < lods.Length; i++)
        {
            for (int j = 0; j < lods[i].renderers.Length; j++)
            {
                if (m_currentRenderer == lods[i].renderers[j])
                    currentRendererLodIndex = i;
            }
        }
        if (currentRendererLodIndex == -1)
        {
            Debug.Log("Lightmapped LOD : lod index not found on " + gameObject.name);
            return;
        }

        //Apply settings from LOD0 to current LOD
        var renderers = lods[currentRendererLodIndex].renderers;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                try
                {
                    renderers[i].lightProbeUsage = lods[0].renderers[i].lightProbeUsage;
                    renderers[i].lightmapIndex = lods[0].renderers[i].lightmapIndex;
                    renderers[i].realtimeLightmapIndex = lods[0].renderers[i].realtimeLightmapIndex;
                    // Blackrazor edit - avoid attempting to set lightmap scale on static LODs, causes warning spam
                    if (!renderers[i].isPartOfStaticBatch)
                    {
                        renderers[i].lightmapScaleOffset = lods[0].renderers[i].lightmapScaleOffset;
                        renderers[i].realtimeLightmapScaleOffset = lods[0].renderers[i].realtimeLightmapScaleOffset;
                    }

                }
                catch
                {
                    if(Debug.isDebugBuild)
                        Debug.Log("Lightmapped LOD : Error setting lightmap settings on " + gameObject.name);
                }
            }
        }
    }
}

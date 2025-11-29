using UnityEngine;
using System.Collections.Generic;

public class CachePanel : MonoBehaviour
{
    public int pathCount = 0;
    public int flowFieldMapCount = 0; 

    void Update()
    {
        pathCount = PathPool.pathCount;
        flowFieldMapCount = PathPool.flowFieldMapCount;
    }
}
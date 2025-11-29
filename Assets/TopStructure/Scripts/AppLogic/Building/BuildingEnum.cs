using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EmpireClash
{
    // 建筑类型枚举
    public enum BuildingType
    {
        Resource,
        Defense,
        Military,
        Wall,
        Decoration
    }

    // 建筑状态枚举
    public enum BuildingState
    {
        Idle,
        Building,
        Upgrading,
        Working,
        Destroyed
    }
}
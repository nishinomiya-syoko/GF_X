using UnityEngine;

public class EntityTransform : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public Transform body;
    [Tooltip("头部")]
    public Transform head_horizontal;
    [Tooltip("头部子节点")]
    public Transform head_vertical;
    [Tooltip("炮管")]
    public Transform barrel;
    [Tooltip("弹口")]
    public Transform muzzle;

}
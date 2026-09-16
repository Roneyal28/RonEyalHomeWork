using UnityEngine;

// Any reusable prefab can use this same pool, not only bullets.
public class PooledObject : MonoBehaviour
{
    public PoolManager Owner { get; internal set; }
    public PooledObject Prefab { get; internal set; }
    public bool IsInPool { get; internal set; }

    public void ReturnToPool()
    {
        if (Owner != null) Owner.Release(this);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PoolManager : MonoBehaviour
{
    [Header("Pool Setup")]
    [SerializeField, Min(1), InspectorName("Pre-made Bullets")]
    [FormerlySerializedAs("preMadeBulletCount")]
    private int preMadeBullets = 30;

    // One dictionary, one implementation, separate stack for each prefab.
    private readonly Dictionary<PooledObject, Stack<PooledObject>> pools = new();
    public int PreMadeBulletCount => preMadeBullets;
    public int CreatedCount => transform.childCount;
    public int ActiveCount
    {
        get
        {
            int count = 0;
            foreach (Transform child in transform)
                if (child.gameObject.activeSelf) count++;
            return count;
        }
    }

    public void Prewarm(PooledObject prefab, int count)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            pool = new Stack<PooledObject>(count);
            pools.Add(prefab, pool);
        }
        while (pool.Count < count) pool.Push(Create(prefab));
    }

    private PooledObject Create(PooledObject prefab)
    {
        var item = Instantiate(prefab, transform);
        item.name = prefab.name + " (Pooled " + transform.childCount.ToString("00") + ")";
        item.Owner = this;
        item.Prefab = prefab;
        item.IsInPool = true;
        item.gameObject.SetActive(false);
        return item;
    }

    public PooledObject Get(PooledObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefab)) Prewarm(prefab, 0);
        var pool = pools[prefab];
        // Grow only when empty; returned objects are reused next time.
        var item = pool.Count > 0 ? pool.Pop() : Create(prefab);
        item.IsInPool = false;
        item.transform.SetPositionAndRotation(position, rotation);
        // Caller initializes state before activating the object.
        return item;
    }

    public void Release(PooledObject item)
    {
        if (item == null || item.Owner != this || item.IsInPool) return;
        item.IsInPool = true; // Guard against returning the same object twice.
        item.gameObject.SetActive(false);
        pools[item.Prefab].Push(item);
    }
}

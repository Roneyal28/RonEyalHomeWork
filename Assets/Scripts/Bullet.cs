using UnityEngine;

public class Bullet : PooledObject
{
    public SpriteRenderer visual;
    private BulletData data;
    private float distanceTravelled;
    private Sprite defaultSprite;
    private static readonly int TargetMask = 1 << 6;

    public void Launch(BulletData bulletData)
    {
        if (defaultSprite == null) defaultSprite = visual.sprite;
        data = bulletData;
        distanceTravelled = 0f;
        visual.sprite = data.sprite != null ? data.sprite : defaultSprite;
        visual.color = data.color;
        Vector2 spriteSize = visual.sprite.bounds.size;
        visual.transform.localScale = new Vector3(data.size.x / spriteSize.x, data.size.y / spriteSize.y, 1f);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        float step = Mathf.Min(data.speed * Time.deltaTime, data.range - distanceTravelled);
        // Sweep the whole step in 3D so fast bullets cannot skip a target.
        if (Physics.Raycast(transform.position, transform.forward, out var hit, step, TargetMask))
        {
            var target = hit.collider.GetComponent<Target>();
            if (target != null) target.TakeDamage(data.damage);
            ReturnToPool();
            return;
        }
        transform.position += transform.forward * step;
        distanceTravelled += step;
        if (distanceTravelled >= data.range) ReturnToPool();
    }
}

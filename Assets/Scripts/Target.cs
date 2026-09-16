using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public int maxHealth = 100;
    public float resetDelay = 1.5f;
    public Renderer visual;
    public Text healthLabel;
    public int CurrentHealth { get; private set; }
    public int Knockdowns { get; private set; }
    private Collider hitbox;
    private MaterialPropertyBlock colors;
    private Color normalColor;
    private float resetAt;
    private float flashUntil;

    private void Awake()
    {
        hitbox = GetComponent<Collider>();
        colors = new MaterialPropertyBlock();
        normalColor = visual.sharedMaterial.color;
        ResetTarget();
    }

    public void TakeDamage(int damage)
    {
        if (CurrentHealth == 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        flashUntil = Time.time + 0.08f;
        healthLabel.text = CurrentHealth + " / " + maxHealth;
        if (CurrentHealth == 0)
        {
            Knockdowns++;
            hitbox.enabled = false;
            healthLabel.text = "RESETTING";
            resetAt = Time.time + resetDelay;
        }
    }

    private void Update()
    {
        if (CurrentHealth == 0 && Time.time >= resetAt) ResetTarget();
        Color color = CurrentHealth == 0 ? new Color(0.25f, 0.3f, 0.35f) :
            Time.time < flashUntil ? Color.white : normalColor;
        colors.SetColor("_Color", color);
        visual.SetPropertyBlock(colors);
    }

    private void ResetTarget()
    {
        CurrentHealth = maxHealth;
        hitbox.enabled = true;
        healthLabel.text = CurrentHealth + " / " + maxHealth;
    }
}

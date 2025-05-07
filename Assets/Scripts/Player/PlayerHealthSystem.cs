using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour, IDamageable
{
    [Header("Damage Entries")]
    [SerializeField, Tooltip("List of tags and how much damage each does.")]
    private List<DamageEntry> damageEntries = new List<DamageEntry>();
    [Serializable]
    public struct DamageEntry
    {
        [Tooltip("Tag of the GameObject that can damage the player.")] public string tag;
        [Tooltip("Amount of health to subtract when an object with this tag hits the player.")] public int damageAmount;
    }

    [Header("Health Settings")]
    [SerializeField, Tooltip("Maximum health of the character.")] private int maxHealth = 3;
    public int MaxHealth => maxHealth;

    public event Action<int> OnHealthChanged;
    public event Action OnDeath;
    
    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int unusedAmount, GameObject source)
    {
        if (currentHealth <= 0) return;

        var entry = damageEntries.Find(e => e.tag == source.tag);
        if (entry.tag == null) return;

        currentHealth = Mathf.Max(currentHealth - entry.damageAmount, 0);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth == 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public int GetCurrentHealth() => currentHealth;

    public bool CanBeDamagedBy(GameObject source)
    {
        return damageEntries.Exists(e => e.tag == source.tag);
    }

    public int GetDamageAmountForTag(string tag)
    {
        var entry = damageEntries.Find(e => e.tag == tag);
        return entry.tag != null ? entry.damageAmount : 0;
    }
}

public interface IDamageable
{
    void TakeDamage(int damage, GameObject source);
    void Heal(int amount);
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private PlayerHealthSystem healthSystem;
    private List<Image> hearts = new List<Image>();

    public void SetHealthSystem(PlayerHealthSystem newHealthSystem)
    {
        healthSystem = newHealthSystem;

        if (healthSystem != null)
        {
            InitializeHearts();
            healthSystem.OnHealthChanged += UpdateHearts;
            UpdateHearts(healthSystem.GetCurrentHealth());
        }
    }

    private void InitializeHearts()
    {
        //Clean up existing hearts
        foreach (var heart in hearts)
        {
            Destroy(heart.gameObject);
        }
        hearts.Clear();

        int maxH = healthSystem.MaxHealth;
        for (int i = 0; i < maxH; i++)
        {
            GameObject h = Instantiate(heartPrefab, transform);
            hearts.Add(h.GetComponent<Image>());
        }
    }

    private void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].sprite = (i < currentHealth) ? fullHeartSprite : emptyHeartSprite;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged -= UpdateHearts;
        }
    }
}
using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [SerializeField, Tooltip("The Player prefab.")] private GameObject _playerPrefab;
    [SerializeField, Tooltip("The GameObject representing the red dot.")] private GameObject _redDotPrefab;
    private HealthUI _healthUI;

    private void Start()
    {
        _healthUI = FindObjectOfType<HealthUI>();
        if (_healthUI == null)
        {
            Debug.LogError("HealthUI not found in the scene. Please ensure a HealthUI component exists and is active.");
            return;
        }

        if (_playerPrefab != null)
        {
            GameObject player = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
            GameObject redDot = Instantiate(_redDotPrefab, transform.position, Quaternion.identity);

            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetRedDot(redDot);
            }
            else
            {
                Debug.LogError("Player prefab does not have a PlayerController component.");
            }

            PlayerHealthSystem healthSystem = player.GetComponent<PlayerHealthSystem>();
            if (healthSystem != null && _healthUI != null)
            {
                _healthUI.SetHealthSystem(healthSystem);
            }
            else
            {
                Debug.LogError("Player prefab does not have a PlayerHealthSystem component or HealthUI is not assigned.");
            }
        }
        else
        {
            Debug.LogError("Player prefab is not assigned in the Inspector.");
        }
    }
}
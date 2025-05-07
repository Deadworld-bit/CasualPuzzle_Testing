using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Cell : MonoBehaviour
{
    [Header("Replacement Settings")]    
    [SerializeField] private GameObject _replacementPrefab;
    [SerializeField] private GameObject _destroyEffectPrefab;

    [Header("Collision Settings")]
    [SerializeField] private string _triggerTag = "Player";

    private void OnCollisionEnter(Collision collision)
    {
        if (string.IsNullOrEmpty(_triggerTag) || collision.gameObject.CompareTag(_triggerTag))
        {
            ExplodeAndReplace();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(_triggerTag) || other.CompareTag(_triggerTag))
        {
            ExplodeAndReplace();
        }
    }

    private void ExplodeAndReplace()
    {
        Vector3 originalPosition = transform.position;

        if (_destroyEffectPrefab != null)
        {
            Instantiate(_destroyEffectPrefab, originalPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Destroy effect prefab not assigned on {name}.");
        }

        if (_replacementPrefab != null)
        {
            Vector3 replacementPosition = new Vector3(originalPosition.x, 0f, originalPosition.z);
            Instantiate(_replacementPrefab, replacementPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Replacement prefab not assigned on {name}.");
        }

        Destroy(gameObject);
    }
}

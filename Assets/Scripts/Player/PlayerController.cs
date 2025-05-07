using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Input System")]
    [SerializeField] private InputAction _pointerPositionAction;
    [SerializeField] private InputAction _pointerPressAction;

    [Header("Player Settings")]
    [SerializeField, Tooltip("How fast the character moves.")] private float _moveSpeed = 10f;
    [SerializeField, Tooltip("How fast the character rotates when changing direction.")] private float _rotationSpeed = 360f; // Degrees per second
    [SerializeField, Tooltip("Force applied to the player when damaged.")] private float pushbackForce = 5f;

    [Header("Effect")]
    [SerializeField, Tooltip("Prefab for the smoke effect when the player dies.")] private GameObject smokeEffectPrefab;
    [SerializeField, Tooltip("Prefab for the winning effect when the player reaches the end.")] private GameObject winningEffectPrefab;

    private Camera _mainCamera;
    private Timer _timer;
    private Rigidbody _playerRigidbody;
    private Animator _animator;
    private PlayerHealthSystem _healthSystem;
    private GameObject _redDot;

    private bool _isHolding = false;
    private bool _isInputDisabled = false;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _playerRigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _healthSystem = GetComponent<PlayerHealthSystem>();
        _timer = FindObjectOfType<Timer>();

        if (_redDot != null)
        {
            _redDot.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Red Dot GameObject is not assigned in the Inspector.");
        }

        if (_timer == null)
        {
            Debug.LogWarning("Timer not found in the scene.");
        }

        _healthSystem.OnDeath += HandlePlayerDeath;
    }

    private void OnEnable()
    {
        _pointerPositionAction.Enable();
        _pointerPressAction.Enable();
    }

    private void OnDisable()
    {
        _pointerPositionAction.Disable();
        _pointerPressAction.Disable();
    }

    private void Update()
    {
        if (_isInputDisabled) return;

        HandleInput();
    }

    private void HandleInput()
    {
        bool isPressing = _pointerPressAction.ReadValue<float>() > 0;
        Vector2 pointerPosition = _pointerPositionAction.ReadValue<Vector2>();

        if (isPressing && !_isHolding)
        {
            StartHolding(pointerPosition);
        }
        else if (!isPressing && _isHolding)
        {
            StopHolding();
        }

        if (_isHolding)
        {
            Vector3 targetPosition = GetWorldPositionOnPlane(pointerPosition);
            UpdateRedDotPosition(targetPosition);
            MoveTowards(targetPosition);
            RotateTowards(targetPosition);
        }
    }

    private void StartHolding(Vector2 pointerPosition)
    {
        _isHolding = true;
        if (_redDot != null)
        {
            _redDot.SetActive(true);
            _redDot.transform.position = GetWorldPositionOnPlane(pointerPosition) + Vector3.up * 1f;
        }
    }

    private void StopHolding()
    {
        _isHolding = false;
        if (_redDot != null)
        {
            _redDot.SetActive(false);
        }
        _playerRigidbody.velocity = Vector3.zero;
        _animator.SetBool("isMoving", false);
    }

    private void UpdateRedDotPosition(Vector3 targetPosition)
    {
        if (_redDot != null)
        {
            _redDot.transform.position = targetPosition + Vector3.up * 1f;
        }
        _animator.SetBool("isMoving", true);
    }

    private Vector3 GetWorldPositionOnPlane(Vector2 screenPos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return transform.position;
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        _playerRigidbody.velocity = direction * _moveSpeed;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage, GameObject source)
    {
        ApplyPushback(source);
        DisableInput();

        StartCoroutine(ReEnableInput());
    }

    private void ApplyPushback(GameObject source)
    {
        Vector3 pushDirection = (transform.position - source.transform.position).normalized;
        _playerRigidbody.AddForce(pushDirection * pushbackForce, ForceMode.Impulse);
    }

    private void DisableInput()
    {
        _isInputDisabled = true;
        _isHolding = false;
        if (_redDot != null)
        {
            _redDot.SetActive(false);
        }
        _playerRigidbody.velocity = Vector3.zero;
        _animator.SetBool("isMoving", false);
    }

    private IEnumerator ReEnableInput()
    {
        yield return new WaitForSeconds(0.5f);
        _isInputDisabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyDamage(collision.gameObject);
    }

    private void TryApplyDamage(GameObject source)
    {
        if (!_healthSystem.CanBeDamagedBy(source)) return;

        int damage = _healthSystem.GetDamageAmountForTag(source.tag);
        if (damage <= 0) return;

        _healthSystem.TakeDamage(damage, source);
        TakeDamage(damage, source);
    }

    public void Heal(int amount)
    {
        throw new System.NotImplementedException();
    }

    public void SetRedDot(GameObject redDot)
    {
        _redDot = redDot;
        if (_redDot != null)
        {
            _redDot.SetActive(false);
        }
    }

    private void HandlePlayerDeath()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        if (smokeEffectPrefab != null)
        {
            Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Smoke effect prefab is not assigned in PlayerController.");
        }

        _isInputDisabled = true;
        _playerRigidbody.velocity = Vector3.zero;

        if (_timer != null)
        {
            _timer.StopTimer();
        }

        UIManager.instance.ShowFailUI();
    }

    public void OnWin()
    {
        if (winningEffectPrefab != null)
        {
            Instantiate(winningEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Winning effect prefab is not assigned in PlayerController.");
        }

        _isInputDisabled = true;
        _playerRigidbody.velocity = Vector3.zero;

        if (_timer != null)
        {
            _timer.StopTimer();
            int score = _timer.CalculateScore();
            UIManager.instance.ShowWinningUI(score);
        }
        else
        {
            UIManager.instance.ShowWinningUI(0);
        }
    }
}
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField, Tooltip("Starting countdown duration in seconds.")] private float countdownDuration = 60f;

    private float remainingTime;
    private bool isRunning = false;

    void Start()
    {
        ResetTimer();
        StartTimer();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        remainingTime = countdownDuration;
        UpdateDisplay(remainingTime);
    }

    void Update()
    {
        if (!isRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateDisplay(0f);
            isRunning = false;

            UIManager.instance.ShowFailUI();
            return;
        }

        UpdateDisplay(remainingTime);
    }

    private void UpdateDisplay(float time)
    {
        float t = Mathf.Max(0f, time);
        string minutes = ((int)t / 60).ToString("00");
        string seconds = ((int)(t % 60)).ToString("00");
        if (timerText != null)
            timerText.text = minutes + ":" + seconds;
    }

    public float GetRemainingTime() => remainingTime;

    public int CalculateScore() => Mathf.Max(0, (int)(remainingTime * 10));
}

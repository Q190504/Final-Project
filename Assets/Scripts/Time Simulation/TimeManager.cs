using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Settings")]
    public float secondsPerTick = 1f;

    public float defaultTimeScale = 1f;

    public float speedUpFirstTimeScale = 5f;

    public bool IsPaused { get; set; }
    public bool IsSpeedUp { get; set; }

    public bool IsStarted { get; set; }

    public float CurrentTick { get; private set; }

    public int CurrentDay => Mathf.FloorToInt(CurrentTick);

    [Header("Input")]
    [SerializeField] private InputAction toggleTimeAction;
    [SerializeField] private InputAction togglePauseAction;

    [Header("Events")]
    [SerializeField] private BoolPublisherSO togglePauseSO;
    [SerializeField] private FloatPublisherSO onToggleTimeSO;
    [SerializeField] private FloatPublisherSO onTickIncreaseSO;
    [SerializeField] private StringPublisherSO onDayIncreaseSO;

    //public event Action<float> OnAdvanceTime;
    //public event Action<int> OnTick;

    private EventScheduler scheduler = new();
    private float currentTimeScale = 1f;
    private float thisTickTimer = 0f;

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is enabled
    /// </summary>
    void OnEnable()
    {
        toggleTimeAction.Enable();
        togglePauseAction.Enable();
    }

    /// <summary>
    /// Standard Unity function called whenever the attached gameobject is disabled
    /// </summary>
    void OnDisable()
    {
        toggleTimeAction.Disable();
        togglePauseAction.Disable();
    }

    void Awake()
    {
        Instance = this;
        if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if (toggleTimeAction.bindings.Count == 0)
        {
            Debug.LogWarning("The Speed Up Time Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }

        if (togglePauseAction.bindings.Count == 0)
        {
            Debug.LogError("The Pause Action does not have a binding set! Make sure that each Input Action has a binding set or the controller will not work!");
        }
    }

    void Update()
    {
        HandleInput();

        if (IsPaused || !IsStarted)
            return;

        float deltaTick = Time.deltaTime * currentTimeScale / secondsPerTick;

        float newTick = CurrentTick + deltaTick;
        scheduler.AdvanceTo(newTick);
        CurrentTick = newTick;

        thisTickTimer += deltaTick;

        onTickIncreaseSO.RaiseEvent(thisTickTimer);

        if (thisTickTimer >= 1f)
        {
            SetDay();
            thisTickTimer -= 1f;
        }
    }

    private void HandleInput()
    {
        if (toggleTimeAction.WasPressedThisFrame())
        {
            if (IsPaused)
                TogglePause();

            if (currentTimeScale == defaultTimeScale)
            {
                IsSpeedUp = true;
                currentTimeScale = speedUpFirstTimeScale;
            }
            else
            {
                IsSpeedUp = false;
                currentTimeScale = defaultTimeScale;
            }

            onToggleTimeSO.RaiseEvent(currentTimeScale);
        }

        if (togglePauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public ScheduledEvent ScheduleEvent(float delayTicks, Action action, EventPriority priority = EventPriority.RandomEvent)
    {
        return scheduler.Schedule(delayTicks, action, priority);
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;

        togglePauseSO.RaiseEvent(IsPaused);
    }

    public void SetTimeScale(float scale)
    {
        currentTimeScale = scale;
    }

    public void SetDefaultTimeScale()
    {
        currentTimeScale = defaultTimeScale;
    }

    private void SetDay()
    {
        onDayIncreaseSO.RaiseEvent(((int)CurrentTick + 1).ToString());
    }

    //public void StepOneEvent()
    //{
    //    IsPaused = true;

    //    float nextTime = scheduler.CurrentTime;

    //    scheduler.AdvanceTo(nextTime + 0.0001f);

    //    CurrentTick = scheduler.CurrentTime;
    //}

    public void StartMatch()
    {
        SetDefaultTimeScale();
        IsStarted = true;
        thisTickTimer = 0;
        CurrentTick = 0;
        SetDay();
        SpreadMethodManager.Instance.StartMethods();
    }
}
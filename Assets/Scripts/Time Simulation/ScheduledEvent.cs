using System;

public class ScheduledEvent
{
    public float ExecutionTime;
    public int Priority;
    public Action Action;
    public bool isCancelled;

    public ScheduledEvent(float executionTime, int priority, Action action, bool isCancelled)
    {
        ExecutionTime = executionTime;
        Priority = priority;
        Action = action;
        this.isCancelled = isCancelled;
    }
}
using System;

public class ScheduledEvent
{
    public float ExecutionTime;
    public int EventPriority;
    public int EventOrder;
    public Action Action;
    public bool isCancelled;

    public ScheduledEvent(float executionTime, int priority, int order, Action action, bool isCancelled)
    {
        ExecutionTime = executionTime;
        EventPriority = priority;
        EventOrder = order;
        Action = action;
        this.isCancelled = isCancelled;
    }
}
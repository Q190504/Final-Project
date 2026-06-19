using System;
using UnityEngine;

public class EventScheduler
{
    private readonly PriorityQueue<ScheduledEvent> eventQueue;

    private readonly ScheduledEventComparer comparer;

    private EventPriority lastPriority = EventPriority.None;
    private int nextOrder = 0;

    public float CurrentTime { get; private set; }

    private bool needUpdateVisual;

    public EventScheduler()
    {
        comparer = new ScheduledEventComparer();

        eventQueue = new PriorityQueue<ScheduledEvent>(comparer);

        CurrentTime = 0f;
    }

    public void AdvanceTo(float time)
    {
        CurrentTime = time;
        needUpdateVisual = false;

        while (true)
        {
            if (eventQueue.Count == 0)
                break;

            ScheduledEvent next = eventQueue.Peek();

            if (next.ExecutionTime > CurrentTime)
                break;

            eventQueue.Dequeue();

            if (!next.isCancelled)
            {
                next.Action?.Invoke();
                needUpdateVisual = true;
            }
        }

        if (needUpdateVisual)
        {
            UIManager.Instance.UpdateCellsVisualEveryTick();

            needUpdateVisual = false;
        }
    }

    public ScheduledEvent Schedule(float delay, Action action, EventPriority priority = EventPriority.None, int order = -1)
    {
        ScheduledEvent e;
        if (priority != lastPriority)
            nextOrder = -1;

        if (order < 0)
            e = new(CurrentTime + delay, (int)priority, nextOrder++, action, false);
        else
            e = new(CurrentTime + delay, (int)priority, order, action, false);

        eventQueue.Enqueue(e);
        lastPriority = priority;
        return e;
    }

    public void Cancel(ScheduledEvent e)
    {
        e.isCancelled = true;
    }

    public void Clear()
    {
        eventQueue.Clear();
        CurrentTime = 0;
        lastPriority = EventPriority.None;
        nextOrder = -1;
    }
}
using System;
using UnityEngine;

public class EventScheduler
{
    private readonly PriorityQueue<ScheduledEvent> eventQueue;
    private readonly ScheduledEventComparer comparer;

    public float CurrentTime { get; private set; }

    private bool needUpdateVisual = false;
    private bool stillhasActionThisTime = true;

    public EventScheduler()
    {
        comparer = new ScheduledEventComparer();
        eventQueue = new PriorityQueue<ScheduledEvent>(comparer);
        CurrentTime = 0f;
    }

    public void AdvanceTo(float time)
    {
        CurrentTime = time;
        ScheduledEvent next;
        stillhasActionThisTime = true;
        needUpdateVisual = false;

        while (stillhasActionThisTime)
        {
            next = eventQueue.Peek();

            if (next == default || next.ExecutionTime > CurrentTime)
            {
                stillhasActionThisTime = false;
                break;
            }

            eventQueue.Dequeue();

            if (!next.isCancelled)
            {
                next.Action?.Invoke();
                needUpdateVisual = true;
            }
        }

        if (needUpdateVisual)
        {
            UIManager.Instance.UpdateCellsVisual();
            needUpdateVisual = false;
        }

        stillhasActionThisTime = false;
    }

    public ScheduledEvent Schedule(
        float delay,
        Action action,
        EventPriority priority = EventPriority.RandomEvent)
    {
        ScheduledEvent e = new(CurrentTime + delay, (int)priority, action, false);

        eventQueue.Enqueue(e);

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
    }
}
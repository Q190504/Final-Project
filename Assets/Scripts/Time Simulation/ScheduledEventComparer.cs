using System.Collections.Generic;

public class ScheduledEventComparer : IComparer<ScheduledEvent>
{
    public int Compare(ScheduledEvent a, ScheduledEvent b)
    {
        int timeCompare = a.ExecutionTime.CompareTo(b.ExecutionTime);

        if (timeCompare != 0)
            return timeCompare;

        int priorityCompare = a.EventPriority.CompareTo(b.EventPriority);

        if (priorityCompare != 0)
            return priorityCompare;

        return a.EventOrder.CompareTo(b.EventOrder);
    }
}
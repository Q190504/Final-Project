using System.Collections.Generic;

public class ScheduledEventComparer : IComparer<ScheduledEvent>
{
    public int Compare(ScheduledEvent a, ScheduledEvent b)
    {
        int timeCompare = a.ExecutionTime.CompareTo(b.ExecutionTime);
        if (timeCompare != 0)
            return timeCompare;

        return a.Priority.CompareTo(b.Priority);
    }
}
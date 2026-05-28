using System;
using System.Collections.Generic;

public static class DungeonCalendarUtility
{
    public static readonly DayOfWeek[] WeeklyOrder =
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };

    public static string GetDayLabel(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "MONDAY",
            DayOfWeek.Tuesday => "TUESDAY",
            DayOfWeek.Wednesday => "WEDNESDAY",
            DayOfWeek.Thursday => "THURSDAY",
            DayOfWeek.Friday => "FRIDAY",
            DayOfWeek.Saturday => "SATURDAY",
            DayOfWeek.Sunday => "SUNDAY",
            _ => day.ToString().ToUpperInvariant()
        };
    }

    public static string GetWeekText(IEnumerable<DayOfWeek> days)
    {
        if (days == null)
        {
            return "Always";
        }

        List<string> labels = new List<string>();
        foreach (DayOfWeek day in days)
        {
            labels.Add(GetDayLabel(day));
        }

        return labels.Count == 0 ? "Always" : string.Join(", ", labels);
    }

    public static bool IsAvailableToday(IEnumerable<DayOfWeek> days)
    {
        if (days == null)
        {
            return true;
        }

        DayOfWeek today = DateTime.UtcNow.DayOfWeek;
        foreach (DayOfWeek day in days)
        {
            if (day == today)
            {
                return true;
            }
        }

        return false;
    }
}

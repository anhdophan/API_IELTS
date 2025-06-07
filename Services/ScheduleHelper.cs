using System;
using System.Collections.Generic;
using api.Models;

namespace api.Services
{
    public static class ScheduleHelper
    {
        public static List<StudySession> GenerateStudySessions(Class cls, List<DateTime> examDates = null)
        {
            var sessions = new List<StudySession>();
            var examSet = new HashSet<DateTime>(examDates ?? new List<DateTime>());

            var dayOfWeekMap = new Dictionary<string, DayOfWeek>(StringComparer.OrdinalIgnoreCase)
            {
                {"Sunday", DayOfWeek.Sunday},
                {"Monday", DayOfWeek.Monday},
                {"Tuesday", DayOfWeek.Tuesday},
                {"Wednesday", DayOfWeek.Wednesday},
                {"Thursday", DayOfWeek.Thursday},
                {"Friday", DayOfWeek.Friday},
                {"Saturday", DayOfWeek.Saturday}
            };

            for (var date = cls.StartDate.Date; date <= cls.EndDate.Date; date = date.AddDays(1))
            {
                foreach (var sched in cls.Schedule)
                {
                    if (dayOfWeekMap.TryGetValue(sched.DayOfWeek, out var dow) && date.DayOfWeek == dow)
                    {
                        var isExam = examSet.Contains(date);
                        sessions.Add(new StudySession
                        {
                            Date = date,
                            DayOfWeek = sched.DayOfWeek,
                            StartTime = sched.StartTime,
                            EndTime = sched.EndTime,
                            Material = isExam ? "Exam" : $"Material for {date:yyyy-MM-dd}",
                            IsExam = isExam
                        });
                    }
                }
            }
            return sessions;
        }
    }
}
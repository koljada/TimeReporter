using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace TimeReporter
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            DateTime today = DateTime.Now;
            EventLog[] eventLogs = EventLog.GetEventLogs();

            var breaks = GetBreaks(eventLogs.FirstOrDefault(x => x.Log == "Security").Entries);
            double totalBreakTime = breaks.Sum(x => (x.Value - x.Key).TotalMinutes);
            double roundedTotalBreakTime = Math.Ceiling(totalBreakTime / 5) * 5;

            EventLogEntry firstTodayLog = eventLogs.FirstOrDefault(x => x.Log == "System").Entries.OfType<EventLogEntry>()
                .Where(e => e.TimeGenerated.Date == today.Date)
                .OrderBy(x => x.TimeGenerated)
                .FirstOrDefault();
            DateTime startTime = firstTodayLog.TimeGenerated;

            double totalMinutes = (today - startTime).TotalMinutes - roundedTotalBreakTime;

            Console.WriteLine();
            string output = $"Start   : {startTime:hh:mm}\n" +
                $"Break   : {roundedTotalBreakTime:n2}m\n" +
                $"End     : {today:hh:mm}\n" +
                $"Duration: {(totalMinutes / 60):n2}h";
            Console.WriteLine(output);
            Clipboard.SetText(output);

            Console.WriteLine("\nPress d to show detailed break timeline or any other key to exit");
            string key = Console.ReadLine();
            if (key == "d")
            {
                Console.WriteLine();
                breaks.ForEach(b => Console.WriteLine($"\t From:{b.Key} to:{b.Value}. Duration: {(b.Value - b.Key).TotalMinutes:n2}m"));
                Console.WriteLine();
                Console.WriteLine($"\t Total break time: {totalBreakTime:n2}m");
                Console.ReadLine();
            }
        }

        private static List<KeyValuePair<TimeSpan, TimeSpan>> GetBreaks(EventLogEntryCollection collection)
        {
            var today = DateTime.Now.Date;
            var breaks = new List<KeyValuePair<TimeSpan, TimeSpan>>();
            TimeSpan? breakStart = null;

            foreach (var log in collection.OfType<EventLogEntry>().Where(x => x.TimeGenerated.Date == today))
            {
                if (log.InstanceId == 4800)
                {
                    breakStart = log.TimeGenerated.TimeOfDay;
                }
                else if (log.InstanceId == 4801 && breakStart.HasValue)
                {
                    breaks.Add(new KeyValuePair<TimeSpan, TimeSpan>(breakStart.Value, log.TimeGenerated.TimeOfDay));
                    breakStart = null;
                }
            }

            return breaks;
        }
    }
}

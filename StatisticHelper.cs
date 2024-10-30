using System;
using System.Collections.Generic;

namespace Frigate_Helper
{
    // A static class designed to handle statistics management.
    public static class StatisticHelper
    {
        // Delegate for handling statistic readiness events.
        public delegate void StatisticEventHandler(IStatistics s);
        
        // Event that is triggered when statistics are updated and ready.
        public static event StatisticEventHandler? StatisticReady;

        // A thread-safe dictionary to store statistics by topic.
        readonly static Dictionary<string, IStatistics> statistics = new Dictionary<string, IStatistics>();

        /// <summary>
        /// Updates or adds a statistic for the given topic.
        /// </summary>
        /// <typeparam name="T">The type of the statistic data.</typeparam>
        /// <param name="topic">The topic associated with the statistic.</param>
        /// <param name="e">The event that may affect the statistic.</param>
        /// <param name="defaultValue">The default value if the statistic is created.</param>
        /// <param name="refreshData">Action to refresh data for the statistic.</param>
        /// <returns>The updated or newly created statistic.</returns>
        public static IStatistics Update<T>(string topic, Event? e, T defaultValue, Action<Statistic<T>.StatData> refreshData)
        {
            lock(statistics) // Ensure exclusive access to the dictionary.
            {
                // Try to get the statistic based on the topic.
                if (!statistics.TryGetValue(topic, out IStatistics? value))
                {
                    // Create a new statistic if it does not exist.
                    value = new Statistic<T>(topic, defaultValue, refreshData);
                    statistics.Add(topic, value);
                }
                
                // Add the event if it is not null.
                if (e != null) value.Add(e);
                
                return value; // Return the statistic.
            }
        }

        /// <summary>
        /// Updates statistics for moving and stationary states.
        /// </summary>
        /// <param name="topic">The base topic for statistics.</param>
        /// <param name="e">The event that may indicate movement status.</param>
        /// <returns>The updated topic string.</returns>
        public static string Update_MoveStationary(string topic, Event? e)
        {
            // Update moving statistics.
            Update(topic + "/moving", e, 0, x =>
            {
                // Increment if the event indicates not stationary.
                if (x.ev?.IsStationary == false) x.value++;
            });
            
            // Update stationary statistics.
            Update(topic + "/stationary", e, 0, x =>
            {
                // Increment if the event indicates stationary.
                if (x.ev?.IsStationary == true) x.value++;
            });
            
            return topic + "/"; // Return the updated topic.
        }

        /// <summary>
        /// Clears all events from the stored statistics.
        /// </summary>
        public static void Clear()
        {
            lock(statistics) // Ensure exclusive access to the dictionary.
            {
                // Iterate through all statistics and clear their events.
                foreach (var s in statistics.Values)
                {
                    s.ClearEvents();
                }
            }
        }

        /// <summary>
        /// Refreshes all statistics and triggers the StatisticReady event if any statistic has changed.
        /// </summary>
        /// <param name="forceRefresh">Indicates whether to force a refresh of all statistics.</param>
        public static void RefreshAll(bool forceRefresh = false)
        {
            lock(statistics) // Ensure exclusive access to the dictionary.
            {
                // Iterate through all statistics.
                foreach (var s in statistics.Values)
                {
                    s.Refresh(); // Refresh the statistic.

                    // If the statistic is changed or if forced, trigger the event.
                    if (forceRefresh || s.IsChanged)
                        StatisticReady?.Invoke(s);
                }
            }
        }

        /// <summary>
        /// Dumps the current state of all statistics to the console.
        /// </summary>
        public static void ConsoleDump()
        {
            // Iterate through all statistics and print their string representations.
            foreach (var s in statistics.Values)
            {
                Console.WriteLine(s.ToString());
            }
        }
    }
}
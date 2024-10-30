using System;
using System.Collections.Generic; // Required for List<T>

namespace Frigate_Helper
{
    public interface IStatistics
    {
        string Topic { get; }
        string ToPayload();
        void Add(Event e);
        void Refresh();
        void ClearEvents();
        bool IsChanged { get; }
    }

    public class Statistic<T> : IStatistics
    {
        private string topic; // The topic name for this statistic
        private bool isChanged = true; // Flag indicating if the statistic has changed since the last refresh
        private bool firstRun = true; // Flag to check if this is the first run of the refresh

        // Nested class to hold data for refreshing statistics
        public class StatData
        {
            public T? value; // Current value being processed
            public Event? ev; // Current event associated with this statistic
        }

        private T? currentValue; // Holds the current value of the statistic
        private T? lastValue; // Holds the last recorded value of the statistic
        private T resetValue; // Initial value used to reset the statistic
        private List<Event> events = new List<Event>(); // List of events associated with this statistic
        private Action<StatData> refreshData; // Action to execute when refreshing data

        public T? Value 
        { 
            get => currentValue; 
            set => currentValue = value; // Allows setting the current value
        }

        public string Topic 
        { 
            get => topic; 
            set => topic = value; // Allows setting the topic
        }

        public Statistic(string topic, T initValue, Action<StatData> refreshData)
        {
            this.topic = topic; // Set the topic
            this.refreshData = refreshData; // Set the refresh action
            currentValue = resetValue = initValue; // Initialize current and reset value
        }

        public void Refresh()
        {
            lastValue = currentValue; // Store the last value before refresh
            currentValue = resetValue; // Reset current value to the predefined reset value

            // Prepare a temporary data holder for current refresh process
            StatData data = new StatData { value = currentValue };

            // Use a for loop instead of ForEach for better performance
            for (int i = 0; i < events.Count; i++)
            {
                data.ev = events[i]; // Set the current event in the data holder
                refreshData.Invoke(data); // Invoke the refresh action with the current data
            }

            // Finalize the current value after processing events
            currentValue = data.value; // Update current value with the result from event processing

            // Determine if there has been any change since the last refresh
            if (firstRun)
            {
                firstRun = false; // Mark the first run as complete
            }
            else
            {
                // Cache the comparison result for efficiency
                bool valuesEqual = EqualityComparer<T>.Default.Equals(currentValue, lastValue);
                isChanged = !valuesEqual; // Track if there was a change
            }
        }

        public void Add(Event e)
        {
            events.Add(e); // Add the incoming event to the list
        }

        public void ClearEvents()
        {
            events.Clear(); // Remove all events from the list
        }

        public bool IsChanged => isChanged; // Property indicating changes since the last refresh

        public string ToPayload()
        {
            // Convert the current value to a string, return empty string if null
            return Value?.ToString() ?? string.Empty; 
        }

        public override string ToString()
        {
            return string.Format("    {0}: {1}", Topic, Value); // Format the output string for display
        }
    }
}
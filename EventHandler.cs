using System;
using System.Collections.Generic;
using System.Linq;

namespace Frigate_Helper
{
    public class EventHandler
    {
        // Dictionary to store events, keyed by their unique ID
        Dictionary<string, Event> events = new Dictionary<string, Event>();

        // Method to add or update an event
        public void Add(Event e)
        {
            // Ensure the event ID is not null
            if (e.ID != null)
            {
                lock (this) // Locking to ensure thread safety
                {
                    // Check if the event already exists in the dictionary
                    if (events.TryGetValue(e.ID, out Event? last))
                    {
                        // If the event is an update and already exists
                        if (!e.IsUpdate)
                        {
                            // Log a duplicate event
                            Console.WriteLine($"DUPLICATE: {e.ID}");
                            return; // Exit early if it's a duplicate that shouldn't update
                        }
                        // Log an update for the existing event
                        Console.WriteLine($"Updating: {e.ID}, Span: {last.Lifetime.TotalSeconds}");
                        events[e.ID] = e; // Update the event in the dictionary
                    }
                    else
                    {
                        // Log the new event, differentiating between a fresh event and an update
                        Console.WriteLine(e.IsUpdate ? $"New: {e.ID} from update" : $"New: {e.ID}");
                        events.Add(e.ID, e); // Add the new event to the dictionary
                    }
                }
            }
        }

        // Method to delete an event
        public void Delete(Event e)
        {
            // Ensure the event ID is not null
            if (e.ID != null)
            {
                lock (this) // Locking to ensure thread safety
                {
                    // Attempt to remove the event from the dictionary
                    if (events.Remove(e.ID))
                    {
                        // Log the deletion of the event
                        Console.WriteLine($"Delete: {e.ID}");
                    }
                }
            }
        }

        // Method to handle incoming events
        public void Handle(Event e)
        {
            // If the event indicates it's an ending state, delete it
            if (e.IsEnd)
            {
                Console.WriteLine($"End: {(e.ID != null ? e.ID : "NA")}");
                Delete(e);
            }
            else
            {
                // Otherwise, add the event to the collection
                Add(e);
            }
            // After handling, generate statistics based on current events
            GenerateStatistics();
        }

        // Method to generate statistics on current events
        internal void GenerateStatistics(bool forceRefresh = false)
        {
            Dictionary<string, Event> tempEvents;
            lock (this) // Locking to ensure thread safety while accessing the events dictionary
            {
                // Create a temporary copy of the events for processing
                tempEvents = new Dictionary<string, Event>(events);
            }

            StatisticHelper.Clear(); // Clear existing statistics before updating

            // Iterate through the copied events to generate statistics
            foreach (var e in tempEvents.Values)
            {
                // Check if the event has expired
                if (e.IsExpired)
                {
                    // Log the expiration of the event
                    Console.WriteLine($"Expiring: {e.ID}");
                    Delete(e); // Delete the expired event
                }
                else
                {
                    // Update the overall statistics for all events
                    StatisticHelper.Update_MoveStationary("all", e);
                    if (e.Label != null)
                    {
                        // Update statistics per label if available
                        var parentTopic = StatisticHelper.Update_MoveStationary($"labels/{e.Label}", e);
                        if (e.Camera != null)
                        {
                            // Update camera statistics related to this event
                            StatisticHelper.Update_MoveStationary($"{parentTopic}cameras/{e.Camera}", e);
                        }
                        foreach (var zone in e.CurrentZones)
                        {
                            // Update zone statistics associated with this event
                            StatisticHelper.Update_MoveStationary($"{parentTopic}zones/{zone}", e);
                        }
                    }

                    foreach (var zone in e.CurrentZones)
                    {
                        // Handle events in the context of their specific zones
                        var parentTopic = StatisticHelper.Update_MoveStationary($"zones/{zone}", e);
                        if (e.Label != null)
                        {
                            // Update label statistics for events in this zone
                            StatisticHelper.Update_MoveStationary($"{parentTopic}labels/{e.Label}", e);
                        }
                    }

                    // If a camera is associated with the event, update camera statistics
                    if (e.Camera != null)
                    {
                        var parentTopic = StatisticHelper.Update_MoveStationary($"cameras/{e.Camera}", e);
                        foreach (var zone in e.CurrentZones)
                        {
                            // Update zone statistics for the associated camera event
                            StatisticHelper.Update_MoveStationary($"{parentTopic}zones/{zone}", e);
                        }
                    }
                }
            }
            // Refresh all statistics, optionally forcing a refresh
            StatisticHelper.RefreshAll(forceRefresh);
            StatisticHelper.ConsoleDump(); // Log all statistics to the console
        }

        // Method to check if any events are expired
        internal bool CheckExpired()
        {
            lock (this) // Locking to ensure thread safety
            {
                // Return true if any event has expired
                return events.Values.Any(e => e.IsExpired);
            }
        }
    }
}
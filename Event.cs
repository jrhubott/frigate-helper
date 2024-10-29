using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Frigate_Helper;

/// <summary>
/// Represents an event with associated data and metadata.
/// </summary>
public class Event
{
    // Stores the parsed JSON data of the event.
    JObject? json;
    
    // Timestamp indicating when the event was created.
    DateTime created;

    /// <summary>
    /// Initializes a new instance of the Event class with the specified event data.
    /// </summary>
    /// <param name="eventData">The JSON string representing the event data.</param>
    public Event(string eventData)
    {
        // Parse the JSON string and store it.
        json = JObject.Parse(eventData);
        
        // Set the creation time to the current UTC time.
        created = DateTime.UtcNow;
    }

    /// <summary>
    /// Returns the JSON representation of the event as a string.
    /// </summary>
    /// <returns>A string containing the JSON data.</returns>
    public override string ToString()
    {
        return json!.ToString();
    }

    /// <summary>
    /// Gets the creation time of the event.
    /// </summary>
    public DateTime CreatedTime => created;

    /// <summary>
    /// Determines whether the event is expired based on its lifetime.
    /// </summary>
    public bool IsExpired  
    {
        get
        {
            // An event is considered expired if its lifetime exceeds 65 seconds.
            return Lifetime.TotalSeconds > 65;
        }
    }

    /// <summary>
    /// Gets the time span representing the duration since the event was created.
    /// </summary>
    public TimeSpan Lifetime {
        get
        {
            // Calculate the difference between the current time and the creation time.
            return DateTime.UtcNow.Subtract(CreatedTime);
        }
    }
    
    /// <summary>
    /// Gets the "after" section of the JSON data.
    /// </summary>
    public JToken? After => json!["after"];

    /// <summary>
    /// Gets the ID of the event from the "after" section.
    /// </summary>
    public string? ID => (string?)After!["id"];

    /// <summary>
    /// Gets the type of the event.
    /// </summary>
    public string? Type => (string?)json!["type"];

    /// <summary>
    /// Determines whether the event is of type "new".
    /// </summary>
    public bool IsNew => (Type == "new");

    /// <summary>
    /// Determines whether the event is of type "end".
    /// </summary>
    public bool IsEnd => (Type == "end");

    /// <summary>
    /// Determines whether the event is of type "update".
    /// </summary>
    public bool IsUpdate => (Type == "update");

    /// <summary>
    /// Gets the camera associated with the event from the "after" section.
    /// </summary>
    public string? Camera => (string?)After!["camera"];

    /// <summary>
    /// Gets the label associated with the event from the "after" section.
    /// </summary>
    public string? Label => (string?)After!["label"];

    /// <summary>
    /// Gets the list of current zones from the "after" section.
    /// </summary>
    public string[] CurrentZones => 
        After?["current_zones"]?.Select(e => (string)e).Where(s => s != null).ToArray() ?? Array.Empty<string>();

    /// <summary>
    /// Determines whether the event is stationary based on the "after" section.
    /// </summary>
    public bool? IsStationary => (bool?)After!["stationary"];
}

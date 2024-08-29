using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using Newtonsoft.Json.Linq;

namespace Frigate_Helper;


public class Event
{
    JObject? json;
    DateTime created;
    public Event(string eventData)
    {
        json = JObject.Parse(eventData);
        created = DateTime.UtcNow;
    }
    public override string ToString()
    {
        return json!.ToString();
    }

    public DateTime CreatedTime => created;

    public bool IsExpired  
    {
            get
            {
                if(Lifetime.TotalSeconds > 65)return true;
                else return false;
            }
    }

    public TimeSpan Lifetime {
        get
        {
            TimeSpan difference = DateTime.UtcNow.Subtract(CreatedTime);
            return difference;
        }
    }
    
    public JToken? After => json!["after"];

    public string? ID => (string?)After!["id"];
    public string? Type => (string?)json!["type"];

    public bool IsNew => (Type == "new");
    public bool IsEnd => (Type == "end");
    public bool IsUpdate => (Type == "update");

    public string? Camera => (string?)After!["camera"];
    public string? Label => (string?)After!["label"];

    public string[] CurrentZones {
        get {
            List<string> a = new List<string>();
            var t = After!["current_zones"];

            if(t != null)
            {
                foreach(JToken e in t)
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    string? s = (string)e;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                    if (s != null)
                        a.Add(s);
                }
            }
            return [.. a];
        }
    }
    public bool? IsStationary => (bool?)After!["stationary"];
}
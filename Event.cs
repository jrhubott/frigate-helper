using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Frigate_Helper;


public class Event
{
    JObject? json;
    public Event(string eventData)
    {
        json = JObject.Parse(eventData);
    }
    public override string ToString()
    {
        return json!.ToString();
    }

    
    public JToken? After => json!["after"];

    public string? ID => (string?)After!["id"];
    public string? Type => (string?)json!["type"];

    public bool IsNew => (Type == "new");
    public bool IsEnd => (Type == "end");
    public bool IsUpdate => (Type == "update");

    public string? Camera => (string?)After!["camera"];
    public string? Label => (string?)After!["label"];
    public bool? IsStationary => (bool?)After!["stationary"];
}
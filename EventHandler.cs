using System;
using MQTTnet.Server;

namespace Frigate_Helper;

public class EventHandler
{
    Dictionary<string,Event> events = [];

    public void Add(Event e)
    {
        if(e.ID != null)
        {
            lock(this)
            {
                if(events.Keys.Contains(e.ID))
                {
                    //Update
                    Console.WriteLine("Updating: " + e.ID.ToString());
                    events[e.ID] = e;
                }
                else
                {
                    //Add
                    Console.WriteLine("New: " + e.ID.ToString());
                    events.Add(e.ID,e);
                }
            }
        }
    }

    public void Delete(Event e)
    {
        if(e.ID != null)
        {
            lock(this)
            {
                if(events.Keys.Contains(e.ID))
                {
                    //Delete
                    events.Remove(e.ID);
                }
            }
        }
    }

    public void Handle(Event e)
    {
        if(e.IsEnd)
        {
            Delete(e);
        }
        else
        {
            Add(e);
        }
    }
}

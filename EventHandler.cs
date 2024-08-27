using System;
using System.Security.Cryptography.X509Certificates;
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

        GenerateStatistics();
    }

    private void GenerateStatistics()
    {
        Dictionary<string,Event> tempEvents;

        int moving = 0;
        int stationary = 0;


        //Copy the dictionary for performance and to be non-blocking
        lock(this)
        {
            tempEvents = events.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
        }

        //Loop through it to figure out things
        foreach(var e in tempEvents.Values)
        {
            if(e.IsStationary is not null and true)
                stationary++;
            else
                moving++;
        }

        Console.WriteLine("Moving = {0}, Stationary = {1}",moving,stationary);
    }
}

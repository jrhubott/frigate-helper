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
                    Console.WriteLine("Delete: " + e.ID.ToString());
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

        //Copy the dictionary for performance and to be non-blocking
        lock(this)
        {
            tempEvents = events.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
        }

        Statistic.Clear();

        //Loop through it to figure out things
        foreach(var e in tempEvents.Values)
        {
            //Check if the event is timed out
            if(e.IsExpired)
            {
                Console.WriteLine("Expiring: {0}", e.ID);
                Delete(e);
            }
            else
            {
                //Create the all cameras stat
                Statistic.Update("all",e);

                //Camera statistics
                if(e.Camera is not null)
                    Statistic.Update(e.Camera,e);
            }
        }

        //Refresh All Statistics
        Statistic.RefreshAll();
        Statistic.ConsoleDump();

    }
}

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
                if(events.TryGetValue(e.ID, out Event? last))
                {
                    if(e.IsUpdate == false)
                    {
                        Console.WriteLine("DUPLICATE: {0}", e.ID.ToString());
                    }

                    Console.WriteLine("Updating: {0}, Span: {1}", e.ID.ToString(), last.Lifetime.TotalSeconds.ToString());
                    events[e.ID] = e;
                }
                else
                {
                    if(e.IsUpdate == true)
                    {
                        //Must be the first time this message came in
                        Console.WriteLine("New: {0} from update",e.ID.ToString());
                    }
                    else
                    {
                        Console.WriteLine("New: " + e.ID.ToString());
                    }
                    //Add
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
            Console.WriteLine("End: " + ((e.ID != null) ? e.ID.ToString() : "NA"));
            Delete(e);
        }
        else
        {
            Add(e);
        }

        GenerateStatistics();
    }

    internal void GenerateStatistics()
    {
        Dictionary<string,Event> tempEvents;

        //Copy the dictionary for performance and to be non-blocking
        lock(this)
        {
            tempEvents = events.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
        }

        StatisticHelper.Clear();

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
                StatisticHelper.Update_MoveStationary("all",e);

                if(e.Label is not null)
                {
                    var parentTopic = StatisticHelper.Update_MoveStationary("labels/" + e.Label ,e);
 
                    if(e.Camera is not null)
                    {
                        StatisticHelper.Update_MoveStationary(parentTopic + "cameras/" + e.Camera,e);
                    }

                    foreach(var zone in e.CurrentZones)
                    {       
                        StatisticHelper.Update_MoveStationary(parentTopic  + "zones/" + zone,e);
                    }
                }

                foreach(var zone in e.CurrentZones)
                {
                    var parentTopic = StatisticHelper.Update_MoveStationary("zones/" + zone,e);

                    if(e.Label is not null)
                    {
                        StatisticHelper.Update_MoveStationary(parentTopic + "labels/" + e.Label ,e);
                    }
                }

                //Camera statistics
                if(e.Camera is not null)
                {
                    var parentTopic = StatisticHelper.Update_MoveStationary("cameras/" + e.Camera,e);

                    foreach(var zone in e.CurrentZones)
                    {
                        StatisticHelper.Update_MoveStationary(parentTopic + "zones/" + zone,e);
                    }
                }
            }
        }

        //Refresh All Statistics
        StatisticHelper.RefreshAll();
        StatisticHelper.ConsoleDump();

    }

    internal bool CheckExpired()
    {
        Dictionary<string,Event> tempEvents;
        //Copy the dictionary for performance and to be non-blocking
        lock(this)
        {
            tempEvents = events.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
        }

        int count = 0;

        foreach(var e in tempEvents.Values)
        {
            //Check if the event is timed out
            if(e.IsExpired)
            {
                count++;
            }
        }

        return count > 0;
    }
}

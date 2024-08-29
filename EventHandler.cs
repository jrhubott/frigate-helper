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
                //Create the all cameras stat
                StatisticHelper.Update("all/moving",e, x => {
                    if(x.ev?.IsStationary is not null and false)
                        x.stat++;
                    });
                StatisticHelper.Update("all/stationary",e, x => {
                    if(x.ev?.IsStationary is not null and true)
                        x.stat++;
                    });

                if(e.Label is not null)
                {
                    StatisticHelper.Update("labels/" + e.Label + "/moving",e, x => {if(x.ev?.IsStationary is not null and false)x.stat++;});
                    StatisticHelper.Update("labels/" + e.Label + "/stationary",e, x => {if(x.ev?.IsStationary is not null and true)x.stat++;});    

                    if(e.Camera is not null)
                    {
                        StatisticHelper.Update("labels/" + e.Label + "/cameras/" + e.Camera + "/moving",e, x => {if(x.ev?.IsStationary is not null and false)x.stat++;});
                        StatisticHelper.Update("labels/" + e.Label + "/cameras/" + e.Camera + "/stationary",e, x => {if(x.ev?.IsStationary is not null and true)x.stat++;});
                    }

                    foreach(var zone in e.CurrentZones)
                    {
                        StatisticHelper.Update("labels/" + e.Label + "/zones/" + zone + "/moving",e, x => {if(x.ev?.IsStationary is not null and false)x.stat++;});
                        StatisticHelper.Update("labels/" + e.Label + "/zones/" + zone + "/stationary",e, x => {if(x.ev?.IsStationary is not null and true)x.stat++;}); 
                    }
                }

                foreach(var zone in e.CurrentZones)
                {
                    StatisticHelper.Update("zones/" + zone + "/moving",e, x => {if(x.ev?.IsStationary is not null and false)x.stat++;});
                    StatisticHelper.Update("zones/" + zone + "/stationary",e, x => {if(x.ev?.IsStationary is not null and true)x.stat++;}); 
                }

                //Camera statistics
                if(e.Camera is not null)
                {
                    StatisticHelper.Update("cameras/" + e.Camera + "/moving",e, x => {if(x.ev?.IsStationary is not null and false)x.stat++;});
                    StatisticHelper.Update("cameras/" + e.Camera + "/stationary",e, x => {if(x.ev?.IsStationary is not null and true)x.stat++;});

                    if(e.Label is not null)
                    {
                        StatisticHelper.Update("cameras/" + e.Camera + "/labels/" + e.Label + "/moving",e, x => {if(x.ev?.IsStationary is not null and false)x.stat++;});
                        StatisticHelper.Update("cameras/" + e.Camera + "/labels/" + e.Label + "/stationary",e, x => {if(x.ev?.IsStationary is not null and true)x.stat++;});                                                
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

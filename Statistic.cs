using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Frigate_Helper;

public abstract class Statistic
{
    public delegate void StatisticEventHandler(Statistic s);
    static public event StatisticEventHandler? StatisticReady;

    string name;
    string payload;

    protected List<Event> events = new List<Event>();
    public Statistic(string name)
    {
        this.name = name;
    }

    public string Name => name;

    public string Payload { get => payload; set => payload = value; }

    public void Add(Event e)
    {
        events.Add(e);
    }

    public void ClearEvents()
    {
        events.Clear();
    }

    public abstract void Refresh();
    
    

    readonly static Dictionary<string,Statistic> statistics = [];
    public static Statistic Update(string name, Event? e =null)
    {
        //check if it exists
        statistics.TryGetValue(name, out Statistic? value);
        if(value==null)
        {
            value = new CameraStatistic(name);
            statistics.Add(name,value);
        }

        if(e!=null)value.Add(e);

        return value;
    }

    public static void Clear()
    {
        foreach(var s in statistics)
        {
            s.Value.ClearEvents();
            
        }
       
    }

    public static void RefreshAll()
    {
        foreach(var s in statistics)
        {
            s.Value.Refresh();
            StatisticReady!.Invoke(s.Value);
        }
    }

    public static void ConsoleDump()
    {
        
        foreach(var s in statistics)
        {
            Console.WriteLine(s.Value.ToString());
        }
    }


    
}

public class CameraStatistic(string name) : Statistic(name)
{
    int moving = 0;
    int stationary = 0;

    public int Moving { get => moving; set => moving = value; }
    public int Stationary { get => stationary; set => stationary = value; }


    public override void Refresh()
    {
        moving = 0;
        stationary = 0;

        events.ForEach(x =>
        {
            if(x.IsStationary is not null and true)
                stationary++;
            else
                Moving++;
        });
    }

    public override string ToString()
    {
        return string.Format("    {0} - Moving: {1}, Stationary: {2}",Name, Moving,stationary);
    }
}


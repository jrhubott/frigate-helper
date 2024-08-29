using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Frigate_Helper;


public class Statistic<T>
{
    string topic;

    public struct StatData
    {
        public T stat;
        public Event ev;
    }
    
    T stat;
    T resetValue;
    List<Event> events = new List<Event>();
    Action<StatData> refreshData;

    public T Stat { get => stat; set => stat = value; }
    public string Topic { get => topic; set => topic = value; }

    public Statistic(string topic, T initValue, Action<StatData> refreshData)
    {
        this.topic = topic;
        this.refreshData = refreshData;
        stat = resetValue = initValue;
    }

    public void Refresh()
    {
        stat = resetValue;
        StatData data = new StatData();

        events.ForEach(x =>  
        {
            data.stat = stat;
            data.ev = x;
            refreshData.Invoke(data);
        });

        //Finalize
        stat = data.stat;
    }

     public void Add(Event e)
    {
        events.Add(e);
    }

    public void ClearEvents()
    {
        events.Clear();
    }

    public override string ToString()
    {
        return string.Format("    {0}: {1}",Topic, Stat);
    }
}

public class StatisticHelper
{
    public delegate void StatisticEventHandler(Statistic<int> s);
    static public event StatisticEventHandler? StatisticReady;

    


    readonly static Dictionary<string,Statistic<int>> statistics = [];
    public static Statistic<int> Update(string topic, Event? e, Action<Statistic<int>.StatData> refreshData)
    {
        //check if it exists
        statistics.TryGetValue(topic, out Statistic<int>? value);
        if(value==null)
        {
            value = new Statistic<int>(topic,0,refreshData);
            statistics.Add(topic,value);
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

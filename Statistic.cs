using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Frigate_Helper;

public interface IStatistics
{
    public string Topic {get;}
    public string ToPayload();

    public void Add(Event e);
    public void Refresh();
    public void ClearEvents();
}

public class Statistic<T> : IStatistics
{
    string topic;

    public class StatData
    {
        public T? stat;
        public Event? ev;
    }
    
    T? stat;
    T resetValue;
    List<Event> events = new List<Event>();
    Action<StatData> refreshData;

    public T? Stat { get => stat; set => stat = value; }
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

    public string ToPayload()
    {
        string? payload = Stat?.ToString();
        if(payload != null)
            return payload;
        else
            return "";
    }

    public override string ToString()
    {
        return string.Format("    {0}: {1}",Topic, Stat);
    }
}


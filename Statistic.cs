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
    public bool IsChanged {get;}
}

public class Statistic<T> : IStatistics
{
    string topic;
    bool isChanged = true;
    bool firstRun = true;
    public class StatData
    {
        public T? value;
        public Event? ev;
    }
    
    T? currentValue;
    T? lastValue;    T resetValue;
    List<Event> events = new List<Event>();
    Action<StatData> refreshData;

    public T? Value { get => currentValue; set => currentValue = value; }
    public string Topic { get => topic; set => topic = value; }

    public Statistic(string topic, T initValue, Action<StatData> refreshData)
    {
        this.topic = topic;
        this.refreshData = refreshData;
        currentValue = resetValue = initValue;
    }

    public void Refresh()
    {
        lastValue = currentValue;
        currentValue = resetValue;
        StatData data = new StatData();
        data.value = currentValue;
        events.ForEach(x =>  
        {
            data.ev = x;
            refreshData.Invoke(data);
        });

        //Finalize
        currentValue = data.value;

        if(firstRun)
        {
            firstRun = false;
        }
        else
        {
            isChanged = !EqualityComparer<T>.Default.Equals(currentValue, lastValue);
        }
    }

     public void Add(Event e)
    {
        events.Add(e);
    }

    public void ClearEvents()
    {
        events.Clear();
    }

    public bool IsChanged => isChanged;
    
    public string ToPayload()
    {
        string? payload = Value?.ToString();
        if(payload != null)
            return payload;
        else
            return "";
    }

    public override string ToString()
    {
        return string.Format("    {0}: {1}",Topic, Value);
    }
}


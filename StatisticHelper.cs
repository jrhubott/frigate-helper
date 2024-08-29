using System;

namespace Frigate_Helper;

public static class StatisticHelper
{
    public delegate void StatisticEventHandler(IStatistics s);
    static public event StatisticEventHandler? StatisticReady;

    


    readonly static Dictionary<string,IStatistics> statistics = [];
    public static IStatistics Update<T>(string topic, Event? e, T defaultValue, Action<Statistic<T>.StatData> refreshData)
    {
        //check if it exists
        lock(statistics)
        {
            statistics.TryGetValue(topic, out IStatistics? value);
            if(value==null)
            {
                value = new Statistic<T>(topic,defaultValue,refreshData);
                statistics.Add(topic,value);
        
            }

            if(e!=null)value.Add(e);

            return value;
        }   
    }

      public static void Clear()
    {
        lock(statistics)
        {       
            foreach(var s in statistics)
            {
                s.Value.ClearEvents();            
            }
        }
       
    }

    public static void RefreshAll()
    {
        lock(statistics)
        {
            foreach(var s in statistics)
            {
                s.Value.Refresh();
                StatisticReady!.Invoke(s.Value);
            }
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


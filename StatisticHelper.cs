using System;

namespace Frigate_Helper;

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


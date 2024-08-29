using System;

namespace Frigate_Helper;

public class StatisticHelper
{
    public delegate void StatisticEventHandler(IStatistics s);
    static public event StatisticEventHandler? StatisticReady;

    


    readonly static Dictionary<string,IStatistics> intStatistics = [];
    public static IStatistics Update(string topic, Event? e, Action<Statistic<int>.StatData> refreshData)
    {
        //check if it exists
        intStatistics.TryGetValue(topic, out IStatistics? value);
        if(value==null)
        {
            value = new Statistic<int>(topic,0,refreshData);
            intStatistics.Add(topic,value);
        }

        if(e!=null)value.Add(e);

        return value;
    }

      public static void Clear()
    {
        foreach(var s in intStatistics)
        {
            s.Value.ClearEvents();            
        }
       
    }

    public static void RefreshAll()
    {
        foreach(var s in intStatistics)
        {
            s.Value.Refresh();
            StatisticReady!.Invoke(s.Value);
        }
    }

    public static void ConsoleDump()
    {
        
        foreach(var s in intStatistics)
        {
            Console.WriteLine(s.Value.ToString());
        }
    }
}


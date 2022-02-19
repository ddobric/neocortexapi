using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TimeSeriesSequence.Entity.HelperClasses;

namespace TimeSeriesSequence
{
    public static class HelperMethods
    {
        public static List<Slot> GetSlots()
        {
            List<Slot> timeSlots = new List<Slot>
            {
               new Slot { Segment="00", StartTime=new TimeSpan(0,0,0), EndTime= new TimeSpan(0,59,59) },
               new Slot { Segment="01", StartTime=new TimeSpan(1,0,0), EndTime= new TimeSpan(1,59,59) },
               new Slot { Segment="02", StartTime=new TimeSpan(2,0,0), EndTime= new TimeSpan(2,59,59) },
               new Slot { Segment="03", StartTime=new TimeSpan(3,0,0), EndTime= new TimeSpan(3,59,59) },
               new Slot { Segment="04", StartTime=new TimeSpan(4,0,0), EndTime= new TimeSpan(4,59,59) },
               new Slot { Segment="05", StartTime=new TimeSpan(5,0,0), EndTime= new TimeSpan(5,59,59) },
               new Slot { Segment="06", StartTime=new TimeSpan(6,0,0), EndTime= new TimeSpan(6,59,59) },
               new Slot { Segment="07", StartTime=new TimeSpan(7,0,0), EndTime= new TimeSpan(7,59,59) },
               new Slot { Segment="08", StartTime=new TimeSpan(8,0,0), EndTime= new TimeSpan(8,59,59) },
               new Slot { Segment="09", StartTime=new TimeSpan(9,0,0), EndTime= new TimeSpan(9,59,59) },
               new Slot { Segment="10", StartTime=new TimeSpan(10,0,0), EndTime= new TimeSpan(10,59,59) },
               new Slot { Segment="11", StartTime=new TimeSpan(11,0,0), EndTime= new TimeSpan(11,59,59) },
               new Slot { Segment="12", StartTime=new TimeSpan(12,0,0), EndTime= new TimeSpan(12,59,59) },
               new Slot { Segment="13", StartTime=new TimeSpan(13,0,0), EndTime= new TimeSpan(13,59,59) },
               new Slot { Segment="14", StartTime=new TimeSpan(14,0,0), EndTime= new TimeSpan(14,59,59) },
               new Slot { Segment="15", StartTime=new TimeSpan(15,0,0), EndTime= new TimeSpan(15,59,59) },
               new Slot { Segment="16", StartTime=new TimeSpan(16,0,0), EndTime= new TimeSpan(16,59,59) },
               new Slot { Segment="17", StartTime=new TimeSpan(17,0,0), EndTime= new TimeSpan(17,59,59) },
               new Slot { Segment="18", StartTime=new TimeSpan(18,0,0), EndTime= new TimeSpan(18,59,59) },
               new Slot { Segment="19", StartTime=new TimeSpan(19,0,0), EndTime= new TimeSpan(19,59,59) },
               new Slot { Segment="20", StartTime=new TimeSpan(20,0,0), EndTime= new TimeSpan(20,59,59) },
               new Slot { Segment="21", StartTime=new TimeSpan(21,0,0), EndTime= new TimeSpan(21,59,59) },
               new Slot { Segment="22", StartTime=new TimeSpan(22,0,0), EndTime= new TimeSpan(22,59,59) },
               new Slot { Segment="23", StartTime=new TimeSpan(23,0,0), EndTime= new TimeSpan(23,59,59) },
            };

            return timeSlots;
        }  
    }
}

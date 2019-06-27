using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public interface IHtmModule
    {
        string Name { get; set; }
   
    }

    /// <summary>
    /// Defines the interface, which needs to be implemented by all classes, which can be inserted as a layer in region.
    /// </summary>
    public interface IHtmModule<TIN, TOUT> : IHtmModule
    {
     

        TOUT Compute(TIN input, bool learn);

        //IModuleData Output { get; }

        //IModuleData Input { get; }
    }

  
    public interface IModuleData
    {

    }

    //public interface IIntegerArrayData : IModuleData
    //{
    //    int[] Data { get; }
    //}




    //public class IntegerArrayOutput : IIntegerArrayData
    //{
    //    public int[] Output;

    //    public IntegerArrayOutput(int[] array)
    //    {
    //        this.Output = array;
    //    }

      
    //    public int[] Data { get { return this.Output; } }

 
    //}
}

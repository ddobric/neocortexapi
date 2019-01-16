using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class Region
    {
        #region Private Members
        private ILogger logger;

        private Dictionary<String, Layer<IInference>> layers = new Dictionary<string, Layer<IInference>>();
               
        private Region upstreamRegion;
        private Network parentNetwork;
        private Region downstreamRegion;
        private Layer<IInference> tail;
        private Layer<IInference> head;

        private Object input;

        /** Marker flag to indicate that assembly is finished and Region initialized */
        private bool assemblyClosed;
        
        private String Name;
        #endregion

        #region Constructors and Initialization
        public Region(String name, Network network)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name may not be null or empty. " +
                    "...not that anyone here advocates name calling!");
            }

            this.Name = name;
            this.parentNetwork = network;
        }
        #endregion

        public Region getUpstreamRegion()
        {
            return upstreamRegion;
        }

        public Region close()
        {
            if (layers.Count < 1)
            {
                logger.LogWarning("Closing region: " + Name + " before adding contents.");
                return this;
            }

            completeAssembly();

            Layer<IInference>  layer = tail;
            do
            {
                layer.close();
            } while ((layer = layer.getNext()) != null);

            return this;
        }

        public bool start()
        {
            if (!assemblyClosed)
            {
                close();
            }

            if (tail.hasSensor())
            {
                logger.LogInformation("Starting Region [" + this.Name + "] input Layer thread.");
                tail.start();
                return true;
            }
            else
            {
                
                logger.LogWarning("Start called on Region [" + this.Name + "] with no effect due to no Sensor present.");
            }

            return false;
        }

        /**
   * Connects the output of the specified {@code Region} to the 
   * input of this Region
   * 
   * @param inputRegion   the Region who's emissions will be observed by 
   *                      this Region.
   * @return
   */
        Region connect(Region inputRegion)
        {
            inputRegion.observe().subscribe(new IObserver<IInference>() {
            ManualInput localInf = new ManualInput();

            @Override public void onCompleted()
            {
                tail.notifyComplete();
            }
            @Override public void onError(Throwable e) { e.printStackTrace(); }
            @SuppressWarnings("unchecked")
                @Override public void onNext(Inference i)
            {
                localInf.sdr(i.getSDR()).recordNum(i.getRecordNum()).classifierInput(i.getClassifierInput()).layerInput(i.getSDR());
                if (i.getSDR().length > 0)
                {
                    ((Layer<Inference>)tail).compute(localInf);
                }
            }
        });
        // Set the upstream region
        this.upstreamRegion = inputRegion;
        inputRegion.downstreamRegion = this;
        
        return this;
    }
}
}

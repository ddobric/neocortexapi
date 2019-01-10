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
        
        private String name;
        #endregion

        #region Constructors and Initialization
        public Region(String name, Network network)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name may not be null or empty. " +
                    "...not that anyone here advocates name calling!");
            }

            this.name = name;
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
                logger.LogWarning("Closing region: " + name + " before adding contents.");
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
                logger.LogInformation("Starting Region [" + this.name + "] input Layer thread.");
                tail.start();
                return true;
            }
            else
            {
                
                LOGGER.warn("Start called on Region [" + getName() + "] with no effect due to no Sensor present.");
            }

            return false;
        }
    }
}

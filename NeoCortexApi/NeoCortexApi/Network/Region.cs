using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.
{
    public class Region
    {
        #region Private Members

        private Dictionary<String, Layer<Inference>> layers = new HashMap<>();

        private ILogger logger;

        private Region upstreamRegion;
        private Network parentNetwork;
        private Region downstreamRegion;
        private Layer tail;
        private Layer head;

        private Object input;

        /** Marker flag to indicate that assembly is finished and Region initialized */
        private bool assemblyClosed;
        
        private String name;
        #endregion

        public Region getUpstreamRegion()
        {
            return upstreamRegion;
        }

        public Region close()
        {
            if (layers.size() < 1)
            {
                logger.LogWarning("Closing region: " + name + " before adding contents.");
                return this;
            }

            completeAssembly();

            Layer  l = tail;
            do
            {
                l.close();
            } while ((l = l.getNext()) != null);

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

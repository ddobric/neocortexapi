using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace NeoCortexApi
{
    public class Region //where T : IInference
    {
        #region Private Members
        private ILogger logger;

        private Region upstreamRegion;
        private CortexNetwork parentNetwork;
        private Region downstreamRegion;

        /** Stores the overlap of algorithms state for {@link Inference} sharing determination */
        byte flagAccumulator = 0;
        /** 
         * Indicates whether algorithms are repeated, if true then no, if false then yes
         * (for {@link Inference} sharing determination) see {@link Region#configureConnection(Layer, Layer)} 
         * and {@link Layer#getMask()}
         */
        bool layersDistinct = true;

        /// <summary>
        /// All layers.
        /// </summary>
        private Dictionary<String, Layer<IInference>> layers = new Dictionary<string, Layer<IInference>>();

        /// <summary>
        /// The lowest layer with no input.
        /// </summary>
        private Layer<IInference> tail;

        /// <summary>
        /// The highest layer with no output.
        /// </summary>
        private Layer<IInference> head;

        /// <summary>
        /// All OUTPUT layers.
        /// </summary>
        private List<Layer<IInference>> sources;

        /// <summary>
        /// All INPUT layers.
        /// </summary>
        private List<Layer<IInference>> sinks;

        private Object input;

        /** Marker flag to indicate that assembly is finished and Region initialized */
        private bool assemblyClosed;

        private String Name;
        #endregion

        #region Constructors and Initialization
        public Region(String name, CortexNetwork network)
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


        /**
    * Connects two layers to each other in a unidirectional fashion 
    * with "toLayerName" representing the receiver or "sink" and "fromLayerName"
    * representing the sender or "source".
    * 
    * This method also forwards shared constructs up the connection chain
    * such as any {@link Encoder} which may exist, and the {@link Inference} result
    * container which is shared among layers.
    * 
    * @param toLayerName       the name of the sink layer
    * @param fromLayerName     the name of the source layer
    * @return
    * @throws IllegalStateException if Region is already closed
    */

        public Region connect(String toLayerName, String fromLayerName)
        {
            if (assemblyClosed)
            {
                throw new OperationCanceledException("Cannot connect Layers when Region has already been closed.");
            }

            Layer<IInference> inputLayer = (Layer<IInference>)this.layers.FirstOrDefault(kp => kp.Key == toLayerName).Value;
            Layer<IInference> outputLayer = (Layer<IInference>)this.layers.FirstOrDefault(kp => kp.Key == fromLayerName).Value;
            if (inputLayer == null)
            {
                throw new ArgumentException("Could not lookup (to) Layer with name: " + toLayerName);
            }
            else if (outputLayer == null)
            {
                throw new ArgumentException("Could not lookup (from) Layer with name: " + fromLayerName);
            }

            outputLayer.NextLayer = inputLayer;

            inputLayer.PreviousLayer = outputLayer;

            // Connect out to in
            configureConnection(inputLayer, outputLayer);

            connect(inputLayer, outputLayer);

            return this;
        }

            /**
      * Used to manually input data into a {@link Region}, the other way 
      * being the call to {@link Region#start()} for a Region that contains
      * a {@link Layer} which in turn contains a {@link Sensor} <em>-OR-</em>
      * subscribing a receiving Region to this Region's output Observable.
      * 
      * @param input One of (int[], String[], {@link ManualInput}, or Map&lt;String, Object&gt;)
      */
      
    public void Compute(IInference input)
        {
            if (!assemblyClosed)
            {
                close();
            }

            this.input = input;

            ((Layer<IInference>)tail).Compute(input);
        }

        /// <summary>
        /// Configures connection between two layers, by filling lists of all 
        /// output (source) and input (sink) layers.
        /// </summary>
        /// <typeparam name="TIN">Input layer instance.</typeparam>
        /// <typeparam name="TOUT">Output layer instance.</typeparam>
        /// <param name="input">Input (sink) layer.</param>
        /// <param name="output">Output (source) layer.</param>
        private void configureConnection<TIN, TOUT>(TIN input, TOUT output)
                where TIN : Layer<IInference>
                where TOUT : Layer<IInference>
        {
            if (assemblyClosed)
            {
                throw new ArgumentException("Cannot add Layers when Region has already been closed.");
            }

            List<Layer<IInference>> allLayers = new List<Layer<IInference>>();
            allLayers.AddRange(sources);
            allLayers.AddRange(sinks);

            //byte inMask = in.getMask();
            //byte outMask = out.getMask();

            output.mas
            if (!allLayers.Contains(output))
            {
                layersDistinct = (flagAccumulator & outMask) < 1;
                flagAccumulator |= outMask;
            }

            if (!allLayers.contains(input))
            //{
            //    layersDistinct = (flagAccumulator & inMask) < 1;
            //    flagAccumulator |= inMask;
            //}

            sources.Add(output);
            sinks.Add(input);
        }


        private class ObservableRegHelper : IObserver<IInference>
        {
            private Layer<IInference> inputLayer;

            public ObservableRegHelper(Layer<IInference> inputLayer)
            {
                this.inputLayer = inputLayer;
            }

            public void OnCompleted()
            {
                inputLayer.NotifyComplete();
            }

            public void OnError(Exception error)
            {
                throw error;
            }

            /// <summary>
            /// Invoked after layer has completed calculation.
            /// </summary>
            /// <param name="inference"></param>
            public void OnNext(IInference inference)
            {
             

                if (layersDistinct)
                {
                    this.inputLayer.compute(inference);
                }
                else
                {
                    ManualInput newInf = new ManualInput();
                    newInf.Sdr = inference.Sdr;
                    newInf.RecordNum = inference.RecordNum;
                    newInf.LayerInput = inference.Sdr;
                    this.inputLayer.compute(newInf);
                }
            }
        }

        /**
 * Called internally to actually connect two {@link Layer} 
 * {@link Observable}s taking care of other connection details such as passing
 * the inference up the chain and any possible encoder.
 * 
 * @param in         the sink end of the connection between two layers
 * @param out        the source end of the connection between two layers
 * @throws IllegalStateException if Region is already closed 
 */
        private void connect<TIN, TOUT>(TIN inputLayer, TOUT outputLayer)
              where TIN : Layer<IInference>
              where TOUT : Layer<IInference>
        {
            var observable = new ObservableRegHelper(inputLayer);
            outputLayer.Subscribe(observable);

            //    output.Subscribe(new Subscriber<Inference>() {
            //    ManualInput localInf = new ManualInput();

            //    @Override public void onCompleted() { in.notifyComplete(); }
            //    @Override public void onError(Throwable e) { e.printStackTrace(); }
            //    @Override public void onNext(Inference i)
            //    {
            //        if (layersDistinct)
            //        {
            //            in.compute(i);
            //        }
            //        else
            //        {
            //            localInf.sdr(i.getSDR()).recordNum(i.getRecordNum()).layerInput(i.getSDR());
            //            in.compute(localInf);
            //        }
            //    }
            //});
        }



        /**
    * Called by {@link #start()}, {@link #observe()} and {@link #connect(Region)}
    * to finalize the internal chain of {@link Layer}s contained by this {@code Region}.
    * This method assigns the head and tail Layers and composes the {@link Observable}
    * which offers this Region's emissions to any upstream {@link Region}s.
*/
        private void completeAssembly()
        {
            if (!assemblyClosed)
            {
                if (layers.Count == 0) return;

                if (layers.Count == 1)
                {
                    head = tail = layers.Values.First();
                }

                if (tail == null)
                {
                    List<Layer<IInference>> temp = new List<Layer<IInference>>(sources);
                    temp.RemoveAll(el => sinks.Contains(el));
                    if (temp.Count != 1)
                    {
                        throw new ArgumentException("Detected misconfigured Region too many or too few sinks.");
                    }

                    // Tail is lowest layer (sensor), which has no input from other layer. 
                    tail = temp.First();
                }

                if (head == null)
                {
                    List<Layer<IInference>> temp = new List<Layer<IInference>>(sinks);
                    temp.RemoveAll(el => sources.Contains(el));
                    if (temp.Count != 1)
                    {
                        throw new ArgumentException("Detected misconfigured Region too many or too few sources.");
                    }

                    // Head is most top layer, which has no output.
                    head = temp.First();
                }

                regionObservable = head.observe();

                assemblyClosed = true;
            }
        }
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

            Layer<IInference> layer = tail;
            do
            {
                layer.CloseInit();
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

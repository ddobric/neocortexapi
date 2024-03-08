Global inhibition
MaxBoost 5.0
MIN_PCT_OVERLAP_DUTY_CYCLES = 1.0
After 300 cycles boost disabled

NewBorn effect.
if (cycle >= 300)
                {
                    mem.setMaxBoost(0.0);
                    mem.updateMinPctOverlapDutyCycles(0.0);
                }

Result for all inputs stable after 300 cycles.
----------------------------------------------------------------------------

 double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;
            int inputBits = 100;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();
            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 10);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });

            p.Set(KEY.MAX_BOOST, maxBoost);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, minOctOverlapCycles);

            // Local inhibition
            // Stops the bumping of inactive columns.
            //p.Set(KEY.IS_BUMPUP_WEAKCOLUMNS_DISABLED, true); Obsolete.use KEY.MIN_PCT_OVERLAP_DUTY_CYCLES = 0;
            //p.Set(KEY.POTENTIAL_RADIUS, 50);
            //p.Set(KEY.GLOBAL_INHIBITION, false);
            //p.setInhibitionRadius(15);

            // Global inhibition
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            //p.Set(KEY.POTENTIAL_RADIUS, inputBits);
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.
            //p.setInhibitionRadius( Automatically set on the columns pace in a case of global inhibition.);

            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));
            double max = 20;

            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            EncoderBase encoder = new ScalarEncoder(settings);

            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 , 11.0, 12.});

            RunSpStabilityExperiment2(maxBoost, minOctOverlapCycles, inputBits, p, encoder, inputValues);
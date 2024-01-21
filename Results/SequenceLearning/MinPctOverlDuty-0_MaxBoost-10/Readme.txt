In this experiment I used MinPctOverlDuty-0 and MaxBoost-10.
After 100 cycles, I switched to MaxBoost-0.
System get stable in approx. 10 min.


            int inputBits = 100;
            int numColumns = 2048;
            Parameters p = Parameters.getAllDefaultParameters();

            p.Set(KEY.RANDOM, new ThreadSafeRandom(42));
            p.Set(KEY.INPUT_DIMENSIONS, new int[] { inputBits });
            p.Set(KEY.CELLS_PER_COLUMN, 15);
            p.Set(KEY.COLUMN_DIMENSIONS, new int[] { numColumns });


            //p.Set(KEY.POTENTIAL_RADIUS, 50);
            //p.Set(KEY.GLOBAL_INHIBITION, false);

            p.Set(KEY.GLOBAL_INHIBITION, true);
            p.Set(KEY.LOCAL_AREA_DENSITY, -1); // In a case of global inhibition.

            //p.setNumActiveColumnsPerInhArea(10);
            // N of 40 (40= 0.02*2048 columns) active cells required to activate the segment.
            p.setNumActiveColumnsPerInhArea(0.02 * numColumns);
            // Activation threshold is 10 active cells of 40 cells in inhibition area.
            p.setActivationThreshold(10);
            p.setInhibitionRadius(15);

            //
            // Stops the bumping of inactive columns.
            p.Set(KEY.MAX_BOOST, 10.0);
            p.Set(KEY.DUTY_CYCLE_PERIOD, 100000);
            p.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.0);

            // Max number of synapses on the segment.
            p.setMaxNewSynapsesPerSegmentCount((int)(0.02 * numColumns));
            p.setPermanenceIncrement(0.15);
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

            //List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 2.0, 0.0, 1.0, 2.0, 0.0, 1.0, 2.0, 2.0, 0.0, 0.1, 2.0 });
            List<double> inputValues = new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0 });
         

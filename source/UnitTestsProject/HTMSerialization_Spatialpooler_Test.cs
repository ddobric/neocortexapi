using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexEntities.NeuroVisualizer;

namespace UnitTestsProject
{
    
    
    public class SpatialPooler

    {
        public static Parameters GetDefaultParams()
        {
            Random rnd = new Random(42);

            var parameters = Parameters.getAllDefaultParameters();
            parameters.Set(KEY.POTENTIAL_RADIUS, 10);
            parameters.Set(KEY.POTENTIAL_PCT, 0.75);
            parameters.Set(KEY.GLOBAL_INHIBITION, false);
            parameters.Set(KEY.LOCAL_AREA_DENSITY, -1.0);
            parameters.Set(KEY.NUM_ACTIVE_COLUMNS_PER_INH_AREA, 80.0);
            parameters.Set(KEY.STIMULUS_THRESHOLD, 0);
            parameters.Set(KEY.SYN_PERM_INACTIVE_DEC, 0.01);
            parameters.Set(KEY.SYN_PERM_ACTIVE_INC, 0.1);
            parameters.Set(KEY.SYN_PERM_CONNECTED, 0.1);
            parameters.Set(KEY.MIN_PCT_OVERLAP_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.MIN_PCT_ACTIVE_DUTY_CYCLES, 0.001);
            parameters.Set(KEY.WRAP_AROUND, true);
            parameters.Set(KEY.DUTY_CYCLE_PERIOD, 10);
            parameters.Set(KEY.MAX_BOOST, 1.0);
            parameters.Set(KEY.RANDOM, rnd);

            return parameters;
        }

        // Creates an empty pooler.
        SpatialPooler sp = new SpatialPooler();

        // Creates the cell-space
        Connections mem = new Connections();
        private HomeostaticPlasticityController homeostaticPlasticityActivator;

        

        public SpatialPooler(HomeostaticPlasticityController homeostaticPlasticityActivator)
        {
        }

        public SpatialPooler()
        {
        }

        public void SerializeSpatialPooler()
        {
            HomeostaticPlasticityController homeostaticPlasticityActivator = new HomeostaticPlasticityController();
            SpatialPooler spatial = new SpatialPooler(homeostaticPlasticityActivator);

            using (StreamWriter sw = new StreamWriter($"ser_{nameof(SerializeSpatialPooler)}.txt"))
            {
                spatial.SerializeSpatialPooler(sw);
            }

        }

        private void SerializeSpatialPooler(StreamWriter sw)
        {
            throw new NotImplementedException();
        }

        internal void Serialize(StreamWriter sw)
        {
            throw new NotImplementedException();
        }
    }

} 

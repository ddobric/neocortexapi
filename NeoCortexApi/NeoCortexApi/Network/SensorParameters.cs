using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class SensorParameters
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        public ICollection<string> Keys { get; set; }

        public object this[string key]
        {
            get
            {
                return parameters[key];
            }

            set
            {
                parameters[key] = value;
            }
        }

        public override int GetHashCode()
        {
            const int prime = 31;

            int result = 1;

            foreach (var item in this.parameters)
            {
                result = prime * result + item.Value.GetHashCode();
            }

            return result;
        }
    }
}

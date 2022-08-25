// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Entities
{
    public class DistributedMemory
    {
        public IDistributedDictionary<int, Column> ColumnDictionary { get; set; }

        //public IDistributedDictionary<int, Pool> PoolDictionary { get; set; }

        // override object.Equals
        public override bool Equals(object obj)
        {
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var dm = obj as DistributedMemory;
            
            return this.Equals(dm);
        }

        public bool Equals(DistributedMemory obj)
        {
            if (ReferenceEquals(this, obj))            
                return true;
            
            if(obj == null)            
                return false;
            
            return this.ColumnDictionary.Equals(obj.ColumnDictionary);
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((ColumnDictionary == null) ? 0 : ColumnDictionary.GetHashCode());
            return result;
        }

    }
}

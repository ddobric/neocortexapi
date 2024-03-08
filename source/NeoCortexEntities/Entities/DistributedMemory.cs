// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace NeoCortexApi.Entities
{
    public class DistributedMemory
    {
        public IDistributedDictionary<int, Column> ColumnDictionary { get; set; }

        //public IDistributedDictionary<int, Pool> PoolDictionary { get; set; }
    }
}

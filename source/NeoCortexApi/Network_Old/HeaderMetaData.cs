// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Encoders;
using System.Collections.Generic;

namespace NeoCortexApi.Network
{
    public class HeaderMetaData
    {
        public List<FieldMetaType> FieldMetaData { get; set; }

        public List<string> FieldNames { get; set; }

        public List<string> SensorFlags { get; set; }
    }
}

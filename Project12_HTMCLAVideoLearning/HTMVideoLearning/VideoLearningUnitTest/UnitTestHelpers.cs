// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Extensions.Logging;
using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;

namespace VideoLearningUnitTest
{
    public class UnitTestHelpers
    {

        ///// <summary>
        ///// Creates pooler instance.
        ///// </summary>
        ///// <param name="poolerMode"></param>
        ///// <returns></returns>
        //public static SpatialPooler CreatePooler(PoolerMode poolerMode)
        //{
        //    SpatialPooler sp;

        //    if (poolerMode == PoolerMode.Multinode)
        //        sp = new SpatialPoolerParallel();
        //    else if (poolerMode == PoolerMode.Multicore)
        //        sp = new SpatialPoolerMT();
        //    else
        //        sp = new SpatialPooler();

        //    return sp;
        //}
    }


    public enum PoolerMode
    {
        SingleThreaded,

        Multicore,

        Multinode
    }
}

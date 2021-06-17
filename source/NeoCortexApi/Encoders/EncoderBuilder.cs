// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Encoders
{
    public abstract class EncoderBuilder<TBuilder,TEncoder, T>
    {
            protected int n;
            protected int w;
            protected double minVal;
            protected double maxVal;
            protected double radius;
            protected double resolution;
            protected bool periodic;
            protected bool clipInput;
            protected bool forced;
            protected String name;

            protected EncoderBase<T> encoder;

            public TEncoder build()
            {
                if (encoder == null)
                {
                    throw new InvalidOperationException("Subclass did not instantiate builder type " +
                        "before calling this method!");
                }

                encoder.N = this.n;
                encoder.W = w;
                encoder.MinVal=minVal;
                encoder.MaxVal=maxVal;
                encoder.Radius=radius;
                encoder.Resolution=resolution;
                encoder.Periodic=periodic;
                encoder.ClipInput=clipInput;
                encoder.Forced=forced;
                encoder.Name=name;

                return (E)encoder;
            }

            public TBuilder FromN(int n)
            {
                this.n = n;
                return (TBuilder)this;
            }
            public TBuilder FromW(int w)
            {
                this.w = w;
                return (TBuilder)this;
            }
            public TBuilder minVal(double minVal)
            {
                this.minVal = minVal;
                return (TBuilder)this;
            }
            public TBuilder maxVal(double maxVal)
            {
                this.maxVal = maxVal;
                return (TBuilder)this;
            }
            public TBuilder radius(double radius)
            {
                this.radius = radius;
                return (TBuilder)this;
            }
            public TBuilder resolution(double resolution)
            {
                this.resolution = resolution;
                return (TBuilder)this;
            }
            public TBuilder periodic(bool periodic)
            {
                this.periodic = periodic;
                return (TBuilder)this;
            }
            public TBuilder clipInput(bool clipInput)
            {
                this.clipInput = clipInput;
                return (TBuilder)this;
            }
            public TBuilder forced(bool forced)
            {
                this.forced = forced;
                return (TBuilder)this;
            }
            public TBuilder name(String name)
            {
                this.name = name;
                return (TBuilder)this;
            }
        }
    
}


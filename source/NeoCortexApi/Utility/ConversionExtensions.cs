// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Globalization;

namespace NeoCortexApi.Utility
{
    public static class ConversionExtensions
    {

        /// <summary>
        /// Converting a value to given type
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="t">expected type</param>
        /// <returns>converted object</returns>
        public static object Convert(this object value, Type t)
        {
            Type underlyingType = Nullable.GetUnderlyingType(t);

            if (underlyingType != null && value == null)
            {
                return null;
            }
            Type basetype = underlyingType == null ? t : underlyingType;
            return System.Convert.ChangeType(value, basetype, new CultureInfo("en-us").NumberFormat);
        }

        /// <summary>
        /// Converting a value to given type
        /// </summary>
        /// <typeparam name="T">expected type</typeparam>
        /// <param name="value">value</param>
        /// <returns>converted object</returns>
        public static T Convert<T>(this object value)
        {
            return (T)value.Convert(typeof(T));
        }
    }
}
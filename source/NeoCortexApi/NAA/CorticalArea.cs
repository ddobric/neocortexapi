// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using NeoCortexApi.DataMappers;
using NeoCortexApi.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi
{
    /// <summary>
    /// Cortical column that consists of cells. It does not contain mini-columns.
    /// </summary>
    public class CorticalArea
    {
        protected ConcurrentDictionary<int, Segment> _segmentMap = new ConcurrentDictionary<int, Segment>();

        /// <summary>
        /// Number of cells in the _area.
        /// </summary>
        private int _numCells;

        /// <summary>
        /// Map of active cells and their indexes in the virtual sparse array.
        /// </summary>
        private ConcurrentDictionary<long, Cell> _currActiveCells = new ConcurrentDictionary<long, Cell>();

        /// <summary>
        /// Sparse map of cells that have been involved in learning. Their indexes in the virtual sparse array.
        /// </summary>
        private ConcurrentDictionary<long, Cell> _allCellsSparse = new ConcurrentDictionary<long, Cell>();

        /// <summary>
        /// The name of the _area. It must be unique in the application.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get the list of active cells from indicies.
        /// </summary>
        public IList<Cell> ActiveCells
        {
            get
            {
                var actCells = _currActiveCells.Values;

                return actCells.ToList();
            }
        }

        public long[] ActiveCellsIndicies
        {
            get
            {
                return _currActiveCells.Keys.ToArray();
            }
            set
            {
                _currActiveCells = new ConcurrentDictionary<long, Cell>();

                foreach (var cellIndex in value)
                {
                    Cell cell;

                    if (!_allCellsSparse.TryGetValue(cellIndex, out cell))
                    {
                        cell = new Cell(CreateIdFromString(Name), (int)cellIndex);

                        _allCellsSparse.TryAdd(cellIndex, cell);                       
                    }

                    _currActiveCells.TryAdd(cellIndex, cell);                    
                }
            }
        }


        /// <summary>
        /// Gets the number of outgoing synapses of all active cells.
        /// </summary>
        public int NumOutgoingSynapses
        {
            get
            {
                return this.ActiveCells.SelectMany(el => el.ReceptorSynapses).Count();
            }
        }

        /// <summary>
        /// Gets the number of incoming synapses at apical segments.
        /// </summary>
        public int NumIncomingApicalSynapses
        {
            get
            {
                return this.ActiveCells.SelectMany(cell => cell.ApicalDendrites).SelectMany(aSeg => aSeg.Synapses).Count();                
            }
        }

        public CorticalArea(int index, string name, int numCells)
        {
            this.Name = name;

            this._numCells = numCells;

            _currActiveCells = new ConcurrentDictionary<long, Cell>();
        }

        public override string ToString()
        {
            return $"{Name} - Cells: {_numCells} - Active Cells : {_currActiveCells.Count}";
        }

        /// <summary>
        /// Creates the ID from the string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int CreateIdFromString(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the hash value to an integer
            int numericId = BitConverter.ToInt32(hashBytes, 0);

            return numericId;
        }
    }
}

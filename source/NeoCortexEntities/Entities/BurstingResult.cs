// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System.Collections.Generic;

namespace NeoCortexApi.Entities
{

    /// <summary>
    /// Helper entity for column bursting.
    /// </summary>
    public class BurstingResult
    {

        public BurstingResult(IList<Cell> cells, Cell bestCell)
        {
            this.Cells = cells;
            this.BestCell = bestCell;
        }

        /// <summary>
        ///  List of the processed column's cells.
        /// </summary>
        public IList<Cell> Cells { get; set; }


        /// <summary>
        /// Chosen as the active cell.
        /// </summary>
        public Cell BestCell { get; set; }

        
        public override bool Equals(object obj)
        {
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var br = obj as BurstingResult;
            
            return this.Equals(br);
        }

        public bool Equals(BurstingResult obj)
        {
            if (ReferenceEquals(this, obj))            
               return true;

            if (obj == null)
                return false;

            if (Cells == null)
            {
                if (obj.Cells != null)
                    return false;
            }

            if (BestCell == null)
            {
                if (obj.BestCell != null)
                    return false;
            }

            if (obj.Cells.ElementsEqual(Cells) && obj.BestCell.Equals(BestCell))
                return true;
            else
                return false;            

        }

        
        public override int GetHashCode()
        {
            // TODO: write implementation of GetHashCode() here
            throw new System.NotImplementedException();
            return base.GetHashCode();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WinSudoku
{
    /// <summary>
    ///  An n times n matrix with entries from 0 to n-1 so that each entry occurs exactly once in each row and column. This class may 
    /// represent and complete partial latin squares.
    /// </summary>
    public class LatinSquare
    {

        /**
         * Allowed or determined values in each cell
         */
        internal NumberSet[][] entries;

        /**
         * Entries which are determined logically while setting other entries.
         */
        internal HashSet<Entry> pendingFindings = new HashSet<Entry>();

        /// <summary>
        /// Looking at a partial latin square as a mathematician, it appears to be a set of triples from {1..n}x{1..n}x{1..n} where no two 
        /// triples equal in exactly 2 components.The actual matrix is only a representation of it choosing one component as entry and           
        /// the other two as row and column index.There are obviously 3 such matrices (if rows and colums are interchangeable), this attribute
        /// is the one where the original column index plays the role of the row index, the original entry values become column indices and the original
        /// rows become entries.
        /// </summary>
        internal LatinSquare nextTransposition;

        /**
         * Call create(int) for new instance!
         */
        protected LatinSquare(int size)
        {
            entries = new NumberSet[size][];
            for (int i = 0; i < size; i++)
            {
                entries[i] = new NumberSet[size];
                for (int j = 0; j < size; j++)
                {
                    entries[i][j] = new NumberSet(size);
                }
            }
        }

        /**
         * Not to be used except in inheriting classes. Protected would not permit call from static methods.
        */
        internal LatinSquare(LatinSquare nt) : this(nt.entries.Length)
        {
            nextTransposition = nt;
        }

        /**
         * Creates an instance linked with its two alternative forms.
         */
        public static LatinSquare create(int size)
        {
            LatinSquare result = new LatinSquare(size);
            result.nextTransposition = new LatinSquare(new LatinSquare(result));
            return result;
        }

        public virtual LatinSquare CreateCopy()
        {            
            LatinSquare result = new LatinSquare(entries, this);
            LatinSquare nnt = new LatinSquare(nextTransposition.nextTransposition.entries, result);
            LatinSquare nt = new LatinSquare(nextTransposition.entries, nnt);
            result.nextTransposition = nt;
            return result;
        }

        internal LatinSquare(NumberSet[][] originalEntries, LatinSquare nt )
        {
            entries = CloneEntries(originalEntries);
            nextTransposition = nt;
        }

        internal static NumberSet[][] CloneEntries(NumberSet[][] original)
        {            
            NumberSet[][] clone = new NumberSet[original.Length][];
            for (int i = 0; i < original.Length; i++)
            {
                clone[i] = new NumberSet[original.Length];
                for (int j = 0; j < original.Length; j++)
                {
                    clone[i][j] = new NumberSet(original[i][j]);
                }
            }
            return clone;
        }

        /**
         * Specifies an entry value which might have various implications abaut the other entries.
         */
        public virtual void SetEntry(int row, int col, int num)
        {
            if (entries[row][col].SetValue(num) == NumberSet.ALREADYKNOWN)
            {
                return;
            }
            nextTransposition.SetEntry(col, num, row);
            for (int i = 0; i < entries.Length; i++)
            {
                if (i != row)
                {
                    forbidValue(i, col, num);
                }
                if (i != col)
                {
                    forbidValue(row, i, num);
                }
            }
        }

        /// <summary>
        /// Returns the entry in specified cell or OK when unknown
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public int GetEntry(int row, int col)
        {
            return entries[row][col].getValue();
        }


        /// <summary>
        /// Enter all numbers which have been been derived from entries made so far.
        /// </summary>
        public void AddFindings()
        {
            while (AddOwnFindings() || nextTransposition.AddOwnFindings() || nextTransposition.nextTransposition.AddOwnFindings())
            {
                // just repeat
            }
        }

        private String toString(NumberSet[][] value)
        {
            var sb = new System.Text.StringBuilder();
            for (int row = 0; row < value.Length; row++)
            {
                for (int col = 0; col < value[row].Length; col++)
                {
                    sb.Append(value[row][col]);
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private bool AddOwnFindings()
        {
            bool changed = false;
            while (pendingFindings.Count > 0)
            {
                Entry finding = pendingFindings.First();  
                SetEntry(finding.Row, finding.Col, finding.Num);
                pendingFindings.Remove(finding);
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Specifies that an entry cannot be made.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="num"></param>
        /// <exception cref="IllegalEntryException">In case it is known that the restriction makes the matrix impossible to complete.</exception>
        protected virtual void forbidValue(int row, int col, int num)
        {
            int state = entries[row][col].ForbidValue(num);
            switch (state)
            {
                case NumberSet.ALREADYKNOWN: break;
                case NumberSet.UNKNOWN:
                    nextTransposition.forbidValue(col, num, row);
                    break;
                default:
                    pendingFindings.Add(new Entry(row, col, state));
                    break;
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int row = 0; row < entries.Length; row++)
            {
                for (int col = 0; col < entries[0].Length; col++)
                {
                    int num = entries[row][col].getValue();
                    sb.Append(num == NumberSet.UNKNOWN ? "  ." : String.Format("{0, 3}", num));
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            LatinSquare other = (LatinSquare)obj;
            for (int row = 0; row < entries.Length; row++)
            {
                for (int col = 0; col < entries[row].Length; col++)
                {
                    if (GetEntry(row, col) != other.GetEntry(row, col))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int result = 0;
            int prime = 3;
            for (int row = 0; row < entries.Length; row++)
            {
                for (int col = 0; col < entries[row].Length; col++)
                {
                        result = prime * result + GetEntry(row, col);                    
                }
            }
            return result;
        }
    }
   
}
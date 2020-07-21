using System;
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
         * Creates an instance linked with its two alternative forms.
         */
        public static LatinSquare create(int size)
        {
            LatinSquare a = new LatinSquare(size);
            LatinSquare b = new LatinSquare(size);
            LatinSquare c = new LatinSquare(size);
            a.nextTransposition = b;
            b.nextTransposition = c;
            c.nextTransposition = a;
            return a;
        }

        /**
         * Returns an entry of specified cell or OK if unknown.
         */
        public int getNum(int row, int col)
        {
            return (entries[row][col].getValue());
        }

        private NumberSet[][] CloneEntries()
        {
            NumberSet[][] clone = new NumberSet[entries.Length][];
            for (int i = 0; i < entries.Length; i++)
            {
                clone[i] = new NumberSet[entries.Length];
                for (int j = 0; j < entries.Length; j++)
                {
                    clone[i][j] = new NumberSet(entries[i][j]);
                }
            }
            return clone;
        }

        /**
         * Not to be used except in inheriting classes. Protected would not permit call from static methods.
         */
        internal LatinSquare(LatinSquare nt) : this(nt.entries.Length)
        {
            nextTransposition = nt;
        }

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


        /**
         * Fix all entries which have been derived from entries made so far.
         */
        public void AddFindings()
        {
            while (AddOwnFindings() || nextTransposition.AddOwnFindings() || nextTransposition.nextTransposition.AddOwnFindings())
            {
                // just repeat
            }
        }

        /// <summary>
        /// Uses brute force to complete the whole matrix if possible. 
        /// </summary>
        /// <returns>number of entries which turned out to be wrong and have been rolled back</returns>
        public int complete(bool doReverse)
        {
            AddFindings();
            int numberRestores = 0;
            var target = findEntryWithMinChoices();
            if (target.Row == -1)
            {
                return numberRestores;
            }
            NumberSet[][] backupA = CloneEntries();
            NumberSet[][] backupB = nextTransposition.CloneEntries();
            NumberSet[][] backupC = nextTransposition.nextTransposition.CloneEntries();
            IllegalEntryException last = null;

            List<int> values = entries[target.Row][target.Col].GetAllowedValues();
            if (doReverse)
            {
                values.Reverse();
            }
            foreach (int num in values)
            {
                try
                {
                    SetEntry(target.Row, target.Col, num);
                    numberRestores += complete(doReverse);
                    return numberRestores;
                }
                catch (IllegalEntryException e)
                {
                    numberRestores++;
                    last = e;
                    entries = backupA;
                    pendingFindings.Clear();
                    nextTransposition.entries = backupB;
                    nextTransposition.pendingFindings.Clear();
                    nextTransposition.nextTransposition.entries = backupC;
                    nextTransposition.nextTransposition.pendingFindings.Clear();
                }
            }
            throw last;
        }

        private Entry findEntryWithMinChoices()
        {
            int minChoices = entries.Length;
            int targetRow = -1;
            int targetCol = -1;

            for (int row = 0; row < entries.Length; row++)
                for (int col = 0; col < entries.Length; col++)
                {
                    int choices = entries[row][col].GetNumPossibleValues();
                    if (choices == 2)
                    {
                        return new Entry(row, col, NumberSet.UNKNOWN);
                    }
                    if (choices > 1 && choices < minChoices)
                    {
                        minChoices = choices;
                        targetRow = row;
                        targetCol = col;
                    }
                }
            return new Entry(targetRow, targetCol, NumberSet.UNKNOWN);
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
                    if (GetEntry(row, col) == other.GetEntry(row, col))
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
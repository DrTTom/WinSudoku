using System;
using System.Collections.Generic;

namespace WinSudoku
{

    /// <summary>
    /// A set of numbers with fast access (nothing faster than direct adressing). 
    /// </summary>
    public class NumberSet
    {
        /// <summary>
        /// Returned by ForbidValue(int) in case the entry is still unknown.
        /// </summary>
        public const int UNKNOWN = -1;
        /*
         * Return value to indicate that the state of this object did not change.
         */
        public const int ALREADY_KNOWN = -2;

        private bool[] allowedValues;
        private int numPossibleValues;
        private int value = UNKNOWN;

        /**
         * Creates instance with all entries 0,...,size-1 possible.
         */
        public NumberSet(int size)
        {
            allowedValues = new bool[size];
            for (int i = 0; i < size; i++)
            {
                allowedValues[i] = true;
            }
            numPossibleValues = size;
        }

        /**
         * Creates copy.
         */
        internal NumberSet(NumberSet original)
        {
            allowedValues = new bool[original.allowedValues.Length];
            Array.Copy(original.allowedValues, allowedValues, allowedValues.Length);
            numPossibleValues = original.numPossibleValues;
            value = original.value;
        }

        /**
         * Forbids a value.
         */
        /// <returns>OK, ALREADY_KNOWN or the last remaining allowed value</returns>
        /// <exception cref="IllegalEntryException">if there remains no possible entry</exception>
        public int ForbidValue(int num)
        {
            if (allowedValues[num])
            {
                if (numPossibleValues <= 1)
                {
                    throw new IllegalEntryException();
                }
                allowedValues[num] = false;
                numPossibleValues--;
                if (numPossibleValues == 1)
                {
                    return Array.IndexOf(allowedValues, true);
                }
                return UNKNOWN;
            }
            return ALREADY_KNOWN;
        }

        /**
         * Specifies a value.
         */
        /// <returns>ALREADY_KNOWN or the value just set</returns>
        /// <exception cref="IllegalEntryException">if value cannot be set</exception>
        public int SetValue(int num)
        {
            if (allowedValues[num])
            {
                if (value == num)
                {
                    return ALREADY_KNOWN;
                }
                value = num;
                for (int i = 0; i < allowedValues.Length; i++)
                {
                    allowedValues[i] = i == num;
                }
                numPossibleValues = 1;
                return value;
            }
            throw new IllegalEntryException();
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < allowedValues.Length; i++)
            {
                if (allowedValues[i])
                    sb.Append(i);
            }
            return sb.ToString().PadLeft(allowedValues.Length);
        }

        /*
         * Returns the specified value or OK if unknown.
         */
        public int getValue()
        {
            return value;
        }

        /**
         * Returns the number of possible values.
         */
        public int GetNumPossibleValues()
        {
            return numPossibleValues;
        }

        /// <summary>
        /// Returns a list of possible values.
        /// </summary>
        public List<int> GetAllowedValues()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < allowedValues.Length; i++)
            {
                if (allowedValues[i])
                {
                    result.Add(i);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if number is allowed.
        /// </summary>
        /// <param name="num"> must be in range 0-size</param>
        public bool IsAllowed(int num)
        {
            return allowedValues[num];
        }
    }
}

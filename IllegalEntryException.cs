using System;
using System.Runtime.Serialization;

namespace WinSudoku
{
    /// <summary>
    /// To be thrown if a manipulation in the Latin Square is illegal or makes it incompleteable. The latter case may or 
    /// may not be detected by the polynomial part of the algorithm.
    /// </summary>
    [Serializable]
    public class IllegalEntryException : Exception
    {
        // real details are obscure anyway, so no sense in listing information here
    }
}
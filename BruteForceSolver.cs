using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSudoku
{
    /// <summary>
    /// Took the try and revert method out of LatinSquare to prepare parallization.
    /// </summary>
    public class BruteForceSolver
    {
        public int Effort { get; set; }
        public bool Reverse { get; set; }
        
        public LatinSquare Complete(LatinSquare square)
        {
            if (square == null) throw new ArgumentNullException(nameof(square));

            square.AddFindings();
            
            var target = findEntryWithMinChoices(square);
            if (target.Row == -1)
            {
                return square;
            }
            IllegalEntryException last = null;

            List<int> values = square.entries[target.Row][target.Col].GetAllowedValues();
            if (Reverse)
            {
                values.Reverse();
            }
            int remaining = values.Count;
            foreach (int num in values)
            {
                try
                {
                    LatinSquare candidate = (--remaining == 0 ? square : square.CreateCopy());
                    candidate.SetEntry(target.Row, target.Col, num);
                    return Complete(candidate);
                }
                catch (IllegalEntryException e)
                {
                    Effort++;
                    last = e;                    
                }
            }
            throw last;
        }

        private Entry findEntryWithMinChoices(LatinSquare lq)
        {
            int minChoices = lq.entries.Length;
            int targetRow = -1;
            int targetCol = -1;

            for (int row = 0; row < lq.entries.Length; row++)
                for (int col = 0; col < lq.entries.Length; col++)
                {
                    int choices = lq.entries[row][col].GetNumPossibleValues();
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

    }
}

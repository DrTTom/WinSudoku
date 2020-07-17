using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSudoku
{

    /// <summary>
    /// A latin square with the additional small square restrictions.
    /// </summary>
    public class Sudoku : LatinSquare
    {
        private int cellRows;
        private int cellCols;

        private NumberSet[][] positionInCell;

        private Sudoku(int cellRows, int cellCols) : base(cellCols * cellRows)
        {
            this.cellRows = cellRows;
            this.cellCols = cellCols;
            positionInCell = new NumberSet[entries.Length][];
            for (int cell = 0; cell < entries.Length; cell++)
            {
                positionInCell[cell] = new NumberSet[entries.Length];
                for (int num = 0; num < entries.Length; num++)
                {
                    positionInCell[cell][num] = new NumberSet(entries.Length);
                }
            }
        }

        /// <summary>
        /// Creates instance.
        /// </summary>
        /// <param name="cellRows">number of rows per small cell</param>
        /// <param name="cellCols">number of columns per small cell</param>
        /// <returns></returns>
        public static Sudoku create(int cellRows, int cellCols)
        {
            Sudoku a = new Sudoku(cellRows, cellCols);
            LatinSquare b = new LatinSquare(a);
            LatinSquare c = new LatinSquare(b);
            a.nextTransposition = c;
            return a;
        }

        public override void SetEntry(int row, int col, int num)
        {
            base.SetEntry(row, col, num);
            int cellRow = row / cellRows;
            int cellOffsetRow = cellRow * cellRows;
            int cellCol = col / cellCols;
            int cellOffsetCol = cellCol * cellCols;
            for (int i = cellOffsetRow; i < cellOffsetRow + cellRows; i++)
            {
                for (int j = cellOffsetCol; j < cellOffsetCol + cellCols; j++)
                {
                    if (i != row || j != col)
                    {
                        forbidValue(i, j, num);
                    }
                }
            }

            for (int otherNum = 0; otherNum < entries.Length; otherNum++)
            {
                if (otherNum==num)
                {
                    break;
                }
                HashSet<int> rows = new HashSet<int>();
                HashSet<int> cols = new HashSet<int>();
                for (int rowInCell = cellOffsetRow; rowInCell < cellOffsetRow + cellRows; rowInCell++)
                {
                    for (int colInCell = cellOffsetCol; colInCell < cellCols + cellOffsetCol; colInCell++)
                    {
                        if (entries[rowInCell][colInCell].IsAllowed(otherNum))
                        {
                            rows.Add(rowInCell);
                            cols.Add(colInCell);
                        }
                    }
                }
                if (rows.Count == 0 || cols.Count == 0)
                {
                    throw new IllegalEntryException();
                }
                if (rows.Count==1)
                {
                    if (cols.Count == 1)
                    {
                        pendingFindings.Add(new Entry(rows.First(), cols.First(), otherNum));
                    }
                    else
                    {
                        for (int otherCol = 0; otherCol < entries.Length; otherCol++)
                        {
                            if (otherCol<cellOffsetCol || otherCol>=cellOffsetCol+cellCols)
                            {
                                forbidValue(rows.First(), otherCol, otherNum);
                            }
                        }
                    }
                }            
                else if (cols.Count==1)
                {
                    for (int otherRow = 0; otherRow < entries.Length; otherRow++)
                    {
                        if (!rows.Contains(otherRow))
                        {
                            forbidValue(otherRow, cols.First(), otherNum);
                        }
                    }
                }
            }
        }
    }
}

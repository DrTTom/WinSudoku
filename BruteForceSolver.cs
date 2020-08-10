using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace WinSudoku
{
    /// <summary>
    /// Backtracking algorithm implemented both sequentially using recursive depth-first-search and in parrallel unrecursively with undefined 
    /// search sequence. Parallelization does not work well because additional threads require synchronization overhead and increase 
    /// the overall effort by computing more invalid alternatives. However, using 4 threads increases speed by up to factor 2. 
    /// Instances of this class are for one time use only!
    /// </summary>
    public class BruteForceSolver
    {
        public int Effort { get; set; }

        private LatinSquare solution;
        private Tasks<LatinSquare> tasks = new Tasks<LatinSquare>();

        public bool Reverse { get; set; }

        private void AddEntry()
        {
            while (solution == null)
            {
                LatinSquare square = tasks.GetNext();
                if (square == null)
                {
                    return;
                }
                try
                {
                    addOneEntry(square);
                }
                finally
                {
                    tasks.Done();
                }
            }
        }

        private void addOneEntry(LatinSquare square)
        {
            try
            {
                while (solution == null)
                {
                    square.AddFindings();
                    var target = findEntryWithMinChoices(square);
                    if (target.Row == -1)
                    {
                        solution = square;
                        tasks.AbortAll();
                        return;
                    }

                    List<int> values = square.entries[target.Row][target.Col].GetAllowedValues();
                    if (Reverse)
                    {
                        values.Reverse();
                    }

                    int remaining = values.Count;
                    foreach (int num in values)
                    {
                        if (--remaining > 0)
                        {
                            try
                            {
                                LatinSquare candidate = square.CreateCopy();
                                candidate.SetEntry(target.Row, target.Col, num);
                                tasks.Add(candidate);
                            }
                            catch (IllegalEntryException)
                            {
                                Effort++;
                            }
                        }
                        else
                        {
                            square.SetEntry(target.Row, target.Col, num); // just continue loop instead of wait for another task                            
                        }
                    }
                }
            }
            catch (IllegalEntryException)
            {
                Effort++;
            }
        }

        public LatinSquare CompleteParallel(LatinSquare square, int numberThreads)
        {
            tasks.Add(square);

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < numberThreads; i++)
            {
                threads.Add(new Thread(AddEntry));
            }
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            if (solution == null)
            {
                throw new IllegalEntryException();
            }
            return solution;
        }


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

using NUnit.Framework;
using WinSudoku;

namespace WinSudokuTest
{
    /// <summary>
    /// Test for the Sudoku class. Integration of classes LatinSquare and NumberSet is included here. If this was a productive code, 
    /// unit tests for those classes would have been written separately first.
    /// </summary>
    public class SudokuTest
    {        
        /// <summary>
        /// Just looking how to write a test. Real test would be more careful.
        /// </summary>
        [Test]
        public void Test1()
        {
            Sudoku systemUnderTest = Sudoku.create(3, 3);
            systemUnderTest.SetEntry(0, 0, 0);
            systemUnderTest.SetEntry(0, 1, 1);
            systemUnderTest.SetEntry(2, 0, 3);
            systemUnderTest.SetEntry(2, 1, 4);
            systemUnderTest.SetEntry(2, 2, 5);
            systemUnderTest.SetEntry(1, 3, 2);
            systemUnderTest.AddFindings();
            Assert.AreEqual(2, systemUnderTest.GetEntry(0, 2));

            Assert.Pass();
        }
    }
}
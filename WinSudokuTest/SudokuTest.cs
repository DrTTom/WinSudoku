using NUnit.Framework;
using WinSudoku;
using System.Collections.Generic;
using System.Threading;

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
        public void AddEntries()
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

        /// <summary>
        /// Checks whether synchronizing works. 
        /// </summary>
        [Test]
        public void Synchronize()
        {
            Tasks<string> systemUnderTest = new Tasks<string>();
            systemUnderTest.Add("first");
            Assert.AreEqual("first", systemUnderTest.GetNext());

            List<string> obtained = new List<string>();
            Thread consumer = new Thread(() =>
            {
                while (true)
                {
                    string value = systemUnderTest.GetNext();
                    if (value == null) { return; }
                    obtained.Add(value);
                    systemUnderTest.Done();
                }
            });
            consumer.Start();
            Assert.IsTrue(consumer.IsAlive);
            Assert.AreEqual(0, obtained.Count);
            systemUnderTest.Add("second");
            Thread.Sleep(50);
            Assert.AreEqual("second", obtained[0]);
            Assert.IsTrue(consumer.IsAlive);
            systemUnderTest.Done();
            Thread.Sleep(50);
            Assert.IsFalse(consumer.IsAlive);
        }

    }
}
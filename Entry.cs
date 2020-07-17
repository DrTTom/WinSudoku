namespace WinSudoku
{
    /// <summary>
    /// Just a triple. Would be a lot easier in Kotlin of Java with lombok.
    /// </summary>
    internal class Entry
    {
        public int Row { get; }
        public int Col { get; }
        public int Num { get; }

       internal Entry(int row, int col, int num)
        {
            this.Row = row;
            this.Col = col;
            this.Num = num;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Entry p = (Entry)obj;
                return (Row == p.Row) && (Col == p.Col) && (Num == p.Num);
            }
        }

        public override int GetHashCode()
        {
            return Row + 7 * Col + 49 * Num;
        }
    }
}
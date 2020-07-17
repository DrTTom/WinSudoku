namespace WinSudoku
{
    /// <summary>
    /// Just a triple. Would be a lot easier in Kotlin of Java with lombok.
    /// </summary>
    internal class Entry
    {
        int row; int col; int num;

        internal Entry(int row, int col, int num)
        {
            this.row = row;
            this.col = col;
            this.num = num;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Entry p = (Entry)obj;
                return (row == p.row) && (col == p.col) && (num == p.num);
            }
        }

        public override int GetHashCode()
        {
            return col + 7 * row + 49 * num;
        }


        internal int getCol()
        {
            return col;
        }

        internal int getNum()
        {
            return num;
        }

        internal int getRow()
        {
            return row;
        }
    }
}
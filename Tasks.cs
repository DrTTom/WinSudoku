using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WinSudoku
{
    public class Tasks<T> where T : class
    {
        private Stack<T> elements = new Stack<T>();

        private int numberRunning;

        private bool disableAdding;

        public void Add(T element)
        {
            if (!disableAdding)
            {
                lock (this)
                {
                    elements.Push(element);
                    Monitor.Pulse(this);
                }
            }
        }

        /// <summary>
        /// Shall block until element is available or no more to come.
        /// </summary>
        /// <returns>null if there is no more task</returns>
        public T GetNext()
        {
            lock (this)
            {
                while (elements.Count > 0 || numberRunning > 0)
                {
                    if (elements.Count > 0)
                    {
                        numberRunning++;
                        return elements.Pop();
                    }
                    Monitor.Wait(this);
                }
                return null;
            }
        }


        /// <summary>
        /// Removes all elements and signals that 
        /// </summary>
        public void AbortAll()
        {
            lock (this)
            {
                elements.Clear();
                numberRunning = 0;
                disableAdding = true;
                Monitor.Pulse(this);
            }
        }

        public void Done()
        {
            lock (this)
            {
                numberRunning--;
                Monitor.Pulse(this);
            }
        }
    }
}

namespace Plus.Game.Rooms.PathFinding
{
    using System;

    internal sealed class MinHeap<T> where T : IComparable<T>
    {
        private int capacity;
        private T temp;
        private T mheap;
        private T[] array;
        private T[] tempArray;

        public int Count { get; private set; }

        public MinHeap() : this(16) { }

        public MinHeap(int capacity)
        {
            Count = 0;
            this.capacity = capacity;
            array = new T[capacity];
        }

        public void BuildHead()
        {
            int position;
            for (position = (Count - 1) >> 1; position >= 0; position--)
            {
                MinHeapify(position);
            }
        }

        public void Add(T item)
        {
            Count++;
            if (Count > capacity)
            {
                DoubleArray();
            }
            array[Count - 1] = item;
            var position = Count - 1;

            var parentPosition = (position - 1) >> 1;

            while (position > 0 && array[parentPosition].CompareTo(array[position]) > 0)
            {
                temp = array[position];
                array[position] = array[parentPosition];
                array[parentPosition] = temp;
                position = parentPosition;
                parentPosition = (position - 1) >> 1;
            }
        }

        private void DoubleArray()
        {
            capacity <<= 1;
            tempArray = new T[capacity];
            CopyArray(array, tempArray);
            array = tempArray;
        }

        private static void CopyArray(T[] source, T[] destination)
        {
            int index;
            for (index = 0; index < source.Length; index++)
            {
                destination[index] = source[index];
            }
        }

        public T ExtractFirst()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }
            temp = array[0];
            array[0] = array[Count - 1];
            Count--;
            MinHeapify(0);
            return temp;
        }

        private void MinHeapify(int position)
        {
            do
            {
                var left = (position << 1) + 1;
                var right = left + 1;
                int minPosition;

                if (left < Count && array[left].CompareTo(array[position]) < 0)
                {
                    minPosition = left;
                }
                else
                {
                    minPosition = position;
                }

                if (right < Count && array[right].CompareTo(array[minPosition]) < 0)
                {
                    minPosition = right;
                }

                if (minPosition != position)
                {
                    mheap = array[position];
                    array[position] = array[minPosition];
                    array[minPosition] = mheap;
                    position = minPosition;
                }
                else
                {
                    return;
                }

            } while (true);
        }
    }
}

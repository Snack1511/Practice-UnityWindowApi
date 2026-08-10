using System;
using System.Collections.Generic;

namespace Script.GameFlow.Command
{
    /// <summary>
    /// 명령 우선순위 큐. 이진 힙이다.
    ///
    /// .NET 6 의 <c>System.Collections.Generic.PriorityQueue</c> 를 쓸 수 없어 직접 구현했다.
    /// 이 프로젝트의 API 호환 레벨은 .NET Standard 2.1 이고 해당 타입은 .NET 6 에서 추가됐다.
    /// 런타임이 .NET 6 이상으로 올라가면 이 클래스는 BCL 타입으로 대체할 수 있다.
    ///
    /// 정렬 조건은 생성자로 주입한다. 비교자가 없으면 넣은 순서를 유지한다.
    /// </summary>
    public class CmdPriorityQueue
    {
        private readonly List<Entry> heap = new List<Entry>();
        private readonly IComparer<ICmdBase> comparer;

        /// <summary>같은 우선순위끼리는 넣은 순서를 지키기 위한 일련번호.</summary>
        private long sequence;

        /// <param name="comparer">
        /// 정렬 조건. null 이면 우선순위를 두지 않고 넣은 순서대로 나온다.
        /// </param>
        public CmdPriorityQueue(IComparer<ICmdBase> comparer = null)
        {
            this.comparer = comparer;
        }

        public int Count => heap.Count;

        public void Enqueue(ICmdBase command)
        {
            heap.Add(new Entry(command, sequence++));
            SiftUp(heap.Count - 1);
        }

        public ICmdBase Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("빈 큐에서 Dequeue 할 수 없다.");

            Entry root = heap[0];

            int last = heap.Count - 1;
            heap[0] = heap[last];
            heap.RemoveAt(last);

            if (heap.Count > 0)
                SiftDown(0);

            return root.Command;
        }

        public void Clear()
        {
            heap.Clear();
            sequence = 0;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (Compare(heap[index], heap[parent]) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                int smallest = index;

                if (left < heap.Count && Compare(heap[left], heap[smallest]) < 0)
                    smallest = left;

                if (right < heap.Count && Compare(heap[right], heap[smallest]) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        /// <summary>
        /// 비교자가 동률을 내거나 아예 없으면 일련번호로 가른다.
        /// 힙은 원래 안정 정렬이 아니라서, 이렇게 해야 같은 우선순위의 순서가 뒤집히지 않는다.
        /// </summary>
        private int Compare(Entry left, Entry right)
        {
            if (comparer != null)
            {
                int result = comparer.Compare(left.Command, right.Command);
                if (result != 0)
                    return result;
            }

            return left.Sequence.CompareTo(right.Sequence);
        }

        private void Swap(int left, int right)
        {
            (heap[left], heap[right]) = (heap[right], heap[left]);
        }

        private readonly struct Entry
        {
            public readonly ICmdBase Command;
            public readonly long Sequence;

            public Entry(ICmdBase command, long sequence)
            {
                Command = command;
                Sequence = sequence;
            }
        }
    }
}

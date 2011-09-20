using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XtreemGeek.TagCaching.Util
{
    /// <summary>
    /// This list tracks only the tail, and the head has to be tracked by calling objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TailList<T>
    {
        private class Node
        {
            public T Value { get; set; }

            public bool IsNonEmpty { get; set; }

            public Node Next { get; set; }
        }

        private Node _Current { get; set; }

        public TailList()
        {
            _Current = new Node();
        }

        public void Add(T value)
        {
            // lock(this) is not typically advised but I don't want to create another object
            // just for the sake of lock, i'm worried about the GC overhead

            lock (this)
            {
                Node previous = _Current;
                _Current = new Node();

                previous.Value = value;
                previous.Next = _Current;
                previous.IsNonEmpty = true;
            }
        }

        /// <summary>
        /// always give the latest node as the list head
        /// </summary>
        public object ListHeadToken
        {
            get { return _Current; }
        }

        /// <summary>
        /// Enumerate from the list-head snapshot that the caller
        /// has obtained.
        /// </summary>
        /// <param name="listHeadToken"></param>
        /// <returns></returns>
        public IEnumerable<T> GetEnumerable(object listHeadToken)
        {
            Node node = (Node)listHeadToken;
            while (node.IsNonEmpty)
            {
                yield return node.Value;
                node = node.Next;
            }
        }
    }
}

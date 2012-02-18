using System;
using System.Collections.Generic;

namespace Cloudy.Helpers
{
    public static class LinkedListExtensions
    {
        /// <summary>
        /// Enumerates the nodes in the linked list.
        /// </summary>
        public static IEnumerable<LinkedListNode<T>> EnumerateNodes<T>(
            this LinkedList<T> list)
        {
            LinkedListNode<T> node = list.First;
            while (node != null)
            {
                yield return node;
                node = node.Next;
            }
        }
    }
}

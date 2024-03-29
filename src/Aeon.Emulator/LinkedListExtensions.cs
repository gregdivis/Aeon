﻿using System;
using System.Collections.Generic;

namespace Aeon.Emulator
{
    internal static class LinkedListExtensions
    {
        /// <summary>
        /// Replaces an existing linked list node with zero or more new nodes.
        /// </summary>
        /// <typeparam name="T">Type of list item.</typeparam>
        /// <param name="list">Linked list instance.</param>
        /// <param name="originalItem">Item to replace.</param>
        /// <param name="newItems">Values to insert in place of the original item.</param>
        public static void Replace<T>(this LinkedList<T> list, T originalItem, T[] newItems)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(newItems);

            var originalNode = list.Find(originalItem);
            if (originalNode == null)
                throw new ArgumentException("Original item not found.");

            if (originalNode.Previous == null)
            {
                list.RemoveFirst();
                for (int i = newItems.Length - 1; i >= 0; i--)
                    list.AddFirst(newItems[i]);
            }
            else
            {
                var previous = originalNode.Previous;
                list.Remove(originalNode);
                for (int i = newItems.Length - 1; i >= 0; i--)
                    list.AddAfter(previous, newItems[i]);
            }
        }
        /// <summary>
        /// Replaces an existing linked list node with a new node.
        /// </summary>
        /// <typeparam name="T">Type of list item.</typeparam>
        /// <param name="list">Linked list instance.</param>
        /// <param name="originalItem">Item to replace.</param>
        /// <param name="newItems">New item to replace the original with.</param>
        public static void Replace<T>(this LinkedList<T> list, T originalItem, T newItem)
        {
            ArgumentNullException.ThrowIfNull(list);

            var originalNode = list.Find(originalItem);
            if (originalNode == null)
                throw new ArgumentException("Original item not found.");

            if (originalNode.Previous == null)
            {
                list.RemoveFirst();
                list.AddFirst(newItem);
            }
            else
            {
                var previous = originalNode.Previous;
                list.Remove(originalNode);
                list.AddAfter(previous, newItem);
            }
        }
    }
}

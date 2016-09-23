using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Skahal.Common;
using UnityEngine;

namespace Buildron.Domain.Sorting
{
    /// <summary>
    /// Sorting algorithm base class.
    /// </summary>
    public abstract class SortingAlgorithmBase<TItem>
        : ISortingAlgorithm<TItem> where TItem : System.IComparable<TItem>
    {
        #region Events
        /// <summary>
        /// Occurs when sorting begin.
        /// </summary>
        public event EventHandler<SortingBeginEventArgs> SortingBegin;

        /// <summary>
        /// Occurs when sorting items swapped.
        /// </summary>
        public event EventHandler<SortingItemsSwappedEventArgs<TItem>> SortingItemsSwapped;

        /// <summary>
        /// Occurs when sorting end.
        /// </summary>
        public event EventHandler<SortingEndedEventArgs> SortingEnded;
        #endregion

        #region Events
        private bool m_wasAlreadySorted;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Buildron.Domain.Sorting.SortingAlgorithmBase`1"/> class.
        /// </summary>
        /// <param name="name">The algorithm name.</param>
        protected SortingAlgorithmBase(string name)
        {
            Name = name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the comparer.
        /// </summary>
        public IComparer<TItem> Comparer { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Sorts the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="comparer">The equality comparer.</param>
        public IEnumerator Sort(IList<TItem> items, IComparer<TItem> comparer)
        {
            m_wasAlreadySorted = true;
            Comparer = comparer;
            var enumerator = PerformSort(items);

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }

            // If items was already sorted, then SortingBegin was not raised inside Swap.
            if (m_wasAlreadySorted)
            {
                SortingBegin.Raise(this, new SortingBeginEventArgs(m_wasAlreadySorted));
            }

            SortingEnded.Raise(this, new SortingEndedEventArgs(m_wasAlreadySorted));
        }

        /// <summary>
        /// Performs the sort.
        /// </summary>
        /// <returns>The sort.</returns>
        /// <param name="items">Items.</param>
        protected abstract IEnumerator PerformSort(IList<TItem> items);

        /// <summary>
        /// Determines whether item is greater than the other item.
        /// </summary>
        /// <returns><c>true</c> if this instance is greater than the specified item other; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        /// <param name="other">Other.</param>
        protected bool IsGreaterThan(TItem item, TItem other)
        {
            return Comparer.Compare(item, other) > 0;
        }

        /// <summary>
        /// Determines whether item is lower than the other item.
        /// </summary>
        /// <returns><c>true</c> if this instance is greater than the specified item other; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        /// <param name="other">Other.</param>
        protected bool IsLowerThan(TItem item, TItem other)
        {
            return Comparer.Compare(item, other) < 0;
        }

        /// <summary>
        /// Swap the specified items.
        /// </summary>
        /// <param name="items">Items.</param>
        /// <param name="item1Index">Item1 index.</param>
        /// <param name="item2Index">Item2 index.</param>
        protected WaitForSeconds Swap(IList<TItem> items, int item1Index, int item2Index)
        {
            var item1 = items[item1Index];
            var item2 = items[item2Index];
            items[item1Index] = item2;
            items[item2Index] = item1;

            // If a swap it's happen, so it's not already sorted.
            // Raise SortingBegin before first swap.
            if (m_wasAlreadySorted)
            {
                m_wasAlreadySorted = false;
                SortingBegin.Raise(this, new SortingBeginEventArgs(false));
            }

            SortingItemsSwapped.Raise(this, new SortingItemsSwappedEventArgs<TItem>(item1, item2));
            

            return new WaitForSeconds(SortingAlgorithmFactory.SwappingTime);
        }      
        #endregion
    }
}
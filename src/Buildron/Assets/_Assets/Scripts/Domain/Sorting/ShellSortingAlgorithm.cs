#region Usings
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using UnityEngine;
#endregion

namespace Buildron.Domain.Sorting
{	
	/// <summary>
	/// Shell sorting algorithm: http://www.sorting-algorithms.com/shell-sort
	/// </summary>
	public class ShellSortingAlgorithm<TItem> : SortingAlgorithmBase<TItem> where TItem : System.IComparable<TItem>
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Buildron.Domain.Sorting.ShellSortingAlgorithm`1"/> class.
		/// </summary>
		public ShellSortingAlgorithm () : base("Shell Sort")
		{
		}
		#endregion

		#region Methods
		/// <summary>
		/// Performs the sort.
		/// </summary>
		/// <returns>The sort.</returns>
		/// <param name="items">Items.</param>
		protected override IEnumerator PerformSort (IList<TItem> items)
		{
			TItem temp;
			int j;
			int increment = (items.Count) / 2;
			
			while (increment > 0) {
				for (int index = 0; index < items.Count; index++) {
					j = index;
					temp = items [index];
					
					while ((j >= increment) && IsGreaterThan(items[j - increment], temp)) {
						yield return Swap(items, j, j - increment);
						j = j - increment;
					}
				}
				if (increment / 2 != 0) {
					increment = increment / 2;
				} else if (increment == 1) {
					increment = 0;
				} else {
					increment = 1;
				}
			}				
		}
		#endregion
	}
}
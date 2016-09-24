﻿using Buildron.Domain.Sorting;
using Buildron.Domain.Builds;

namespace Buildron.Domain.RemoteControls
{
	/// <summary>
	/// Sort builds remote control command.
	/// </summary>
	public class SortBuildsRemoteControlCommand : IRemoteControlCommand
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Buildron.Domain.RemoteControls.SortBuildsRemoteControlCommand"/> class.
		/// </summary>
		/// <param name="algorithm">Algorithm.</param>
		/// <param name="sortBy">Sort by.</param>
		public SortBuildsRemoteControlCommand (ISortingAlgorithm<IBuild> algorithm, SortBy sortBy)
		{
			Algorithm = algorithm;
			SortBy = sortBy;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Buildron.Domain.RemoteControls.SortBuildsRemoteControlCommand"/> class.
		/// </summary>
		/// <param name="algorithmType">Algorithm type.</param>
		/// <param name="sortBy">Sort by.</param>
		public SortBuildsRemoteControlCommand(SortingAlgorithmType algorithmType, SortBy sortBy)
		{
			AlgorithmType = algorithmType;
			SortBy = sortBy;
		}
        #endregion

        #region Properties
		/// <summary>
		/// Gets or sets the algorithm.
		/// </summary>
		/// <value>The algorithm.</value>
		public ISortingAlgorithm<IBuild> Algorithm { get; set; }

		/// <summary>
		/// Gets or sets the type of the algorithm.
		/// </summary>
		/// <value>The type of the algorithm.</value>
		public SortingAlgorithmType AlgorithmType{ get; set; }

		/// <summary>
		/// Gets or sets the sort by.
		/// </summary>
		/// <value>The sort by.</value>
     	public SortBy SortBy { get; set; }
		#endregion
	}
}
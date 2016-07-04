using System;
using System.Linq;
using Buildron.Domain;
using Skahal.Infrastructure.Framework.Repositories;
using Skahal.Logging;
using Skahal.Common;
using UnityEngine;

namespace Buildron.Domain.Servers
{
	/// <summary>
	/// Server domain service.
	/// </summary>
	public class ServerService : IServerService
	{
		#region Events
		/// <summary>
		/// Occurs when server state is updated.
		/// </summary>
		public event EventHandler<ServerStateUpdatedEventArgs> StateUpdated;
		#endregion

		#region Fields
		private IRepository<ServerState> m_repository;
		private ISHLogStrategy m_log;
		private ServerState m_state;
	    #endregion

	    #region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Buildron.Domain.Servers.ServerService"/> class.
		/// </summary>
		/// <param name="repository">Repository.</param>
		/// <param name="log">Log.</param>
		public ServerService(IRepository<ServerState> repository, ISHLogStrategy log)
	    {
			m_log = log;
			m_repository = repository;
			m_state = m_repository.All ().FirstOrDefault (s => !s.IsNew) ?? new ServerState ();

			m_state.IsShowingHistory = false;
			m_state.IsSorting = false;
	    }
		#endregion

		#region Methods
		/// <summary>
		/// Saves the state.
		/// </summary>
		/// <param name="state">The state to save.></param>
		public void SaveState(ServerState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException ("state");
			}

			if (m_state.Id == 0)
			{
				m_log.Debug("Creating a new ServerState:");
				m_repository.Create(state);
			}
			else 
			{
				m_log.Warning("Updating an current ServerState: Z: {0}", m_state.CameraPosition.z);
				state.Id = m_state.Id;
				m_repository.Modify(state);
			}

			m_state = state;

			StateUpdated.Raise(this, new ServerStateUpdatedEventArgs(state));
		}

		/// <summary>
		/// Gets the state.
		/// </summary>
		/// <returns>The state.</returns>
		public ServerState GetState()
		{
			return m_state;
		}
	    #endregion
	}
}
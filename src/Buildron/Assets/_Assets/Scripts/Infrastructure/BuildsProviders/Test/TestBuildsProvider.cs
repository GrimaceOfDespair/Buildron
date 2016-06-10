#region Usings
using System.Collections.Generic;
using Buildron.Domain;
using Skahal.Common;
using UnityEngine;
#endregion

public class TestBuildsProvider : IBuildsProvider
{
	#region Fields
	private List<Build> m_builds;
	#endregion
	
	#region Constructors
	public TestBuildsProvider ()
	{
		m_builds = new List<Build> ();
		int id = 0;
		
		System.Action<string, string> addBuild = (name, projectName) =>
		{
			id++;
			m_builds.Add (new Build () 
			{
				Id = id.ToString (),
				Configuration = new BuildConfiguration ()
				{
					Id = id.ToString (),
					Name = name,
					Project = new BuildProject () 
					{
						Name = projectName
					}
				}
			});
		};
		
		for (int i = 0; i < 1; i++) {
			addBuild ("iOS - Classic", "Ships N'Battles");
			addBuild ("iOS - HD", "Ships N'Battles");
			addBuild ("Android", "Ships N'Battles");
			addBuild ("Mac", "Ships N'Battles");
			addBuild ("Web", "Ships N'Battles");
			addBuild ("Windows Phone", "Ships N'Battles");
			addBuild ("OUYA", "Ships N'Battles");
		
			addBuild ("1.4.3", "Card-o-matic");
			addBuild ("1.?.?", "Card-o-matic");
		
			addBuild ("Mac", "Buildron");
			//addBuild ("Windows", "Buildron");
		
			//addBuild ("iOS", "Buildron RC");
			//addBuild ("Android", "Buildron RC");	
		}
	}
	#endregion
	
	#region IBuildsProvider implementation
	public event System.EventHandler<BuildUpdatedEventArgs> BuildUpdated;
	public event System.EventHandler BuildsRefreshed;	
	public event System.EventHandler ServerUp;
	public event System.EventHandler ServerDown;
	public event System.EventHandler UserAuthenticationSuccessful;
	public event System.EventHandler UserAuthenticationFailed;
	
	public string Name { get { return "Test"; }}
	public AuthenticationRequirement AuthenticationRequirement { get { return AuthenticationRequirement.Never; } }
	public string AuthenticationTip { get { return ""; }}
	
	public void RefreshAllBuilds ()
	{
		int buildsCount = m_builds.Count;
		
		for (int i = 0; i < buildsCount; i++) {
			var b = m_builds [i];
			b.Status = RandomStatus ();
			b.LastRanStep = new BuildStep () { 
				Name = "Test",
				StepType = (BuildStepType)Random.Range(0, 8)
			};
			
			b.PercentageComplete = Random.Range (0f, 1f);
			
			b.TriggeredBy = new BuildUser ();
			b.TriggeredBy.Name = "User " + Random.Range (0, 10);
			b.TriggeredBy.UserName = b.TriggeredBy.Name;
			b.TriggeredBy.Email = SHRandomHelper.NextBool () ? "giacomelli@gmail.com" : "giusepe@gmail.com";
			b.TriggeredBy.Builds.Add (b);
			
			b.LastChangeDescription = System.DateTime.Now.ToLongTimeString ();
			b.Date = System.DateTime.Now;
			BuildUpdated.Raise (this, new BuildUpdatedEventArgs (b));
		}
		
		BuildsRefreshed.Raise (this);
	}
	
	public void RunBuild (User user, Build build)
	{
		build.Status = BuildStatus.Running;
	}
	
	public void StopBuild (User user, Build build)
	{
		build.Status = BuildStatus.Canceled;
	}
	
	private BuildStatus RandomStatus ()
	{
		return (BuildStatus)Random.Range (1, (int)BuildStatus.RunningDeploy);
	}
	
	public void AuthenticateUser (User user)
	{
		if (string.IsNullOrEmpty (user.Password)) {
			UserAuthenticationFailed (this, System.EventArgs.Empty);
		} else {
			UserAuthenticationSuccessful(this, System.EventArgs.Empty);
		}
	}
	
	#endregion
}
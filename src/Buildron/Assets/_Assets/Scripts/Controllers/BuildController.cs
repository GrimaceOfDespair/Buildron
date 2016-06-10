#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using Buildron.Domain;
using Skahal.Camera;
using Skahal.Logging;
using Skahal.Rendering;
using UnityEngine;
using UnityEngine.UI;
#endregion

/// <summary>
/// Build controller.
/// </summary>
public class BuildController : MonoBehaviour
{
	#region Fields
	private static UnityEngine.Object s_buildPrefab = Resources.Load ("BuildPrefab");
	private static UnityEngine.Object s_buildFailedExplosionPrefab = Resources.Load ("BuildFailedExplosionPrefab");
	private static UnityEngine.Object s_buildSuccessFireworksPrefab = Resources.Load ("BuildSuccessFireworksPrefab");
	private static UnityEngine.Object s_buildHidingEffectPrefab = Resources.Load ("BuildHidingEffectPrefab");
	private BuildProgressBarController m_progressBar;
	private bool m_groundReachdAlreadRaised;
	private Text m_projectLabel;
	private Text m_configurationLabel;
	private Image m_runningStatusIcon;
	private Image m_userAvatarIcon;
	private GameObject m_body;
    private Rigidbody m_rigidbody;
    private Collider m_bodyCollider;
    private Collider m_topEdge;
	private Collider m_bottomEdge;
	private Collider m_leftEdge;
	private Collider m_rightEdge;
    private Renderer m_bodyRenderer;
    private Renderer m_focusedPanelRenderer;
	private bool m_isFirstCheckState = true;
    private BuildStatus m_lastCheckStateStatus;
	private GameObject m_focusedPanel;
    
	#endregion
	
	#region Properties
	public Build Data { get; set; }
	public bool IsHistoryBuild { get; set; }
	public bool IsVisible { get; private set; }
	public bool HasReachGround { get; private set; }
	
	public string ProjectText {
		get {
			return m_projectLabel.text;
		}
		
		set {
			if (m_projectLabel == null) {
				m_projectLabel = transform.FindChild ("Canvas/ProjectLabel").GetComponent<Text> ();
			}
			
			m_projectLabel.text = value;
		}
	}
	public static int GroundReachedCount { get; private set; }
	public static int VisiblesCount { get { return CountVisibles(); } }
	#endregion
	
	#region Editor properties
	public Material BuildHidingMaterial;
	
	public Color SuccessColor = new Color (1, 0, 0.95f, 1);
	public Color FailedColor = new Color(1, 0, 0, 1);
	public Color RunningColor = new Color (1, 0.98f, 0, 1);
	public Color QueuedColor = new Color (0.36f, 0.45f, 0.55f, 1);
	
	public Material UserAvatarIconMaterial;
	public Vector3 StatusChangedForce = new Vector3(0, 10, 0);
	public float BuildHighlightZChange = -2;
	public float VisibleMaxYVelocity = 0.1f;
	public Sprite[] BuildRunningIcons;
	#endregion
	
	#region Life cycle 
	private void Start ()
	{        
		IsVisible = true;
		
		if (m_projectLabel == null) {
			ProjectText = Data.Configuration.Project.Name;
		}
		
		m_configurationLabel = transform.FindChild ("Canvas/ConfigurationLabel").GetComponent<Text> ();
		m_configurationLabel.text = Data.Configuration.Name;
		
		m_runningStatusIcon = transform.FindChild ("Canvas/RunningStatusIcon").GetComponent<Image> ();
		m_runningStatusIcon.enabled = false;
		m_userAvatarIcon = transform.FindChild ("Canvas/UserAvatarIcon").GetComponent<Image> ();
		m_userAvatarIcon.enabled = false;
		
		m_body = transform.FindChild ("Buildron-Totem").gameObject;
		m_focusedPanel = transform.FindChild ("FocusedPanel").gameObject;		
		m_topEdge = transform.FindChild ("Edges/TopEdge").GetComponent<Collider>();
		m_bottomEdge = transform.FindChild ("Edges/BottomEdge").GetComponent<Collider>();
		m_leftEdge = transform.FindChild ("Edges/LeftEdge").GetComponent<Collider>();
		m_rightEdge = transform.FindChild ("Edges/RightEdge").GetComponent<Collider>();

        m_rigidbody = GetComponent<Rigidbody>();
        m_bodyCollider = GetComponent<Collider>();
        m_bodyRenderer = m_body.GetComponent<Renderer>();
        m_focusedPanelRenderer = m_focusedPanel.GetComponent<Renderer>();

        m_progressBar = GetComponentInChildren<BuildProgressBarController> ();
		CheckState ();
	
		if (!IsHistoryBuild) {
			Data.StatusChanged += delegate {
				if (Data.Status <= BuildStatus.Running && Data.Status != BuildStatus.Queued) {
					m_rigidbody.AddForce (StatusChangedForce);
				}
				
				CheckState ();
			};
				
			Data.TriggeredByChanged += delegate {
				UpdateUserAvatar ();	
			};
		}
		
		Messenger.Register (gameObject, 
			"OnRemoteControlConnected", 
			"OnRemoteControlDisconnected");
	} 

    private void OnRemoteControlConnected ()
	{
		var rc = RemoteControlService.GetConnectedRemoteControl ();
		var rcUserName = rc == null ? null : rc.UserName.ToLowerInvariant ();
		var buildUserName = Data.TriggeredBy == null ? null : Data.TriggeredBy.UserName.ToLowerInvariant ();
		
		if (!String.IsNullOrEmpty (rcUserName) 
			&& !String.IsNullOrEmpty (buildUserName)
			&& rcUserName.Equals (buildUserName)) {
			m_userAvatarIcon.enabled = true;
		}
	}
	
	private void OnRemoteControlDisconnected ()
	{
		m_userAvatarIcon.enabled = true;
		UpdateUserAvatar();
	}
	
	private void CheckState ()
	{
        SHLog.Debug("{0} | {1} | {2}", m_isFirstCheckState, m_lastCheckStateStatus, Data.Status);
        var statusChanged = m_isFirstCheckState || m_lastCheckStateStatus != Data.Status;      

        if (Data.IsRunning) {
			m_progressBar.Show ();
		} else {
			m_progressBar.Hide ();
		}

        Color color;

        switch (Data.Status)
        {
            case BuildStatus.Failed:
            case BuildStatus.Error:
            case BuildStatus.Canceled:
                color = FailedColor;
                UpdateRunningStatusIcon(true);                

                if (statusChanged)
                {
                    CreateFailedEffects();
                    Messenger.Send("OnBuildFailed", gameObject);
                }
                break;

            case BuildStatus.Success:
                color = SuccessColor;
                UpdateRunningStatusIcon(true);                

                if (statusChanged)
                {
                    CreateSuccessEffects();
                    Messenger.Send("OnBuildSuccess", gameObject);
                }
                break;

            case BuildStatus.Queued:
                color = QueuedColor;
                UpdateRunningStatusIcon(true);

                if (statusChanged)
                {
                    Messenger.Send("OnBuildQueued", gameObject);
                }
                break;

            default:
                color = RunningColor;
                UpdateRunningStatusIcon(false);

                m_progressBar.UpdateValue(Data.PercentageComplete);

                if (statusChanged)
                {
                    Messenger.Send("OnBuildRunning", gameObject);
                }
                break;
        }

        UpdateUserAvatar();

        m_bodyRenderer.materials[1].SetColor("_Color", color);
        m_focusedPanelRenderer.materials[1].SetColor("_Color", color);

        m_isFirstCheckState = false;
        m_lastCheckStateStatus = Data.Status;
    }		
				
	private void UpdateRunningStatusIcon (bool hide)
	{	
		if (Data.LastRanStep == null) {
			m_runningStatusIcon.sprite = BuildRunningIcons[(int)BuildStepType.None];
		} else {
			m_runningStatusIcon.sprite = BuildRunningIcons[(int)Data.LastRanStep.StepType];
		}			
		
		m_runningStatusIcon.enabled = IsVisible || !hide;
	}
	
	private void UpdateUserAvatar ()
	{
		m_userAvatarIcon.enabled = !(!IsVisible || Data.IsRunning);
		
		BuildUserService.GetUserPhoto (Data.TriggeredBy, (photo) => {
			m_userAvatarIcon.enabled = !(!IsVisible || Data.IsRunning);
			m_userAvatarIcon.sprite = photo.ToSprite();
		});
	}
	
	private void OnCollisionEnter ()
	{
		if (!m_groundReachdAlreadRaised && !IsHistoryBuild) {
			m_groundReachdAlreadRaised = true;
			GroundReachedCount++;
			HasReachGround = true;
			Messenger.Send ("OnBuildReachGround", gameObject);
		}
	}

	private void Hide ()
	{
		if (IsVisible)
        {            
			CheckState ();
			CreateHiddingEffect ();

            m_rigidbody.isKinematic = true;
			m_bodyCollider.enabled = false;
            m_topEdge.enabled = false;
            m_rightEdge.enabled = false;
            m_bottomEdge.enabled = false;
            m_leftEdge.enabled = false;
            
			m_body.SetActive(false);
			IsVisible = false;
			m_runningStatusIcon.enabled = false;
			m_userAvatarIcon.enabled = false;
			m_projectLabel.enabled = false;
			m_configurationLabel.enabled = false;
			m_progressBar.Hide ();
            HasReachGround = false;
            m_groundReachdAlreadRaised = false;
            Messenger.Send ("OnBuildHidden");

            WakeUpSleepingBuilds();
        }
	}

    /// <summary>
    /// Issue #9: https://github.com/skahal/Buildron/issues/9
    /// If a build was removed, maybe there are space between builds totems and some can be sleeping.
    /// Wake everyone!
    /// </summary>
    private void WakeUpSleepingBuilds()
    {      
        foreach (var visibleBuild in BuildController.GetVisibles())
        {
            var rb = visibleBuild.GetComponent<BuildController>().m_rigidbody;

            if (rb.IsSleeping())
            {
                SHLog.Debug("Wake up build game object: {0}", visibleBuild.name);
                rb.WakeUp();
            }
        }
    }
	
	private void Show ()
	{
		if (!IsVisible)
        {
			CheckState ();
			transform.position += Vector3.up * 20;

            m_rigidbody.isKinematic = false;
			m_bodyCollider.enabled = true;
            m_topEdge.enabled = true;
            m_rightEdge.enabled = true;
            m_bottomEdge.enabled = true;
            m_leftEdge.enabled = true;

            m_body.SetActive(true);
			IsVisible = true;			
			UpdateUserAvatar ();
			m_projectLabel.enabled = true;
			m_configurationLabel.enabled = true;

            if (Data.IsRunning)
            {
                m_runningStatusIcon.enabled = true;
                m_progressBar.Show();
            }

			Messenger.Send ("OnBuildVisible");
		}
	}
	
	private void CreateFailedEffects ()
	{
		if (!IsHistoryBuild) {
			var explosion = (GameObject)GameObject.Instantiate (s_buildFailedExplosionPrefab);
			explosion.transform.parent = transform;
			explosion.transform.position = transform.position;
			SHCameraHelper.Shake ();
		}
	}
	
	private void CreateSuccessEffects ()
	{
		if (!m_isFirstCheckState && !IsHistoryBuild) {
			var fireworks = (GameObject)GameObject.Instantiate (s_buildSuccessFireworksPrefab);
			fireworks.transform.parent = transform;
			fireworks.transform.position = transform.position;
		}
	}
	
	private void CreateHiddingEffect ()
	{
		var effect = (GameObject)GameObject.Instantiate (s_buildHidingEffectPrefab);
		effect.transform.parent = transform;
		effect.transform.position = transform.position;
		effect.GetComponent<ParticleSystem> ().Play ();
	}
	
	#region Static methods
	private static string GetName (Build b)
	{
		return b.Id;
	}
	
	public static GameObject GetGameObject (Build b)
	{
		return GameObject.Find (GetName(b));
	}
	
	public static bool ExistsGameObject (Build b)
	{
		return GetGameObject (b) != null;
	}
	
	public static GameObject CreateGameObject (Build b)
	{
		var go = GetGameObject (b);
		
		if (go == null) {
			go = (GameObject)GameObject.Instantiate (s_buildPrefab);
			var build = go.GetComponent<BuildController> ();
			build.Data = b;
			go.name = GetName (b);		
		}
		
		return go;
	}
	
	private static IEnumerable<GameObject> GetVisiblesQuery ()
	{
		var builds = GameObject.FindGameObjectsWithTag ("Build").Select (b => b.GetComponent<BuildController> ());	
		
		return builds.Where (
				b => b != null 
			&& b.m_body != null 
			&& b.IsVisible
			&& !b.IsHistoryBuild).Select (b => b.gameObject);
	}
	
	public static IList<GameObject> GetVisibles ()
	{
		return GetVisiblesQuery().ToList ();
		
	}
	
	public static int CountVisibles ()
	{
		return GetVisiblesQuery ().Count ();
	}
	
	public static bool HasNotVisiblesFromTop ()
	{
		var builds = GameObject.FindGameObjectsWithTag ("Build").Select (b => b.GetComponent<BuildController> ());	
	
		return builds.Any (
				b => b != null 
			&& b.HasReachGround
			&& b.IsVisible
			&& !b.IsHistoryBuild
			&& Mathf.Abs (b.m_rigidbody.velocity.y) <= b.VisibleMaxYVelocity
			&& !b.m_topEdge.IsVisibleFrom (Camera.main)
			&& (b.m_leftEdge.IsVisibleFrom (Camera.main) 
			|| b.m_rightEdge.IsVisibleFrom (Camera.main)
			|| b.m_bottomEdge.IsVisibleFrom (Camera.main)));
	}
	
	public static bool HasNotVisiblesFromSides ()
	{
		var builds = GameObject.FindGameObjectsWithTag ("Build").Select (b => b.GetComponent<BuildController> ());	

		return builds.Any (
				b => b != null 
			&& b.HasReachGround
			&& b.IsVisible
			&& !b.IsHistoryBuild
			&& Mathf.Abs (b.m_rigidbody.velocity.y) <= b.VisibleMaxYVelocity
			&& b.m_topEdge.IsVisibleFrom (Camera.main)
			&& (!b.m_leftEdge.IsVisibleFrom (Camera.main) 
			|| !b.m_rightEdge.IsVisibleFrom (Camera.main)
			|| !b.m_bottomEdge.IsVisibleFrom (Camera.main)));
	}
	
	public static IList<GameObject> GetVisiblesOrderByPosition ()
	{
		var buildsGO = GameObject.FindGameObjectsWithTag ("Build");
		var buildsControllers = buildsGO.Select (b => b.GetComponent<BuildController> ());
		
		var query = from c in buildsControllers
					where c.m_body != null 
						&& c.IsVisible
						&& !c.IsHistoryBuild
					orderby 
						Mathf.CeilToInt (c.m_body.transform.position.x) ascending, 
						Mathf.CeilToInt (c.m_body.transform.position.y) descending,
						c.gameObject.name ascending
					select c.gameObject;
		
		
		return query.ToList ();
	}

    /// <summary>
    /// Verify if all builds physics are sleeping.
    /// </summary>
    /// <remarks>
    /// Works well with the value of "Sleep Threshold" in the "Project settings\Physics" as "0.05".
    /// </remarks>
    /// <returns>True if all builds are sleeping.</returns>
    public static bool AreAllSleeping()
    {
        return BuildController.GetVisibles().All(b => b.GetComponent<BuildController>().m_rigidbody.IsSleeping());
    }

    public static bool HasAllReachGround()
    {
        return BuildController.GetVisibles().All(b => b.GetComponent<BuildController>().HasReachGround);
    }
    #endregion

    #endregion
}
using System.Xml;
using System.Text.RegularExpressions;
using Buildron.Domain.Users;

namespace Buildron.Infrastructure.BuildsProvider.Jenkins
{	
	/// <summary>
	/// A parser for User.
	/// </summary>
	public static class JenkinsUserParser
	{
		#region Fields
		private static Regex s_findTimerRegex = new Regex ("(Started by timer|Iniciado pelo temporizador)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		#endregion

		#region Methods
		public static User ParseUserFromBuildResponse (XmlDocument xmlDoc)
		{
			User user = null;
			var userNode = xmlDoc.SelectSingleNode ("//action/cause/userName");
			
			if (userNode == null) {
				userNode = xmlDoc.SelectSingleNode ("//changeSet/item/author/fullName");
			}
			
			if (userNode != null) {
				user = new User ();
				user.UserName = userNode.InnerText;
			}
			
			if (userNode == null) {
				var shortDescription = xmlDoc.SelectSingleNode ("//action/cause/shortDescription");
				
				if (shortDescription != null && s_findTimerRegex.IsMatch(shortDescription.InnerText)) {
					user = new User ();
					user.UserName = "timer";
					user.Kind = UserKind.ScheduledTrigger;
				}
			}
			

			return user;
		}
		
		public static User ParseUserFromUserResponse (XmlDocument xmlDoc)
		{
			var user = new User ();
			user.UserName = xmlDoc.SelectSingleNode ("//user/id").InnerText;
			user.Name = xmlDoc.SelectSingleNode ("//user/fullName").InnerText;
			
			var emailNode = xmlDoc.SelectSingleNode ("//user/property/address");
			
			if (emailNode != null) {
				user.Email = emailNode.InnerText;
			}
			
			return user;
		}
		#endregion
	}
}
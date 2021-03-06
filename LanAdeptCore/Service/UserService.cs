﻿using LanAdeptCore.Manager;
using LanAdeptData.DAL;
using LanAdeptData.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanAdeptCore.Service
{
    public static class UserService
	{
		private static UnitOfWork uow
		{
			get { return UnitOfWork.Current; }
		}

		public static User GetLoggedInUser()
		{
			if (!IsUserLoggedIn())
				return null;

			return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(HttpContext.Current.User.Identity.GetUserId());
		}

		public static bool IsUserLoggedIn()
		{
			return HttpContext.Current.User.Identity.IsAuthenticated;
		}

		public static bool IsTeamLeader(int tournamentID)
		{
			if (IsUserLoggedIn())
			{
				IEnumerable<Team> teams = uow.TeamRepository.GetByTeamLeaderID(GetLoggedInUser().Id);
				return teams.Where(t => t.Tournament.TournamentID == tournamentID).Count() > 0;
			}

			return false;
		}

		public static bool IsTeamLeader()
		{
			if (IsUserLoggedIn())
			{
				return uow.TeamRepository.GetByTeamLeaderID(GetLoggedInUser().Id).Count() > 0;
			}

			return false;
		}

		public static int GetNbTeamDemand()
		{
			if (!IsTeamLeader())
				return 0;

			int nbDemands = 0;
			IEnumerable<Team> teams = uow.TeamRepository.GetByTeamLeaderID(GetLoggedInUser().Id);

			foreach (Team team in teams)
			{
				nbDemands += team.Demandes.Count;
			}

			return nbDemands;
		}

		public static int GetNbTeams()
		{
			if (!IsTeamLeader())
				return 0;

			IEnumerable<Team> teams = uow.TeamRepository.GetByTeamLeaderID(GetLoggedInUser().Id);

			return teams.Count();
		}
	}
}

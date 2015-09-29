﻿using LanAdeptData.Model;
using System.Collections.Generic;

namespace LanAdept.Views.Tournament.ModelController
{
	public class TeamModel
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public int TournamentID { get; set; }

		public virtual int UserID { get; set; }

		public virtual User TeamLeader { get; set; }

		public virtual LanAdeptData.Model.Tournament Tournament { get; set; }

		public virtual ICollection<User> Users { get; set; }

		public TeamModel()
		{

		}

		public TeamModel(Team team)
		{
			Id = team.TeamID;
			Name = team.Name;
			Users = team.Users;
			UserID = team.UserID;
			TeamLeader = team.TeamLeader;
		}
	}
}
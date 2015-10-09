﻿using LanAdeptData.DAL;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Net;
using LanAdeptData.Model;
using LanAdeptCore.Service;
using System.Data;
using LanAdeptCore.Attribute.Authorization;
using System.Linq;

namespace LanAdept.Views.Tournament.ModelController
{
	public class TournamentController : Controller
	{
		UnitOfWork uow = UnitOfWork.Current;

		[AllowAnonymous]
		public ActionResult Index()
		{
			List<TournamentModel> tournamentModels = new List<TournamentModel>();
			IEnumerable<LanAdeptData.Model.Tournament> tournaments = uow.TournamentRepository.Get();
			foreach (LanAdeptData.Model.Tournament tournament in tournaments)
			{
				TournamentModel tournamentModel = new TournamentModel(tournament);
				List<TeamModel> teamModels = new List<TeamModel>();
				foreach (LanAdeptData.Model.Team team in tournament.Teams)
				{
					TeamModel teamModel = new TeamModel(team);
					teamModels.Add(teamModel);
				}
				tournamentModel.Teams = teamModels;
				tournamentModels.Add(tournamentModel);
			}
			return View(tournamentModels);
		}

		[AllowAnonymous]
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			LanAdeptData.Model.Tournament tournament = uow.TournamentRepository.GetByID(id);

			if (tournament == null)
			{
				return HttpNotFound();
			}

			TournamentModel tournamentModel = new TournamentModel(tournament);
			if (UserService.IsUserLoggedIn())
			{
				tournamentModel.CanAddTeam = true;
				tournamentModel.IsTeamLeader = UserService.IsTeamLeader();
			}


			List<TeamModel> teamModels = new List<TeamModel>();
			foreach (LanAdeptData.Model.Team team in tournament.Teams)
			{
				TeamModel teamModel = new TeamModel(team);

				if (UserService.IsUserLoggedIn())
				{
					if (team.TeamLeaderTag.UserID == UserService.GetLoggedInUser().UserID)
					{
						tournamentModel.IsTeamLeader = true;
						tournamentModel.CanAddTeam = false;
						teamModel.IsMyTeamForTeamLeader = true;
						teamModel.IsTeamDemande = false;
						teamModel.IsMyTeam = false;
					}
					else
					{
						foreach (GamerTag gamer in team.GamerTags)
						{
							if (gamer.User.UserID == UserService.GetLoggedInUser().UserID)
							{
								tournamentModel.CanAddTeam = false;
								teamModel.IsMyTeamForTeamLeader = false;
								teamModel.IsTeamDemande = false;
								teamModel.IsMyTeam = true;
							}
						}

						if (!teamModel.IsMyTeam)
						{
							List<Demande> demandes = uow.DemandeRepository.GetByTeamId(team.TeamID);
							foreach (Demande demande in demandes)
							{
								if (demande.Team.TeamID == team.TeamID)
								{
									tournamentModel.CanAddTeam = false;
									teamModel.IsMyTeamForTeamLeader = false;
									teamModel.IsTeamDemande = true;
									teamModel.IsMyTeam = false;
								}
							}
						}
					}
				}
				teamModels.Add(teamModel);
			}

			tournamentModel.Teams = teamModels;

			User user = UserService.GetLoggedInUser();
			if (user != null)
			{
				tournamentModel.IsConnected = true;
				tournamentModel.GamerTags = uow.GamerTagRepository.GetByUser(user);
			}

			return View(tournamentModel);
		}

		[AuthorizePermission("user.tournament.team.add")]
		public ActionResult Addteam(int id)
		{
			LanAdeptData.Model.Tournament tournament = uow.TournamentRepository.GetByID(id);
			foreach (LanAdeptData.Model.Team team in tournament.Teams)
			{
				if (team.TeamLeaderTag.User == UserService.GetLoggedInUser())
				{
					return RedirectToAction("Details", new { id = team.Tournament.TournamentID });
				}
			}
			foreach (Demande demande in uow.DemandeRepository.Get())
			{
				if (demande.Team.TournamentID == tournament.TournamentID)
				{
					return RedirectToAction("Details", new { id = tournament.TournamentID });
				}
			}

			AddTeamModel model = new AddTeamModel();
			model.Tournament = tournament;
			model.TournamentID = model.Tournament.TournamentID;
			model.GamerTags = uow.GamerTagRepository.GetByUser(UserService.GetLoggedInUser());
			return View(model);
		}

		[AuthorizePermission("user.tournament.team.add")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Addteam(AddTeamModel teamModel)
		{
			LanAdeptData.Model.Tournament tournament = uow.TournamentRepository.GetByID(teamModel.TournamentID);

			if (tournament.IsStarted || tournament.IsOver)
			{
				return RedirectToAction("Details", new { id = tournament.TournamentID });
			}

			foreach (LanAdeptData.Model.Team team in tournament.Teams)
			{
				if (team.TeamLeaderTag.User == UserService.GetLoggedInUser())
				{
					return RedirectToAction("Details", new { id = tournament.TournamentID });
				}
			}

			if (ModelState.IsValid)
			{
				LanAdeptData.Model.Team team = new LanAdeptData.Model.Team();
				team.Name = teamModel.Name;
				team.Tag = teamModel.Tag;
				User user = UserService.GetLoggedInUser();
				team.TeamLeaderTag = uow.GamerTagRepository.GetByUserAndGamerTagID(user, teamModel.GamerTagId);

				List<GamerTag> listGamerTags = new List<GamerTag>();
				listGamerTags.Add(team.TeamLeaderTag);
				team.GamerTags = listGamerTags;
				team.Tournament = uow.TournamentRepository.GetByID(teamModel.TournamentID);

				uow.TeamRepository.Insert(team);

				if (team.Tournament.Teams == null)
				{
					ICollection<LanAdeptData.Model.Team> teamList;
					teamList = new List<LanAdeptData.Model.Team>();
					teamList.Add(team);
					team.Tournament.Teams = teamList;
				}
				else
				{
					team.Tournament.Teams.Add(team);
				}

				uow.TournamentRepository.Update(team.Tournament);

				uow.Save();
				return RedirectToAction("Details", new { id = team.Tournament.TournamentID });
			}

			teamModel.GamerTags = uow.GamerTagRepository.GetByUser(UserService.GetLoggedInUser());
			return View(teamModel);
		}

		[AuthorizePermission("user.tournament.gamertag.add")]
		public ActionResult AddGamerTag(string gamertag)
		{
			User user = UserService.GetLoggedInUser();

			if (!uow.GamerTagRepository.HasSameGamerTag(user, gamertag))
			{
				GamerTag gamerTag = new GamerTag();
				gamerTag.Gamertag = gamertag;
				gamerTag.User = user;

				uow.GamerTagRepository.Insert(gamerTag);
				uow.Save();

				return Json(new GamerTagResponse() { HasError = false, ErrorMessage = "", GamerTagID = gamerTag.GamerTagID, Gamertag = gamerTag.Gamertag }, JsonRequestBehavior.AllowGet);
			}

			return Json(new GamerTagResponse() { HasError = true, ErrorMessage = "Vous avez déja un GamerTag avec ce nom", GamerTagID = 0, Gamertag = gamertag }, JsonRequestBehavior.AllowGet); ;
		}

		[AuthorizePermission("user.tournament.team.join")]
		public ActionResult JoinTeam(JoinTeamModel model)
		{
			if (model.GamerTagID == null || model.TournamentID == null || model.TeamID == null)
			{
				//TODO : Add client error
				return HttpNotFound();
			}

			LanAdeptData.Model.Tournament tournament = uow.TournamentRepository.GetByID(model.TournamentID);
			if (tournament.IsStarted || tournament.IsOver)
			{
				return RedirectToAction("Details", new { id = tournament.TournamentID });
			}

			foreach (Demande item in uow.DemandeRepository.GetByTeamId(model.TeamID))
			{
				if (item.GamerTag.GamerTagID == model.GamerTagID)
				{
					return RedirectToAction("Details", new { id = tournament.TournamentID });
				}
			}

			Demande demande = new Demande();
			User user = UserService.GetLoggedInUser();

			GamerTag gamerTag = uow.GamerTagRepository.GetByUserAndGamerTagID(user, model.GamerTagID.Value);
			if (gamerTag == null)
			{
				//TODO : Add client error
				return HttpNotFound();
			}

			demande.GamerTag = gamerTag;

			LanAdeptData.Model.Team team = uow.TeamRepository.GetByID(model.TeamID);
			if (team == null)
			{
				//TODO : Add client error
				return HttpNotFound();
			}
			demande.Team = team;

			uow.DemandeRepository.Insert(demande);
			uow.Save();

			return RedirectToAction("Details", new { id = model.TournamentID });
		}
	}
}
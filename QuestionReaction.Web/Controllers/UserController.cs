﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuestionReaction.Data;
using QuestionReaction.Services.Interfaces;
using QuestionReaction.Services.Models;
using QuestionReaction.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuestionReaction.Web.Controllers
{
    [Authorize]
    public class UserController: Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IPollService _pollService;
        private readonly AppDbContext _ctx;
        private readonly IUserService _userService;
        private int _currentUserId => int.Parse(User.Claims.Single(u => u.Type == "id").Value);
        public UserController(ILogger<UserController> logger, IPollService pollService, AppDbContext ctx, IUserService userService)
        {
            _logger = logger;
            _pollService = pollService;
            _ctx = ctx;
            _userService = userService;
        }

        public async Task<IActionResult> Polls()
        {
            var model = new UserPollsVM();

            var user = await _userService.GetUserByIdAsync(_currentUserId);

            // liste des sondages créés par l'utilisateur
            model.CreatedPolls = _pollService.GetQuestionsByUserIdAsync(_currentUserId)
                .Result
                .Select(p => new QuestionsVM()
                {
                    Id = p.Id,
                    Title = p.Title,
                    MultipleChoices = p.MultipleChoices,
                    VoteUid = p.VoteUid,
                    ResultUid = p.ResultUid,
                    IsActive = p.IsActive
                })
                .ToList();

            // liste des sondages auxquels l'utilisateur à été invité sauf ceux qu'il a créé
            var allPolls = await _pollService.GetQuestionsByGuestAsync(user.Mail);
            if (allPolls != null)
            {
                model.JoinedPolls = allPolls
                    .Where(q => q.User != user)
                    .Select(q => new QuestionsVM()
                    {
                        Id = q.Id,
                        Title = q.Title,
                        MultipleChoices = q.MultipleChoices,
                        VoteUid = q.VoteUid,
                        ResultUid = q.ResultUid,
                        IsActive = q.IsActive
                    })
                    .ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AddPolls()
        {
            var model = new UserAddPollsVM() { CurrentUserId = _currentUserId };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPolls(UserAddPollsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                model.Choices = new List<string>
                    {
                        model.Choice1, model.Choice2, model.Choice3, model.Choice4, model.Choice5
                    }
                    .Where(c => c != null)
                    .ToList();
                var pollId = await _pollService.AddPollAsync(model);

                return RedirectToAction(nameof(PollsLinks), new { pollId = pollId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PollsLinks(int pollId)
        {
            var poll = await _pollService.GetQuestionByIdAsync(pollId);
            var linkBase = "https://" + Request.Host.Value + "/User/";
            var model = new PollsLinksPageVM()
            {
                VoteLink = linkBase + "Vote?voteUid=" + poll.VoteUid,
                VoteUid = poll.VoteUid,
                ResultLink = linkBase + "Result?resultUid=" + poll.ResultUid,
                ResultUid = poll.ResultUid,
                DisableLink = linkBase + "Disable?disableUid=" + poll.DisableUid,
                DisableUid = poll.DisableUid,
                IsActive = poll.IsActive
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Disable(string disableUid)
        {
            await _pollService.DisableQuestionAsync(disableUid);
            return RedirectToAction(nameof(Polls));
        }

        [HttpGet]
        public async Task<IActionResult> Vote(string voteUid)
        {
            var question = await _pollService.GetQuestionByVoteUidAsync(voteUid);
            var userMail = _userService.GetUserByIdAsync(_currentUserId).Result.Mail;

            if (await _ctx.Guests
                .Where(g => g.Mail == userMail)
                .Where(g => g.Question == question)
                .SingleAsync() == null) // le mail de l'utilisateur n'est pas dans la liste des invités
            {
                return RedirectToAction(nameof(ErrorNotInvited));
            }
            else // l'utilisateur est invité et peut donc voter
            {
                var model = new VoteVM()
                {
                    Question = question,
                    VoteNumber = question.Reactions
                        .Where(r => r.QuestionId == question.Id)
                        .Count()
                };
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Vote(VoteVM model)
        {
            var resultUid = await _pollService.AddReactionsAsync(
                model.SelectedChoices.ToList(),
                _currentUserId);
            return RedirectToAction(nameof(Result), new { resultUid = resultUid });
        }

        [HttpGet]
        public async Task<IActionResult> Result(string resultUid)
        {
            var question = await _pollService.GetQuestionByResultUidAsync(resultUid);
            var model = new ResultVM()
            {
                Question = question,
                SortedChoices = await _pollService.SortChoicesByVoteNumber(question.Id),
                VoteNumber = question.Reactions
                        .Where(r => r.QuestionId == question.Id)
                        .Count()
            };
            return View(model);
        }

        public IActionResult ErrorNotInvited()
        {
            return View();
        }
    }
}

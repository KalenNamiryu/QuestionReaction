﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuestionReaction.Data;
using QuestionReaction.Data.Model;
using QuestionReaction.Services.Interfaces;
using QuestionReaction.Services.Models;
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

        public IActionResult Polls()
        {
            var model = new UserPollsVM()
            {
                CreatedPolls = new List<QuestionsVM>()
                {
                    new QuestionsVM()
                    {
                        Title = "quest 1"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 2"
                    }
                },
                JoinedPolls = new List<QuestionsVM>()
                {
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 3"
                    },
                    new QuestionsVM()
                    {
                        Title = "quest 4"
                    }
                }
            };

            var user = _userService.GetUserByIdAsync(_currentUserId);

            // liste des sondages auxquels l'utilisateur à été invité sauf ceux qu'il a créé
            model.JoinedPolls = _ctx.Questions
                .Where(q => q.UserId == user.Id)
                .Select(q => new QuestionsVM()
                {
                    Id = q.Id,
                    Title = q.Title,
                    MultipleChoices = q.MultipleChoices,
                    VoteUid = q.VoteUid,
                    ResultUid = q.ResultUid
                })
                .ToList();

            var guests = _pollService.GetGuestsByQuestionId(1);

            // liste des sondages de l'utilisateur
            model.CreatedPolls = _ctx.Questions
                .Where(q => q.UserId == user.Id)
                .Select(q => new QuestionsVM()
                {
                    Id = q.Id,
                    Title = q.Title,
                    MultipleChoices = q.MultipleChoices,
                    VoteUid = q.VoteUid,
                    ResultUid = q.ResultUid
                })
                .ToList();
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
                await _pollService.AddPollAsync(model);
                return RedirectToAction(nameof(Polls)); // redirection a changer vers la page des liens
            }
        }

    }
}

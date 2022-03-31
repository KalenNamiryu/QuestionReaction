﻿using QuestionReaction.Data.Model;
using QuestionReaction.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionReaction.Services.Interfaces
{
    public interface IPollService
    {
        /// <summary>
        /// Ajout d'un sondage dans la BDD a partir d'un ViewModel
        /// </summary>
        /// <param name="model"></param>
        Task AddPollAsync(UserAddPollsVM model);
        /// <summary>
        /// Créé un guid au format string sans les tirets
        /// </summary>
        /// <returns>guid au format string</returns>
        string AddGuid();
        /// <summary>
        /// Récupération des invités d'un sondage
        /// </summary>
        /// <param name="questionId">id du sondage</param>
        /// <returns>Liste de Guest</returns>
        Task<List<Guest>> GetGuestsByQuestionId(int questionId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guestMail"></param>
        /// <returns></returns>
        Task<List<Question>> GetPollsByGuestMailAsync(string guestMail);

    }
}

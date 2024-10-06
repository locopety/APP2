using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Sanssoussi.Areas.Identity.Data;
using Sanssoussi.Models;

namespace Sanssoussi.Controllers
{
    public class HomeController : Controller
    {
        private readonly SqliteConnection _dbConnection;

        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<SanssoussiUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<SanssoussiUser> userManager, IConfiguration configuration)
        {
            this._logger = logger;
            this._userManager = userManager;
            this._dbConnection = new SqliteConnection(configuration.GetConnectionString("SanssoussiContextConnection"));
        }

        public IActionResult Index()
        {
            this.ViewData["Message"] = "Parce que marcher devrait se faire SansSoussi";
            return this.View();
        }

        [HttpGet]
        public async Task<IActionResult> Comments()
        {
            var comments = new List<string>();

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this._logger.LogInformation("Unauthorized user attempted to get a comment");
                Unauthorized(new { message = "Vous devez vous connecter" });
                return this.View(comments);
            }

            var cmd = new SqliteCommand($"Select Comment from Comments where UserId = @UserId", this._dbConnection);
            cmd.Parameters.AddWithValue("@UserId", user.Id);
            this._dbConnection.Open();
            var rd = await cmd.ExecuteReaderAsync();

            while (rd.Read())
            {
                comments.Add(HttpUtility.HtmlEncode(rd.GetString(0)));
            }
            //this._logger.LogInformation($"user {user.Id} display all his comments");

            rd.Close();
            this._dbConnection.Close();

            return this.View(comments);
        }

        [HttpPost]
        public async Task<IActionResult> Comments(string comment)
        {
            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this._logger.LogInformation("Unauthorized user attempted to post a comment");
                return Unauthorized(new { message = "Vous devez vous connecter" });
            }

            // sanitize user input by encoding string to html
            string sanitizedComment = HttpUtility.HtmlEncode(comment);

            var cmd = new SqliteCommand(
                $"insert into Comments (UserId, CommentId, Comment) Values (@UserId, @CommentId, @Comment)",
                this._dbConnection);

            cmd.Parameters.AddWithValue("@UserId", user.Id);
            cmd.Parameters.AddWithValue("@CommentId", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@Comment", sanitizedComment);

            this._dbConnection.Open();
            await cmd.ExecuteNonQueryAsync();

            this._logger.LogInformation($"user {user.Id} add new comment : {sanitizedComment}");

            return this.Ok("Commentaire ajouté");
        }

        public async Task<IActionResult> Search(string searchData)
        {
            var searchResults = new List<string>();

            var user = await this._userManager.GetUserAsync(this.User);
            if (user == null)
            {
                this._logger.LogInformation("Unauthorized user attempted to search");
                return this.View(searchResults);
            }
            if (string.IsNullOrEmpty(searchData))
            {
                return this.View(searchResults);
            }

            string sanitizedsearchData = HttpUtility.HtmlEncode(searchData);

            var cmd = new SqliteCommand($"Select Comment from Comments where UserId = @UserId and Comment like @searchData", this._dbConnection);

            cmd.Parameters.AddWithValue("@UserId", user.Id);
            cmd.Parameters.AddWithValue("@searchData", "%" + sanitizedsearchData + "%");

            this._dbConnection.Open();
            var rd = await cmd.ExecuteReaderAsync();
            while (rd.Read())
            {
                searchResults.Add(HttpUtility.HtmlEncode(rd.GetString(0)));
            }

            this._logger.LogInformation($"user {user.Id} search for comment : {searchData}");

            rd.Close();
            this._dbConnection.Close();

            return this.View(searchResults);
        }

        public IActionResult About()
        {
            return this.View();
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Emails()
        {
            return this.View();
        }
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Emails(object form)
        {
            var searchResults = new List<string>();

            var user = await this._userManager.GetUserAsync(this.User);
            var roles = await this._userManager.GetRolesAsync(user);
            if (roles.Contains("admin"))
            {
                var cmd = new SqliteCommand("select Email from AspNetUsers", this._dbConnection);
                this._dbConnection.Open();
                var rd = await cmd.ExecuteReaderAsync();
                while (rd.Read())
                {
                    searchResults.Add(rd.GetString(0));
                }

                rd.Close();

                this._dbConnection.Close();
            }

            return this.Json(searchResults);
        }
        */
    }
}
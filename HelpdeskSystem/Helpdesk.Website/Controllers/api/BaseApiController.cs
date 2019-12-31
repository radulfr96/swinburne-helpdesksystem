using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Helpdesk.Common.Responses;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Helpdesk.Website.Controllers.api
{
    /// <summary>
    /// This is used as the base for all of the api controllers as all them will need
    /// the methods and attributes contained within this object
    /// </summary>
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected helpdesksystemContext context;

        protected static Logger s_logger = LogManager.GetCurrentClassLogger();

        public BaseApiController(helpdesksystemContext _context)
        {
            context = _context;
        }

        /// <summary>
        /// Used to generate a readable and formatted bad request message to return to the user
        /// </summary>
        /// <param name="response">The response with the bad request messages</param>
        /// <returns></returns>
        protected string BuildBadRequestMessage(BaseResponse response)
        {
            string message = string.Empty;
            try
            {
                foreach (StatusMessage statusMessage in response.StatusMessages)
                    message += statusMessage.Message + "\n";
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to build bad request message.");
                message = string.Empty;
            }

            return message;
        }

        /// <summary>
        /// Used to verify that the user is authorized to use the confidential parts of the system
        /// </summary>
        /// <returns>Indicates whether or not the user is authorized</returns>
        protected bool IsAuthorized()
        {
            bool result = true;
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                string UserName = identity.Name;

                string id = identity.Claims.Where(c => c.Type.Contains(@"/sid")).FirstOrDefault().Value;

                if (!identity.IsAuthenticated)
                {
                    s_logger.Warn($"{identity.Name} IsAuthenticated = false");
                    return false;
                }

                var facade = new UsersFacade(new UsersDataLayer(context));
                result = facade.VerifyUser(UserName, id);
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to check user is authorised.");
                result = false;
            }
            return result;
        }

        protected string GetUsername()
        {
            string username = "";

            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                username = identity.Name;
            }
            catch(Exception ex)
            {
                s_logger.Error(ex, "Unable to get username from identity.");
            }

            return username;
        }
    }
}
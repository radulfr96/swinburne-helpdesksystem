using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.Services;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Helpdesk.Website
{
    /// <summary>
    /// Job used to export the system database to a ZIP file with CSVs.
    /// </summary>
    public class ExportDatabaseJob : IJob
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();

        private helpdesksystemContext dbContext;

        public ExportDatabaseJob()
        {
            dbContext = new helpdesksystemContext();
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var facade = new HelpdeskFacade(
                new HelpdeskDataLayer(dbContext)
                , new UsersDataLayer(dbContext)
                , new UnitsDataLayer(dbContext)
                , new TopicsDataLayer(dbContext)
                , new StudentDataLayer(dbContext)
                , new QueueDataLayer(dbContext)
                , new CheckInDataLayer(dbContext));
                if (facade.ExportDatabase().Status != HttpStatusCode.OK)
                {
                    s_logger.Error("Unable to export database.");
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to export database.");
            }
            return Task.CompletedTask;
        }
    }
}

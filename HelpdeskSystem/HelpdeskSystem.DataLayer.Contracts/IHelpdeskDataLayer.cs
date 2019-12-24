using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Helpdesk;
using Helpdesk.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// This is used to perform CRUD actions for the helpdesks and timespans
    /// </summary>
    public interface IHelpdeskDataLayer : IDisposable
    {
        /// <summary>
        /// This method is used to add a new helpdesk to the database
        /// </summary>
        /// <param name="request">The information of the helpdesk</param>
        /// <returns>The id of the helpdesk that was added</returns>
        void AddHelpdesk(Helpdesksettings helpdesk);

        /// <summary>
        /// Used to get a helpdesk from the database
        /// </summary>
        /// <param name="id">The id of the helpdesk requested</param>
        /// <returns>The resulting DTO of the helpdesk</returns>
        Helpdesksettings GetHelpdesk(int id);

        /// <summary>
        /// Used to retreive all the helpdesks
        /// </summary>
        /// <returns>The list of all the helpdesks as DTOs</returns>
        List<Helpdesksettings> GetHelpdesks();

        /// <summary>
        /// Used to get a datatable with all of the helpdesk records
        /// </summary>
        /// <returns>Datatable with the helpdesk records</returns>
        DataTable GetHelpdesksAsDataTable();

        /// <summary>;
        /// Used to get a datatable with all of the helpdeskunit records
        /// </summary>
        /// <returns>Datatable with the helpdeskunit records</returns>
        DataTable GetHelpdeskUnitsAsDataTable();

        /// Used to get a datatable with all of the timespan records
        /// </summary>
        /// <returns>Datatable with the timespans records</returns>
        DataTable GetTimeSpansAsDataTable();

        /// <summary>
        /// Used to retreive all the active helpdesks
        /// </summary>
        /// <returns>The list of all the active helpdesks as DTOs</returns>
        List<Helpdesksettings> GetActiveHelpdesks();

        /// <summary>
        /// This method adds a timespan to the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The id of the timespan added</returns>
        void AddTimeSpan(Timespans timespan);

        /// <summary>
        /// Used to retreve a timespan by its id
        /// </summary>
        /// <param name="id">The id of the timespan</param>
        /// <returns>The timespan DTO</returns>
        Timespans GetTimeSpan(int id);

        Timespans GetTimeSpanByName(string name);

        /// <summary>
        /// This method retrieves a list of all the timespans in the database
        /// </summary>
        /// <returns>A list of timespans retrieved from the database</returns>
        List<Timespans> GetTimeSpans();

        /// <summary>
        /// Used to delete a specific timespan from the database
        /// </summary>
        /// <param name="id">The SpanID of the timespan to be deleted</param>
        /// <returns>Boolean indicating success or failure</returns>
        bool DeleteTimeSpan(Timespans timespan);

        void Save();
    }
}

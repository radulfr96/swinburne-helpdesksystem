using Helpdesk.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle CRUD for topic records in the database
    /// </summary>
    public interface ITopicsDataLayer
    {
        /// <summary>
        /// This method retrieves all topics of a specific unit from the database
        /// </summary>
        /// <param name="id">ID of the unit to retrieve topics of</param>
        /// <returns>A list of topics</returns>
        List<TopicDTO> GetTopicsByUnitID(int id);

        /// <summary>
        /// Used to get a datatable with all of the topic records
        /// </summary>
        /// <returns>Datatable with the topic records</returns>
        DataTable GetTopicsAsDataTable();
    }
}

using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Units;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle CRUD for unit records in the database
    /// </summary>
    public interface IUnitsDataLayer
    {
        /// <summary>
        /// Adds a unit to the database using provided unit request.
        /// </summary>
        /// <param name="request">The request containing the information to add a unit.</param>
        /// <returns>The id of the unit.</returns>
        int? AddUnit(AddUpdateUnitRequest request);

        /// <summary>
        /// Retrieves a unit from the database using provided unit id.
        /// </summary>
        /// <param name="id">The id of the unit to retrieve.</param>
        /// <returns></returns>
        UnitDTO GetUnit(int id);

        /// <summary>
        /// Retrieves a unit from the database using provided unit name.
        /// </summary>
        /// <param name="name">The name of the unit to retrieve.</param>
        /// <returns>The unit DTO</returns>
        UnitDTO GetUnitByNameAndHelpdeskId(string name, int helpdeskId);

        /// <summary>
        /// Retrieves all units under a specific helpdesk id
        /// </summary>
        /// <param name="id">ID of the helpdesk to retrieve from</param>
        /// <returns>A list of unit DTOs</returns>
        List<UnitDTO> GetUnitsByHelpdeskID(int id, bool getActive);

        /// <summary>
        /// Retrieves a unit from the database using provided unit code.
        /// </summary>
        /// <param name="code">The name of the unit to retrieve.</param>
        /// <returns>The unit DTO</returns>
        UnitDTO GetUnitByCodeAndHelpdeskId(string code, int helpdeskId);

        /// <summary>
        /// Used to get a datatable with all of the unit records
        /// </summary>
        /// <returns>Datatable with the unit records</returns>
        DataTable GetUnitsAsDataTable();

        /// <summary>
        /// Updates a specific unit
        /// </summary>
        /// <param name="id">ID of the unit to be deleted</param>
        /// <returns>Indication of the result of the operation</returns>
        bool UpdateUnit(int id, AddUpdateUnitRequest request);

        /// <summary>
        /// Deletes a specific unit
        /// </summary>
        /// <param name="id">ID of the unit to be deleted</param>
        /// <returns>Indication of the result of the operation</returns>
        bool DeleteUnit(int id);
    }
}

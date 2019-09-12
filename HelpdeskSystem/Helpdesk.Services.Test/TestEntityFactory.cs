﻿using System;
using System.Collections.Generic;
using Helpdesk.Common.Requests;
using Helpdesk.Common.Requests.Helpdesk;
using Helpdesk.Common.Requests.Units;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.Helpdesk;
using Helpdesk.Common.Responses.Units;
using Helpdesk.Common.Utilities;

namespace Helpdesk.Services.Test
{
    /// <summary>
    /// Helpdesk test data to be returned by TestEntityFactory. Includes Request and Response used for DB entity creation.
    /// </summary>
    public class TestDataHelpdesk
    {
        public TestDataHelpdesk (AddHelpdeskRequest request, AddHelpdeskResponse response)
        {
            Request = request;
            Response = response;
        }
        public AddHelpdeskRequest Request { get; }
        public AddHelpdeskResponse Response { get; }
    }

    /// <summary>
    /// Unit test data to be returned by TestEntityFactory. Includes Request and Response used for DB entity creation.
    /// </summary>
    public class TestDataUnit
    {
        public TestDataUnit(AddUpdateUnitRequest request, AddUpdateUnitResponse response)
        {
            Request = request;
            Response = response;
        }
        public AddUpdateUnitRequest Request { get; }
        public AddUpdateUnitResponse Response { get; }
    }

    /// <summary>
    /// Class with methods for generating test entities on the database.
    /// </summary>
    public class TestEntityFactory
    {
        public TestEntityFactory(bool populateEmptyStrings = true)
        {
            PopulateEmptyStrings = populateEmptyStrings;
        }

        /// <summary>
        /// Adds a test helpdesk in the database.
        /// </summary>
        /// <param name="name">Name of the Helpdesk (auto-generates if not provided, or empty string is provided).</param>
        /// <param name="hasCheckin">Determines if the helpdesk utilises the check-in/check-out functionality.</param>
        /// <param name="hasQueue">Determines if the helpdesk utilises the queue functionality.</param>
        /// <returns></returns>
        public TestDataHelpdesk AddHelpdesk(string name = "", bool? hasCheckin = true, bool? hasQueue = true)
        {
            var request = new AddHelpdeskRequest();
            if (name == "" && PopulateEmptyStrings) request.Name = AlphaNumericStringGenerator.GetString(10); else request.Name = name;

            request.Name = string.IsNullOrEmpty(name) && PopulateEmptyStrings ? AlphaNumericStringGenerator.GetString(10) : name;

            request.HasCheckIn = (bool)hasCheckin;
            request.HasQueue = (bool)hasQueue;

            var facade = new HelpdeskFacade();
            var response = facade.AddHelpdesk(request);

            TestDataHelpdesk data = new TestDataHelpdesk(request, response);
            return data;
        }

        /// <summary>
        /// Adds a test unit to the database.
        /// </summary>
        /// <param name="unitID">The ID of the unit to update. Should not be provided if adding a new helpdesk.</param>
        /// <param name="helpdeskID">The ID of the helpdesk that the unit is being added to.</param>
        /// <param name="name">Name of the unit (auto-generates if not provided, or empty string is provided).</param>
        /// <param name="code">Code of the unit (auto-generates if not provided, or empty string is provided).</param>
        /// <param name="isDeleted">Whether or not the helpdesk is removed from the system.</param>
        /// <param name="topics">A list of topics associated with the unit.</param>
        /// <returns></returns>
        public TestDataUnit AddUpdateUnit(int unitID = 0, int? helpdeskID = null, string name = "", string code = "", bool? isDeleted = false, List<string> topics = null)
        {
            var request = new AddUpdateUnitRequest();
            if (helpdeskID != null) request.HelpdeskID = (int)helpdeskID;
            if (name != null)
            {
                if (name == "" && PopulateEmptyStrings) request.Name = AlphaNumericStringGenerator.GetString(10); else request.Name = name;
            }
            if (code != null)
            {
                if (code == "" && PopulateEmptyStrings) request.Code = AlphaNumericStringGenerator.GetString(8); else request.Code = code;
            }
            if (isDeleted != null) request.IsDeleted = (bool)isDeleted;
            if (topics != null) request.Topics = topics;

            var facade = new UnitsFacade();
            var response = facade.AddOrUpdateUnit(unitID, request);

            TestDataUnit data = new TestDataUnit(request, response);
            return data;
        }


        // GETTERS & SETTERS

        public bool PopulateEmptyStrings { get; set; }
    }
}

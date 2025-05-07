namespace HubSpot.NET.Api.Ticket
{
    using HubSpot.NET.Api.Ticket.Dto;
    using HubSpot.NET.Core;
    using HubSpot.NET.Core.Extensions;
    using HubSpot.NET.Core.Interfaces;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class HubSpotTicketApi : IHubSpotTicketApi
    {
        private readonly IHubSpotClient _client;

        public HubSpotTicketApi(IHubSpotClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Creates a ticket entity
        /// </summary>
        /// <typeparam name="T">Implementation of TicketHubSpotModel</typeparam>
        /// <param name="entity">The entity</param>
        /// <returns>The created entity (with ID set)</returns>
        public T Create<T>(T entity) where T : TicketHubSpotModel, new()
        {
            var path = $"{entity.RouteBasePath}";
            var data = _client.Execute<T>(path, entity, Method.Post, SerialisationType.PropertyBag);
            return data;
        }

        /// <summary>
        /// Gets a single ticket by ID
        /// </summary>
        /// <param name="ticketId">ID of the ticket</param>
        /// <typeparam name="T">Implementation of TicketHubSpotModel</typeparam>
        /// <returns>The ticket entity or null if the ticket does not exist</returns>
        public T GetById<T>(long ticketId) where T : TicketHubSpotModel, new()
        {
            var path = $"{new T().RouteBasePath}/{ticketId}";

            try
            {
                var data = _client.Execute<T>(path, Method.Get, SerialisationType.PropertyBag);
                data.Id = data.ObjectId;
                return data;
            }
            catch (HubSpotException exception)
            {
                if (exception.ReturnedError.StatusCode == HttpStatusCode.NotFound)
                    return null;
                throw;
            }
        }

        /// <summary>
        /// Updates a given ticket
        /// </summary>
        /// <typeparam name="T">Implementation of TicketHubSpotModel</typeparam>
        /// <param name="entity">The ticket entity</param>
        /// <returns>The updated ticket entity</returns>
        public T Update<T>(T entity) where T : TicketHubSpotModel, new()
        {
            if (entity.Id < 1)
                throw new ArgumentException("Ticket entity must have an id set!");

            var path = $"{entity.RouteBasePath}/{entity.Id}";

            var data = _client.Execute<T>(path, entity, Method.Patch, SerialisationType.PropertyBag);
            return data;
        }

        /// <summary>
        /// Gets a list of tickets
        /// </summary>
        /// <typeparam name="T">Implementation of TicketListHubSpotModel</typeparam>
        /// <param name="opts">Options (limit, after) relating to request</param>
        /// <returns>List of tickets</returns>
        public TicketListHubSpotModel<T> List<T>(ListRequestOptionsV3 opts = null) where T : TicketHubSpotModel, new()
        {
            if (opts == null)
                opts = new ListRequestOptionsV3();

            var path = $"{new TicketListHubSpotModel<T>().RouteBasePath}"
                .SetQueryParam("limit", opts.Limit);

            if (!string.IsNullOrEmpty(opts.After))
                path = path.SetQueryParam("after", opts.After);

            if (opts.PropertiesToInclude.Any())
                path = path.SetQueryParam("properties", opts.PropertiesToInclude);

            var data = _client.ExecuteList<TicketListHubSpotModel<T>>(path, convertToPropertiesSchema: true);

            return data;
        }

        /// <summary>
        /// Gets a list of tickets associated to a HubSpot Object
        /// </summary>
        /// <typeparam name="T">Implementation of TicketListHubSpotModel</typeparam>
        /// <param name="includeAssociations">Bool include associated Ids</param>
        /// <param name="hubId">Long Id of Hubspot object related to tickets</param>
        /// <param name="objectName">String name of Hubspot object related to tickets</param>
        /// <param name="opts">Options (limit, offset) relating to request</param>
        /// <returns>List of tickets</returns>
        public TicketListHubSpotModel<T> ListAssociated<T>(bool includeAssociations, long ticketId, ListRequestOptions opts = null) where T : TicketHubSpotModel, new()
        {
            if (opts == null)
                opts = new ListRequestOptions();

            var path = $"{new TicketListHubSpotModel<T>().RouteBasePath}/{ticketId}/associations"
                .SetQueryParam("limit", opts.Limit);

            if (opts.Offset.HasValue)
                path = path.SetQueryParam("offset", opts.Offset);

            if (includeAssociations)
                path = path.SetQueryParam("includeAssociations", "true");

            if (opts.PropertiesToInclude.Any())
                path = path.SetQueryParam("properties", opts.PropertiesToInclude);

            var data = _client.ExecuteList<TicketListHubSpotModel<T>>(path, opts, convertToPropertiesSchema: true);

            return data;
        }

        /// <summary>
        /// Deletes a given ticket (by ID)
        /// </summary>
        /// <param name="ticketId">ID of the ticket</param>
        public void Delete(long ticketId)
        {
            var path = $"{new TicketHubSpotModel().RouteBasePath}/{ticketId}";

            _client.Execute(path, method: Method.Delete, convertToPropertiesSchema: true);
        }

        /// <summary>
        /// Gets a list of tickets based on a search criteria
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="TicketHubSpotModel"/></typeparam>
        /// <param name="opts">Options (limit, offset) and search criteria relating to request</param>
        /// <returns>List of tickets</returns>
        public SearchHubSpotModel<T> Search<T>(SearchRequestOptions opts = null) where T : TicketHubSpotModel, new()
        {
            if (opts == null)
                opts = new SearchRequestOptions();

            var path = "/crm/v3/objects/tickets/search";

            var data = _client.ExecuteList<SearchHubSpotModel<T>>(path, opts, Method.Post, convertToPropertiesSchema: true);

            return data;
        }

        /// <summary>
        /// Associate a company to a ticket
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="TicketHubSpotModel"/></typeparam>
        /// <param name="entity">The ticket to associate the company with</param>
        /// <param name="companyId">The Id of the company to associate the ticket with</param>
        public T AssociateToCompany<T>(T entity, long companyId, string associationCategory = "HUBSPOT_DEFINED", int associationTypeId = 26) where T : TicketHubSpotModel, new()
        {
            var path = $"https://api.hubapi.com/crm/v4/objects/tickets/{entity.Id}/associations/company/{companyId}";

            _client.Execute(path, new List<object> { new
            {
                associationCategory = associationCategory,
                associationTypeId = associationTypeId
            }}, method: Method.Put, convertToPropertiesSchema: true);
            entity.Associations.AssociatedCompany = new[] { companyId };
            return entity;
        }

        /// <summary>
        /// Delete a company association for a ticket
        /// </summary>
        /// <param name="ticketId">The ticket for which to delete the company assocation</param>
        /// <param name="companyId">The Id of the company to delete the association for </param>
        public void DeleteCompanyAssociation(long ticketId, long companyId)
        {
            var path = $"https://api.hubapi.com/crm/v4/objects/tickets/{ticketId}/associations/company/{companyId}";

            _client.Execute(path, method: Method.Delete, convertToPropertiesSchema: true);
        }

        /// <summary>
        /// Associate a contact to a ticket
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="TicketHubSpotModel"/></typeparam>
        /// <param name="entity">The ticket to associate the contact with</param>
        /// <param name="contactId">The Id of the contact to associate the ticket with</param>
        public T AssociateToContact<T>(T entity, long contactId, string associationCategory = "HUBSPOT_DEFINED", int associationTypeId = 16) where T : TicketHubSpotModel, new()
        {
            var path = $"https://api.hubapi.com/crm/v4/objects/tickets/{entity.Id}/associations/contact/{contactId}";

            _client.Execute(path, new List<object> { new
            {
                associationCategory = associationCategory,
                associationTypeId = associationTypeId
            }}, method: Method.Put, convertToPropertiesSchema: true);
            entity.Associations.AssociatedContacts = new[] { contactId };
            return entity;
        }

        /// <summary>
        /// Delete a contact association for a ticket
        /// </summary>
        /// <param name="ticketId">The ticket for which to delete the contact assocation</param>
        /// <param name="contactId">The Id of the contact to delete the association for </param>
        public void DeleteContactAssociation(long ticketId, long contactId)
        {
            var path = $"https://api.hubapi.com/crm/v4/objects/tickets/{ticketId}/associations/contact/{contactId}";

            _client.Execute(path, method: Method.Delete, convertToPropertiesSchema: true);
        }

		/// <summary>
		/// Associate a deal to a ticket
		/// </summary>
		/// <typeparam name="T">Implementation of <see cref="TicketHubSpotModel"/></typeparam>
		/// <param name="entity">The ticket to associate the deal with</param>
		/// <param name="dealId">The Id of the deal to associate the ticket with</param>
		public T AssociateToDeal<T>(T entity, long dealId, string associationCategory = "HUBSPOT_DEFINED", int associationTypeId = 28) where T : TicketHubSpotModel, new()
        {
            var path = $"https://api.hubapi.com/crm/v4/objects/tickets/{entity.Id}/associations/deals/{dealId}";

            _client.Execute(path, new List<object> { new
            {
                associationCategory = associationCategory,
                associationTypeId = associationTypeId
            }}, method: Method.Put, convertToPropertiesSchema: true);
            entity.Associations.AssociatedDeals = new[] { dealId };
            return entity;
        }

        /// <summary>
        /// Delete a deal association for a ticket
        /// </summary>
        /// <param name="ticketId">The ticket for which to delete the deal assocation</param>
        /// <param name="contactId">The Id of the deal to delete the association for </param>
        public void DeleteDealAssociation(long ticketId, long dealId)
        {
            var path = $"https://api.hubapi.com/crm/v4/objects/tickets/{ticketId}/associations/deal/{dealId}";

            _client.Execute(path, method: Method.Delete, convertToPropertiesSchema: true);
        }

        /// <summary>
        /// Gets a list of associations for a given ticket
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="TicketHubSpotModel"/></typeparam>
        /// <param name="entity">The ticket to get associations for</param>
        public T GetAssociations<T>(T entity) where T : TicketHubSpotModel, new()
        {
            var companyPath = $"https://api.hubapi.com/crm/v4/objects/tickets/{entity.Id}/associations/company";
            long? offSet = null;

            var companyResults = new List<AssociationResult>();
            do
            {
                var companyAssociations = _client.ExecuteList<AssociationListHubSpotModel>(string.Format("{0}?limit=100{1}", companyPath, offSet == null ? null : "&offset=" + offSet), convertToPropertiesSchema: false);
                if (companyAssociations.Results.Any())
                    companyResults.AddRange(companyAssociations.Results);
                if (companyAssociations.HasMore)
                    offSet = companyAssociations.Offset;
                else
                    offSet = null;
            } while (offSet != null);
            if (companyResults.Any())
                entity.Associations.AssociatedCompany = companyResults.Select(r => r.ToObjectId.Value).ToArray();
            else
                entity.Associations.AssociatedCompany = null;

            // see https://legacydocs.hubspot.com/docs/methods/crm-associations/crm-associations-overview
            var contactPath = $"https://api.hubapi.com/crm/v4/objects/tickets/{entity.Id}/associations/contact";

            var contactResults = new List<AssociationResult>();
            do
            {
                var contactAssociations = _client.ExecuteList<AssociationListHubSpotModel>(string.Format("{0}?limit=100{1}", contactPath, offSet == null ? null : "&offset=" + offSet), convertToPropertiesSchema: false);
                if (contactAssociations.Results.Any())
                    contactResults.AddRange(contactAssociations.Results);
                if (contactAssociations.HasMore)
                    offSet = contactAssociations.Offset;
                else
                    offSet = null;
            } while (offSet != null);
            if (contactResults.Any())
                entity.Associations.AssociatedContacts = contactResults.Select(r => r.ToObjectId.Value).ToArray();
            else
                entity.Associations.AssociatedContacts = null;

            return entity;
        }


    }
}

using HubSpot.NET.Api.Company;
using HubSpot.NET.Api.Company.Dto;
using HubSpot.NET.Api.Contact;
using HubSpot.NET.Api.Contact.Dto;
using HubSpot.NET.Api.Deal;
using HubSpot.NET.Api.Deal.Dto;
using HubSpot.NET.Api.Ticket;
using HubSpot.NET.Api.Ticket.Dto;
using HubSpot.NET.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HubSpot.NET.Tests.Integration
{
    [TestClass]
    public class TicketTests
    {
        [TestMethod]
        public void Create_SampleTicket_IdPropertyIsSet()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);
            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            // Act
            TicketHubSpotModel ticket = ticketApi.Create(sampleTicket);

            try
            {
                // Assert
                Assert.IsNotNull(ticket.Id, "The Id was not set and returned.");
                Assert.AreEqual(sampleTicket.Pipeline, ticket.Pipeline);
                Assert.AreEqual(sampleTicket.Stage, ticket.Stage);
                Assert.AreEqual(sampleTicket.Priority, ticket.Priority);
                Assert.AreEqual(sampleTicket.OwnerId, ticket.OwnerId);
                Assert.AreEqual(sampleTicket.Subject, ticket.Subject);
                Assert.AreEqual(sampleTicket.Description, ticket.Description);
                Assert.AreEqual(sampleTicket.Category, ticket.Category);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
            }
        }

		[TestMethod]
		public void Update_SampleTicket_PropertiesAreSet()
		{
			// Arrange
			var now = DateTime.Now.ToString("yyMMddHHmmss");
			var ticketApi = new HubSpotTicketApi(TestSetUp.Client);
			var sampleTicket = new TicketHubSpotModel
			{
				Pipeline = "0",
				Stage = "1",
				Priority = "HIGH",
				OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
				Subject = $"Subject {now}",
				Description = "Description",
				Category = "PRODUCT_ISSUE"
			};

			TicketHubSpotModel ticket = ticketApi.Create(sampleTicket);

			// Act
			const string updatedDescription = "Description Updated";
			ticket.Description = updatedDescription;

			const string updatedSubject = "Subject Updated";
			ticket.Subject = updatedSubject;

			var updatedTicket = ticketApi.Update(ticket);

			try
			{
				// Assert
				Assert.IsNotNull(updatedTicket.Id, "The Id was not set and returned.");
				Assert.AreEqual(updatedTicket.Pipeline, ticket.Pipeline);
				Assert.AreEqual(updatedTicket.Stage, ticket.Stage);
				Assert.AreEqual(updatedTicket.Priority, ticket.Priority);
				Assert.AreEqual(updatedTicket.OwnerId, ticket.OwnerId);
				Assert.AreEqual(updatedTicket.Subject, updatedSubject);
				Assert.AreEqual(updatedTicket.Description, updatedDescription);
				Assert.AreEqual(updatedTicket.Category, ticket.Category);
			}
			finally
			{
				// Clean-up
				ticketApi.Delete(updatedTicket.Id.Value);
			}
		}

		[TestMethod]
        public void Get_SampleTicket_TicketIsRetrieved()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);
            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            TicketHubSpotModel ticket = ticketApi.Create(sampleTicket);

            try
            {
                // Act
                var retrievedTicket = ticketApi.GetById<TicketHubSpotModel>(ticket.Id.Value);

                // Assert
                Assert.IsNotNull(retrievedTicket.Id, "The Id was not set and returned.");
                Assert.AreEqual(ticket.Id.Value, retrievedTicket.Id.Value);
                Assert.AreEqual(sampleTicket.Pipeline, retrievedTicket.Pipeline);
                Assert.AreEqual(sampleTicket.Stage, retrievedTicket.Stage);
                Assert.AreEqual(sampleTicket.Priority, retrievedTicket.Priority);
                Assert.AreEqual(sampleTicket.Subject, retrievedTicket.Subject);
                Assert.AreEqual(sampleTicket.Description, retrievedTicket.Description);
                Assert.AreEqual(sampleTicket.Category, retrievedTicket.Category);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
            }
        }

        [TestMethod]
        public void Delete_SampleTicket_TicketIsDeleted()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);
            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            TicketHubSpotModel ticket = ticketApi.Create(sampleTicket);

            // Act
            ticketApi.Delete(ticket.Id.Value);

            // Assert
            ticket = ticketApi.GetById<TicketHubSpotModel>(ticket.Id.Value);
            Assert.IsNull(ticket, "The ticket was searchable and not deleted.");
        }

        [TestMethod]
        public void List_5SamplesLimitedTo3WithContinuations_ReturnsCollectionWith3ItemsWithContinuationDetails()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);

            IList<TicketHubSpotModel> sampleTickets = new List<TicketHubSpotModel>();
            for (int i = 1; i <= 5; i++)
            {
                var ticket = ticketApi.Create(new TicketHubSpotModel()
                {
                    Pipeline = "0",
                    Stage = "1",
                    Priority = "HIGH",
                    OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                    Subject = $"Subject {now}-{i}",
                    Description = "Description",
                    Category = "PRODUCT_ISSUE"
                });
                sampleTickets.Add(ticket);
            }

            try
            {
                var listRequestOptions = new ListRequestOptionsV3
                {
                    Limit = 3,
                    PropertiesToInclude = new List<string> { "subject", "content" }
                };

                // Act
                TicketListHubSpotModel<TicketHubSpotModel> results = ticketApi.List<TicketHubSpotModel>(listRequestOptions);

                // Assert
                Assert.IsFalse(string.IsNullOrEmpty(results.Paging.Next.After), "Did not identify more results are available.");
                Assert.AreEqual(3, results.Tickets.Count, "Did not return 3 of the 5 results.");
                Assert.AreEqual(false, results.Tickets.Any(c => string.IsNullOrWhiteSpace(c.Subject)), "Some tickets do not have a subject.");

                // Second Act
                listRequestOptions.After = results.Paging.Next.After;
                var results2 = ticketApi.List<TicketHubSpotModel>(listRequestOptions);

                Assert.IsNull(results2.Paging, "Did not identify at the end of results.");
                Assert.AreEqual(2, results2.Tickets.Count, "Did not return 2 of the 5 results.");
                Assert.AreEqual(false, results2.Tickets.Any(c => string.IsNullOrWhiteSpace(c.Subject)), "Some deals do not have a subject.");
            }
            finally
            {
                // Clean-up
                for (int i = 0; i < sampleTickets.Count; i++)
                {
                    ticketApi.Delete(sampleTickets[i].Id.Value);
                }
            }
        }

        [TestMethod]
        public void AssociateToCompany_TicketIsAssociatedToCompany()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);

            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            var ticket = ticketApi.Create(sampleTicket);

            var companyApi = new HubSpotCompanyApi(TestSetUp.Client);

            var sampleCompany = new CompanyHubSpotModel
            {
                Name = $"Sample Company {now}",
                Domain = $"mysamplecompany{now}.com",
                Website = $"http://www.mysamplecompany{now}.com",
                Description = "Sample company description",
                Country = "United States"
            };

            var company = companyApi.Create(sampleCompany);

            // Act
            ticketApi.AssociateToCompany(ticket, company.Id.Value);

            var ticketAssociations = ticketApi.GetAssociations(ticket);

            try
            {
                // Assert
                Assert.IsTrue(ticketAssociations.Associations.AssociatedCompany.Any());
                Assert.IsNull(ticketAssociations.Associations.AssociatedContacts);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
                companyApi.Delete(company.Id.Value);
            }
        }

        [TestMethod]
        public void AssociateToContact_TicketIsAssociatedToContact()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);

            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            var ticket = ticketApi.Create(sampleTicket);

            var companyApi = new HubSpotCompanyApi(TestSetUp.Client);

            var sampleCompany = new CompanyHubSpotModel
            {
                Name = $"Sample Company {now}",
                Domain = $"mysamplecompany{now}.com",
                Website = $"http://www.mysamplecompany{now}.com",
                Description = "Sample company description",
                Country = "United States"
            };

            var company = companyApi.Create(sampleCompany);

            var contactApi = new HubSpotContactApi(TestSetUp.Client);

            var sampleContact = new ContactHubSpotModel
            {
                Email = $"sample@mysamplecompany{now}.com",
                FirstName = "firstname",
                LastName = "lastname",
                Company = sampleCompany.Name,
                AssociatedCompanyId = company.Id,
            };

            var contact = contactApi.Create(sampleContact);

            // Act
            ticketApi.AssociateToContact(ticket, contact.Id.Value);

            var ticketAssociations = ticketApi.GetAssociations(ticket);

            try
            {
                // Assert
                Assert.IsTrue(ticketAssociations.Associations.AssociatedContacts.Any());
                Assert.IsNotNull(ticketAssociations.Associations.AssociatedCompany);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
                contactApi.Delete(contact.Id.Value);
                companyApi.Delete(company.Id.Value);                               
            }
        }

        [TestMethod]
        public void AssociateToDeal_TicketIsAssociatedToDeal()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);

            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            var ticket = ticketApi.Create(sampleTicket);

            var dealApi = new HubSpotDealApi(TestSetUp.Client);

            var sampleDeal = new DealHubSpotModel
            {
                Amount = 10000,
                Name = $"New Created Deal {now}",
                DateCreated = DateTime.UtcNow
            };

            var deal = dealApi.Create(sampleDeal);

            // Act
            ticketApi.AssociateToDeal(ticket, deal.Id.Value);

            var ticketAssociations = ticketApi.GetAssociations(ticket);

            try
            {
                // Assert
                Assert.IsTrue(ticketAssociations.Associations.AssociatedDeals.Any());
                Assert.IsNull(ticketAssociations.Associations.AssociatedContacts);
                Assert.IsNull(ticketAssociations.Associations.AssociatedCompany);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
                dealApi.Delete(deal.Id.Value);
            }
        }


        [TestMethod]
        public void DeleteCompanyAssociation_CompanyAssociationIsDeleted()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);

            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            var ticket = ticketApi.Create(sampleTicket);

            var companyApi = new HubSpotCompanyApi(TestSetUp.Client);

            var sampleCompany = new CompanyHubSpotModel
            {
                Name = $"Sample Company {now}",
                Domain = $"mysamplecompany{now}.com",
                Website = $"http://www.mysamplecompany{now}.com",
                Description = "Sample company description",
                Country = "United States"
            };

            var company = companyApi.Create(sampleCompany);

            // Act
            ticketApi.AssociateToCompany(ticket, company.Id.Value);

            ticketApi.DeleteCompanyAssociation(ticket.Id.Value, company.Id.Value);

            var ticketAssociations = ticketApi.GetAssociations(ticket);

            try
            {
                // Assert
                Assert.IsNull(ticketAssociations.Associations.AssociatedCompany);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
                companyApi.Delete(company.Id.Value);
            }
        }

        [TestMethod]
        public void DeleteContactAssociation_ContactAssociationIsDeleted()
        {
            // Arrange
            var now = DateTime.Now.ToString("yyMMddHHmmss");
            var ticketApi = new HubSpotTicketApi(TestSetUp.Client);

            var sampleTicket = new TicketHubSpotModel
            {
                Pipeline = "0",
                Stage = "1",
                Priority = "HIGH",
                OwnerId = Convert.ToInt64(TestSetUp.GetAppSetting("HubspotOwner")),
                Subject = $"Subject {now}",
                Description = "Description",
                Category = "PRODUCT_ISSUE"
            };

            var ticket = ticketApi.Create(sampleTicket);

            var companyApi = new HubSpotCompanyApi(TestSetUp.Client);

            var sampleCompany = new CompanyHubSpotModel
            {
                Name = $"Sample Company {now}",
                Domain = $"mysamplecompany{now}.com",
                Website = $"http://www.mysamplecompany{now}.com",
                Description = "Sample company description",
                Country = "United States"
            };

            var company = companyApi.Create(sampleCompany);

            var contactApi = new HubSpotContactApi(TestSetUp.Client);

            var sampleContact = new ContactHubSpotModel
            {
                Email = $"sample@mysamplecompany{now}.com",
                FirstName = "firstname",
                LastName = "lastname",
                Company = sampleCompany.Name,
                AssociatedCompanyId = company.Id,
            };

            var contact = contactApi.Create(sampleContact);

            // Act
            ticketApi.AssociateToContact(ticket, contact.Id.Value);

            ticketApi.DeleteContactAssociation(ticket.Id.Value, contact.Id.Value);

            var ticketAssociations = ticketApi.GetAssociations(ticket);

            try
            {
                // Assert
                Assert.IsNull(ticketAssociations.Associations.AssociatedContacts);
            }
            finally
            {
                // Clean-up
                ticketApi.Delete(ticket.Id.Value);
                contactApi.Delete(contact.Id.Value);
                companyApi.Delete(company.Id.Value);
            }
        }
    }
}

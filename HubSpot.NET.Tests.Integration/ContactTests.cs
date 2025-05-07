using System.Collections.Generic;
using System.Linq;

using HubSpot.NET.Api.Contact;
using HubSpot.NET.Api.Contact.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HubSpot.NET.Tests.Integration
{
	[TestClass]
	public class ContactTests
	{
		[TestMethod]
		public void Search_5SamplesLimitedTo3WitContinuations_ReturnsCollectionWith3ItemsWithContinuationDetails()
		{
			// Arrange
			var contactApi = new HubSpotContactApi(TestSetUp.Client);
			IList<ContactHubSpotModel> sampleContacts = new List<ContactHubSpotModel>();
			for (int i = 1; i <= 5; i++)
			{
				var contact = contactApi.Create(new ContactHubSpotModel()
				{
					FirstName = "Test",
					LastName = $"User {i:N0}",
					Email = $"Test.User.{i:N0}@sampledomain.com"
				});
				sampleContacts.Add(contact);
			}

            // HubSpot is rather slow to update... wait 5 seconds to allow it to catch up
            System.Threading.Thread.Sleep(10 * 1000);

            try
            {
				var searchOptions = new ContactSearchRequestOptions
				{
					Query = "sampledomain.com",
					Limit = 3,
					SortBy = "lastname",
					Order = SortingOrderType.Ascending
				};

				// Act
				ContactSearchHubSpotModel<ContactHubSpotModel> results = contactApi.Search<ContactHubSpotModel>(searchOptions);

				// Assert
				Assert.AreEqual(5, results.Total, "Did not identify a total of 5 results.");
				Assert.AreEqual(3, results.Contacts.Count, "Did not return 3 of the 5 results.");
				Assert.AreEqual(false, results.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Email)), "Some contacts do not have email addresses.");
				Assert.AreEqual($"User 1", results.Contacts[0].LastName, $"Last Name '{results.Contacts[0].LastName}' did not match User 1.");
				Assert.AreEqual($"User 2", results.Contacts[1].LastName, $"Last Name '{results.Contacts[1].LastName}' did not match User 2.");
				Assert.AreEqual($"User 3", results.Contacts[2].LastName, $"Last Name '{results.Contacts[2].LastName}' did not match User 3.");
				Assert.AreNotEqual(0, results.ContinuationOffset);

				// Second Act
				searchOptions.Offset = results.ContinuationOffset;
				results = contactApi.Search<ContactHubSpotModel>(searchOptions);

				Assert.AreEqual(5, results.Total, "Did not identify a total of 5 results.");
				Assert.AreEqual(2, results.Contacts.Count, "Did not return 2 of the 5 results.");
				Assert.AreEqual(false, results.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Email)), "Some contacts do not have email addresses.");
				Assert.AreEqual($"User 4", results.Contacts[0].LastName, $"Last Name '{results.Contacts[0].LastName}' did not match User 4.");
				Assert.AreEqual($"User 5", results.Contacts[1].LastName, $"Last Name '{results.Contacts[1].LastName}' did not match User 5.");
			}
			finally
			{
				// Clean-up
				for (int i = 0; i < sampleContacts.Count; i++)
				{
					contactApi.Delete(sampleContacts[i].Id.Value);
				}
			}
		}

		[TestMethod]
		public void RecentlyCreated_5SamplesLimitedTo3WitContinuations_ReturnsCollectionWith3ItemsWithContinuationDetails()
		{
			// Arrange
			var contactApi = new HubSpotContactApi(TestSetUp.Client);
			IList<ContactHubSpotModel> sampleContacts = new List<ContactHubSpotModel>();
			for (int i = 1; i <= 5; i++)
			{
				var contact = contactApi.Create(new ContactHubSpotModel()
				{
					FirstName = "Created Test",
					LastName = $"User {i:N0}",
					Email = $"Test.User.{i:N0}@sampledomain.com"
				});
				sampleContacts.Add(contact);
			}

			// HubSpot is rather slow to update the list... wait 10 seconds to allow it to catch up
			System.Threading.Thread.Sleep(10 * 1000);

			try
			{
				var searchOptions = new ListRecentRequestOptions
				{
					Limit = 3
				};

				// Act
				ContactListHubSpotModel<ContactHubSpotModel> results = contactApi.RecentlyCreated<ContactHubSpotModel>(searchOptions);

				// Assert
				Assert.IsTrue(results.MoreResultsAvailable, "Did not identify more results are available.");
				Assert.AreEqual(3, results.Contacts.Count, "Did not return 3 of the 5 results.");
				Assert.AreEqual(false, results.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Email)), "Some contacts do not have email addresses.");
				Assert.AreNotEqual(0, results.ContinuationOffset);

				// Second Act
				searchOptions.Offset = results.ContinuationOffset;
				var results2 = contactApi.RecentlyCreated<ContactHubSpotModel>(searchOptions);

				Assert.IsFalse(results2.MoreResultsAvailable, "Did not identify at the end of results.");
				Assert.AreEqual(2, results2.Contacts.Count, "Did not return 2 of the 5 results.");
				Assert.AreEqual(false, results2.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Email)), "Some contacts do not have email addresses.");
			}
			finally
			{
				// Clean-up
				for (int i = 0; i < sampleContacts.Count; i++)
				{
					contactApi.Delete(sampleContacts[i].Id.Value);
				}
			}
		}

		[TestMethod]
		public void RecentlyUpdated_3SamplesLimitedTo2WitContinuations_ReturnsCollectionWith2ItemsWithContinuationDetails()
		{
			// Arrange
			var contactApi = new HubSpotContactApi(TestSetUp.Client);
			IList<ContactHubSpotModel> sampleContacts = new List<ContactHubSpotModel>();
			for (int i = 1; i <= 5; i++)
			{
				var contact = contactApi.Create(new ContactHubSpotModel()
				{
					FirstName = "Created Test",
					LastName = $"User {i:N0}",
					Email = $"Test.User.{i:N0}@sampledomain.com"
				});
				sampleContacts.Add(contact);
			}

			for (int i = 0; i < sampleContacts.Count; i++)
			{
				ContactHubSpotModel contact = sampleContacts[i];
				contact.FirstName = $"Updated Test";
				contactApi.Update(contact);
				// This is intentional to skip to every odd item
				i++;
			}

			// HubSpot is rather slow to update the list... wait 10 seconds to allow it to catch up
			System.Threading.Thread.Sleep(10 * 1000);

			try
			{
				var searchOptions = new ListRecentRequestOptions
				{
					Limit = 2
				};

				// Act
				ContactListHubSpotModel<ContactHubSpotModel> results = contactApi.RecentlyUpdated<ContactHubSpotModel>(searchOptions);

				// Assert
				Assert.IsTrue(results.MoreResultsAvailable, "Did not identify more results are available.");
				Assert.AreEqual(2, results.Contacts.Count, "Did not return 3 of the 5 results.");
				Assert.AreEqual(false, results.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Email)), "Some contacts do not have email addresses.");
				Assert.AreNotEqual(0, results.ContinuationOffset);

				// Cannot actually test recently updated as recently created polutes the results.
			}
			finally
			{
				// Clean-up
				for (int i = 0; i < sampleContacts.Count; i++)
				{
					contactApi.Delete(sampleContacts[i].Id.Value);
				}
			}
		}

		[TestMethod]
		public void Create_SampleDetails_IdProeprtyIsSet()
		{
			// Arrange
			var contactApi = new HubSpotContactApi(TestSetUp.Client);
			var sampleContact = new ContactHubSpotModel
			{
				FirstName = "Test",
				LastName = $"User Create",
				Email = "Test.User.Create@sampledomain.com",
				Phone = "123-456-789",
				Company = "Sample Company"
			};

			// Act
			ContactHubSpotModel contact = contactApi.Create(sampleContact);

			try
			{
				// Assert
				Assert.IsNotNull(contact.Id, "The Id was not set and returned.");
				Assert.AreEqual(sampleContact.FirstName, contact.FirstName);
				Assert.AreEqual(sampleContact.LastName, contact.LastName);
				// HubSpot stores all email address in lowercase
				Assert.AreEqual(sampleContact.Email?.ToLowerInvariant(), contact.Email);
				Assert.AreEqual(sampleContact.Phone, contact.Phone);
				Assert.AreEqual(sampleContact.Company, contact.Company);
			}
			finally
			{
				// Clean-up
				contactApi.Delete(contact.Id.Value);
			}
		}

		[TestMethod]
		public void Update_SampleDetails_PropertiesAreUpdated()
		{
			// Arrange
			var contactApi = new HubSpotContactApi(TestSetUp.Client);
			var sampleContact = new ContactHubSpotModel
			{
				FirstName = "Test",
				LastName = $"User Update",
				Email = "Test.User.Update@sampledomain.com",
				Phone = "123-456-789",
				Company = "Sample Company"
			};

			ContactHubSpotModel contact = contactApi.Create(sampleContact);

			contact.Phone = "1234-5678";
			contact.Company = "Second Sample Company";

			// Act
			contactApi.Update(contact);

			try
			{
				// Assert
				Assert.AreNotEqual(sampleContact.Phone, contact.Phone);
				Assert.AreNotEqual(sampleContact.Company, contact.Company);
				Assert.AreEqual("1234-5678", contact.Phone);
				Assert.AreEqual("Second Sample Company", contact.Company);

				// Second Act
				contact = contactApi.GetByEmail<ContactHubSpotModel>(sampleContact.Email);

				// Second Assert
				Assert.AreNotEqual(sampleContact.Phone, contact.Phone);
				Assert.AreNotEqual(sampleContact.Company, contact.Company);
				Assert.AreEqual("1234-5678", contact.Phone);
				Assert.AreEqual("Second Sample Company", contact.Company);
			}
			finally
			{
				// Clean-up
				contactApi.Delete(contact.Id.Value);
			}
		}

		[TestMethod]
		public void Delete_SampleContact_ContactIsDeleted()
		{
			// Arrange
			var contactApi = new HubSpotContactApi(TestSetUp.Client);
			var sampleContact = new ContactHubSpotModel
			{
				FirstName = "Test",
				LastName = $"User Delete",
				Email = "Test.User.Delete@sampledomain.com",
				Phone = "123-456-789",
				Company = "Sample Company"
			};

			ContactHubSpotModel contact = contactApi.Create(sampleContact);

			// Act
			contactApi.Delete(contact.Id.Value);

			// Assert
			contact = contactApi.GetByEmail<ContactHubSpotModel>(sampleContact.Email);
			Assert.IsNull(contact, "The contact was searchable and not deleted.");
		}
	}
}
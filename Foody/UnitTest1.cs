using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using StorySpoil.Models;

namespace StorySpoil
{
    [TestFixture]
    public class StorySpoilTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("vanyo1", "123456!");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName = username, password = password });

            var response = loginClient.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login failed!");

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            return json.GetProperty("accessToken").GetString()!;
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            var newStory = new StoryDTO
            {
                Title = "Test Story",
                Description = "Story created during tests",
                Url = ""
            };
            request.AddJsonBody(newStory);

            var response = client.Execute<ApiResponseDTO>(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Data?.Msg, Is.EqualTo("Successfully created!"));
            Assert.IsNotNull(response.Data?.StoryId);

            createdStoryId = response.Data!.StoryId!;
        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/Story/Edit/{createdStoryId}", Method.Put);
            var updatedStory = new StoryDTO
            {
                Title = "Updated Test Story",
                Description = "Edited description",
                Url = ""
            };
            request.AddJsonBody(updatedStory);

            var response = client.Execute<ApiResponseDTO>(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data?.Msg, Is.EqualTo("Successfully edited"));
        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = client.Execute<List<StoryDTO>>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Data?.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Story/Create", Method.Post);
            var badStory = new StoryDTO
            {
                Title = "",
                Description = ""
            };
            request.AddJsonBody(badStory);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var request = new RestRequest("/Story/Edit/00000000-0000-0000-0000-000000000000", Method.Put);
            var updatedStory = new StoryDTO
            {
                Title = "Ghost Story",
                Description = "Non existing",
                Url = ""
            };
            request.AddJsonBody(updatedStory);

            var response = client.Execute<ApiResponseDTO>(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Data?.Msg, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Story/Delete/00000000-0000-0000-0000-000000000000", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Data?.Msg, Is.EqualTo("Unable to delete this story spoiler!"));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}
 


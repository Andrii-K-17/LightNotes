using System.Net;
using System.Net.Http.Json;
using LightNotes.Application.DTOs.Auth;
using LightNotes.Application.DTOs.Notes;
using LightNotes.Tests.Integration;
using Xunit;

namespace LightNotes.Tests.Integration.Controllers;

public class NotesControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string CreateUniqueEmail() => $"{Guid.NewGuid()}@example.com";

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var email = CreateUniqueEmail();
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "ValidPassword123!",
            Name = "Test user"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authData?.Token
                                                     ?? throw new Exception("Token not received"));
        return client;
    }

    private static async Task<HttpResponseMessage> HttpPostCreateTestNoteAsync(HttpClient client, string title = "Test note", string content = "Content", string color = "#FFFFFF")
    {
        var request = new NoteRequestDto
        {
            Title = title,
            Content = content,
            Color = color
        };

        var response = await client.PostAsJsonAsync("/api/notes", request);
        return response;
    }

    private static async Task<T> ReadHttpResponseContentAsAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<T>();
        return content ?? throw new Exception("Invalid response content");
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task GetAllNotes_ReturnsNotesListOrUnauthorized_DependsOnAuthorization(bool isAuthorized, HttpStatusCode expectedStatus)
    {
        var client = isAuthorized ? await CreateAuthorizedClientAsync() : _client;

        if (isAuthorized)
        {
            await HttpPostCreateTestNoteAsync(client);
        }

        var getResponse = await client.GetAsync("/api/notes");

        Assert.Equal(expectedStatus, getResponse.StatusCode);

        if (isAuthorized)
        {
            var notes = await getResponse.Content.ReadFromJsonAsync<List<NoteResponseDto>>();
            Assert.NotNull(notes);
            Assert.NotEmpty(notes);
        }
    }

    [Theory]
    [InlineData("Valid title", "Content", "#FFFFFF", HttpStatusCode.Created)]
    [InlineData("", "Content", "#FFFFFF", HttpStatusCode.BadRequest)]
    [InlineData("Valid title", "", "#FFFFFF", HttpStatusCode.BadRequest)]
    public async Task CreateNote_ReturnsCreatedOrBadRequest_DependsOnDataValidity(string title, string content, string color, HttpStatusCode expectedStatus)
    {
        var client = await CreateAuthorizedClientAsync();
        var response = await HttpPostCreateTestNoteAsync(client, title, content, color);

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.NotFound)]
    public async Task GetNoteById_ReturnsNoteOrNotFound_DependsOnIdValidity(bool validId, HttpStatusCode expectedStatus)
    {
        var client = await CreateAuthorizedClientAsync();
        var createResponse = await HttpPostCreateTestNoteAsync(client, "Title", "Content", "#FFFFFF");
        var note = await ReadHttpResponseContentAsAsync<NoteResponseDto>(createResponse);

        var noteId = validId ? note.Id : Guid.NewGuid();
        var getResponse = await client.GetAsync($"/api/notes/{noteId}");

        Assert.Equal(expectedStatus, getResponse.StatusCode);

        if (validId)
        {
            var receivedNote = await ReadHttpResponseContentAsAsync<NoteResponseDto>(getResponse);
            Assert.Equal(note.Id, receivedNote!.Id);
        }
    }

    [Theory]
    [InlineData(true, HttpStatusCode.OK)]
    [InlineData(false, HttpStatusCode.NotFound)]
    public async Task UpdateNote_ReturnsUpdatedNoteOrNotFound_DependsOnNoteExistence(bool validId, HttpStatusCode expectedStatus)
    {
        var client = await CreateAuthorizedClientAsync();

        var noteId = Guid.NewGuid();

        if (validId)
        {
            var createResponse = await HttpPostCreateTestNoteAsync(client, "Original title", "Content", "#AFFFFF");
            var note = await ReadHttpResponseContentAsAsync<NoteResponseDto>(createResponse);
            noteId = note.Id;
        }

        var updateRequest = new NoteRequestDto
        {
            Title = "Updated title",
            Content = "Updated content",
            Color = "#FFFFFF"
        };

        var response = await client.PutAsJsonAsync($"/api/notes/{noteId}", updateRequest);

        Assert.Equal(expectedStatus, response.StatusCode);

        if (expectedStatus == HttpStatusCode.OK)
        {
            var updatedNote = await ReadHttpResponseContentAsAsync<NoteResponseDto>(response);
            Assert.NotNull(updatedNote);
            Assert.Equal("Updated title", updatedNote.Title);
            Assert.Equal("Updated content", updatedNote.Content);
            Assert.Equal("#FFFFFF", updatedNote.Color);
        }
    }

    [Fact]
    public async Task ArchiveNote_ThenRestoreNote_WorksCorrectly()
    {
        var client = await CreateAuthorizedClientAsync();
        var createResponse = await HttpPostCreateTestNoteAsync(client, "To archive", "Content", "#FFFFFF");
        var note = await ReadHttpResponseContentAsAsync<NoteResponseDto>(createResponse);

        var archiveResponse = await client.DeleteAsync($"/api/notes/{note.Id}");
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        var restoreResponse = await client.PostAsync($"/api/notes/{note.Id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);
        Assert.NotNull(restoreResponse);
    }

    [Theory]
    [InlineData("archived", HttpStatusCode.OK)]
    [InlineData("not archived", HttpStatusCode.OK)]
    [InlineData("non existent", HttpStatusCode.NotFound)]
    public async Task RestoreNote_ReturnsExpectedStatus_DependsOnNoteState(string noteState, HttpStatusCode expectedStatus)
    {
        var client = await CreateAuthorizedClientAsync();
        Guid noteId;

        if (noteState == "archived")
        {
            var createResponse = await HttpPostCreateTestNoteAsync(client, "Archived", "Content", "#FFFFFF");
            var note = await ReadHttpResponseContentAsAsync<NoteResponseDto>(createResponse);

            var archiveResponse = await client.DeleteAsync($"/api/notes/{note.Id}");
            
            Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

            noteId = note.Id;
        }
        else if (noteState == "not archived")
        {
            var createResponse = await HttpPostCreateTestNoteAsync(client, "Active", "Content", "#FFFFFF");
            var note = await ReadHttpResponseContentAsAsync<NoteResponseDto>(createResponse);
            noteId = note.Id;
        }
        else
        {
            noteId = Guid.NewGuid();
        }

        var restoreResponse = await client.PostAsync($"/api/notes/{noteId}/restore", null);

        Assert.Equal(expectedStatus, restoreResponse.StatusCode);
    }

    [Theory]
    [InlineData(true, HttpStatusCode.NoContent)]
    [InlineData(false, HttpStatusCode.NotFound)]
    public async Task DeleteNotePermanently_ReturnsNoContentOrNotFound_DependsOnNoteExistence(bool noteExists, HttpStatusCode expectedStatus)
    {
        var client = await CreateAuthorizedClientAsync();
        var createResponse = await HttpPostCreateTestNoteAsync(client, "Permanent delete", "Content", "#FFFFFF");
        var note = await ReadHttpResponseContentAsAsync<NoteResponseDto>(createResponse);

        var noteId = noteExists ? note.Id : Guid.NewGuid();

        var deleteResponse = await client.DeleteAsync($"/api/notes/{noteId}/permanent");

        Assert.Equal(expectedStatus, deleteResponse.StatusCode);
    }
}

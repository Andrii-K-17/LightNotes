using AutoMapper;
using LightNotes.Application.DTOs.Notes;
using LightNotes.Application.DTOs.Chat;
using LightNotes.Domain.Entities;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using LightNotes.Domain.Enums;
using LightNotes.Application.MappingProfiles;

namespace LightNotes.Tests.Unit.MappingProfiles;

public class NoteProfileTests
{
    private readonly IMapper _mapper;

    public NoteProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<NoteProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Map_NoteCollaborator_ToNoteCollaboratorDto_MapsUserDataOrUnknown()
    {
        var collaboratorWithUser = new NoteCollaborator
        {
            UserId = Guid.NewGuid(),
            Role = Role.Editor,
            User = new User { Name = "User", Email = "user@example.com" }
        };

        var collaboratorWithoutUser = new NoteCollaborator
        {
            UserId = Guid.NewGuid(),
            Role = Role.Viewer
        };

        var dtoWithUser = _mapper.Map<NoteCollaboratorDto>(collaboratorWithUser);
        var dtoWithoutUser = _mapper.Map<NoteCollaboratorDto>(collaboratorWithoutUser);

        Assert.Equal("User", dtoWithUser.UserName);
        Assert.Equal("user@example.com", dtoWithUser.UserEmail);
        Assert.Equal("Unknown", dtoWithoutUser.UserName);
        Assert.Equal("Unknown", dtoWithoutUser.UserEmail);
    }

    [Fact]
    public void Map_NoteRequestDto_To_Note_OnlyMapsTitleAndContent()
    {
        var request = new NoteRequestDto
        {
            Title = "Test Note",
            Content = "Content"
        };

        var note = _mapper.Map<Note>(request);

        Assert.Equal(request.Title, note.Title);
        Assert.Equal(request.Content, note.Content);
        Assert.Null(note.Owner);
        Assert.Equal(Guid.Empty, note.OwnerId);
        Assert.Empty(note.Collaborators);
        Assert.Empty(note.ChatMessages);
        Assert.Empty(note.Tags);
    }

    [Fact]
    public void Map_Note_To_NoteResponseDto_MapsOwnerTagsAndCollaboratorsCorrectly()
    {
        var owner = new User { Id = Guid.NewGuid(), Name = "OwnerName" };
        var collaboratorUser = new User { Id = Guid.NewGuid(), Name = "CollabName", Email = "collab@example.com" };
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Sample Note",
            Owner = owner,
            Tags = [ new() { Tag = "tag1" }, new() { Tag = "tag2" } ],
            Collaborators =
            [
                new()
                {
                    UserId = collaboratorUser.Id,
                    Role = Role.Editor,
                    User = collaboratorUser
                },
                new()
                {
                    UserId = Guid.NewGuid(),
                    Role = Role.Viewer
                }
            ]
        };

        var dto = _mapper.Map<NoteResponseDto>(note);

        Assert.Equal("OwnerName", dto.OwnerName);
        Assert.Equal(2, dto.Tags.Count);
        Assert.Contains(dto.Tags, t => t.Tag == "tag1");
        Assert.Contains(dto.Tags, t => t.Tag == "tag2");
        Assert.Equal(2, dto.Collaborators.Count);

        var collabWithUser = dto.Collaborators.First(c => c.UserId == collaboratorUser.Id);

        Assert.Equal("CollabName", collabWithUser.UserName);
        Assert.Equal("collab@example.com", collabWithUser.UserEmail);
        Assert.Equal(Role.Editor, collabWithUser.Role);

        var collabWithoutUser = dto.Collaborators.First(c => c.UserName == "Unknown");

        Assert.Equal("Unknown", collabWithoutUser.UserEmail);
        Assert.Equal(Role.Viewer, collabWithoutUser.Role);
    }

    [Fact]
    public void Map_NoteTag_To_And_From_Dto()
    {
        var dto = _mapper.Map<NoteTagDto>(new NoteTag { Tag = "test tag" });
        var tag = _mapper.Map<NoteTag>(dto);

        Assert.Equal("test tag", dto.Tag);
        Assert.Equal("test tag", tag.Tag);
    }

    [Fact]
    public void Map_ChatMessage_To_ChatMessageDto_MapsSenderNameOrUnknown()
    {
        var sender = new User { Name = "SenderName" };
        var messageWithSender = new ChatMessage
        {
            Text = "Hello",
            Sender = sender
        };
        var messageWithoutSender = new ChatMessage
        {
            Text = "Hi"
        };

        var dtoWithSender = _mapper.Map<ChatMessageDto>(messageWithSender);
        var dtoWithoutSender = _mapper.Map<ChatMessageDto>(messageWithoutSender);

        Assert.Equal("SenderName", dtoWithSender.SenderName);
        Assert.Equal("Unknown", dtoWithoutSender.SenderName);
    }
}

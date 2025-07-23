using AutoMapper;
using LightNotes.Application.DTOs.Notes;
using LightNotes.Application.DTOs.Chat;
using LightNotes.Domain.Entities;

namespace LightNotes.Application.MappingProfiles;

/// <summary>
/// AutoMapper профіль для перетворення між сутностями та DTO, пов'язаними з нотатками
/// </summary>
public class NoteProfile : Profile
{
    public NoteProfile()
    {
        // Перетворення між NoteCollaborator і NoteCollaboratorDto
        CreateMap<NoteCollaborator, NoteCollaboratorDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.Name : "Unknown"))
            .ForMember(dest => dest.UserEmail,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : "Unknown"));

        // Перетворення з NoteRequestDto у Note
        CreateMap<NoteRequestDto, Note>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Owner, opt => opt.Ignore())
            .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
            .ForMember(dest => dest.ChatMessages, opt => opt.Ignore())
            .ForMember(dest => dest.Tags, opt => opt.Ignore());

        // Перетворення з Note у NoteResponseDto
        CreateMap<Note, NoteResponseDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src => src.Owner != null ? src.Owner.Name : "Unknown"))
            .ForMember(dest => dest.Tags,
                opt => opt.MapFrom(src => src.Tags
                    .Select(t => new NoteTagDto { Tag = t.Tag })
                    .ToList()))
            .ForMember(dest => dest.Collaborators,
                opt => opt.MapFrom(src => src.Collaborators
                    .Select(nc => new NoteCollaboratorDto
                    {
                        UserId    = nc.UserId,
                        UserName  = nc.User != null ? nc.User.Name  : "Unknown",
                        UserEmail = nc.User != null ? nc.User.Email : "Unknown",
                        Role      = nc.Role
                    })
                    .ToList()));

        // Двостороннє перетворення між NoteTag і NoteTagDto
        CreateMap<NoteTag, NoteTagDto>();
        CreateMap<NoteTagDto, NoteTag>();

        // Перетворення з ChatMessage у ChatMessageDto
        CreateMap<ChatMessage, ChatMessageDto>()
            .ForMember(dest => dest.SenderName,
                opt => opt.MapFrom(src => src.Sender != null ? src.Sender.Name : "Unknown"));
    }
}

using AutoMapper;
using ToDoWithAuth.Models;
using ToDoWithAuth.Models.DTOs;

namespace ToDoWithAuth.Helpers
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles() {
            CreateMap<User, AppUser>();
            CreateMap<TaskItem, AppTask>();
            CreateMap<RegisterDto, User>();
        }
    }
}
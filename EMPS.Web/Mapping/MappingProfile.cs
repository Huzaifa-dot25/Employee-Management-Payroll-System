using AutoMapper;
using EMPS.Core.Entities;
using EMPS.Web.Models;

namespace EMPS.Web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Department Mappings
            CreateMap<Department, DepartmentViewModel>()
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count))
                .ReverseMap();

            // Designation Mappings
            CreateMap<Designation, DesignationViewModel>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees.Count))
                .ReverseMap();

            // Employee Mappings
            CreateMap<Employee, EmployeeViewModel>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.Designation.Name))
                .ReverseMap()
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Designation, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}

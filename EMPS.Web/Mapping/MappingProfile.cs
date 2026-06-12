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
                .ReverseMap()
                .ForMember(dest => dest.Department, opt => opt.Ignore());

            // Employee Mappings
            CreateMap<Employee, EmployeeViewModel>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.DesignationName, opt => opt.MapFrom(src => src.Designation.Name))
                .ReverseMap()
                .ForMember(dest => dest.Department, opt => opt.Ignore())
                .ForMember(dest => dest.Designation, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Attendance Mappings
            CreateMap<Attendance, AttendanceViewModel>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : null))
                .ReverseMap()
                .ForMember(dest => dest.Employee, opt => opt.Ignore());

            // Leave Mappings
            CreateMap<LeaveRequest, LeaveRequestViewModel>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : null))
                .ReverseMap()
                .ForMember(dest => dest.Employee, opt => opt.Ignore());

            // Payroll Mappings
            CreateMap<Payroll, PayrollViewModel>()
                .ForMember(dest => dest.EmployeeName,       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : null))
                .ForMember(dest => dest.EmployeeCode,       opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : null))
                .ForMember(dest => dest.DepartmentName,     opt => opt.MapFrom(src => src.Employee != null && src.Employee.Department != null ? src.Employee.Department.Name : null))
                .ForMember(dest => dest.DesignationName,    opt => opt.MapFrom(src => src.Employee != null && src.Employee.Designation != null ? src.Employee.Designation.Name : null))
                .ForMember(dest => dest.HasPayslip,         opt => opt.MapFrom(src => src.Payslip != null))
                .ForMember(dest => dest.PayslipId,          opt => opt.MapFrom(src => src.Payslip != null ? (int?)src.Payslip.Id : null))
                .ForMember(dest => dest.PayslipCode,        opt => opt.MapFrom(src => src.Payslip != null ? src.Payslip.PayslipCode : null))
                .ReverseMap()
                .ForMember(dest => dest.Employee, opt => opt.Ignore())
                .ForMember(dest => dest.Payslip,  opt => opt.Ignore());
        }
    }
}

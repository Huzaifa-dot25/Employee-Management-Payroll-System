using System;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Department> Departments { get; }
        IRepository<Designation> Designations { get; }
        IRepository<Employee> Employees { get; }
        IRepository<Attendance> Attendances { get; }
        IRepository<LeaveRequest> LeaveRequests { get; }
        IRepository<Payroll> Payrolls { get; }
        IRepository<Payslip> Payslips { get; }
        IRepository<Notification> Notifications { get; }
        
        Task<int> SaveChangesAsync(string userId = "System");
    }
}

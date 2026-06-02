using System;
using System.Threading.Tasks;
using EMPS.Core.Entities;
using EMPS.Core.Interfaces;
using EMPS.Infrastructure.Data;

namespace EMPS.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<Department>? _departments;
        private IRepository<Designation>? _designations;
        private IRepository<Employee>? _employees;
        private IRepository<Attendance>? _attendances;
        private IRepository<LeaveRequest>? _leaveRequests;
        private IRepository<Payroll>? _payrolls;
        private IRepository<Payslip>? _payslips;
        private IRepository<Notification>? _notifications;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Department> Departments => _departments ??= new Repository<Department>(_context);
        public IRepository<Designation> Designations => _designations ??= new Repository<Designation>(_context);
        public IRepository<Employee> Employees => _employees ??= new Repository<Employee>(_context);
        public IRepository<Attendance> Attendances => _attendances ??= new Repository<Attendance>(_context);
        public IRepository<LeaveRequest> LeaveRequests => _leaveRequests ??= new Repository<LeaveRequest>(_context);
        public IRepository<Payroll> Payrolls => _payrolls ??= new Repository<Payroll>(_context);
        public IRepository<Payslip> Payslips => _payslips ??= new Repository<Payslip>(_context);
        public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);

        public async Task<int> SaveChangesAsync(string userId = "System")
        {
            return await _context.SaveChangesAsync(userId);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

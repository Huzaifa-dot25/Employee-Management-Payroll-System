using System.Collections.Generic;
using System.Threading.Tasks;
using EMPS.Core.Entities;

namespace EMPS.Core.Interfaces.Services
{
    public interface IDesignationService
    {
        Task<IEnumerable<Designation>> GetAllDesignationsAsync();
        Task<Designation?> GetDesignationByIdAsync(int id);
        Task CreateDesignationAsync(Designation designation, string userId);
        Task UpdateDesignationAsync(Designation designation, string userId);
        Task DeleteDesignationAsync(int id, string userId);
        Task<IEnumerable<Designation>> GetDesignationsByDepartmentAsync(int departmentId);
    }
}

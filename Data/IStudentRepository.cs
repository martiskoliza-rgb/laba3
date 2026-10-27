using StudentCabinetMVVM.Models;

namespace StudentCabinetMVVM.Data;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<int> SaveAsync(Student student);
    Task<int> DeleteAsync(Student student);
}
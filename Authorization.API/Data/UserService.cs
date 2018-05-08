using Authorization.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Authorization.API.Data
{

    public interface IUserService
    {
        Task<int> AddAsync(User user);
        Task<bool> AnyByEmailAsync(string email);
        void Dispose();
        Task<User> FirstOrDefaultByEmailAsync(string email);
        Task<User> FirstOrDefaultByIdAsync(Guid id);
        Task<int> UpdateAsync(User user);
    }

    public class UserService : IDisposable, IUserService
    {
        private readonly DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AnyByEmailAsync(String email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<int> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<User> FirstOrDefaultByEmailAsync(String email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<int> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<User> FirstOrDefaultByIdAsync(Guid id)
        {
            return await _context.Users.Include(u => u.Telephones).FirstOrDefaultAsync(u => u.Id == id);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                    _context.Dispose();
            }
        }
    }
}

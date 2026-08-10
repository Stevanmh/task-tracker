using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<List<User>> GetAllAsync()
        => await _context.Users.OrderBy(u => u.Name).ToListAsync();

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}

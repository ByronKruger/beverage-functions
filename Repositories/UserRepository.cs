using Coffeeg.Entities;
using Coffeeg.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using TestFunctionApp.Data;

namespace TestFunctionApp.Repositories
{
    public class UserRepository(CoffeegDbContext Context) : IUserRepository
    {
        public IEnumerable<User> FindUserByName(string name)
        {
            return Context.Users
                .Where(u => u.UserName.StartsWith(name))
                .AsQueryable();
        }

        public async Task<User> FindUserByUsernameAsync(string username)
        {
            return await Context.Users
                .Where(u => u.UserName == username)
                .FirstOrDefaultAsync();
        }
    }
}

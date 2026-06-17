using Coffeeg.Entities;
using Coffeeg.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFunctionApp.Data;

namespace TestFunctionApp.Repositories
{
    public class AdminBeverageManagementRepository(CoffeegDbContext Context) : IAdminBeverageManagementRepository
    {
        public async Task<bool> AddBeverageType(BeverageType beverageType)
        {
            foreach (var ing in beverageType.Ingredients)
            {
                Context.Attach(ing);
            }

            Context.BeverageTypes.Add(beverageType);
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddIngredient(Ingredient ingredient)
        {
            //foreach (var beverageType in ingredient.BeverageTypes)
            //    Context.Attach(beverageType);

            Context.Ingredients.Add(ingredient);
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> BeverageTypeExists(string description)
        {
            return await Context.BeverageTypes.AnyAsync(bt => bt.Descr == description);
        }

        public async Task<bool> IngredientExists(string description)
        {
            return await Context.Ingredients.AnyAsync(i => i.Descr == description);
        }
    }
}

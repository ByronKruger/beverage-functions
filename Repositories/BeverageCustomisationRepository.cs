using AutoMapper;
using Coffeeg.Dtos;
using Coffeeg.Entities;
using Coffeeg.Helpers;
using Coffeeg.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestFunctionApp.Data;

namespace TestFunctionApp.Repositories
{
    public class BeverageCustomisationRepository(CoffeegDbContext Context, IMapper Mapper, ILogger<BeverageCustomisationRepository> _logger) :
        IBeverageCustomisationRepository
    {
        public Task<UserBeverageCustomisationResult> GetUserBeverageCustomisation(
            GetUserBeverageCustomisation entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<BeverageCustomisation>> CreateBeverageCustomisation(
            CreateBeverageCustomisation dto)
        {

            Console.WriteLine($"===== CONSOLE: REPO UserId = {dto.UserId} =====");
            Console.WriteLine($"===== CONSOLE: ComplexIngredientIds = {string.Join(",", dto.ComplexIngredientAmounts.Select(x => x.ComplexIngredientId))} =====");
            _logger.LogError("===== ILOGGER: REPO reached =====");
            //var beverageType = new BeverageType { Id = dto.BeverageTypeId };
            //Context.Attach(beverageType); // attach existing beverage type for newly created customisation
            //var user = new User { Id = "946ceec5-a8f5-4142-ae30-7da59f08246a" };
            //var user = new User { Id = dto.UserId };
            //Context.Attach(user);

            // Optional but recommended: fail fast with a clear domain error
            // instead of letting the database throw a low-level FK exception.
            Console.WriteLine($"\n\n**********************");
            Console.WriteLine($"UserId:\t{dto.UserId}");
            Debug.WriteLine($"\n\n**********************");
            Debug.WriteLine($"UserId:\t{dto.UserId}");

            _logger.LogInformation("=== CreateBeverageCustomisation START ===");
            _logger.LogInformation("Received UserId from DTO: '{UserId}' (Length: {Length})",
                dto.UserId, dto.UserId?.Length ?? 0);

            _logger.LogWarning("!!! CRITICAL TEST LOG (WARN) - {Time} !!!", DateTime.UtcNow);
            _logger.LogError("!!! CRITICAL TEST LOG (ERR) - {Time} !!!", DateTime.UtcNow);

            var userExists = await Context.Users
                .AnyAsync(u => u.Id == dto.UserId);

            if (!userExists)
                return Result<BeverageCustomisation>.Failure(
                    $"User '{dto.UserId}' does not exist.");

            var beverageCustomisation = new BeverageCustomisation
            {
                //BeverageType = beverageType,
                BeverageTypeId = dto.BeverageTypeId,
                //User = user,
                UserId = dto.UserId,
                CreateDt = DateTime.Now
            };

            //entity.BeverageType = beverageType;
            var ingredientAmounts = dto.IngredientAmounts;

            foreach (var ia in ingredientAmounts)
                beverageCustomisation.IngredientAmounts.Add(new Coffeeg.Entities.IngredientAmount
                { IngredientId = ia.IngredientId, Amount = ia.Amount });

            var complexIngredientAmounts = dto.ComplexIngredientAmounts;
            //foreach (var ca in complexIngredientAmounts)
            //    beverageCustomisation.ComplexIngredientAmounts.Add(new Coffeeg.Entities.ComplexIngredientAmount
            //    { ComplexIngredientId = ca.ComplexIngredientId, Amount = ca.Amount });

            foreach (var ca in dto.ComplexIngredientAmounts)
            {
                beverageCustomisation.ComplexIngredientAmounts.Add(new Coffeeg.Entities.ComplexIngredientAmount
                {
                    ComplexIngredientId = ca.ComplexIngredientId,
                    Amount = ca.Amount
                });
            }

            // ===== FORCE THE VALUES INTO THE EXCEPTION =====
            var ids = beverageCustomisation.ComplexIngredientAmounts
                .Select(x => x.ComplexIngredientId)
                .ToList();

            var duplicateCheck = ids
                .GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key} (x{g.Count()})")
                .ToList();

            if (duplicateCheck.Any())
            {
                // This will appear in the Log Stream as part of the exception
                throw new InvalidOperationException(
                    $"DUPLICATE ComplexIngredientIds detected before SaveChanges: [{string.Join(", ", ids)}]. " +
                    $"Duplicates: {string.Join(", ", duplicateCheck)}");
            }

            Context.BeverageCustomisations.Add(beverageCustomisation);

            // Deduplicate by ComplexIngredientId (take the last one sent)
            var uniqueComplex = dto.ComplexIngredientAmounts
                .GroupBy(x => x.ComplexIngredientId)
                .Select(g => g.Last())   // or .First(), or throw if you want strictness
                .ToList();

            beverageCustomisation.ComplexIngredientAmounts.Clear();

            foreach (var ca in uniqueComplex)
            {
                beverageCustomisation.ComplexIngredientAmounts.Add(new Coffeeg.Entities.ComplexIngredientAmount
                {
                    ComplexIngredientId = ca.ComplexIngredientId,
                    Amount = ca.Amount
                });
            }

            try
            {
                if (await Context.SaveChangesAsync() > 0)
                    return Result<BeverageCustomisation>.Success(beverageCustomisation);
                else
                    return Result<BeverageCustomisation>.Failure("No rows affected");
            }
            catch (DbUpdateException ex)
            {
                // Enrich the exception so we can see the values even if the above check was somehow bypassed
                throw new InvalidOperationException(
                    $"DbUpdateException. ComplexIngredientIds that were about to be inserted: [{string.Join(", ", ids)}]", ex);
            }



            //_logger.LogError("=== Values that will be inserted ===");

            //foreach (var ca in beverageCustomisation.ComplexIngredientAmounts)
            //{
            //    _logger.LogError("ComplexIngredientAmount → ComplexIngredientId = {Id}, Amount = {Amount}",
            //        ca.ComplexIngredientId, ca.Amount);
            //}

            //Context.BeverageCustomisations.Add(beverageCustomisation);

            //if (await Context.SaveChangesAsync() > 0)
            //    return Result<BeverageCustomisation>.Success(beverageCustomisation);
            //else
            //    return Result<BeverageCustomisation>.Failure("");

        }
        //FK_BeverageCustomisation_BeverageType_BeverageTypeId

        //public async Task<Result<BeverageCustomisation>> UpdateBeverageCustomisation(BeverageCustomisation entity)
        //{
        //    //var bcId = entity.Id;

        //    //// ───────────────────────────────────────────────
        //    ////  Option A – Most efficient (recommended) – ExecuteUpdate style (EF Core 7+)
        //    //// ───────────────────────────────────────────────

        //    //// IngredientAmounts
        //    //foreach (var update in entity.IngredientAmounts)
        //    //{
        //    //    await Context.IngredientAmounts
        //    //        .Where(ia => ia.BeverageCustomisationId == entity.Id
        //    //                  && ia.IngredientId == update.Id)
        //    //        .ExecuteUpdateAsync(setters => setters
        //    //            .SetProperty(ia => ia.Amount, update.Amount));
        //    //}

        //    //// Same for ComplexIngredientAmounts
        //    //foreach (var update in entity.ComplexIngredientAmounts)
        //    //{
        //    //    await Context.ComplexIngredientAmounts
        //    //        .Where(cia => cia.BeverageCustomisationId == entity.Id
        //    //                   && cia.ComplexIngredientId == update.Id)
        //    //        .ExecuteUpdateAsync(setters => setters
        //    //            .SetProperty(cia => cia.Amount, update.Amount));
        //    //}

        //    //var fullEntity = await Context.BeverageCustomisations
        //    //    .Where(bc => bc.Id == entity.Id)
        //    //    .Include(bc => bc.BeverageType.Ingredients)
        //    //        .ThenInclude(i => i.ComplexIngredients)
        //    //    .Include(bc => bc.IngredientAmounts)
        //    //    .Include(bc => bc.ComplexIngredientAmounts)
        //    //    .SingleOrDefaultAsync();

        //    //return Result<BeverageCustomisation>.Success(fullEntity);

        //    Context.BeverageCustomisations.Update(entity);

        //    foreach (var item in entity.ComplexIngredientAmounts)
        //        Context.ComplexIngredientAmounts.Update(item);

        //    foreach (var item in entity.IngredientAmounts)
        //        Context.IngredientAmounts.Update(item);

        //    await Context.SaveChangesAsync();

        //    var fullEntity = await Context.BeverageCustomisations
        //        .Where(bc => bc.Id == entity.Id)
        //        .Include(bc => bc.BeverageType.Ingredients)
        //            .ThenInclude(i => i.ComplexIngredients)
        //        .Include(bc => bc.IngredientAmounts)
        //        .Include(bc => bc.ComplexIngredientAmounts)
        //        .SingleOrDefaultAsync();

        //    return Result<BeverageCustomisation>.Success(fullEntity);
        //}

        public async Task<Result<BeverageCustomisation>> UpdateBeverageCustomisation(
            BeverageCustomisation entity)
        {
            var beverageCust = await Context.BeverageCustomisations
                .Where(bc => bc.Id == entity.Id)
                .Include(bc => bc.ComplexIngredientAmounts)
                .Include(bc => bc.IngredientAmounts)
                .SingleOrDefaultAsync();

            foreach (var cia in entity.ComplexIngredientAmounts)
            {
                var ciaToUpdate = beverageCust.ComplexIngredientAmounts
                    .Where(c => c.Id == cia.Id).First();
                ciaToUpdate.Amount = cia.Amount;
            }

            foreach (var ia in entity.IngredientAmounts)
            {
                var iaToUpdate = beverageCust.IngredientAmounts
                    .Where(ia =>
                        ia.BeverageCustomisationId == entity.Id &&
                        ia.IngredientId == ia.IngredientId).Single();

                //.Where(c => c.Id == ia.Id).First();
                iaToUpdate.Amount = ia.Amount;
            }

            await Context.SaveChangesAsync();

            var fullEntity = await Context.BeverageCustomisations
                .Where(bc => bc.Id == entity.Id)
                .Include(bc => bc.BeverageType.Ingredients)
                    .ThenInclude(i => i.ComplexIngredients)
                .Include(bc => bc.IngredientAmounts)
                .Include(bc => bc.ComplexIngredientAmounts)
                .SingleOrDefaultAsync();

            return Result<BeverageCustomisation>.Success(fullEntity);
        }

        public async Task<Result<List<BeverageType>>> GetBeverageTypesAsync()
        {
            var beverageTypes = await Context.BeverageTypes
                //.Include(bt => bt.Ingredients)
                .Include(bt => bt.Ingredients)
                    .ThenInclude(i => i.ComplexIngredients)
                .AsNoTracking()
                .ToListAsync();

            //if (beverageTypes.Count == 0) return Result<List<BeverageType>>.Success([]);

            return Result<List<BeverageType>>.Success(beverageTypes);
        }

        public async Task<List<User>> GetUserByNames(string name)
        {
            return await Context.Users
                .Where(u => u.UserName.StartsWith(name) ||
                    u.FirstName.StartsWith(name) ||
                    u.LastName.StartsWith(name))
                .ToListAsync();
        }

        public async Task<List<BeverageCustomisation>> GetBeverageCustomisationsForUser(string userId)
        {
            return await Context.BeverageCustomisations
                .Include(bc => bc.BeverageType)
                    .ThenInclude(bt => bt.Ingredients)
                        .ThenInclude(i => i.ComplexIngredients)
                .Include(bc => bc.IngredientAmounts)
                .Include(bc => bc.ComplexIngredientAmounts)
                .Include(bc => bc.User)
                .Where(bc => bc.UserId == userId)
                .ToListAsync();
        }

        public async Task<BeverageCustomisation?> GetBeverageCustomisationByBeverageType(
            string userId, int beverageTypeId)
        {
            //return await Context.BeverageCustomisations.FindAsync(beverageCustomisationId);
            return await Context.BeverageCustomisations
                .Where(bc => bc.BeverageTypeId == beverageTypeId &&
                    bc.UserId == userId)
                .Include(bc => bc.BeverageType.Ingredients)
                    .ThenInclude(i => i.ComplexIngredients)
                .Include(bc => bc.IngredientAmounts)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BeverageCustomisation>> GetRecentBeverageCustomisations()
        {
            return await Context.BeverageCustomisations
                .Where(bc => bc.CreateDt >= DateTime.UtcNow.AddMinutes(45))
                .Include(bc => bc.User)
                .Include(bc => bc.BeverageType.Ingredients)
                    //.ThenInclude(bt => bt.Ingredients)
                    .ThenInclude(i => i.ComplexIngredients)
                .Include(bc => bc.IngredientAmounts)
                .Include(bc => bc.ComplexIngredientAmounts)
                .OrderByDescending(bc => bc.CreateDt)
                .ToListAsync();
        }

        //public async Task<List<BeverageCustomisation>> GetBeverageCustomisations()
        //{
        //    return await Context.BeverageTypes
        //        .Include(bt => bt.Ingredients)
        //            .ThenInclude(i => i.ComplexIngredients)
        //        .Select(bt =>

        //            new BeverageCustomisation
        //            {
        //                BeverageType = bt
        //            }
        //        )
        //        .ToListAsync();
        //}
    }
    //public async Task<Result<BeverageCustomisation>> UpdateBeverageCustomisation(BeverageCustomisation entity)
    //{

    //    Context.Attach(entity);
    //    //entity.BeverageTypeId = 
    //    Context.Entry(entity).State = EntityState.Modified;
    //    await Context.SaveChangesAsync();

    //    foreach (var e in entity.ComplexIngredientAmounts)
    //    {
    //        //e.BeverageCustomisationId = entity.Id;
    //        Context.ComplexIngredientAmounts.Attach(e);
    //        Context.Entry(e).State = EntityState.Modified;
    //    }

    //    foreach (var e in entity.IngredientAmounts)
    //    {
    //        //e.BeverageCustomisationId = entity.Id;
    //        Context.IngredientAmounts.Attach(e);
    //        Context.Entry(e).State = EntityState.Modified;
    //    }

    //    await Context.SaveChangesAsync();

    //    //if (await Context.SaveChangesAsync() > 0)
    //    //{

    //    var fullEntity = await Context.BeverageCustomisations
    //        .Where(bc => bc.Id == entity.Id)
    //        .Include(bc => bc.BeverageType.Ingredients)
    //            .ThenInclude(i => i.ComplexIngredients)
    //        .Include(bc => bc.IngredientAmounts)
    //        .Include(bc => bc.ComplexIngredientAmounts)
    //        .SingleOrDefaultAsync();

    //    return Result<BeverageCustomisation>.Success(fullEntity);
    //    //}
    //    //else
    //    //return Result<BeverageCustomisation>.Failure("Could not update beverage customisation");
    //}

    //public async Task<Result<BeverageCustomisation>> UpdateBeverageCustomisation(BeverageCustomisation updated)
    //{
    //    // Fetch current state (tracked)
    //    var current = await Context.BeverageCustomisations
    //        .Include(bc => bc.IngredientAmounts)
    //        .Include(bc => bc.ComplexIngredientAmounts)
    //        .FirstOrDefaultAsync(bc => bc.Id == updated.Id);

    //    if (current == null)
    //        return Result<BeverageCustomisation>.Failure("Not found");

    //    // Update scalar properties on parent
    //    Context.Entry(current).CurrentValues.SetValues(updated);

    //    // Sync child collections (most robust way)
    //    SyncCollection(current.IngredientAmounts, updated.IngredientAmounts, (a, b) => a.Id == b.Id);
    //    SyncCollection(current.ComplexIngredientAmounts, updated.ComplexIngredientAmounts, (a, b) => a.Id == b.Id);

    //    await Context.SaveChangesAsync();

    //    // Reload full graph for return (or project if you want to avoid over-fetching)
    //    var full = await Context.BeverageCustomisations
    //        .Include(bc => bc.BeverageType)
    //            .ThenInclude(bt => bt.Ingredients)
    //                .ThenInclude(i => i.ComplexIngredients)
    //        .Include(bc => bc.IngredientAmounts)
    //        .Include(bc => bc.ComplexIngredientAmounts)
    //        .SingleAsync(bc => bc.Id == updated.Id);

    //    return Result<BeverageCustomisation>.Success(full);
    //}

    //// Helper (can be extension method / shared utility)
    //private void SyncCollection<T>(
    //    ICollection<T> currentItems,
    //    ICollection<T> incomingItems,
    //    Func<T, T, bool> idComparer)
    //    where T : class
    //{
    //    var toRemove = currentItems
    //        .Where(c => !incomingItems.Any(i => idComparer(c, i)))
    //        .ToList();

    //    foreach (var remove in toRemove)
    //        currentItems.Remove(remove);

    //    foreach (var incoming in incomingItems)
    //    {
    //        var existing = currentItems.FirstOrDefault(c => idComparer(c, incoming));
    //        if (existing != null)
    //        {
    //            Context.Entry(existing).CurrentValues.SetValues(incoming);
    //        }
    //        else
    //        {
    //            currentItems.Add(incoming);
    //        }
    //    }
    //}
    //}
}

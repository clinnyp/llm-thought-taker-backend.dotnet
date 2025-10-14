using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using llm_thought_taker.Data;
using llm_thought_taker.Models;

namespace llm_thought_taker.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var usersGroup = app.MapGroup("/users");

        // POST /user
        usersGroup.MapPost("/", CreateUser);

        // GET /user
        usersGroup.MapGet("/", GetAllUsers);

        // DELETE /user/{externalUserId}
        usersGroup.MapDelete("/{externalUserId}", DeleteUser);
    }

    private static async Task<IResult> CreateUser(
        [FromBody] User user,
        AppDbContext db)
    {
        if (user == null)
        {
            return Results.BadRequest(new { message = "Invalid user" });
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = "User created Successfully", user });
    }

    private static async Task<IResult> GetAllUsers(AppDbContext db)
    {
        var users = await db.Users.ToListAsync();
        return Results.Ok(users);
    }

    private static async Task<IResult> DeleteUser(
        string externalUserId,
        AppDbContext db)
    {
        var user = await db.Users
            .Where(u => u.ExternalId == externalUserId)
            .SingleOrDefaultAsync();

        if (user == null)
        {
            return Results.NotFound(new { message = "User not found" });
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = "User Deleted Successfully", user });
    }
}


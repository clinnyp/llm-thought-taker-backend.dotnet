using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using llm_thought_taker.Data;
using llm_thought_taker.Models;

namespace llm_thought_taker.Functions;

public class UsersFunctions
{
  
    private readonly AppDbContext _db;
    
    public UsersFunctions(AppDbContext db)
    {
        _db = db;
    }
    
    [Function("CreateUser")]
    public async Task<HttpResponseData> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user")] HttpRequestData req)
    {
       var user = await req.ReadFromJsonAsync<User>();
       if (user == null)
       {
           var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
           await badRes.WriteAsJsonAsync(new { message = "Invalid user" });
           return badRes;
       }
       _db.Users.Add(user);
       await _db.SaveChangesAsync();
       var res = req.CreateResponse(HttpStatusCode.OK);
       await res.WriteAsJsonAsync(new { message = "User created Successfully", user });;
       return res;
    }

    [Function("GetUsers")]
    public async Task<HttpResponseData> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user")] HttpRequestData req)
    {
        var users = _db.Users.ToList();
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(users);
        return res;
    }

}
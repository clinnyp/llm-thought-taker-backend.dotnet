using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using GenerativeAI;
using llm_thought_taker.Data;
using llm_thought_taker.DTOs;
using llm_thought_taker.Models;
using Microsoft.EntityFrameworkCore;


namespace llm_thought_taker.Functions;

public class NotesFunctions
{
    private readonly AppDbContext _db;
    private readonly GenerativeModel _model;

    public NotesFunctions(AppDbContext db, GenerativeModel googleAi)
    {
        _db = db;
        _model = googleAi;
    }
    
    [Function("GetNoteById")]
    public async Task<HttpResponseData> GetNoteById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notes/{noteId}/users/{externalUserId}")] HttpRequestData req, string noteId, string externalUserId)
    {
        try
        {
            var user = await _db.Users.Where(u => u.ExternalId == externalUserId).SingleOrDefaultAsync();
            if (user == null)
            {
                var badRes = req.CreateResponse(HttpStatusCode.NotFound);
                await badRes.WriteAsJsonAsync(new { message = "Note belonging to user not found" });
                return badRes;
            } 
            // var note = await _db.Notes.Where(n => n.Id == Guid.Parse(noteId) && n.UserId == user.Id ).SingleOrDefaultAsync();
            var note = await _db.Notes.Where(n => n.Id == Guid.Parse(noteId)).FirstOrDefaultAsync(u => u.User.ExternalId == externalUserId); 
            // Console.WriteLine("this is the note", note);
            if (note == null)
            {
                var badRes = req.CreateResponse(HttpStatusCode.NotFound);
                await badRes.WriteAsJsonAsync(new { message = "Note not found" });
                return badRes;
            }
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(new { message = "Note Retrieved Successfully", note });
            return res;
        }
        catch (Exception e)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { message = $"Error retrieving notes: {e.Message}" });
            return errorResponse;  
        }
    }

    [Function("CreateNote")]
    public async Task<HttpResponseData> CreateNote(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notes")] HttpRequestData req, string externalUserId)
    {
        var noteRequest = await req.ReadFromJsonAsync<NoteRequest>();
        if (noteRequest == null)
        {
            var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRes.WriteAsJsonAsync(new { message = "Invalid Note" });
            return badRes;
        }
        var user = await _db.Users.Where(u => u.ExternalId == noteRequest.ExternalUserId).SingleOrDefaultAsync();
        if (user == null)
        {
            var badRes = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRes.WriteAsJsonAsync(new { message = "User not found" });
            return badRes;
        }

        var note = new Note
        {
            UserId = user.Id,
            Title = noteRequest.Title,
            Content = noteRequest.Content,
            Prompt = noteRequest.Prompt,
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "Note Created Successfully", note });;
        return res;
    }
    
    [Function("DeleteNote")]
    public async Task<HttpResponseData> DeleteNote(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "notes/{noteId}")] HttpRequestData req, string noteId)
    {
        var note = await _db.Notes.Where(n => n.Id == Guid.Parse(noteId)).SingleOrDefaultAsync();
        if (note == null)
        {
           var badRes = req.CreateResponse(HttpStatusCode.NotFound); 
           await badRes.WriteAsJsonAsync(new { message = "Note not found" });
           return badRes;
        }
        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "Note Deleted Successfully", note });;
        return res;
    }
    
    
    
    [Function("GetAllNotesForUser")]
    public async Task<HttpResponseData> GetAllNotesForUser(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notes/users/{externalUserId}")] HttpRequestData req, string externalUserId)
    {
        try
        {
            var notes = await _db.Users.Where(u => u.ExternalId == externalUserId).SelectMany(u => u.Notes).OrderByDescending(note => note.Created).ToListAsync();
            if (notes.Count == 0)
            {
                var badRes = req.CreateResponse(HttpStatusCode.NotFound);
                await badRes.WriteAsJsonAsync(new { message = "No notes found for user" });
                return badRes;
            }
            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteAsJsonAsync(new { message = "Notes Retrieved Successfully", notes });;
            return res;
        }
        catch (Exception e)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { message = $"Error retrieving notes: {e.Message}" });
            return errorResponse; 
        }
    }
    
    [Function("GenerateChatResponse")]
    public async Task<HttpResponseData> GenerateChatResponse(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "generate_chat")] HttpRequestData req)
    {
        var prompt = await req.ReadFromJsonAsync<PromptRequest>();
        var modelResponse = await _model.GenerateContentAsync(prompt.Prompt);
        if (prompt.Prompt == null)
        {
           return req.CreateResponse(HttpStatusCode.BadRequest); 
        }
        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new { message = "Chat Generated Successfully", chat = modelResponse.Text() });;
        return res;
    }

}
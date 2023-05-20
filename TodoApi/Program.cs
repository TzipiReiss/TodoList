using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoListContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "CorsPolicy",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("CorsPolicy");

app.MapGet("/items", async (TodoListContext db) => await db.Items.ToListAsync());

app.MapPost(
    "/items",
    async (Item item, TodoListContext db) =>
    {
        db.Items.Add(item);
        await db.SaveChangesAsync();
        return Results.Created($"/items/{item.Id}", item);
    }
);

app.MapPut(
    "/items/{id}",
    async (int id, bool isComplete, TodoListContext db) =>
    {
        var temp = await db.Items.FindAsync(id);

        if (temp is null)
            return Results.NotFound();
        temp.IsComplete = isComplete;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
);

app.MapDelete(
    "/items/{id}",
    async (int id, TodoListContext db) =>
    {
        var item = await db.Items.FindAsync(id);
        if (item is null)
            return Results.NotFound();
        db.Items.Remove(item);
        await db.SaveChangesAsync();
        return Results.Ok(item);
    }
);

app.MapGet("/", () => {return Results.Ok("server api is running");});

app.Run();

using DynamicQuery.AspNetCore;
using DynamicQuery.AspNetCore.Test.Controllers;
using DynamicQuery.AspNetCore.Test.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.MapGet("/api/users/minimal", (DynamicQuery<User> query) =>
{
    return UserData.Users.Apply(query).ToList();
});

app.Run();

public partial class Program;

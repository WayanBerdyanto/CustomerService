using System.Text;
using CustomerService.DAL;
using CustomerService.DAL.Interfaces;
using CustomerService.DTO;
using CustomerService.Models;
using CustomerService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUsers, UserDAL>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/users", (IUsers user) =>
{
    // BCrypt.Net.BCrypt.HashPassword(userModel.Password);
    return Results.Ok(user.GetAll());
});

app.MapGet("/users/{username}", (string username, IUsers user) =>
{
    var users = user.GetByName(username);
    if (users == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(users);
});


app.MapPost("/user", (IUsers user, Users obj) =>
{
    try
    {
        Users users = new Users
        {
            UserName = obj.UserName,
            Password = obj.Password,
            FullName = obj.FullName
        };
        user.Insert(users);
        return Results.Created($"/user/{users.UserName}", users);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPut("/user", (IUsers user, Users obj) =>
{
    try
    {
        Users users = new Users
        {
            Password = obj.Password,
            FullName = obj.FullName
        };
        user.Update(users);
        return Results.Created($"/user/{users.UserName}", users);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapDelete("/user/{username}", (IUsers user, string username) =>
{
    try
    {
        user.DeleteUser(username);
        return Results.Ok(new { success = true, message = "request delete successful" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/login", async (IUsers user, Users login) =>
{

    try
    {
        var users = user.ValidateUser(login.UserName, login.Password);
        if (users != null)
        {
            TokenJwt tokenJwt = new TokenJwt(users, builder.Configuration);

            var token = tokenJwt.GenerateJwtToken();
            return Results.Ok(new { Message = "Login Berhasil", Token = token });
        }
        else
        {
            return Results.Unauthorized();
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapPut("/users/updatebalance", async (IUsers userDal, UserUpdateBalanceDTO userDto) =>
{
    try
    {
        await userDal.UpdateBalanceAsync(userDto.UserName, userDto.Balance);
        return Results.Ok(new { Message = "Users Balance updated successfully" });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Message = "An error occurred while updating the Users Balance", Error = ex.Message });
    }
});

app.MapPut("/users/updateShippingBalance", async (IUsers userDal, UserUpdateBalanceDTO userDto) =>
{
    try
    {
        await userDal.UpdateBalanceAsync(userDto.UserName, userDto.Balance);
        return Results.Ok(new { Message = "Users Balance updated successfully" });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Message = "An error occurred while updating the Users Balance", Error = ex.Message });
    }
});


app.MapPut("/users/updateBackBalance", async (IUsers userDal, UserUpdateBalanceDTO userDto) =>
{
    try
    {
        await userDal.UpdateBackBalanceAsync(userDto.UserName, userDto.Balance);
        return Results.Ok(new { Message = "Users Balance updated successfully" });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Message = "An error occurred while updating the Users Balance", Error = ex.Message });
    }
});

app.MapPut("/users/topUpBackBalance", async (IUsers userDal, UserUpdateBalanceDTO userDto) =>
{
    try
    {
        await userDal.TopUpBalanceAsync(userDto.UserName, userDto.Balance);
        return Results.Ok(new { Message = "TopUp Balance updated successfully" });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { Message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Message = "An error occurred while updating the Users Balance", Error = ex.Message });
    }
});


app.UseHttpsRedirection();


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

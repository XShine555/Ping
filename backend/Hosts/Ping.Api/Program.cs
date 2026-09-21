using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Ping.Api.Endpoints;
using Ping.Api.Extensions;
using Ping.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

const string devCorsPolicy = "dev-cors";

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddProblemDetails();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options => options.AddPolicy(devCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:3100").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
}

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseCors(devCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app
    .MapUserEndpoints()
    .MapFriendEndpoints()
    .MapServerEndpoints()
    .MapChannelEndpoints()
    .MapMessageEndpoints()
    .MapAttachmentEndpoints()
    .MapCallEndpoints();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();

public partial class Program;

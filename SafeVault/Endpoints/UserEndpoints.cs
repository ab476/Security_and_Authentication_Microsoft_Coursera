using Microsoft.AspNetCore.Http.HttpResults;

namespace SafeVault.Endpoints;

public static class UserEndpoints
{
    private const string BaseRoute = "/api/users";

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
                       .WithTags("Users");

        // GET /api/users/{username}
        group.MapGet("/{username}", 
            async Task<Results<Ok<UserResponse>, BadRequest<string>, NotFound>> (
                string username,
                IUserService userService,
                CancellationToken ct) =>
            {
                if (!InputValidator.IsSafeUsername(username))
                {
                    return TypedResults.BadRequest("Invalid username. Only letters, numbers, underscore, dot, hyphen allowed.");
                }
                var request = new GetUserRequest(username);
                var result = await userService.GetUserAsync(request, ct);

                return result is not null
                    ? TypedResults.Ok(result)
                    : TypedResults.NotFound();
            }
        )
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/users
        group.MapPost("/", 
            async Task<Results<Created<UserResponse>, BadRequest<string>>> (
                CreateUserRequest request,
                IUserService userService,
                CancellationToken ct) =>
            {
                if (!InputValidator.IsSafeUsername(request.Username))
                {
                    return TypedResults.BadRequest("Invalid username. Only letters, numbers, underscore, dot, hyphen allowed.");
                }

                if (!InputValidator.IsSafeEmail(request.Email))
                {
                    return TypedResults.BadRequest("Invalid or unsafe email format.");
                }
                await userService.SaveUserAsync(request, ct);

                var response = await userService.GetUserAsync(new GetUserRequest(request.Username), ct)!;

                return TypedResults.Created($"{BaseRoute}/{request.Username}", response);
            }
        )
        .Produces<UserResponse>(StatusCodes.Status201Created);

        return app;
    }
}

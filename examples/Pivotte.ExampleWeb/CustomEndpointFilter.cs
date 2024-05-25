namespace Pivotte.ExampleWeb;

public class CustomEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(
        EndpointFilterInvocationContext endpointFilterInvocationContext,
        EndpointFilterDelegate next)
    {
        try
        {
            return await next(endpointFilterInvocationContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
            return Results.StatusCode(500);
        }
    }
}

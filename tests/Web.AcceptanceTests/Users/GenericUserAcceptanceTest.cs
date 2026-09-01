using System.Net.Http.Json;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Users;

namespace VibraHeka.Web.AcceptanceTests.Users;

public abstract class GenericUserAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    private const string AdminCreateTherapistEndpoint = "/api/v1/users/admin/create-therapist";
    private const string AdminGetTherapistsEndpoint = "/api/v1/users/admin/therapists";
    private const string GetUserProfileEndpointTemplate = "/api/v1/users/{0}";

    
    // Get users endpoint
    protected Task<HttpResponseMessage> InvokeGetUserProfileEndpoint(Guid userId) => Client.GetAsync(string.Format(GetUserProfileEndpointTemplate, userId));
    
    protected Task<UserDTO> PerformGetUserProfile(Guid userId) => PerformCallAndRetrieveContent<UserDTO>(() => InvokeGetUserProfileEndpoint(userId));
    
    
    // Admin endpoints for creating and retrieving therapists
    protected Task<HttpResponseMessage> InvokeCreateTherapistEndpoint(CreateTherapistRequest request) => Client.PutAsJsonAsync(AdminCreateTherapistEndpoint, request);
    
    protected Task<CreateTherapistResponse> PerformCreateTherapist(CreateTherapistRequest request) => PerformCallAndRetrieveContent<CreateTherapistResponse>(() => InvokeCreateTherapistEndpoint(request));

    protected Task<HttpResponseMessage> InvokeGetTherapistsEndpoint() => Client.GetAsync(AdminGetTherapistsEndpoint);
    
    protected Task<IEnumerable<UserDTO>> PerformGetTherapists() => PerformCallAndRetrieveContent<IEnumerable<UserDTO>>(InvokeGetTherapistsEndpoint);
    
    
    protected CreateTherapistRequest ValidCreateTherapistRequest(
        string? email = null,
        string? firstName = null,
        string? middleName = null,
        string? lastName = null,
        string? phoneNumber = null)
    {
        return new CreateTherapistRequest
        {
            Email = email ?? $"{Guid.NewGuid():N}@example.com",
            FirstName = firstName ?? "Valid Therapist",
            MiddleName = middleName ?? "Valid Middle",
            LastName = lastName ?? "Valid Last",
            PhoneNumber = phoneNumber ?? "+34911111222",
            TimezoneID = "Europe/Madrid"
        };
    }
}

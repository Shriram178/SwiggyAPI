using System.Net.Http.Headers;
using System.Text.Json;
using Foodiee.DTO;

namespace Foodiee.Services
{
    public class KeycloakService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public KeycloakService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetAdminAccessTokenAsync()
        {
            var keycloakUrl = _config["Keycloak:AuthUrl"]?.TrimEnd('/');
            var realm = _config["Keycloak:Realm"]!;
            var adminClientId = _config["Keycloak:AdminClientId"]!;
            var adminClientSecret = _config["Keycloak:AdminClientSecret"]!;
            var adminUsername = _config["Keycloak:AdminUsername"]!;
            var adminPassword = _config["Keycloak:AdminPassword"]!;
            var tokenResponse = await _httpClient.PostAsync(
                $"{keycloakUrl}/realms/master/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = adminClientId,
                    ["client_secret"] = adminClientSecret,
                    ["username"] = adminUsername,
                    ["password"] = adminPassword
                }));
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var err = await tokenResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Keycloak admin login failed: {err}");
            }
            var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            return tokenJson.GetProperty("access_token").GetString() ?? throw new Exception("Could not retrieve admin token from Keycloak.");
        }

        /// <summary>
        /// 1) Uses client_credentials to get an admin token.
        /// 2) Calls /admin/realms/{realm}/users to create the new user.
        /// 3) Sets the initial password.
        /// 4) Assigns "restaurant-owner" role if requested.
        /// </summary>
        public async Task RegisterNewUserAsync(UserRegisterDto dto)
        {
            var keycloakUrl = _config["Keycloak:AuthUrl"]?.TrimEnd('/');
            var realm = _config["Keycloak:Realm"]!;
            var adminToken = await GetAdminAccessTokenAsync();

            if (string.IsNullOrEmpty(adminToken))
                throw new Exception("Could not retrieve admin token from Keycloak.");

            int length = dto.Username.Length;
            string firstName = dto.Username.Substring(0, length / 2);
            string lastName = dto.Username.Substring(length / 2);

            // 2. Create the new user
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", adminToken);

            var userCreate = new
            {
                username = dto.Username,
                email = dto.Email,
                enabled = true,
                firstName = firstName,
                lastName = lastName
            };

            var createResponse = await _httpClient.PostAsJsonAsync(
                $"{keycloakUrl}/admin/realms/{realm}/users",
                userCreate
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var err = await createResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Keycloak user creation failed: {err}");
            }

            // 3. Extract user ID from Location header
            var location = createResponse.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(location))
                throw new Exception("Keycloak did not return the new user’s ID.");

            var userId = location.Split('/').Last();

            // 4. Set the user's initial password
            var passwordPayload = new
            {
                type = "password",
                temporary = false,
                value = dto.Password
            };

            var setPasswordResponse = await _httpClient.PutAsJsonAsync(
                $"{keycloakUrl}/admin/realms/{realm}/users/{userId}/reset-password",
                passwordPayload
            );

            if (!setPasswordResponse.IsSuccessStatusCode)
            {
                var err = await setPasswordResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Keycloak password reset failed: {err}");
            }

            // 5. Assign the restaurant-owner role if applicable
            if (dto.IsRestaurantOwner)
            {
                // Get the role representation
                var roleResponse = await _httpClient.GetAsync(
                    $"{keycloakUrl}/admin/realms/{realm}/roles/restaurant-owner"
                );

                if (!roleResponse.IsSuccessStatusCode)
                {
                    var err = await roleResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to fetch role: {err}");
                }

                var role = await roleResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();

                var assignRoleResponse = await _httpClient.PostAsJsonAsync(
                    $"{keycloakUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm",
                    new[] { role }
                );

                if (!assignRoleResponse.IsSuccessStatusCode)
                {
                    var err = await assignRoleResponse.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to assign role: {err}");
                }
            }
        }


        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var keycloakUrl = _config["Keycloak:AuthUrl"]?.TrimEnd('/');
            var realm = _config["Keycloak:Realm"]!;
            var clientId = _config["Keycloak:ClientId"]!;
            var clientSecret = _config["Keycloak:ClientSecret"]!;

            // 1) Build the form data
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = clientId,
                ["username"] = dto.Username,
                ["password"] = dto.Password,
                ["client_secret"] = clientSecret
            };


            // 2) POST to the token endpoint
            var tokenEndpoint = $"{keycloakUrl}/realms/{realm}/protocol/openid-connect/token";
            var response = await _httpClient.PostAsync(
            tokenEndpoint,
                new FormUrlEncodedContent(formData)
            );

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Keycloak login failed ({response.StatusCode}): {err}");
            }

            // 3) Read Keycloak’s JSON into our LoginResponseDto
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            if (loginResponse == null)
                throw new Exception("Unexpected empty response from Keycloak token endpoint.");

            return loginResponse;
        }

    }
}

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

        /// <summary>
        /// 1) Uses client_credentials to get an admin token.
        /// 2) Calls /admin/realms/{realm}/users to create the new user.
        /// 3) Sets the initial password.
        /// </summary>
        public async Task RegisterNewUserAsync(UserRegisterDto dto)
        {
            var keycloakUrl = _config["Keycloak:AuthUrl"]?.TrimEnd('/');
            var realm = _config["Keycloak:Realm"]!;
            var adminClientId = _config["Keycloak:AdminClientId"]!;
            var adminClientSecret = _config["Keycloak:AdminClientSecret"]!;
            var adminUsername = _config["Keycloak:AdminUsername"]!;
            var adminPassword = _config["Keycloak:AdminPassword"]!;

            // 1. Get an admin access token (client_credentials)
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
            var adminToken = tokenJson.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(adminToken))
                throw new Exception("Could not retrieve admin token from Keycloak.");

            int length = dto.Username.Length;
            string firstName = dto.Username.Substring(0, length / 2);
            string lastName = dto.Username.Substring(length / 2);

            // 2. Create the new user in Keycloak
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

            // 3. Keycloak returns 201 Created → Location header has /users/{id}
            var location = createResponse.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(location))
                throw new Exception("Keycloak did not return the new user’s ID.");

            var segments = location.Split('/');
            var userId = segments.Last();

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

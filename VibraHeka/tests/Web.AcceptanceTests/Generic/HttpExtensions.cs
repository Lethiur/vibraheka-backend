using System.Data;
using System.Text.Json;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Web.AcceptanceTests.Generic;

public static class HttpExtensions
{
    extension(HttpResponseMessage responseMessage)
    {
        public async Task<ResponseEntity> GetAsResponseEntityAndContentAs<T>()
        {
            string stream = await responseMessage.Content.ReadAsStringAsync();

            ResponseEntity res = JsonSerializer.Deserialize<ResponseEntity>(stream, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new DataException($"There was a problem deserializing {stream}");

            if (res.Content == null) return res;

            T? entity = ((JsonElement)res.Content).Deserialize<T>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            res.Content = entity;
            return res;
        }

        public async Task<ResponseEntity> GetAsResponseEntity()
        {
            string stream = await responseMessage.Content.ReadAsStringAsync();

            ResponseEntity? res = JsonSerializer.Deserialize<ResponseEntity>(stream, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });


            return res ?? throw new DataException($"There was a problem deserializing {stream}");
        }
    }
}

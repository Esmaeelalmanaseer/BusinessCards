using System.Net.Http.Headers;
using System.Text;

namespace Api.IntegrationTests;

public static class HttpClientExtensions
{
    public static MultipartFormDataContent AsMultipartFile(this byte[] bytes, string field, string fileName, string contentType)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        content.Add(file, field, fileName);
        return content;
    }

    public static StringContent AsJson(this string json) =>
        new StringContent(json, Encoding.UTF8, "application/json");
}

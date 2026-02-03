using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Diagnostics;
using YAAL;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using static YAAL.FileSettings;
using static YAAL.LauncherSettings;
using System.ComponentModel;

public static partial class WebManager
{
    private static readonly HttpClient client = new HttpClient();
    public static bool IsValidURL(string url)
    {
        Uri uriResult;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        return result;
    }
    public static async Task<bool> DownloadFile(string url, string savePath)
    {
        using HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");

        try
        {
            using HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ErrorManager.ThrowError(
                "WebManager - Download failed",
                "Trying to download " + url + " ended with status code " + response.StatusCode + " " + response.ReasonPhrase
                );
                return false;
            }
            try
            {
                byte[] file = await response.Content.ReadAsByteArrayAsync();
                FileManager.SaveFile(savePath, file);
                Trace.WriteLine($"Downloaded: {savePath}");
                return true;
            }
            catch (Exception e)
            {
                ErrorManager.ThrowError(
                "WebManager - Download attempt raised an exception",
                "Trying to download " + url + " raised the following exception : " + e.Message
                );
                return false;
            }
            
        }
        catch (Exception e)
        {
            ErrorManager.ThrowError(
                "WebManager - Request attempt raised an exception",
                "The first request trying to download " + url + " raised the following exception : " + e.Message
                );
            return false;
        }
    }
}

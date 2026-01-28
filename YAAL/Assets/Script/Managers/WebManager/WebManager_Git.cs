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
    public static async Task GetGitHubReleaseRequest(string originalURL, string versionTag, Action<HttpResponseMessage?> onComplete)
    {
        if(originalURL == null || originalURL == "")
        {
            ErrorManager.AddNewError(
                "WebManager - Empty or null url",
                "GetGitHubReleaseRequest was sent an empty or null URL. This is not allowed."
                );
            return;
        }
        string APILink = SanitizeGitURL(originalURL);

        if(versionTag.Length > 0){
            if(versionTag == "Latest")
            {
                APILink += "/latest";
            } else
            {
                APILink += "/tags/" + versionTag;
            }
        }


        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(APILink),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");

        try
        {
            HttpResponseMessage response = client.SendAsync(request).Result;

            if (!response.IsSuccessStatusCode)
            {
                if(versionTag == "Latest")
                {
                    // This is taken care off somewhere else, no need to error out
                    onComplete?.Invoke(null);
                    return;
                }
                ErrorManager.ThrowError(
                    "WebManager - Failed to contact github",
                    "The HTTP request asking for the release list failed with status code : " + response.StatusCode
                    );
                onComplete?.Invoke(null);
                return;
            }

            onComplete?.Invoke(response);
        }
        catch (Exception ex )
        {
            ErrorManager.ThrowError(
                    "WebManager - HTTP request raised an exception",
                    "The HTTP request asking for the release list raised the following exception : " + ex.Message
                    );
            onComplete?.Invoke(null);
        }
    }

    public static string SanitizeGitURL(string rawURL)
    {
        if (!IsValidGitURL(rawURL))
        {
            ErrorManager.ThrowError(
                "WebManager - Invalid github url",
                "Selected url " + rawURL + " doesn't fit expected format for a github url. Please use the url of the releases page instead."
                );
            return "";
        }

        try
        {
            Uri uri = new Uri(rawURL);
            string[] segments = uri.AbsolutePath.Trim('/').Split('/');

            if (segments.Length >= 2)
            {
                return $"https://github.com/{segments[0]}/{segments[1]}".Replace("https://github.com/", "https://api.github.com/repos/") + "/releases";
            }
            else
            {
                ErrorManager.ThrowError(
                "WebManager - Incomplete github url",
                "Selected url " + rawURL + " doesn't fit expected format for a github url, it should be at least https://github.com/username/projectName"
                );
                return "";
            }
        }
        catch (Exception ex)
        {
            ErrorManager.ThrowError(
                "WebManager - Failed to sanitize URL",
                "Trying to sanitize provided URL raised the following exception : " + ex.Message
                );
            return "";
        }
    }

    public static async Task<string> GetLatestVersion(string url)
    {
        if (url == null)
        {
            return null;
        }
        HttpResponseMessage response = null;
        await GetGitHubReleaseRequest(url, "Latest", result =>
        {
            response = result;
        });

        if (response != null)
        {
            string jsonString = await response.Content.ReadAsStringAsync();
            JObject latest = JObject.Parse(jsonString);
            string tag = latest["tag_name"]?.ToString();

            return tag;
        }
        await GetGitHubReleaseRequest(url, "", result =>
        {
            response = result;
        });

        var json = await response.Content.ReadAsStringAsync();

        JArray releases = JArray.Parse(json);

        var latestOfficial = releases
            .OrderByDescending(r => DateTime.Parse(r["published_at"]?.ToString() ?? "1970-01-01"))
            .FirstOrDefault();

        if (latestOfficial != null)
        {
            return latestOfficial["tag_name"]?.ToString();
        }
        else
        {
            ErrorManager.AddNewError(
                "WebManager - Couldn't get latest release",
                "Something stopped YAAL from getting the latest release for this customLauncher. Please check other errors for more information."
                );
            return null;
        }

    }

    public static async Task<List<string>> GetVersions(string url)
    {
        List<string> output = new List<string>();
        if (url == null)
        {
            return null;
        }
        HttpResponseMessage response = null;


        await GetGitHubReleaseRequest(url, "", result =>
        {
            response = result;
        });

        if (response == null)
        {
            ErrorManager.AddNewError(
                "WebManager - HTTP request got no response",
                "While trying to get the available versions from " + url + " we got no response whatsoever"
                );
            return null;
        }

        string jsonString = await response.Content.ReadAsStringAsync();
        JArray releases = JArray.Parse(jsonString);

        foreach (var item in releases)
        {
            output.Add(item["tag_name"]?.ToString());
        }

        return output;
    }

    public static async Task GetVersions(string url, CLM clMaker){
        List<string> options = await GetVersions(url);
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            // TODO : this will need to be on games, not on CLM
            //clMaker.SetGitVersions(options);
        });
    }

    public static bool IsValidGitURL(string url)
    {
        return (url != null && IsValidURL(url) && url.Contains("github.com"));
    }

    public static async Task<bool> DownloadUpdatedApworld(Cache_Game customLauncher, string targetVersion){

        HttpResponseMessage response = null;

        await GetGitHubReleaseRequest(customLauncher.GetSetting(githubURL), targetVersion, result =>
        {
            response = result;
        });

        if (response == null)
        {
            if (string.IsNullOrEmpty(targetVersion))
            {
                ErrorManager.ThrowError(
                    "WebManager - Download request got no response",
                    "While trying to download the latest version from " + customLauncher.GetSetting(githubURL) + " we got no response whatsoever"
                    );
            } else
            {
                ErrorManager.ThrowError(
                    "WebManager - Download request got no response",
                    "While trying to download target version " + targetVersion + " from " + customLauncher.GetSetting(githubURL) + " we got no response whatsoever"
                    );
            }
                
            return false;
        }

        JToken releaseJson;

        if (string.IsNullOrEmpty(targetVersion))
        {
            // This is a list — find the latest non-draft
            string jsonString = await response.Content.ReadAsStringAsync();
            JArray releases = JArray.Parse(jsonString);
            releaseJson = releases.FirstOrDefault(r => r["draft"]?.ToObject<bool>() == false);
        }
        else
        {
            Trace.WriteLine(response);
            string jsonString = await response.Content.ReadAsStringAsync();
            // Direct tag-based result — a single object
            releaseJson = JObject.Parse(jsonString);
        }
        

        if (releaseJson == null)
        {
            ErrorManager.ThrowError(
                "WebManager - Couldn't find target release",
                "Couldn't access requested release from " + customLauncher.GetSetting(githubURL)
                );
            return false;
        }


        string version = releaseJson["tag_name"]?.ToString() ?? "UnknownVersion";
        JArray assets = (JArray)releaseJson["assets"];

        if (assets == null || assets.Count == 0)
        {
            ErrorManager.ThrowError(
                "WebManager - Release is empty",
                "Requested version doesn't appear to contain any file, something probably went very wrong."
                );
            return false;
        }

        string filters = customLauncher.GetSetting(LauncherSettings.filters);
        bool ignoreExtensions = (filters == "All" || filters == "");

        string[] processedFilters = filters
            .Split(',')
            .Select(e => e.Trim().ToLower())
            .Where(e => !string.IsNullOrEmpty(e))
            .ToArray();

        string gameName = customLauncher.GetSetting(launcherName);
        string folder = Path.Combine(IOManager.GetSaveLocation(ManagedApworlds), gameName, version);
        // ManagedApworlds > launcher name > version

        List<string> downloadedFilesURL = new List<string>();
        List<string> downloadedFilesPath = new List<string>();

        bool hasDownloadedAnything = false;

        foreach (JToken asset in assets)
        {
            string name = asset["name"]?.ToString();
            string downloadUrl = asset["browser_download_url"]?.ToString();

            string[] extensions = processedFilters.Where(f => f.StartsWith('.')).ToArray();
            string[] filenames = processedFilters.Where(f => !f.StartsWith('.')).ToArray();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(downloadUrl))
            {
                continue;
            }

            if (ignoreExtensions || extensions.Any(ext => name.ToLower().EndsWith(ext) || filenames.Any(file => name.ToLower().Contains(file))))
            {
                string savePath = Path.Combine(folder, name);
                if (!IOManager.HasThisBeenAlreadyDownloaded(gameName, version, downloadUrl))
                {
                    if(await DownloadFile(downloadUrl, savePath))
                    {
                        downloadedFilesURL.Add(downloadUrl);
                        downloadedFilesPath.Add(savePath);
                        hasDownloadedAnything = true;
                    } else
                    {
                        return false;
                    }
                } else {
                    Trace.WriteLine("Aborted download, file already downloaded : " + name);
                }

            }
        }

        if (hasDownloadedAnything)
        {
            IOManager.AddDownloadedFilesToVersion(gameName, version, downloadedFilesURL, downloadedFilesPath);
        }

        return true;
    }
}

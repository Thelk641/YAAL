using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YAAL;
using static System.Net.WebRequestMethods;

public static partial class WebManager
{
    public static async Task<Cache_Room> ParseRoomURL(string roomURL)
    {
        Cache_Room output = new Cache_Room();
        output.URL = roomURL;
        if (roomURL == "localhost")
        {
            output.address = "localhost";
            output.port = "38281";
            return output;
        }

        output.address = "archipelago.gg";

        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(roomURL),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");


        try
        {
            HttpResponseMessage response = await client.SendAsync(request);
            string html = await response.Content.ReadAsStringAsync();
            string[] lines = html.Split('\n');
            int slotNumber = 1;
            int offset = -1;
            Cache_RoomSlot slot = new Cache_RoomSlot();

            foreach (var item in lines)
            {
                string cleaned = item.Trim();
                if(cleaned == "")
                {
                    continue;
                }

                if(cleaned.Contains("This room has a <a href=\""))
                {
                    string[] splitTracker = cleaned.Split('\"');
                }

                if(cleaned == "<td>" +  slotNumber + "</td>")
                {
                    offset = 7;
                }

                switch (offset)
                {
                    case 6:
                        slot.slotName = ParseSlotNameLine(cleaned);
                        offset--;
                        break;
                    case 5:
                        slot.gameName = ParseGameNameLine(cleaned);
                        offset--;
                        break;
                    case 3:
                        slot.patchURL = ParsePatchLine(cleaned);
                        if(slot.patchURL == "")
                        {
                            // If there's no patch, it has one less line
                            offset -= 2;
                        } else
                        {
                            offset--;
                        }
                        break;
                    case 0:
                        slot.trackerURL = ParseTrackerLine(cleaned);
                        output.slots[slot.slotName] = slot;
                        if(output.trackerPageURL == "")
                        {
                            output.trackerPageURL = slot.trackerURL.TrimEnd('1').TrimEnd('/').TrimEnd('0').TrimEnd('/');
                        }
                        slot = new Cache_RoomSlot();
                        offset--;
                        slotNumber++;
                        break;
                    case -1:
                        break;
                    default:
                        offset--;
                        break;
                }
            }
        }
        catch (Exception e)
        {
            ErrorManager.ThrowError(
                "WebManager_Room - Exception while trying to get room",
                "Trying to parse URL " + roomURL + " triggered the following exception : " + e.Message
                );
            return new Cache_Room();
        }

        output = await GetRoomPort(output);
        output = await GetCheeseTrackerURL(output);

        return output;
    }

    private static string ParseSlotNameLine(string line)
    {
        string cleaned = line.Replace("</a></td>", "");
        string[] split = cleaned.Split('>');
        return WebUtility.HtmlDecode(split.Last<string>());
    }

    private static string ParseGameNameLine(string line)
    {
        return WebUtility.HtmlDecode(line.Replace("<td>", "").Replace("</td>", "").Trim());
    }

    private static string ParseTrackerLine(string line)
    {
        return WebUtility.HtmlDecode(ParseLine(line, "/tracker/"));
    }

    private static string ParsePatchLine(string line)
    {
        if(line == "No file to download for this game.")
        {
            return "";
        }

        return WebUtility.HtmlDecode(ParseLine(line, "/dl_patch"));
    }

    private static string ParseLine(string line, string pattern)
    {
        string[] splitLine = line.Split('\"');
        foreach (var item in splitLine)
        {
            if (item.Contains(pattern))
            {
                return "https://archipelago.gg" + item;
            }
        }

        return "";
    }

    public static async Task<Cache_ItemTracker> ParseTrackerItems(string trackerURL)
    {
        if (!IsValidURL(trackerURL))
        {
            return new Cache_ItemTracker();
        }

        Cache_ItemTracker output = new Cache_ItemTracker();
        Dictionary<string, string> parsed = new Dictionary<string, string>();

        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(trackerURL),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");


        try
        {
            HttpResponseMessage response = await client.SendAsync(request);
            string html = await response.Content.ReadAsStringAsync();
            string[] lines = html.Split('\n');

            int i = 0;

            foreach (var item in lines)
            {
                if (item.Contains("Switch To Generic Tracker"))
                {
                    string trueURL = trackerURL.Replace("tracker", "generic_tracker");
                    return await ParseTrackerItems(trueURL);
                }

                if (item.Contains("received-table"))
                {
                    break;
                }
                ++i;
            }

            i += 12;

            for (int j = i; j < lines.Length; j += 5)
            {
                string cleaned = lines[j].Trim();
                if (cleaned.StartsWith("<td>"))
                {
                    string itemName = cleaned.Replace("<td>", "").Replace("</td>", "");
                    string itemNbr = lines[j + 2].Trim().Replace("<td>", "").Replace("</td>", "");
                    parsed[itemNbr] = itemName;
                } else
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            ErrorManager.ThrowError(
                "WebManager_Room - Exception while trying to get items from tracker",
                "Trying to get items from URL " + trackerURL + " triggered the following exception : " + e.Message
                );
            return new Cache_ItemTracker();
        }

        List<int> numbers = new List<int>();
        foreach (var item in parsed)
        {
            if(int.TryParse(item.Key, out int trueNbr))
            {
                numbers.Add(trueNbr);
            }
        }
        numbers.Sort();
        List<int> invertedNumbers = new List<int>();


        for (int i = numbers.Count - 1; i > -1; i--)
        {
            invertedNumbers.Add(numbers[i]);
        }

        foreach (var item in invertedNumbers)
        {
            output.items.Add(item + " - " + parsed[item.ToString()]);
        }

        output.trackerURL = trackerURL;
        return output;
    }

    public static async Task<Cache_Room> GetCheeseTrackerURL(Cache_Room output)
    {
        string trackerURL = output.trackerPageURL;
        string baseCheese = "https://cheesetrackers.theincrediblewheelofchee.se/tracker/";

        var payload = new
        {
            url = output.trackerPageURL
        };



        string jsonPayload = JsonSerializer.Serialize(payload);
        HttpRequestMessage postRequest = new HttpRequestMessage(HttpMethod.Post, baseCheese);
        postRequest.Headers.Add("User-Agent", "YAAL");
        postRequest.Headers.Referrer = new Uri("https://cheesetrackers.theincrediblewheelofchee.se/");
        postRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //HttpResponseMessage postResponse = await client.SendAsync(postRequest);
        var postResponse = await client.PostAsync(
                            "https://cheesetrackers.theincrediblewheelofchee.se/api/tracker",
                            new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                            );

        if (!postResponse.IsSuccessStatusCode)
        {
            return output;
        }
        string postResponseText = await postResponse.Content.ReadAsStringAsync();

        using JsonDocument doc = JsonDocument.Parse(postResponseText);
        string cheeseID = doc.RootElement.GetProperty("tracker_id").GetString();

        output.cheeseTrackerURL = baseCheese + cheeseID;

        return output;
    }

    public static async Task<Cache_Room> GetRoomPort(Cache_Room output)
    {
        if (output.URL == "localhost")
        {
            output.port = "38281";
            return output;
        }

        string[] split = output.URL.Split('/');
        string APILink = "https://archipelago.gg/api/room_status/" + split.Last<string>();

        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(APILink),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");

        try
        {
            HttpResponseMessage response = client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                Cache_RoomStatus status = JsonSerializer.Deserialize<Cache_RoomStatus>(jsonString);
                Trace.WriteLine("port : " + status.last_port.ToString());
                output.port = status.last_port.ToString();
            }
        }
        catch (Exception e)
        {
            Trace.WriteLine("Exception while trying to get port : " + e.Message);
        }

        return output;
    }

    public static async Task<string> DownloadPatch(string asyncName, string slotName, string URL, bool replace = false)
    {
        string dir = IOManager.GetSlotDirectory(asyncName, slotName);

        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(URL),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");


        try
        {
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode) {
                string fullName = response.Content.Headers.ContentDisposition.ToString();
                if (!fullName.Contains("filename="))
                {
                    ErrorManager.ThrowError(
                        "WebManager_Room - Failed to parse patch name",
                        "When trying to get the patch name, the headers contained this : " + response.Content.Headers.ContentDisposition.ToString() + " which doesn't contain filename= as expected."
                        );
                    return "";
                }
                string[] split = fullName.Split("filename=");
                string fullPath = Path.Combine(dir, split.Last<string>());

                var downloadStream = await client.GetStreamAsync(URL);
                FileStream fileStream;
                if (replace)
                {
                    fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite);
                } else
                {
                    fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                await downloadStream.CopyToAsync(fileStream);
                fileStream.Close();
                return fullPath;
            } else
            {
                ErrorManager.ThrowError(
                    "WebManager_Room - Couldn't reach URL",
                    "Failed to reach URL " + URL
                    );
            }
        } catch (Exception e)
        {
            ErrorManager.AddNewError(
                "WebManager_Room - Downloading patch raised an exception",
                "While trying to download patch from " + URL + " the following exception was raised : " + e.Message
                );
        }

        return "";
    }
}
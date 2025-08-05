using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            output.IP = "localhost";
            output.port = "38281";
            return output;
        }

        output.IP = "archipelago.gg";

        Debug.WriteLine(roomURL);

        HttpRequestMessage request = new HttpRequestMessage()
        {
            RequestUri = new Uri(roomURL),
            Method = HttpMethod.Get,
        };

        request.Headers.Add("User-Agent", "YAAL");


        try
        {
            HttpResponseMessage response = client.SendAsync(request).Result;
            Debug.WriteLine(response.IsSuccessStatusCode);
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
            Debug.WriteLine("Exception while trying to get tracker : " + e.Message);
        }

        output = await GetRoomPort(output);
        output = await GetCheeseTrackerURL(output);

        return output;
    }

    private static string ParseSlotNameLine(string line)
    {
        string cleaned = line.Replace("</a></td>", "");
        string[] split = cleaned.Split('>');
        return split.Last<string>();
    }

    private static string ParseGameNameLine(string line)
    {
        return line.Replace("<td>", "").Replace("</td>", "").Trim();
    }

    private static string ParseTrackerLine(string line)
    {
        return ParseLine(line, "/tracker/");
    }

    private static string ParsePatchLine(string line)
    {
        if(line == "No file to download for this game.")
        {
            return "";
        }

        return ParseLine(line, "/dl_patch");
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

    public static async Task<Cache_Room> GetCheeseTrackerURL(Cache_Room output)
    {
        string trackerURL = output.trackerPageURL;
        string baseCheese = "https://cheesetrackers.theincrediblewheelofchee.se/api/tracker";

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

            if (!response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                Cache_RoomStatus status = JsonSerializer.Deserialize<Cache_RoomStatus>(jsonString);
                output.port = status.last_port.ToString();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception while trying to get port : " + e.Message);
        }

        return output;
    }
}
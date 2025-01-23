using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF.Sample
{
    public static class FunctionCallHelper
    {
        public static JObject HandleWeatherFunctionCall(FunctionCallArgument argument)
        {
            JObject weatherResult = new JObject();
            try
            {
                var name = argument.Name;
                var arguments = argument.Arguments;
                if (!string.IsNullOrEmpty(arguments))
                {

                    var functionCallArgs = JObject.Parse(arguments);
                    var city = functionCallArgs["city"]?.ToString();
                    if (!string.IsNullOrEmpty(city))
                    {
                        weatherResult = GetWeatherFake(city);
                    }
                    else
                    {
                        Console.WriteLine("City not provided for get_weather function.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid arguments.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing function call arguments: {ex.Message}");
            }

            return weatherResult;
        }

        public static JObject HandleNotepadFunctionCall(FunctionCallArgument argument)
        {
            JObject rtn = new JObject();
            try
            {
                var name = argument.Name;
                var callId = argument.CallId;
                var arguments = argument.Arguments;
                if (!string.IsNullOrEmpty(arguments))
                {
                    var functionCallArgs = JObject.Parse(arguments);
                    var content = functionCallArgs["content"]?.ToString();
                    var date = functionCallArgs["date"]?.ToString();
                    if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(date))
                    {
                        WriteToNotepad(date, content);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid arguments.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing function call arguments: {ex.Message}");
            }
            return rtn;
        }

        private static JObject GetWeatherFake(string city)
        {
            var weatherResponse = new JObject
            {
                 { "City", city },
                 { "Temperature", "30°C" }
             };

            return weatherResponse;
        }

        private static void WriteToNotepad(string date, string content)
        {
            try
            {
                string filePath = System.IO.Path.Combine(Environment.CurrentDirectory, "temp.txt");

                // Write the date and content to a text file
                File.AppendAllText(filePath, $"Date: {date}\nContent: {content}\n\n");

                // Open the text file in Notepad
                Process.Start("notepad.exe", filePath);
                Console.WriteLine("Content written to Notepad.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to Notepad: {ex.Message}");
            }
        } 
    }
}

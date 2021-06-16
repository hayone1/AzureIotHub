
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class newerTest : MonoBehaviour
{
    // Start is called before the first frame update
    public static EventHandler onKeypress;
    void Start()
    {
        Main();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)){
            onKeypress?.Invoke(this, null);
            // Debug.Log("I pressed C");
        }
    }

    private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        // private  static string s_connectionString = "HostName=FinalYearHub.azure-devices.net;DeviceId=TestDevice;SharedAccessKey=HpB2T173C3WkFptslBVpJ5vk02d9Lgml8DR/uGTzKks=";
        private  static string s_connectionString = "HostName=FinalYearHub.azure-devices.net;DeviceId=MyDotnetDevice;SharedAccessKey=c/HyNUWA04EyjH5wdfpOuY4PtlCG+gI3BMuqARb7kww=";

        private static TimeSpan s_telemetryInterval = TimeSpan.FromSeconds(2); // Seconds

        private static async void Main()
        {
            Debug.Log("IoT Hub Quickstarts #1 - Simulated device.");

            // This sample accepts the device connection string as a parameter, if present
            // ValidateConnectionString(args);
            Debug.Log("is the delay here?");

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Create a handler for the direct method call
            Debug.Log("or is the delay here?");
            await Task.Run(()=>
            {
                //a long-running operation...
                s_deviceClient.SetMethodHandlerAsync("SetTelemetryInterval", SetTelemetryInterval, null);
            });
            // await s_deviceClient.SetMethodHandlerAsync("SetTelemetryInterval", SetTelemetryInterval, null);

            // Set up a condition to quit the sample
            Debug.Log("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            onKeypress += (sender, eventArgs) =>
            {
                // eventArgs.Cancel = true;
                cts.Cancel();
                Debug.Log("Exiting...");
            };

            // Run the telemetry loop
            await SendDeviceToCloudMessagesAsync(cts.Token);

            // SendDeviceToCloudMessagesAsync is designed to run until cancellation has been explicitly requested by Console.CancelKeyPress.
            // As a result, by the time the control reaches the call to close the device client, the cancellation token source would
            // have already had cancellation requested.
            // Hence, if you want to pass a cancellation token to any subsequent calls, a new token needs to be generated.
            // For device client APIs, you can also call them without a cancellation token, which will set a default
            // cancellation timeout of 4 minutes: https://github.com/Azure/azure-iot-sdk-csharp/blob/64f6e9f24371bc40ab3ec7a8b8accbfb537f0fe1/iothub/device/src/InternalClient.cs#L1922
            await s_deviceClient.CloseAsync();

            s_deviceClient.Dispose();
            Debug.Log("Device simulator finished.");
        }

        private static void ValidateConnectionString(string[] args)
        {
            if (args.Any())
            {
                try
                {
                    var cs = IotHubConnectionStringBuilder.Create(args[0]);
                    s_connectionString = cs.ToString();
                }
                catch (Exception)
                {
                    Debug.Log($"Error: Unrecognizable parameter '{args[0]}' as connection string.");
                    Environment.Exit(1);
                }
            }
            else
            {
                try
                {
                    _ = IotHubConnectionStringBuilder.Create(s_connectionString);
                }
                catch (Exception)
                {
                    Debug.Log("This sample needs a device connection string to run. Program.cs can be edited to specify it, or it can be included on the command-line as the only parameter.");
                    Environment.Exit(1);
                }
            }
        }

        // Handle the direct method call
        private static Task<MethodResponse> SetTelemetryInterval(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // Check the payload is a single integer value
            if (int.TryParse(data, out int telemetryIntervalInSeconds))
            {
                s_telemetryInterval = TimeSpan.FromSeconds(telemetryIntervalInSeconds);

                Console.ForegroundColor = ConsoleColor.Green;
                Debug.Log($"Telemetry interval set to {s_telemetryInterval}");
                Console.ResetColor();

                // Acknowlege the direct method call with a 200 success message
                string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            else
            {
                // Acknowlege the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }
        }

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new System.Random();

            while (!ct.IsCancellationRequested)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                string messageBody = Newtonsoft.Json.JsonConvert.SerializeObject(
                    new
                    {
                        temperature = currentTemperature,
                        humidity = currentHumidity,
                    });
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
                await Task.Run(()=>
            {
                //a long-running operation...
                s_deviceClient.SendEventAsync(message);
                // Debug.Log("I got here");
                Debug.Log($"{DateTime.Now} > Sending message: {messageBody}");
            });
                // await s_deviceClient.SendEventAsync(message);
                // Debug.Log($"{DateTime.Now} > Sending message: {messageBody}");

                try
                {
                    await Task.Delay(s_telemetryInterval, ct);
                }
                catch (TaskCanceledException)
                {
                    // User canceled
                    return;
                }
            }
            Debug.Log("end sending message");
        }
}


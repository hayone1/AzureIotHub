using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Microsoft.Azure.Devices.Client;
// using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Text;
// using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;



public class IotHubConnection : MonoBehaviour
{
    // Start is called before the first frame update
    // string device_id = "UnityTestDevice";
     String connectionString = "HostName=FinalYearHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=1HpoEt1u1AX95BvW9qB2O3YwIXEESMcnOCVz60d7V1k=";
    string fake;
    public static EventHandler onKeypress;
    public struct Parameters{
        public double currentTemperature;
        public double currentHumidity;
    }
    public static Parameters _parametrs = new Parameters();

   
    
    private void Start() {
    //     registryManager = RegistryManager.CreateFromConnectionString(connectionString);
    //    OnServerInitialized();
            Main();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.C)){
            onKeypress?.Invoke(this, null);
            Debug.Log("I pressed C");
        }
    }


    private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private  static string s_connectionString = "HostName=FinalYearHub.azure-devices.net;DeviceId=TestDevice;SharedAccessKey=HpB2T173C3WkFptslBVpJ5vk02d9Lgml8DR/uGTzKks=";

        private static async void Main()
        {
            Debug.Log("IoT Hub Quickstarts #1 - Simulated device.");

            // This sample accepts the device connection string as a parameter, if present
            // ValidateConnectionString(args);

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Set up a condition to quit the sample
            var cts = new CancellationTokenSource();
            onKeypress += (sender, eventArgs) =>
            {
                // eventArgs.Cancel = true;
                cts.Cancel();
                Debug.Log("Exiting...");
            };

            // Run the telemetry loop
            Debug.Log("Press control-C to exit.");
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

        private  void ValidateConnectionString(string[] args)
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

        // Async method to send simulated telemetry
        private AsyncOperation ok;
        private  static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            var rand = new System.Random();

            while (!ct.IsCancellationRequested)
            {
                _parametrs.currentTemperature = minTemperature + rand.NextDouble() * 15;
                _parametrs.currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                string messageBody = Newtonsoft.Json.JsonConvert.SerializeObject(_parametrs);
                
                // string messageBody = JsonSerializer.Serialize(
                //     new
                //     {
                //         temperature = currentTemperature,
                //         humidity = currentHumidity,
                //     });
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (_parametrs.currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message
            Debug.Log("Press control-k to exit.");
                
                await Task.Run(()=>
            {
                //a long-running operation...
                s_deviceClient.SendEventAsync(message);
                // Debug.Log("I got here");
                Debug.Log($"{DateTime.Now} > Sending message: {messageBody}");
            });

                await Task.Delay(1000);
            }
        }
}

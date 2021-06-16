using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.Devices.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class newSendTelemetry : MonoBehaviour
{
    // Start is called before the first frame update
    public static EventHandler onKeypress;

    private static DeviceClient s_deviceClient;
    private static readonly TransportType s_transportType = TransportType.Mqtt;
    // private static string deviceConnectionString = "HostName=FinalYearHub.azure-devices.net;DeviceId=TestDevice;SharedAccessKey=HpB2T173C3WkFptslBVpJ5vk02d9Lgml8DR/uGTzKks=";
    private  static string deviceConnectionString = "HostName=FinalYearHub.azure-devices.net;DeviceId=MyDotnetDevice;SharedAccessKey=c/HyNUWA04EyjH5wdfpOuY4PtlCG+gI3BMuqARb7kww=";

    private static TimeSpan s_telemetryInterval = TimeSpan.FromSeconds(2); // Seconds

    void Start()
    {
        Main();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)){
            onKeypress?.Invoke(this, null);
            Debug.Log("I pressed C");
        }
    }

    private static async void Main()
    {
        Debug.Log("IoT Hub Quickstarts #1 - Simulated device.");

            // This sample accepts the device connection string as a parameter, if present
            // ValidateConnectionString(args);
            Debug.Log("is the delay here?");

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, s_transportType);

            // Create a handler for the direct method call
            Debug.Log("or is the delay here?");

            


        Debug.Log("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            onKeypress += (sender, eventArgs) =>
            {
                // eventArgs.Cancel = true;
                cts.Cancel();
                Debug.Log("Exiting...");
            };

        await SendDeviceToCloudMessagesAsync(cts.Token);

        await s_deviceClient.CloseAsync();

            s_deviceClient.Dispose();
            Debug.Log("Device simulator finished.");
        
    }

    private static async Task SendDeviceToCloudMessagesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var telemetryDataPoint = new
        {
            deviceId = "okok",
            highTemp = 14.5,
            lowTemp = "yo"
        };

        string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
        using Message message = new Message(Encoding.ASCII.GetBytes(messageString)){
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
        };

        message.Properties.Add("temperatureAlert", (telemetryDataPoint.highTemp > 30) ? "true" : "false");


        await Task.Run(() => {s_deviceClient.SendEventAsync(message);});
        Debug.Log("I got here");

        try {
            await Task.Delay(s_telemetryInterval, ct);
        }
        catch (TaskCanceledException){
            return;
        }
        }

        // await s_deviceClient.SendEventAsync(_message);
    }

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
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ReadMessages : MonoBehaviour
{
    // Start is called before the first frame update
    private static EventHubClient eventHubClient;
    public static EventHandler onKeypress;
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
            Debug.Log("I pressed C in receive");
        }
    }

    public async void Main()
    {
        // eventHubClient = EventHubClient.CreateFromConnectionString(ReadD2cMessages.Parameters.EventHubConnectionString, ReadD2cMessages.Parameters.EventHubName);
        eventHubClient = EventHubClient.Create(ReadD2cMessages.Parameters.EventHubName);
        // eventHubClient = EventHubClient.CreateFromConnectionString()

        var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;    //get the partitions info
        if (d2cPartitions == null) {throw new ArgumentNullException(nameof(d2cPartitions));}    //throw exception if not found
        string data = "";

        using var cts = new CancellationTokenSource();
            onKeypress += (sender, eventArgs) =>
            {
                // eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

        foreach(string partition in d2cPartitions){
            
            var result = await ReceiveMessagesFromDeviceAsync(partition, cts.Token);

            data = result;
            if (data != ""){
                return;
            }
        }
    }

    private static async Task<string> ReceiveMessagesFromDeviceAsync(string partition, CancellationToken _token)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            Task<EventData> eventData = new Task<EventData>(null);
            String data = string.Empty;
            while (!(_token.IsCancellationRequested))
            {
                await Task.Run(()=>
            {
                //a long-running operation...
                // s_deviceClient.SendEventAsync(message);
                // Debug.Log("I got here");
                eventData = eventHubReceiver.ReceiveAsync();
                // Debug.Log($"{DateTime.Now} > Sending message: {messageBody}");
            });
                if (eventData == null) {continue;};

                data = Encoding.UTF8.GetString(eventData.Result.GetBytes());
                UnityEngine.Debug.Log($"Message received. Data: '{data}'");

                try
                {
                    await Task.Delay(s_telemetryInterval, _token);
                }
                catch (TaskCanceledException)
                {
                    // User canceled
                    // return;
                    return data;
                }
            }
            Debug.Log("end sending message");
            return data;
        }
}

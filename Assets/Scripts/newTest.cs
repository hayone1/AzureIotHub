using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Devices.Client;
// using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
public class newTest : MonoBehaviour
{
    // Start is called before the first frame update
    string deviceConnectionString = "HostName=FinalYearHub.azure-devices.net;DeviceId=TestDevice;SharedAccessKey=HpB2T173C3WkFptslBVpJ5vk02d9Lgml8DR/uGTzKks=";
    DeviceClient _deviceClient;
    EventHubClient eventHubClient;
    

    void Start()
    {
        // eventHubClient = EventHubClient.CreateFromConnectionString(Az)
        _deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);
        var telemetryDataPoint = new{
            deviceId = "TestDevice",
            windSpeed = 50,
            latitude = "17.483635"

        };
        var messageJson = JsonUtility.ToJson(telemetryDataPoint);
        var NewtonJson = JsonConvert.SerializeObject(telemetryDataPoint);
        Debug.Log("first Json is " + messageJson);
        Debug.Log("second Json is " + NewtonJson);

        var message = new Message(Encoding.ASCII.GetBytes(NewtonJson));
        AsyncSendEvents(message);
        

    }

    public async void AsyncSendEvents(Message _jsonMessage)
        {
            await Task.Run(()=>
            {
                //a long-running operation...
                _deviceClient.SendEventAsync(_jsonMessage);
                Debug.Log("I got here");
            });
        }

    // Update is called once per frame
    // void Update()
    // {
        
    // }
}

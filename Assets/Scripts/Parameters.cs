// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// using CommandLine;

namespace ReadD2cMessages
{
    /// <summary>
    /// Parameters for the application
    /// </summary>
    public static class Parameters
    {
        public static string IotHubSharedAccessKeyName = "service";
        public static string EventHubCompatibleEndpoint { get; set; } = "sb://ihsuprodblres055dednamespace.servicebus.windows.net/";
        public static string EventHubName { get; set; } = "iothub-ehub-finalyearh-11977231-13f5cef02d";
        public static string SharedAccessKey { get; set; } = "RZnKWLAfrdCAh2CIx/Iz6veRNt4u5VTpQ9K2lxBrQyo=";
        public static string EventHubConnectionString { get; set; } = "Endpoint=sb://ihsuprodblres055dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=1HpoEt1u1AX95BvW9qB2O3YwIXEESMcnOCVz60d7V1k=;EntityPath=iothub-ehub-finalyearh-11977231-13f5cef02d";

        public static string GetEventHubConnectionString()
        {
            return EventHubConnectionString ?? $"Endpoint={EventHubCompatibleEndpoint};SharedAccessKeyName={IotHubSharedAccessKeyName};SharedAccessKey={SharedAccessKey}";
        }
    }
}

using System;
using SharpPcap;

class Program
{
    static void Main(string[] args)
    {
        // List all devices
        var devices = CaptureDeviceList.Instance;

        if (devices.Count < 1)
        {
            Console.WriteLine("No devices found! Ensure Npcap is installed.");
            return;
        }

        Console.WriteLine("Available devices:");
        for (int i = 0; i < devices.Count; i++)
        {
            Console.WriteLine($"{i}: {devices[i].Description}");
        }

        Console.Write("Select a device to start capturing: ");
        string? input = Console.ReadLine();
        if (int.TryParse(input, out int deviceIndex) && deviceIndex >= 0 && deviceIndex < devices.Count)
        {
            var device = devices[deviceIndex];
            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);
            device.Open(); // Updated: No DeviceMode required.

            Console.WriteLine($"Listening on {device.Description}. Press Ctrl+C to stop.");
            device.StartCapture();

            Console.CancelKeyPress += (sender, e) =>
            {
                device.StopCapture();
                device.Close();
                Console.WriteLine("Capture stopped.");
            };

            while (true) ; // Keep the program running
        }
        else
        {
            Console.WriteLine("Invalid device index.");
        }
    }

    private static void OnPacketArrival(object sender, PacketCapture e)
    {
        Console.WriteLine($"Packet captured at {e.Header.Timeval.Date}");
    }
}

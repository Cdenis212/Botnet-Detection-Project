using System;
using SharpPcap;
using PacketDotNet;

class Program
{
    static void Main(string[] args)
    {
        // List all available devices
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

            // Attach event handler and open the device
            device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);
            device.Open(); // Use default promiscuous mode

            Console.WriteLine($"Listening on {device.Description}. Press Ctrl+C to stop.");
            device.StartCapture();

            // Graceful stop on Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                device.StopCapture();
                device.Close();
                Console.WriteLine("Packet capture stopped.");
            };

            // Keep the program running
            while (true) ;
        }
        else
        {
            Console.WriteLine("Invalid device index.");
        }
    }

    private static void OnPacketArrival(object sender, PacketCapture e)
    {
        var rawPacket = e.GetPacket();
        Console.WriteLine($"Captured Packet at: {e.Header.Timeval.Date}");

        try
        {
            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            if (packet.PayloadPacket is IPv4Packet ipv4Packet)
            {
                Console.WriteLine($"IPv4: {ipv4Packet.SourceAddress} -> {ipv4Packet.DestinationAddress}");
                Console.WriteLine($"Protocol: {ipv4Packet.Protocol}, Packet Size: {ipv4Packet.TotalPacketLength}");
            }
            else if (packet.PayloadPacket is IPv6Packet ipv6Packet)
            {
                Console.WriteLine($"IPv6: {ipv6Packet.SourceAddress} -> {ipv6Packet.DestinationAddress}");
            }
            else
            {
                Console.WriteLine("Non-IP Packet Detected");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing packet: {ex.Message}");
        }

        Console.WriteLine("----------------------------------------------------\n");
    }
}

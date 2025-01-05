using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

class Program
{
    private const string connectionString = "Endpoint=sb://orderprocessingnamespace.servicebus.windows.net/;SharedAccessKeyName=akall;SharedAccessKey=9bAZE59E61l/ymW48I7mVeW/EYgTzRsSS+ASbKCX8/U=;EntityPath=orderqueue";
    private const string queueName = "orderqueue";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Simulating message retries to Dead Letter Queue...");
        await ReceiveAndSimulateRetriesAsync();
    }

    static async Task ReceiveAndSimulateRetriesAsync()
    {
        var client = new ServiceBusClient(connectionString);
        var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock // Ensure message stays in the queue
        });

        try
        {
            int maxDeliveryAttempts = 3; // Matches Max Delivery Count set in the Azure Portal

            for (int attempt = 1; attempt <= maxDeliveryAttempts; attempt++)
            {
                // Fetch the same message
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));

                if (message != null)
                {
                    Console.WriteLine($"Attempt {attempt}: Received message: {message.Body.ToString()}");

                    // Simulate failure by abandoning the message
                    await receiver.AbandonMessageAsync(message);
                    Console.WriteLine($"Attempt {attempt}: Message abandoned.");
                }
                else
                {
                    Console.WriteLine("No message available to receive.");
                    break;
                }
            }

            Console.WriteLine("Message should now be moved to the Dead Letter Queue.");
        }
        finally
        {
            await receiver.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}

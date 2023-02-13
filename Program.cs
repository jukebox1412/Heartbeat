namespace Heartbeat
{
    internal class Program
    {
        private static bool DoExit;
        private static HttpClient client;
        private static CancellationTokenSource cts;

        static void Main(string[] args)
        {
            DoExit = false;
            client = new HttpClient();

            Console.WriteLine("Press any key to exit!");

            // Create the token source.
            cts = new CancellationTokenSource();

            // Pass the token to the cancelable operation.
            ThreadPool.QueueUserWorkItem(new WaitCallback(DoLoop), cts.Token);


            Console.ReadLine();
            if (!cts.IsCancellationRequested)
                cts.Cancel();

            cts.Dispose();
        }

        private static async void DoLoop(object? obj)
        {
            CancellationToken token = (CancellationToken)obj!;

            // loop forever unless cancellation token is called
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    // exit
                    Console.WriteLine($"Ending {nameof(DoLoop)} thread");
                    break;
                }

                Thread.Sleep(2000);
                await DoPing();
            }
        }

        private static async Task DoPing()
        {
            string url = "https://www.google.com";
            try
            {
                var res = await client.GetAsync(url);
                if (res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[{DateTime.Now}] Pinged {url}: {res.StatusCode}");
                }
                else
                {
                    Console.WriteLine($"[{DateTime.Now}] Pinged {url} but unsuccessful status code: {res.StatusCode}");
                    cts.Cancel();
                }
            }
            catch (Exception e)
            {
                // cancel the loop and print message
                Console.WriteLine($"[{DateTime.Now}] Unable to ping {url}, {e.Message} {e.StackTrace}");
                cts.Cancel();
            }
        }
    }
}
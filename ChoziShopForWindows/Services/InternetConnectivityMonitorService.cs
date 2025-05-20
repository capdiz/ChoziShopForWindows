using ChoziShopSharedConnectivity.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ChoziShopForWindows.Services
{
    public class InternetConnectivityMonitorService
    {      
        private readonly ILogger<InternetConnectivityMonitorService> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private NamedPipeClientStream? _namedClientPipe;
        private bool _disposed;

        public event Action<ConnectivityStatus>? StatusChanged;

        public InternetConnectivityMonitorService(ILogger<InternetConnectivityMonitorService> logger)
        {
            _logger = logger;
            Task.Run(() => StartListening(_cancellationTokenSource));
        }

        private async Task StartListening(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    using (_namedClientPipe = new NamedPipeClientStream(
                        ".",
                        "ChoziShopMessagingPipe",
                        PipeDirection.In,
                        PipeOptions.Asynchronous))
                    {

                       await _namedClientPipe.ConnectAsync(10000, cancellationTokenSource.Token);

                        var buffer = new byte[4096];
                        var memory = new Memory<byte>(buffer);

                        while (!cancellationTokenSource.IsCancellationRequested)
                        {
                            var bytesRead = await _namedClientPipe.ReadAsync(memory, cancellationTokenSource.Token);
                            if (bytesRead == 0) break;
                            Debug.WriteLine($"Reading bytes: {bytesRead}");
                            var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            var status = JsonSerializer.Deserialize<ConnectivityStatus>(json)!;
                            // Don't update UI immediately, Queue the status change
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Debug.WriteLine($"Status changed: {status}");
                                StatusChanged?.Invoke(status);
                            });
                        }

                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Listening", "Operation cancelled");
                    break;
                }
                catch (TimeoutException)
                {
                    Debug.WriteLine("Worker service unavailable. ensure it's running");
                    await Task.Delay(15000, cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WorkerService pipe connection error: {ex.Message}");
                    await Task.Delay(5000, cancellationTokenSource.Token);
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _namedClientPipe?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Diagnostics;
using System.Net;
using Xunit.Abstractions;

namespace StackExchange.Redis.Tests;

public class SocketTests : TestBase
{
    protected override string GetConfiguration() => TestConfig.Current.PrimaryServerAndPort;
    public SocketTests(ITestOutputHelper output) : base (output) { }

    [FactLongRunning]
    public void CheckForSocketLeaks()
    {
        const int count = 2000;
        for (var i = 0; i < count; i++)
        {
            using var _ = Create(clientName: "Test: " + i);
            // Intentionally just creating and disposing to leak sockets here
            // ...so we can figure out what's happening.
        }
        // Force GC before memory dump in debug below...
        CollectGarbage();

        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
    }

    [Fact]
    public void CreateSocket_ShouldNotThrowNotImplementedException()
    {
        var uriEndpoint = new UriEndPoint(new Uri("http://127.0.0.1:1234/")); // from 'Microsoft.AspNetCore.Connections' namespace
        SocketManager.CreateSocket(uriEndpoint);
        /* throws:
           -------
            System.NotImplementedException
            This property is not implemented by this class.
               at System.Net.EndPoint.get_AddressFamily()
               at StackExchange.Redis.SocketManager.CreateSocket(EndPoint endpoint) in C:\code\forked\StackExchange.Redis\src\StackExchange.Redis\SocketManager.cs:line 216
               at StackExchange.Redis.Tests.SocketTests.CreateSocket_ShouldNotThrowNotImplementedException() in C:\code\forked\StackExchange.Redis\tests\StackExchange.Redis.Tests\SocketTests.cs:line 36
        */
    }

    /// <summary>
    /// An <see cref="EndPoint"/> defined by a <see cref="System.Uri"/>.
    /// </summary>
    private class UriEndPoint : EndPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriEndPoint"/> class.
        /// </summary>
        /// <param name="uri">The <see cref="System.Uri"/> defining the <see cref="EndPoint"/>.</param>
        public UriEndPoint(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        /// <summary>
        /// The <see cref="System.Uri"/> defining the <see cref="EndPoint"/>.
        /// </summary>
        public Uri Uri { get; }

        public override string ToString() => Uri.ToString();
    }
}

using System;
using System.Buffers;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CodeElements.NetworkCall;
using CodeElements.NetworkCall.NetSerializer;
using Maze.Administration.Library.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace Maze.Administration.ControllerExtensions
{
    public class CallTransmissionChannel<TInterface> : ChannelBase
    {
        private readonly IServiceProvider _serverProvider;
        protected NetworkCallClient<TInterface> Client;

        public CallTransmissionChannel(IServiceProvider serverProvider)
        {
            _serverProvider = serverProvider;
        }

        public TInterface Interface => Client.Interface;

        public void SuspendSubscribing()
        {
            Client.SuspendSubscribing();
        }

        public void ResumeSubscribing()
        {
            Client.ResumeSubscribing();
        }

        protected override void ReceiveData(byte[] buffer, int offset, int count)
        {
            Client.ReceiveData(buffer, offset);
        }

        protected override void Initialize(HttpResponseMessage responseMessage)
        {
            var serializer = GetSerializer(responseMessage.Content.Headers);
            Client = new NetworkCallClient<TInterface>(serializer, ArrayPool<byte>.Shared)
            {
                SendData = SendData, CustomOffset = RequiredOffset, WaitTimeout = TimeSpan.Zero
            };
        }

        private Task SendData(BufferSegment data)
        {
            return Send(data.Buffer, data.Offset, data.Length, hasOffset: true);
        }

        protected virtual INetworkSerializer GetSerializer(HttpContentHeaders responseHeaders)
        {
            var serializer = responseHeaders.ContentEncoding.FirstOrDefault();
            if (serializer != null)
            {
                switch (serializer)
                {
                    case var name when name.Equals("netserializer", StringComparison.OrdinalIgnoreCase):
                        return _serverProvider.GetRequiredService<NetSerializerNetworkSerializer>();
                }
            }

            return _serverProvider.GetRequiredService<NetSerializerNetworkSerializer>(); //default
        }

        protected override void InternalDispose()
        {
            Client?.Dispose();
        }
    }
}
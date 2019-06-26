using System;
using System.Collections.Generic;
using System.Text;

namespace Feign
{
    public interface IFeignClientPipeline
    {
        bool Enabled { get; set; }
        event EventHandler<BuildingRequestEventArgs> BuildingRequest;
        event EventHandler<SendingRequestEventArgs> SendingRequest;
        event EventHandler<CancelRequestEventArgs> CancelRequest;
        event EventHandler<ErrorRequestEventArgs> ErrorRequest;
        event EventHandler<ReceivingResponseEventArgs> ReceivingResponse;
        event EventHandler<InitializingEventArgs> Initializing;
        event EventHandler<DisposingEventArgs> Disposing;
        event EventHandler<FallbackRequestEventArgs> FallbackRequest;
    }
}

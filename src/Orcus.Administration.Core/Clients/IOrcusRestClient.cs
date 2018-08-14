﻿using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Orcus.Administration.Core.Clients
{
    public interface IOrcusRestClient : IRestClient, IDisposable
    {
        string Username { get; }
        HubConnection HubConnection { get; }
    }
}
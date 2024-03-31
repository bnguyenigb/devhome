﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.Messaging.Messages;
using DevHome.Common.Environments.Models;

namespace DevHome.SetupFlow.Models.Environments;

public class CreationProviderChangedMessage : ValueChangedMessage<ComputeSystemProviderDetails>
{
    public CreationProviderChangedMessage(ComputeSystemProviderDetails value)
        : base(value)
    {
    }
}

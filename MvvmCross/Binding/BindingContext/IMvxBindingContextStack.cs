﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

namespace MvvmCross.Binding.BindingContext
{
    public interface IMvxBindingContextStack<TContext>
    {
        TContext Current { get; }

        void Push(TContext context);

        TContext Pop();
    }
}

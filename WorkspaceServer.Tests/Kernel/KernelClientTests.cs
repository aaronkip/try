﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Newtonsoft.Json;
using WorkspaceServer.Kernel;
using Xunit;

namespace WorkspaceServer.Tests.Kernel
{
    public class KernelClientTests
    {
        [Fact]
        public async Task Kernel_can_be_interacted_using_kernel_client()
        {
            var kernel = new CompositeKernel
            {
                new CSharpKernel()
            };

            var input = new MemoryStream();
            var writer = new StreamWriter(input, Encoding.UTF8);
            writer.WriteMessage(new SubmitCode(@"var x = 
123;"));
            writer.WriteMessage(new SubmitCode("x"));
            writer.WriteMessage(new Quit());

            input.Position = 0;

            var output = new MemoryStream();

            var streamKernel = new KernelStreamClient(kernel,
                                                      new StreamReader(input),
                                                      new StreamWriter(output));

            var task = streamKernel.Start();
            await task;

            output.Position = 0;
            var reader = new StreamReader(output, Encoding.UTF8);

            var text = reader.ReadToEnd();
            var events = text.Split(Environment.NewLine)
                             .Select(JsonConvert.DeserializeObject<StreamKernelEvent>);

            events.Should().Contain(e => e.EventType == "ValueProduced");
        }
    }
}
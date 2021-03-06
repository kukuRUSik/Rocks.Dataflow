﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Rocks.Dataflow.Fluent;
using Rocks.Dataflow.Tests.FluentTests.Infrastructure;

namespace Rocks.Dataflow.Tests.FluentTests.CompositionTests
{
    public class FluentAsyncTests
    {
        [Fact]
        public async Task TransformAction_CorrectlyBuild()
        {
            // arrange
            var result = new ConcurrentBag<string>();

            var sut = DataflowFluent
                .ReceiveDataOfType<int>()
                .TransformAsync(async x =>
                                      {
                                          await Task.Yield();
                                          return x.ToString(CultureInfo.InvariantCulture);
                                      })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo("1", "2", "3");
        }


        [Fact]
        public async Task TransformManyAction_CorrectlyBuild()
        {
            // arrange
            var result = new ConcurrentBag<string>();

            var sut = DataflowFluent
                .ReceiveDataOfType<TestDataflowContext<int>>()
                .TransformManyAsync<TestDataflowContext<string>>(async x =>
                                                                       {
                                                                           await Task.Yield();
                                                                           return new[] { new TestDataflowContext<string> { Data = x.ToString() } };
                                                                       })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x.Data);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }.CreateDataflowContexts()).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo("1", "2", "3");
        }


        [Fact]
        public async Task TransformTransformAction_CorrectlyBuild()
        {
            // arrange
            var result = new ConcurrentBag<int>();

            var sut = DataflowFluent
                .ReceiveDataOfType<int>()
                .TransformAsync(async x =>
                                      {
                                          await Task.Yield();
                                          return x.ToString(CultureInfo.InvariantCulture);
                                      })
                .TransformAsync(async s =>
                                      {
                                          await Task.Yield();
                                          return int.Parse(s);
                                      })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo(1, 2, 3);
        }


        [Fact]
        public async Task TransformTransformManyAction_CorrectlyBuild()
        {
            // arrange
            var result = new ConcurrentBag<char>();

            var sut = DataflowFluent
                .ReceiveDataOfType<int>()
                .TransformAsync(async x =>
                                      {
                                          await Task.Yield();
                                          return x.ToString(CultureInfo.InvariantCulture);
                                      })
                .TransformManyAsync<char>(async s =>
                                                {
                                                    await Task.Yield();
                                                    return s.ToCharArray();
                                                })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo('1', '2', '3');
        }


        [Fact]
        public async Task TransformAction_WithNullReturn_CorrectlyExecuted()
        {
            // arrange
            var result = new ConcurrentBag<int>();

            var sut = DataflowFluent
                .ReceiveDataOfType<TestDataflowContext<int>>()
                .TransformAsync(async x =>
                                      {
                                          await Task.Yield();

                                          if (x.Data == 2)
                                              return null;

                                          return x;
                                      })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x.Data);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }.CreateDataflowContexts()).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo(1, 3);
        }


        [Fact]
        public async Task TransformActionSync_WithNullReturn_CorrectlyExecuted()
        {
            // arrange
            var result = new ConcurrentBag<int>();

            var sut = DataflowFluent
                .ReceiveDataOfType<TestDataflowContext<int>>()
                .TransformAsync(async x =>
                                      {
                                          await Task.Yield();

                                          if (x.Data == 2)
                                              return null;

                                          return x;
                                      })
                .WithBoundedCapacity(100)
                .Action(x => result.Add(x.Data))
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }.CreateDataflowContexts()).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo(1, 3);
        }


        [Fact]
        public async Task ProcessAction_CorrectlyBuild()
        {
            // arrange
            var result = new ConcurrentBag<int>();

            var sut = DataflowFluent
                .ReceiveDataOfType<int>()
                .ProcessAsync(async x =>
                                    {
                                        await Task.Yield();
                                    })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo(1, 2, 3);
        }


        [Fact]
        public async Task ProcessProcessAction_CorrectlyBuild()
        {
            // arrange
            var result = new ConcurrentBag<int>();

            var sut = DataflowFluent
                .ReceiveDataOfType<int>()
                .ProcessAsync(async x =>
                                    {
                                        await Task.Yield();
                                    })
                .ProcessAsync(async x =>
                                    {
                                        await Task.Yield();
                                    })
                .WithBoundedCapacity(100)
                .ActionAsync(async x =>
                                   {
                                       await Task.Yield();
                                       result.Add(x);
                                   })
                .WithMaxDegreeOfParallelism();


            // act
            var dataflow = sut.CreateDataflow();
            await dataflow.ProcessAsync(new[] { 1, 2, 3 }).ConfigureAwait(false);


            // assert
            result.Should().BeEquivalentTo(1, 2, 3);
        }


        [Fact]
        public void ActionActionAsync_Throws()
        {
            // arrange

            // act
            Action action = () => DataflowFluent
                                      .ReceiveDataOfType<int>()
                                      .ActionAsync(async x =>
                                                         {
                                                             await Task.Yield();
                                                         })
                                      .ActionAsync(async x =>
                                                         {
                                                             await Task.Yield();
                                                         })
                                      .CreateDataflow();


            // assert
            action.Should().Throw<InvalidOperationException>();
        }


        [Fact]
        public void ProcessAction_FirstWaitsForTheSecond_DataflowEndedOnlyAfterBothProcessed()
        {
            // arrange
            var second_item_finished = new SemaphoreSlim(0, 1);
            var completed_items = new ConcurrentBag<string>();

            var sut = DataflowFluent
                .ReceiveDataOfType<string>()
                .ProcessAsync(async x =>
                                    {
                                        await Task.Yield();

                                        if (x == "1")
                                            await second_item_finished.WaitAsync().ConfigureAwait(false);
                                    })
                .Action(x =>
                        {
                            completed_items.Add(x);

                            if (x == "2")
                                second_item_finished.Release();
                        })
                .CreateDataflow();


            // act
            if (!sut.ProcessAsync(new[] { "1", "2" }).Wait(2000))
                throw new TimeoutException();


            // assert
            completed_items.Should().Equal("2", "1");
        }
    }
}
﻿using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rocks.Dataflow.Fluent;
using Rocks.Dataflow.Tests.FluentTests.Infrastructure;

namespace Rocks.Dataflow.Tests.FluentTests.CompositionTests
{
	[TestClass]
	public class FluentAsyncTests
	{
		[TestMethod]
		public async Task TransformAction_CorrectlyBuilded ()
		{
			// arrange
			var result = new ConcurrentBag<string> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<int> ()
				.TransformAsync (async x =>
				{
					await Task.Yield ();
					return x.ToString (CultureInfo.InvariantCulture);
				})
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 });


			// assert
			result.Should ().BeEquivalentTo ("1", "2", "3");
		}


		[TestMethod]
		public async Task TransformManyAction_CorrectlyBuilded ()
		{
			// arrange
			var result = new ConcurrentBag<string> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<TestDataflowContext<int>> ()
				.TransformManyAsync<TestDataflowContext<string>> (async x =>
				{
					await Task.Yield ();
					return new[] { new TestDataflowContext<string> { Data = x.ToString () } };
				})
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x.Data);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 }.CreateDataflowContexts ());


			// assert
			result.Should ().BeEquivalentTo ("1", "2", "3");
		}


		[TestMethod]
		public async Task TransformTransformAction_CorrectlyBuilded ()
		{
			// arrange
			var result = new ConcurrentBag<int> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<int> ()
				.TransformAsync (async x =>
				{
					await Task.Yield ();
					return x.ToString (CultureInfo.InvariantCulture);
				})
				.TransformAsync (async s =>
				{
					await Task.Yield ();
					return int.Parse (s);
				})
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 });


			// assert
			result.Should ().BeEquivalentTo (1, 2, 3);
		}


		[TestMethod]
		public async Task TransformTransformManyAction_CorrectlyBuilded ()
		{
			// arrange
			var result = new ConcurrentBag<char> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<int> ()
				.TransformAsync (async x =>
				{
					await Task.Yield ();
					return x.ToString (CultureInfo.InvariantCulture);
				})
				.TransformManyAsync<char> (async s =>
				{
					await Task.Yield ();
					return s.ToCharArray ();
				})
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 });


			// assert
			result.Should ().BeEquivalentTo ('1', '2', '3');
		}


		[TestMethod]
		public async Task TransformAction_WithNullReturn_CorrectlyExecuted ()
		{
			// arrange
			var result = new ConcurrentBag<int> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<TestDataflowContext<int>> ()
				.TransformAsync (async x =>
				{
					await Task.Yield ();

					if (x.Data == 2)
						return null;

					return x;
				})
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x.Data);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 }.CreateDataflowContexts ());


			// assert
			result.Should ().BeEquivalentTo (1, 3);
		}


		[TestMethod]
		public async Task TransformActionSync_WithNullReturn_CorrectlyExecuted ()
		{
			// arrange
			var result = new ConcurrentBag<int> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<TestDataflowContext<int>> ()
				.TransformAsync (async x =>
				{
					await Task.Yield ();

					if (x.Data == 2)
						return null;

					return x;
				})
				.WithBoundedCapacity (100)
				.Action (x => result.Add (x.Data))
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 }.CreateDataflowContexts ());


			// assert
			result.Should ().BeEquivalentTo (1, 3);
		}


		[TestMethod]
		public async Task ProcessAction_CorrectlyBuilded ()
		{
			// arrange
			var result = new ConcurrentBag<int> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<int> ()
				.ProcessAsync (async x => { await Task.Yield (); })
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 });


			// assert
			result.Should ().BeEquivalentTo (1, 2, 3);
		}


		[TestMethod]
		public async Task ProcessProcessAction_CorrectlyBuilded ()
		{
			// arrange
			var result = new ConcurrentBag<int> ();

			var sut = DataflowFluent
				.ReceiveDataOfType<int> ()
				.ProcessAsync (async x => { await Task.Yield (); })
				.ProcessAsync (async x => { await Task.Yield (); })
				.WithBoundedCapacity (100)
				.ActionAsync (async x =>
				{
					await Task.Yield ();
					result.Add (x);
				})
				.WithMaxDegreeOfParallelism ();


			// act
			var dataflow = sut.CreateDataflow ();
			await dataflow.Process (new[] { 1, 2, 3 });


			// assert
			result.Should ().BeEquivalentTo (1, 2, 3);
		}
	}
}
﻿// <copyright file="ParallelTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.ThreadingTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using MbUnit.Framework;
    using Threading;

    [TestFixture]
    public class ParallelForEachTests
    {
        [Test, ApartmentState(ApartmentState.MTA)]
        [Column( 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 1001)]
        public void ParallelForEachInvokesEveryItemOnceMTAOnePerCore(int count)
        {
            // ensure One-Per-Core
            ThreadQueue.Start(Environment.ProcessorCount);

            var items = new double[count];
            var pairs = new List<KeyValuePair<int, double>>();
            for (var i = 0; i < items.Length; i++)
            {
                pairs.Add(new KeyValuePair<int, double>(i, i));
            }
            Parallel.ForEach(pairs, pair => items[pair.Key] = pair.Value );

            Parallel.ForEach(pairs,
                pair =>
                {
                    items[pair.Key] = pair.Value + 1000;
                }
                );

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1000+i, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.STA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 1001)]
        public void ParallelForEachInvokesEveryItemOnceSTAOnePerCore(int count)
        {
            // ensure One-Per-Core
            ThreadQueue.Start(Environment.ProcessorCount);

            var items = new double[count];
            var pairs = new List<KeyValuePair<int, double>>();
            for (var i = 0; i < items.Length; i++)
            {
                pairs.Add(new KeyValuePair<int, double>(i, i));
            }
            Parallel.ForEach(pairs, pair => items[pair.Key] = pair.Value);

            Parallel.ForEach(pairs,
                pair =>
                {
                    items[pair.Key] = pair.Value + 1000;
                }
                );

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1000 + i, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 1001)]
        public void ParallelForEachInvokesEveryItemOnceMTATwoPerCore(int count)
        {
            // ensure Two-Per-Core
            ThreadQueue.Start(2 * Environment.ProcessorCount);

            var items = new double[count];
            var pairs = new List<KeyValuePair<int, double>>();
            for (var i = 0; i < items.Length; i++)
            {
                pairs.Add(new KeyValuePair<int, double>(i, i));
            }
            Parallel.ForEach(pairs, pair => items[pair.Key] = pair.Value);

            Parallel.ForEach(pairs,
                pair =>
                {
                    items[pair.Key] = pair.Value + 1000;
                }
                );

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1000 + i, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.STA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 1001)]
        public void ParallelForEachInvokesEveryItemOnceSTATwoPerCore(int count)
        {
            // ensure Two-Per-Core
            ThreadQueue.Start(2 * Environment.ProcessorCount); 
            
            var items = new double[count];
            var pairs = new List<KeyValuePair<int, double>>();
            for (var i = 0; i < items.Length; i++)
            {
                pairs.Add(new KeyValuePair<int, double>(i, i));
            }
            Parallel.ForEach(pairs, pair => items[pair.Key] = pair.Value);

            Parallel.ForEach(pairs,
                pair =>
                {
                    items[pair.Key] = pair.Value + 1000;
                }
                );

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1000 + i, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        public void DoesNotGetConfusedByMultipleStartShutdown()
        {
            ThreadQueue.Shutdown();
            ThreadQueue.Shutdown();

            ThreadQueue.Start(2);
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            Control.NumberOfParallelWorkerThreads = 2;
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            ThreadQueue.Start(4);
            Assert.AreEqual(4, ThreadQueue.ThreadCount);
            Assert.AreEqual(4, Control.NumberOfParallelWorkerThreads);

            ThreadQueue.Shutdown();
            ThreadQueue.Start();
            Assert.AreEqual(4, ThreadQueue.ThreadCount);

            ThreadQueue.Start(2);
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            var items = new double[50];
            var pairs = new List<KeyValuePair<int, double>>();
            for (var i = 0; i < items.Length; i++)
            {
                items[i] = i;
                pairs.Add(new KeyValuePair<int, double>(i, i));
            }

            Parallel.ForEach(pairs,
                pair =>
                {
                    items[pair.Key] = pair.Value + 1000;
                }
                );

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1000 + i, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        public void DoesDetectAndResolveRecursiveParallelization()
        {
            var countSharedBetweenClosures = 0;
            var values = new int[10];
            
            Assert.DoesNotThrow(
                () =>
                Parallel.ForEach(values,
                    j => Interlocked.Increment(ref countSharedBetweenClosures)));

            Assert.AreEqual(10, countSharedBetweenClosures);
            countSharedBetweenClosures = 0;

            Parallel.ForEach(values,
                i =>
                Parallel.ForEach(values,
                    j => Interlocked.Increment(ref countSharedBetweenClosures)));

            Assert.AreEqual(100, countSharedBetweenClosures);
            countSharedBetweenClosures = 0;

            Parallel.ForEach(values,
                i =>
                Parallel.ForEach(values,
                    j =>
                    Parallel.ForEach(values,
                        k => Interlocked.Increment(ref countSharedBetweenClosures))));

            Assert.AreEqual(1000, countSharedBetweenClosures);
        }
    }
}
// <copyright file="RetryWithExponentialBackoff.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Utilities
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;

    /// <summary>
    /// Retry the request with Exponential back-off retry policy.
    /// </summary>
    public class RetryWithExponentialBackoff
    {
        private readonly int maxRetries;
        private readonly int delay;
        private int retryCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryWithExponentialBackoff"/> class.
        /// </summary>
        /// <param name="maxRetries">max retry count</param>
        /// <param name="delay">delay in milliseconds</param>
        public RetryWithExponentialBackoff(int maxRetries = 4, int delay = 2000)
        {
            this.maxRetries = maxRetries;
            this.delay = delay;
        }

        /// <summary>
        /// Retry the task execution with exponential back-off policy.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="task">Task to execute</param>
        /// <param name="data">data of Type T</param>
        /// <param name="successCallback">Success callback</param>
        /// <param name="failureCallback">Failure Callback</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RunAsync<T>(Task task, T data, Func<T, Task> successCallback, Func<Exception, T, Task> failureCallback)
        {
            while (this.retryCount <= this.maxRetries)
            {
                try
                {
                    await task;
                    await successCallback(data);
                    break;
                }
                catch (HttpException httpException)
                {
                    await failureCallback(httpException, data);
                    await this.Delay();
                }
                catch (Exception ex)
                {
                    await failureCallback(ex, data);
                    break;
                }
            }
        }

        private Task Delay()
        {
            this.retryCount++;
            return Task.Delay((int)Math.Pow(this.delay, this.retryCount));
        }
    }
}
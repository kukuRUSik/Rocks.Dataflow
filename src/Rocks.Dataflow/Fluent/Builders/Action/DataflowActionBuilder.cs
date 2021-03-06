﻿using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;

namespace Rocks.Dataflow.Fluent.Builders.Action
{
    public class DataflowActionBuilder<TStart, TInput> :
        DataflowFinalBuilder<DataflowActionBuilder<TStart, TInput>, TStart, TInput>
    {
        #region Private fields

        private readonly Func<TInput, Task> processAsync;
        private readonly Action<TInput> processSync;

        #endregion

        #region Construct

        public DataflowActionBuilder([CanBeNull] IDataflowBuilder<TStart, TInput> previousBuilder,
                                     [NotNull] Func<TInput, Task> processAsync)
            : base(previousBuilder)
        {
            if (processAsync == null)
                throw new ArgumentNullException(nameof(processAsync));

            this.processAsync = processAsync;
        }


        public DataflowActionBuilder([CanBeNull] IDataflowBuilder<TStart, TInput> previousBuilder,
                                     [NotNull] Action<TInput> processSync)
            : base(previousBuilder)
        {
            if (processSync == null)
                throw new ArgumentNullException(nameof(processSync));

            this.processSync = processSync;
        }

        #endregion

        #region Protected properties

        /// <summary>
        ///     Gets the builder instance that will be returned from the
        ///     <see cref="DataflowExecutionBlockBuilder{TStart,TBuilder,TInput}" /> methods.
        /// </summary>
        protected override DataflowActionBuilder<TStart, TInput> Builder => this;

        #endregion

        #region Protected methods

        /// <summary>
        ///     Creates a dataflow block from current configuration.
        /// </summary>
        protected override ITargetBlock<TInput> CreateBlock()
        {
            ActionBlock<TInput> block;

            if (this.processAsync != null)
            {
                block = new ActionBlock<TInput>
                    (async input =>
                           {
                               // ReSharper disable once CompareNonConstrainedGenericWithNull
                               if (input == null)
                                   return;

                               try
                               {
                                   await this.processAsync(input).ConfigureAwait(false);
                               }
                               catch (Exception ex)
                               {
                                   var logger = input as IDataflowErrorLogger;
                                   if (logger != null)
                                       logger.OnException(ex);
                                   else if (this.DefaultExceptionLogger != null)
                                       this.DefaultExceptionLogger(ex, input);
                               }
                           },
                     this.options);
            }
            else
            {
                block = new ActionBlock<TInput>
                    (input =>
                     {
                         // ReSharper disable once CompareNonConstrainedGenericWithNull
                         if (input == null)
                             return;

                         try
                         {
                             this.processSync(input);
                         }
                         catch (Exception ex)
                         {
                             var logger = input as IDataflowErrorLogger;
                             if (logger != null)
                                 logger.OnException(ex);
                             else
                                 this.DefaultExceptionLogger?.Invoke(ex, input);
                         }
                     },
                     this.options);
            }

            return block;
        }

        #endregion
    }
}
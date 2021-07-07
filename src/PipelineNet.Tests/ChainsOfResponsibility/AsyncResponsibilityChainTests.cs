using PipelineNet.ChainsOfResponsibility;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PipelineNet.Tests.ChainsOfResponsibility
{
    public class AsyncResponsibilityChainTests
    {
        #region Parameter definitions
        public class MyException : Exception
        {
            public string HandlerName { get; set; }
        }

        public class InvalidateDataException : MyException
        { }

        public class UnavailableResourcesException : MyException
        { }
        #endregion

        #region Middleware definitions
        public class SyncReplaceNewLineMiddleware : IAsyncMiddleware<string, string>
        {
            public Task<string> Run(string input, Func<string, Task<string>> executeNext,object args = null)
            {
                var newLineReplaced = input.Replace("\n", " ");

                var nextMiddleware = executeNext(newLineReplaced);
                var result = nextMiddleware.Result;
                return Task.FromResult(result);
            }
        }

        public class SyncTrimMiddleware : IAsyncMiddleware<string, string>
        {
            public Task<string> Run(string input, Func<string, Task<string>> executeNext,object args = null)
            {
                var trimmedString = input.Trim();

                var nextMiddleware = executeNext(trimmedString);
                var result = nextMiddleware.Result;
                return Task.FromResult(result);
            }
        }

        public class UnavailableResourcesExceptionHandler : IAsyncMiddleware<Exception, bool>
        {
            public async Task<bool> Run(Exception exception, Func<Exception, Task<bool>> executeNext, object args = null)
            {
                var castedException = exception as UnavailableResourcesException;
                if (castedException != null)
                {
                    castedException.HandlerName = this.GetType().Name;
                    return true;
                }
                return await executeNext(exception);
            }
        }

        public class InvalidateDataExceptionHandler : IAsyncMiddleware<Exception, bool>
        {
            public async Task<bool> Run(Exception exception, Func<Exception, Task<bool>> executeNext, object args = null)
            {
                var castedException = exception as InvalidateDataException;
                if (castedException != null)
                {
                    castedException.HandlerName = this.GetType().Name;
                    return true;
                }
                return await executeNext(exception);
            }
        }

        public class MyExceptionHandler : IAsyncMiddleware<Exception, bool>
        {
            public async Task<bool> Run(Exception exception, Func<Exception, Task<bool>> executeNext,object args = null)
            {
                var castedException = exception as MyException;
                if (castedException != null)
                {
                    castedException.HandlerName = this.GetType().Name;
                    return true;
                }
                return await executeNext(exception);
            }
        }
        #endregion

        [Fact]
        public async Task Execute_CreateChainOfMiddlewareToHandleException_TheRightMiddleHandlesTheException()
        {
            var responsibilityChain = new AsyncResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
                .Chain<UnavailableResourcesExceptionHandler>()
                .Chain<InvalidateDataExceptionHandler>()
                .Chain<MyExceptionHandler>();

            // Creates an invalid exception that should be handled by 'InvalidateDataExceptionHandler'.
            var invalidException = new InvalidateDataException();

            var result = await responsibilityChain.Execute(invalidException);

            // Check if the given exception was handled
            Assert.True(result);

            // Check if the correct handler handled the exception.
            Assert.Equal(typeof(InvalidateDataExceptionHandler).Name, invalidException.HandlerName);
        }

        [Fact]
        public async Task Execute_ChainOfMiddlewareThatDoesNotHandleTheException_ChainReturnsDefaultValue()
        {
            var responsibilityChain = new AsyncResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
                .Chain<UnavailableResourcesExceptionHandler>()
                .Chain<InvalidateDataExceptionHandler>()
                .Chain<MyExceptionHandler>();

            // Creates an ArgumentNullException, that will not be handled by any middleware.
            var excception = new ArgumentNullException();

            // The result should be the default for 'bool'.
            var result = await responsibilityChain.Execute(excception);

            Assert.Equal(default(bool), result);
        }

        [Fact]
        public async Task Execute_ChainOfMiddlewareWithFinallyFunc_FinallyFuncIsExecuted()
        {
            const string ExceptionSource = "EXCEPTION_SOURCE";

            var responsibilityChain = new AsyncResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
                .Chain<UnavailableResourcesExceptionHandler>()
                .Chain(typeof(InvalidateDataExceptionHandler))
                .Chain<MyExceptionHandler>()
                .Finally((ex) =>
                {
                    ex.Source = ExceptionSource;
                    return Task.FromResult(true);
                });

            // Creates an ArgumentNullException, that will not be handled by any middleware.
            var exception = new ArgumentNullException();

            // The result should true, since the finally function will be executed.
            var result = await responsibilityChain.Execute(exception);

            Assert.True(result);

            Assert.Equal(ExceptionSource, exception.Source);
        }

        /// <summary>
        /// Tests the <see cref="ResponsibilityChain{TParameter, TReturn}.Chain(Type)"/> method.
        /// </summary>
        [Fact]
        public void Chain_AddTypeThatIsNotAMiddleware_ThrowsException()
        {
            var responsibilityChain = new AsyncResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver());
            Assert.Throws<ArgumentException>(() =>
            {
                responsibilityChain.Chain(typeof(ResponsibilityChainTests));
            });
        }



        /// <summary>
        /// Try to generate a deadlock in synchronous middleware.
        /// </summary>
        [Fact]
        public void Execute_SynchronousChainOfResponsibility_SuccessfullyExecute()
        {
            var responsibilityChain = new AsyncResponsibilityChain<string, string>(new ActivatorMiddlewareResolver())
                .Chain<SyncReplaceNewLineMiddleware>()
                .Chain<SyncTrimMiddleware>()
                .Finally(input => Task.FromResult(input));

            var resultTask = responsibilityChain.Execute("  Test\nwith spaces\n and new lines \n ");
            var result = resultTask.Result;

            Assert.Equal("Test with spaces  and new lines", result);
        }
    }
}

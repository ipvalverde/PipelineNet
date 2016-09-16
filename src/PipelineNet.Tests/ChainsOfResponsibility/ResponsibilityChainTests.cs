using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipelineNet.ChainsOfResponsibility;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using System;
using System.Text.RegularExpressions;

namespace PipelineNet.Tests.ChainsOfResponsibility
{
    [TestClass]
    public class ResponsibilityChainTests
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
        public class UnavailableResourcesExceptionHandler : IMiddleware<Exception, bool>
        {
            public bool Run(Exception exception, Func<Exception, bool> executeNext)
            {
                var castedException = exception as UnavailableResourcesException;
                if (castedException != null)
                {
                    castedException.HandlerName = this.GetType().Name;
                    return true;
                }
                return executeNext(exception);
            }
        }

        public class InvalidateDataExceptionHandler : IMiddleware<Exception, bool>
        {
            public bool Run(Exception exception, Func<Exception, bool> executeNext)
            {
                var castedException = exception as InvalidateDataException;
                if (castedException != null)
                {
                    castedException.HandlerName = this.GetType().Name;
                    return true;
                }
                return executeNext(exception);
            }
        }

        public class MyExceptionHandler : IMiddleware<Exception, bool>
        {
            public bool Run(Exception exception, Func<Exception, bool> executeNext)
            {
                var castedException = exception as MyException;
                if (castedException != null)
                {
                    castedException.HandlerName = this.GetType().Name;
                    return true;
                }
                return executeNext(exception);
            }
        }
        #endregion

        [TestMethod]
        public void Execute_CreateChainOfMiddlewareToHandleException_TheRightMiddleHandlesTheException()
        {
            var responsibilityChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
                .Chain<UnavailableResourcesExceptionHandler>()
                .Chain<InvalidateDataExceptionHandler>()
                .Chain<MyExceptionHandler>();

            // Creates an invalid exception that should be handled by 'InvalidateDataExceptionHandler'.
            var invalidException = new InvalidateDataException();

            var result = responsibilityChain.Execute(invalidException);

            // Check if the given exception was handled
            Assert.IsTrue(result);

            // Check if the correct handler handled the exception.
            Assert.AreEqual(typeof(InvalidateDataExceptionHandler).Name, invalidException.HandlerName);
        }

        [TestMethod]
        public void Execute_ChainOfMiddlewareThatDoesNotHandleTheException_ChainReturnsDefaultValue()
        {
            var responsibilityChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
                .Chain<UnavailableResourcesExceptionHandler>()
                .Chain<InvalidateDataExceptionHandler>()
                .Chain<MyExceptionHandler>();

            // Creates an ArgumentNullException, that will not be handled by any middleware.
            var excception = new ArgumentNullException();

            // The result should be the default for 'bool'.
            var result = responsibilityChain.Execute(excception);

            Assert.AreEqual(default(bool), result);
        }

        [TestMethod]
        public void Execute_ChainOfMiddlewareWithFinallyFunc_FinallyFuncIsExecuted()
        {
            const string ExceptionSource = "EXCEPTION_SOURCE";

            var responsibilityChain = new ResponsibilityChain<Exception, bool>(new ActivatorMiddlewareResolver())
                .Chain<UnavailableResourcesExceptionHandler>()
                .Chain<InvalidateDataExceptionHandler>()
                .Chain<MyExceptionHandler>()
                .Finally((ex) =>
                {
                    ex.Source = ExceptionSource;
                    return true;
                });

            // Creates an ArgumentNullException, that will not be handled by any middleware.
            var exception = new ArgumentNullException();

            // The result should true, since the finally function will be executed.
            var result = responsibilityChain.Execute(exception);

            Assert.IsTrue(result);

            Assert.AreEqual(ExceptionSource, exception.Source);
        }
    }
}

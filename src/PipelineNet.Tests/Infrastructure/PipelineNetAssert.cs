using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineNet.Tests.Infrastructure
{
    public static class PipelineNetAssert
    {
        /// <summary>
        /// Assert that the code inside the given action will throw an exception.
        /// </summary>
        /// <typeparam name="TException">The exception type that will be expected.</typeparam>
        /// <param name="statements">The action that is expected to throw the exception.</param>
        public static void ThrowsException<TException>(Action statements)
            where TException : Exception
        {
            TException exception = null;
            try
            {
                statements();
            }
            catch(TException ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception,
                string.Format("The code did not thrown the expected exception \"{0}\".", typeof(TException)));
        }
    }
}

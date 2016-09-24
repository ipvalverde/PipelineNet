using Microsoft.VisualStudio.TestTools.UnitTesting;
using PipelineNet.Middleware;
using PipelineNet.MiddlewareResolver;
using PipelineNet.Pipelines;
using PipelineNet.Tests.Infrastructure;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineNet.Tests.Pipelines
{
    [TestClass]
    public class AsyncPipelineTests
    {
        #region Parameter definitions
        public enum Gender
        {
            Male,
            Female,
            Other
        }

        public class PersonModel
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public Gender? Gender { get; set; }

            public int Level { get; set; }
        }
        #endregion

        #region Middleware definitions
        public class PersonWithEvenId : IAsyncMiddleware<PersonModel>
        {
            public async Task Run(PersonModel context, Func<PersonModel, Task> executeNext)
            {
                if (context.Id.HasValue && context.Id.Value % 2 == 0)
                    context.Level = 1;
                await executeNext(context);
            }
        }

        public class PersonWithOddId : IAsyncMiddleware<PersonModel>
        {
            public async Task Run(PersonModel context, Func<PersonModel, Task> executeNext)
            {
                if (context.Id.HasValue && context.Id.Value % 2 != 0)
                    context.Level = 2;
                await executeNext(context);
            }
        }

        public class PersonWithEmailName : IAsyncMiddleware<PersonModel>
        {
            public async Task Run(PersonModel context, Func<PersonModel, Task> executeNext)
            {
                if (!string.IsNullOrWhiteSpace(context.Name) && Regex.IsMatch(context.Name, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                    context.Level = 3;
                await executeNext(context);
            }
        }

        public class PersonWithGenderProperty : IAsyncMiddleware<PersonModel>
        {
            public async Task Run(PersonModel context, Func<PersonModel, Task> executeNext)
            {
                if (context.Gender.HasValue)
                    context.Level = 4;

                Thread.Sleep(10);
                await executeNext(context);
            }
        }
        #endregion

        [TestMethod]
        public async Task Execute_RunSeveralMiddleware_SuccessfullyExecute()
        {
            var pipeline = new AsyncPipeline<PersonModel>(new ActivatorMiddlewareResolver())
                .Add<PersonWithEvenId>()
                .Add<PersonWithOddId>()
                .Add<PersonWithEmailName>()
                .Add<PersonWithGenderProperty>();

            // This person model has a name that matches the 'PersonWithEmailName' middleware.
            var personModel = new PersonModel
            {
                Name = "this_is_my_email@servername.js"
            };

            await pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 3, which is configured by 'PersonWithEmailName' middleware.
            Assert.AreEqual(3, personModel.Level);
        }

        [TestMethod]
        public async Task Execute_RunSamePipelineTwice_SuccessfullyExecute()
        {
            var pipeline = new AsyncPipeline<PersonModel>(new ActivatorMiddlewareResolver())
                .Add<PersonWithEvenId>()
                .Add<PersonWithOddId>()
                .Add<PersonWithEmailName>()
                .Add<PersonWithGenderProperty>();

            // This person model has a name that matches the 'PersonWithEmailName' middleware.
            var personModel = new PersonModel
            {
                Name = "this_is_my_email@servername.js"
            };

            await pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 3, which is configured by 'PersonWithEmailName' middleware.
            Assert.AreEqual(3, personModel.Level);


            // Creates a new instance with a 'Gender' property. The 'PersonWithGenderProperty'
            // middleware should be the last one to be executed.
            personModel = new PersonModel
            {
                Name = "this_is_my_email@servername.js",
                Gender = Gender.Other
            };

            pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 4, which is configured by 'PersonWithGenderProperty' middleware.
            Assert.AreEqual(4, personModel.Level);
        }

        [TestMethod]
        public async Task Execute_RunSeveralMiddlewareWithTwoBeingDynamiccalyAdded_SuccessfullyExecute()
        {
            var pipeline = new AsyncPipeline<PersonModel>(new ActivatorMiddlewareResolver())
                .Add<PersonWithEvenId>()
                .Add(typeof(PersonWithOddId))
                .Add<PersonWithEmailName>()
                .Add(typeof(PersonWithGenderProperty));

            // This person model has a gender, so the last middleware will be the one handling the input.
            var personModel = new PersonModel
            {
                Gender = Gender.Female
            };

            await pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 4, which is configured by 'PersonWithGenderProperty' middleware.
            Assert.AreEqual(4, personModel.Level);
        }

        /// <summary>
        /// Tests the <see cref="AsyncPipeline{TParameter}.Add(Type)"/> method.
        /// </summary>
        [TestMethod]
        public void Add_AddTypeThatIsNotAMiddleware_ThrowsException()
        {
            var pipeline = new AsyncPipeline<PersonModel>(new ActivatorMiddlewareResolver());
            PipelineNetAssert.ThrowsException<ArgumentException>(() =>
            {
                pipeline.Add(typeof(AsyncPipelineTests));
            });
        }
    }
}

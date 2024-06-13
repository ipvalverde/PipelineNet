using PipelineNet.Middleware;
using PipelineNet.Pipelines;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace PipelineNet.Tests.Pipeline
{
    public class PipelineTests
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
        public class PersonWithEvenId : IMiddleware<PersonModel>
        {
            public void Run(PersonModel context, Action<PersonModel> executeNext)
            {
                if (context.Id.HasValue && context.Id.Value % 2 == 0)
                    context.Level = 1;
                executeNext(context);
            }
        }

        public class PersonWithOddId : IMiddleware<PersonModel>
        {
            public void Run(PersonModel context, Action<PersonModel> executeNext)
            {
                if (context.Id.HasValue && context.Id.Value % 2 != 0)
                    context.Level = 2;
                executeNext(context);
            }
        }

        public class PersonWithEmailName : IMiddleware<PersonModel>
        {
            public void Run(PersonModel context, Action<PersonModel> executeNext)
            {
                if (!string.IsNullOrWhiteSpace(context.Name) && Regex.IsMatch(context.Name, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                    context.Level = 3;
                executeNext(context);
            }
        }

        public class PersonWithGenderProperty : IMiddleware<PersonModel>
        {
            public void Run(PersonModel context, Action<PersonModel> executeNext)
            {
                if (context.Gender.HasValue)
                    context.Level = 4;
                executeNext(context);
            }
        }
        #endregion

        [Fact]
        public void Execute_RunSeveralMiddleware_SuccessfullyExecute()
        {
            var pipeline = new Pipeline<PersonModel>()
                .Add<PersonWithEvenId>()
                .Add<PersonWithOddId>()
                .Add<PersonWithEmailName>()
                .Add<PersonWithGenderProperty>();

            // This person model has a name that matches the 'PersonWithEmailName' middleware.
            var personModel = new PersonModel
            {
                Name = "this_is_my_email@servername.js"
            };

            pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 3, which is configured by 'PersonWithEmailName' middleware.
            Assert.Equal(3, personModel.Level);
        }

        [Fact]
        public void Execute_RunSamePipelineTwice_SuccessfullyExecute()
        {
            var pipeline = new Pipeline<PersonModel>()
                .Add<PersonWithEvenId>()
                .Add<PersonWithOddId>()
                .Add<PersonWithEmailName>()
                .Add<PersonWithGenderProperty>();

            // This person model has a name that matches the 'PersonWithEmailName' middleware.
            var personModel = new PersonModel
            {
                Name = "this_is_my_email@servername.js"
            };
            pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 3, which is configured by 'PersonWithEmailName' middleware.
            Assert.Equal(3, personModel.Level);


            // Creates a new instance with a 'Gender' property. The 'PersonWithGenderProperty'
            // middleware should be the last one to be executed.
            personModel = new PersonModel
            {
                Name = "this_is_my_email@servername.js",
                Gender = Gender.Other
            };

            pipeline.Execute(personModel);

            // Check if the level of 'personModel' is 4, which is configured by 'PersonWithGenderProperty' middleware.
            Assert.Equal(4, personModel.Level);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineNet.Tests.ChainsOfResponsibility
{
    [TestClass]
    public class ResponsibilityChainTests
    {
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

        public class PersonWithEvenId : IMiddleware<PersonModel>
        {
            public int? Run(PersonModel context, Action<PersonModel> executeNext)
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
    }
}

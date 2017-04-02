﻿using NRules.IntegrationTests.TestAssets;
using NUnit.Framework;

namespace NRules.IntegrationTests
{
    [TestFixture]
    public class ForwardChainingTest : BaseRuleTestFixture
    {
        [Test]
        public void Fire_OneMatchingFactErrorInSecondCondition_FiresFirstRuleAndChainsSecond()
        {
            //Arrange
            var fact1 = new FactType1 {TestProperty = "Valid Value 1", JoinProperty = "Valid Value 1"};
            Session.Insert(fact1);

            //Act
            Session.Fire();

            //Assert
            AssertFiredOnce<ForwardChainingFirstRule>();
            AssertFiredOnce<ForwardChainingSecondRule>();
        }
        
        [Test]
        public void Fire_OneMatchingFact_FiresFirstRuleAndChainsSecond()
        {
            //Arrange
            var fact1 = new FactType1 {TestProperty = "Valid Value 1", JoinProperty = null};
            Session.Insert(fact1);

            //Act - Assert
            var ex = Assert.Throws<RuleActionEvaluationException>(() => Session.Fire());
            Assert.NotNull(ex.InnerException);
            Assert.IsInstanceOf<RuleConditionEvaluationException>(ex.InnerException);
        }

        [Test]
        public void Fire_OneMatchingFactOfOneKindAndOneOfAnotherKind_FiresSecondRuleDirectlyAndChained()
        {
            //Arrange
            var fact1 = new FactType1 {TestProperty = "Valid Value 1", JoinProperty = "Valid Value 1"};
            var fact2 = new FactType2 {TestProperty = "Valid Value 2"};

            Session.Insert(fact1);
            Session.Insert(fact2);

            //Act
            Session.Fire();

            //Assert
            AssertFiredOnce<ForwardChainingFirstRule>();
            AssertFiredTwice<ForwardChainingSecondRule>();
        }

        protected override void SetUpRules()
        {
            SetUpRule<ForwardChainingFirstRule>();
            SetUpRule<ForwardChainingSecondRule>();
        }

        public class FactType1
        {
            public string TestProperty { get; set; }
            public string JoinProperty { get; set; }
        }

        public class FactType2
        {
            public string TestProperty { get; set; }
            public string JoinProperty { get; set; }
        }

        public class ForwardChainingFirstRule : BaseRule
        {
            public override void Define()
            {
                FactType1 fact1 = null;

                When()
                    .Match<FactType1>(() => fact1, f => f.TestProperty.StartsWith("Valid"));
                Then()
                    .Do(ctx => Action(ctx))
                    .Do(ctx => ctx.Insert(new FactType2
                    {
                        TestProperty = fact1.JoinProperty,
                        JoinProperty = fact1.TestProperty
                    }));
            }
        }

        public class ForwardChainingSecondRule : BaseRule
        {
            public override void Define()
            {
                FactType2 fact2 = null;

                When()
                    .Match<FactType2>(() => fact2, f => f.TestProperty.StartsWith("Valid"));
                Then()
                    .Do(ctx => Action(ctx));
            }
        }
    }
}
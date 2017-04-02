using NRules.IntegrationTests.TestAssets;
using NUnit.Framework;

namespace NRules.IntegrationTests
{
    [TestFixture]
    public class IdentityMatchRuleTest : BaseRuleTestFixture
    {
        [Test]
        public void Fire_MatchingFact_FiresOnce()
        {
            //Arrange
            var fact = new FactType {TestProperty = "Valid value"};
            Session.Insert(fact);

            //Act
            Session.Fire();

            //Assert
            AssertFiredOnce();
        }

        [Test]
        public void Fire_TwoMatchingFacts_FiresTwice()
        {
            //Arrange
            var fact1 = new FactType {TestProperty = "Valid value"};
            var fact2 = new FactType {TestProperty = "Valid value"};
            var facts = new[] {fact1, fact2};
            Session.InsertAll(facts);

            //Act
            Session.Fire();

            //Assert
            AssertFiredTwice();
        }

        [Test]
        public void Fire_MatchingFactInsertedAndRetracted_DoesNotFire()
        {
            //Arrange
            var fact = new FactType {TestProperty = "Valid value"};
            Session.Insert(fact);
            Session.Retract(fact);

            //Act
            Session.Fire();

            //Assert
            AssertDidNotFire();
        }

        [Test]
        public void Fire_MatchingFactInsertedAndUpdatedToInvalid_DoesNotFire()
        {
            //Arrange
            var fact = new FactType {TestProperty = "Valid value"};
            Session.Insert(fact);
            fact.TestProperty = "Invalid value";
            Session.Update(fact);

            //Act
            Session.Fire();

            //Assert
            AssertDidNotFire();
        }

        [Test]
        public void Fire_NoMatchingFact_DoesNotFire()
        {
            //Arrange
            //Act
            Session.Fire();

            //Assert
            AssertDidNotFire();
        }

        protected override void SetUpRules()
        {
            SetUpRule<TestRule>();
        }

        public class FactType
        {
            public string TestProperty { get; set; }
        }

        public class TestRule : BaseRule
        {
            public override void Define()
            {
                FactType fact1 = null;
                FactType fact2 = null;

                When()
                    .Match<FactType>(() => fact1, f => f.TestProperty.StartsWith("Valid"))
                    .Match<FactType>(() => fact2, f => ReferenceEquals(f, fact1));

                Then()
                    .Do(ctx => Action(ctx));
            }
        }
    }
}
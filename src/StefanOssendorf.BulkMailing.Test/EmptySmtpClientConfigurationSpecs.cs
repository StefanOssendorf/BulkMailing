using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {

    [Subject(typeof (EmptySmtpClientConfiguration))]
    public class Creating_new_empty_configuration {
        static SmtpClientConfiguration mConfiguration;

        Because of = () => mConfiguration = new EmptySmtpClientConfiguration();

        It Should_be_marked_as_empty_config = () => mConfiguration.IsEmpty.ShouldBeTrue();
        It Should_set_default_values_for_empty_config = () => {
            mConfiguration.EnableSsl.ShouldBeFalse();
            mConfiguration.Host.ShouldBeNull();
            mConfiguration.Password.ShouldBeNull();
            mConfiguration.Port.ShouldEqual(0);
            mConfiguration.UseDefaultCredentials.ShouldBeFalse();
            mConfiguration.UserName.ShouldBeNull();
        };
    }

    [Subject(typeof (EmptySmtpClientConfiguration))]
    public class Cloning_empty_configuration {
        static SmtpClientConfiguration mSut;
        static SmtpClientConfiguration mResult;

        Establish context = () => mSut = new EmptySmtpClientConfiguration();
        Because of = () => mResult = mSut.Clone();

        It Should_create_a_new_empty_config = () => mResult.ShouldNotBeTheSameAs(mSut);
    }
}
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof(SmtpClientConfiguration))]
    public class When_assign_propertie_values {
        static SmtpClientConfiguration mSut;

        Establish context = () => mSut = new SmtpClientConfiguration();
        Because of = () => {
            mSut.EnableSsl = true;
            mSut.Host = "127.0.0.1";
            mSut.Password = "Password";
            mSut.Port = 30;
            mSut.UseDefaultCredentials = true;
            mSut.UserName = "User";
        };

        It Should_set_them_correctly = () => {
            mSut.EnableSsl.ShouldBeTrue();
            mSut.Host.ShouldEqual("127.0.0.1");
            mSut.Password.ShouldEqual("Password");
            mSut.Port.ShouldEqual(30);
            mSut.UseDefaultCredentials.ShouldBeTrue();
            mSut.UserName.ShouldEqual("User");
        };
    }

    [Subject(typeof(SmtpClientConfiguration))]
    public class Creating_a_new_smtpClientConfiguration {
        static SmtpClientConfiguration mSut;

        Because of = () => mSut = new SmtpClientConfiguration();

        It Should_not_be_marked_as_empty = () => mSut.IsEmpty.ShouldBeFalse();
        It Should_set_25_as_default_port = () => mSut.Port.ShouldEqual(25);
    }

    [Subject(typeof(SmtpClientConfiguration))]
    public class When_cloning_configuration {
        static SmtpClientConfiguration mSut;
        static SmtpClientConfiguration mResult;

        Establish context = () => mSut = new SmtpClientConfiguration {
            EnableSsl = true,
            Port = 1,
            UserName = "User",
            Host = "127.0.0.1",
            UseDefaultCredentials = true,
            Password = "PW"
        };
        Because of = () => mResult = mSut.Clone();

        It Should_create_a_new_instance = () => mResult.ShouldNotBeTheSameAs(mSut);
        It Should_have_the_same_values = () => mResult.ShouldBeLike(mSut);
    }
}
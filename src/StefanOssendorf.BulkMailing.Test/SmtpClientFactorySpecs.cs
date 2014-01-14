using System.Net;
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof (SmtpClientFactory))]
    public class When_creating_an_client_with_empty_config {
        static ISmtpClientFactory mFactory;
        static SmtpClientWrapper mClient;

        Establish context = () => mFactory = new SmtpClientFactory();
        Because of = () => mClient = (SmtpClientWrapper)mFactory.Create();

        It Should_configure_the_client_from_appConfig = () => mClient.Host.ShouldEqual("127.0.0.1");
    }

    [Subject(typeof (SmtpClientFactory))]
    public class When_creanting_an_client_with_provided_config {
        static ISmtpClientFactory mFactory;
        static SmtpClientWrapper mClient;
        static SmtpClientConfiguration mConfig;

        Establish context = () => {
            mConfig = new SmtpClientConfiguration {
                Port = 1337,
                Host = "127.0.0.1",
                UserName = "user",
                Password = "passw",
                EnableSsl = true
            };
            mFactory = new SmtpClientFactory(mConfig);
        };
        Because of = () => mClient = (SmtpClientWrapper)mFactory.Create();

        It Should_configure_the_client_from_provided_config = () => {
            mClient.Host.ShouldEqual("127.0.0.1");
            mClient.EnableSsl.ShouldBeTrue();
            mClient.Port.ShouldEqual(1337);
            ((NetworkCredential) mClient.Credentials).UserName.ShouldEqual("user");
            ((NetworkCredential) mClient.Credentials).Password.ShouldEqual("passw");
        };
    }
}
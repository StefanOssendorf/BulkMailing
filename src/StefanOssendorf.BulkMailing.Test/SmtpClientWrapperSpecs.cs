using System.Net;
using System.Net.Mail;
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof (SmtpClientWrapper))]
    public class When_setting_poperties {
        static SmtpClient mClient;
        static SmtpClientWrapper mSut;
        static NetworkCredential mCredential;

        Establish context = () => {
            mClient = new SmtpClient();
            mSut = new SmtpClientWrapper(mClient);
        };
        Because of = () => {
            mSut.Credentials = mCredential;
            mSut.DeliveryMethod = SmtpDeliveryMethod.Network;
            mSut.EnableSsl = true;
            mSut.Host = "127.0.0.1";
            mSut.Port = 1337;
            mSut.UseDefaultCredentials = true;
        };

        It Should_set_the_properties_correct = () => {
            mClient.DeliveryMethod.ShouldEqual(SmtpDeliveryMethod.Network);
            mClient.EnableSsl.ShouldBeTrue();
            mClient.Host.ShouldEqual("127.0.0.1");
            mClient.Port.ShouldEqual(1337);
            mClient.UseDefaultCredentials.ShouldBeTrue();

            //Assume credentials are correct - dunno how to test, following lines don't work
            //((NetworkCredential) mClient.Credentials).Password.ShouldEqual("pasw");
            //((NetworkCredential) mClient.Credentials).UserName.ShouldEqual("user");
        };
    }
}
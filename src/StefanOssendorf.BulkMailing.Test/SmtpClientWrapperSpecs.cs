using System;
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
            mSut.UseDefaultCredentials = false;
            mSut.Credentials = new NetworkCredential("user", "passw");
            mSut.DeliveryMethod = SmtpDeliveryMethod.Network;
            mSut.EnableSsl = true;
            mSut.Host = "127.0.0.1";
            mSut.Port = 1337;
        };

        It Should_set_properties_correct = () => {
            mClient.DeliveryMethod.ShouldEqual(SmtpDeliveryMethod.Network);
            mClient.EnableSsl.ShouldBeTrue();
            mClient.Host.ShouldEqual("127.0.0.1");
            mClient.Port.ShouldEqual(1337);
            mClient.UseDefaultCredentials.ShouldBeFalse();
            ((NetworkCredential) mClient.Credentials).Password.ShouldEqual("passw");
            ((NetworkCredential) mClient.Credentials).UserName.ShouldEqual("user");
        };
    }

    [Subject(typeof (SmtpClientWrapper))]
    public class When_reading_properties {
        static SmtpClient mClient;
        static SmtpClientWrapper mSut;
        static NetworkCredential mCredential;
        static bool mEnableSsl;
        static SmtpDeliveryMethod mDeliveryMethod;
        static string mHost;
        static int mPort;
        static bool mUseDefaultCredentials;

        Establish context = () => {
            mClient = new SmtpClient {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("user", "passw"),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Host = "127.0.0.1",
                Port = 1337
            };
            mSut = new SmtpClientWrapper(mClient);
        };

        Because of = () => {
            mCredential = (NetworkCredential)mSut.Credentials;
            mDeliveryMethod = mSut.DeliveryMethod;
            mEnableSsl = mSut.EnableSsl;
            mHost = mSut.Host;
            mPort = mSut.Port;
            mUseDefaultCredentials = mSut.UseDefaultCredentials;
        };

        It Should_read_properties_correct = () => {
            mCredential.Password.ShouldEqual("passw");
            mCredential.UserName.ShouldEqual("user");
            mDeliveryMethod.ShouldEqual(SmtpDeliveryMethod.Network);
            mHost.ShouldEqual("127.0.0.1");
            mPort.ShouldEqual(1337);
            mUseDefaultCredentials.ShouldBeFalse();
        };
    }

    [Subject(typeof (SmtpClientWrapper))]
    public class When_dispose_called {
        static SmtpClient mClient;
        static SmtpClientWrapper mSut;

        Establish context = () => {
            mClient = new SmtpClient();
            mSut = new SmtpClientWrapper(mClient);
        };

        Because of = () => mSut.Dispose();

        private It Should_dispose_used_smtpClient = () => Catch.Exception(() => mClient.Send(null)).ShouldBeOfType<ObjectDisposedException>();
    }

    [Subject(typeof (SmtpClientWrapper))]
    public class When_sending_an_email {
        static SmtpClientWrapper mSut;
        static Exception mException;

        Establish context = () => {
            mSut = new SmtpClientWrapper();
        };

        Because of = () => mException = Catch.Exception(() => mSut.Send(new MailMessage("test@test.org", "test2@est2.org", "subject", "body")));

        It Should_try_to_send_the_mail_via_smtpClient = () => mException.ShouldBeOfType<SmtpException>();
    }

    [Subject(typeof (SmtpClientWrapper))]
    public class When_disposing_multiple_times {
        static SmtpClientWrapper mSut;
        static Exception mException;

        Establish context = () => mSut = new SmtpClientWrapper();
        Because of = () => {
            mSut.Dispose();
            mException = Catch.Exception(mSut.Dispose);
        };

        It Should_not_throw_an_exception = () => mException.ShouldBeNull();
    }
}
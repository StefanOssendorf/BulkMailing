using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using FakeItEasy;
using Machine.Specifications;
using Machine.Specifications.Utility;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof (MailSender))]
    public class When_calling_startSending_after_disposing {
        static MailSender mSender;
        static Exception mException;

        Establish context = () => {
            mSender = new MailSender();
            mSender.Dispose();
        };

        Because of = () => mException = Catch.Exception(() => mSender.StartSending(new List<MailSenderMessage>()));
        
        It Should_throw_an_objectDisposedException = () => mException.ShouldBeOfType<ObjectDisposedException>();
    }

    [Subject(typeof (MailSender))]
    public class When_calling_startSending_with_10_messages {
        static MailSender mSender;
        static List<MailSenderMessage> mMessages;
        static MailStreamResult mResult;
        static ISmtpClient mClient;
        static int mCount;

        Establish context = () => {
            var facotry = A.Fake<ISmtpClientFactory>();
            mClient = A.Fake<ISmtpClient>();
            A.CallTo(() => facotry.Create()).Returns(mClient);
            mSender = new MailSender(facotry);

            mMessages = new List<MailSenderMessage>();
            for (int i = 1; i < 11; i++) {
                mMessages.Add(CreateMessage(i));
            }
            mCount = 0;
        };

        Because of = () => {
            mResult = mSender.StartSending(mMessages);
            var output = mResult.Result;
            var cenum = output.GetConsumingEnumerable();
            cenum.Each(item => ++mCount);
        };

        It Should_call_client_send_10_times = () => A.CallTo(() => mClient.Send(null)).WithAnyArguments().MustHaveHappened(Repeated.Exactly.Times(10));
        It Result_should_be_finished = () => mResult.IsFinished.ShouldBeTrue();

        static MailSenderMessage CreateMessage(int i) {
            return new MailSenderMessage(new MailMessage(),i);
        }
    }

    [Subject(typeof (MailSender))]
    public class When_calling_stopSending {
        static MailSender mSender;
        static List<MailSenderMessage> mMessages;
        static MailStreamResult mResult;

        Establish context = () => {
            var facotry = A.Fake<ISmtpClientFactory>();
            var client = A.Fake<ISmtpClient>();
            A.CallTo(() => facotry.Create()).ReturnsLazily(() => {
                Thread.Sleep(2);
                return client;
            });
            mSender = new MailSender(facotry);

            mMessages = new List<MailSenderMessage>();
            for (int i = 0; i < 10; i++) {
                mMessages.Add(CreateMessage(i));
            }
        };
        static MailSenderMessage CreateMessage(int i) {
            return new MailSenderMessage(new MailMessage(),i);
        }

        Because of = () => {
            mResult = mSender.StartSending(mMessages);
            mSender.StopSending();
        };

        It Should_cancel_all_messages = () => mResult.Result.GetConsumingEnumerable().ShouldEachConformTo(msg => msg.Canceled);
        It Should_set_isStopped = () => mResult.IsStopped.ShouldBeTrue();
        It Should_not_set_isFinished = () => mResult.IsFinished.ShouldBeFalse();
        It Should_not_set_hasError = () => mResult.HasError.ShouldBeFalse();
    }

    [Subject(typeof (MailSender))]
    public class When_having_problems_with_sending {
        static MailSender mSender;
        static List<MailSenderMessage> mMessages;
        static MailStreamResult mResult;
        static ISmtpClient mClient;

        Establish context = () => {
            var facotry = A.Fake<ISmtpClientFactory>();
            mClient = A.Fake<ISmtpClient>();
            A.CallTo(() => facotry.Create()).Returns(mClient);
            A.CallTo(() => mClient.Send(null)).WithAnyArguments().Throws(new SmtpException());
            mSender = new MailSender(facotry);

            mMessages = new List<MailSenderMessage>();
            for (int i = 0; i < 10; i++) {
                mMessages.Add(CreateMessage(i));
            }
        };
        static MailSenderMessage CreateMessage(int i) {
            return new MailSenderMessage(new MailMessage(), i);
        }

        Because of = () => mResult = mSender.StartSending(mMessages);

        It Each_message_should_have_an_error = () => mResult.Result.GetConsumingEnumerable().ShouldEachConformTo(msg => msg.HasError);
        It Should_not_set_hasError = () => mResult.HasError.ShouldBeFalse();
    }

    [Subject(typeof (MailSender))]
    public class When_calling_dispose_during_sending {
        static MailSender mSender;
        static List<MailSenderMessage> mMessages;
        static MailStreamResult mResult;
        static List<ISmtpClient> mClients;
        

        Establish context = () => {
            mClients = new List<ISmtpClient>();
            var facotry = A.Fake<ISmtpClientFactory>();
            A.CallTo(() => facotry.Create()).ReturnsLazily(() => {
                var client = A.Fake<ISmtpClient>();
                mClients.Add(client);
                A.CallTo(() => client.Send(null)).WithAnyArguments().Invokes(() => Thread.Sleep(1));
                return client;
            });

            mSender = new MailSender(facotry);

            mMessages = new List<MailSenderMessage>();
            for (int i = 0; i < 10; i++) {
                mMessages.Add(CreateMessage(i));
            }
        };
        static MailSenderMessage CreateMessage(int i) {
            return new MailSenderMessage(new MailMessage(), i);
        }

        Because of = () => {
            mResult = mSender.StartSending(mMessages);
            mSender.Dispose();
        };

        It Should_stop_sending = () => mResult.IsStopped.ShouldBeTrue();
        It Should_dispose_all_used_smtpClients = () => mClients.ForEach(client => A.CallTo(() => client.Dispose()).MustHaveHappened());
        It Should_set_all_messages_to_canceled = () => mResult.Result.GetConsumingEnumerable().ShouldEachConformTo(msg => msg.Canceled);
    }
}
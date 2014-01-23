using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using FakeItEasy;
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    public static class MailSenderSpecs {

        [Subject(typeof(MailSender))]
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

        [Subject(typeof(MailSender))]
        public class When_calling_startSending_with_10_messages {
            static MailSender mSender;
            static List<MailSenderMessage> mMessages;
            static MailStreamResult mResult;
            static ISmtpClient mClient;
            static int mCounter;

            Establish context = () => {
                var facotry = A.Fake<ISmtpClientFactory>();
                mClient = A.Fake<ISmtpClient>();
                A.CallTo(() => facotry.Create()).Returns(mClient);
                A.CallTo(() => mClient.Send(null)).WithAnyArguments().Invokes(() => Interlocked.Increment(ref mCounter));
                mSender = new MailSender(facotry);

                mMessages = new List<MailSenderMessage>();
                for (int i = 1; i < 11; i++) {
                    mMessages.Add(CreateMessage(i));
                }
                mCounter = 0;
            };

            Because of = () => {
                mResult = mSender.StartSending(mMessages);
                mResult.BackgroundTask.Wait();
            };

            It Should_call_client_send_10_times = () => mCounter.ShouldEqual(10);
            It Result_should_be_finished = () => mResult.IsFinished.ShouldBeTrue();

            static MailSenderMessage CreateMessage(int i) {
                return new MailSenderMessage(new MailMessage(), i);
            }
        }

        [Subject(typeof(MailSender))]
        public class When_calling_stopSending {
            static MailSender mSender;
            static List<MailSenderMessage> mMessages;
            static MailStreamResult mResult;
            static CancellationTokenSource mCts;

            Establish context = () => {
                mCts = new CancellationTokenSource();
                var facotry = A.Fake<ISmtpClientFactory>();
                var client = A.Fake<ISmtpClient>();
                A.CallTo(() => facotry.Create()).Returns(client);
                mSender = new MailSender(facotry, mCts);

                mMessages = new List<MailSenderMessage>();
                for (int i = 0; i < 10; i++) {
                    mMessages.Add(CreateMessage(i));
                }
            };
            static MailSenderMessage CreateMessage(int i) {
                return new MailSenderMessage(new MailMessage(), i);
            }

            Because of = () => {
                mCts.Cancel();
                mResult = mSender.StartSending(mMessages);
                mSender.StopSending();
            };

            It Should_cancel_all_messages = () => mResult.Result.GetConsumingEnumerable().ShouldEachConformTo(msg => msg.Canceled);
            It Should_set_isStopped = () => mResult.IsStopped.ShouldBeTrue();
            It Should_not_set_isFinished = () => mResult.IsFinished.ShouldBeFalse();
            It Should_not_set_hasError = () => mResult.HasError.ShouldBeFalse();
        }

        [Subject(typeof(MailSender))]
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

        [Subject(typeof(MailSender))]
        public class When_calling_dispose_during_sending {
            static MailSender mSender;
            static List<MailSenderMessage> mMessages;
            static MailStreamResult mResult;
            static List<ISmtpClient> mClients;
            static List<MailSendResult> mResultMessages;



            Establish context = () => {
                mResultMessages = new List<MailSendResult>();

                mClients = new List<ISmtpClient>();
                var facotry = A.Fake<ISmtpClientFactory>();
                A.CallTo(() => facotry.Create()).ReturnsLazily(() => {
                    var client = A.Fake<ISmtpClient>();
                    mClients.Add(client);
                    return client;
                });

                mSender = new MailSender(facotry);

                mMessages = new List<MailSenderMessage>();
                for (int i = 0; i < 50000; i++) {
                    mMessages.Add(CreateMessage(i));
                }
            };
            static MailSenderMessage CreateMessage(int i) {
                return new MailSenderMessage(new MailMessage(), i);
            }

            Because of = () => {
                mResult = mSender.StartSending(mMessages);
                Thread.Sleep(100);
                mSender.Dispose();
                mResult.Result.GetConsumingEnumerable().ForEach(msg => mResultMessages.Add(msg));
            };

            It Should_stop_sending = () => mResult.IsStopped.ShouldBeTrue();
            // No concrete number possible, it depends too much on task scheduling
            It Should_set_at_least_one_message_to_canceled = () => mResultMessages.ShouldContain(msg => msg.Canceled);
        }

        [Subject(typeof(MailSender))]
        public class When_disposing_after_sending {
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
                mResult.BackgroundTask.Await();
                mSender.Dispose();
            };

            It Should_call_dispose_on_all_smtpClients = () => mClients.ForEach(client => A.CallTo(() => client.Dispose()).MustHaveHappened());
        }

        [Subject(typeof(MailSender))]
        public class When_disposing_multiple_times {
            static MailSender mSender;
            static Exception mException;

            Establish context = () => {
                mException = null;
                mSender = new MailSender(new EmptySmtpClientConfiguration());
                mSender.Dispose();
            };

            Because of = () => {
                mException = Catch.Exception(() => mSender.Dispose());
            };

            It Should_not_throw_an_exception = () => mException.ShouldBeNull();
        }
    }
}
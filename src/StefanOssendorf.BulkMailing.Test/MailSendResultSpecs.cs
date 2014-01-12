using System;
using System.Net.Mail;
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof (MailSendResult))]
    public class When_sending_is_successful {
        static MailSendResult mResult;

        private Establish context = () => mResult = new MailSendResult(new MailSenderMessage(new MailMessage()) { UserIdentifier = "1" });
        private Because of = () => mResult.Successful = true;

        It Should_be_successful = () => mResult.Successful.ShouldBeTrue();
        It Should_have_a_message = () => mResult.MailMessage.ShouldNotBeNull();
        It Should_have_the_correct_userIdentifier = () => mResult.UserIdentifier.ShouldEqual("1");
        It Should_not_be_canceled = () => mResult.Canceled.ShouldBeFalse();
        It Should_not_have_an_error = () => mResult.HasError.ShouldBeFalse();
        It Should_not_have_an_exception = () => mResult.Exception.ShouldBeNull();
    }

    [Subject(typeof (MailSendResult))]
    public class When_sending_is_canceled {
        static MailSendResult mResult;

        private Establish context = () => mResult = new MailSendResult(new MailSenderMessage(new MailMessage(),"1"));
        private Because of = () => mResult.Canceled = true;

        It Should_not_be_successful = () => mResult.Successful.ShouldBeFalse();
        It Should_have_a_message = () => mResult.MailMessage.ShouldNotBeNull();
        It Should_have_the_correct_userIdentifier = () => mResult.UserIdentifier.ShouldEqual("1");
        It Should_be_canceled = () => mResult.Canceled.ShouldBeTrue();
        It Should_not_have_an_error = () => mResult.HasError.ShouldBeFalse();
        It Should_not_have_an_exception = () => mResult.Exception.ShouldBeNull();
    }

    [Subject(typeof (MailSendResult))]
    public class When_error_ocurred_during_sending {
        static MailSendResult mResult;

        private Establish context = () => mResult = new MailSendResult(new MailSenderMessage { UserIdentifier = "1", Message = new MailMessage() });
        private Because of = () =>  mResult.Exception = new Exception();

        It Should_not_be_successful = () => mResult.Successful.ShouldBeFalse();
        It Should_have_a_message = () => mResult.MailMessage.ShouldNotBeNull();
        It Should_have_the_correct_userIdentifier = () => mResult.UserIdentifier.ShouldEqual("1");
        It Should_not_be_canceled = () => mResult.Canceled.ShouldBeFalse();
        It Should_have_an_error = () => mResult.HasError.ShouldBeTrue();
        It Should_have_an_exception = () => mResult.Exception.ShouldNotBeNull();
    }
}
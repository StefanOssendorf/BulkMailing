using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof (MailStreamResult))]
    public class When_sending_is_finished {
        static MailStreamResult mSut;

        Because of = () => {
            var t1 = Task.FromResult(1);
            mSut = new MailStreamResult(new BlockingCollection<MailSendResult>(), t1);
        };

        It Should_be_finished = () => mSut.IsFinished.ShouldBeTrue();
        It Should_not_be_stopped = () => mSut.IsStopped.ShouldBeFalse();
        It Should_have_no_error = () => mSut.HasError.ShouldBeFalse();
    }

    [Subject(typeof (MailSendResult))]
    public class When_sending_is_stopped {
        static MailStreamResult mSut;


        Because of = () => {
            mSut = new MailStreamResult(new BlockingCollection<MailSendResult>(), CreateCanceledTask());
        };

        It Should_not_be_finished = () => mSut.IsFinished.ShouldBeFalse();
        It Should_be_stopped = () => mSut.IsStopped.ShouldBeTrue();
        It Should_have_no_error = () => mSut.HasError.ShouldBeFalse();

        static Task CreateCanceledTask() {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            return tcs.Task;
        }
    }

    [Subject(typeof(MailSendResult))]
    public class When_an_error_has_occured_during_sending {
        static MailStreamResult mSut;


        Because of = () => {
            mSut = new MailStreamResult(new BlockingCollection<MailSendResult>(), CreateFaultedTask());
        };
        
        It Should_not_be_finished = () => mSut.IsFinished.ShouldBeFalse();
        It Should_not_be_stopped = () => mSut.IsStopped.ShouldBeFalse();
        It Should_have_an_error = () => mSut.HasError.ShouldBeTrue();
        It Should_have_an_exception = () => mSut.Exception.ShouldNotBeNull();


        static Task CreateFaultedTask() {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetException(new AggregateException());
            return tcs.Task;
        }
    }
}
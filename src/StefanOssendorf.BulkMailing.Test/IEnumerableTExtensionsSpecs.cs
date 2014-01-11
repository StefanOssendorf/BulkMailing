using System.Collections.Concurrent;
using System.Collections.Generic;
using Machine.Specifications;

namespace StefanOssendorf.BulkMailing.Test {
    [Subject(typeof(IEnumerableTExtensions))]
    public class When_converting_enumerable_to_blockingcollection {
        static BlockingCollection<int> mResult;
        static List<int> mSut;

        Establish context = () => mSut = new List<int> { 1, 2, 3, 4, 5 };
        Because of = () => mResult = mSut.ToBlockingCollection();

        It Should_create_a_blockingCollection = () => mResult.ShouldNotBeNull();
        It Should_add_all_items = () => mResult.ShouldContainOnly(mSut);
    }
}
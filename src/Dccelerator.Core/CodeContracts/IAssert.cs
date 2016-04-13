using System;
using System.Linq.Expressions;
using JetBrains.Annotations;


namespace Dccelerator.CodeContracts {

    /// <summary>
    /// Defines an method parameters and just a variables assetion helper.
    /// 
    /// Use it like
    /// 
    /// string error;
    /// if (!Guard.GetAsserter(someRequest)
    ///        .That(x => x.Document.Type.Id != Guid.Empty)
    ///        .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
    ///        .IsValid(out error)) {
    ///     return error;
    /// }
    /// 
    /// of with exception thrown:
    /// using (var assert = Guard.GetExceptionAsserter(someRequest) { 
    ///     assert
    ///        .That(x => x.Document.Type.Id != Guid.Empty)
    ///        .That(x => !string.IsNullOrWhiteSpace(x.Kind.Name)
    ///        .IsValid() 
    /// }
    /// 
    /// </summary>
    /// <seealso cref="Guard.GetAsserter{T}"/>
    /// <seealso cref="Guard.GetExceptionAsserter{T}"/>
    public interface IAssert<TRequest> : IDisposable {

        string ErrorMessage { get; }


        [MustUseReturnValue, NotNull]
        IAssert<TRequest> That(Expression<Func<TRequest, bool>> that);


        bool IsValid();


        [ContractAnnotation("=>true,error:null; =>false,error:notnull")]
        bool IsValid(out string error);

    }
}
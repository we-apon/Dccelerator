using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using Dccelerator.DataAccess;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.SqlClient;
using FakeItEasy;
using Machine.Specifications;

namespace Dccelerator.Specifications.Shared.DataAccess.Adapters.MsSql {

    [Subject(typeof(SqlClientRepository))]
    class When_getting_select_query {
        Establish context = () => {
            _repository = A.Fake<SqlClientRepository>(options => options.CallsBaseMethods());

            _info = A.Fake<IAdoEntityInfo>();
            A.CallTo(() => _info.EntityName).Returns(_entityName);

            _criteria = new[] {
                new DataCriterion {Name = "Name1"},
                new DataCriterion {Name = "Name2"},
                new DataCriterion {Name = "Name3"},
            };
        };

        Because of = () => _query = _repository.SelectQuery(_info, _criteria);

        It should_be_correct = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            actualWords.ShouldEqual(expectedWords);
        };

        static SqlClientRepository _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static IDataCriterion[] _criteria;
        static string _query;

        static string _validQuery = "select * " +
                                    "from EntName " +
                                    "where Name1 = @Name1 " +
                                    "and Name2 = @Name2 " +
                                    "and Name3 = @Name3";
    }


    [Subject(typeof(SqlClientRepository))]
    class When_getting_insert_query {
        Establish context = () => {
            _repository = A.Fake<SqlClientRepository>(options => options.CallsBaseMethods());

            _info = A.Fake<IAdoEntityInfo>();
            A.CallTo(() => _info.EntityName).Returns(_entityName);
            A.CallTo(() => _info.PersistedProperties).Returns(new Dictionary<string, PropertyInfo>() {
                {"Name1", null},
                {"Name2", null},
                {"Name3", null},
            });

            _entity = new object();
        };

        Because of = () => _query = _repository.InsertQuery(_info, _entity);

        It should_be_correct = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            actualWords.ShouldEqual(expectedWords);
        };

        static object _entity;
        static SqlClientRepository _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static string _query;

        static string _validQuery = "insert into EntName ( Name1, Name2, Name3 ) " +
                                    "values ( @Name1, @Name2, @Name3 ) ";
    }


    [Subject(typeof(SqlClientRepository))]
    class When_getting_update_query {
        Establish context = () => {
            _repository = A.Fake<SqlClientRepository>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.PrimaryKeyParameterOf(A<IAdoEntityInfo>.Ignored, A<object>.Ignored))
                .Returns(new SqlParameter() {ParameterName = "Id"});

            _info = A.Fake<IAdoEntityInfo>();
            A.CallTo(() => _info.EntityName).Returns(_entityName);
            A.CallTo(() => _info.PersistedProperties).Returns(new Dictionary<string, PropertyInfo>() {
                {"Id", null},
                {"Name1", null},
                {"Name2", null},
                {"Name3", null},
            });

            _entity = new object();
        };

        Because of = () => _query = _repository.UpdateQuery(_info, _entity);

        It should_be_correct = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            actualWords.ShouldEqual(expectedWords);
        };

        static object _entity;
        static SqlClientRepository _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static string _query;

        static string _validQuery = "update EntName " +
                                    "set Name1 = @Name1, " +
                                    "Name2 = @Name2, " +
                                    "Name3 = @Name3 " +
                                    "where Id = @Id";
    }

    [Subject(typeof(SqlClientRepository))]
    class When_getting_delete_query {
        Establish context = () => {
            _repository = A.Fake<SqlClientRepository>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.PrimaryKeyParameterOf(A<IAdoEntityInfo>.Ignored, A<object>.Ignored))
                .Returns(new SqlParameter() {ParameterName = "Id"});

            _info = A.Fake<IAdoEntityInfo>();
            A.CallTo(() => _info.EntityName).Returns(_entityName);
            A.CallTo(() => _info.PersistedProperties).Returns(new Dictionary<string, PropertyInfo>() {
                {"Id", null},
                {"Name1", null},
                {"Name2", null},
                {"Name3", null},
            });

            _entity = new object();
        };

        Because of = () => _query = _repository.DeleteQuery(_info, _entity);

        It should_be_correct = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            actualWords.ShouldEqual(expectedWords);
        };

        static object _entity;
        static SqlClientRepository _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static string _query;

        static string _validQuery = "delete from EntName " +
                                    "where Id = @Id";
    }


}
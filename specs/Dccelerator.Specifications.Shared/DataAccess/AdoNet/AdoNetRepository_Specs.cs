#if DEBUG

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Dccelerator.DataAccess;
using Dccelerator.DataAccess.Ado;
using Dccelerator.DataAccess.Ado.SqlClient;
using Dccelerator.UnRandomObjects;
using FakeItEasy;
using Machine.Specifications;

namespace Dccelerator.Specifications.Shared.DataAccess.AdoNet {

    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_read_command_text_using_queries {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();
            _dataCriteria = new IDataCriterion[StaticRandom.Next(0, 9)];

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.SelectQuery(A<IAdoEntityInfo>.Ignored, A<IEnumerable<IDataCriterion>>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(true);
        };

        Because of = () => _query = _repository.ReadCommandText(_info, _dataCriteria);

        It should_return_select_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_SelectQuery_method = () => A.CallTo(() => _repository.SelectQuery(_info, _dataCriteria)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static IDataCriterion[] _dataCriteria;
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_read_command_text_using_procedures {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();
            _dataCriteria = new IDataCriterion[StaticRandom.Next(0, 9)];

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.SelectProcedure(A<IAdoEntityInfo>.Ignored, A<IEnumerable<IDataCriterion>>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(false);
        };

        Because of = () => _query = _repository.ReadCommandText(_info, _dataCriteria);

        It should_return_select_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_SelectQuery_method = () => A.CallTo(() => _repository.SelectProcedure(_info, _dataCriteria)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static IDataCriterion[] _dataCriteria;
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_insert_command_text_using_queries {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.InsertQuery(A<IAdoEntityInfo>.Ignored, A<string>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(true);
        };

        Because of = () => _query = _repository.InsertCommandText(_info, _entityName);

        It should_return_insert_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_InsertQuery_method = () => A.CallTo(() => _repository.InsertQuery(_info, _entityName)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static string _entityName = "EntName";
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_insert_command_text_using_procedures {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.InsertProcedure(A<IAdoEntityInfo>.Ignored, A<string>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(false);
        };

        Because of = () => _query = _repository.InsertCommandText(_info, _entityName);

        It should_return_insert_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_InsertQuery_method = () => A.CallTo(() => _repository.InsertProcedure(_info, _entityName)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static string _entityName = "EntName";
    }



    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_update_command_text_using_queries {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.UpdateQuery(A<IAdoEntityInfo>.Ignored, A<string>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(true);
        };

        Because of = () => _query = _repository.UpdateCommandText(_info, _entityName);

        It should_return_update_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_UpdateQuery_method = () => A.CallTo(() => _repository.UpdateQuery(_info, _entityName)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static string _entityName = "EntName";
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_update_command_text_using_procedures {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.UpdateProcedure(A<IAdoEntityInfo>.Ignored, A<string>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(false);
        };

        Because of = () => _query = _repository.UpdateCommandText(_info, _entityName);

        It should_return_update_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_UpdateQuery_method = () => A.CallTo(() => _repository.UpdateProcedure(_info, _entityName)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static string _entityName = "EntName";
    }

    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_delete_command_text_using_queries {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.DeleteQuery(A<IAdoEntityInfo>.Ignored, A<string>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(true);
        };

        Because of = () => _query = _repository.DeleteCommandText(_info, _entityName);

        It should_return_delete_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_DeleteQuery_method = () => A.CallTo(() => _repository.DeleteQuery(_info, _entityName)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static string _entityName = "EntName";
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_delete_command_text_using_procedures {
        Establish context = () => {
            _expectedQuery = RandomMaker.MakeString();

            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.DeleteProcedure(A<IAdoEntityInfo>.Ignored, A<string>.Ignored)).Returns(_expectedQuery);

            _info = A.Fake<IAdoEntityInfo>();

            A.CallTo(() => _info.UsingQueries).Returns(false);
        };

        Because of = () => _query = _repository.DeleteCommandText(_info, _entityName);

        It should_return_delete_query = () => _query.ShouldEqual(_expectedQuery);

        It should_call_DeleteQuery_method = () => A.CallTo(() => _repository.DeleteProcedure(_info, _entityName)).MustHaveHappened(Repeated.Exactly.Once);

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _query;
        static string _expectedQuery;
        static string _entityName = "EntName";
    }




    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_select_query {
        Establish context = () => {
            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.DatabaseSpecificNameOf(A<string>.Ignored)).ReturnsLazily(call => "##" + call.Arguments[0]);

            _info = A.Fake<IAdoEntityInfo>();
            A.CallTo(() => _info.EntityName).Returns(_entityName);

            _criteria = new[] {
                new DataCriterion {Name = "Name1"},
                new DataCriterion {Name = "Name2"},
                new DataCriterion {Name = "Name3"},
            };
        };

        Because of = () => _query = _repository.SelectQuery(_info, _criteria);

        It should_return_correct_sql_query = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            actualWords.ShouldEqual(expectedWords);
        };

        It should_use_database_specific_parameter_names = () => {
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name1")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name2")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name3")).MustHaveHappened(Repeated.Exactly.Once);
        };

        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static IDataCriterion[] _criteria;
        static string _query;

        static string _validQuery = "select * " +
                                    "from EntName " +
                                    "where Name1 = ##Name1 " +
                                    "and Name2 = ##Name2 " +
                                    "and Name3 = ##Name3";
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_insert_query {
        Establish context = () => {
            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.DatabaseSpecificNameOf(A<string>.Ignored)).ReturnsLazily(call => "##" + call.Arguments[0]);

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

        It should_return_correct_sql_query = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            actualWords.ShouldEqual(expectedWords);
        };

        It should_use_database_specific_parameter_names = () => {
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name1")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name2")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name3")).MustHaveHappened(Repeated.Exactly.Once);
        };

        static object _entity;
        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static string _query;

        static string _validQuery = "insert into EntName ( Name1, Name2, Name3 ) " +
                                    "values ( ##Name1, ##Name2, ##Name3 ) ";
    }


    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_update_query {
        Establish context = () => {
            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.DatabaseSpecificNameOf(A<string>.Ignored)).ReturnsLazily(call => "##" + call.Arguments[0]);

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

        It should_return_correct_sql_query = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            actualWords.ShouldEqual(expectedWords);
        };

        It should_use_database_specific_parameter_names = () => {
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Id")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name1")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name2")).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Name3")).MustHaveHappened(Repeated.Exactly.Once);
        };

        static object _entity;
        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static string _query;

        static string _validQuery = "update EntName " +
                                    "set Name1 = ##Name1, " +
                                    "Name2 = ##Name2, " +
                                    "Name3 = ##Name3 " +
                                    "where Id = ##Id";
    }

    [Subject(typeof(AdoNetRepository<DbCommand, DbParameter, DbConnection>))]
    class When_getting_delete_query {
        Establish context = () => {
            _repository = A.Fake<AdoNetRepository<DbCommand, DbParameter, DbConnection>>(options => options.CallsBaseMethods());
            A.CallTo(() => _repository.DatabaseSpecificNameOf(A<string>.Ignored)).ReturnsLazily(call => "##" + call.Arguments[0]);

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

        It should_return_correct_sql_query = () => {
            Console.WriteLine($"Expected:\n{_validQuery}\nActual:\n{_query}");

            var expectedWords = _validQuery.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var actualWords = _query.Split(new[] {' ', '\n', '\t'}, StringSplitOptions.RemoveEmptyEntries);

            actualWords.ShouldEqual(expectedWords);
        };

        It should_use_database_specific_parameter_names = () => {
            A.CallTo(() => _repository.DatabaseSpecificNameOf("Id")).MustHaveHappened(Repeated.Exactly.Once);
        };

        static object _entity;
        static AdoNetRepository<DbCommand, DbParameter, DbConnection> _repository;
        static IAdoEntityInfo _info;
        static string _entityName = "EntName";
        static string _query;

        static string _validQuery = "delete from EntName " +
                                    "where Id = ##Id";
    }


}

#endif
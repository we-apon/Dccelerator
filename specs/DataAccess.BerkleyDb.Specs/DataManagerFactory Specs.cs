using System;
using System.IO;
using Dccelerator.DataAccess;
using Dccelerator.DataAccess.BerkeleyDb;
using Machine.Specifications;
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming


namespace With_Dccelerator.DataAccess.BerkleyDb_assembly {

    namespace When_using_BDbDataManagerFactory {

        class SomeEntity {}


        class And_resolving_getter_for_some_entity {
            Establish context = () => _factory = new BDbDataManagerFactory(_environmentPath, _dbFilePath, "asddasda");

            Because of = () => _getter = _factory.GetterFor<SomeEntity>();

            It should_return_BdbDataGetter = () => {
                _getter.ShouldNotBeNull();
                _getter.ShouldBeAssignableTo<BDbDataGetter<SomeEntity>>();
            };


            It should_pass_environment_and_database_file_path_to_data_getter = () => {
                
            };

            static BDbDataManagerFactory _factory;
            static IDataGetter<SomeEntity> _getter;
            static readonly string _environmentPath = AppDomain.CurrentDomain.BaseDirectory;
            static readonly string _dbFilePath = Path.Combine(_environmentPath, $"{nameof(And_resolving_getter_for_some_entity)}.bdb");
        }

    }
}
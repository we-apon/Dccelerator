using System;
using Dccelerator.DataAccess.Ado.ReadingRepositories;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.Ado {
    class AdoNetEntityInfo : EntityInfo {
        public AdoNetEntityInfo(Type type) : base(type) {}
      
        public override IInternalReadingRepository GlobalReadingRepository => _readingRepository ?? (_readingRepository = AllowCache ? new CachedReadingRepository() : new DirectReadingRepository());
        IInternalReadingRepository _readingRepository;

    }
}
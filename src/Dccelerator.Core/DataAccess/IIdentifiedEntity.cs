
using System;


namespace Dccelerator.DataAccess.Lazy
{
    public interface IIdentifiedEntity
    {
        Guid Id { get; set; }
    }
}
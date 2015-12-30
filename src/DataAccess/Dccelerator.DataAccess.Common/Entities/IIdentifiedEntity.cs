
using System;


namespace Dccelerator.DataAccess.Entities
{
    public interface IIdentifiedEntity
    {
        Guid Id { get; set; }
    }
}
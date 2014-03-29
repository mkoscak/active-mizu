using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageImporter.Entities
{
    public interface IEntity
    {
        string GetTableName();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities.Interfaces
{
    public interface IEntity
    {
        int Id { get; set; } // Уникальный идентификатор
        string Name { get; set; }
    }
}

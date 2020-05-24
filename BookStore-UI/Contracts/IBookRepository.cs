using BookStore_UI.Models;
using BookStore_UI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_UI.Contracts
{
    public interface IBookRepository : IBaseRepository<Book>
    {

    }
}

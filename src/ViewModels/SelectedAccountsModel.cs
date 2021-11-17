using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IS_distance_learning.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_distance_learning.ViewModels
{
    [Keyless]
    public class SelectedAccountsModel
    {
        public List<Account> SelectedAccounts { get; set; }
    }
}

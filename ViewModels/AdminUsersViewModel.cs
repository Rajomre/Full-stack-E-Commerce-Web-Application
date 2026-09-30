using Shoping_webapplication_project_.Models;
using System.Collections.Generic;

namespace Shoping_webapplication_project_.ViewModels
{
    public class AdminUsersViewModel
    {
        public List<User> Users { get; set; }

        public string Search { get; set; }
        public string Role { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }


    }
}
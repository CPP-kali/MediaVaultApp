
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieAppMySQL
{
    public static class CurrentUser
    {
        public static User LoggedInUser { get; set; }  // Propiedad para almacenar el usuario actual
    }
}

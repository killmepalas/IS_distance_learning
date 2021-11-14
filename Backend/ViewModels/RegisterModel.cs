using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.ViewModels
{
    [Keyless]
    public class RegisterModel
    {
        [Required(ErrorMessage = "Не указан Login")]
        [MaxLength(30)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Не указано Имя")]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указано Отчество")]
        [MaxLength(30)]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Не указано Фамилия")]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Не указана роль")]
        public int RoleId { get; set; }
        public string Role { get; set; }
    }
}

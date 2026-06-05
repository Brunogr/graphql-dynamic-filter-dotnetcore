namespace DynamicQuery.AspNetCore.Test.Models
{
    public class User
    {
        private User()
        {
        }

        public User(string name, int age, Address address)
        {
            Name = name;
            Age = age;
            Address = address;
            Date = DateTime.Now;
        }

        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public Address Address { get; set; } = null!;
        public List<Role> Roles { get; set; } = new List<Role>();

        private DateTime _birthDate;
        private DateTimeOffset Date { get; set; }

        public DateTime BirthDate
        {
            get => DateTime.Now.AddYears(-Age);
            set => _birthDate = value;
        }
    }

    public class Role
    {
        public Role(int? id, string name)
        {
            Id = id;
            Name = name;
        }

        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Address
    {
        public Address(string street, int? number, Zip zip)
        {
            Street = street;
            Number = number;
            Zip = zip;
        }

        public string Street { get; set; } = string.Empty;
        public int? Number { get; set; }
        public Zip Zip { get; private set; } = null!;
    }

    public class Zip
    {
        public Zip(int number, string country)
        {
            Number = number;
            Country = country;
        }

        public string Country { get; set; } = string.Empty;
        public int Number { get; set; }
    }

    public static class UserData
    {
        public static List<User> Users { get; } = new List<User>
        {
            new User("Bruno", 27, new Address("street 1", 23, new Zip(123456, "USA")))
            {
                Roles = new List<Role> { new Role(1, "Admin") }
            },
            new User("Fred", 33, new Address("street 2", null, new Zip(1234567, "BR")))
            {
                Roles = new List<Role> { new Role(2, "Admin") }
            },
            new User("Albert", 37, new Address("street 3", 43, new Zip(54375445, "BR")))
            {
                Roles = new List<Role> { new Role(null, "Read"), new Role(3, "Write") }
            },
            new User("Lucao", 23, new Address("street 4", 53, new Zip(76878979, "PT")))
            {
                Roles = new List<Role> { new Role(4, "Read"), new Role(5, "Write") }
            },
            new User("Luide", 28, new Address("street 5", 63, new Zip(65756443, "PT")))
            {
                Roles = new List<Role> { new Role(6, "Read"), new Role(7, "Write") }
            }
        };
    }
}

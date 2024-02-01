using System;

namespace LegacyApp.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            ProveAddUser(args);
        }

        public static void ProveAddUser(string[] args)
        {
            /*
			 *	DO NOT CHANGE THIS FILE AT ALL
        	*/

            var userService = new UserService();
            var addResult = userService.AddUser("Bruno", "Camba", "bruno.camba@gmail.com", new DateTime(1993, 1, 1), 4);
            Console.WriteLine("Adding Bruno Camba was " + (addResult ? "successful" : "unsuccessful"));
        }
    }
}

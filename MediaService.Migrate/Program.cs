using System;

namespace MediaService.Migrate
{
   public static class Program
   {
      static void Main(string[] args)
      {
         Migrator.Migrate();
         Console.WriteLine("Finished");
         Console.ReadLine();
      }
   }
}
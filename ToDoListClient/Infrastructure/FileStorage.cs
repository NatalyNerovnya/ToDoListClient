using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ToDoListClient.Models;

namespace ToDoListClient.Infrastructure
{
    public static class FileStorage
    {
        private static readonly string storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"Debug\\".ToCharArray()), "storage.txt");

        /// <summary>
        /// Update file in memory
        /// </summary>
        public static void UpdateFile(List<ToDoItemViewModel> listOfItems)
        {
            if (!File.Exists(storagePath))
            {
                File.Create(storagePath).Close();
            }
            string json = JsonConvert.SerializeObject(listOfItems);
            File.WriteAllText(storagePath, json);
        }

        /// <summary>
        /// Get data from the file
        /// </summary>
        /// <returns>List of items</returns>
        public static IList<ToDoItemViewModel> GetData()
        {
            if (!File.Exists(storagePath))
                return null;
            return JsonConvert.DeserializeObject<IList<ToDoItemViewModel>>(File.ReadAllText(storagePath));
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ToDoListClient.Models;
using ToDoListClient.Services;

namespace ToDoListClient.Controllers
{
    /// <summary>
    /// Processes todolocal requests.
    /// </summary>
    public class LocalController : ApiController
    {
        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();



        private static int counter = 0;

        private static List<ToDoItemViewModel> listOfItems;
        private static Dictionary<int?, int?> ids;
        private static List<int> updateIds = new List<int>();
        private static List<int> deleteId = new List<int>();
        private readonly string storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(@"Debug\\".ToCharArray()), "storage.txt");

        /// <summary>
        /// Returns all todolocal-items for the current user.
        /// </summary>
        /// <returns>The list of todolocal-items.</returns>
        public IList<ToDoItemViewModel> Get()
        {
            if (listOfItems == null)
            {
                var userId = userService.GetOrCreateUser();
                var items = todoService.GetItems(userId);
                ids = new Dictionary<int?, int?>();
                listOfItems = new List<ToDoItemViewModel>();
                foreach (var item in items)
                {
                    listOfItems.Add(new ToDoItemViewModel()
                    {
                        Name = item.Name,
                        IsCompleted = item.IsCompleted,
                        ToDoId = item.ToDoId,
                        UserId = item.UserId,
                        ToDoLocalId = listOfItems.Count
                    });
                    ids.Add(listOfItems.Count - 1, item.ToDoId);
                }
                UpdateFile();
            }
            return listOfItems;
        }

        /// <summary>
        /// Updates the existing todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal-item to update.</param>
        public void Put(int id )
        {
            if (counter == 1)
            {
                UpdateCloude();
                counter = 0;
            }
            listOfItems.Find(m => m.ToDoLocalId == id).IsCompleted = !listOfItems.Find(m => m.ToDoLocalId == id).IsCompleted;
            updateIds.Add(id);
            UpdateFile();
            counter++;
        }

        /// <summary>
        /// Deletes the specified todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal item identifier.</param>
        public void Delete(int id)
        {
            if (counter == 2)
            {
                UpdateCloude();
                counter = 0;
            }
            var item = listOfItems.Find(m => m.ToDoLocalId == id);
            listOfItems.Remove(item);
            deleteId.Add(id);
            UpdateFile();
            counter++;
        }

        /// <summary>
        /// Creates a new todolocal-item.
        /// </summary>
        /// <param name="todo">The todolocal-item to create.</param>
        public void Post(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            listOfItems.Add(new ToDoItemViewModel()
            {
                Name = todo.Name,
                IsCompleted =  todo.IsCompleted,
                UserId = todo.UserId,
                ToDoLocalId = listOfItems.Count
            });
            ids.Add(listOfItems.Count - 1, null);
            UpdateFile();
        }


        private void UpdateFile()
        {
            if (!File.Exists(storagePath))
            {
                File.Create(storagePath).Close();
            }
            string json = JsonConvert.SerializeObject(listOfItems);
            File.WriteAllText(storagePath, json);
        }
        private void UpdateCloude()
        {
            var dictionaryCash = ids.ToDictionary(k => k.Key, v => v.Value);
            var itemsCash = listOfItems.ToArray();
            ToDosController controller = new ToDosController();
            var itemsInCloude = controller.Get();

            for (int i = 0; i < listOfItems.Count; i++)
            {
                var item = itemsCash[i];

                if (deleteId.Contains(i))
                {
                    controller.Delete(item.ToDoId);
                    continue;
                }

                if (ids[i] == null)
                {
                    controller.Post(new ToDoItemViewModel()
                    {
                        Name = item.Name,
                        IsCompleted = item.IsCompleted,
                        ToDoLocalId = i,
                        UserId = item.UserId
                    });
                    itemsInCloude = controller.Get();
                    ids[i] = itemsInCloude.Last().ToDoId;
                }

                
                if (updateIds.Contains(i))
                {
                    controller.Put(item);
                }
            }
            updateIds = new List<int>();
            deleteId = new List<int>();
        }
    }
}

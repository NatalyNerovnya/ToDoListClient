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
                CreateLocalList();
            }
            UpdateFile();
            return listOfItems;
        }

        /// <summary>
        /// Updates the existing todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal-item to update.</param>
        public void Put(int id)
        {
            listOfItems.Find(m => m.ToDoLocalId == id).IsCompleted = !listOfItems.Find(m => m.ToDoLocalId == id).IsCompleted;
            updateIds.Add(id);
            UpdateFile();
        }

        /// <summary>
        /// Deletes the specified todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal item identifier.</param>
        public void Delete(int id)
        {
            var item = listOfItems.Find(m => m.ToDoLocalId == id);
            listOfItems.Remove(item);
            deleteId.Add(id);
            UpdateFile();
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
                IsCompleted = todo.IsCompleted,
                UserId = todo.UserId,
                ToDoLocalId = listOfItems.Count
            });
            ids.Add(listOfItems.Count - 1, null);
            UpdateFile();
        }

        /// <summary>
        /// Update file in memory
        /// </summary>
        private void UpdateFile()
        {
            if (!File.Exists(storagePath))
            {
                File.Create(storagePath).Close();
            }
            string json = JsonConvert.SerializeObject(listOfItems);
            File.WriteAllText(storagePath, json);
        }

        /// <summary>
        /// Create list from cloude or file
        /// </summary>
        private void CreateLocalList()
        {
            IList<ToDoItemViewModel> items;
            var userId = userService.GetOrCreateUser();
            if (!File.Exists(storagePath))
            {
                items = todoService.GetItems(userId);
            }
            else
            {
                items = JsonConvert.DeserializeObject<IList<ToDoItemViewModel>>(File.ReadAllText(storagePath));
            }

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
        }

        /// <summary>
        /// Uploud data
        /// </summary>
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

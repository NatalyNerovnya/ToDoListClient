using System;
using System.Collections.Generic;
using System.IO;
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
        private static Dictionary<int, int?> ids;  
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
                ids = new Dictionary<int, int?>();
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
            if (!File.Exists(storagePath))
            {
                File.Create(storagePath).Close();
            }
            string json = JsonConvert.SerializeObject(listOfItems);
            File.WriteAllText(storagePath, json);
            return listOfItems;
        }

        /// <summary>
        /// Updates the existing todolocal-item.
        /// </summary>
        /// <param name="todo">The todolocal-item to update.</param>
        public void Put(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            listOfItems.Find(m => m.ToDoLocalId == todo.ToDoLocalId).IsCompleted = !todo.IsCompleted;
            //todoService.UpdateItem(todo);
        }

        /// <summary>
        /// Deletes the specified todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal item identifier.</param>
        public void Delete(int id)
        {
            var item = listOfItems.Find(m => m.ToDoLocalId == id);
            ids[id] = null;
            listOfItems.Remove(item);
            //todoService.DeleteItem(id);
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
            //todoService.CreateItem(todo);
        }
    }
}

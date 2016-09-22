using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ToDoListClient.Models;
using ToDoListClient.Services;
using System.Threading.Tasks;
using ToDoListClient.Infrastructure;

namespace ToDoListClient.Controllers
{
    /// <summary>
    /// Processes todolocal requests.
    /// </summary>
    public class LocalController : ApiController
    {
        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();

        /// <summary>
        /// Returns all todolocal-items for the current user.
        /// </summary>
        /// <returns>The list of todolocal-items.</returns>
        public IList<ToDoItemViewModel> Get()
        {
            if (CollectionStorage.ListOfItems == null)
            {
                var userId = userService.GetOrCreateUser();
                CollectionStorage.CreateLocalList(userId, todoService);
                FileStorage.UpdateFile(CollectionStorage.ListOfItems);
            }

            return CollectionStorage.ListOfItems;
        }

        /// <summary>
        /// Updates the existing todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal-item to update.</param>
        public void Put(int id)
        {
            CollectionStorage.ListOfItems.Find(m => m.ToDoLocalId == id).IsCompleted = !CollectionStorage.ListOfItems.Find(m => m.ToDoLocalId == id).IsCompleted;
            CollectionStorage.UpdateIds.Add(id);
            FileStorage.UpdateFile(CollectionStorage.ListOfItems);
        }

        /// <summary>
        /// Deletes the specified todolocal-item.
        /// </summary>
        /// <param name="id">The todolocal item identifier.</param>
        public void Delete(int id)
        {
            var item = CollectionStorage.ListOfItems.Find(m => m.ToDoLocalId == id);
            CollectionStorage.ListOfItems.Remove(item);
            CollectionStorage.DeleteId.Add(id);
            CollectionStorage.Ids.Remove(id);
            FileStorage.UpdateFile(CollectionStorage.ListOfItems);
        }

        /// <summary>
        /// Creates a new todolocal-item.
        /// </summary>
        /// <param name="todo">The todolocal-item to create.</param>
        public void Post(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            CollectionStorage.ListOfItems.Add(new ToDoItemViewModel()
            {
                Name = todo.Name,
                IsCompleted = todo.IsCompleted,
                UserId = todo.UserId,
                ToDoLocalId = CollectionStorage.ListOfItems.Count
            });
            CollectionStorage.Ids.Add(CollectionStorage.Ids.Last().Key + 1, null);
            FileStorage.UpdateFile(CollectionStorage.ListOfItems);
        }
    }
}

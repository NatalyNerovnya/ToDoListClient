using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoListClient.Controllers;
using ToDoListClient.Models;
using ToDoListClient.Services;

namespace ToDoListClient.Infrastructure
{
    public static class CollectionStorage
    {
        public static List<ToDoItemViewModel> ListOfItems { get; set; }
        public static Dictionary<int?, int?> Ids { get; set; }
        public static List<int> UpdateIds { get; set; }
        public static List<int> DeleteId { get; set; }

        static CollectionStorage()
        {
            UpdateIds = new List<int>();
            DeleteId = new List<int>();
        }


        /// <summary>
        /// Create list from cloude or file
        /// </summary>
        public static void CreateLocalList(int userId, ToDoService todoService)
        {
            IList<ToDoItemViewModel> items;
            items = FileStorage.GetData() == null ? todoService.GetItems(userId) : FileStorage.GetData();

            Ids = new Dictionary<int?, int?>();
            ListOfItems = new List<ToDoItemViewModel>();
            foreach (var item in items)
            {
                ListOfItems.Add(new ToDoItemViewModel()
                {
                    Name = item.Name,
                    IsCompleted = item.IsCompleted,
                    ToDoId = item.ToDoId,
                    UserId = item.UserId,
                    ToDoLocalId = ListOfItems.Count
                });
                Ids.Add(ListOfItems.Count - 1, item.ToDoId);
            }
        }

        /// <summary>
        /// Uploud data async
        /// </summary>
        private static void UpdateCloude()
        {
            var dictionaryCash = Ids.ToDictionary(k => k.Key, v => v.Value);
            var itemsCash = ListOfItems.ToArray();
            ToDosController controller = new ToDosController();
            var itemsInCloude = controller.Get();

            for (int i = 0; i < ListOfItems.Count; i++)
            {
                var item = itemsCash[i];

                if (DeleteId.Contains(i))
                {
                    controller.Delete(item.ToDoId);
                    continue;
                }

                if (Ids[i] == null)
                {
                    controller.Post(new ToDoItemViewModel()
                    {
                        Name = item.Name,
                        IsCompleted = item.IsCompleted,
                        ToDoLocalId = i,
                        UserId = item.UserId
                    });
                    itemsInCloude = controller.Get();
                    Ids[i] = itemsInCloude.Last().ToDoId;
                }

                if (UpdateIds.Contains(i))
                {
                    controller.Put(item);
                }
            }
            UpdateIds = new List<int>();
            DeleteId = new List<int>();
        }
    }
}
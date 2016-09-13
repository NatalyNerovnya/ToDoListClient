var tasksManager = function() {

    // appends a row to the tasks table.
    // @parentSelector: selector to append a row to.
    // @obj: task object to append.
    var appendRow = function(parentSelector, obj) {
        var tr = $("<tr data-id='" + obj.ToDoLocalId + "'></tr>");
        tr.append("<td><input type='checkbox' class='completed' " + (obj.IsCompleted ? "checked" : "") + "/></td>");
        tr.append("<td class='name' >" + obj.Name + "</td>");
        //tr.append("<td><input type='button' class='delete-button' value='Delete' /></td>");
        tr.append("<button type='button' class='btn btn-primary.delete-button'>Delete</button>");
        $(parentSelector).append(tr);
    }

    // adds all tasks as rows (deletes all rows before).
    // @parentSelector: selector to append a row to.
    // @tasks: array of tasks to append.
    var displayTasks = function(parentSelector, tasks) {
        $(parentSelector).empty();
        $.each(tasks, function(i, item) {
            appendRow(parentSelector, item);
        });
    };

    // starts loading tasks from server.
    // @returns a promise.
    var loadTasks = function () {
        return $.getJSON("/api/local");
    };

    // starts creating a task on the server.
    // @isCompleted: indicates if new task should be completed.
    // @name: name of new task.
    // @return a promise.
    var createTask = function (isCompleted, name, taskId) {

        localStorage.setObject(taskId, { name: name, isCompleted: isCompleted });
        var t = localStorage.getObject(taskId);
        return $.post("/api/local",
        {
            IsCompleted: isCompleted,
            Name: name
        });
    };

    // starts updating a task on the server.
    // @id: id of the task to update.
    // @isCompleted: indicates if the task should be completed.
    // @name: name of the task.
    // @return a promise.
    var updateTask = function(id, isCompleted, name) {
        return $.ajax(
        {
            url: "/api/local",
            type: "PUT",
            contentType: 'application/json',
            data: JSON.stringify({
                ToDoId: id,
                IsCompleted: isCompleted,
                Name: name
            })
        });
    };

    // starts deleting a task on the server.
    // @taskId: id of the task to delete.
    // @return a promise.
    var deleteTask = function (taskId) {
        return $.ajax({
            url: "/api/local/" + taskId,
            type: 'DELETE'
        });
    };

    // returns public interface of task manager.
    return {
        loadTasks: loadTasks,
        displayTasks: displayTasks,
        createTask: createTask,
        deleteTask: deleteTask,
        updateTask: updateTask
    };

}();


$(function () {
    //var store = new Storage();

    //// Add array to storage
    //var products = [
    //    { name: "Fish", price: 2.33 },
    //    { name: "Bacon", price: 1.33 }
    //];
    //store.set("products", products);

    //// Retrieve items from storage
    //store.get("products").then(tasksManager.loadTasks)
    //        .done(function (tasks) {
    //            tasksManager.displayTasks("#tasks > tbody", tasks);
    //        });

    // add new task button click handler
    $("#newCreate").click(function() {
        var isCompleted = $('#newCompleted')[0].checked;
        var name = $('#newName')[0].value;

        var taskBody = $("#tasks > tbody");
        var firstParent = $("#tasks > tbody").parent();
        var secondParent = firstParent.parent();
        var taskId = taskBody.parent().parent().attr("data-id");

        tasksManager.createTask(isCompleted, name, taskId)
            .then(tasksManager.loadTasks)
            .done(function(tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
            });
    });

    // bind update task checkbox click handler
    $("#tasks > tbody").on('change', '.completed', function () {
        var tr = $(this).parent().parent();
        var taskId = tr.attr("data-id");
        var isCompleted = tr.find('.completed')[0].checked;
        var name = tr.find('.name').text();
        
        tasksManager.updateTask(taskId, isCompleted, name)
            .then(tasksManager.loadTasks)
            .done(function (tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
            });
    });

    // bind delete button click for future rows
    $('#tasks > tbody').on('click', '.delete-button', function() {
        var taskId = $(this).parent().parent().attr("data-id");
        tasksManager.deleteTask(taskId)
            .then(tasksManager.loadTasks)
            .done(function(tasks) {
                tasksManager.displayTasks("#tasks > tbody", tasks);
            });
    });

    // load all tasks on startup
    tasksManager.loadTasks().done(tasksManager.loadTasks)
        .then(function (tasks) {
            tasksManager.displayTasks("#tasks > tbody", tasks);
        });
});

//function Storage() {

//    this.get = function (name) {
//        return JSON.parse(window.localStorage.getItem(name));
//    };

//    this.set = function (name, value) {
//        window.localStorage.setItem(name, JSON.stringify(value));
//    };

//    this.clear = function () {
//        window.localStorage.clear();
//    };
//}
window.agileApp = window.agileApp || {};

window.agileApp.datacontext = (function () {

	var datacontext = {
		getRoomList: getRoomList,
		createRoomItem: createRoomItem,
		saveNewRoomItem: saveNewRoomItem,
		saveChangedRoomItem: saveChangedRoomItem,
		deleteRoomItem: deleteRoomItem,
	};

	return datacontext;

	function getRoomList(roomListsObservable, errorObservable) {
		return ajaxRequest("get", roomItemUrl().concat("getRooms"))
            .done(getSucceeded)
            .fail(getFailed);

		function getSucceeded(data) {
			var mappedTodoLists = $.map(data, function (list) { return new createRoomItem(list); });
			roomListsObservable(mappedTodoLists);
		}

		function getFailed() {
			errorObservable("Error retrieving todo lists.");
		}
	}
	function createRoomItem(data) {
		return new datacontext.roomItem(data); // roomItem is injected by agile.model.js
	}
	function saveNewRoomItem(roomItem) {
		clearErrorMessage(roomItem);
		return ajaxRequest("post", roomItemUrl())
            .done(function (result) {
            	roomItem.roomItemId = result.roomItemId;
            })
            .fail(function () {
            	roomItem.errorMessage("Error adding a new todo item.");
            });
	}
	function deleteRoomItem(roomItem) {
		return ajaxRequest("delete", roomItemUrl(roomItem.id))
            .fail(function () {
            	roomItem.errorMessage("Error occured while removing room item.");
            });
	}
	function saveChangedRoomItem(roomItem) {
		clearErrorMessage(roomItem);
		return ajaxRequest("put", roomItemUrl(roomItem.roomItemId), roomItem, "text")
            .fail(function () {
            	roomItem.errorMessage("Error occured while updating room item.");
            });
	}
	function saveChangedRoomList(todoList) {
		clearErrorMessage(todoList);
		return ajaxRequest("put", todoListUrl(todoList.todoListId), todoList, "text")
            .fail(function () {
            	todoList.errorMessage("Error updating the todo list title. Please make sure it is non-empty.");
            });
	}
	// Private
	function clearErrorMessage(entity) { entity.errorMessage(null); }

	function ajaxRequest(type, url, data, dataType) { // Ajax helper
		console.log("Ajax request to the url: ".concat(url));

		var options = {
			dataType: dataType || "json",
			contentType: "application/json",
			cache: false,
			type: type,
			data: data ? data.toJson() : null
		};
		var antiForgeryToken = $("#antiForgeryToken").val();
		if (antiForgeryToken) {
			options.headers = {
				'RequestVerificationToken': antiForgeryToken
			}
		}
		return $.ajax(url, options);
	}
	// routes
	function roomItemUrl(id) { return "/api/room/" + (id || ""); }

})();
window.agileApp.accountViewModel = (function (ko, datacontext) {
	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var allPriviliges = ko.observableArray(),
        error = ko.observable(),
        addRoomItem = function () {
        	var roomItem = datacontext.createRoomItem();
        	roomItem.active(false);
        	roomItem.name("Add name here..");
        	roomItem.description("");
        	datacontext.saveNewRoomItem(roomItem)
                .then(addSucceeded)
                .fail(addFailed);

        	function addSucceeded() {
        		showRoomList(todoList);
        	}
        	function addFailed() {
        		error("Save of new roomList failed");
        	}
        },
        showRoomList = function (roomItem) {
        	roomList.unshift(roomItem); // Insert new room at the front
        },

        deleteRoomItem = function (roomItem) {
        	roomList.remove(roomItem);
        	datacontext.deleteRoomItem(roomItem)
                .fail(deleteFailed);

        	function deleteFailed() {
        		showRoomList(roomList); // re-show the restored list
        	}
        };

	datacontext.getRoomList(roomList, error); // load roomList

	return {
		roomList: roomList,
		error: error,
		addRoomItem: addRoomItem,
		deleteRoomItem: deleteRoomItem
	};

})(ko, agileApp.datacontext);

// Initiate the Knockout bindings
ko.applyBindings(window.agileApp.roomListViewModel);
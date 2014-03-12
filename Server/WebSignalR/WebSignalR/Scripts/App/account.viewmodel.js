window.agileApp.accountViewModel = (function (ko, datacontext, notifyService) {
	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var allPrivileges = ko.observableArray(),
        error = ko.observable(),
        addNewUser = function () {
        	var registerVM = datacontext.createRegisterVM();
        	datacontext.saveNewRoomItem(registerVM)
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
	// load privileges
	datacontext.getRoomList(roomList, error)
	.done(function (data) { })
	.fail(function () {
		if (window.agileApp.notifyService !== undefined) {
			window.agileApp.notifyService.error("Unable to retrieve privilege list.", null, true);
		}
		error("Error retrieving privileges.");
	});

	return {
		allPrivileges: allPrivileges,
		error: error,
		addNewUser: addNewUser,
		deleteRoomItem: deleteRoomItem
	};

})(ko, agileApp.datacontext, agileApp.notifyService);

// Initiate the Knockout bindings
ko.applyBindings(window.agileApp.roomListViewModel);
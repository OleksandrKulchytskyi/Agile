window.agileApp.roomListViewModel = (function (ko, datacontext, notify) {
	/// <field name="roomList" value="[new datacontext.roomList()]"></field>
	var roomList = ko.observableArray(),
        error = ko.observable(),
        addRoomItem = function () {
			var roomItem = datacontext.createRoomItem();
			console.log(roomItem);
			roomItem.active(false);
			roomItem.name("Name here..");
			roomItem.description("Description here");
			roomItem.errorMessage("");
			roomItem.isSaved(false);
			showRoomList(roomItem);
		},
		showRoomList = function (roomItem) {
			roomList.unshift(roomItem); // Insert new room at the front
		},

		deleteRoomItem = function (roomItem) {
			datacontext.deleteRoomItem(roomItem)
				.done(deleteSuccess)
		        .fail(deleteFailed);
			function deleteSuccess(data) {
				notify.success("Room " + data.Name + " has been deleted.", null, true);
				roomList.remove(function (room) { return room.id == data.Id });
			}
			function deleteFailed() {
				notify.error("Fail to delete room.", null, true);
			}
		},
		navigateToRoom = function (roomItem) {
			if (window.agileApp.baseAddress !== undefined) {
				console.log(roomItem.name());
				window.location.href = window.agileApp.baseAddress + "roomactivity?roomName=" + roomItem.name();
			}
		};

	notify.info("Loading rooms...", null, true);
	datacontext.getRoomList(roomList, error); // load roomList

	return {
		roomList: roomList,
		error: error,
		navigateToRoom: navigateToRoom,
		addRoomItem: addRoomItem,
		deleteRoomItem: deleteRoomItem
	};

})(ko, agileApp.datacontext, agileApp.notifyService);

// Initiate the Knockout bindings
ko.applyBindings(window.agileApp.roomListViewModel);

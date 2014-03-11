﻿window.agileApp.roomListViewModel = (function (ko, datacontext, notify) {
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
        		console.log(data);
        		roomList.remove(data);
        	}

        	function deleteFailed() {
        		notify.error("Fail to delete", null, true);
        	}
        };

	datacontext.getRoomList(roomList, error); // load roomList
	notify.info("Loading", null, true);

	return {
		roomList: roomList,
		error: error,
		addRoomItem: addRoomItem,
		deleteRoomItem: deleteRoomItem
	};

})(ko, agileApp.datacontext, agileApp.notifyService);

// Initiate the Knockout bindings
ko.applyBindings(window.agileApp.roomListViewModel);

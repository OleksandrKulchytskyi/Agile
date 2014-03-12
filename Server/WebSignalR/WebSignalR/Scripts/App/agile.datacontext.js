window.agileApp = window.agileApp || {};

window.agileApp.datacontext = (function () {

	var datacontext = {
		getRoomList: getRoomList,
		createRoomItem: createRoomItem,
		saveNewRoomItem: saveNewRoomItem,
		saveChangedRoomItem: saveChangedRoomItem,
		deleteRoomItem: deleteRoomItem,
		detachUserFromRoom: detachUserFromRoom,
		jsonify: objJsonify,
		getPrivileges: getPrivileges,
		createRegisterVM:initRegisterVM
	};

	return datacontext;

	function getRoomList(roomListsObservable, errorObservable) {
		var url = roomItemUrl().concat("getRooms")
		console.log(url);
		return ajaxRequest("get", url)
            .done(getSucceeded)
            .fail(getFailed);

		function getSucceeded(data) {
			var mappedRoomLists = $.map(data, function (list) {
				var item = new createRoomItem(list);
				item.isSaved(true);
				return item;
			});
			roomListsObservable(mappedRoomLists);
		}

		function getFailed() {
			if (window.agileApp.notifyService !== undefined) {
				window.agileApp.notifyService.error("Fail to retrieve room list.", null, true);
			}
			errorObservable("Error retrieving room lists.");
		}
	}
	function createRoomItem(data) {
		return new datacontext.roomItem(data); // roomItem is injected by agile.model.js
	}
	function saveNewRoomItem(newRoom) {
		clearErrorMessage(newRoom);
		return ajaxRequest("post", roomItemUrl().concat("createRoom/"), newRoom, "application/json");
	}
	function deleteRoomItem(roomItem) {
		clearErrorMessage(roomItem);
		return ajaxRequest("delete", roomItemUrl(roomItem.id))
            .fail(function () {
            	roomItem.errorMessage("Error occured while removing room item.");
            });
	}
	function saveChangedRoomItem(room) {
		clearErrorMessage(room);
		return ajaxRequest("put", roomItemUrl(room.id), room, "text")
            .fail(function () {
            	room.errorMessage("Error occured while updating room item.");
            });
	}

	function detachUserFromRoom(roomId, userId) {
		return ajaxRequest("put", roomItemUrl() + "LeaveRoom/?roomId=" + roomId + "&userId=" + userId)
		 .fail(function (XMLHttpRequest, textStatus, errorThrown) {
		 	console.log(errorThrown);
		 });
	}

	function getPrivileges() {
		var url = privilegeUrl().concat("getPrivileges");
		return ajaxRequest('get', url);
	}

	function initRegisterVM(data) {
		return new datacontext.registerVM(data);
	}

	// Private
	function objJsonify(data) {
		return ko.toJSON(data)
	}

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
	function roomItemUrl(id) { return getBaseUrl() + "api/room/" + (id || ""); }

	function getBaseUrl() {
		try {

			if (window.agileApp.baseAddress === undefined) {

				if (typeof String.prototype.endsWith !== 'function') {
					String.prototype.endsWith = function (suffix) {
						return this.indexOf(suffix, this.length - suffix.length) !== -1;
					};
				}

				var l = window.location;
				var base_url = l.protocol + "//" + l.host + "/" + l.pathname.split('/')[1];
				if (!base_url.endsWith("/")) {
					base_url = base_url + "/"
				}
				window.agileApp.baseAddress = base_url;
				console.log("Base url is:" + window.agileApp.baseAddress);
				return base_url;
			}
			else
				return window.agileApp.baseAddress
		}
		catch (arg) {
			return null;
		}
	}

	function privilegeUrl(id) { return getBaseUrl() + "api/privileges/" + (id || ""); }
})();
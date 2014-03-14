(function (ko, datacontext) {

	datacontext.roomItem = roomItem;
	datacontext.registerVM = registerVM;
	datacontext.privilegeVM = privilegeVM;
	datacontext.changePasswordVM = changePasswordVM;

	function roomItem(data) {
		var self = this;
		data = data || {};

		// Persisted properties
		self.id = data.Id;
		self.name = ko.observable(data.Name);
		self.description = ko.observable(data.Description);
		self.active = ko.observable(data.Active);

		self.connectedUsers = ko.observableArray(ko.utils.arrayMap(data.ConnectedUsers, function myfunction(userdata) {
			return new user(userdata, self);
		}));

		// Non-persisted properties
		self.errorMessage = ko.observable();
		self.isSaved = ko.observable(false);

		saveChanges = function () {
			if (self.isSaved() == true) {
				console.log("Modifying room.");
				return datacontext.saveChangedRoomItem(self);
			}
		};

		self.toJson = function () { return ko.toJSON(self) };

		// Auto-save when these properties change
		self.name.subscribe(saveChanges);
		self.description.subscribe(saveChanges);
		self.active.subscribe(saveChanges);

		self.saveNewRoom = function (room) {
			datacontext.saveNewRoomItem(self).done(savedCallback).fail(failHandler);

			function savedCallback(savedRoom) {
				window.agileApp.notifyService.success("Room has been saved.", null, true);
				console.log("On success " + savedRoom.Id);
				self.id = savedRoom.Id;
				self.isSaved(true);
			};
			function failHandler() {
				window.agileApp.notifyService.error("Fail to save room.", null, true);
				self.errorMessage("Unable to save room.");
			};
		}
	};

	function user(data, room) {
		var self = this;
		data = data || {};

		self.room = room;
		self.id = data.Id;
		self.name = ko.observable(data.Name);

		self.toJson = function () { return ko.toJSON(self) };

		self.detachUser = function (user, event) {
			var rid = self.room.id;
			var uid = self.id;
			console.log("User id: " + uid + " room id: " + rid);
			datacontext.detachUserFromRoom(rid, uid);
			self.room.connectedUsers.remove(user);
		}
	}

	function registerVM(data) {
		var self = this;
		data = data || {};

		self.id = data.Id;
		self.userName = ko.observable(data.UserName);
		self.password = ko.observable(data.Password);
		self.confirmPassword = ko.observable(data.ConfirmPassword);
		self.toJson = function () { return ko.toJSON(self) };

		self.registerUser = function () {
			return datacontext.registerNewUser(self);
		}
	}

	function changePasswordVM(data) {
		var self = this;
		data = data || {};

		self.password = ko.observable(data.Password);
		self.confirmPassword = ko.observable(data.ConfirmPassword);
		self.toJson = function () { return ko.toJSON(self) };

		self.change = function () {
			return datacontext.changeUserPassword(self);
		}
	}

	function privilegeVM(data) {
		var self = this;
		data = data || {};

		self.id = ko.observable(data.Id);
		self.name = ko.observable(data.Name);
		self.description = ko.observable(data.Description);
		self.toJson = function () { return ko.toJSON(self) };
	}

	// convert raw roomItem data objects into array of roomItems
	function importRoom(roomItem) {
		/// <returns value="[new roomItem()]"></returns>
		return datacontext.createRoomItem(roomItem);
	};
})(ko, agileApp.datacontext);
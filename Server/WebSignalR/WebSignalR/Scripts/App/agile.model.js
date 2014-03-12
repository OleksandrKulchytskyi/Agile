(function (ko, datacontext) {
	datacontext.roomItem = roomItem;
	datacontext.registerVM = registerVM;

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
			if (self.isSaved)
				return datacontext.saveChangedRoomItem(self);
		};

		// Auto-save when these properties change
		self.name.subscribe(saveChanges);
		self.description.subscribe(saveChanges);
		self.active.subscribe(saveChanges);

		self.saveNewRoom = function (room) {
			datacontext.saveNewRoomItem(self)
			.always(alwaysHandler);

			function alwaysHandler(output, status, xhr) {

				try {
					var res = output.getResponseHeader ? output.getResponseHeader.get('Id')
							: xhr.getResponseHeader.get('Id');
					console.log("Result is :" + res);
					console.log(output.getResponseHeader.get('Id'))
				}
				catch (error) {
					console.log(error);
				}

				if (output.status == 201) {
					self.id = res;
					self.errorMessage("");
					self.isSaved(true);
					if (window.agileApp.notifyService !== undefined) {
						window.agileApp.notifyService.success("Room has been saved.", null, true);
					}
				}
				else {
					self.errorMessage("Save of new room failed");
					if (window.agileApp.notifyService !== undefined) {
						window.agileApp.notifyService.success("Fail to save room.", null, true);
					}
				}
			}
		}

		self.toJson = function () { return ko.toJSON(self) };
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
	}

	// convert raw roomItem data objects into array of roomItems
	function importRoom(roomItem) {
		/// <returns value="[new roomItem()]"></returns>
		return datacontext.createRoomItem(roomItem);
	};
})(ko, agileApp.datacontext);
(function (ko, datacontext) {
	datacontext.roomItem = roomItem;

	function roomItem(data) {
		var self = this;
		data = data || {};
		console.log("Initiating roomItem");
		console.log(data);
		// Persisted properties
		self.id = data.Id;
		self.name = ko.observable(data.Name);
		self.description = ko.observable(data.Description);
		self.active = ko.observable(data.Active);
		self.connectedUsers = ko.observableArray(ko.utils.arrayMap(data.ConnectedUsers, function myfunction(userdata) {
			return new user(userdata);
		}));
		// Non-persisted properties
		self.errorMessage = ko.observable();

		saveChanges = function () {
			return datacontext.saveChangedRoomItem(self);
		};

		// Auto-save when these properties change
		self.name.subscribe(saveChanges);
		self.description.subscribe(saveChanges);
		self.active.subscribe(saveChanges);

		self.toJson = function () { return ko.toJSON(self) };
	};

	function user(data) {
		var self = this;
		console.log("Initiating user");
		console.log(data);
		data = data || {};
		self.id = ko.observable(data.Id);
		self.name = ko.observable(data.Name);

		self.toJson = function () { return ko.toJSON(self) };
	}

	// convert raw roomItem data objects into array of roomItems
	function importRoom(roomItem) {
		/// <returns value="[new roomItem()]"></returns>
		return datacontext.createRoomItem(roomItem);
	};
})(ko, agileApp.datacontext);
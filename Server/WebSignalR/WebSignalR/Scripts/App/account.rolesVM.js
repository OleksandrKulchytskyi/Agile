var RolesViewModel = function (data) {
	var self = this;
	data = data || {};

	self.users = ko.observableArray(ko.utils.arrayMap(data.UserList, function myfunction(userdata) {
		return new UserModel(userdata);
	}));

	self.roles = ko.observableArray(ko.utils.arrayMap(data.AvaliableRoles, function (roleData) {
		return new RoleModel(roleData);
	}));

	self.userRoles = ko.observableArray();
	self.addedRoles = ko.observableArray();
	self.removedRoles = ko.observableArray();

	self.selectedUser = ko.observable();
	self.selectedRole = ko.observable();

	self.addRole = function () {
		if (self.selectedUser() && self.selectedRole()) {
			var role = self.selectedRole();

			var existingRole = ko.utils.arrayFirst(self.userRoles(), function (item) {
				return item.id === role.id;
			});
			console.log(existingRole);
			if (existingRole == null) {
				self.userRoles.push(role);
				self.addedRoles.push(role);
			}
		}
	}

	getRolesForUser = function () {
		agileApp.datacontext.getUserPrivileges(self.selectedUser().id).
		done(function (data) {
			clearTemp();
			var mappedPriv = $.map(data, function (role) {
				return new RoleModel(role);
			});

			self.userRoles(mappedPriv);
			agileApp.notifyService.success("Successfully retrieved user roles.", {}, true);
		})
		.fail(function (e) {
			clearTemp();
			agileApp.notifyService.error("Unable to retrieve user roles", {}, true);
		});
	};

	function clearTemp() {
		self.addedRoles([]);
		self.removedRoles([]);
	}

	appendUserRole = function myfunction() {
		if (self.addedRoles().length > 0) {
			var role = self.addedRoles()[0];
			console.log(role);
			agileApp.datacontext.addUserPrivilege(self.selectedUser().id, role.id).
			done(function (data) {
				agileApp.notifyService.success("User role has been added.", {}, true);
			}).
			fail(function () {
				agileApp.notifyService.warning("Unable to append user role.", {}, true);
			});
		}
	}

	self.selectedUser.subscribe(getRolesForUser);
	self.addedRoles.subscribe(appendUserRole);
};


var UserModel = function (data) {

	var self = this;
	data = data || {};

	self.id = data.Id;
	self.name = data.Name;

}

var RoleModel = function (data) {

	var self = this;
	data = data || {};

	self.id = data.Id;
	self.name = data.Name;
	self.description = data.Description;
}
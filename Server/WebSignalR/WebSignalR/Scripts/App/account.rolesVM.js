var RolesViewModel = function (data) {
	var self = this;
	data = data || {};

	self.users = ko.observableArray(ko.utils.arrayMap(data.UserList, function myfunction(userdata) {
		return new UserModel(userdata);
	}));

	self.roles = ko.observableArray(ko.utils.arrayMap(data.AvaliableRoles, function (roleData) {
		return new RoleModel(roleData);
	}));

	self.addRole = function (user, rolesId) {

	}
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
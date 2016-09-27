/// <reference path="../_references.js" />

function DashboardViewModel() {
  var self = this;
   
  self.domains = ko.observableArray([]);
  self.docs = ko.observable();

  var summary = $.getJSON('api/summary');
  summary.done(function (data) {
    self.domains(data);
  })
}
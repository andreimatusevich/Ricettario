// Bind twitter typeahead
function createTypeahead(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
    var el = $(element);
    var allBindings = allBindingsAccessor();
    var substringMatcher = function (array, displayKey) {
        return function findMatches(q, cb) {
            var matches, substrRegex, contrRegex;

            // an array that will be populated with substring matches
            matches = [];

            // regex used to determine if a string contains the substring `q`
            substrRegex = new RegExp(q, 'i');
            contrRegex = new RegExp(getContrString(q), 'i');

            // iterate through the pool of strings and for any string that
            // contains the substring `q`, add it to the `matches` array
            $.each(array, function (i, item) {
                var key = item[displayKey];
                if (substrRegex.test(key) || contrRegex.test(key)) {
                    // the typeahead jQuery plugin expects suggestions to a
                    // JavaScript object, refer to typeahead docs for more info
                    matches.push(item);
                }
            });


            cb(matches);
        };
    };
    var typeaheadOpts = {};

    //if (allBindings.typeaheadOptions) {
    //    $.each(allBindings.typeaheadOptions, function(optionName, optionValue) {
    //        typeaheadOpts[optionName] = ko.utils.unwrapObservable(optionValue);
    //    });
    //}
    typeaheadOpts.displayKey = allBindings.typeaheadOptions.displayKey;
    var array = valueAccessor();
    typeaheadOpts.source = substringMatcher(ko.isObservable(array) ? ko.utils.unwrapObservable(array) : array, typeaheadOpts.displayKey);

    el.attr("autocomplete", "off").typeahead({
        hint: true,
        highlight: true,
        minLength: 1,
    }, typeaheadOpts);
    if (typeof allBindings.typeaheadOptions.onSelect !== 'undefined') {
        el.bind("typeahead:selected", allBindings.typeaheadOptions.onSelect);
    }
}

// This will be called once when the binding is first applied to an element,
// and again whenever any observables/computeds that are accessed change
// Update the DOM element based on the supplied values here.
ko.bindingHandlers.typeahead = {
    init: createTypeahead,
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var el = $(element);
        el.typeahead('destroy');
        el.unbind("typeahead:selected");
        createTypeahead(element, valueAccessor, allBindings, viewModel, bindingContext);
    }
};

// polyfill
if (typeof String.prototype.startsWith != 'function') {
    // see below for better implementation!
    String.prototype.startsWith = function (str) {
        return this.indexOf(str) == 0;
    };
}

if (typeof String.prototype.getHostname != 'function') {
    // see below for better implementation!
    String.prototype.getHostname = function () {
        var l = document.createElement("a");
        l.href = this;
        return l.hostname;
    };
}

function PopupDialog(config) {
    var self = this;
    self.config = $.extend(true,
    {
        success: function (_) { },
        validate: function (_) { return true; }
    }, config);

    self.data = ko.observable();
    self.show = function (item) {
        self.data(item);
    };

    self.validate = self.config.validate;
    self.success = self.config.success;

    self.submit = function (data) {
        if (self.validate(data)) {
            self.config.success(data, self);
            return true;
        }
        return false;
    };

    self.enableValidation = function () {
        self.validate = function (_) {
            var isValid = self.errors().length == 0;
            if (!isValid) {
                self.errors.showAllMessages();
            }
            return isValid;
        };

        self.errors = ko.validation.group(self);
    };
}

function RestBackedViewModel(config) {
    var url = config.baseUrl;
    var configTemplate = {
        postUrl: url + '/Post',
        putUrl: url + '/Put',
        deleteUrl: url + '/Delete',
        jsonUrl: url + '/Get',
        template: 'template' + config.viewName,
        editTemplate: 'editTemplate' + config.viewName
    };
    return new RestBackedViewModelCustom($.extend(configTemplate, config));
}

function RestBackedViewModelCustom(config) {
    var self = this;
    self.config = $.extend(true, {
        postUrl: '',
        putUrl: '',
        deleteUrl: '',
        jsonUrl: '',
        template: '',
        editTemplate: '',
        defaults: { changed: false },
        search: { changed: false }
    }, config);

    self.items = ko.mapping.fromJS([]);
    self.defaults = ko.mapping.fromJS([self.config.defaults]);
    self.search = ko.mapping.fromJS([self.config.search]);
    self.errorMessage = ko.observable().extend({ notify: 'always' });

    self.load = function (data) {
        var promise = $.getJSON(self.config.jsonUrl, ko.toJS(data), function (array) {
            var arrayLength = array.length;
            for (var i = 0; i < arrayLength; i++) {
                array[i].changed = false;
            }
            ko.mapping.fromJS(array, self.items);
        }).fail(function (error) {
            self.onError(error);
        });

        return promise;
    };

    self.reload = function () {
        self.load(self.search()[0]);
    };

    self.beginEdit = function (data) {
        data.shadowCopy = ko.toJSON(data);
        data.changed(true);
        return true;
    };

    self.cancelEdit = function (data) {
        var object = JSON.parse(data.shadowCopy);
        ko.mapping.fromJS(object, {}, data);
        return true;
    };

    self.add = function (data) {
        self.ajax("POST", self.config.postUrl, data).then(self.reload);
    };

    self.save = function (data) {
        self.ajax("PUT", self.config.putUrl, data);
    };

    self.delete = function (data) {
        self.ajax("DELETE", self.config.deleteUrl, data).then(self.reload);
    };

    self.ajax = function (method, url, data) {
        var json = ko.toJSON(data);
        var promise = $.ajax({
            url: url,
            type: method,
            contentType: "application/json",
            data: json
        }).done(function () {
            data.changed(false);
        }).fail(function (error) {
            self.onError(error);
        });
        return promise;
    };

    self.onError = function (error) {
        self.errorMessage(error.status + ' ' + error.statusText + ' ' + error.responseText);
    };

    self.getTemplateName = function (data) {
        return data.changed() ? self.config.editTemplate : self.config.template;
    };

    //initial load of data
    self.reload();
}
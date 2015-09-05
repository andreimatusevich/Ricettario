"use strict";
//UUID. note: not RFC4122-compliant.
var guid = (function (s4) {
    return function () {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    };
})(function () {
    return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
});

// Outer HTML
if (!$.fn.outerHtml) {
    $.fn.outerHtml = function () {
        if (this.length === 0) {
            return false;
        }
        var elem = this[0], name = elem.tagName.toLowerCase();
        if (elem.outerHTML) {
            return elem.outerHTML;
        }
        var attrs = $.map(elem.attributes, function (i) {
            return i.name + '="' + i.value + '"';
        });
        return "<" + name + (attrs.length > 0 ? " " + attrs.join(" ") : "") + ">" + elem.innerHTML + "</" + name + ">";
    };
}

// Bind twitter typeahead
ko.bindingHandlers.typeahead = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var allBindings = allBindingsAccessor();
        var substringMatcher = function (strs) {
            return function findMatches(q, cb) {
                var matches, substrRegex;

                // an array that will be populated with substring matches
                matches = [];

                // regex used to determine if a string contains the substring `q`
                substrRegex = new RegExp(q, 'i');

                // iterate through the pool of strings and for any string that
                // contains the substring `q`, add it to the `matches` array
                $.each(strs, function (i, str) {
                    if (substrRegex.test(str)) {
                        // the typeahead jQuery plugin expects suggestions to a
                        // JavaScript object, refer to typeahead docs for more info
                        matches.push({ value: str });
                    }
                });

                cb(matches);
            };
        };
        var typeaheadOpts = {
            source: substringMatcher(ko.utils.unwrapObservable(valueAccessor()))
        };

        if (allBindings.typeaheadOptions) {
            $.each(allBindings.typeaheadOptions, function (optionName, optionValue) {
                typeaheadOpts[optionName] = ko.utils.unwrapObservable(optionValue);
            });
        }

        $element.attr("autocomplete", "off").typeahead({
            hint: true,
            highlight: true,
            minLength: 1
        }, typeaheadOpts);
    }
};

// Bind Twitter Progress
ko.bindingHandlers.progress = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);

        var bar = $('<div/>', {
            'class': 'progress-bar',
            'data-bind': 'style: { width:' + valueAccessor() + ' }'
        });

        $element.attr('id', guid())
            .addClass('progress progress-info')
            .append(bar);

        ko.applyBindingsToDescendants(viewModel, $element[0]);
    }
};

// Bind Twitter Alert
ko.bindingHandlers.alert = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var alertInfo = ko.utils.unwrapObservable(valueAccessor());

        var dismissBtn = $('<button/>', {
            'type': 'button',
            'class': 'close',
            'data-dismiss': 'alert'
        }).html('&times;');

        var alertMessage = $('<p/>').html(alertInfo.message);

        $element.addClass('alert alert-' + alertInfo.priority)
            .append(dismissBtn)
            .append(alertMessage);
    }
};

// Bind Twitter Tooltip
ko.bindingHandlers.tooltip = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element, options, tooltip;
        options = ko.utils.unwrapObservable(valueAccessor());
        $element = $(element);

        // If the title is an observable, make it auto-updating.
        if (ko.isObservable(options.title)) {
            var isToolTipVisible = false;

            $element.on('show.bs.tooltip', function () {
                isToolTipVisible = true;
            });
            $element.on('hide.bs.tooltip', function () {
                isToolTipVisible = false;
            });

            // "true" is the bootstrap default.
            var origAnimation = options.animation || true;
            options.title.subscribe(function () {
                if (isToolTipVisible) {
                    $element.data('bs.tooltip').options.animation = false; // temporarily disable animation to avoid flickering of the tooltip
                    $element.tooltip('fixTitle') // call this method to update the title
                        .tooltip('show');
                    $element.data('bs.tooltip').options.animation = origAnimation;
                }
            });
        }

        tooltip = $element.data('bs.tooltip');
        if (tooltip) {
            $.extend(tooltip.options, options);
        } else {
            $element.tooltip(options);
        }
    }
};

// Bind Twitter Popover
ko.bindingHandlers.popover = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var popoverBindingValues = ko.utils.unwrapObservable(valueAccessor());
        var template = popoverBindingValues.template || false;
        var options = popoverBindingValues.options || { title: 'popover' };
        var data = popoverBindingValues.data || false;
        if (template !== false) {
            if (data) {
                options.content = "<!-- ko template: { name: template, if: data, data: data } --><!-- /ko -->";
            }
            else {
                options.content = $('#' + template).html();
            }
            options.html = true;
        }
        $element.on('shown.bs.popover', function (event) {

            var popoverData = $(event.target).data();
            var popoverEl = popoverData['bs.popover'].$tip;
            var options = popoverData['bs.popover'].options || {};
            var button = $(event.target);
            var buttonPosition = button.position();
            var buttonDimensions = {
                x: button.outerWidth(),
                y: button.outerHeight()
            };

            if (data) {
                ko.applyBindings({ template: template, data: data }, popoverEl[0]);
            }
            else {
                ko.applyBindings(viewModel, popoverEl[0]);
            }

            var popoverDimensions = {
                x: popoverEl.outerWidth(),
                y: popoverEl.outerHeight()
            };

            popoverEl.find('button[data-dismiss="popover"]').click(function () {
                button.popover('hide');
            });

            switch (options.placement) {
                case 'right':
                    popoverEl.css({
                        left: buttonDimensions.x + buttonPosition.left,
                        top: (buttonDimensions.y / 2 + buttonPosition.top) - popoverDimensions.y / 2
                    });
                    break;
                case 'left':
                    popoverEl.css({
                        left: buttonPosition.left - popoverDimensions.x,
                        top: (buttonDimensions.y / 2 + buttonPosition.top) - popoverDimensions.y / 2
                    });
                    break;
                case 'top':
                    popoverEl.css({
                        left: buttonPosition.left + (buttonDimensions.x / 2 - popoverDimensions.x / 2),
                        top: buttonPosition.top - popoverDimensions.y
                    });
                    break;
                case 'bottom':
                    popoverEl.css({
                        left: buttonPosition.left + (buttonDimensions.x / 2 - popoverDimensions.x / 2),
                        top: buttonPosition.top + buttonDimensions.y
                    });
                    break;
            }
        });

        $element.popover(options);

        return { controlsDescendantBindings: true };

    }
};

// Bind Twitter Modal
ko.bindingHandlers.modal = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {

        var $element = $(element);
        var modalBindingValues = ko.utils.unwrapObservable(valueAccessor());
        var template = modalBindingValues.template || false;
        var options = modalBindingValues.options || {};
        var data = modalBindingValues.data || false;
        var fade = modalBindingValues.fade || false;
        var openModal = modalBindingValues.openModal || false;
        options.show = false;

        var modalAttr = {
            'class': "modal" + (fade ? ' fade' : ''),
            'tab-index': '-1',
            'role': 'dialog',
            'aria-hidden': 'true'
        };
        if (data) {
            modalAttr['data-bind'] = "template: { name: template, if: data, data: data }";
        }

        var modal = $('<div/>', modalAttr);

        if (!data) {
            modal.html($('#' + template).html());
        }


        modal.modal(options);

        $element.on('click', function () {
            if (data) {
                ko.applyBindings({
                    template: template,
                    data: data
                }, modal[0]);

            } else {
                ko.applyBindings(viewModel, modal[0]);
            }
            modal.modal('show');
            if (openModal) {
                openModal();
            }
            $('.modal-backdrop').css({ height: $(window).height(), position: 'fixed' });
        });

        return { controlsDescendantBindings: true };

    }
};
function isNullOrUndefined(obj) {
    return obj === null || obj === undefined;
}
class AutoCompleteResult {
    constructor(result) {
        this.Result = result;
    }
    toString() {
        return "<li>" + this.Result + "</li>"
    }
    getElement(...events) {
        var li = document.createElement('li');
        li.innerHTML = this.Result;
        events.forEach(item => {
            li.addEventListener(item.type, item.listener);
        });
        return li;

    }
}
class ChangeResult {
    static Changed() {
        return new ChangeResult(2);
    }
    static Concatted() {
        return new ChangeResult(1);
    }
    static Nothing() {
        return new ChangeResult(0);
    }
    constructor(resultNumber) {
        this.Result = resultNumber
    }
}
class ErrorContainer {
    constructor() {
    }
}
class Tag {
    constructor(name, values) {
        this.Name = name;
        this.Values = values;
    }
}

class TagContainer {

    constructor(selector, area) {
        this.className = selector;
        this.Tags = new Array();
        this.TagArea = area;

    }
    addExistingTags() {
        var existings = this.TagArea.Options.existingTags;
        var currentNativeInputTags = this.TagArea.getNativeInput().value;
        currentNativeInputTags = currentNativeInputTags.split(this.TagArea.Options.concatChar);

        existings = currentNativeInputTags.concat(existings);

        if (existings && (existings instanceof Array)) {
            var that = this;
            existings.forEach((item) => {
                if (item) {
                    that.addTag(item);
                }
            });
        }
    }
    getTagListElement() {
        return this.TagArea.getMainContainer().querySelector("." + this.className);
    }
    getCount() {
        return this.Tags.length;
    }
    getTag(tag) {
        if (this.TagArea.Options.formatter) {
            tag = this.TagArea.Options.formatter(tag);
        }
        return this.Tags.filter(f => f.Name == tag)[0];
    }
    getTagElement(tag) {
        var arrayHasThat = this.getTag(tag);
        if (!arrayHasThat)
            return undefined;
        if (this.TagArea.Options.formatter) {
            tag = this.TagArea.Options.formatter(tag);
        }
        var element = this.getTagListElement().querySelector('[data-id="' + tag + '"]');
        if (!element)
            return undefined;

        return element;
    }
    onError(args) {
        this.TagArea.Options.onError(new ErrorMessage(args.type, args.message, this.TagArea.getInputElement()));
    }
    addTag(tag) {


        if (!tag) {
            this.onError({ type: 'warning', message: this.TagArea.CultureResolver.resolve("_no_empty_field_allowed") });
            return;
        }
        if (this.getCount() >= this.TagArea.Options.maxTagCount) {
            this.onError({ type: 'warning', message: this.TagArea.CultureResolver.resolve('_max_input_reached', this.Options.maxTagCount) });
            return;
        }
        var exists = this.getTag(tag);
        if (exists) {
            var element = this.getTagElement(tag);
            element.className += " warning";
            var resolvedMessage = this.TagArea.CultureResolver.resolve("_already_added", tag);
            this.onError({ type: 'warning', message: resolvedMessage });
            return;
        }
        if (this.TagArea.Options.formatter) {
            tag = this.TagArea.Options.formatter(tag);
        }
        this.Tags.push(new Tag(tag, { error: undefined }));
        try {
            var template = this.TagArea.Options.template(tag);
            let container = this;
            if (typeof template === "string" || template instanceof String) {
                var temporaryElement = document.createElement('div');
                temporaryElement.innerHTML = template;
                template = temporaryElement.firstChild;
            }
            template.addEventListener('click', (event) => {
                var eventIsFromTag = event.target.className.indexOf('tag') > -1;
                var tagName = event.target.getAttribute('data-id');
                if (!eventIsFromTag) {
                    tagName = event.target.parentElement.getAttribute('data-id');
                }

                this.TagArea.Options.editable && container.removeTag(tagName);
            });


            let list = this.getTagListElement();
            list.appendChild(template);
            this.TagArea.clearInput();
            this.TagArea.tagAddedd(new Tag(tag));
        } catch{
            var tag = this.Tags.pop();
            console.error(tag);
            throw new Error('Error adding tag! Check template function and/or tag-list element!');

        }
    }
    setError(tag, message) {
        var messageBasedOnCurrentUI = this.TagArea.CultureResolver.resolveImplicitly(message);
        this.onError({ type: 'warning', message: messageBasedOnCurrentUI });
    }
    removeTag(tag) {
        var exists = this.getTag(tag);

        if (exists) {
            this.Tags = this.Tags.filter(t => t.Name != tag);
            var tagElement = this.getTagListElement().querySelector(this.TagArea.Options.tagSelector(tag));
            var list = this.getTagListElement();
            list.removeChild(tagElement);
            this.TagArea.tagRemoved(new Tag(tag));
        }
    }
    clear() {
        var currentCount = this.getCount();
        var tagsToBeRemoved = this.Tags.map(c => c.Name);
        for (var i = 0; i <= currentCount; i++) {
            if (this.Tags.length == 0) {
                break;
            }
            this.removeTag(tagsToBeRemoved[i]);
        }
    }
}
class ErrorMessage {
    constructor(type, message, forElement) {
        this.Type = type;
        this.Message = message;
        this.Element = forElement;
    }
}
class CultureResolver {
    constructor(languages, preffered) {
        this.Languages = languages;
        this.Language = preffered;
    }
    defaultCulture() {
        return "tr";
    }
    isDefault() {
        return !(this.Language) || (this.Language == this.defaultCulture());
    }
    resolve(key, ...arg) {
        var message = "";
        if (this.isDefault()) {
            message = this.Languages[this.defaultCulture()][key];
        }
        message = this.Languages[this.Language][key];
        message = this.replaceMessageArguments(message, arg);
        return message;
    }
    replaceMessageArguments(message, args) {
        if (!args || !(args instanceof Array)) {
            return message;
        }
        args.forEach((element, index) => {
            message = message.replace('$$$value' + index + '$$$', element);
        });
        return message;
    }
    //==Resolves message with previous supplied UI lang
    // Sample input = {"tr":"Mümkün değil","en":"impossible"}
    resolveImplicitly(messageArguments, ...args) {
        var cultureKey = this.isDefault() ? this.defaultCulture() : this.Language;
        var message = (typeof messageArguments === "object") ?
            (messageArguments[cultureKey])
            :
            messageArguments;
        try { message = this.replaceMessageArguments(message, arg); } catch (e) { }
        return message;
    }


}
class TagProvider {
    static None() {
        var plain = new TagProvider();
        plain.init = () => { return 0; };
        return plain;
    }
    constructor(options) {
        this.LastSuggestion = NaN;
        this.LastInput = NaN;
        this.Options = {
            waitInterval: 250,
            minInput: 3,
            url: '',
            cacheResults: false,
            beforeSend: false,
            dataRecieved: false,
            method: 'GET',
            HTMLElementToBind: undefined,
            isSatisfies: false
        };
        this.Options = { ...this.Options, ...options };

    }
    localSatisfaction(text, lastInput) {
        if (!this.Options.isSatisfies(text, lastInput, this.LastSuggestion)) { return false; }
        var first = text && text.length  >= this.Options.minInput;
        var second = Number.isNaN(lastInput) ? true : ((new Date().getTime() - lastInput) >= this.Options.waitInterval);
        return first && second;
    }
    init() {
        if (!this.Options.HTMLElementToBind) { return; }
        var options = this.Options;
        var that = this;
        this.Options.HTMLElementToBind.addEventListener('keyup', function (e) {
            if (that.localSatisfaction(e.target.value, that.LastInput)) {
                $.ajax({
                    cache: options.cacheResults,
                    url: options.url + encodeURIComponent(e.target.value),
                    method: options.method,
                    beforeSend: options.beforeSend,
                    success: (data) => {
                        that.LastSuggestion = new Date().getTime();
                        options.dataRecieved(data);
                    }
                });
            }
            that.LastInput = new Date().getTime();

        });
    }
}

class Ajax{
    constructor(options){
        this.Options={
            url:'',
            method:'GET',
            cache:true,
            beforeSend:()=>{},
            success:(response)=>{},
            error:(x,h,r)=>{}
        };
        this.Options={...this.Options,...options};
        this.prepareRequest();
    }
    prepareRequest(){
        this.Request = new XMLHttpRequest();
        this.Request.onreadystatechange = function () {
           this.handleResponse();
          };
    }
    handleResponse(){
        var DONE = 4; // readyState 4 means the request is done.
        var OK = 200; // status 200 is a successful return.
        if (this.Request.readyState === DONE) {
          if (this.Request.status === OK) {
            console.log(this.Request.responseText); // 'This is the returned text.'
          } else {
            console.log('Error: ' + this.Request.status); // An error occurred during the request.
          }
        }

    }
}
class TagArea {

    constructor(name, options) {
        this.Options = {
            tagProvider: false,
            autoCompleteResults: false,
            language: {
                tr: {
                    _max_input_reached: "En fazla $$$value0$$$ etiket ekleyebilirsiniz!",
                    _restricted_char: "$$$value0$$$ karakteri engellenmiş, lütfen başka bir karakter girin!",
                    _already_added: "$$$value0$$$ etiketi zaten eklenmiştir!"
                },
                _no_empty_field_allowed: 'Boş etiket kaydedilemez!'
            },
            en: {
                _max_input_reached: "Max allowed tag count is $$$value0$$$!",
                _restricted_char: "$$$value0$$$ is restricted. Please type sth different!",
                _already_added: "The tag '$$$value0$$$' is already added!",
                _no_empty_field_allowed: 'Cannot add empty tag!'
            },
            existingTags: [],
            excludeRegex: /_|\n/,
            splitChars: [',', 13],
            concatChar: '_',
            editable: true,
            useLang: function () {
                return navigator.language;
            },
            caseSensitive: true,
            maxTagCount: 5,
            selector: '',
            onError: (arg) => {
                this.Options.displayErrorMessage(arg);
                console.log(arg);
            },
            displayErrorMessage: (arg) => {
                var errorContainer = arg.Element.parentElement.querySelector('em');
                if (!arg.Message) {
                    errorContainer.innerHTML = arg.Message;
                    errorContainer.className = "";
                    return;
                }
                errorContainer.innerHTML = arg.Message;
                errorContainer.className = "text-danger d-block";
            },
            formatter: (tagname) => {
                if (!this.Options.caseSensitive) {
                    tagname = tagname.toLowerCase();
                }
                return tagname.replace("_", " ").replace(',', '');
            },
            template: (tag) => {
                if (this.formatter) {
                    tag = this.formatter(tag);
                }
                var tagElement = document.createElement('span');
                tagElement.className = "tag tag_removable";
                tagElement.setAttribute('data-id', tag);
                tagElement.innerHTML = tag + ' ';
                if (this.Options.editable) {
                    var close = document.createElement('span');
                    close.className = "fa fa-times";
                    tagElement.appendChild(close);
                }
                return tagElement;
            },
            tagSelector: (tag) => {
                if (this.formatter) {
                    tag = this.formatter(tag);
                }
                return '[data-id="' + tag + '"]';
            }
        };
        this.Options = { ...this.Options, ...options };

        var language = this.Options.useLang instanceof Function && this.Options.useLang();
        if (!language) {
            language = this.Options.useLang;
        }
        if (!language) {
            language = navigator.language;
        }

        this.CultureResolver = new CultureResolver(this.Options.language, language);
        this.className = name;
        this.ErrorContainer = new ErrorContainer("error_container");
        this.TagContainer = new TagContainer('tag_list', this);
        this.TagProvider = this.Options.tagProvider || TagProvider.None();
        this.TagProvider.Options.dataRecieved = (typeof this.Options.autoCompleteResults == "object") ? this.Options.autoCompleteResults.handler : (data) => this.autoCompleteResults(data);
        this.TagProvider.Options.isSatisfies = (...args) => { return this.ShouldAutoComplete(args); };
        this.AutoComplete = {
            Index: -1,
            Count: 0,
            Selection: ""
        };
    }
    ShouldAutoComplete(args) {
        return this.AutoComplete.Selection.replace(" ", "") == "";
    }
    navigateThroughAutocomplete(code) {
        if (code == 38) {
            this.navigateUpInAutoComplete();
        }
        else if (code == 40) {
            this.navigateDownInAutoComplete();
        }

    }
    navigateUpInAutoComplete() {
        if (this.AutoComplete.Index == 0) {
            this.AutoComplete.Index = -1;
            this.AutoComplete.Selection = "";
        }
        else if (this.AutoComplete.Index - 1 == this.AutoComplete.Count) {
            this.AutoComplete.Index = 0;
        }
        else {
            this.AutoComplete.Index -= 1;
        }
        if (this.AutoComplete.Index < -1) {
            this.AutoComplete.Index = -1;
        }

        this.setAutoCompleteSelection();
    }
    getAutoCompleteList() {
        return this.getAutoCompleteContainer().querySelectorAll("li");
    }
    getAutoCompleteValue(index) {
        index = index ? index : this.AutoComplete.Index;
        var results = this.getAutoCompleteContainer().querySelectorAll("li");
        if (results && index > -1 && results.length >= index - 1) {
            return results[index].innerHTML;
        }
        return "";
    }
    markAutoCompleteValue(index) {
        index = index ? index : this.AutoComplete.Index;
        var results = this.getAutoCompleteContainer().querySelectorAll("li");
        results.forEach((item) => { item.className = item.className.replace("active", ""); });
        if (results && index > -1 && results.length >= index - 1) {
            var liElem = results[index];
            liElem.className += "active";
            return true;
        }
        return false;
    }
    focusInputElement() {
        var event = new FocusEvent("focus");
        this.getInputElement().dispatchEvent(event);
    }
    setAutoCompleteSelection() {
        this.AutoComplete.Selection = this.getAutoCompleteValue();
        this.markAutoCompleteValue(this.AutoComplete.Index);
        this.getInputElement().value = this.AutoComplete.Selection;
        this.focusInputElement();
    }
    navigateDownInAutoComplete() {
        if (this.AutoComplete.Index == this.AutoComplete.Count - 1) {
            this.AutoComplete.Index = 0;
        }
        else {
            this.AutoComplete.Index += 1;
        }

        if (this.AutoComplete.Index < -1) {
            this.AutoComplete.Index = -1;
        }
        this.setAutoCompleteSelection();

    }
    getAutoCompleteSelectedIndexByValue(value) {
        var list = this.getAutoCompleteList();
        var desired = -1;
        list.forEach((item, index) => {
            if (item.innerHTML == value) {
                desired = index;
            }
        })
        return desired;
    }
    addAutoCompleteResultItem(item) {
        var container = this.getAutoCompleteContainer();
        container.appendChild(item.getElement({
            type: 'click', listener: (event) => {
                var index = this.getAutoCompleteSelectedIndexByValue(event.target.innerHTML);
                this.AutoComplete.Index = index;
                this.AutoComplete.Selection = event.target.innerHTML;
                this.setAutoCompleteSelection();
                this.hideAutoCompleteContainer();
                var event = new Event('keyup');
                event.key = "enter";
                event.which = 13;
                this.getInputElement().dispatchEvent(event);
        }}));
    }
    clearAutoCompleteContainer() {
        this.AutoComplete.Count = 0;
        this.AutoComplete.Index = -1;
        this.AutoComplete.Selection = "";

        var container = this.getAutoCompleteContainer();
        container.innerHTML = "";
    }
    autoCompleteResults(data) {
        this.clearAutoCompleteContainer();
        this.AutoComplete.Count = data.length;
        if (!this.isVisibleAutoCompleteContainer()) {
            this.showAutoCompleteContainer();
        }
        if (!data || data.length == 0) {
            this.addAutoCompleteResultItem(new AutoCompleteResult(this.getInputElement().value));
            return;
        }
        data.forEach(item => {
            var listItem = new AutoCompleteResult(item);
            this.addAutoCompleteResultItem(listItem);
        });
    }
    tagAddedd(tag) {
        var result = ChangeResult.Nothing();
        var native = this.getNativeInput();
        var existingList = native.value && native.value.split(this.Options.concatChar);
        var existsPreviously = [...new Set(existingList)].filter(c => c == tag.Name);
        if (native.value && (!existsPreviously || existsPreviously.length === 0)) {
            native.value += this.Options.concatChar + tag.Name;
            result = ChangeResult.Concatted();//added
        }
        else if (!native.value) {
            native.value = tag.Name;
            result = ChangeResult.Changed();
        }

        this.hideAutoCompleteContainer();
        this.focusInputElement();
        return result;
    }
    proveTagList() {
        var tagList = this.TagContainer.Tags.map(t => t.Name);
        var native = this.getNativeInput();
        var tagAsString = '';
        tagList.forEach((item, index) => {
            tagAsString += (index > 0 ? "_" : "") + item;
        });
        var actualInputList = native.value;
        if (actualInputList == tagAsString) {
            return true;
        }
        return false;

    }
    tagRemoved(tag) {
        var native = this.getNativeInput();
        if (native.value && native.value.split(this.Options.concatChar).length > 1) {
            native.value = native.value.replace(this.Options.concatChar + tag.Name, "");
            return;
        }
        native.value = "";
    }
    isVisibleAutoCompleteContainer() {
        var container = this.getAutoCompleteContainer();
        return container.className.indexOf("d-none") < 0;
    }
    showAutoCompleteContainer() {
        if (!this.isVisibleAutoCompleteContainer()) {
            var container = this.getAutoCompleteContainer();
            container.className = container.className.replace("d-none", "");
        }
    }
    hideAutoCompleteContainer() {
        this.clearAutoCompleteContainer();
        if (this.isVisibleAutoCompleteContainer()) {
            var container = this.getAutoCompleteContainer();
            container.className.replace('d-none');
        }
    }
    getAutoCompleteContainer() {
        return this.getMainContainer().querySelector('ul.tag_autocomplete');
    }
    getNativeInput() {
        return document.querySelector(this.Options.selector);
    }
    clearError() {
        this.Options.displayErrorMessage(new ErrorMessage("", "", this.getInputElement()));
    }
    clearInput() {
        return this.getInputElement().value = "";
    }
    getInputElement() {
        var inputElement = document.querySelector(this.Options.selector + '.tagging_input');

        return inputElement || (this.getNativeInput());
    }
    getCurrentInputValue() {
        return this.getInputElement().value;
    }
    getMainContainer() {
        return this.getInputElement().parentElement;
    }
    getTagListElement() {
        return this.getMainContainer().querySelector('.' + this.TagContainer.className);
    }
    getChildren() {
        return this.Children;
    }
    getTag(tag) {
        return TagContainer.getTag(tag);
    }
    warnTag(tag) {
        var tag = getTag(tag);
        this.ErrorContainer.warnTag(message);
    }
    removeWarning() {
        let area = this.getMainContainer();
        area.className = area.className.replace('warning', '');


    }
    removeWarningFromTags() {
        return this.getTagListElement().querySelectorAll('.warning').forEach(v => v.className = v.className.replace('warning', ''));
    }
    handleBackspace(event) {
        if (event.which != 8 ) {
            return undefined;
        }
        this.clearError();
        this.hideAutoCompleteContainer();
        if (event.target.value.length > 0) {
            return;
        }

        let $list = this.getTagListElement();
        // remove former warning message
        this.removeWarningFromTags();

        var lastChild = $list.lastChild;
        // if there is no child, just return
        if (!lastChild) {
            return;
        }
        // remove child
        //$list.removeChild(lastChild);
        var tag = lastChild.getAttribute('data-id');
        this.TagContainer.removeTag(tag);
        if ($list.children.length < 5) {
            this.removeWarning();
        }
        //place tag value to input
        event.target.value = tag + " ";

    }
    exludeInputs(event) {
        var splitChars = this.Options.splitChars.filter(f => f == event.key || f == event.which);
        if (splitChars && splitChars.length) {
            event.preventDefault();
            return;
        }
        if (this.Options.excludeRegex.test(event.key)) {
            this.Options.onError(new ErrorMessage('warning', this.CultureResolver.resolve("_restricted_char", event.key), this.getInputElement()));
            event.preventDefault();
            return;
        }
    }
    registerEvents() {
        var lookupChars = this.Options.splitChars;
        var container = this.TagContainer;
        var that = this;
        var shouldPreventDefault = lookupChars.filter(f => f === "Enter" || f === 13)[0];

        this.Options.editable && this.getInputElement().addEventListener('keyup', function (event) {
            if (!shouldPreventDefault) {
                event.preventDefault();
            }
            if (lookupChars.filter(s => s == event.key || s == event.which)[0]) {
                that.removeWarningFromTags();
                if (that.Options.maxTagCount > container.getCount()) {
                    that.clearError();
                    lookupChars.forEach((item) => {
                        if (item == 13) {
                            event.target.value = event.target.value.replace("\n", "");
                        }
                        event.target.value = event.target.value.replace(item, '');
                    });
                    if (event.target.value)
                        container.addTag(event.target.value)
                }
                else {
                    that.Options.onError(new ErrorMessage('warning', that.CultureResolver.resolve("_max_input_reached", that.Options.maxTagCount), that.getInputElement()));

                }
            }
            if (event.which == 38 || event.which == 40) {
                that.navigateThroughAutocomplete(event.which);
            }
        });
        this.Options.editable && this.getInputElement().addEventListener('keydown', function (event) {
            that.handleBackspace(event);
            that.exludeInputs(event);
        });
        this.Options.editable && this.getMainContainer().addEventListener('click', function (event) {
            let input = event.target.querySelector('.tagging_input');
            if (input) {
                input.focus();
            }
        });
        this.Options.editable && this.TagProvider.init();
    }
}

var vanillaTag = VANILLATAG = function () {

    return {
        init: (options) => {
            var area = new TagArea("tag_area", options);
            VANILLATAG.renderArea(area);
            area.TagContainer.addExistingTags();
            return area.TagContainer;
        },
        renderArea: (areaToBeRendered) => {
            var mainContainer = document.createElement('div');
            mainContainer.className = areaToBeRendered.className;
            var errorContainer = document.createElement('em');
            errorContainer.className = "";
            var list = document.createElement('div');
            list.className = areaToBeRendered.TagContainer.className;
            mainContainer.appendChild(errorContainer);
            mainContainer.appendChild(list);
            var nativeInput = areaToBeRendered.getNativeInput();
            if (!nativeInput) {
                throw new Error("No such input found with a selector of : " + areaToBeRendered.Options.selector);
            }
            var parent = nativeInput.parentElement;
            parent.removeChild(nativeInput);
            var tempDiv = document.createElement('div');
            tempDiv.appendChild(nativeInput);
            var shadowTempDiv = document.createElement('div');
            shadowTempDiv.innerHTML = tempDiv.innerHTML;
            var shadowInput = document.createElement('textarea');//(shadowTempDiv.firstChild;
            for (let indexOfAttribute = 0; indexOfAttribute < nativeInput.attributes.length; indexOfAttribute++) {
                var nativeAtt = nativeInput.attributes[indexOfAttribute];
                var shadowAtt = nativeAtt.cloneNode();
                shadowAtt.ownerElement = shadowInput;
                shadowInput.setAttributeNode(shadowAtt);
            }
            nativeInput.className += " d-none";
            shadowInput.className += " tagging_input";

            shadowInput.setAttribute('name', '');
            mainContainer.appendChild(nativeInput);
            if (areaToBeRendered.Options.editable) {
                mainContainer.appendChild(shadowInput);
            }
            var autoCompleteUl = document.createElement('ul');
            autoCompleteUl.className += "tag_autocomplete d-none";
            mainContainer.appendChild(autoCompleteUl);

            parent.appendChild(mainContainer);

            areaToBeRendered.TagProvider.Options.HTMLElementToBind = areaToBeRendered.getInputElement();
            areaToBeRendered.registerEvents();

        }
    };
}();
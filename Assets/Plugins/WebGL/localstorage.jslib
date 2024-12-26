mergeInto(LibraryManager.library, {
    SaveToLocalStorage: function(key, value) {
        // Convert the C# strings to JavaScript strings
        var jsKey = UTF8ToString(key);
        var jsValue = UTF8ToString(value);
        localStorage.setItem(jsKey, jsValue);
    },

    LoadFromLocalStorage: function(key) {
        // Convert the C# string to a JavaScript string
        var jsKey = UTF8ToString(key);
        var value = localStorage.getItem(jsKey);
        if (value === null) {
            return 0; // Return null pointer if key doesn't exist
        }
        var lengthBytes = lengthBytesUTF8(value) + 1;
        var stringOnHeap = _malloc(lengthBytes);
        stringToUTF8(value, stringOnHeap, lengthBytes);
        return stringOnHeap;
    },

    DeleteFromLocalStorage: function(key) {
        var jsKey = UTF8ToString(key);
        localStorage.removeItem(jsKey);
    },

    ClearLocalStorage: function() {
        localStorage.clear();
    }
});

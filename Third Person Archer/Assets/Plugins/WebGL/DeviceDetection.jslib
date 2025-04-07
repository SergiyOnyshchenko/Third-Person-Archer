mergeInto(LibraryManager.library, {
    IsMobile: function () {
        var isMobile = /Android|iPhone|iPad|iPod|Windows Phone|Mobile/i.test(navigator.userAgent);
        return isMobile ? 1 : 0;
    }
});
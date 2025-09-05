window.clipboardInterop = {
    copyToClipboard: async function () {
        try {
            const selection = window.getSelection();
            await navigator.clipboard.writeText(selection.toString());
            return true;
        } catch (err) {
            console.error('Failed to copy: ', err);
            return false;
        }
    },

    cutToClipboard: async function (x, y) {
        try {
            const element = document.elementFromPoint(x, y);
            if (element instanceof HTMLInputElement ||
                element instanceof HTMLTextAreaElement) {
                element.focus();
                document.execCommand('cut');
                return true;
            }

            const selection = window.getSelection();
            await navigator.clipboard.writeText(selection.toString());
            document.execCommand('delete');
            return true;
        } catch (err) {
            console.error('Failed to cut: ', err);
            return false;
        }
    },

    pasteFromClipboard: async function (x, y) {
        try {
            const element = document.elementFromPoint(x, y);
            if (element instanceof HTMLInputElement ||
                element instanceof HTMLTextAreaElement) {
                const text = await navigator.clipboard.readText();
                element.focus();
                document.execCommand('insertText', false, text);
                return true;
            }   
            return false;
        } catch (err) {
            console.error('Failed to paste:', err);
            return false;
        }
    },

    selectAll: function (x, y) {
        const element = document.elementFromPoint(x, y);
        if (element instanceof HTMLInputElement ||
            element instanceof HTMLTextAreaElement) {
            element.focus();
            document.execCommand('selectAll');
            return true;
        }
        return false;
    },
    insertTranslation: function (text) {
        const button = document.activeElement;
        const container = button.closest('.translation-container');
        if (!container) return false;

        // Знаходимо textarea всередині контейнера
        const textarea = container.querySelector('textarea');
        if (!textarea) return false;

        // Фокусуємось і вставляємо текст
        textarea.focus();
        document.execCommand('selectAll', false);
        document.execCommand('insertText', false, text);
        return true;
    }
};
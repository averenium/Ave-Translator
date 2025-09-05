window.adaptiveTextBox = {
    // Зберігаємо таймер для батчингу
    batchTimer: null,
    // Зберігаємо елементи, що очікують оновлення
    pendingUpdates: new Set(),
    // Максимальна кількість елементів в батчі
    batchSize: 100,
    // максимальна висота в пікселях
    maxHeight: 600,

    initialize: function (element, dotNetRef) {
        if (!element) return;

        this.queueUpdate(element);

        element.addEventListener('input', () => {
            const currentHeight = parseInt(element.style.height) || 0;
            const newHeight = this.calculateHeight(element);

            if (currentHeight !== newHeight) {
                element.style.height = newHeight + 'px';
            }
        });
    },

    queueUpdate: function(element) {
        if (!element) return;
        
        this.pendingUpdates.add(element);

        if (this.batchTimer) return;

        this.batchTimer = setTimeout(() => {
            this.processBatch();
        }, 16);
    },

    calculateHeight: function(element) {
        const originalOverflow = element.style.overflow;
        const originalHeight = element.style.height;
        
        element.style.overflow = 'hidden';
        element.style.height = 'auto';
        
        const contentHeight = element.scrollHeight;
        
        element.style.overflow = originalOverflow;
        element.style.height = originalHeight;
        
        const finalHeight = Math.min(contentHeight, this.maxHeight);
        
        if (contentHeight > this.maxHeight) {
            element.style.overflow = 'auto';
        } else {
            element.style.overflow = 'hidden';
        }
        
        return finalHeight;
    },

    processBatch: function() {
        if (this.pendingUpdates.size === 0) {
            this.batchTimer = null;
            return;
        }

        const currentBatch = new Set(this.pendingUpdates);
        this.pendingUpdates.clear();

        requestAnimationFrame(() => {
            const elements = Array.from(currentBatch);
            
            for (let i = 0; i < elements.length; i += this.batchSize) {
                const batch = elements.slice(i, i + this.batchSize);
                
                batch.forEach(element => {
                    const currentHeight = parseInt(element.style.height) || 0;
                    const newHeight = this.calculateHeight(element);

                    if (currentHeight !== newHeight) {
                        element.style.height = newHeight + 'px';
                    }
                });
            }

            if (this.pendingUpdates.size > 0) {
                this.batchTimer = setTimeout(() => {
                    this.processBatch();
                }, 16);
            } else {
                this.batchTimer = null;
            }
        });
    },

    adjust: function(element) {
        this.queueUpdate(element);
    }
};
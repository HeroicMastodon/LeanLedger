import type {MaybePromise} from "$lib/index";

export class Dialog {
    value: HTMLDialogElement | undefined = $state();

    async open(onOpen?: () => MaybePromise<any>) {
        console.log("open")
        console.log(this.value);
        if (onOpen) {
            await onOpen();
        }

        this.value?.showModal();
    }

    async close(onClose?: () => MaybePromise<boolean>) {
        let shouldClose = true;
        if (onClose && typeof onClose === 'function') {
            shouldClose = await onClose();
        }

        if (shouldClose) {
            this.value?.close();
        }
    }
}

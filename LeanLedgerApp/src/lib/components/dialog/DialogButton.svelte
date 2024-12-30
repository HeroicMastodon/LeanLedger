<script lang="ts">

    import DefaultDialog from "$lib/components/dialog/DefaultDialog.svelte";
    import type {MaybePromise} from "$lib";

    type ColorType = 'primary' | 'error' | 'warning' | 'success';

    let dialog: HTMLDialogElement | undefined = $state();
    let props: {
        text: string;
        children?: any;
        title: string;
        onConfirm: () => MaybePromise<boolean>;
        confirmText: string;
        class?: string;
        confirmButtonColorType?: ColorType;
        cancelButtonColorType?: ColorType;
        cancelText?: string;
        onclick?: () => MaybePromise<any>;
        oncancel?: () => MaybePromise<any>;
    } = $props();

    async function onConfirm() {
        const shouldClose = await props.onConfirm();

        if (shouldClose) {
            await close();
        }
    }
    async function onClick() {
        if (props.onclick) {
            await props.onclick();
        }

        dialog?.showModal();
    }

    async function close() {
        if (props.oncancel) {
            await props.oncancel();
        }
        dialog?.close();
    }
</script>

<button
    onclick={onClick}
    class="btn {props.class ?? ''}"
>{props.text}
</button>
<DefaultDialog bind:dialog={dialog}>
    <div class="flex flex-col gap-4">
        <h2 class="h2">{props.title}</h2>
        {#if props.children}
            {@render props.children()}
        {/if}
        <div class="flex gap-4 justify-end">
            <button
                onclick={close}
                class="btn variant-outline-{props.cancelButtonColorType}"
            >{props.cancelText ?? "Cancel"}
            </button>
            <button
                onclick={onConfirm}
                class="btn variant-filled-{props.confirmButtonColorType}"
            >
                {props.confirmText}
            </button>
        </div>
    </div>
</DefaultDialog>
